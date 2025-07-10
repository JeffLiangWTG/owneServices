using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PreUpgrade.Public.Rating
{
	[UseSnapshotProtection]
	[TestedType(typeof(RenameJK_RH_NKRatingCommodityCodeToJK_RH_NKConsolCommodity))]
	public class RenameJK_RH_NKRatingCommodityCodeToJK_RH_NKConsolCommodityTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameJK_RH_NKRatingCommodityCodeToJK_RH_NKConsolCommodity();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropFunctionIfExists("GetJobConsolsWithSecurityContext");
			DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, JobConsolSchema.Constants.TableName, newColumnName, oldColumnName);
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, JobConsolSchema.Constants.TableName, oldColumnName));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, JobConsolSchema.Constants.TableName, newColumnName));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, JobConsolSchema.Constants.TableName, newColumnName));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, JobConsolSchema.Constants.TableName, oldColumnName));
		}

		protected override string ReasonNotToBeMapped => "Baseline";

		const string oldColumnName = "JK_RH_NKRatingCommodityCode";
		const string newColumnName = JobConsolSchema.Constants.JK_RH_NKConsolCommodity;

		#region Implementations

		protected override void SetUp()
		{
			base.SetUp();
			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
		}

		protected override sealed void TearDown()
		{
			disposableAdminConnection.Dispose();
			base.TearDown();
		}

		IDisposable disposableAdminConnection;

		#endregion
	}
}
