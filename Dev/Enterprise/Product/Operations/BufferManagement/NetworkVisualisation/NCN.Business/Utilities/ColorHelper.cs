using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public static class ColorHelper
	{
		public static ZString GetColorName(int color, ColorList colorList)
		{
			var knownColor = (KnownColor)color;
			var colorName = colorList.GetCodeFromDescription(knownColor.ToString());
			return colorName;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "This is simpler and more intuitive than returning an ad-hoc struct.")]
		public static int SetColorName(ColorList colorList, ZString value, out ZString colorName)
		{
			var enumName = colorList.GetDescriptionFromCode(value);
			colorName = value;

			if (Enum.TryParse<KnownColor>(enumName, out var knownColor))
			{
				return (int)knownColor;
			}
			else
			{
				return 0;
			}
		}
	}
}
