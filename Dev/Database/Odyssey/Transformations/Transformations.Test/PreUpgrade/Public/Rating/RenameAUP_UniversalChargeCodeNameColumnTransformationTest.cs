using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PreUpgrade.Public.Rating
{
	[UseSnapshotProtection]
	[TestedType(typeof(RenameAUP_UniversalChargeCodeNameColumnTransformation))]
	public class RenameAUP_UniversalChargeCodeNameColumnTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new RenameAUP_UniversalChargeCodeNameColumnTransformation();

		protected override void PrepareTestData()
		{
			EnsureOldColumnExists(oldColumnName, AccChargeCodeUniversalCodeMappingSchema.AUP_Code);

			AssertColumnNamesAreOld();

			var testDataCreator = new TransformationTestDataCreator();
			var companyPk = testDataCreator.CreateCompany("CMP", "AU");
			var accChargeCode = RatingTransformationHelper.CreateAccChargeCode("TEST", "XXX", companyPk);
			chargeCode = "AUSTEST";
			accChargeCodeUniversalCodeMapping = Guid.NewGuid();

			const string createSQL = "INSERT INTO dbo.AccChargeCodeUniversalCodeMapping(AUP_PK, AUP_AC, AUP_UniversalChargeCode, AUP_SystemCreateTimeUtc, AUP_SystemCreateUser, AUP_SystemLastEditTimeUtc, AUP_SystemLastEditUser) VALUES (@pk, @accChargeCodePK, @chargeCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(createSQL))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, accChargeCodeUniversalCodeMapping);
				command.AddParameter("@accChargeCodePK", SqlDbType.UniqueIdentifier, accChargeCode);
				command.AddParameterBasedOnDbColumn("@chargeCode", chargeCode, AccChargeCodeUniversalCodeMappingSchema.AUP_Code);
				command.ExecuteNonQuery();
			}
		}

		protected override void AssertTransformationResults()
		{
			AssertColumnNamesAreNew();
			string newChargeCode = "";

			TestConnection.ExecuteReader($"select top 1 {newColumnName} from dbo.AccChargeCodeUniversalCodeMapping where AUP_PK = '{accChargeCodeUniversalCodeMapping}'",
				record =>
				{
					newChargeCode = record[newColumnName] as string;
				});

			AssertEquals(chargeCode, newChargeCode);
		}

		Guid accChargeCodeUniversalCodeMapping;
		string chargeCode;

		const string oldColumnName = "AUP_UniversalChargeCode";
		const string newColumnName = AccChargeCodeUniversalCodeMappingSchema.Constants.AUP_Code;

		void EnsureOldColumnExists(string oldName, SchemaColumn schemaColumn)
		{
			if (DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, schemaColumn.Name))
			{
				new DbColumnDependencyRemover(AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, schemaColumn.Name).DropRelateObjects(TestConnection);
				DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, schemaColumn.Name, oldName);
			}
			else if (!DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, oldName))
			{
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, oldName, schemaColumn.TypeInfo);
			}
		}

		void AssertColumnNamesAreNew()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, newColumnName));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, oldColumnName));
		}

		void AssertColumnNamesAreOld()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, oldColumnName));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, AccChargeCodeUniversalCodeMappingSchema.Constants.TableName, newColumnName));
		}

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
