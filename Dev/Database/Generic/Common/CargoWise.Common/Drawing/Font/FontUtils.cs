using System.Drawing;

namespace CargoWise.Common
{
	public static class FontUtils
	{
		public static bool IsStyleAvailable(FontFamily fontFamily, FontStyle style)
		{
			var os2Table = new OS2Table(fontFamily, style);
			var result = true;

			if (style == FontStyle.Regular)
			{
				result = os2Table.IsRegular;
			}
			else
			{
				if ((style & FontStyle.Bold) == FontStyle.Bold)
				{
					result = result && os2Table.IsBold;
				}
				if ((style & FontStyle.Italic) == FontStyle.Italic)
				{
					result = result && os2Table.IsItalic;
				}
				if ((style & FontStyle.Strikeout) == FontStyle.Strikeout)
				{
					result = result && os2Table.IsStrikeout;
				}
				if ((style & FontStyle.Underline) == FontStyle.Underline)
				{
					result = result && os2Table.IsUnderscore;
				}
			}

			return result;
		}
	}
}
