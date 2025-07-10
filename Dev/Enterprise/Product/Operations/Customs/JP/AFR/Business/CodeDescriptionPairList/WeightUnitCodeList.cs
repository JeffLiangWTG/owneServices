using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class WeightUnitCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Kilogram = Core.Constants.Weight.Kilograms;
			public const string Pound = Core.Constants.Weight.Pounds;
			public const string MetricTon = Core.Constants.Weight.Tonnes;
		}

		public static class Descriptions
		{
			public static MultilingualString Kilogram { get { return Core.Constants.Weight.GetDescription(Codes.Kilogram, Core.Constants.PluralState.NonPlural); } }
			public static MultilingualString Pound { get { return Core.Constants.Weight.GetDescription(Codes.Pound, Core.Constants.PluralState.NonPlural); } }
			public static MultilingualString MetricTon { get { return Core.Constants.Weight.GetDescription(Codes.MetricTon, Core.Constants.PluralState.NonPlural); } }
		}

		public WeightUnitCodeList()
		{
			AddPair(Codes.Kilogram, Descriptions.Kilogram);
			AddPair(Codes.Pound, Descriptions.Pound);
			AddPair(Codes.MetricTon, Descriptions.MetricTon);
		}
	}
}
