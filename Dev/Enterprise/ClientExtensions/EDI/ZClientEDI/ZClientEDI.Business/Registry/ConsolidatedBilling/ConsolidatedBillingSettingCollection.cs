using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ConsolidatedBillingSettingCollection : RegistryBusinessObjectCollectionTemplate<ConsolidatedBillingSetting>
	{
		public ConsolidatedBillingSettingCollection() : this(null, null)
		{
		}

		public ConsolidatedBillingSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ConsolidatedBillingSetting(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ConsolidatedBillingSettingCollection(fallbackLevel, factory);
		}
	}
}
