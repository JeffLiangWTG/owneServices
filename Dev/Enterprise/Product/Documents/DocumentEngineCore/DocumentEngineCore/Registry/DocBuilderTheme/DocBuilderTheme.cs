using System.Drawing;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocBuilderTheme : NonPersistentBusinessObject, IDocBuilderTheme, IXmlSerializable, IObsoleteValidation
	{
		public static readonly Color DefaultPrimaryColor = Color.FromArgb(255, 150, 150, 150);
		public static readonly Color DefaultSecondaryColor = Color.FromArgb(255, 51, 51, 51);

		ZString name;
		MultilingualString nameMultilingual;
		readonly ZBool isCustomizable;
		DocBuilderThemeItem[] items;

		DocBuilderTheme()
		{
		}

		public DocBuilderTheme(ZString name, ZBool isCustomizable)
		: this(name, (NoResString)name, isCustomizable)
		{ }

		public DocBuilderTheme(ZString name, MultilingualString nameMultilingual, ZBool isCustomizable)
		{
			this.name = name;
			this.nameMultilingual = nameMultilingual;
			this.isCustomizable = isCustomizable;
		}

		public ZString Name
		{
			get { return name; }
		}

		public virtual ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		public MultilingualString NameMultilingual
		{
			get
			{
				return nameMultilingual ?? (NoResString)"";
			}
			set
			{
				nameMultilingual = value;
				NameMultilingualInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NameMultilingualInfo
		{
			get { return GetZPropertyInfo(nameof(NameMultilingual)); }
		}

		public ZBool IsCustomizable
		{
			get { return isCustomizable; }
		}

		public DocBuilderThemeItem[] Items
		{
			get { return items ?? (items = GetThemeItems()); }
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public static DocBuilderTheme FromXml(string xml)
		{
			return FromXml(xml, false);
		}

		public static DocBuilderTheme FromXml(string xml, bool isCustomizable)
		{
			var result = new DocBuilderTheme(string.Empty, isCustomizable);
			var textReader = new StringReader(xml);
			var xmlReader = XmlReader.Create(textReader);
			result.ReadXml(xmlReader);
			return result;
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement("DocBuilderTheme");
			name = reader.ReadElementString("Name");

			reader.ReadStartElement("DocBuilderThemeItems");
			foreach (var themeItem in Items)
			{
				themeItem.ReadXml(reader);
			}

			reader.ReadEndElement();
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("DocBuilderTheme");
			writer.WriteElementString("Name", name);

			writer.WriteStartElement("DocBuilderThemeItems");
			foreach (var themeItem in Items)
			{
				themeItem.WriteXml(writer);
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		public void CopyThemeItemsTo(DocBuilderTheme theme)
		{
			DocumentHeading.CopyTo(theme.DocumentHeading);
			PageNumberHeading.CopyTo(theme.PageNumberHeading);
			PrimaryHeading.CopyTo(theme.PrimaryHeading);
			PrimaryBody.CopyTo(theme.PrimaryBody);
			SecondaryHeading.CopyTo(theme.SecondaryHeading);
			SecondaryBody.CopyTo(theme.SecondaryBody);
		}

		public readonly DocBuilderThemeItem DocumentHeading = new DocumentHeadingDocBuilderThemeItem();
		public readonly DocBuilderThemeItem PageNumberHeading = new PageNumberHeadingDocBuilderThemeItem();
		public readonly DocBuilderThemeItem PrimaryHeading = new PrimaryHeadingDocBuilderThemeItem();
		public readonly DocBuilderThemeItem PrimaryBody = new PrimaryBodyDocBuilderThemeItem();
		public readonly DocBuilderThemeItem SecondaryHeading = new SecondaryHeadingDocBuilderThemeItem();
		public readonly DocBuilderThemeItem SecondaryBody = new SecondaryBodyDocBuilderThemeItem();

		DocBuilderThemeItem[] GetThemeItems()
		{
			return new DocBuilderThemeItem[]
			{
				DocumentHeading,
				PageNumberHeading,
				PrimaryHeading,
				PrimaryBody,
				SecondaryHeading,
				SecondaryBody,
			};
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class DocumentHeadingDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal DocumentHeadingDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.DocumentHeading;
				Description = DocBuilderThemeItemList.Descriptions.DocumentHeading;
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class PageNumberHeadingDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal PageNumberHeadingDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.PageNumberHeading;
				Description = DocBuilderThemeItemList.Descriptions.PageNumberHeading;
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class PrimaryHeadingDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal PrimaryHeadingDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.PrimaryHeading;
				Description = DocBuilderThemeItemList.Descriptions.PrimaryHeading;
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class PrimaryBodyDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal PrimaryBodyDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.PrimaryBody;
				Description = DocBuilderThemeItemList.Descriptions.PrimaryBody;
			}

			protected bool Color1Argb_ReadOnly { get { return true; } }
			protected bool FontName_ReadOnly { get { return true; } }
			protected bool FontSize_ReadOnly { get { return true; } }
			protected bool FontColorArgb_ReadOnly { get { return true; } }
			protected bool FontBold_ReadOnly { get { return true; } }
			protected bool FontItalic_ReadOnly { get { return true; } }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class SecondaryHeadingDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal SecondaryHeadingDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.SecondaryHeading;
				Description = DocBuilderThemeItemList.Descriptions.SecondaryHeading;
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		class SecondaryBodyDocBuilderThemeItem : DocBuilderThemeItem
		{
			internal SecondaryBodyDocBuilderThemeItem()
			{
				Name = DocBuilderThemeItemList.Codes.SecondaryBody;
				Description = DocBuilderThemeItemList.Descriptions.SecondaryBody;
			}

			protected bool Color1Argb_ReadOnly { get { return true; } }
			protected bool FontName_ReadOnly { get { return true; } }
			protected bool FontSize_ReadOnly { get { return true; } }
			protected bool FontColorArgb_ReadOnly { get { return true; } }
			protected bool FontBold_ReadOnly { get { return true; } }
			protected bool FontItalic_ReadOnly { get { return true; } }
		}
	}
}
