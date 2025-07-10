using System.Drawing;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// The outcome of a candidate label caption measurement.
	/// </summary>
	public class LabelCaptionMeasurement
	{
		public LabelCaptionMeasurement(Rectangle captionBounds, string caption, bool truncated)
		{
			this.CaptionBounds = captionBounds;
			this.Caption = caption;
			this.Truncated = truncated;
		}

		[DpiState(DpiState.ScaledVariant)]
		public Rectangle CaptionBounds { get; private set; }
		public string Caption { get; private set; }
		public bool Truncated { get; private set; }
	}
}
