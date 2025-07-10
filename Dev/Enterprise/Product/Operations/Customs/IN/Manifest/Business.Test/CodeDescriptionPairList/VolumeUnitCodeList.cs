using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class VolumeUnitCodeListTest : TestCaseWithFactory
{
	public void TestList()
	{
		var list = new VolumeUnitCodeList();
		AssertEquals(6, list.Count);
		AssertEquals("US Gallons", list.GetDescriptionFromCode("UG"));
		AssertEquals("Milliliter", list.GetDescriptionFromCode("ML"));
		AssertEquals("Kiloliter", list.GetDescriptionFromCode("KL"));
		AssertEquals("Cubic Feet", list.GetDescriptionFromCode("CF"));
		AssertEquals("Cubic Centimeters", list.GetDescriptionFromCode("CC"));
		AssertEquals("Liter", list.GetDescriptionFromCode("L"));
	}
}
