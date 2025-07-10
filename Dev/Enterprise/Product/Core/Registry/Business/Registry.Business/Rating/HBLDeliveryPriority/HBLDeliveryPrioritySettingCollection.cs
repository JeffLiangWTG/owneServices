using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "HBLDeliveryFallBackPriorities")]
	public class HBLDeliveryPrioritySettingCollection : RegistryBusinessObjectCollectionTemplate<HBLDeliveryPrioritySetting>
	{
		public HBLDeliveryPrioritySettingCollection()
			: this(null, null)
		{
		}

		public HBLDeliveryPrioritySettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public HBLDeliveryPriorityConfig ParentConfiguration { get; internal set; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new HBLDeliveryPrioritySetting(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var newCollection = new HBLDeliveryPrioritySettingCollection(fallbackLevel, factory);
			newCollection.ParentConfiguration = ParentConfiguration;

			return newCollection;
		}
	}
}
