
namespace Enterprise.Barcode.Business
{
	internal struct BarcodeValue
	{
		public string Text;
		public BarcodeSymbologie Symbologie;
		public double Confidence;

		public BarcodeValue(string text, double confidence, BarcodeSymbologie symbologie)
		{
			this.Text = text;
			this.Symbologie = symbologie;
			this.Confidence = confidence;
		}
	}
}
