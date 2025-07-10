using System;
using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Yard.Testing
{
	[TestedType(typeof(PopulateFacilityForWorkOrderHeader))]
	public class PopulateFacilityForWorkOrderHeaderTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropIndexIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "FK_RX__MWO_WW_Facility");

			DBTransformationTestHelper.DropConstraintIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "MNRWorkOrderHeader_MWO_WW_Facility_FK2_WhsWarehouse_RRR_120N", TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, MNRWorkOrderHeaderSchema.Constants.MWO_WW_Facility, TestConnection);

			DBTransformationTestHelper.DropIndexIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "NR_RC__MWO_ParentID");
			DBTransformationTestHelper.DropConstraintIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "MNRWorkOrderHeader_MWO_GB_BRANCH_FK2_GlbBranch_RRR_120N", TestConnection);

			var sql = new StringBuilder();

			var glbBranch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			warehouse = new WhsWarehouse("WH1", "CYD", glbBranch.PK).AppendInsertAndReturnObject(sql);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var unitLineItem = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			yardUnitState = new CYDYardUnitState(warehouse, null, null, null, null, null, "AAAA00008", unitLineItem: unitLineItem)
			{
				YUS_YRL_ReceiveLine = receiveAdviceLine1
			}.AppendInsertAndReturnObject(sql);

			var startingGrade = new RefEquipmentGrade("CWC", "Cargo Worthy").AppendInsertAndReturnObject(sql);
			var endGrade = new RefEquipmentGrade("MLT", "Malt Quality").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var surveyHeaderPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"
				INSERT INTO dbo.MNRWorkOrderHeader (MWO_PK, MWO_ParentID, MWO_Type, MWO_ParentTableCode, MWO_SystemLastEditUser, MWO_SystemLastEditTimeUtc, MWO_JobNumber, MWO_SystemCreateUser, MWO_SystemCreateTimeUtc, MWO_REG_StartEquipmentGrade, MWO_REG_EndEquipmentGrade)
				VALUES ('{surveyHeaderPK}', '{yardUnitState.PK}', 'STL', 'YUS', '~BP', '2023-07-01 07:21:00', 'JOB123', '~BP', '2023-07-01 07:21:00', '{startingGrade.PK}', '{endGrade.PK}')
			");

			Assert(!DbObjectCreator.ColumnExists(TestConnection, MNRWorkOrderHeaderSchema.Constants.TableName, "MWO_WW_Facility"));
		}

		protected override void AssertTransformationResults()
		{
			Assert(DbObjectCreator.ColumnExists(TestConnection, MNRWorkOrderHeaderSchema.Constants.TableName, "MWO_WW_Facility"));

			var workOrderHeaders = MNRWorkOrderHeader.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be one work order header", 1, workOrderHeaders.Length);
			var assignedWhsToWorkOrderHeader = WhsWarehouse.ShallowLoadFromDB(TestConnection).Single(l => l.PK == workOrderHeaders[0].MWO_WW_Facility);
			AssertEquals("Yard unit warehouse should be assigned as workOrderHeader warehouse", warehouse.PK, assignedWhsToWorkOrderHeader.PK);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateFacilityForWorkOrderHeader();
		CYDYardUnitState yardUnitState;
		WhsWarehouse warehouse;
	}
}
