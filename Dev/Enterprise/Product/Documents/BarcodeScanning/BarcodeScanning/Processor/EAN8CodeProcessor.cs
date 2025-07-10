using ZXing;

namespace Enterprise.Barcode.Business
{
	public class EAN8CodeProcessor : ZXingBarcodeProcessor
	{
		public EAN8CodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.EAN_8;
		}
	}
}
