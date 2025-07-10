using System;
using System.Drawing;
using CargoWise.Common;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine
{
	public static class WatermarkFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static Watermark GetWatermark(SerialisableWatermark watermarkInfo)
		{
			Argument.NotNull(watermarkInfo, nameof(watermarkInfo)); // Suggested By ReviewBot

			if (!(watermarkInfo.UseTextWatermark && watermarkInfo.TextWatermark != null
				|| !watermarkInfo.UseTextWatermark && watermarkInfo.ImageWatermark != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(watermarkInfo));
			}

			Watermark result;

			if (watermarkInfo.UseTextWatermark)
			{
				result = new TextWatermark(
					watermarkInfo.TextWatermark,
					Watermark.GetHorizontalAlignment(watermarkInfo.HorizontalAlignment),
					Watermark.GetVerticalAlignment(watermarkInfo.VerticalAlignment),
					watermarkInfo.HorizontalOffset,
					watermarkInfo.VerticalOffset,
					watermarkInfo.Rotation,
					Color.FromArgb(watermarkInfo.Opacity, 0, 0, 0),
					"Arial",
					watermarkInfo.FontSize,
					FontStyle.Bold);
			}
			else
			{
				result = new ImageWatermark(
					watermarkInfo.ImageWatermark,
					Watermark.GetHorizontalAlignment(watermarkInfo.HorizontalAlignment),
					Watermark.GetVerticalAlignment(watermarkInfo.VerticalAlignment),
					watermarkInfo.HorizontalOffset,
					watermarkInfo.VerticalOffset,
					watermarkInfo.Rotation);
			}

			return result;
		}
	}
}
