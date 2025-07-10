using System.Linq;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PreUpgrade.Public.Yard
{
	[TestedType(typeof(PopulateYardUnitDetailsForMNRWorkOrderHeader))]
	public class PopulateYardUnitDetailsForMNRWorkOrderHeaderTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();
			DBTransformationTestHelper.DropIndexIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "NR_RC__MWO_ParentID");
			DBTransformationTestHelper.DropConstraintIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, "Constraint_MWO_ParentTableCode", TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(MNRWorkOrderHeaderSchema.Constants.TableName, MNRWorkOrderHeaderSchema.Constants.MWO_ParentID, TestConnection);

			branch = new GlbBranch("COL").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("WH1", "CYD", branch.PK).AppendInsertAndReturnObject(sql);
			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var unitLineItem = new CYDUnitLineItem(refContainer).AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);

			var unitLine1 = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);
			var receiveAdviceLine1 = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine1 }.AppendInsertAndReturnObject(sql);

			yardUnitState = new CYDYardUnitState(warehouse,null, null, null, null, null, "AAAA00008", unitLineItem: unitLineItem)
			{
				YUS_YRL_ReceiveLine = receiveAdviceLine1
			}.AppendInsertAndReturnObject(sql);

			var startingGrade = new RefEquipmentGrade("CWC", "Cargo Worthy").AppendInsertAndReturnObject(sql);
			var endGrade = new RefEquipmentGrade("MLT", "Malt Quality").AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.MNRWorkOrderHeader (MWO_PK, MWO_Type, MWO_JobNumber, MWO_SystemLastEditUser, MWO_SystemLastEditTimeUtc, MWO_SystemCreateUser, MWO_SystemCreateTimeUtc, MWO_REG_StartEquipmentGrade, MWO_REG_EndEquipmentGrade, MWO_WW_Facility)
		VALUES (NEWID(), 'STL', 'JOB123', '~BP', '2023-07-01 07:21:00', '~BP', '2023-07-01 07:21:00', '{startingGrade.PK}', '{endGrade.PK}', '{warehouse.PK}')
");
		}

		protected override void AssertTransformationResults()
		{
			var workOrders = MNRWorkOrderHeader.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expect one work order to remain", 1, workOrders.Length);
			AssertEquals("Parent ID is the yard unit state PK", yardUnitState.PK, workOrders.First().MWO_ParentID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateYardUnitDetailsForMNRWorkOrderHeader();
		CYDYardUnitState yardUnitState;
		GlbBranch branch;
	}
}
