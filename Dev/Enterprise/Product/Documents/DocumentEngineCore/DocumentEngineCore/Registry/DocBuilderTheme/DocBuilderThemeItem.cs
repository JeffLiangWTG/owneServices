using System;
using System.Drawing;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry
{
	public abstract class DocBuilderThemeItem : NonPersistentBusinessObject, IXmlSerializable, IObsoleteValidation
	{
		ZString name;
		ZInt color1Argb;
		ZInt color2Argb;
		ZString fontName;
		ZInt fontSize;
		ZInt fontColorArgb;
		ZBool fontBold;
		ZBool fontItalic;
		Font font;

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString Name
		{
			get { return name; }
			set
			{
				if (value != name)
				{
					CheckMaximumLength(NameInfo, value);
					name = value;
					NameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		public MultilingualString Description { get; set; }

		public ZInt Color1Argb
		{
			get { return color1Argb; }
			set
			{
				if (value != color1Argb)
				{
					color1Argb = value;
					Color1ArgbInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo Color1ArgbInfo
		{
			get { return GetZPropertyInfo(nameof(Color1Argb)); }
		}

		public Color Color1
		{
			get { return Color.FromArgb(Color1Argb); }
			set { Color1Argb = value.ToArgb(); }
		}

		public ZInt Color2Argb
		{
			get { return color2Argb; }
			set
			{
				if (value != color2Argb)
				{
					color2Argb = value;
					Color2ArgbInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo Color2ArgbInfo
		{
			get { return GetZPropertyInfo(nameof(Color2Argb)); }
		}

		public Color Color2
		{
			get { return Color.FromArgb(Color2Argb); }
			set { Color2Argb = value.ToArgb(); }
		}

		public ZInt FontColorArgb
		{
			get { return fontColorArgb; }
			set
			{
				if (value != fontColorArgb)
				{
					fontColorArgb = value;
					FontColorArgbInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FontColorArgbInfo
		{
			get { return GetZPropertyInfo(nameof(FontColorArgb)); }
		}

		public Color FontColor
		{
			get { return Color.FromArgb(FontColorArgb); }
			set { FontColorArgb = value.ToArgb(); }
		}

		[CargoWise.ComponentModel.MaxLength(128)]
		public ZString FontName
		{
			get { return fontName; }
			set
			{
				if (value != fontName)
				{
					CheckMaximumLength(FontNameInfo, value);
					fontName = value;
					FontNameInfo.RefreshBinding();
					ResetFont();
				}
			}
		}

		public ZPropertyInfo FontNameInfo
		{
			get { return GetZPropertyInfo(nameof(FontName)); }
		}

		public ZInt FontSize
		{
			get { return fontSize; }
			set
			{
				if (value != fontSize)
				{
					fontSize = value;
					FontSizeInfo.RefreshBinding();
					ResetFont();
				}
			}
		}

		public ZPropertyInfo FontSizeInfo
		{
			get { return GetZPropertyInfo(nameof(FontSize)); }
		}

		public ZBool FontBold
		{
			get { return fontBold; }
			set
			{
				if (value != fontBold)
				{
					fontBold = value;
					FontBoldInfo.RefreshBinding();
					ResetFont();
				}
			}
		}

		public ZPropertyInfo FontBoldInfo
		{
			get { return GetZPropertyInfo(nameof(FontBold)); }
		}

		public ZBool FontItalic
		{
			get { return fontItalic; }
			set
			{
				if (value != fontItalic)
				{
					fontItalic = value;
					FontItalicInfo.RefreshBinding();
					ResetFont();
				}
			}
		}

		public ZPropertyInfo FontItalicInfo
		{
			get { return GetZPropertyInfo(nameof(FontItalic)); }
		}

		public Font Font
		{
			get { return font ?? (font = GetValidFont()); }
			set
			{
				FontName = value.Name;
				FontSize = Convert.ToInt32(value.Size);
				FontBold = value.Bold;
				FontItalic = value.Italic;
				ResetFont();
			}
		}

		void ResetFont()
		{
			font = null;
		}

		Font GetValidFont()
		{
			var fontFamily = FontFamily.Families.Find(FontName);
			if (fontFamily == null)
			{
				FontName = defaultFontName;
				fontFamily = FontFamily.Families.Find(FontName);
			}
			if (fontFamily != null)
			{
				var style = FontStyle.Regular;
				var isStyleSet = false;

				if (FontBold && FontUtils.IsStyleAvailable(fontFamily, FontStyle.Bold))
				{
					style |= FontStyle.Bold;
					style ^= FontStyle.Regular;
					isStyleSet = true;
				}

				if (FontItalic && FontUtils.IsStyleAvailable(fontFamily, FontStyle.Italic))
				{
					style |= FontStyle.Italic;
					style ^= FontStyle.Regular;
					isStyleSet = true;
				}

				if (!isStyleSet || !FontUtils.IsStyleAvailable(fontFamily, style))
				{
					style = GetDefaultStyle(fontFamily);
				}

				return new Font(FontName, FontSize, style);
			}

			return null;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string defaultFontName = "Arial";

		FontStyle GetDefaultStyle(FontFamily fontFamily)
		{
			var styles = new FontStyle[]
			{
				FontStyle.Regular,
				FontStyle.Italic,
				FontStyle.Bold,
				FontStyle.Bold | FontStyle.Italic
			};

			return Array.Find(styles, delegate(FontStyle style)
			{
				return FontUtils.IsStyleAvailable(fontFamily, style);
			});
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement("DocBuilderThemeItem");
			Name = reader.ReadElementString("Name");
			Color1 = Color.FromArgb(ZInt.ParseEmptyAsZero(reader.ReadElementString("Color1")));
			Color2 = Color.FromArgb(ZInt.ParseEmptyAsZero(reader.ReadElementString("Color2")));
			FontName = reader.ReadElementString("FontFamily");
			FontSize = ZInt.ParseEmptyAsZero(reader.ReadElementString("FontSize"));
			FontBold = new ZBool(reader.ReadElementString("FontBold"));
			FontItalic = new ZBool(reader.ReadElementString("FontItalic"));
			FontColor = Color.FromArgb(ZInt.ParseEmptyAsZero(reader.ReadElementString("FontColor")));
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("DocBuilderThemeItem");
			writer.WriteElementString("Name", Name);
			writer.WriteElementString("Color1", Color1.ToArgb().ToString());
			writer.WriteElementString("Color2", Color2.ToArgb().ToString());
			writer.WriteElementString("FontFamily", FontName);
			writer.WriteElementString("FontSize", FontSize.ToString());
			writer.WriteElementString("FontBold", FontBold.ToString());
			writer.WriteElementString("FontItalic", FontItalic.ToString());
			writer.WriteElementString("FontColor", FontColor.ToArgb().ToString());
			writer.WriteEndElement();
		}

		public void CopyTo(DocBuilderThemeItem themeItem)
		{
			themeItem.name = name;
			themeItem.color1Argb = color1Argb;
			themeItem.color2Argb = color2Argb;
			themeItem.fontName = fontName;
			themeItem.fontSize = fontSize;
			themeItem.fontColorArgb = fontColorArgb;
			themeItem.fontBold = fontBold;
			themeItem.fontItalic = fontItalic;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}
	}

	static partial class ExtenstionMethods
	{
		internal static FontFamily Find(this FontFamily[] fontFamilies, string name)
		{
			return Array.Find(fontFamilies,
				delegate(FontFamily fontFamily)
				{
					return fontFamily.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
				});
		}
	}
}
