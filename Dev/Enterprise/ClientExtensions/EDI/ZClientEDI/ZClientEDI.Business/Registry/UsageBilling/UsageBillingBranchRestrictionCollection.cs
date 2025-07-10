using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageBillingBranchRestrictionCollection : RegistryBusinessObjectCollectionTemplate<UsageBillingBranchRestriction>
	{
		public UsageBillingBranchRestrictionCollection() : this(null, null)
		{
		}

		public UsageBillingBranchRestrictionCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UsageBillingBranchRestrictionCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UsageBillingBranchRestriction(CurrentFallbackLevel, CurrentFactory);
		}

		public bool IsInvoicingBranchAllowed(string productCode, ZGuid invoicingBranchPK)
		{
			var branches = this.OfType<UsageBillingBranchRestriction>().Where(x => x.ProductCode.EqualsIgnoringCase(productCode));
			if (branches.Any())
			{
				return branches.Any(x => x.InvoicingBranch == invoicingBranchPK);
			}
			else
			{
				return true;
			}
		}
	}
}
