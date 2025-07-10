using System.Drawing;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ColorThemeSelector : RegistryBusinessObjectTemplate, IObsoleteValidation
	{
		#region Properties

		#region Chosen Theme

		[List("ColorThemeList")]
		[MaxLength(ThemeNameMaxLength)]
		public ZString ChosenThemeName
		{
			get { return chosenThemeName; }
			set
			{
				if (chosenThemeName != value)
				{
					CheckMaximumLength(ChosenThemeNameInfo, value);
					chosenThemeName = value;
					ChosenThemeNameInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						ValidateChosenThemeName();
					}
				}
			}
		}

		ZString chosenThemeName;

		const int ThemeNameMaxLength = 50;

		public ZPropertyInfo ChosenThemeNameInfo
		{
			get { return GetZPropertyInfo(nameof(ChosenThemeName)); }
		}

		void ValidateChosenThemeName()
		{
			ChosenThemeNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ChosenThemeNameInfo);
			ListValidation.ErrorIfInvalidCode(ChosenThemeNameInfo, ColorThemeList);
		}

		public ColorTheme ChosenTheme
		{
			get { return ColorThemeList.GetThemeFromName(ChosenThemeName); }
		}

		#endregion

		#region Copy Theme

		[List("ColorThemeList")]
		[MaxLength(ThemeNameMaxLength)]
		public ZString ColorThemeToCopy
		{
			get { return colorThemeToCopy; }
			set
			{
				if (colorThemeToCopy != value)
				{
					CheckMaximumLength(ColorThemeToCopyInfo, value);
					colorThemeToCopy = value;
					ColorThemeToCopyInfo.RefreshBinding();
				}
			}
		}

		ZString colorThemeToCopy;

		public ZPropertyInfo ColorThemeToCopyInfo
		{
			get { return GetZPropertyInfo(nameof(ColorThemeToCopy)); }
		}

		public ColorTheme TemplateCopyTheme
		{
			get { return ColorThemeList.GetThemeFromName(ColorThemeToCopy); }
		}

		#endregion

		#endregion

		public void SetDefaultsFromTheme()
		{
			CopyColorsOver(TemplateCopyTheme, ChosenTheme);

			ColorThemeToCopy = ZString.Empty;
		}

		#region Lookups

		public ColorThemeList ColorThemeList
		{
			get { return fColorThemeList ?? (fColorThemeList = new ColorThemeList()); }
		}
		ColorThemeList fColorThemeList;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var theme = new ColorThemeSelector();
			theme.ChosenThemeName = ChosenThemeName;

			foreach (ColorThemeList.ColorThemeItem themeItem in theme.ColorThemeList)
			{
				CopyColorsOver(ColorThemeList.GetThemeFromName(themeItem.Code), themeItem.Theme);
			}

			return theme;
		}

		static void CopyColorsOver(ColorTheme src, ColorTheme dest)
		{
			if (dest != null && dest.CanBeModified && src != null)
			{
				for (int i = 0; i < src.ChosenColors.Length; i++)
				{
					dest.ChosenColors[i].Color = src.ChosenColors[i].Color;
				}
			}
		}

		public override bool Equals(object obj)
		{
			var otherColor = obj as ColorThemeSelector;
			return otherColor != null &&
				otherColor.ChosenThemeName == ChosenThemeName && (
					ChosenTheme == null ||
					!ChosenTheme.CanBeModified ||
					Enumerable.SequenceEqual(ChosenTheme.ChosenColors, otherColor.ChosenTheme.ChosenColors)
				);
		}

		public override int GetHashCode()
		{
			return ChosenTheme?.GetHashCode() ?? 23;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement("ChosenThemName");
			writer.WriteValue(chosenThemeName);
			writer.WriteEndElement();

			writer.WriteStartElement("CustomThemes");

			foreach (ColorThemeList.ColorThemeItem themeItem in ColorThemeList)
			{
				if (themeItem.Theme.CanBeModified)
				{
					var customThemeNumber = themeItem.Theme.Name.GetUnresolvedString().Replace("Custom ", "");
					writer.WriteStartElement("Theme" + customThemeNumber);
					foreach (DescribedColor item in themeItem.Theme.ChosenColors)
					{
						writer.WriteStartElement(item.ColorPropertyName);
						Color toWrite = item.Color.ToArgb() == 0 ? SystemColors.Control : item.Color;
						writer.WriteValue(toWrite.ToArgb().ToString());
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			chosenThemeName = reader.ReadElementString("ChosenThemName");

			reader.Reader.ReadStartElement("CustomThemes");

			while (reader.Reader.Name.StartsWith((NoResString)"Theme"))
			{
				CustomColorTheme customTheme = (CustomColorTheme)ColorThemeList.GetThemeFromName((NoResString)"Custom " + reader.Reader.Name.Replace((NoResString)"Theme", (NoResString)""));
				if (customTheme != null)
				{
					reader.Reader.ReadStartElement(reader.Reader.Name);
					foreach (DescribedColor item in customTheme.ChosenColors)
					{
						string readValue = reader.ReadElementString(item.ColorPropertyName);
						if (!string.IsNullOrEmpty(readValue))
						{
							int parsedValue = int.Parse(readValue);
							if (parsedValue != 0)
							{
								item.Color = Color.FromArgb(255, Color.FromArgb(parsedValue)); //ensure opacity
							}
							else
							{
								item.Color = SystemColors.Control;
							}
						}
						else
						{
							item.Color = SystemColors.Control;
						}
					}

					reader.Reader.ReadEndElement();
				}
			}

			reader.Reader.ReadEndElement();
		}
		#endregion
	}
}
