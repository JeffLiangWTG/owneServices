using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class DataMatrixCodeProcessorTest : ZXingBarcodeProcessorTestCase<DataMatrixCodeProcessor>
	{
		public void TestCreateDataMatrixCode()
		{
			var bitMap = ProcessorForTest.CreateCode("Hello World", new BarCodeCreationOptions());
			var parsedResult = ProcessorForTest.ParseCode(bitMap);

			AssertEquals("Hello World", parsedResult.Text);
		}

		#region Implementations

		protected override DataMatrixCodeProcessor CreateProcessorForTest()
		{
			return new DataMatrixCodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("data_matrix_single.png"));
				}
			}
		}
		protected override string SingleBarCodeBitmapContent { get; } = "MECARD:N:Justin Chen;TEL:+86 18888888888;EMAIL:justin.chen@wisetechglobal.com;";

		#endregion
	}
}
