using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ChargeDistributeByListTest : TestCaseWithFactory
	{
		public void TestChargeDistributeByList()
		{
			var chargeDistributeByList = new ChargeDistributeByList();
			AssertEquals(6, chargeDistributeByList.Count);
			AssertContainsExactElementsInAnyOrder(chargeDistributeByList.GetAllCodes(), new[] { "NWT", "QTY", "VAL", "VOL", "WGT", "FOB" });
			AssertEquals(chargeDistributeByList.GetDescriptionFromCode("NWT"), "Net Weight");
			AssertEquals(chargeDistributeByList.GetDescriptionFromCode("FOB"), "FOB Value");
		}
	}
}
