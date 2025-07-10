using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CH.Business;

public static class SwissCustomsConstants
{
	public static class CustomsStatusCodes
	{
		public const string ShipmentRelease = "203";
		public const string CustomsDeclarationReceived = "204";
		public const string SubmittedToTaxud = "205";
	}

	public static class MeasurementUnits
	{
		internal const string NetWeightUOM = "KGM";
		internal const string GrossWeightUOM = "KGMG";
		internal const string UnitOfOneThousandUOM = "MIL";
	}

	public static class PostCodes
	{
		public const string Code7562 = "7562";
		public const string Code7563 = "7563";
	}

	[CodeAlive("HouseConsignmentCardinalityLimit is used in CH.NCTS solution")]
	public static class Limits
	{
		public const int HouseConsignmentCardinalityLimitV4 = 99;
		public const int HouseConsignmentCardinalityLimitV5 = 1999;
		public const decimal MaxTobaccoQuantityBasedTaxationCustomsValue = 1000.0m;
		public const decimal MaxTobaccoQuantityBasedTaxationCustomsQuantity = 10.0m;
		public const int ConsignmentItemsCardinalityLimit = 1999;
	}

	public static class SelectionResultCodes
	{
		public const string FreeWithout = "1";
		public const string FreeWith = "2";
		public const string Blocked = "3";
	}
}
