using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Calculates the size and appropriate label caption for a given width / font.
	/// </summary>
	public static class StringRenderingHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static string MeasureBestFit(string[] strings, Func<string, bool> hasObstructionFunc, out bool isCaptionTruncated)
		{
			var result = string.Empty;
			isCaptionTruncated = false;

			if (strings != null && strings.Length > 0)
			{
				var captionsLongToShort = strings.OrderLongToShort();
				foreach (var item in captionsLongToShort)
				{
					result = item;
					isCaptionTruncated = hasObstructionFunc(item);
					if (!isCaptionTruncated)
					{
						return result;
					}
				}

				foreach (var truncatedCaption in GetTruncatedCaptions(result))
				{
					result = truncatedCaption + "...";
					if (!hasObstructionFunc(result))
					{
						break;
					}
				}
			}

			return result;
		}

		public static bool HasObstruction(Control control, Size measuredTextSize)
			=> HasObstruction(control.Size, measuredTextSize, control.Font.Height);

		public static bool HasObstruction(Size displaySize, Size measuredTextSize, int fontHeight)
		{
			var linesFitted = (int)Math.Round((float)measuredTextSize.Height / fontHeight); // I don't find any problems with Math.Round
#if WINZOR
			return measuredTextSize.Width - TextRenderer.GetWidthAdjustment() > displaySize.Width || (measuredTextSize.Height > displaySize.Height && linesFitted > 1);
#else
			return measuredTextSize.Width > displaySize.Width || (measuredTextSize.Height > displaySize.Height && linesFitted > 1);
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "strings")]
		public static string MeasureBestFit(string[] strings, int width, Font font, int height)
		{
			SizeF size;
			bool truncated;
			return MeasureBestFit(strings, width, "", font, height, StringRenderingOptions.None, out size, out truncated);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "strings")]
		public static string MeasureBestFit(string[] strings, int width, Font font, int height, StringRenderingOptions options, out SizeF size, out bool truncated)
		{
			return MeasureBestFit(strings, width, "", font, height, options, out size, out truncated);
		}

		/// <summary>
		/// Find the best fit caption and measure it's size. For performance, the outcome of this calculation is
		/// cached statically.
		/// NOTE: You should aim the same string[] *reference* each time to make sure a re-calculation isn't required
		///       for performance.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "strings")]
		public static string MeasureBestFit(string[] strings, int width, string addition, Font font, int height, StringRenderingOptions options, out SizeF size, out bool truncated)
		{
			size = SizeF.Empty;
			var result = "";

			truncated = false;
			if (strings != null && strings.Length > 0)
			{
				var key = new CacheKey(strings, addition, font, height, width, options);
				var measurement = measurementCache[key];
				if (measurement == null)
				{
					string caption = MeasureBestFitUncached(strings, addition, font, height, width, options, out size, out truncated);
					measurement = new Measurement(caption, size, truncated);
					measurementCache.Add(key, measurement);
				}
				size = measurement.Size;
				result = measurement.Caption;
				truncated = measurement.Truncated;
			}
			return result;
		}

		#region Measurement / CacheKey classes

		class Measurement
		{
			public Measurement(string caption, SizeF size, bool truncated)
			{
				Caption = caption;
				Size = size;
				Truncated = truncated;
			}

			public string Caption { get; private set; }
			public SizeF Size { get; private set; }
			public bool Truncated { get; private set; }
		}

		class CacheKey
		{
			public CacheKey(string[] captions, string labelPostfix, Font font, int height, int width, StringRenderingOptions options)
			{
				this.captions = captions;
				this.labelPostfix = labelPostfix;
				this.fontName = font.Name;
				this.fontHeight = height;
				this.fontStyle = font.Style;
				this.width = width;
				this.options = options;
			}

			public override bool Equals(object obj)
			{
				CacheKey rhs = obj as CacheKey;
				bool result = true;
				result = result && captions == rhs.captions; // comparing references of arrays produced the best performance
				result = result && labelPostfix == rhs.labelPostfix;
				result = result && fontName == rhs.fontName;
				result = result && fontHeight == rhs.fontHeight;
				result = result && fontStyle == rhs.fontStyle;
				result = result && width == rhs.width;
				result = result && options == rhs.options;
				return result;
			}

			public override int GetHashCode()
			{
				return captions.GetHashCode() ^ width.GetHashCode();
			}

			readonly string[] captions;
			readonly string labelPostfix;
			readonly string fontName;
			readonly int fontHeight;
			readonly FontStyle fontStyle;
			readonly int width;
			readonly StringRenderingOptions options;
		}

		#endregion

		#region Implementation

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<CacheKey, Measurement> measurementCache = new LRUCache<CacheKey, Measurement>(1000);

		static string MeasureBestFitUncached(string[] strings, string addition, Font font, int height, int width, StringRenderingOptions options, out SizeF size, out bool truncated)
		{
			truncated = false;
			var captionsLongToShort = strings.OrderLongToShort();
			var result = MeasureBestFit(captionsLongToShort, addition, font, height, width, options, out size);
			if (options.HasFlag(StringRenderingOptions.Truncate) && captionsLongToShort.Length > 0 && !string.IsNullOrEmpty(captionsLongToShort.Last()) && (string.IsNullOrEmpty(result) || size.Width > width))
			{
				result = MeasureBestFit(GetTruncatedCaptions(captionsLongToShort.Last()), addition, font, height, width, options, out size, true);
				truncated = true;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "I don't find any problems with Math.Round")]
		static string MeasureBestFit(IEnumerable<string> orderedLongToShort, string addition, Font font, int height, int width, StringRenderingOptions options, out SizeF size, bool truncated = false)
		{
			var result = "";
			var lowestLinesFitted = int.MaxValue;
			size = SizeF.Empty;

			foreach (string item in orderedLongToShort)
			{
				var caption = item;

				if (truncated)
				{
					caption += "...";
				}
				var currentSize = MeasureText(caption + addition, font, height, width, options);
				var linesFitted = (int)Math.Round(currentSize.Height / height); // I don't find any problems with Math.Round

				if (linesFitted < lowestLinesFitted ||
					(currentSize.Width != width && size.Width > width && currentSize.Width < size.Width) ||
					(currentSize.Width < width && size.Width < width && currentSize.Width > size.Width))
				{
					result = caption + addition;
					size = currentSize;
					lowestLinesFitted = linesFitted;
					if (linesFitted == 1 && currentSize.Width < width)
					{
						break;
					}
				}
			}

			return result;
		}

		static SizeF MeasureText(string text, Font font, int height, int width, StringRenderingOptions options)
		{
			var size = options.HasFlag(StringRenderingOptions.Wrap) ?
				ControlDpiScalingHelper.NewScaledSize(width, 999999, false) :
				ControlDpiScalingHelper.NewScaledSize(999999, height, false);
			return TextRenderer.MeasureText(text, font, size, TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
		}

		public static Size MeasureText(string text, Font font, Size proposedSize)
		{
			return TextRenderer.MeasureText(text, font, proposedSize, TextFormatFlags.WordBreak);
		}

		public static IEnumerable<string> GetTruncatedCaptions(string caption)
		{
			while (caption.Length > 1)
			{
				caption = caption.Remove(caption.Length - 1);
				yield return caption;
			}
		}

		#endregion
	}
}
