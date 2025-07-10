using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class BMComponentAcceptabilityBandTest : TestCaseWithFactory
	{
		public void TestImportAcceptabilityBandWithComponent()
		{
			var system = Factory.New<IBMSystem>();
			system.FS_Name = "sneep";

			var component = Factory.New<IBMComponent>();
			component.FC_FS_System = system.PK;
			component.FC_Name = "sleep";

			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "fluff";
			band.BAB_FC_Component = component.PK;

			var filter = band.FilterRule;
			filter.S9_FilterData = new ZBlob(new byte[] { 1, 2 });

			var userValues = filter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2 });

			var filter2 = band.SupersetItemsFilterRule;
			filter2.S9_FilterData = new ZBlob(new byte[] { 3, 4 });

			var userValues2 = filter2.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues2.S0_FilterDataValues = new ZBlob(new byte[] { 3, 4 });

			Factory.Save();

			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(band))
			{
				band.Delete();
				filter.Delete();
				filter2.Delete();
				userValues.Delete();
				userValues2.Delete();

				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: AcceptabilityBand
--- Import Process Finished -----------------------------------------------------------
BMComponentAcceptabilityBand - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 2 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_Name, "fluff");

				var loadedBand = Factory.Load<BMComponentAcceptabilityBand>(query).Single();

				var loadedSupersetFilter = loadedBand.SupersetItemsFilterRule;
				var loadedFilterRule = loadedBand.FilterRule;
				var loadedSupersetUserData = loadedSupersetFilter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
				var loadedFilterRuleUserData = loadedFilterRule.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());

				AssertArrayEqualsByElements(new byte[] { 3, 4 }, loadedSupersetUserData.S0_FilterDataValues);
				AssertArrayEqualsByElements(new byte[] { 1, 2 }, loadedFilterRuleUserData.S0_FilterDataValues);
				AssertArrayEqualsByElements(new byte[] { 3, 4 }, loadedSupersetFilter.S9_FilterData);
				AssertArrayEqualsByElements(new byte[] { 1, 2 }, loadedFilterRule.S9_FilterData);

				AssertEquals(component.PK, loadedBand.BAB_FC_Component);
			}
		}

		public void TestImportAcceptabilityBandWithoutComponent()
		{
			var band = Factory.New<BMComponentAcceptabilityBand>();
			band.BAB_Name = "fluff";

			var filter = band.FilterRule;
			filter.S9_FilterData = new ZBlob(new byte[] { 1, 2 });

			var userValues = filter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2 });

			var filter2 = band.SupersetItemsFilterRule;
			filter2.S9_FilterData = new ZBlob(new byte[] { 3, 4 });

			var userValues2 = filter2.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues2.S0_FilterDataValues = new ZBlob(new byte[] { 3, 4 });

			Factory.Save();

			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(band))
			{
				band.Delete();
				filter.Delete();
				filter2.Delete();
				userValues.Delete();
				userValues2.Delete();

				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: AcceptabilityBand
--- Import Process Finished -----------------------------------------------------------
BMComponentAcceptabilityBand - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 2 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 2 inserts, 0 updates, 0 deletes
				".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(BMComponentAcceptabilityBandSchema.BAB_Name, "fluff");

				var loadedBand = Factory.Load<BMComponentAcceptabilityBand>(query).Single();

				var loadedSupersetFilter = loadedBand.SupersetItemsFilterRule;
				var loadedFilterRule = loadedBand.FilterRule;
				var loadedSupersetUserData = loadedSupersetFilter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
				var loadedFilterRuleUserData = loadedFilterRule.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
				Assert(new ZBlob(new byte[] { 3, 4 }) == loadedSupersetUserData.S0_FilterDataValues);
				Assert(new ZBlob(new byte[] { 1, 2 }) == loadedFilterRuleUserData.S0_FilterDataValues);
				Assert(new ZBlob(new byte[] { 3, 4 }) == loadedSupersetFilter.S9_FilterData);
				Assert(new ZBlob(new byte[] { 1, 2 }) == loadedFilterRule.S9_FilterData);
			}
		}

		public void TestImportMultipleAcceptabilityBands_ShouldNotShareFilters()
		{
			var band1 = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "It's circular using of the word");
			var band2 = BMSTestHelper.CreateAcceptabilityBand(Factory, 1, 2, 3, 4, 5, 6, "And that's from you");

			var filter1 = band1.FilterRule;
			var filter2 = band2.FilterRule;

			var supersetFilter1 = band1.SupersetItemsFilterRule;
			var supersetFilter2 = band2.SupersetItemsFilterRule;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(filter1, "2+2=5");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(filter2, "2+2=5");

			FilterStripsTestHelper.AddCustomSQLFilterStrip(supersetFilter1, "2+2=5");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(supersetFilter2, "2+2=5");

			Factory.Save();

			using (var dataStream1 = NativeDataTransferTestHelper.ExportToStream(band1))
			using (var dataStream2 = NativeDataTransferTestHelper.ExportToStream(band2))
			{
				band1.Delete();
				band2.Delete();

				Factory.Save();

				AssertEquals(true, filter1.IsDeleted);
				AssertEquals(true, filter2.IsDeleted);
				AssertEquals(true, supersetFilter1.IsDeleted);
				AssertEquals(true, supersetFilter2.IsDeleted);

				var importLog1 = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream1);
				var importLog2 = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream2);

				const string expectedImportLog = @"--- Start Import Process --------------------------------------------------------------
Processed: AcceptabilityBand
--- Import Process Finished -----------------------------------------------------------
BMComponentAcceptabilityBand - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 2 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 2 inserts, 0 updates, 0 deletes
";

				AssertMultilineASCIIEquals("Import log for band1", expectedImportLog, importLog1);
				AssertMultilineASCIIEquals("Import log for band2", expectedImportLog, importLog2);

				var newFactory = Factory.CreateNewFactory();
				var loadedBand1 = newFactory.LoadTop1<BMComponentAcceptabilityBand>(new ZQuery(BMComponentAcceptabilityBandSchema.BAB_Name, "It's circular using of the word"));
				var loadedBand2 = newFactory.LoadTop1<BMComponentAcceptabilityBand>(new ZQuery(BMComponentAcceptabilityBandSchema.BAB_Name, "And that's from you"));

				AssertNotNull(loadedBand1.FilterRule);
				AssertNotNull(loadedBand2.FilterRule);
				AssertNotNull(loadedBand1.SupersetItemsFilterRule);
				AssertNotNull(loadedBand2.SupersetItemsFilterRule);

				AssertNotEquals(loadedBand1.FilterRule, loadedBand2.FilterRule);
				AssertNotEquals(loadedBand1.SupersetItemsFilterRule, loadedBand2.SupersetItemsFilterRule);
			}
		}
	}
}
