using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlobalTrackingShipmentVisibilityServiceEhubIDCollection : RegistryBusinessObjectCollection
	{
		public GlobalTrackingShipmentVisibilityServiceEhubIDCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlobalTrackingShipmentVisibilityServiceEhubID(CurrentFallbackLevel);
		}

		public GlobalTrackingShipmentVisibilityServiceEhubIDCollection(FallbackLevel currentFallbackLevel)
			: base(currentFallbackLevel)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GlobalTrackingShipmentVisibilityServiceEhubIDCollection();
		}

		public new GlobalTrackingShipmentVisibilityServiceEhubID this[int i]
		{
			get { return (GlobalTrackingShipmentVisibilityServiceEhubID)base[i]; }
		}

		public new GlobalTrackingShipmentVisibilityServiceEhubID AddNew()
		{
			return (GlobalTrackingShipmentVisibilityServiceEhubID)base.AddNew();
		}

		public GlobalTrackingShipmentVisibilityOptions Parent { get; set; }

		public GlobalTrackingShipmentVisibilityServiceEhubID Add(ZString code, ZString service, ZString ehubID)
		{
			var result = AddNew();
			result.Code = code;
			result.Service = service;
			result.EhubID = ehubID;

			return result;
		}
	}
}
