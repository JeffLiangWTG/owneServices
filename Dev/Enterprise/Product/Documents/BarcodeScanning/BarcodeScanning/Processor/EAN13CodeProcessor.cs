using ZXing;

namespace Enterprise.Barcode.Business
{
	public class EAN13CodeProcessor : ZXingBarcodeProcessor
	{
		public EAN13CodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.EAN_13;
		}
	}
}
