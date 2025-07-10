using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class OrgCusAccountProvider : Enterprise.MasterFiles.Business.OrgCusAccountProvider
	{
		public OrgCusAccountProvider()
			: base(Core.Constants.CountryCodes.Germany)
		{
		}

		public override Enterprise.MasterFiles.Business.OrgCusAccountLookups GetNewLookups(OrgCusAccount orgCusAccount) => new OrgCusAccountLookups(orgCusAccount);

		public override Enterprise.MasterFiles.Business.OrgCusAccountValidation GetNewValidation(OrgCusAccount orgCusAccount) => new OrgCusAccountValidation(orgCusAccount);
	}
}
