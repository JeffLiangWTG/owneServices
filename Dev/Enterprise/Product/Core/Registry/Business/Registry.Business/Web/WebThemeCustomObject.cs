using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[CodeProperty(Schema.ThemeName), DescriptionProperty(Schema.ThemeName)]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebThemeCustomObject : RegistryBusinessObjectTemplate
	{
		public WebThemeCustomObject()
		{
		}

		public WebThemeCustomObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ThemeName = "ThemeName";
			public const string CSS = "CSS";
			public const string DefaultThemeName = "Standard";
		}

		#endregion

		#region Properties

		public ZString ThemeName
		{
			get { return themeName; }
			set
			{
				if (themeName != value)
				{
					SetNonPersistentPropertyValue<ZString>(ThemeNameInfo, ref themeName, value);
					if (!IsValidationSuspended)
					{
						ValidateThemeName();
					}
				}
			}
		}
		ZString themeName;

		public ZPropertyInfo ThemeNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ThemeName));
			}
		}

		public bool ThemeName_ReadOnly
		{
			get { return ThemeName == Schema.DefaultThemeName; }
		}

		public void ValidateThemeName()
		{
			ThemeNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ThemeNameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ThemeNameInfo);
		}

		public ZString CSS
		{
			get { return css; }
			set
			{
				if (css != value)
				{
					SetNonPersistentPropertyValue<ZString>(CSSInfo, ref css, value);
				}
			}
		}
		ZString css;

		public ZPropertyInfo CSSInfo
		{
			get { return GetZPropertyInfo(nameof(CSS)); }
		}

		#endregion

		#region Images

		public WebCustomThemeImageBusinessObjectCollection ImageCollection
		{
			get
			{
				if (imageCollection == null)
				{
					imageCollection = new WebCustomThemeImageBusinessObjectCollection(CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(imageCollection);
				}
				return imageCollection;
			}
		}
		WebCustomThemeImageBusinessObjectCollection imageCollection;

		public WebCustomThemeImageBusinessObject FindImage(string fileName)
		{
			return ImageCollection.Cast<WebCustomThemeImageBusinessObject>().FirstOrDefault(i => i.ImageName.EqualsIgnoringCase(fileName));
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebThemeCustomObject();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var clonedImage = (WebThemeCustomObject)clone;
			using (clonedImage.GetValidationSuspender())
			{
				clonedImage.ThemeName = ThemeName;
				clonedImage.CSS = CSS;

				foreach (WebCustomThemeImageBusinessObject theme in ImageCollection)
				{
					var clonedTheme = (WebCustomThemeImageBusinessObject)theme.Clone(CurrentFallbackLevel, CurrentFactory);
					clonedImage.ImageCollection.Add(clonedTheme);
				}
			}
			clonedImage.ResumeValidation();
		}

		#endregion

		#region Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ThemeName = reader.ReadElementString(Schema.ThemeName);
			CSS = reader.ReadElementString(Schema.CSS);
			imageCollection = (WebCustomThemeImageBusinessObjectCollection)ImageCollectionSerialiser.Deserialize(reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ThemeName, ThemeName);
			writer.WriteElementString(Schema.CSS, CSS);
			ImageCollectionSerialiser.Serialize(writer, ImageCollection);
		}

		ZXmlSerializer imageCollectionSerialiser;
		ZXmlSerializer ImageCollectionSerialiser
		{
			get
			{
				if (imageCollectionSerialiser == null)
				{
					imageCollectionSerialiser = ZXmlSerializer.New(typeof(WebCustomThemeImageBusinessObjectCollection));
				}
				return imageCollectionSerialiser;
			}
		}

		#endregion

	}
}
