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
	class STACusCAeMHHouseArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusCAeMHHouseArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Canadian Customs eManifest House Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusCAeMHHouseSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusCAeMHHouseSchema.BW_HouseBill;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusCAeMHHouseSchema.BW_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusCAeMHHouseSchema.BW_ParentID;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHHouseContainerPivotSchema.Constants.TableName, CusCAeMHHouseContainerPivotSchema.BPA_BW_House, isReversed: false),
				new ArchiveRelationshipForSTATest(CusCAeMHHouseSchema.Constants.TableName, CusCAeMHHouseSchema.PK, CusCAeMHItemSchema.Constants.TableName, CusCAeMHItemSchema.BX_BW_House, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusCAeMHHouseWithCusCAeMHHouseContainerPivot()
		{
			var cusCAeMHHouseType = ObjectFactory.GetType("CusCAeMHHouse");
			var cusCAeMHHouseContainerPivotType = ObjectFactory.GetType("CusCAeMHHouseContainerPivot");

			var cusCAeMHMaster = Factory.New<ICusCAeMHMaster>();
			var cusCAeMHHouse = Factory.New<ICusCAeMHHouse>();
			cusCAeMHHouse.BW_BP_Master = cusCAeMHMaster.PK;

			var cusCAeMHContainer = Factory.New<ICusCAeMHContainer>();
			cusCAeMHContainer.BQ_BP_Master = cusCAeMHMaster.PK;
			var cusCAeMHHouseContainerPivot = Factory.New<ICusCAeMHHouseContainerPivot>();
			cusCAeMHHouseContainerPivot.BPA_BQ_Container = cusCAeMHContainer.PK;
			cusCAeMHHouseContainerPivot.BPA_BW_House = cusCAeMHHouse.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Canadian Customs eManifest House to exist.", 1, Factory.GetDatabaseCount(cusCAeMHHouseType, new ZQuery(CusCAeMHHouseSchema.PK, cusCAeMHHouse.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container Pivot to exist.", 1, Factory.GetDatabaseCount(cusCAeMHHouseContainerPivotType, new ZQuery(CusCAeMHHouseContainerPivotSchema.PK, cusCAeMHHouseContainerPivot.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Canadian Customs eManifest House to be deleted.", 0, Factory.GetDatabaseCount(cusCAeMHHouseType, new ZQuery(CusCAeMHHouseSchema.PK, cusCAeMHHouse.PK)));
				AssertEquals("Expected Canadian Customs eManifest House Container Pivot to be deleted.", 0, Factory.GetDatabaseCount(cusCAeMHHouseContainerPivotType, new ZQuery(CusCAeMHHouseContainerPivotSchema.PK, cusCAeMHHouseContainerPivot.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
