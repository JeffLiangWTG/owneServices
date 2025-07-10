using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.LandedCosting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.LandedCosting
{
	[TestedType(typeof(LandedCostingTablesClusterKeyTransformation))]
	sealed class LandedCostingTablesClusterKeyTransformationTest : DataTransformationTestCase
	{
		#region Overrides of OfflineClusterKeyTransformationBaseTest

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var trans = new LandedCostingTablesClusterKeyTransformation();
			trans.Initialise(null, new DummyUpgradeManager());
			return trans;
		}

		protected override void PrepareTestData()
		{
			DropIndexesAndConstraints();
			DeleteNumberFountain("CustomsDeclarationClusterKey");

			var dataCreator = new TransformationTestDataCreator();
			var recDate = new DateTime(2024, 06, 15);

			var companyPK = dataCreator.CreateGlbCompany("AAA", "AU");
			var branchPK = dataCreator.CreateGlbBranch("DDD", companyPK);
			var orgPk = dataCreator.CreateOrg("TES");
			var buyerOrganization1PK = dataCreator.CreateOrgHeader("BUYER1", "Buyer organization1");
			var buyerAddress1PK = dataCreator.CreateOrgAddress(buyerOrganization1PK, "123 street", "OAX", "Blah", "", "THBKK");

			jobDec1Pk = dataCreator.CreateJobDeclaration("AU", branchPK, companyPK, orgPk, "", recDate, 1, string.Empty);
			var jobComInvHeaderPK = dataCreator.CreateJobComInvoiceHeader(branchPK, jobDec1Pk, 1, dataModel: "AU");
			var jobComInvLinePK = dataCreator.CreateJobComInvoiceLine(jobComInvHeaderPK, 1, dataModel: "AU");
			jobDec2Pk = dataCreator.CreateJobDeclaration("AU", branchPK, companyPK, orgPk, "", recDate, 2, string.Empty);
			lCHeader1Pk = dataCreator.CreateLandedCostHeader("ACT", 0.1M, recDate, jobDec1Pk, "JE", companyPK);
			lCInput1Pk = dataCreator.CreateLandCostInput("DESC", 12.3m, jobComInvLinePK, "JI", lCHeader1Pk);
			lcHistory1Pk = dataCreator.CreateLandedCostHistory("ACT", "AU", 1.2m, lCHeader1Pk, "JI");
			lLCItem1Pk = dataCreator.CreateLandedLineCostItem("TDT", 123.4m, lcHistory1Pk);

			var order1Pk = dataCreator.CreateJobOrderHeader(buyerAddress1PK, "PLT");
			lCHeader2Pk = dataCreator.CreateLandedCostHeader("ACT", 0.2M, recDate, order1Pk, "JD", companyPK);
			lCInput2Pk = dataCreator.CreateLandCostInput("DESC", 45.6m, order1Pk, "JD", lCHeader2Pk);
			lcHistory2Pk = dataCreator.CreateLandedCostHistory("ACT", "AU", 3.4m, lCHeader2Pk, "JO");
			lLCItem2Pk = dataCreator.CreateLandedLineCostItem("TDT", 45.6m, lcHistory2Pk);
		}

		protected override void AssertPreConditions()
		{
			CombineAssertions("[PreConditions]: ", () =>
			{
				AssertDatabaseValue(JobDeclarationSchema.JE_ClusterKey, jobDec1Pk, 1);
				AssertDatabaseValue(JobDeclarationSchema.JE_ClusterKey, jobDec2Pk, 2);
				AssertDatabaseValue(LandedCostHeaderSchema.LT_ClusterKey, lCHeader1Pk, 0);
				AssertDatabaseValue(LandCostInputSchema.LI_ClusterKey, lCInput1Pk, 0);
				AssertDatabaseValue(LandedCostHistorySchema.LH_ClusterKey, lcHistory1Pk, 0);
				AssertDatabaseValue(LandedLineCostItemSchema.LZ_ClusterKey, lLCItem1Pk, 0);

				AssertDatabaseValue(LandedCostHeaderSchema.LT_ClusterKey, lCHeader2Pk, 0);
				AssertDatabaseValue(LandCostInputSchema.LI_ClusterKey, lCInput2Pk, 0);
				AssertDatabaseValue(LandedCostHistorySchema.LH_ClusterKey, lcHistory2Pk, 0);
				AssertDatabaseValue(LandedLineCostItemSchema.LZ_ClusterKey, lLCItem2Pk, 0);
			});
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions("JobDeclaration.JE_ClusterKey should not be touched.", () =>
			{
				AssertDatabaseValue(JobDeclarationSchema.JE_ClusterKey, jobDec1Pk, 1);
				AssertDatabaseValue(JobDeclarationSchema.JE_ClusterKey, jobDec2Pk, 2);
			});

			CombineAssertions("ClusterKey should match the parent JobDeclaration", () =>
			{
				AssertDatabaseValue(LandedCostHeaderSchema.LT_ClusterKey, lCHeader1Pk, 1);
				AssertDatabaseValue(LandCostInputSchema.LI_ClusterKey, lCInput1Pk, 1);
				AssertDatabaseValue(LandedCostHistorySchema.LH_ClusterKey, lcHistory1Pk, 1);
				AssertDatabaseValue(LandedLineCostItemSchema.LZ_ClusterKey, lLCItem1Pk, 1);
			});

			CombineAssertions("ClusterKey should be the next value from the fountain", () =>
			{
				AssertDatabaseValue(LandedCostHeaderSchema.LT_ClusterKey, lCHeader2Pk, 3);
				AssertDatabaseValue(LandCostInputSchema.LI_ClusterKey, lCInput2Pk, 3);
				AssertDatabaseValue(LandedCostHistorySchema.LH_ClusterKey, lcHistory2Pk, 3);
				AssertDatabaseValue(LandedLineCostItemSchema.LZ_ClusterKey, lLCItem2Pk, 3);
			});

			AssertNumberFountain(lCHeader2Pk, "CustomsDeclarationClusterKey", fountainExpectedToHaveOwner: false, expectedNextValue: 4);
		}

		#endregion

		Guid jobDec1Pk;
		Guid jobDec2Pk;
		Guid lCHeader1Pk;
		Guid lCInput1Pk;
		Guid lcHistory1Pk;
		Guid lLCItem1Pk;

		Guid lCHeader2Pk;
		Guid lCInput2Pk;
		Guid lcHistory2Pk;
		Guid lLCItem2Pk;

		void AssertDatabaseValue<T>(string message, SchemaColumn schemaColumn, Guid pk, T expectedColumnValue)
		{
			AssertEquals(typeof(T), schemaColumn.DotNetType);

			var selectQuery = $"SELECT [{schemaColumn.Name}] FROM [{schemaColumn.TableSchema.SqlSchemaName}].[{schemaColumn.TableName}] WHERE [{schemaColumn.TableSchema.PK.Name}] = @PrimaryKeyValue";
			using var command = Db.Connection.Command(selectQuery);
			command.AddParameter("@PrimaryKeyValue", SqlDbType.UniqueIdentifier, pk);
			var result = (T)command.ExecuteScalar();

			AssertEquals($"{message} - {schemaColumn.TableName}.{schemaColumn.Name} value does not match expected", expectedColumnValue, result);
		}

		void AssertDatabaseValue<T>(SchemaColumn schemaColumn, Guid pk, T expectedColumnValue)
		{
			AssertDatabaseValue(string.Empty, schemaColumn, pk, expectedColumnValue);
		}

		void AssertNumberFountain(Guid bizOPk, string fountainName, bool fountainExpectedToHaveOwner = true, int expectedNextValue = 1)
		{
			AssertNotEquals(Guid.Empty, bizOPk);
			var numberFountainOwner = Db.Connection.ExecuteScalar($"SELECT SN_Owner FROM dbo.StmNums WHERE SN_Name = '{fountainName}' AND SN_Value = {expectedNextValue}");
			CombineAssertions(() =>
			{
				AssertNotNull("numberFountainOwner should not ne null", numberFountainOwner);
				AssertEquals(fountainExpectedToHaveOwner ? bizOPk : Guid.Empty, numberFountainOwner);
			});
		}

		void DropIndexesAndConstraints()
		{
			var participatingClusterKeys = new[]
			{
				LandedCostHeaderSchema.Constants.LT_ClusterKey,
				LandCostInputSchema.Constants.LI_ClusterKey,
				LandedCostHistorySchema.Constants.LH_ClusterKey,
				LandedLineCostItemSchema.Constants.LZ_ClusterKey
			};

			ClusterKeyTransformationHelper.DropClusterKeyIndexesAndConstraints(TestConnection, participatingClusterKeys);
		}

		void DeleteNumberFountain(string numberFountainName)
		{
			Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.StmNums WHERE SN_Name = '{numberFountainName}'");
		}
	}
}
