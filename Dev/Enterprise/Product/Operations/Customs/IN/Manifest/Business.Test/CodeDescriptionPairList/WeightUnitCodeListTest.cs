using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class WeightUnitCodeListTest : TestCaseWithFactory
{
	public void TestList()
	{
		var list = new WeightUnitCodeList();
		AssertEquals(6, list.Count);
		AssertEquals("One Hundred Kilogram", list.GetDescriptionFromCode("Q"));
		AssertEquals("Britain Ton", list.GetDescriptionFromCode("BT"));
		AssertEquals("Ten Grams", list.GetDescriptionFromCode("TG"));
		AssertEquals("Kilogram", list.GetDescriptionFromCode("KG"));
		AssertEquals("Pound", list.GetDescriptionFromCode("LB"));
		AssertEquals("Tonne", list.GetDescriptionFromCode("T"));
	}
}
