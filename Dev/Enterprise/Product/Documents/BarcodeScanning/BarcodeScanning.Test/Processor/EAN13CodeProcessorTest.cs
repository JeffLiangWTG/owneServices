using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class EAN13CodeProcessorTest : ZXingBarcodeProcessorTestCase<EAN13CodeProcessor>
	{
		public void TestCreateEAN13Code()
		{
			var bitMap = ProcessorForTest.CreateCode("1234567890128", new BarCodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("1234567890128", parsedResult.Text);
		}

		#region Implementations

		protected override EAN13CodeProcessor CreateProcessorForTest()
		{
			return new EAN13CodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("ean13_single.png"));
				}
			}
		}
		protected override string SingleBarCodeBitmapContent { get; } = "9780201379624";

		#endregion
	}
}
