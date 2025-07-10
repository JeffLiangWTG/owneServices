using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Represents a parsed barcode content.
	/// </summary>
	public class BarcodeResultContent
	{
		/// <summary>
		/// Gets or sets the content in text form. NULL if not applicable.
		/// </summary>
		public string Text { get; set; }
		/// <summary>
		/// Gets or sets the content in raw data form. NULL if not applicable.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The arrays returned are not concerned with modifications.")]
		public byte[] RawData { get; set; }
		/// <summary>
		/// Gets or sets the bitmap region in which the barcode is found.
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The arrays returned are not concerned with modifications.")]
		public PointF[] Region { get; set; }
	}
}