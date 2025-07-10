using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class EXDOCCommodityCodesTest : TestCase
	{
		public void TestIsHorticultureOrGrainsAndPlants()
		{
			AssertEquals("GrainsAndPlants", true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.GrainsAndPlants));
			AssertEquals("Horticulture", true, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Horticulture));
			AssertEquals("Meat", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Meat));
			AssertEquals("SkinsAndHides", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.SkinsAndHides));
			AssertEquals("Wool", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Wool));
			AssertEquals("Dairy", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Dairy));
			AssertEquals("Eggs", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Eggs));
			AssertEquals("Fish", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.Fish));
			AssertEquals("InedibleMeat", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.InedibleMeat));
			AssertEquals("OtherGoods", false, EXDOCCommodityCodes.IsHorticultureOrGrainsAndPlants(EXDOCCommodityCodes.Codes.OtherGoods));
		}

		public void TestIsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides()
		{
			AssertEquals("GrainsAndPlants", true, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.GrainsAndPlants));
			AssertEquals("Horticulture", true, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Horticulture));
			AssertEquals("Meat", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Meat));
			AssertEquals("SkinsAndHides", true, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.SkinsAndHides));
			AssertEquals("Wool", true, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Wool));
			AssertEquals("Dairy", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Dairy));
			AssertEquals("Eggs", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Eggs));
			AssertEquals("Fish", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.Fish));
			AssertEquals("InedibleMeat", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.InedibleMeat));
			AssertEquals("OtherGoods", false, EXDOCCommodityCodes.IsWoolOrGrainsAndPlantsOrHorticultureOrSkinsAndHides(EXDOCCommodityCodes.Codes.OtherGoods));
		}
	}
}
