using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebCustomThemeImageBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate<WebCustomThemeImageBusinessObject>
	{
		public WebCustomThemeImageBusinessObjectCollection()
			: this(null, null)
		{
		}

		public WebCustomThemeImageBusinessObjectCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WebCustomThemeImageBusinessObject();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebCustomThemeImageBusinessObjectCollection();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
