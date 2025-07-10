using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Renders a variable length caption on a GroupBox.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public sealed class GroupBoxVariableLengthCaptionRenderer : ControlTextVariableLengthCaptionRenderer
	{
		public GroupBoxVariableLengthCaptionRenderer(GroupBox groupBox)
			: base(groupBox)
		{
		}

		protected override string MeasureBestFitCaption()
		{
			SizeF size;
			bool truncated;
			var font = Control.Font;
			return StringRenderingHelper.MeasureBestFit(Captions, GroupBox.DisplayRectangle.Width, font, font.Height, StringRenderingOptions.Truncate, out size, out truncated);
		}

		GroupBox GroupBox
		{
			get { return (GroupBox)base.Control; }
		}
	}
}
