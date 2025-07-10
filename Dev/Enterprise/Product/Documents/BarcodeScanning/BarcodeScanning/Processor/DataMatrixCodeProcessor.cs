using ZXing;

namespace Enterprise.Barcode.Business
{
	public class DataMatrixCodeProcessor : ZXingBarcodeProcessor
	{
		public DataMatrixCodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.DATA_MATRIX;
		}
	}
}
