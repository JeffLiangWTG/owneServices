using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "HBLDeliveryPriorities")]
	public class HBLDeliveryPriorityConfigCollection : RegistryBusinessObjectCollectionTemplate
	{
		public HBLDeliveryPriorityConfigCollection()
			: this(null, null)
		{
		}

		public HBLDeliveryPriorityConfigCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new HBLDeliveryPriorityConfig(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new HBLDeliveryPriorityConfigCollection(fallbackLevel, factory);

		public new HBLDeliveryPriorityConfig this[int i]
			=> (HBLDeliveryPriorityConfig)Elements[i];

		public new HBLDeliveryPriorityConfig AddNew()
			=> (HBLDeliveryPriorityConfig)base.AddNew();
	}
}
