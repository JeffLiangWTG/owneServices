using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class SalesRelationRuleNodeAdditionalTypesList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string AnySingleActivity = "?";
			public const string AnyNumberOfActivities = "*";
		}

		public static class Descriptions
		{
			public static MultilingualString AnySingleActivity { get { return ResString.GetMultilingualString("SalesRelationNodeAdditionalTypesList|AnySingleActivity", "Any Activity"); } }
			public static MultilingualString AnyNumberOfActivities { get { return ResString.GetMultilingualString("SalesRelationNodeAdditionalTypesList|AnyNumberOfActivities", "Any Number of Activities"); } }
		}
		public SalesRelationRuleNodeAdditionalTypesList()
		{
			AddPair(Codes.AnySingleActivity, Descriptions.AnySingleActivity);
			AddPair(Codes.AnyNumberOfActivities, Descriptions.AnyNumberOfActivities);
		}
	}
}
