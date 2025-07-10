using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	partial class EXDOCCommodityCodes
	{
		public static bool IsHorticultureOrGrainsAndPlants(ZString produceType)
		{
			return produceType == EXDOCCommodityCodes.Codes.Horticulture || produceType == EXDOCCommodityCodes.Codes.GrainsAndPlants;
		}

		public static bool IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(ZString produceType)
		{
			return produceType == EXDOCCommodityCodes.Codes.Horticulture ||
				produceType == EXDOCCommodityCodes.Codes.SkinsAndHides ||
				produceType == EXDOCCommodityCodes.Codes.GrainsAndPlants ||
				produceType == EXDOCCommodityCodes.Codes.Wool;
		}
	}
}
