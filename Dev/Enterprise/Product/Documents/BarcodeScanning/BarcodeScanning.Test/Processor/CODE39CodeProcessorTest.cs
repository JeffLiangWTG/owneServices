using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class CODE39CodeProcessorTest : ZXingBarcodeProcessorTestCase<CODE39CodeProcessor>
	{
		public void TestCreateCODE39Code()
		{
			var bitMap = ProcessorForTest.CreateCode("10404UZ176908720122", new BarCodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("10404UZ176908720122", parsedResult.Text);
		}

		#region Implementations

		protected override CODE39CodeProcessor CreateProcessorForTest()
		{
			return new CODE39CodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("code39_single.png"));
				}
			}
		}
		protected override string SingleBarCodeBitmapContent { get; } = "WIKIPEDIA";

		#endregion
	}
}
