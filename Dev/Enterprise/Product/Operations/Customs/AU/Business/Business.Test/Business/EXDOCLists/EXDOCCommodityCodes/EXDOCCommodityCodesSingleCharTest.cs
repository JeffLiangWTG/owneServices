using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCCommodityCodesSingleCharTest : TestCase
	{
		public void TestGetSingleCharFromThreeCharCode()
		{
			AssertEquals("Dairy", EXDOCCommodityCodesSingleChar.Codes.Dairy, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Dairy));
			AssertEquals("Eggs", EXDOCCommodityCodesSingleChar.Codes.Eggs, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Eggs));
			AssertEquals("Fish", EXDOCCommodityCodesSingleChar.Codes.Fish, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Fish));
			AssertEquals("GrainsAndPlants", EXDOCCommodityCodesSingleChar.Codes.GrainsAndSeeds, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.GrainsAndPlants));
			AssertEquals("Horticulture", EXDOCCommodityCodesSingleChar.Codes.Horticulture, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Horticulture));
			AssertEquals("InedibleMeat", EXDOCCommodityCodesSingleChar.Codes.InedibleMeat, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.InedibleMeat));
			AssertEquals("Meat", EXDOCCommodityCodesSingleChar.Codes.Meat, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Meat));
			AssertEquals("SkinsAndHides", EXDOCCommodityCodesSingleChar.Codes.SkinsAndHides, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.SkinsAndHides));
			AssertEquals("Wool", EXDOCCommodityCodesSingleChar.Codes.Wool, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(EXDOCCommodityCodes.Codes.Wool));
			AssertEquals("NotExistingCode", string.Empty, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode("BLA"));
		}

		public void TestGetThreeCharFromSingleCharCode()
		{
			AssertEquals("Dairy", EXDOCCommodityCodes.Codes.Dairy, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Dairy));
			AssertEquals("Eggs", EXDOCCommodityCodes.Codes.Eggs, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Eggs));
			AssertEquals("Fish", EXDOCCommodityCodes.Codes.Fish, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Fish));
			AssertEquals("GrainsAndPlants", EXDOCCommodityCodes.Codes.GrainsAndPlants, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.GrainsAndSeeds));
			AssertEquals("Horticulture", EXDOCCommodityCodes.Codes.Horticulture, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Horticulture));
			AssertEquals("InedibleMeat", EXDOCCommodityCodes.Codes.InedibleMeat, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.InedibleMeat));
			AssertEquals("Meat", EXDOCCommodityCodes.Codes.Meat, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Meat));
			AssertEquals("SkinsAndHides", EXDOCCommodityCodes.Codes.SkinsAndHides, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.SkinsAndHides));
			AssertEquals("Wool", EXDOCCommodityCodes.Codes.Wool, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode(EXDOCCommodityCodesSingleChar.Codes.Wool));
			AssertEquals("NotExistingCode", string.Empty, EXDOCCommodityCodesSingleChar.GetThreeCharFromSingleCharCode("?"));
		}
	}
}
