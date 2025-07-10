using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingUnitCountAdjustmentCollection : RegistryBusinessObjectCollectionTemplate<BillingUnitCountAdjustment>
	{
		public BillingUnitCountAdjustmentCollection() : this(null, null)
		{
		}

		public BillingUnitCountAdjustmentCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingUnitCountAdjustmentCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillingUnitCountAdjustment(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
