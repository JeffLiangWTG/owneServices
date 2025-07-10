using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BulkAddDiscountLookups
	{
		public BulkAddDiscountLookups(BulkAddDiscount parent, BusinessObjectFactory factory)
		{
			this.parent = parent;
			this.factory = factory;
		}
		readonly BulkAddDiscount parent;
		readonly BusinessObjectFactory factory;

		public CodeDescriptionPairList SystemCodes
		{
			get { return ClientLicenceBillingDiscountLookups.GetSystemCodes(); }
		}

		public CodeDescriptionPairList SubCodes
		{
			get { return ClientLicenceBillingDiscountLookups.GetSubCodes(parent.SystemCode, factory); }
		}

		public CodeDescriptionPairList DiscountTypes
		{
			get { return BillingConstants.GetDiscountTypeList(); }
		}

		public CodeDescriptionPairList ModuleCodes
		{
			get { return LicenceModuleList.Instance.Names; }
		}

		public CodeDescriptionPairList BreakUnits
		{
			get { return BillingConstants.GetDiscountBreakUnitList(); }
		}
	}
}

