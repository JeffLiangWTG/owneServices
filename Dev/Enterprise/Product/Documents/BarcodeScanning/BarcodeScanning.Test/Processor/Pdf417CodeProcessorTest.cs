using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class Pdf417CodeProcessorTest : ZXingBarcodeProcessorTestCase<Pdf417CodeProcessor>
	{
		public void TestCreatePdf417Code()
		{
			var bitMap = ProcessorForTest.CreateCode("Hello World", new Pdf417CodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("Hello World", parsedResult.Text);
		}

		#region Implementations

		protected override Pdf417CodeProcessor CreateProcessorForTest()
		{
			return new Pdf417CodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("pdf417_single.png"));
				}
			}
		}

		#endregion
	}
}
