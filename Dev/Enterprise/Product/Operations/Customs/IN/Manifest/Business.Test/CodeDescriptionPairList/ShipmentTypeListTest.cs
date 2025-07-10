using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class ShipmentTypeCodeListTest : TestCaseWithFactory
{
	public void TestList()
	{
		var list = new ShipmentTypeList();
		AssertEquals(3, list.Count);
		AssertEquals("Total", list.GetDescriptionFromCode("T"));
		AssertEquals("Part-Shipment", list.GetDescriptionFromCode("P"));
		AssertEquals("Split", list.GetDescriptionFromCode("S"));
	}
}
