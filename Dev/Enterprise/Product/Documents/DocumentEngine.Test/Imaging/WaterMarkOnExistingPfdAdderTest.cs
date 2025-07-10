using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Moq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class WaterMarkOnExistingPfdAdderTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdatePdf()
		{
			var sourceFile = new FileStream(BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Imaging\TestData\WaterMarkOnExistingPfdAdder.SourceFile.pdf", FileMode.Open, FileAccess.Read);
			var edoc = new Mock<IeDoc>();
			edoc.SetupProperty(p => p.ImageData);
			edoc.Setup(p => p.GetImageDataReader()).Returns(new CargoWise.IO.Shim.SubStreamableStream(sourceFile));

			WaterMarkOnExistingPfdAdder.UpdateStorageDocPdfToAddRedWatermark(edoc.Object);
			using (var actualMS = new MemoryStream(edoc.Object.ImageData))
			{
				var actualPdfDocument = PdfReader.Open(actualMS);
				var actualWatermark = ((PdfDictionary)((PdfSharp.Pdf.Advanced.PdfReference)actualPdfDocument.Pages[0].Contents.Elements.Items[0]).Value).Stream.ToString();

				var expectPdfDocument = PdfReader.Open(BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Imaging\TestData\WaterMarkOnExistingPfdAdder.Result.pdf");
				var expectWatermark = ((PdfDictionary)((PdfSharp.Pdf.Advanced.PdfReference)expectPdfDocument.Pages[0].Contents.Elements.Items[0]).Value).Stream.ToString();

				AssertEquals("Watermark should be same", expectWatermark, actualWatermark);
			}
		}
	}
}
