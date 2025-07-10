using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class BillingDbUsageCodesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new BillingDbUsageCodes this[int index]
		{
			get { return (BillingDbUsageCodes)Elements[index]; }
		}

		public new BillingDbUsageCodes AddNew()
		{
			return (BillingDbUsageCodes)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillingDbUsageCodes();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BillingDbUsageCodesCollection();
		}
	}
}

