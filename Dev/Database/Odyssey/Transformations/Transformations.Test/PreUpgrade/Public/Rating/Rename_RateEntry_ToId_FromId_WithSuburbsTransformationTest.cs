using System;
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
	[TestedType(typeof(Rename_RateEntry_ToId_FromId_WithSuburbsTransformation))]
	public class Rename_RateEntry_ToId_FromId_WithSuburbsTransformationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new Rename_RateEntry_ToId_FromId_WithSuburbsTransformation();

		protected override void PrepareTestData()
		{
			EnsureOldColumnExists(oldColumnNameTo, RateEntrySchema.TI_R9_ToSuburb);
			EnsureOldColumnExists(oldColumnNameFrom, RateEntrySchema.TI_R9_FromSuburb);

			AssertColumnNamesAreOld();

			var testDataCreator = new TransformationTestDataCreator();
			var companyPk = testDataCreator.CreateCompany("CMP", "AU");
			var orgHeaderPk = RatingTransformationHelper.CreateRateHeader("GLB");
			rateEntry = RatingTransformationHelper.CreateRateEntry(orgHeaderPk, companyPk, "ORG", "SEA", DateTime.UtcNow, DateTime.UtcNow);
			cityTownPkTo = testDataCreator.CreateRefCityTown("someTownTo");
			cityTownPkFrom = testDataCreator.CreateRefCityTown("someTownFrom");

			var updateRateEntryQuery = @$"update dbo.RateEntry
				set TI_ToID = '{cityTownPkTo}', TI_FromID = '{cityTownPkFrom}'
				where TI_PK = '{rateEntry}'";
			TestConnection.ExecuteNonQuery(updateRateEntryQuery);
		}

		protected override void AssertTransformationResults()
		{
			AssertColumnNamesAreNew();
			Guid? newTo = null;
			Guid? newFrom = null;

			TestConnection.ExecuteReader($"select top 1 {newColumnNameTo}, {newColumnNameFrom} from dbo.RateEntry where TI_PK = '{rateEntry}'",
				record =>
				{
					newTo = record[newColumnNameTo] as Guid?;
					newFrom = record[newColumnNameFrom] as Guid?;
				});

			AssertEquals(cityTownPkTo, newTo);
			AssertEquals(cityTownPkFrom, newFrom);
		}

		Guid rateEntry;
		Guid cityTownPkTo;
		Guid cityTownPkFrom;

		const string oldColumnNameTo = "TI_ToId";
		const string oldColumnNameFrom = "TI_FromID";
		const string newColumnNameTo = RateEntrySchema.Constants.TI_R9_ToSuburb;
		const string newColumnNameFrom = RateEntrySchema.Constants.TI_R9_FromSuburb;

		void EnsureOldColumnExists(string oldName, SchemaColumn schemaColumn)
		{
			if (DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, schemaColumn.Name))
			{
				new DbColumnDependencyRemover(RateEntrySchema.Constants.TableName, schemaColumn.Name).DropRelateObjects(TestConnection);
				DbObjectCreator.RenameColumn(TestConnection, Db.SqlDbOwnerSchema, RateEntrySchema.Constants.TableName, schemaColumn.Name, oldName);
			}
			else if (!DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, oldName))
			{
				DbObjectCreator.CreateColumnIfNotExists(TestConnection, RateEntrySchema.Constants.TableName, oldName, schemaColumn.TypeInfo);
			}
		}

		void AssertColumnNamesAreNew()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, newColumnNameTo));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, newColumnNameFrom));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, oldColumnNameTo));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, oldColumnNameFrom));
		}

		void AssertColumnNamesAreOld()
		{
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, oldColumnNameTo));
			AssertEquals(true, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, oldColumnNameFrom));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, newColumnNameTo));
			AssertEquals(false, DbObjectCreator.ColumnExists(TestConnection, RateEntrySchema.Constants.TableName, newColumnNameFrom));
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
