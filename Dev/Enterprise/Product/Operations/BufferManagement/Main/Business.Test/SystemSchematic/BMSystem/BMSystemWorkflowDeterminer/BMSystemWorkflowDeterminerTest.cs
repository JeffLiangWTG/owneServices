using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemWorkflowDeterminer))]
	class BMSystemWorkflowDeterminerTest : EnterpriseBusinessObjectTestCase
	{
		#region Indexes

		public void TestFSW_WorkflowType_ForNormalWorkflowTypes_ShouldUseNaturalKeyCache()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			BMSTestHelper.AddWorkflowType(system, "WKI");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDeterminer = newFactory.LoadFromNaturalKey<BMSystemWorkflowDeterminer>(BMSystemWorkflowDeterminerSchema.FSW_WorkflowType, "WKI");
			AssertNotNull(loadedDeterminer);

			loadedDeterminer.FSW_WorkflowType = "INQ";

			var isNkInCache = newFactory.IsInNaturalKeyCache_ForTest(typeof(BMSystemWorkflowDeterminer), BMSystemWorkflowDeterminerSchema.FSW_WorkflowType, new ZString("INQ"));
			AssertEquals("The workflow type is being set to something other than SIM, so the NK cache should be updated because there can only be one of each determiner that is a non-SIM type. SAD!", true, isNkInCache);
		}

		public void TestFSW_WorkflowType_ForSYSWorkflowType_ShouldNotAffectNaturalKeyCache()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			BMSTestHelper.AddWorkflowType(system, "WKI");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDeterminer = newFactory.LoadFromNaturalKey<BMSystemWorkflowDeterminer>(BMSystemWorkflowDeterminerSchema.FSW_WorkflowType, "WKI");
			AssertNotNull(loadedDeterminer);
		}

		#endregion

		public void TestTableIsCached()
		{
			AssertContains(BMSystemWorkflowDeterminerSchema.Constants.TableName, SystemDataRegistry.Instance.CachedTables.Value);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
				var sectionConfig = config.BufferSection.SectionConfiguration;

				Factory.Save();
			}
		}
	}
}
