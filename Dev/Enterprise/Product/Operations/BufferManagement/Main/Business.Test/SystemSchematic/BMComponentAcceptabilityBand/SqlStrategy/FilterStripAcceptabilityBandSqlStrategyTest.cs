using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	public class FilterStripAcceptabilityBandSqlStrategyTest : TestCaseWithFactory
	{
		public void TestTVPsAreAddedWithoutHittingBandsFactory()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var bandTypesToCheck = new AcceptabilityBandTypes().Cast<ICodeDescription>().Where(c => c.Code != AcceptabilityBandTypes.Codes.SQL).ToArray();
			var bands = new List<BMComponentAcceptabilityBand>();

			var tag = BMSTestHelper.CreateWorkQueue(Factory, "QUE", "Some work queue to avoid throwing exceptions fort the QAG band");

			foreach (var bandType in bandTypesToCheck)
			{
				var band = Factory.New<BMComponentAcceptabilityBand>();
				band.BAB_Name = $"{bandType.Code} band";
				band.BAB_Type = bandType.Code;
				band.BAB_FC_Component = config.Buffer.PK;
				bands.Add(band);
			}

			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var band in bands)
				{
					var bandFactory = new BusinessObjectFactory() { NameForDebugging = $"{band.BAB_Type} factory" };
					var loadedBand = bandFactory.Load<BMComponentAcceptabilityBand>(band.PK);

					var parameters = new AcceptabilityBandSqlBuilderParameters(loadedBand)
					{
						Tag = tag //some work queue tag for the QAG band to avoid throwing exceptions (other bands will ignore this)
					};

					var task = WorkflowFactory.GetMatchingWorkflows(loadedBand, parameters);
					AssertNoExceptionThrown($"Should not throw cross thread access exceptions for {loadedBand.BAB_Type}", () =>
					{
						AssertNotNull($"Should get the result for {loadedBand.BAB_Type}", task.ConfigureAwait(false).GetAwaiter().GetResult());
					});
				}
			});
		}
	}
}
