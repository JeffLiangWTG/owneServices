using System.Drawing;
using WinzorFramework.Extensions;

//As Winforms does not support these useful border properties, they are here to keep the Panel class pure
namespace CargoWise.Windows.UI
{
	public struct KBorderStyle
	{
		public Color BorderColor { get; set; }

		public float BorderWidth { get; set; }

		public KBorderHtmlStyle BorderLineStyle { get; set; }

		public int BorderRadius { get; set; }

		public string GetBorderStyleString()
		{
			var borderRadiusString = $"{(BorderRadius == 0 ? string.Empty : $"border-radius:{BorderRadius}px")};";
			var borderStyleString = $"{(BorderLineStyle == KBorderHtmlStyle.None ? string.Empty : $"border: {BorderWidth}px {BorderLineStyle} {(BorderColor == Color.Empty ? string.Empty : BorderColor.GetColorStyleValue())};")}";
			return borderRadiusString + borderStyleString;
		}
	}
}
