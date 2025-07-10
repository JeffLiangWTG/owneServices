using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocOrgBuyerLink : DocOrgSupplierBuyerLink
	{
		DocOrgBuyerLink(OrgSupplierBuyerLink buyerLink, BusinessObjectFactory factoryToWrap)
			: base(buyerLink, factoryToWrap)
		{
		}

		public static DocOrgBuyerLink New(OrgSupplierBuyerLink buyerLink, BusinessObjectFactory factoryToWrap)
		{
			return buyerLink != null ? new DocOrgBuyerLink(buyerLink, factoryToWrap) : null;
		}

		public override DocOrganisation ToParty
		{
			get { return Buyer; }
		}

		public override DocOrganisation FromParty
		{
			get { return Supplier; }
		}

		public override DocContacts ToPartyContact
		{
			get { return BuyerContact; }
		}

		public override DocContacts FromPartyContact
		{
			get { return SupplierContact; }
		}
	}
}
