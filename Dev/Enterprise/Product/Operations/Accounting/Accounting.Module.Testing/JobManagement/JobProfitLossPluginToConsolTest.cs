using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobProfitLossPluginToConsolTest : TestCaseWithFactory
	{
		public void TestProfitLossContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals("No profit loss", 0, consol.ProfitLossContainer.Count);

			using (JobProfitLossPluginToConsol plugin = new JobProfitLossPluginToConsol(consol))
			{
				AssertEquals("Profit Loss Created", 1, consol.ProfitLossContainer.Count);
			}
		}
	}
}
