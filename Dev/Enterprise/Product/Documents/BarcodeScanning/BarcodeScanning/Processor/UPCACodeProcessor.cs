using ZXing;

namespace Enterprise.Barcode.Business
{
	public class UPCACodeProcessor : ZXingBarcodeProcessor
	{
		public UPCACodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.UPC_A;
		}
	}
}
