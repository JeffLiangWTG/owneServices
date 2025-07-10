using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public static class StringExtensions
	{
		/// <summary>
		/// Checks the width of a string rendered with the provided font, and truncates the string if needed so that it may
		/// fit within the available space
		/// </summary>
		public static string TruncateToFit(this string text, Font font, int avaliableWidth, string truncPostfix = "...")
		{
			//This could be faster, but we'll cross that bridge when we come to it.
			if (text != null && text.Length > 0 && TextRenderer.MeasureText(text, font).Width > avaliableWidth)
			{
				do
				{
					text = text.Remove(text.Length - 1);
				} while (text.Length > 0 && TextRenderer.MeasureText(text + truncPostfix, font).Width > avaliableWidth);

				text = text.TrimEnd() + truncPostfix;
			}

			return text;
		}
	}
}
