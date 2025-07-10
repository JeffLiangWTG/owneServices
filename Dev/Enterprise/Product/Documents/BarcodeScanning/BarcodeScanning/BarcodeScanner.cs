using System.Collections.Specialized;
using System.Drawing;
using System.Linq;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Class to extract barcode data from input image.
	/// </summary>
	public class BarcodeScanner
	{
		/// <summary>
		/// Scan barcodes from input 200dpi (or similar) black and white bitmap.
		/// </summary>
		/// <returns>StringCollection of strings containing barcode data.</returns>
		public StringCollection ExtractBarcodes(string filename)
		{
			using (var inputImage = (Bitmap)Bitmap.FromFile(filename))
			{
				return ExtractBarcodes(inputImage);
			}
		}

		/// <summary>
		/// Scan barcodes from input 200dpi (or similar) black and white bitmap.
		/// </summary>
		/// <returns>StringCollection of strings containing barcode data.</returns>
		public StringCollection ExtractBarcodes(Bitmap inputImage)
		{
			var barcodes = new StringCollection();
			var scanner = GetBarcodeScanner();
			var scannedBarcodes = scanner.Read(inputImage);
			if (scannedBarcodes != null)
			{
				barcodes.AddRange(scannedBarcodes.ToArray());
			}
			return barcodes;
		}

#if DEBUG
		internal virtual
#endif
 IBarcodeScanner GetBarcodeScanner()
		{
			return new EnterpriseScanner();
		}
	}
}
