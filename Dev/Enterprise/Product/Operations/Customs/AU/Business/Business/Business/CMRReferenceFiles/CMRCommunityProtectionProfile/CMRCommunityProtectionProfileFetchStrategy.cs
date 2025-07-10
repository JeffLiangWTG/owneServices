using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionProfileFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CMRCommunityProtectionProfileFetchStrategy(CMRCommunityProtectionProfile profile)
			: base(profile)
		{
		}

		CMRCommunityProtectionProfile profile
		{
			get { return BusinessObject as CMRCommunityProtectionProfile; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(CMRCommunityProtectionRisk), CMRCommunityProtectionRiskSchema.CK_Identifier, profile.CP_CommunityProtectionRiskIdentifier);
		}
	}
}
