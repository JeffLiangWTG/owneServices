using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.Customs.FR.Registry.Testing
{
	public class VATReportConfigurationLookupsTest : TestCaseWithFactory
	{
		public void TestConfigurationLookups()
		{
			var columnConfigurationsManager = new ColumnConfigurationsManager(FRCustomsDataRegistry.FRVATReportPK, false);
			var manager = new CombinedConfigurationManager(columnConfigurationsManager, "AAA");
			manager.Save();

			var lookups = VATReportConfigurationLookups.ConfigurationLookups;
			AssertContains("AAA", lookups.CodesAsString);
		}
	}
}
