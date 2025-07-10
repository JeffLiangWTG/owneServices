using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MX.Business.Testing
{
	class MXRefCusCodeListTypesTest : TestCaseWithFactory
	{
		public void TestGetCustomsFacilitiesList()
		{
			ReferenceTestDataHelper.CreateCustomsFacilitiesCodes(Factory);
			var list = MXRefCusCodeListTypes.GetCustomsFacilities(Factory);
			list.Load();
			AssertContainsExactElementsInAnyOrder(new[] { "123", "45" }, list.Select(x => x.ZZD_Code));
			AssertSame(list, MXRefCusCodeListTypes.GetCustomsFacilities(Factory));
		}
	}
}
