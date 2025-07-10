using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MarketingManager.Business
{
	public class EDICampaignTypeList : CampaignTypeList
	{
		public new abstract class Codes : CampaignTypeList.Codes
		{
			public const string ProductConsultantSurvey = "PCS";
			public const string WiseServicePartnerSurvey = "WSP";
		}

		public new abstract class Descriptions : CampaignTypeList.Descriptions
		{
			public const string ProductConsultantSurvey = "Product Consultant Survey";
			public const string WiseServicePartnerSurvey = "Wise Service Partner Survey";
		}

		public EDICampaignTypeList()
		{
			// Do not add ProductConsultantSurvey into the list that is used as suggestions for DropEdit controls
			// The product consultant survey campaigns are programmatically created in 
			// Enterprise.Client.EDI.Training.Business.TrainingSurveyCampaignSingleton
			AddPair(Codes.WiseServicePartnerSurvey, Descriptions.WiseServicePartnerSurvey);
		}
	}
}

