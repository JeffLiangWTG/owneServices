using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing
{
	class NctsMessageNumberStrategyTest : TestCaseWithFactory
	{
		public void TestGetMessageReferenceNumber()
		{
			var strategy = new NctsMessageNumberStrategy(Factory, "NCT");
			AssertEquals("Message Number", "1", strategy.GetMessageReferenceNumber());
		}
	}
}
