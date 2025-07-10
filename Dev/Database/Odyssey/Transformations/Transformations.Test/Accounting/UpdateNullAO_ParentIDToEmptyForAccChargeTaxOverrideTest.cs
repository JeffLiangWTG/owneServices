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
	[TestedType(typeof(UpdateNullAO_ParentIDToEmptyForAccChargeTaxOverride))]
	public sealed class UpdateNullAO_ParentIDToEmptyForAccChargeTaxOverrideTest : DataTransformationTestCase
	{
		public void TestSkipTransformationWhenAccChargeTaxOverrideNotExists()
		{
			DropParentIDColumnAndDependencies();
			TestConnection.ExecuteNonQuery($"DROP TABLE {AccChargeTaxOverrideSchema.Constants.SqlSchemaName}.{chargeTaxOverrideTableName}");
			AssertEquals(false, DbObjectCreator.TableExists(TestConnection, chargeTaxOverrideTableName));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestSkipTransformationWhenParentIDNotExists()
		{
			DropParentIDColumnAndDependencies();
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, chargeTaxOverrideTableName, chargeCodeParentIDColumnName));

			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestUpdateNullParentIDWithEmptyGuid()
		{
			PrepareParentIdColumn();
			CreateValidDependencyData();
			taxOverridePK_WithNullParentID = CreateChargeTaxOverride();
			AssertAO_ParentIDWithTaxOverridePK(null, taxOverridePK_WithNullParentID);

			RunTransformation();

			AssertAO_ParentIDWithTaxOverridePK(Guid.Empty, taxOverridePK_WithNullParentID);
		}

		public void TestNoUpdateToValidParentID()
		{
			CreateValidDependencyData();
			taxOverridePK_WithValidParentID = CreateChargeTaxOverride(chargeCodePK);
			AssertAO_ParentIDWithTaxOverridePK(chargeCodePK, taxOverridePK_WithValidParentID);

			RunTransformation();

			AssertAO_ParentIDWithTaxOverridePK(chargeCodePK, taxOverridePK_WithValidParentID);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateNullAO_ParentIDToEmptyForAccChargeTaxOverride();

		protected override void SetUp()
		{
			testDbHelper = new TestDbHelper(TestConnection);
		}

		protected override void AssertTransformationResults()
		{
			AssertAO_ParentIDWithTaxOverridePK(Guid.Empty, taxOverridePK_WithNullParentID);
			AssertAO_ParentIDWithTaxOverridePK(Guid.Empty, taxOverridePK_WithEmptyParentID);
			AssertAO_ParentIDWithTaxOverridePK(chargeCodePK, taxOverridePK_WithValidParentID);
		}

		protected override void PrepareTestData()
		{
			PrepareParentIdColumn();
			CreateValidDependencyData();
			taxOverridePK_WithNullParentID = CreateChargeTaxOverride();
			taxOverridePK_WithEmptyParentID = CreateChargeTaxOverride(Guid.Empty);
			taxOverridePK_WithValidParentID = CreateChargeTaxOverride(chargeCodePK);
		}

		Guid CreateChargeTaxOverride(Guid? parentId = null)
		{
			var pk = Guid.NewGuid();
			testDbHelper.Insert(chargeTaxOverrideTableName, new
			{
				AO_PK = pk,
				AO_CostSellAll = "REV",
				AO_A9_DefaultVATClass = defaultVATClassPK,
				AO_AT = taxRatePK,
				AO_GB = branchPK,
				AO_ParentID = parentId,
				AO_DebtorRole = "NCX",
				AO_DefaultingRule = "SUM",
				AO_Direction = "OTH",
				AO_IncoTerm = "DPU",
				AO_JobType = "MWO",
				AO_OrganisationCategory = "NGO",
				AO_TransactionContext = "STD",
				AO_SystemCreateUser = "DMO",
				AO_SystemLastEditUser = "DEM",
				AO_SystemCreateTimeUtc = new DateTime(2024, 3, 8, 10, 30, 10, DateTimeKind.Utc),
				AO_SystemLastEditTimeUtc = new DateTime(2024, 3, 8, 10, 40, 10, DateTimeKind.Utc),
			});

			return pk;
		}

		void PrepareParentIdColumn()
		{
			DropParentIDColumnAndDependencies();
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, chargeTaxOverrideTableName, chargeCodeParentIDColumnName, nameof(SqlDbType.UniqueIdentifier));
		}

		void CreateValidDependencyData()
		{
			defaultVATClassPK = testDbHelper.InsertInvMsg("ABT");
			taxRatePK = testDbHelper.InsertTaxRate("VAT");
			companyPK = testDbHelper.InsertCompany("DAU", "AUD DEMO", "AUD", "AU", false, false);
			branchPK = testDbHelper.InsertBranch("ZZZ", companyPK);
			chargeCodePK = testDbHelper.InsertChargeCode(companyPK, "CC1");
		}

		void AssertAO_ParentIDWithTaxOverridePK(object expectedParentId, Guid taxOverridPK)
		{
			var sql = @$"SELECT {chargeCodeParentIDColumnName}
						FROM {chargeTaxOverrideTableName}
						WHERE AO_PK = @chargeCodePK";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@chargeCodePK", SqlDbType.UniqueIdentifier, taxOverridPK);
				var dataTable = DataUtils.GetDataTableFromCommand(cmd);
				AssertEquals(1, dataTable.Rows.Count);

				expectedParentId = expectedParentId ?? DBNull.Value;
				AssertEquals(expectedParentId, dataTable.Rows[0][chargeCodeParentIDColumnName]);
			}
		}

		void DropParentIDColumnAndDependencies()
		{
			DBTransformationTestHelper.DropFunctionIfExists("ChargeCodeListing_Multilingual");
			DBTransformationTestHelper.DropFunctionIfExists("Report_ChargeCodeTaxSetupAndOverrides");
			DBTransformationTestHelper.DropIndexIfExists(chargeTaxOverrideTableName, "NR_RC__AO_ParentID");
			DBTransformationTestHelper.DropIndexIfExists(chargeTaxOverrideTableName, "NR_RX__AO_ParentID_AO_Origin_AO_Destination_AO_JobType_AO_OrganisationCategory_AO_HomeCountryOrZone_AO_TaxRegCntryOrZone_AO_");
			DBTransformationTestHelper.DropColumnIfExists(chargeTaxOverrideTableName, chargeCodeParentIDColumnName);
		}

		Guid chargeCodePK;
		Guid companyPK;
		Guid branchPK;
		Guid defaultVATClassPK;
		Guid taxRatePK;
		Guid taxOverridePK_WithNullParentID;
		Guid taxOverridePK_WithEmptyParentID;
		Guid taxOverridePK_WithValidParentID;
		readonly string chargeCodeParentIDColumnName = AccChargeTaxOverrideSchema.Constants.AO_ParentID;
		readonly string chargeTaxOverrideTableName = AccChargeTaxOverrideSchema.Constants.TableName;
		TestDbHelper testDbHelper;
	}
}
