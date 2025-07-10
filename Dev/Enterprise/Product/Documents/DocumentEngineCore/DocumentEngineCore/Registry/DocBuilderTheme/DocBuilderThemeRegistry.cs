using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocBuilderThemeRegistry : RegistryBusinessObjectTemplate, IObsoleteValidation
	{
		DocBuilderTheme selectedTheme;
		public DocBuilderTheme SelectedTheme
		{
			get { return selectedTheme ?? Themes[0]; }
			set { selectedTheme = value; }
		}

		List<DocBuilderTheme> themes;
		public List<DocBuilderTheme> Themes
		{
			get { return themes ?? (themes = GetSystemThemes()); }
		}

		void ResetThemes()
		{
			themes = null;
		}

		List<DocBuilderTheme> GetSystemThemes()
		{
			var result = new List<DocBuilderTheme>();
			XmlDocument xmlDocument = new XmlDocument();
			var resourceName = "Enterprise.DocumentEngineCore.Registry.DocBuilderTheme.SystemDocBuilderThemes.xml";
			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				xmlDocument.Load(stream);
			}

			foreach (XmlNode xmlNode in xmlDocument.SelectNodes(@"SystemDocBuilderThemes/DocBuilderTheme"))
			{
				var xml = xmlNode.OuterXml;
				result.Add(DocBuilderTheme.FromXml(xml));
			}

			return result;
		}

		public DocBuilderTheme FindTheme(ZString name)
		{
			return Array.Find(Themes.ToArray(), theme => theme.Name.EqualsIgnoringCase(name));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteStartElement("DocBuilderThemeSelector");
			writer.WriteElementString("SelectedThemeName", SelectedTheme != null ? SelectedTheme.Name.ToString() : string.Empty);

			writer.WriteStartElement("CustomizableThemes");
			foreach (var theme in Themes)
			{
				if (theme.IsCustomizable)
				{
					theme.WriteXml(writer);
				}
			}

			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper readerWrapper)
		{
			ResetThemes();
			var reader = readerWrapper.Reader;

			reader.ReadStartElement("DocBuilderThemeSelector");
			var selectedThemeName = reader.ReadElementString("SelectedThemeName");

			ReadCustomizableThemes(reader);

			reader.ReadEndElement();

			SelectedTheme = FindTheme(selectedThemeName);
		}

		void ReadCustomizableThemes(XmlReader reader)
		{
			var nodeName = "CustomizableThemes";
			if (reader.Name.Equals(nodeName))
			{
				if (reader.IsEmptyElement)
				{
					reader.ReadStartElement(nodeName);
				}
				else
				{
					reader.ReadStartElement(nodeName);

					while (reader.Name.Equals("DocBuilderTheme"))
					{
						var xml = reader.ReadOuterXml();
						var theme = DocBuilderTheme.FromXml(xml, true);
						Themes.Add(theme);
					}

					reader.ReadEndElement();
				}
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new DocBuilderThemeRegistry();

			result.themes = new List<DocBuilderTheme>();
			foreach (DocBuilderTheme theme in this.Themes)
			{
				var newTheme = new DocBuilderTheme(theme.Name, theme.NameMultilingual, theme.IsCustomizable);
				theme.CopyThemeItemsTo(newTheme);
				result.Themes.Add(newTheme);
			}

			if (SelectedTheme != null)
			{
				result.SelectedTheme = result.FindTheme(SelectedTheme.Name);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string DefaultThemeName = "Standard";
	}
}
