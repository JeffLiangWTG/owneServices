using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CampaignCollectionProvider : CollectionProvider
	{
		public CampaignCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return new GlbCompanyCampaignCollection(BusinessObjectFactory, AdditionalQuery);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbCompanyCampaign;

		ZQuery AdditionalQuery
		{
			get
			{
				if (additionalQuery == null)
				{
					additionalQuery = new ZQuery();
					IEnumerable<string> campaignTypes = CampaignTypes;
					if (campaignTypes != null)
					{
						additionalQuery.AddToFilter(JoinCondition.Or, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, new List<string>(campaignTypes));
					}
				}
				return additionalQuery;
			}
		}
		ZQuery additionalQuery;

		public virtual IEnumerable<string> CampaignTypes => null;
	}

	public class VotingCampaignCollectionProvider : CampaignCollectionProvider
	{
		public VotingCampaignCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override IEnumerable<string> CampaignTypes
		{
			get { yield return CampaignTypeList.Codes.Voting; }
		}
	}
}
