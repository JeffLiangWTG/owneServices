using System;
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
	[TestedType(typeof(PopulateYardUnitDetailsForMNRSurvey))]
	public class PopulateYardUnitDetailsForMNRSurveyTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropIndexIfExists(MNRSurveySchema.Constants.TableName, "NR_RC__MRS_ParentID");
			DBTransformationTestHelper.DropColumnIfExists(MNRSurveySchema.Constants.TableName, "MRS_ParentID", TestConnection);

			DBTransformationTestHelper.DropIndexIfExists(MNRSurveySchema.Constants.TableName, "FK_RX__MRS_WW_Facility");
			DBTransformationTestHelper.DropConstraintIfExists(MNRSurveySchema.Constants.TableName, "MNRSurvey_MRS_WW_Facility_FK2_WhsWarehouse_RRR_120N", TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(MNRSurveySchema.Constants.TableName, "MRS_WW_Facility", TestConnection);

			if (!DbObjectCreator.ColumnExists(TestConnection, MNRSurveySchema.Constants.TableName, "MRS_YUS_YardUnitState"))
			{
				TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.[MNRSurvey]
	ADD [MRS_YUS_YardUnitState] UNIQUEIDENTIFIER NULL
");
			}

			var sql = new StringBuilder();
			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var unitLine = new CYDUnitLineItem(refContainer) { YLI_Type = "CNT", YLI_Quantity = 1 }.AppendInsertAndReturnObject(sql);

			var branch = new GlbBranch("COL").AppendInsertAndReturnObject(sql);
			warehouse = new WhsWarehouse("WH1", "CYD", branch.PK).AppendInsertAndReturnObject(sql);
			var receiveAdvice = new CYDReceiveAdvice(warehouse, "JOB001").AppendInsertAndReturnObject(sql);
			var receiveAdviceLine = new CYDReceiveAdviceLine(receiveAdvice) { YRL_YLI_UnitLineItem = unitLine }.AppendInsertAndReturnObject(sql);
			yardUnitState = new CYDYardUnitState(warehouse, null, null, null, loadUser: "", unloadUser: "", "UNT123") { YUS_YRL_ReceiveLine = receiveAdviceLine }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var surveyPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.MNRSurvey (MRS_PK, MRS_SystemLastEditUser, MRS_SystemLastEditTimeUtc, MRS_SystemCreateUser, MRS_SystemCreateTimeUtc, MRS_YUS_YardUnitState)
		VALUES ('{surveyPK}', '~BP', '2023-07-01 07:21:00', '~BP', '2023-07-01 07:21:00', '{yardUnitState.PK}')
");

			Assert(!DbObjectCreator.ColumnExists(TestConnection, MNRSurveySchema.Constants.TableName, "MRS_ParentID"));
			Assert(!DbObjectCreator.ColumnExists(TestConnection, MNRWorkOrderHeaderSchema.Constants.TableName, "MRS_WW_Facility"));
		}

		protected override void AssertTransformationResults()
		{
			Assert(DbObjectCreator.ColumnExists(TestConnection, MNRSurveySchema.Constants.TableName, "MRS_ParentID"));
			Assert(DbObjectCreator.ColumnExists(TestConnection, MNRSurveySchema.Constants.TableName, "MRS_WW_Facility"));

			var surveys = MNRSurvey.ShallowLoadFromDB(TestConnection);
			AssertEquals("There should be one survey", 1, surveys.Length);
			AssertEquals("Yard unit facility should be assigned as survey facility", warehouse.PK, surveys[0].MRS_WW_Facility.FK);
			AssertEquals("Yard unit PK should be assigned as survey parent ID", yardUnitState.PK, surveys[0].MRS_ParentID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateYardUnitDetailsForMNRSurvey();
		CYDYardUnitState yardUnitState;
		WhsWarehouse warehouse;
	}
}
