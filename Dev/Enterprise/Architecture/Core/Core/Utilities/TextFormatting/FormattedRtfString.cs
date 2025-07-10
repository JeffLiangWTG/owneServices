using System;
using System.Drawing;
using System.Linq;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;

namespace Enterprise.ZArchitecture.Core
{
	public struct FormattedRtfString
	{
		public FormattedRtfString(Font defaultFont)
		{
			fDefaultFont = defaultFont;
			fParts = null;
			fRtf = null;
		}

		public static FormattedRtfString operator +(FormattedRtfString lhs, FormattedRtfPart part)
		{
			FormattedRtfString result = lhs;
			result.fParts = new FormattedRtfPart[lhs.Parts.Length + 1];
			lhs.Parts.CopyTo(result.fParts, 0);
			result.fParts[result.fParts.Length - 1] = part;
			return result;
		}

		public static FormattedRtfString operator +(FormattedRtfString lhs, FormattedRtfString rhs)
		{
			FormattedRtfString result = lhs;
			result.fParts = new FormattedRtfPart[lhs.Parts.Length + rhs.Parts.Length];
			lhs.Parts.CopyTo(result.fParts, 0);
			rhs.Parts.CopyTo(result.fParts, lhs.Parts.Length);
			return result;
		}

		public static FormattedRtfString operator +(FormattedRtfString value, string text)
		{
			return value + new FormattedRtfPart(text, value.DefaultFont);
		}

		public FormattedRtfString Add(string text, FontStyle style)
		{
			return this + new FormattedRtfPart(text, new Font(DefaultFont, style));
		}

		Font fDefaultFont;
		public Font DefaultFont
		{
			get
			{
				if (fDefaultFont == null)
				{
					fDefaultFont = ORtfTextUtil.DefaultFont;
				}
				return fDefaultFont;
			}
		}

		string fRtf;
		public string ToRtf()
			=> fRtf ??= EnumerableTree.Merge(
					Equals,
					Parts.Select(part =>
						PlaintextParser
						.Parse(part.Text)
						.Decode()
						.WithTextStyles(new Phrase()
						{
							Font = part.Font.FontFamily.Name,
							Size = new Unit(part.Font.Size, UnitType.Point),
							Bold = part.Font.Bold,
							Italic = part.Font.Italic,
							Underline = part.Font.Underline,
							Strikethrough = part.Font.Strikeout,
						})
					)
				)
				.Reduce(RtfEncoder.CreateFactory())
				.Markup();

		public override string ToString()
		{
			string result = "";
			foreach (FormattedRtfPart part in Parts)
			{
				result += part.Text;
			}
			return result;
		}

		public bool IsEmpty
		{
			get { return Parts.Length == 0; }
		}

		#region Implementation

		#region Constants

		static readonly FormattedRtfPart[] EmptyRtfPartArray = Array.Empty<FormattedRtfPart>();

		#endregion

		FormattedRtfPart[] fParts;
		FormattedRtfPart[] Parts
		{
			get { return fParts ?? EmptyRtfPartArray; }
		}

		#endregion
	}

	public struct FormattedRtfPart
	{
		public FormattedRtfPart(string text, Font font)
		{
			Text = text;
			Font = font;
		}

		public readonly string Text;
		public readonly Font Font;
	}
}
