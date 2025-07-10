namespace Enterprise.Barcode.Business
{
	public class Pdf417CodeCreationOptions : BarCodeCreationOptions
	{
		public string CharacterSet { get; set; }
		public Pdf417CodeErrorCorrectionLevel? ErrorCorrectionLevel { get; set; }
	}

	public enum Pdf417CodeErrorCorrectionLevel
	{
		L0,
		L1,
		L2,
		L3,
		L4,
		L5,
		L6,
		L7,
		L8,
		AUTO
	}
}
