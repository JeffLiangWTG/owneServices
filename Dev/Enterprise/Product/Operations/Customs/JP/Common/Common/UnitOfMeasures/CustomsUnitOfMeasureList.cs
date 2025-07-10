using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public static class CustomsUnitOfMeasureList
	{
		static CodeDescriptionPairList UnitConversions(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("JPCustoms.UnitConversions", delegate
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(Core.Constants.Weight.Decitons, CodeDTN);
				result.AddPair(Core.Constants.Weight.Grams, CodeGRM);
				result.AddPair(Core.Constants.Weight.Hectograms, CodeHGM);
				result.AddPair(Core.Constants.Weight.Kilograms, CodeKGM);
				result.AddPair(Core.Constants.Weight.Kilotonnes, CodeKTN);
				result.AddPair(Core.Constants.Weight.Pounds, CodeLBR);
				result.AddPair(Core.Constants.Weight.PoundsTroy, CodeLBT);
				result.AddPair(Core.Constants.Weight.MetricCarat, CodeCTM);
				result.AddPair(Core.Constants.Weight.Milligrams, CodeMGM);
				result.AddPair(Core.Constants.Weight.OuncesTroy, CodeAPZ);
				result.AddPair(Core.Constants.Weight.Ounces, CodeONZ);
				result.AddPair(Core.Constants.Weight.Tonnes, CodeTNE);
				result.AddPair(Core.Constants.Weight.LongTons, CodeLTN);
				result.AddPair(Core.Constants.Weight.ShortTons, CodeSTN);

				return result;
			});
		}

		public const string CodeDTN = "DTN";
		public const string CodeGRM = "GRM";
		public const string CodeHGM = "HGM";
		public const string CodeKGM = "KGM";
		public const string CodeKTN = "KTN";
		public const string CodeLBR = "LBR";
		public const string CodeLBT = "LBT";
		public const string CodeCTM = "CTM";
		public const string CodeMGM = "MGM";
		public const string CodeAPZ = "APZ";
		public const string CodeONZ = "ONZ";
		public const string CodeTNE = "TNE";
		public const string CodeLTN = "LTN";
		public const string CodeSTN = "STN";

		public static string ConvertGrossUnitsToJPCustomsGrossUnits(string grossUnits, BusinessObjectFactory factory)
		{
			return UnitConversions(factory).GetDescriptionFromCode(grossUnits) ?? grossUnits;
		}

		public static string ConvertJPCustomsGrossUnitsToGrossUnits(string jPCustomsGrossUnits, BusinessObjectFactory factory)
		{
			return UnitConversions(factory).GetCodeFromDescription(jPCustomsGrossUnits) ?? jPCustomsGrossUnits;
		}

		public static CodeDescriptionPairList GetVolumeUnitList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue($"JPCustoms.VolumeUnitList", () =>
			{
				var result = new CodeDescriptionPairList(OLookUpEditType.Volume);
				result.AddPairIfNotExist(VolumeList.Codes.BoardFoot, VolumeList.Descriptions.BoardFoot);
				result.Sort();
				return result;
			});
		}
	}
}
