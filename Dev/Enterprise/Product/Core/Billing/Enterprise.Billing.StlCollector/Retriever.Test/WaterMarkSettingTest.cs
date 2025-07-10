using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	internal class WaterMarkSettingTest : TestCaseWithFactory
	{
		public void TestWaterMarkSettingNotCreatedUntilFirstSet()
		{
			_ = new WaterMarkSetting(Factory, SystemDataRegistry.StlCollectorHighWaterMarkPrefix + "COL");
			Factory.Save();
			var settings = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.Like, SystemDataRegistry.StlCollectorHighWaterMarkPrefix + "%"));
			AssertEquals("No setting should be created as we haven't set a value yet", 0, settings.Length);
		}
	}
}
