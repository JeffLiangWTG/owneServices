using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.Business
{
	public partial class VolumeUnitCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string BoardFootMeasureTimber = "BF";
			public const string CubicFeet = Core.Constants.Volume.CubicFeet;
			public const string CubicMeter = Core.Constants.Volume.CubicMetres;
		}

		public static class Descriptions
		{
			public static MultilingualString BoardFootMeasureTimber { get { return ResString.GetMultilingualString("VolumeUnitCodeList|BoardFootMeasureTimber", "Board Foot Measure (timber)"); } }
			public static MultilingualString CubicFeet { get { return Core.Constants.Volume.GetDescription(Codes.CubicFeet, Core.Constants.PluralState.Plural); } }
			public static MultilingualString CubicMeter { get { return Core.Constants.Volume.GetDescription(Codes.CubicMeter, Core.Constants.PluralState.NonPlural); } }
		}

		public VolumeUnitCodeList()
		{
			AddPair(Codes.BoardFootMeasureTimber, Descriptions.BoardFootMeasureTimber);
			AddPair(Codes.CubicFeet, Descriptions.CubicFeet);
			AddPair(Codes.CubicMeter, Descriptions.CubicMeter);
		}
	}
}
