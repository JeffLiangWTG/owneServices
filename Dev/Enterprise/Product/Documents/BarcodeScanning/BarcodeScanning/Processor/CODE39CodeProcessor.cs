using ZXing;

namespace Enterprise.Barcode.Business
{
	public class CODE39CodeProcessor : ZXingBarcodeProcessor
	{
		public CODE39CodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.CODE_39;
		}
	}
}
