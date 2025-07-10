using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Manifest.Business;

public class VolumeUnitCodeList : CodeDescriptionPairList
{
	public static class Codes
	{
		public const string CubicFeet = Core.Constants.Volume.CubicFeet;
		public const string CubicCentimeters = Core.Constants.Volume.CubicCentimeters;
		public const string Litre = Core.Constants.Volume.Litre;
		public const string Kiloliter = "KL";
		public const string Milliliter = "ML";
		public const string USGallon = "UG";
	}
	public static class Descriptions
	{
		public static MultilingualString CubicFeet { get { return Core.Constants.Volume.GetDescription(Codes.CubicFeet, Core.Constants.PluralState.Plural); } }
		public static MultilingualString CubicCentimeters { get { return Core.Constants.Volume.GetDescription(Codes.CubicCentimeters, Core.Constants.PluralState.NonPlural); } }
		public static MultilingualString Litre { get { return Core.Constants.Volume.GetDescription(Codes.Litre, Core.Constants.PluralState.Plural); } }
		public static MultilingualString Kiloliter { get { return ResString.GetMultilingualString("INManVolumeUnitCodeList|Kiloliter", "Kiloliter"); } }
		public static MultilingualString Milliliter { get { return ResString.GetMultilingualString("INManVolumeUnitCodeList|Milliliter", "Milliliter"); } }
		public static MultilingualString USGallon { get { return ResString.GetMultilingualString("INManVolumeUnitCodeList|USGallon", "US Gallons"); } }
	}

	public VolumeUnitCodeList()
	{
		AddPair(Codes.CubicFeet, Descriptions.CubicFeet);
		AddPair(Codes.CubicCentimeters, Descriptions.CubicCentimeters);
		AddPair(Codes.Litre, Descriptions.Litre);
		AddPair(Codes.Kiloliter, Descriptions.Kiloliter);
		AddPair(Codes.Milliliter, Descriptions.Milliliter);
		AddPair(Codes.USGallon, Descriptions.USGallon);
	}
}
