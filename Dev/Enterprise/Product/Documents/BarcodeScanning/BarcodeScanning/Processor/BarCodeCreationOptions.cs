namespace Enterprise.Barcode.Business
{
	public class BarCodeCreationOptions
	{
		/// <summary>
		/// Gets or sets the width, in pixels.
		/// </summary>
		public int Width { get; set; } = 200;
		/// <summary>
		/// Gets or sets the height, in pixels.
		/// </summary>
		public int Height { get; set; } = 200;
		/// <summary>
		/// Gets or sets the margin, in pixels.
		/// </summary>
		public int Margin { get; set; }
	}
}
