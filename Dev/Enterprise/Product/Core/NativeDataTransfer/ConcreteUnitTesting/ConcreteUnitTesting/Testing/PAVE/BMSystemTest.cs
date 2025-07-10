using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class BMSystemTest : TestCaseWithFactory
	{
		public void TestBMSystem()
		{
			var staffForTest = Factory.New<GlbStaff>();
			staffForTest.GS_Code = "NP";
			staffForTest.GS_LoginName = "NP";

			var staffForTest2 = Factory.New<GlbStaff>();
			staffForTest2.GS_Code = "NP2";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "aed";

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "gef";

			var controlCustomisation = Factory.New<BMControlCustomisation>();
			controlCustomisation.FM_Name = "Neep";
			controlCustomisation.FM_ControlType = "DET";

			var bmTestHelper = ObjectFactory.Get<IBMTestHelper>();

			var system = (BMSystem)bmTestHelper.CreateSystem(Factory, "DUM");
			system.FS_Name = "flass";
			var bucket = bmTestHelper.CreateBucket(system);
			var buffer = (BMComponent)bmTestHelper.CreateBuffer(system, "bnoasd");

			var controlCustomisationLink = system.CustomisedLayoutLinks.AddNew();
			controlCustomisationLink.FML_FM_ControlCustomisation = controlCustomisation.PK;

			var link = (BMComponentLink)bmTestHelper.LinkComponents(bucket, buffer);

			var filter = link.FilterRule;
			filter.S9_FilterData = new ZBlob(new byte[] { 1, 2 });

			var userValues = filter.GetOrCreateLayoutUserData(new FilterStripLayoutsHelper());
			userValues.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2 });

			var subComponent = bmTestHelper.CreateBuffer(system);
			subComponent.FC_FC_ParentComponent = buffer.PK;

			var resourceLink = buffer.ResourceLinks.AddNew();
			resourceLink.FD_GS_NKResource = staffForTest.GS_Code;

			var resourceLink2 = buffer.ResourceLinks.AddNew();
			resourceLink2.FD_GS_NKResource = staffForTest2.GS_Code;

			var releaseGroupLink = system.ReleaseGroups.AddNew();
			releaseGroupLink.FSG_GG_Group = group.PK;

			var componentGroupLink = buffer.ReleaseGroupLinks.AddNew();
			componentGroupLink.FO_GG_ReleaseGroup = group.PK;

			var componentGroupLink2 = buffer.ReleaseGroupLinks.AddNew();
			componentGroupLink2.FO_GG_ReleaseGroup = group2.PK;

			var componentCapacityGroupLink = buffer.ZoneCapacityMultipliers.AddNew();
			componentCapacityGroupLink.BZC_GG_ReleaseGroup = group.PK;

			var componentCapacityGroupLink2 = buffer.ZoneCapacityMultipliers.AddNew();
			componentCapacityGroupLink2.BZC_GG_ReleaseGroup = group2.PK;

			Factory.Save();

			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(system))
			{
				system.Delete();
				AssertEquals(true, filter.IsDeleted);

				Factory.Save();

				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: BMSystem
--- Import Process Finished -----------------------------------------------------------
BMSystem - 1 inserts, 0 updates, 0 deletes
BMControlCustomisationLink - 1 inserts, 0 updates, 0 deletes
BMSystemWorkflowDeterminer - 1 inserts, 0 updates, 0 deletes
BMSystemReleaseGroup - 1 inserts, 0 updates, 0 deletes
BMComponent - 2 inserts, 0 updates, 0 deletes
BMChildComponent - 1 inserts, 0 updates, 0 deletes
BMComponentReleaseGroupLink - 2 inserts, 0 updates, 0 deletes
BMZoneCapacityMultiplier - 2 inserts, 0 updates, 0 deletes
BMComponentResourceLink - 2 inserts, 0 updates, 0 deletes
BMComponentLink - 1 inserts, 0 updates, 0 deletes
StmModuleFilter - 1 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 1 inserts, 0 updates, 0 deletes

				".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(BMSystemSchema.FS_Name, "flass");

				var loadedSystem = Factory.Load<BMSystem>(query).Single();

				AssertEquals(true, loadedSystem.IsForWorkflowType("DUM"));
				AssertEquals(false, loadedSystem.IsForWorkflowType("ORG"));

				var components = loadedSystem.Components;
				AssertEquals(2, components.Count);

				var loadedBuffer = components.Single(c => c.FC_Type == "BUF");
				var loadedBucket = components.Single(c => c.FC_Type == "BUC");

				var loadedComponentLink = loadedBuffer.FromOthersToMeLinks.Single();
				AssertEquals(loadedBucket.PK, loadedComponentLink.FL_FC_ComponentFrom);

				var loadedSubComponent = loadedBuffer.ChildComponents.Single();
				AssertNotNull(loadedSubComponent);

				AssertNotNull(loadedBuffer.GetResourceLink(staffForTest.GS_Code));
				AssertNotNull(loadedBuffer.GetResourceLink(staffForTest2.GS_Code));

				var loadedReleaseGroupLink = loadedSystem.ReleaseGroups.Single();
				AssertEquals(group.PK, loadedReleaseGroupLink.FSG_GG_Group);

				var loadedComponentReleaseGroupLink = loadedBuffer.ReleaseGroupLinks.SingleOrDefault(l => l.FO_GG_ReleaseGroup == group.PK);
				var loadedComponentReleaseGroupLink2 = loadedBuffer.ReleaseGroupLinks.SingleOrDefault(l => l.FO_GG_ReleaseGroup == group2.PK);
				AssertNotNull(loadedComponentReleaseGroupLink);
				AssertNotNull(loadedComponentReleaseGroupLink2);

				var loadedZoneCapacityMultiplier = loadedBuffer.ZoneCapacityMultipliers.SingleOrDefault(l => l.BZC_GG_ReleaseGroup == group.PK);
				var loadedZoneCapacityMultiplier2 = loadedBuffer.ZoneCapacityMultipliers.SingleOrDefault(l => l.BZC_GG_ReleaseGroup == group2.PK);
				AssertNotNull(loadedZoneCapacityMultiplier);
				AssertNotNull(loadedZoneCapacityMultiplier2);

				var loadedCustomisedLink = loadedSystem.CustomisedLayoutLinks.Single();
				AssertEquals(controlCustomisation.PK, loadedCustomisedLink.FML_FM_ControlCustomisation);
				AssertEquals("FS", loadedCustomisedLink.FML_ParentTableCode);
			}
		}

		public void TestImportSystem_WithMultipleComponentLinks_ShouldNotShareFilters()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Malcolm Trumble";

			var bucket1 = BMSTestHelper.CreateBucket(system, "News");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Fake News");
			var bucket3 = BMSTestHelper.CreateBucket(system, "Sad");

			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3);

			var filter1 = link1.FilterRule;
			var filter2 = link2.FilterRule;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(filter1, "2+2=5");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(filter2, "2+2=5");

			Factory.Save();

			using (var dataStream = NativeDataTransferTestHelper.ExportToStream(system))
			{
				system.Delete();
				Factory.Save();

				var importLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(dataStream);

				AssertMultilineASCIIEquals("Insert Log Text", @"--- Start Import Process --------------------------------------------------------------
Processed: BMSystem
--- Import Process Finished -----------------------------------------------------------
BMSystem - 1 inserts, 0 updates, 0 deletes
BMSystemWorkflowDeterminer - 1 inserts, 0 updates, 0 deletes
BMComponent - 3 inserts, 0 updates, 0 deletes
BMComponentLink - 2 inserts, 0 updates, 0 deletes
StmModuleFilter - 2 inserts, 0 updates, 0 deletes
StmModuleFilterUserData - 2 inserts, 0 updates, 0 deletes", importLog);

				var newFactory = Factory.CreateNewFactory();
				var loadedSystem = newFactory.LoadTop1<BMSystem>(new ZQuery(BMSystemSchema.FS_Name, "Malcolm Trumble"));

				AssertEquals(3, loadedSystem.Components.Count);
				var loadedLink1 = loadedSystem.Components.Single(c => c.FC_Name == "News").FromMeToOthersLinks.Single();
				var loadedLink2 = loadedSystem.Components.Single(c => c.FC_Name == "Fake News").FromMeToOthersLinks.Single();

				AssertNotNull(loadedLink1.FilterRule);
				AssertNotNull(loadedLink2.FilterRule);

				AssertNotEquals(loadedLink1.FilterRule, loadedLink2.FilterRule);
			}
		}
	}
}
