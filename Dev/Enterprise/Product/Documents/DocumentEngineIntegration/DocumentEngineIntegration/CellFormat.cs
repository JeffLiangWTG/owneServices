using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.DocumentEngineIntegration
{
	public class CellFormat
	{
		public CellBorderFormat Borders;
		public Color TextColor;
		public Color BackgroundColor;
		public bool IsBackgroundColorAutomatic;
		public FillPatternStyle FillPattern;
		public ZString FormatPattern;
		public HorizontalTextAlignment HTextAlign;
		public VerticalTextAlignment VTextAlign;
		public bool WrapText;

		public string FontName;
		public float FontSize;
		public FontStyle FontStyle;

		public CellFormat()
		{
			Borders = new CellBorderFormat();
			FillPattern = FillPatternStyle.None;
			TextColor = Color.Black;

			FontName = DefaultFontName;
			FontSize = DefaultFontSize;
			FontStyle = DefaultFontStyle;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Font name cannot be translated")]
		internal const string DefaultFontName = "Tahoma"; 
		internal const float DefaultFontSize = 8f;
		internal const FontStyle DefaultFontStyle = FontStyle.Regular;

		public bool FontBold => (FontStyle & FontStyle.Bold) != 0;

		public bool FontItalic => (FontStyle & FontStyle.Italic) != 0;

		public bool FontStrikeout => (FontStyle & FontStyle.Strikeout) != 0;

		public Font GetFont()
		{
			try
			{
				return new Font(FontName, FontSize, FontStyle);
			}
			catch (ArgumentException fontEx)
			{
				try
				{
					using (var fontFamily = new FontFamily(FontName))
					{
						if (!FontUtils.IsStyleAvailable(fontFamily, FontStyle.Regular))
						{
							if (FontUtils.IsStyleAvailable(fontFamily, FontStyle.Bold) && !FontBold)
							{
								return new Font(FontName, FontSize, FontStyle | FontStyle.Bold);
							}
							else if (FontUtils.IsStyleAvailable(fontFamily, FontStyle.Italic) && !FontItalic)
							{
								return new Font(FontName, FontSize, FontStyle | FontStyle.Italic);
							}
							else if (FontUtils.IsStyleAvailable(fontFamily, FontStyle.Bold | FontStyle.Italic) && (!FontBold || !FontItalic))
							{
								return new Font(FontName, FontSize, FontStyle | FontStyle.Bold | FontStyle.Italic);
							}
						}
						else
						{
							return new Font(FontName, FontSize, FontStyle.Regular);
						}
					}
				}
				catch (Exception fontEx2)
				{
					var fontInstalled = FontFamily.Families.Any(family => family.Name.Equals(FontName));
					var errorMessage = $@"FontName:{FontName}, FontSize:{FontSize}, FontStyle:{FontStyle}, Font Installed:{fontInstalled},
Pervious Exception:{fontEx.Message}
Now Exception:{fontEx2.Message}";
					throw new Exception(errorMessage, fontEx2);
				}
				throw;
			}
		}

		public void SetFontParametersFrom(Font font)
		{
			Argument.NotNull(font, nameof(font)); // Suggested By ReviewBot 

			FontName = font.Name;
			FontSize = font.Size;
			FontStyle = font.Style;
		}
	}

	public enum HorizontalTextAlignment
	{
		General = 0,
		Left = 1,
		Centre = 2,
		Right = 3,
		Justify = 4,
	}

	public enum VerticalTextAlignment
	{
		Top,
		Middle,
		Bottom
	}

	public enum FillPatternStyle
	{
		///<summary>Automatic </summary>
		Automatic = 0,

		///<summary>None </summary>
		None = 1,

		///<summary>Solid </summary>
		Solid = 2,

		///<summary>Gray50 </summary>
		Gray50 = 3,

		///<summary>Gray75 </summary>
		Gray75 = 4,

		///<summary>Gray25 </summary>
		Gray25 = 5,

		///<summary>Horizontal </summary>
		Horizontal = 6,

		///<summary>Vertical </summary>
		Vertical = 7,

		///<summary>Down </summary>
		Down = 8,

		///<summary>Up </summary>
		Up = 9,

		///<summary>Diagonal hatch.</summary>
		Checker = 10,

		///<summary>bold diagonal.</summary>
		SemiGray75 = 11,

		///<summary>thin horz lines </summary>
		LightHorizontal = 12,

		///<summary>thin vert lines</summary>
		LightVertical = 13,

		///<summary>thin \ lines</summary>
		LightDown = 14,

		///<summary>thin / lines</summary>
		LightUp = 15,

		///<summary>thin horz hatch</summary>
		Grid = 16,

		///<summary>thin diag</summary>
		CrissCross = 17,

		///<summary>12.5 % gray</summary>
		Gray16 = 18,

		///<summary>6.25 % gray</summary>
		Gray8 = 19,
	}

	public enum CellBorderStyle
	{
		///<summary>None</summary>
		None,

		///<summary>Thin</summary>
		Thin,

		///<summary>Medium</summary>
		Medium,

		///<summary>Dashed</summary>
		Dashed,

		///<summary>Dotted</summary>
		Dotted,

		///<summary>Thick</summary>
		Thick,

		///<summary>Double</summary>
		Double,

		///<summary>Hair</summary>
		Hair,

		///<summary>Medium_dashed</summary>
		Medium_dashed,

		///<summary>Dash_dot</summary>
		Dash_dot,

		///<summary>Medium_dash_dot</summary>
		Medium_dash_dot,

		///<summary>Dash_dot_dot</summary>
		Dash_dot_dot,

		///<summary>Medium_dash_dot_dot</summary>
		Medium_dash_dot_dot,

		///<summary>Slanted_dash_dot</summary>
		Slanted_dash_dot
	}

	public class CellBorder
	{
		public CellBorderStyle BorderStyle;
		public Color BorderColor;
	}

	public class CellBorderFormat
	{
		public CellBorder Top = new CellBorder();
		public CellBorder Bottom = new CellBorder();
		public CellBorder Left = new CellBorder();
		public CellBorder Right = new CellBorder();

		public CellBorderFormat()
		{
		}

		public void SetColor(Color color)
		{
			Top.BorderColor = color;
			Bottom.BorderColor = color;
			Left.BorderColor = color;
			Right.BorderColor = color;
		}

		public bool IsEmpty
		{
			get
			{
				return
					Top.BorderStyle == CellBorderStyle.None &&
					Bottom.BorderStyle == CellBorderStyle.None &&
					Left.BorderStyle == CellBorderStyle.None &&
					Right.BorderStyle == CellBorderStyle.None;
			}
		}

		public Color GetCommonColor()
		{
			var result = Color.Empty;

			if (IsUsedColorsAllTheSame)
			{
				result = GetUsedColors()[0];
			}

			return result;
		}

		public bool IsUsedColorsAllTheSame
		{
			get
			{
				var result = false;
				var usedColor = Color.Empty;

				foreach (var color in GetUsedColors())
				{
					if (usedColor.IsEmpty)
					{
						usedColor = color;
						result = true;
					}
					else
					{
						if (color.ToArgb() != usedColor.ToArgb())
						{
							result = false;
							break;
						}
					}
				}

				return result;
			}
		}

		Color[] GetUsedColors()
		{
			var result = new List<Color>();

			if (Top.BorderStyle != CellBorderStyle.None)
			{
				result.Add(Top.BorderColor);
			}

			if (Bottom.BorderStyle != CellBorderStyle.None)
			{
				result.Add(Bottom.BorderColor);
			}

			if (Left.BorderStyle != CellBorderStyle.None)
			{
				result.Add(Left.BorderColor);
			}

			if (Right.BorderStyle != CellBorderStyle.None)
			{
				result.Add(Right.BorderColor);
			}

			return result.ToArray();
		}
	}
}
