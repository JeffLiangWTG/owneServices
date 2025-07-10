using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebThemeCustomObjectCollection : RegistryBusinessObjectCollectionTemplate<WebThemeCustomObject>
	{
		public WebThemeCustomObjectCollection()
			: this(null, null)
		{
		}

		public WebThemeCustomObjectCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WebThemeCustomObject(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebThemeCustomObjectCollection(fallbackLevel, factory);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public WebThemeCustomObject Find(ZString themeName)
		{
			var result = this.Cast<WebThemeCustomObject>().FirstOrDefault(t => t.ThemeName.EqualsIgnoringCase(themeName)) ?? this.Cast<WebThemeCustomObject>().FirstOrDefault(t => t.ThemeName.EqualsIgnoringCase(WebThemeCustomObject.Schema.DefaultThemeName));
			return result;
		}
	}
}
