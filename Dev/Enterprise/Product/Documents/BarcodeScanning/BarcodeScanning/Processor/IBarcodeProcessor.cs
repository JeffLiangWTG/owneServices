using System.Drawing;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Defines the contract of a barcode processor to parse and create barcodes.
	/// </summary>
	public interface IBarcodeProcessor
	{
		#region Bar Code

		/// <summary>
		/// Parses a piece of content out of a bar code in a specific bitmap.
		/// </summary>
		/// <param name="bitmap">The bitmap containing the bar code.</param>
		/// <returns>The content contained in the bar code.</returns>
		BarcodeResultContent ParseCode(Bitmap bitmap);

		/// <summary>
		/// Creates a new bar code that encapsulates a specific content.
		/// </summary>
		/// <param name="content">The content contained.</param>
		/// <param name="options">The bar code creation options.</param>
		/// <returns>The created bar code bitmap.</returns>
		Bitmap CreateCode(string content, BarCodeCreationOptions options);

		#endregion
	}
}