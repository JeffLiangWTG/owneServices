using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class DeliveryOrderResponseStatusListTest : TestCaseWithFactory
	{
		public void TestList()
		{
			var list = new DeliveryOrderResponseStatusList();
			AssertEquals(3, list.Count);
			AssertEquals("Accepted", list.GetDescriptionFromCode("1"));
			AssertEquals("Received With Errors", list.GetDescriptionFromCode("2"));
			AssertEquals("Reject", list.GetDescriptionFromCode("3"));
		}
	}
}
