using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;
using CusInBondCargoDesc = Enterprise.Customs.TW.Business.CusInBondCargoDesc;
using CusInBondHeader = Enterprise.Customs.TW.Business.CusInBondHeader;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STACusInBondHeaderArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STACusInBondHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone Customs In-Bond Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> CusInBondHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> CusInBondHeaderSchema.BH_VoyageNumber;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = CusInBondHeaderSchema.BH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> CusInBondHeaderSchema.BH_ParentID;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusESNctsHeaderSchema.Constants.TableName, CusESNctsHeaderSchema.CEN_BH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusFRNctsHeaderSchema.Constants.TableName, CusFRNctsHeaderSchema.CFN_BH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.B0_BH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondEquipmentSchema.Constants.TableName, CusInBondEquipmentSchema.BJ_BH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondEventSchema.Constants.TableName, CusInBondEventSchema.BN_BH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondPersonSchema.Constants.TableName, CusInBondPersonSchema.CP_BH_Header, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondHeaderSchema.Constants.TableName, CusInBondHeaderSchema.PK, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.BM_BH, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInbondBillAddRefSchema.Constants.TableName, CusInbondBillAddRefSchema.BR_B0, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondBillSchema.Constants.TableName, CusInBondBillSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_B0, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_BY_Commodity, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInBondFeeSchema.Constants.TableName, CusInBondFeeSchema.BFE_BY, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.PK, CusInBondVehicleCtrlSchema.Constants.TableName, CusInBondVehicleCtrlSchema.BV_BC, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondEquipmentSchema.Constants.TableName, CusInBondEquipmentSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_BJ_Equipment, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondEventSchema.Constants.TableName, CusInBondEventSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondContainerSchema.Constants.TableName, CusInBondContainerSchema.BC_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_B9_InBondMoveDetail, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInBondMoveLineItemSchema.Constants.TableName, CusInBondMoveLineItemSchema.BI_B9, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.PK, CusInvPackSchema.Constants.TableName, CusInvPackSchema.B5_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondCargoDescSchema.Constants.TableName, CusInBondCargoDescSchema.BY_ParentID, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.B9_BM, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.BM_BM_DepartureMovement, isReversed: false),
				new ArchiveRelationshipForSTATest(CusInBondMoveHeaderSchema.Constants.TableName, CusInBondMoveHeaderSchema.PK, CusInBondPayInfoSchema.Constants.TableName, CusInBondPayInfoSchema.BPI_BM, isReversed: false)
			};

		[UseSnapshotProtection]
		public void TestRunArchiveSystem_WhenCusInBondHeader_LinkedToCusInBondBillWithGrandchildren_CusInBondCargoDesc()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_VoyageNumber = "123456";

			var bill = header.Bills.AddNew();

			var cargoDesc = Factory.New<CusInBondCargoDesc>();
			cargoDesc.BY_ParentID = bill.PK;
			cargoDesc.BY_ParentTableCode = "B0";

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Customs In Bond Header to exist.", 1, Factory.GetDatabaseCount(typeof(CusInBondHeader), new ZQuery(CusInBondHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs In Bond Bill to exist.", 1, Factory.GetDatabaseCount(typeof(CusInBondBill), new ZQuery(CusInBondBillSchema.PK, bill.PK)));
				AssertEquals("Expected Customs In Bond Cargo Desc to exist.", 1, Factory.GetDatabaseCount(typeof(CusInBondCargoDesc), new ZQuery(CusInBondCargoDescSchema.PK, cargoDesc.PK)));
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.STA);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.STA, config, logger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected Customs In Bond Header to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusInBondHeader), new ZQuery(CusInBondHeaderSchema.PK, header.PK)));
				AssertEquals("Expected Customs In Bond Bill to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusInBondBill), new ZQuery(CusInBondBillSchema.PK, bill.PK)));
				AssertEquals("Expected Customs In Bond Cargo Desc to be deleted.", 0, Factory.GetDatabaseCount(typeof(CusInBondCargoDesc), new ZQuery(CusInBondCargoDescSchema.PK, cargoDesc.PK)));
			});

			Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}
	}
}
