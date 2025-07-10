using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusBRForeignOperatorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();

			var lookups = foreignOperator.Lookups;

			AssertEquals(5, lookups.MessageStatusList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ACC", "AWA", "", "REJ", "FAL" }, lookups.MessageStatusList.GetAllCodes());
		}

		public void TestCustomsStatusTypeList()
		{
			var foreignOperation = Factory.New<CusBRForeignOperator>();
			var lookups = foreignOperation.Lookups;

			AssertEquals(2, lookups.CustomsStatusTypeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ACT", "DEA" }, lookups.CustomsStatusTypeList.GetAllCodes());
		}
	}
}
