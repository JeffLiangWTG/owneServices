using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusCAeMHMasterArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusCAeMHMasterArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Canadian Customs eManifest Master Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusCAeMHMasterSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusCAeMHMasterSchema.BP_MasterBill;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusCAeMHMasterSchema.BP_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusCAeMHMasterSchema.BP_ParentID;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusCAeMHContainerSchema.Constants.TableName, CusCAeMHContainerSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BQ_Container, isReversed: false),
				new ArchiveRelationshipForSTATest(CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BW_House, isReversed: false),
				new ArchiveRelationshipForSTATest(CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHItemSchema.Constants.TableName, CusCAeMHItemSchema.BX_BW_House, isReversed: false),
				new ArchiveRelationshipForSTATest(CusCAeMHMasterSchema.Constants.TableName, CusCAeMHMasterSchema.PK, CusCAeMHContainerSchema.Constants.TableName, CusCAeMHContainerSchema.BQ_BP_Master, isReversed: false),
				new ArchiveRelationshipForSTATest(CusCAeMHMasterSchema.Constants.TableName, CusCAeMHMasterSchema.PK, CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.BW_BP_Master, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenCusCAeMHMaster_LinkedToCusCAeMHContainerWithCusCAeMHHouseContainerPivot()
		{
			var cusCAeMHMasterType = ObjectFactory.GetType<ICusCAeMHMaster>();
			var cusCAeMHContainerType = ObjectFactory.GetType("CusCAeMHContainer");
			var cusCAeMHHouseContainerPivotType = ObjectFactory.GetType("CusCAeMHHouseContainerPivot");

			var cusCAeMHMaster = Factory.New<ICusCAeMHMaster>();

			var cusCAeMHContainer = Factory.New<ICusCAeMHContainer>();
			cusCAeMHContainer.BQ_BP_Master = cusCAeMHMaster.PK;

			var cusCAeMHHouse = Factory.New<ICusCAeMHHouse>();
			cusCAeMHHouse.BW_BP_Master = cusCAeMHMaster.PK;
			var cusCAeMHHouseContainerPivot = Factory.New<ICusCAeMHHouseContainerPivot>();
			cusCAeMHHouseContainerPivot.BPA_BQ_Container = cusCAeMHContainer.PK;
			cusCAeMHHouseContainerPivot.BPA_BW_House = cusCAeMHHouse.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Canadian Customs eManifest Master to exist.", 1, Factory.GetDatabaseCount(cusCAeMHMasterType, new ZQuery(CusCAeMHMasterSchema.PK, cusCAeMHMaster.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container to exist.", 1, Factory.GetDatabaseCount(cusCAeMHContainerType, new ZQuery(CusCAeMHContainerSchema.PK, cusCAeMHContainer.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container Pivot to exist.", 1, Factory.GetDatabaseCount(cusCAeMHHouseContainerPivotType, new ZQuery(CusCAeMHHouseContainerPivotSchema.PK, cusCAeMHHouseContainerPivot.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Canadian Customs eManifest Master to be deleted.", 0, Factory.GetDatabaseCount(cusCAeMHMasterType, new ZQuery(CusCAeMHMasterSchema.PK, cusCAeMHMaster.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container to be deleted.", 0, Factory.GetDatabaseCount(cusCAeMHContainerType, new ZQuery(CusCAeMHContainerSchema.PK, cusCAeMHContainer.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container Pivot to be deleted.", 0, Factory.GetDatabaseCount(cusCAeMHHouseContainerPivotType, new ZQuery(CusCAeMHHouseContainerPivotSchema.PK, cusCAeMHHouseContainerPivot.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
