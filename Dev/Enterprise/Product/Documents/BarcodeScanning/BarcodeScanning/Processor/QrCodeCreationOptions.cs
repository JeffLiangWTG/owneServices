namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Contains the options to create QR codes.
	/// </summary>
	public class QrCodeCreationOptions : BarCodeCreationOptions
	{
		/// <summary>
		/// Gets or sets the character encoding of the content.
		/// </summary>
		public string CharacterEncoding { get; set; }
		/// <summary>
		/// Gets or sets the version (1 - 40). NULL for automatic versioning.
		/// </summary>
		public int? Version { get; set; }
		/// <summary>
		/// Gets or sets the error correction level.
		/// </summary>
		public QrCodeErrorCorrectionLevel? ErrorCorrectionLevel { get; set; }
	}
	/// <summary>
	/// Defines the levels of QR code error correction.
	/// </summary>
	public enum QrCodeErrorCorrectionLevel
	{
		L = 0,
		M,
		Q,
		H
	}
}
