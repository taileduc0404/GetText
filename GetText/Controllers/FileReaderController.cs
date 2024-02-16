using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using NPOI.XWPF.UserModel;
using System.IO;
using System.Text;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using NPOI.XWPF.Extractor;

public class FileReaderController : Controller
{
	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	public IActionResult UploadFile(IFormFile file)
	{
		try
		{
			if (file != null && file.Length > 0)
			{
				string fileName = file.FileName.ToLower();

				if (fileName.EndsWith(".pdf"))
				{
					string text = ReadPdf(file);
					ViewBag.Text = text;
				}
				else if (fileName.EndsWith(".docx"))
				{
					string text = ReadWord(file);
					ViewBag.Text = text;
				}
				else
				{
					ViewBag.Text = "Unsupported file format";
				}
			}
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}


		return View("Index");
	}

    private string ReadPdf(IFormFile file)
    {
        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            StringBuilder text = new StringBuilder();

            try
            {
                using (var pdfReader = new PdfReader(stream))
                {
                    using (var pdfDocument = new PdfDocument(pdfReader))
                    {
                        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                        {
                            var page = pdfDocument.GetPage(i);
                            var strategy = new LocationTextExtractionStrategy();
                            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                            text.Append(pageText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error reading PDF: {ex.Message}";
            }

            return text.ToString();
        }
    }

    // Validate PDF header
    //private bool IsValidPdf(MemoryStream stream)
    //{
    //	const int PdfHeaderLength = 5;

    //	if (stream.Length < PdfHeaderLength)
    //	{
    //		return false;
    //	}

    //	byte[] headerBytes = new byte[PdfHeaderLength];
    //	stream.Read(headerBytes, 0, PdfHeaderLength);

    //	string header = Encoding.ASCII.GetString(headerBytes);

    //	return header.StartsWith("%PDF");
    //}


    private string ReadWord(IFormFile file)
    {
        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var wordDocument = new XWPFDocument(stream);
            var extractor = new XWPFWordExtractor(wordDocument);
            string extractedText = extractor.Text;

            return extractedText;
        }
    }

}
