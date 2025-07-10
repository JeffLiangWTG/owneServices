using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Manifest.Business;

public class WeightUnitCodeList : CodeDescriptionPairList
{
	public static class Codes
	{
		public const string Kilogram = Core.Constants.Weight.Kilograms;
		public const string Pound = Core.Constants.Weight.Pounds;
		public const string MetricTon = Core.Constants.Weight.Tonnes;
		public const string TenGrams = "TG";
		public const string BritainTon = "BT";
		public const string Quintal = "Q";
	}

	public static class Descriptions
	{
		public static MultilingualString Kilogram { get { return Core.Constants.Weight.GetDescription(Codes.Kilogram, Core.Constants.PluralState.NonPlural); } }
		public static MultilingualString Pound { get { return Core.Constants.Weight.GetDescription(Codes.Pound, Core.Constants.PluralState.NonPlural); } }
		public static MultilingualString MetricTon { get { return Core.Constants.Weight.GetDescription(Codes.MetricTon, Core.Constants.PluralState.NonPlural); } }
		public static MultilingualString TenGrams { get { return ResString.GetMultilingualString("INManWeightUnitCodeList|TenGrams", "Ten Grams"); } }
		public static MultilingualString BritainTon { get { return ResString.GetMultilingualString("INManWeightUnitCodeList|BritainTon", "Britain Ton"); } }
		public static MultilingualString Quintal { get { return ResString.GetMultilingualString("INManWeightUnitCodeList|Quintal", "One Hundred Kilogram"); } }
	}

	public WeightUnitCodeList()
	{
		AddPair(Codes.Kilogram, Descriptions.Kilogram);
		AddPair(Codes.Pound, Descriptions.Pound);
		AddPair(Codes.MetricTon, Descriptions.MetricTon);
		AddPair(Codes.TenGrams, Descriptions.TenGrams);
		AddPair(Codes.BritainTon, Descriptions.BritainTon);
		AddPair(Codes.Quintal, Descriptions.Quintal);
	}
}
