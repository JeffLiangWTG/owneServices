using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "Settings")]
	public class AutoratingViaPortSettingCollection : RegistryBusinessObjectCollectionTemplate<AutoratingViaPortSetting>
	{
		public AutoratingViaPortSettingCollection()
			: this(null, null)
		{
		}

		public AutoratingViaPortSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public AutoratingViaPortConfiguration ParentConfiguration { get; internal set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new AutoratingViaPortSetting(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var newCollection = new AutoratingViaPortSettingCollection(fallbackLevel, factory);
			newCollection.ParentConfiguration = ParentConfiguration;

			return newCollection;
		}
	}
}
