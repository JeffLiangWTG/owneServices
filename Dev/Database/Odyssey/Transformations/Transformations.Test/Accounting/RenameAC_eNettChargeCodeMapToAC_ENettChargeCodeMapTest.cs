using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[UseSnapshotProtection]
	[TestedType(typeof(RenameAC_eNettChargeCodeMapToAC_ENettChargeCodeMap))]

	class RenameAC_eNettChargeCodeMapToAC_ENettChargeCodeMapTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameAC_eNettChargeCodeMapToAC_ENettChargeCodeMap();

		protected override void PrepareTestData()
		{
			if (TestDbObjectHelper.IsColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, AccChargeCodeSchema.Constants.TableName, AccChargeCodeSchema.AC_ENettChargeCodeMap.Name))
			{
				new DbColumnDependencyRemover(AccChargeCodeSchema.Constants.TableName, AccChargeCodeSchema.AC_ENettChargeCodeMap.Name).DropRelateObjects(TestConnection);
				DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, AccChargeCodeSchema.Constants.TableName, AccChargeCodeSchema.AC_ENettChargeCodeMap.Name, "AC_eNettChargeCodeMap");
			}

			if (!TestDbObjectHelper.IsColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, AccChargeCodeSchema.Constants.TableName, "AC_eNettChargeCodeMap"))
			{
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, AccChargeCodeSchema.Constants.TableName, "AC_eNettChargeCodeMap", "varchar(10)");
			}

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.AccChargeCode (AC_PK, AC_ChargeGroup, AC_eNettChargeCodeMap, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES ('{ChargeCodePK}', 'FRT', 'Dummy', GetUtcDate(), '~BP',  GetUtcDate(), '~BP')");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(false, TestDbObjectHelper.IsColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, AccChargeCodeSchema.Constants.TableName, "AC_eNettChargeCodeMap"));
			AssertEquals(true, TestDbObjectHelper.IsColumnExistCaseSensitive(TestConnection, Db.DatabaseName, Db.SqlDbOwnerSchema, AccChargeCodeSchema.Constants.TableName, "AC_ENettChargeCodeMap"));

			AssertEquals("Data should exists", "Dummy", TestConnection.ExecuteScalar<string>($@"SELECT AC_eNettChargeCodeMap FROM dbo.AccChargeCode WHERE AC_PK = '{ChargeCodePK}'"));
		}

		readonly Guid ChargeCodePK = Guid.NewGuid();
	}
}
