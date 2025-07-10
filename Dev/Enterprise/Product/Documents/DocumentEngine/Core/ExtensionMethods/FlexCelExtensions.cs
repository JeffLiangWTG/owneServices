using System.Drawing;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	internal static class FlexCelExtensions
	{
		public static FontStyle ToFontStyle(this TFlxFontStyles flxFontStyles)
		{
			var result = FontStyle.Regular;

			if ((flxFontStyles & TFlxFontStyles.Bold) != 0)
			{
				result |= FontStyle.Bold;
			}

			if ((flxFontStyles & TFlxFontStyles.Italic) != 0)
			{
				result |= FontStyle.Italic;
			}

			return result;
		}
	}
}
