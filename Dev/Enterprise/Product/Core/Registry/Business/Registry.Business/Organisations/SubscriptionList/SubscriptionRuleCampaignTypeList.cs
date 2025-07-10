using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class SubscriptionRuleCampaignTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string ClientRelationshipManagement = "CRM";
			public const string HumanResourcesManagement = "HRM";
		}

		/// <summary>
		/// Shows description only in the drop down edit box
		/// </summary>
		public SubscriptionRuleCampaignTypeList()
		{
			AddPair(Codes.ClientRelationshipManagement, Descriptions.ClientRelationshipManagement);
			AddPair(Codes.HumanResourcesManagement, Descriptions.HumanResourcesManagement);
		}

		public static class Descriptions
		{
			public static MultilingualString ClientRelationshipManagement { get { return ResString.GetMultilingualString("SubscriptionRuleCampaignTypeList|ClientRelationshipManagement", "Client Relationship Management"); } }
			public static MultilingualString HumanResourcesManagement { get { return ResString.GetMultilingualString("SubscriptionRuleCampaignTypeList|HumanResourcesManagement", "Human Resources Management"); } }
		}
	}
}
