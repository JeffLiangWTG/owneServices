using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine
{
	public class TextSizeCalculator
	{
		public const GraphicsUnit DefaultGraphicsUnit = GraphicsUnit.Millimeter;

		public TextSizeCalculator(Font font)
		{
			Argument.NotNull(font, nameof(font));
			Font = font;
		}

		public Font Font { get; }

		public Size GetTextSize(string text) => GetTextSize(text, DefaultGraphicsUnit);

		public Size GetTextSize(string text, GraphicsUnit unit) => GetTextSize(text, Font, unit);

		public Size GetTextSize(string text, Font font, GraphicsUnit unit, int? width = null)
		{
			var result = new Size();

			if (text.Length > 0)
			{
				using (var graphicsManager = new GraphicsManager())
				{
					graphicsManager.Graphics.PageUnit = unit;

					var splits = ((ZString)text).Split(GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak);

					var sizes = width.HasValue ?
						splits.Select(s => graphicsManager.Graphics.MeasureString(s, font, width.Value)) :
						splits.Select(s => graphicsManager.Graphics.MeasureString(s, font));

					SizeF sizef;
					try
					{
						sizef = new SizeF(sizes.Max(s => s.Width), sizes.Sum(s => s.Height));
					}
					catch (ExternalException ex) when (ex.ErrorCode == -2147467259)
					{
						throw new DocumentEngineException(Res.GetString("e39b9ca8-2e5f-4740-8624-3f117695204c", "Can't calculate size of the text '{0}'. Please ensure that there are no special characters in the text.", text), ex);
					}

					var newWidth = (int)Math.Ceiling(sizef.Width);
					var newHeight = (int)Math.Ceiling(sizef.Height);
					if (ShouldUnscale(unit))
					{
						newWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(newWidth);
						newHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(newHeight);
					}
					result = ControlDpiScalingHelper.NewScaledSize(newWidth, newHeight, false);
				}
			}

			return result;
		}

		public int GetLengthInMillimeter(String text) => GetTextSize(text, Font, DefaultGraphicsUnit).Width;

		internal static bool ShouldUnscale(GraphicsUnit unit)
		{
			return unit == GraphicsUnit.Display || unit == GraphicsUnit.Pixel;
		}
	}
}
