using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	public class SurveyCampaignCollectionProvider : CampaignCollectionProvider
	{
		public SurveyCampaignCollectionProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override IEnumerable<string> CampaignTypes
		{
			get { yield return CampaignTypeList.Codes.Survey; }
		}
	}
}
