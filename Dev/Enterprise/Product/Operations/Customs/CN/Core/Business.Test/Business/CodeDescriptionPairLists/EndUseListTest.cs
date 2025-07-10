using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class EndUseListTest : TestCaseWithFactory
	{
		public void TestGetRequiredCargoAttributesByCIQEndUse()
		{
			AssertNoExceptionThrown("Should not throw exception even with empty string.", () => EndUseList.GetRequiredCargoAttributesByCIQEndUse(""));
			AssertNull("Should return null for other inputs", EndUseList.GetRequiredCargoAttributesByCIQEndUse("11"));
			AssertContainsExactElementsInAnyOrder("Should return correct result for other 12", new[] { "14", "15" }, EndUseList.GetRequiredCargoAttributesByCIQEndUse("12"));
			AssertContainsExactElementsInAnyOrder("Should return correct result for other 27", new[] { "14", "15" }, EndUseList.GetRequiredCargoAttributesByCIQEndUse("27"));
		}
	}
}
