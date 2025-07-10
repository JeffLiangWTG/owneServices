using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Integration
{
	public static class RatingBehaviours
	{
		public const string Default = "";
		public const string Spot = "SPT";
		public const string AllInBSAOverridingSpotRate = "SBA";
		public const string FreightBSAOverridingSpotRate = "SBF";
		public const string AllInAdhocOverridingSpotRate = "SAA";
		public const string FreightAdhocOverridingSpotRate = "SAF";
		public const string CreateNewCharge = "NEW";
		public const string ReAutorateCharge = "REA";
		public const string StopFromAutorating = "STP";

		public static class Descriptions
		{
			public static string Default { get { return string.Empty; } }
			public static string Spot { get { return Res.GetString("78E0C88F-0CE5-4166-B941-1628BF948626", "Spot"); } }
			public static string AllInBSAOverridingSpotRate { get { return Res.GetString("627EA6CF-5FF9-4F29-93E4-61B74CA19C43", "Spot Rate (All In) for BSA overriding Autorating"); } }
			public static string FreightBSAOverridingSpotRate { get { return Res.GetString("545093B4-328B-45F2-A9F3-9790EF297403", "Spot Rate (Freight) for BSA overriding Autorating"); } }
			public static string AllInAdhocOverridingSpotRate { get { return Res.GetString("2778FFCD-6CCC-4F6C-AD5C-C14F85CE0C88", "Spot Rate (All In) for Ad-hoc overriding Autorating"); } }
			public static string FreightAdhocOverridingSpotRate { get { return Res.GetString("BEB36685-1345-45AF-BF1E-94A33A2454AF", "Spot Rate (Freight) for Ad-hoc overriding Autorating"); } }
			public static string CreateNewCharge { get { return Res.GetString("4e5eebb4-87db-4415-aa28-0f2b2d8cdfbd", "Create new Charge during AutoRating"); } }
			public static string ReAutorateCharge { get { return Res.GetString("282033f6-8fc2-41e3-b171-55ebd6b8eed4", "Clear and Re-autorate this charge"); } }
			public static string StopFromAutorating { get { return Res.GetString("83dc5daa-4425-4082-a508-77eac2afcd1g", "Stop this charge from AutoRating"); } }
		}

		public static bool IsSpotBehaviour(string behaviour)
		{
			return (behaviour == Spot) || IsAutoRatingOverriderSpotBehaviour(behaviour);
		}

		public static bool IsAutoRatingOverriderSpotBehaviour(string behaviour)
		{
			return IsFreightChargeGroupOverriderBehaviour(behaviour) || IsSpecificChargeOverriderSpotBehaviour(behaviour);
		}

		public static bool IsFreightChargeGroupOverriderBehaviour(string behaviour)
		{
			return (behaviour == AllInAdhocOverridingSpotRate) || (behaviour == AllInBSAOverridingSpotRate);
		}

		public static bool IsSpecificChargeOverriderSpotBehaviour(string behaviour)
		{
			return (behaviour == FreightBSAOverridingSpotRate) || (behaviour == FreightAdhocOverridingSpotRate);
		}

		public static CodeDescriptionPairList GetSpotRatingBehavioursList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Default, Descriptions.Default);
			result.AddPair(Spot, Descriptions.Spot);
			result.AddPair(FreightAdhocOverridingSpotRate, Descriptions.FreightAdhocOverridingSpotRate);
			result.AddPair(AllInAdhocOverridingSpotRate, Descriptions.AllInAdhocOverridingSpotRate);
			result.AddPair(FreightBSAOverridingSpotRate, Descriptions.FreightBSAOverridingSpotRate);
			result.AddPair(AllInBSAOverridingSpotRate, Descriptions.AllInBSAOverridingSpotRate);

			return result;
		}
	}
}
