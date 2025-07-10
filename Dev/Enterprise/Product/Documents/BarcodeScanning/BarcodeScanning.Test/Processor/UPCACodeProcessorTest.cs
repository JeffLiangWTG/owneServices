using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class UPCACodeProcessorTest : ZXingBarcodeProcessorTestCase<UPCACodeProcessor>
	{
		public void TestCreateUPCACode()
		{
			var bitMap = ProcessorForTest.CreateCode("123456789012", new BarCodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("123456789012", parsedResult.Text);
		}

		#region Implementations

		protected override UPCACodeProcessor CreateProcessorForTest()
		{
			return new UPCACodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("upca_single.png"));
				}
			}
		}

		protected override string SingleBarCodeBitmapContent { get; } = "725272730706";

		#endregion
	}
}
