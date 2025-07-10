using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class OrgCusAccountLookups : Enterprise.MasterFiles.Business.OrgCusAccountLookups
	{
		public OrgCusAccountLookups(AutoOrgCusAccount parent)
			: base(parent)
		{
		}

		protected new OrgCusAccount Parent => (OrgCusAccount)base.Parent;

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<OrgCusAccountCodeList>();

		public override CodeDescriptionPairList AccountTypeList => GetTypeList(Parent.CZ_Code);

		CodeDescriptionPairList GetTypeList(ZString accountCode)
		{
			return Factory.GetCachedValue("FR.OrgCusAccountLookups." + accountCode, () =>
			{
				var accountTypeList = new CodeDescriptionPairList();
				if (accountCode == OrgCusAccountCodeList.Codes.DGI || accountCode == OrgCusAccountCodeList.Codes.DGE)
				{
					accountTypeList = new OrgCusAccountDeltaGTypeList();
				}
				else if (accountCode == OrgCusAccountCodeList.Codes.DTA)
				{
					accountTypeList = new OrgCusAccountDeltaTTypeList();
				}
				else if (accountCode == OrgCusAccountCodeList.Codes.DEC)
				{
					accountTypeList = new OrgCusAccountDeltaIETypeList();
				}

				return accountTypeList;
			});
		}

		public override CodeDescriptionPairList ReportingPeriodList => Factory.GetCachedValue<ReportingPeriodList>();

		public override IList IssuerList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
	}
}
