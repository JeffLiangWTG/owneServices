using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Types;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;
using WTG.RtfConverter.Rtf;

namespace Enterprise.ZArchitecture.Core
{
	public static class ORtfTextUtil
	{
		/// <summary>
		/// Is the given string Rtf encoded?
		/// </summary>
		public static bool IsRtf(string rtf)
		{
			return rtf.StartsWith("{\\rtf", StringComparison.Ordinal); // non-semantic text
		}

		/// <summary>
		/// Is the given string Rtf encoded?
		/// </summary>
		public static bool IsRtf(byte[] rtf)
		{
			byte[] firstFewBytes = new byte[10];
			for (int i = 0; i < Math.Min(firstFewBytes.Length, rtf.Length); i++)
			{
				firstFewBytes[i] = rtf[i];
			}
			return IsRtf(Encoding.UTF8.GetString(firstFewBytes));
		}

		/// <summary>
		/// Get an empty string in RTF format.
		/// </summary>
		public static string EmptyRtf => TextToRtf(string.Empty);

		/// <summary>
		/// Get an empty Rtf encoding as a byte array.
		/// </summary>
		public static byte[] EmptyRtfByteArray => Encoding.UTF8.GetBytes(EmptyRtf);

		/// <summary>
		/// Gets an RTF document in a human-readable form.
		/// </summary>
		public static string RtfToText(string rtf, bool isShowHiddenText = true)
		{
			if (!IsRtf(rtf))
			{
				return rtf;
			}

			PlaintextMarkupGeneratorConfiguration plainTextMarkupConfig = new("\n", "\r\n", "\n");
			var parsed = RtfParser.Parse(rtf.TrimEnd());
			if (isShowHiddenText)
			{
				parsed = parsed.Where(action => action is not VisitAction<IRtfNode> { Node: HiddenTextToggle });
			}
			return parsed.Decode()
				.Reduce(PlaintextEncoder.Factory)
				.Markup(plainTextMarkupConfig);
		}

		/// <summary>
		/// Convert Rtf text (as a byte array) into a human-readable text string.
		/// </summary>
		public static string RtfToText(byte[] rtfUTF8Bytes)
		{
			string rtf = Encoding.UTF8.GetString(rtfUTF8Bytes);
			return RtfToText(rtf);
		}

		/// <summary>
		/// Convert Rtf text (as a base 64 string) into a human-readable text string.
		/// </summary>
		public static string Base64RtfToText(string text)
		{
			byte[] data = Convert.FromBase64String(text);
			return RtfToText(data);
		}

		/// <summary>
		/// Get the first human-readable line of an RTF string.
		/// </summary>
		public static string GetRtfFirstTextLine(string rtf)
		{
			return GetRtfFirstTextLine(rtf, -1);
		}

		public static string TextToRtf(string text) => TextToRtf(text, (int)DefaultFontSize);

		public static Font DefaultFont => OFont.GetRichTextBoxFont();

		public static float DefaultFontSize => OFont.GetDefaultFontSize(OFontTypes.RichText);

		/// <summary>
		/// The unit for the fontSize parameter here is points.
		/// </summary>
		public static string TextToRtf(string text, int fontSize)
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			// CW1161 wants our default font to be translatable via Res.GetString, but it's the name of a font. We don't need a translation for font names.
			var phrase = fontSize > 0 ? new Phrase(Font: "Microsoft Sans Serif", Size: new Unit(fontSize, UnitType.Point)) : new Phrase();
#pragma warning restore CW1161 // Res.GetString Analyzer
			return PlaintextParser.Parse(text)
				.Decode()
				.WithTextStyles(phrase)
				.Reduce(RtfEncoder.CreateFactory())
				.Markup();
		}

		public static byte[] TextToRtfBytes(string text)
		{
			var result = TextToRtf(text);
			return Encoding.UTF8.GetBytes(result);
		}

		public static byte[] TextToRtfBytes(string text, int fontSize)
		{
			var result = TextToRtf(text, fontSize);
			return Encoding.UTF8.GetBytes(result);
		}

		/// <summary>
		/// Get the text version of the first line of an RTF encoded string.
		/// </summary>
		public static string GetRtfFirstTextLine(string rtf, int maxLength)
		{
			IEnumerableTree<IPlaintextNode> plainTree = IsRtf(rtf)
				? RtfParser.Parse(rtf)
					.Decode()
					.Reduce(PlaintextEncoder.Factory)
				: PlaintextParser.Parse(rtf);

			var result = plainTree
				.TakeWhile(x => x is not VisitAction<IPlaintextNode>)
				.Markup();

			if (maxLength > 0 && (maxLength < result.Length))
			{
				result = result.Substring(0, maxLength);
			}

			return result;
		}

		public static string GetRtfFirstTextLine(byte[] rtfUTF8Bytes)
		{
			return GetRtfFirstTextLine(rtfUTF8Bytes, -1);
		}

		public static string GetRtfFirstTextLine(byte[] rtfUTF8Bytes, int maxLength)
		{
			var rtf = Encoding.UTF8.GetString(rtfUTF8Bytes);
			return GetRtfFirstTextLine(rtf, maxLength);
		}

		public static bool EqualsExceptGeneratorInfo(string rtf1, string rtf2)
		{
			var result1 = GeneratorInfoRegex.Replace(rtf1, "");
			var result2 = GeneratorInfoRegex.Replace(rtf2, "");
			return result1 == result2;
		}

		// {\*\generator Riched20 10.0.17763}\viewkind4\uc1
		public static readonly Regex GeneratorInfoRegex = new Regex(@"{\\\*\\generator (\S*? )?\S*? \S*?}(\\viewkind4\\uc1)?", RegexOptions.Compiled);

		public static string AppendRtfStrings(string rtf1, string rtf2)
		{
			if (!IsRtf(rtf1))
			{
				rtf1 = TextToRtf(rtf1);
			}

			if (!IsRtf(rtf2))
			{
				rtf2 = TextToRtf(rtf2);
			}

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CW1161 // Res.GetString Analyzer
			return RtfParser.Parse(
				RtfUtils.Append(
					rtf1,
					rtf2,
					addParagraphBreak: true,
					baseStyle: new Phrase(Font: "Microsoft Sans Serif", Size: new Unit((int)DefaultFontSize, UnitType.Point))
				)
			)
#pragma warning restore CS0618 // Type or member is obsolete
					.Decode()
					.Reduce(RtfEncoder.CreateFactory())
					.Markup();
#pragma warning restore CW1161 // Res.GetString Analyzer
		}

		public static ZBlob ConcatRtfString(ZBlob blob, string toConcat)
		{
			string rtfText = blob.IsEmpty ? TextToRtf(toConcat) : AppendRtfStrings(blob.ToUTF8(), System.Environment.NewLine + System.Environment.NewLine + toConcat);
			return ZBlob.FromUTF8(rtfText);
		}

		public static string RtfToHtml(string rtf)
		{
			if (IsRtf(rtf))
			{
				var rtfToHtml = new RtfToHtmlConverter()
				{
					SanitizeLinks = true,
				};
				return rtfToHtml.Convert(rtf);
			}
			else
			{
				var textToHtml = new PlainTextToHtmlConverter() { SanitizeLinks = true };
				return textToHtml.Convert(rtf);
			}
		}

		public static ZBlob RtfToHtml(ZBlob blob)
		{
			if (IsRtf(blob))
			{
				var rtfToHtml = new RtfToHtmlConverter()
				{
					SanitizeLinks = true,
				};
				return ZBlob.FromUTF8(rtfToHtml.Convert(blob.ToUTF8()));
			}
			else
			{
				var textToHtml = new PlainTextToHtmlConverter() { SanitizeLinks = true };
				return ZBlob.FromUTF8(textToHtml.Convert(blob.ToUTF8()));
			}
		}

#if DEBUG
		internal static Regex GeneratorInfoRegex_Exposed => GeneratorInfoRegex;
#endif
	}
}
