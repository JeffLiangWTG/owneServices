using System;
using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	public sealed class ShrinkToFitCalculator : IDisposable
	{
		public ShrinkToFitCalculator(Graphics graphics = null)
		{
			this.graphics = graphics;
		}

		const float minScaledFontSize = 6f;
		bool disposeGraphics;
		Graphics graphics;

		Graphics GetOrCreateGraphics()
		{
			if (graphics == null)
			{
				graphics = Graphics.FromHwnd(IntPtr.Zero);
				disposeGraphics = true;
			}

			return graphics;
		}

		public void Dispose()
		{
			if (disposeGraphics)
			{
				graphics?.Dispose();
			}
		}

		public float ShrinkToFit(string text, SizeF availableSize, IFont font, StringFormat stringFormat)
		{
			if (string.IsNullOrWhiteSpace(text)
				|| availableSize.Width <= 0f
				|| availableSize.Height <= 0f
				|| font == null
				|| stringFormat == null)
			{
				return font?.Size ?? minScaledFontSize;
			}

			var isVerticalText = font.Rotation == 90 || font.Rotation == 180;
			if (isVerticalText)
			{
				availableSize = new SizeF(availableSize.Height, availableSize.Width);
			}

			try
			{
				return ShrinkToFitCore(text, availableSize, font, stringFormat);
			}
			catch (System.Runtime.InteropServices.ExternalException ex)
			{
				ex.Data[nameof(text)] = text;
				ex.Data[nameof(availableSize)] = availableSize;
				ex.Data[nameof(font)] = $"{font.Name} {font.Size}";
				ex.Data[nameof(stringFormat.FormatFlags)] = stringFormat.FormatFlags;

				ErrorReporter.ReportOnce("ShrinkToFitCalculator-ShrinkToFit", // SuppressCodeSmell Reason = programmatic constant
					"This is to help understand why we get ExternalException when ShrinkToFitCalculator.ShrinkToFit is called. If it's possible to find a fix/optimisation then please proceed, otherwise remove this ErrorReporter.", // SuppressCodeSmell Reason = programmatic constant
					ex);
			}

			try
			{
				var sanitizedText = new string('W', text.Length);
				return ShrinkToFitCore(sanitizedText, availableSize, font, stringFormat);
			}
			catch (System.Runtime.InteropServices.ExternalException)
			{
				return font.Size;
			}
		}

		float ShrinkToFitCore(string text, SizeF availableSize, IFont font, StringFormat stringFormat)
		{
			Argument.NotNull(font, nameof(font));

			var maxFontSize = font.Size;

			if (string.IsNullOrWhiteSpace(text)
				|| Math.Min(availableSize.Width, availableSize.Height) <= 0
				|| font.Size <= minScaledFontSize)
			{
				return maxFontSize;
			}

			const float precision = 0.5f;

			var hi = maxFontSize;
			var lo = minScaledFontSize;

			while (true)
			{
				var range = hi - lo;
				var canResizeRange = range > precision;

				if (DoesFit(text, font, hi, availableSize, stringFormat))
				{
					if (hi >= maxFontSize || !canResizeRange)
					{
						break;
					}

					lo = hi;
					hi = RoundDownToNearest5(Math.Min(hi + range / 2, font.Size));

					continue;
				}

				if (!canResizeRange)
				{
					hi = lo;
					break;
				}

				if (DoesFit(text, font, lo, availableSize, stringFormat))
				{
					hi = RoundDownToNearest5(hi - range / 2);
				}
				else
				{
					hi = RoundDownToNearest5(Math.Max(lo - precision, minScaledFontSize));
					lo = RoundDownToNearest5(Math.Max(lo - range / 2, minScaledFontSize));
				}

				if (hi <= minScaledFontSize)
				{
					hi = minScaledFontSize;
					break;
				}
			}

			return hi;
		}

		float RoundDownToNearest5(float number) => (float)(Math.Round(number * 2f, MidpointRounding.AwayFromZero) / 2f); // SuppressCodeSmell Reason = don't want any references to ZArchitecture

		public SizeF CalculateRequiredSpace(string text, IFont font, SizeF availableSize, StringFormat stringFormat) =>
			CalculateRequiredSpace(text, font, font?.Size ?? 0f, availableSize, stringFormat);

		SizeF CalculateRequiredSpace(string text, IFont font, float fontSize, SizeF availableSize, StringFormat stringFormat)
		{
			if (string.IsNullOrEmpty(text)
				|| font == null
				|| availableSize.IsEmpty)
			{
				return SizeF.Empty;
			}

			var gr = GetOrCreateGraphics();

			using (var drawingFont = new System.Drawing.Font(font.Name, fontSize, font.Style))
			{
				var clone = (StringFormat)stringFormat.Clone();
				clone.FormatFlags &= ~StringFormatFlags.NoWrap;

				var requiredSpace = gr.MeasureString(text,
					drawingFont,
					new SizeF(availableSize.Width, Int32.MaxValue),
					clone);

				return requiredSpace;
			}
		}

		bool DoesFit(string text, IFont font, float fontSize, SizeF layoutArea, StringFormat stringFormat)
		{
			var requiredSpace = CalculateRequiredSpace(text, font, fontSize, layoutArea, stringFormat);
			return requiredSpace.Height <= layoutArea.Height;
		}
	}
}
