using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.JP.Common.Utils
{
	public static class BondedLocationCodeTypeDecider
	{
		public static bool IsMarineProductsExportLocation(this ZZRefCusCodeListCombined refCusCodeListCombined)
		{
			return GetBondedLocationCodeType(refCusCodeListCombined).EqualsIgnoringCase(BondedLocationCodeTypeList.Codes.MarineProductsExportLocation);
		}

		public static ZString GetBondedLocationCodeType(this ZZRefCusCodeListCombined refCusCodeListCombined)
		{
			if (refCusCodeListCombined.ZZD_Description == "その他")
			{
				return BondedLocationCodeTypeList.Codes.Other;
			}

			var attribute = refCusCodeListCombined.Attributes.Where(attribute => attribute.ZZE_ZXE_NKName.EqualsIgnoringCase(RefCusCodeListAttributeTypes.Codes.Type)).FirstOrDefault();
			var locationCodeTypeJP = attribute?.ZZE_Value;

			switch (locationCodeTypeJP)
			{
				case "官署":
					return BondedLocationCodeTypeList.Codes.CustomsOffice;
				case "蔵置場":
					return BondedLocationCodeTypeList.Codes.BondedWarehouse;
				case "指定":
					return BondedLocationCodeTypeList.Codes.DesignatedBondedArea;
				case "展示場":
					return BondedLocationCodeTypeList.Codes.CustomsDisplayArea;
				case "工場":
					return BondedLocationCodeTypeList.Codes.CustomsFactory;
				case "ﾊﾞﾝﾆﾝｸ":
					return BondedLocationCodeTypeList.Codes.OtherVanningLocation;
				case "洋上":
					return BondedLocationCodeTypeList.Codes.MarineProductsExportLocation;
				case "他所蔵置":
					return BondedLocationCodeTypeList.Codes.ExBondStorage;
				case "ふ中扱":
					return BondedLocationCodeTypeList.Codes.BargeHandling;
				case "到着即時":
					return BondedLocationCodeTypeList.Codes.DeclaredOnArrival;
				case "本船扱":
					return BondedLocationCodeTypeList.Codes.ShipHandling;
				case "総合":
					return BondedLocationCodeTypeList.Codes.IntegratedBondedArea;
				case "貨物到着前":
					return BondedLocationCodeTypeList.Codes.DeclaredPreArrival;
				default:
					return "";
			}
		}
	}
}
