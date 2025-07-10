using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class EAN8CodeProcessorTest : ZXingBarcodeProcessorTestCase<EAN8CodeProcessor>
	{
		public void TestCreateEAN8Code()
		{
			var bitMap = ProcessorForTest.CreateCode("12345670", new BarCodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("12345670", parsedResult.Text);
		}

		#region Implementations

		protected override EAN8CodeProcessor CreateProcessorForTest()
		{
			return new EAN8CodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("ean8_single.png"));
				}
			}
		}
		protected override string SingleBarCodeBitmapContent { get; } = "90311017";

		#endregion
	}
}
