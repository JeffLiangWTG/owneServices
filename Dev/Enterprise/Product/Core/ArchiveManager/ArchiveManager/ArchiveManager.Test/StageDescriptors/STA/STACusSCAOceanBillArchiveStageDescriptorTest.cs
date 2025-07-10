using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusSCAOceanBillArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusSCAOceanBillArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs Sea Cargo Ocean Bill Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusSCAOceanBillSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusSCAOceanBillSchema.CB_MasterHouseBill;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusSCAOceanBillSchema.CB_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusSCAOceanBillSchema.CB_ParentId;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAContainerSchema.Constants.TableName, CusSCAContainerSchema.CN_CB, isReversed: false),
				new ArchiveRelationshipForSTATest(CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAHouseSchema.Constants.TableName, CusSCAHouseSchema.CA_CB, isReversed: false),
				new ArchiveRelationshipForSTATest(CusSCAOceanBillSchema.Constants.TableName, CusSCAOceanBillSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CB, isReversed: false),
				new ArchiveRelationshipForSTATest(CusSCAContainerSchema.Constants.TableName, CusSCAContainerSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CN, isReversed: false),
				new ArchiveRelationshipForSTATest(CusSCAHouseSchema.Constants.TableName, CusSCAHouseSchema.PK, CusSCAPivotSchema.Constants.TableName, CusSCAPivotSchema.CV_CA, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenArchivingCusIntrastatHeader()
		{
			var bill = Factory.New<CusSCAOceanBill>();

			var container = Factory.New<CusSCAContainer>();
			container.CN_CB = bill.PK;

			var house = Factory.New<CusSCAHouse>();
			house.CA_CB = bill.PK;

			var pivot = Factory.New<CusSCAPivot>();
			pivot.CV_CN = container.PK;
			pivot.CV_CA = house.PK;
			pivot.CV_CB = bill.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs Sea Cargo Ocean Bill to exist.", 1, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), new ZQuery(CusSCAOceanBillSchema.PK, bill.PK)));
				AssertEquals("Expected Customs Sea Cargo Ocean Container to exist.", 1, Factory.GetDatabaseCount(typeof(CusSCAContainer), new ZQuery(CusSCAContainerSchema.PK, container.PK)));
				AssertEquals("Expected Customs Sea Cargo Ocean House to exist.", 1, Factory.GetDatabaseCount(typeof(CusSCAHouse), new ZQuery(CusSCAHouseSchema.PK, house.PK)));
				AssertEquals("Expected Customs Sea Cargo Ocean Pivot to exist.", 1, Factory.GetDatabaseCount(typeof(CusSCAPivot), new ZQuery(CusSCAPivotSchema.PK, pivot.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs Sea Cargo Ocean Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusSCAOceanBill), new ZQuery(CusSCAOceanBillSchema.PK, bill.PK)));
				AssertEquals("Expected Customs Sea Cargo Ocean Container to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusSCAContainer), new ZQuery(CusSCAContainerSchema.PK, container.PK)));
				AssertEquals("Expected Customs Sea Cargo Ocean House to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusSCAHouse), new ZQuery(CusSCAHouseSchema.PK, house.PK)));
				AssertEquals("Expected  Customs Sea Cargo Ocean Pivot to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusSCAPivot), new ZQuery(CusSCAPivotSchema.PK, pivot.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
