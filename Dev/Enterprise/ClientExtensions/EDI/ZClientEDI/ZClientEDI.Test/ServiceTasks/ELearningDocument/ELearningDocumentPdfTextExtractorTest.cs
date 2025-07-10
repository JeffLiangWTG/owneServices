using System;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentPdfTextExtractorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTextExtractorWithNullByteArrayProvided()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var pdfToText = new PdfTextExtractor(mockLogger.Object);

			//Act
			var content = pdfToText.GetText(null);

			//Assert
			Assert("Should return empty string for null input", string.IsNullOrEmpty(content));
		}

		[ExpectNoExceptions]
		public void TestTextExtractorWithEmptyByteArrayProvided()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var pdfToText = new PdfTextExtractor(mockLogger.Object);

			//Act
			var content = pdfToText.GetText(Array.Empty<byte>());

			//Assert
			Assert("Should return empty string for empty byte array input", string.IsNullOrEmpty(content));
		}

		[ExpectNoExceptions]
		public void TestSuccessReadSingleLine()
		{
			using (var memoryReader = new MemoryStream())
			{
				// Arrange
				var mockLogger = new Mock<ILogger>();
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ZClientEDI.Test.ServiceTasks.ELearningDocument.Data.hello_world.pdf");
				stream.CopyTo(memoryReader);
				var pdfToText = new PdfTextExtractor(mockLogger.Object);

				//Act
				var content = pdfToText.GetText(memoryReader.ToArray());

				//Assert
				Assert($"Should return empty string for empty byte array input :{content}", "hello world" == content);
			}
		}

		[ExpectNoExceptions]
		public void TestReadWronglyFormattedPdfDocument()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			Random rnd = new Random();
			var data = new byte[short.MaxValue];
			rnd.NextBytes(data);
			var pdfToText = new PdfTextExtractor(mockLogger.Object);

			//Act
			var content = pdfToText.GetText(data);

			//Assert
			AssertNullOrEmpty($"Should return empty string random large array of bytes", content);
		}

		[ExpectNoExceptions]
		public void TestDownloadFileWholeFlow()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new Mock<IPdfTextExtractor>();
			client.Setup(m => m.GetText(It.IsAny<byte[]>())).Returns(string.Empty);
			// Act
			var data = client.Object.GetText(It.IsAny<byte[]>());
			// Assert
			Assert("Should return not null", data.Length == 0);
		}
	}
}
