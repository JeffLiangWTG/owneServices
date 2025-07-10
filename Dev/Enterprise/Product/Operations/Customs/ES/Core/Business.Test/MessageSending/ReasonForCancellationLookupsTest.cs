using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ReasonForCancellationLookupsTest : TestCaseWithFactory
	{
		public void TestReasonForCancellationList()
		{
			var lookups = new ReasonForCancellationLookups(new ReasonForCancellation(Factory));
			AssertEquals("01, 02, 03, 04", lookups.ReasonForCancellationList.CodesAsString);
		}
	}
}
