using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Billing
{
	public class AccBillingEvents : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CostPosted = "CST";
			public const string RevenuePosted = "REV";
			public const string JRJPosted = "JRJ";
			public const string GWSellApportionmentPosted = "APP";
			public const string GenericPostEvent = "PST";
		}

		public static class Descriptions
		{
			public static MultilingualString CostPosted { get { return ResString.GetMultilingualString("AccBillingEventCode|Cost", "Cost Posted"); } }
			public static MultilingualString RevenuePosted { get { return ResString.GetMultilingualString("AccBillingEventCode|Revenue", "Revenue Posted"); } }
			public static MultilingualString JRJPosted { get { return ResString.GetMultilingualString("AccBillingEventCode|JRJ", "Job Revenue Journal Posted"); } }
			public static MultilingualString GWSellApportionmentPosted { get { return ResString.GetMultilingualString("AccBillingEventCode|GWSaleApportionment", "Gateway Sell Apportionment Posted"); } }
			public static MultilingualString GenericPostEvent { get { return ResString.GetMultilingualString("AccBillingEventCode|GenericPostEvent", "Posted"); } }
		}

		public AccBillingEvents()
		{
			AddPair(Codes.CostPosted, Descriptions.CostPosted);
			AddPair(Codes.RevenuePosted, Descriptions.RevenuePosted);
			AddPair(Codes.JRJPosted, Descriptions.JRJPosted);
			AddPair(Codes.GWSellApportionmentPosted, Descriptions.GWSellApportionmentPosted);
			AddPair(Codes.GenericPostEvent, Descriptions.GenericPostEvent);
		}
	}
}
