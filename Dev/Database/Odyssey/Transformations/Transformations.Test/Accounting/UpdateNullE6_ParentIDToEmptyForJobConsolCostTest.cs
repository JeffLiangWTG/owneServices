using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(UpdateNullE6_ParentIDToEmptyForJobConsolCost))]
	public sealed class UpdateNullE6_ParentIDToEmptyForJobConsolCostTest : DataTransformationTestCase
	{
		public void TestSkipTransformationWhenJobConsolCostNotExists()
		{
			DropTableDependencies();
			TestConnection.ExecuteNonQuery($"DROP TABLE {JobConsolCostSchema.Constants.SqlSchemaName}.{consolCostTableName}");
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, consolCostTableName));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestSkipTransformationWhenParentIDNotExists()
		{
			DropParentIDColumnAndDependencies();
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, consolCostTableName, consolCostParentIDColumnName));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestUpdateNullParentIDWithEmptyGuid()
		{
			PrepareParentIdColumn();
			CreateValidDependencyData();
			consolCostPK_WithNullParentID = CreateJobConsolCost();
			AssertE6_ParentIDWithConsolCostPK(null, consolCostPK_WithNullParentID);

			RunTransformation();

			AssertE6_ParentIDWithConsolCostPK(Guid.Empty, consolCostPK_WithNullParentID);
		}

		public void TestNoUpdateToValidParentID()
		{
			CreateValidDependencyData();
			consolCostPK_WithValidParentID = CreateJobConsolCost(consolPK);
			AssertE6_ParentIDWithConsolCostPK(consolPK, consolCostPK_WithValidParentID);

			RunTransformation();

			AssertE6_ParentIDWithConsolCostPK(consolPK, consolCostPK_WithValidParentID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateNullE6_ParentIDToEmptyForJobConsolCost();

		protected override void SetUp()
		{
			testDbHelper = new TestDbHelper(TestConnection);
		}

		protected override void AssertTransformationResults()
		{
			AssertE6_ParentIDWithConsolCostPK(Guid.Empty, consolCostPK_WithNullParentID);
			AssertE6_ParentIDWithConsolCostPK(Guid.Empty, consolCostPK_WithEmptyParentID);
			AssertE6_ParentIDWithConsolCostPK(consolPK, consolCostPK_WithValidParentID);
		}

		protected override void PrepareTestData()
		{
			PrepareParentIdColumn();
			CreateValidDependencyData();
			consolCostPK_WithNullParentID = CreateJobConsolCost();
			consolCostPK_WithEmptyParentID = CreateJobConsolCost(Guid.Empty);
			consolCostPK_WithValidParentID = CreateJobConsolCost(consolPK);
		}

		void AssertE6_ParentIDWithConsolCostPK(object expectedParentId, Guid consolCostPK)
		{
			var sql = @$"SELECT {consolCostParentIDColumnName}
						FROM {consolCostTableName}
						WHERE E6_PK = @ConsolCostPK";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@ConsolCostPK", SqlDbType.UniqueIdentifier, consolCostPK);
				var dataTable = DataUtils.GetDataTableFromCommand(cmd);
				AssertEquals(1, dataTable.Rows.Count);

				expectedParentId = expectedParentId ?? DBNull.Value;
				AssertEquals(expectedParentId, dataTable.Rows[0][consolCostParentIDColumnName]);
			}
		}

		Guid CreateJobConsolCost(Guid? parentId = null)
		{
			return testDbHelper.InsertConsolCost(jobChargePK, "INV001", DateTime.Today, "AUD", 120M, 0M, 1.0M, 120M, "MAN", creditor: null, aPInvoicePK: null, aRInvoicePK: null, consol: parentId, TestDbHelper.DefaultCompanyPK);
		}

		void CreateValidDependencyData()
		{
			jobChargePK = testDbHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			consolPK = testDbHelper.InsertConsol("C00002288", "ADALV", "CNTAI");
		}

		void PrepareParentIdColumn()
		{
			DropParentIDColumnAndDependencies();
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, consolCostTableName, consolCostParentIDColumnName, nameof(SqlDbType.UniqueIdentifier));
		}

		void DropParentIDColumnAndDependencies()
		{
			DBTransformationTestHelper.DropFunctionIfExists("GetAccrualsForJobAndRelatedOrg");
			DBTransformationTestHelper.DropFunctionIfExists("Report_AirlineCostDetailsAnalysis");
			DBTransformationTestHelper.DropIndexIfExists(consolCostTableName, "NR_RC__E6_ParentID");
			DBTransformationTestHelper.DropColumnIfExists(consolCostTableName, consolCostParentIDColumnName);
		}

		void DropTableDependencies()
		{
			DBTransformationTestHelper.DropConstraintIfExists(JobChargeSchema.Constants.TableName, "JobCharge_JR_E6_FK2_JobConsolCost_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(JobConsolCostAttribSchema.Constants.TableName, "JobConsolCostAttrib_E6A_E6_JobConsolCost_FK2_JobConsolCost_CRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(JobPaymentBasisSchema.Constants.TableName, "JobPaymentBasis_PBS_E6_FK2_JobConsolCost_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(JobChargeSchema.Constants.TableName, "JobCharge_JR_E6_GatewaySellHeader_FK2_JobConsolCost_RRR_120N");
			DBTransformationTestHelper.DropFunctionIfExists("Report_AirlineCostDetailsAnalysis");
			DBTransformationTestHelper.DropFunctionIfExists("GetAccrualsForJobAndRelatedOrg");
		}

		Guid consolPK;
		Guid jobChargePK;
		Guid consolCostPK_WithNullParentID;
		Guid consolCostPK_WithEmptyParentID;
		Guid consolCostPK_WithValidParentID;
		readonly string consolCostParentIDColumnName = JobConsolCostSchema.Constants.E6_ParentID;
		readonly string consolCostTableName = JobConsolCostSchema.Constants.TableName;
		TestDbHelper testDbHelper;
	}
}
