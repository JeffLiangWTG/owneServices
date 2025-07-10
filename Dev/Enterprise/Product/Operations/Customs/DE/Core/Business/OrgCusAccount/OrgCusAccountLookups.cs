using System.Collections;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class OrgCusAccountLookups : Enterprise.MasterFiles.Business.OrgCusAccountLookups
	{
		public OrgCusAccountLookups(AutoOrgCusAccount parent)
			: base(parent)
		{
		}

		public override IList IssuerList => Factory.GetCachedValue<OrgCusAccountIssuerList>();

		public override CodeDescriptionPairList CodeList => Factory.GetCachedValue<OrgCusAccountCodeList>();

		public override CodeDescriptionPairList AccountTypeList => Factory.GetCachedValue<OrgCusAccountTypeList>();
	}
}
