using CargoWise.Data;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Testing
{
	public abstract class PreUpgradeNonTransactionedTransformationTestCase<T> : TestCase where T : DataTransformation, new()
	{
		protected override void SetUp()
		{
			base.SetUp();
			adminConnection = Db.NewAdminConnection();
			Db.ConnectionOverrideForTest = adminConnection;
		}
		AdminConnection adminConnection;

		protected override void TearDown()
		{
			if (adminConnection != null)
			{
				Db.ConnectionOverrideForTest = null;
				adminConnection.Dispose();
				adminConnection = null;
			}
			base.TearDown();
		}

		protected AdminConnection TestConnection
		{
			get
			{
				return adminConnection;
			}
		}

		protected T GetNewTestTransformationInstance()
		{
			var result = new T();
			result.Initialise(new VersionLabel(0, 0), new DummyUpgradeManager());
			return result;
		}

		// Dropping CDC enabled tables/columns requires admin connection so TransactionedTestCase cannot be used. Tests should apply UseSnapshotAttribute instead to roll back transaction.

		protected void AssertColumnMissingDoesNotThrowException(SchemaColumn schemaColumn)
		{
			AssertColumnMissingDoesNotThrowException(schemaColumn.TableName, schemaColumn.Name);
		}

		protected void AssertColumnMissingDoesNotThrowException(string tableName, string columnName)
		{
			DropColumnDependencies(tableName, columnName);
			AssertNoExceptionThrown(GetNewTestTransformationInstance().Run);
		}

		protected void DropColumnDependencies(string tableName, string columnName)
		{
			var schemaName = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchema(tableName)?.SqlSchemaName ?? Db.SqlDbOwnerSchema;
			new DbColumnDependencyRemover(schemaName, tableName, columnName).DropRelateObjects(adminConnection);
			adminConnection.ExecuteNonQuery($"ALTER TABLE {schemaName}.{tableName} DROP COLUMN {columnName}");
		}

		protected void AssertTableMissingDoesNotThrowException(string tableName)
		{
			DropTableIncludingDependencies(tableName);
			AssertNoExceptionThrown(GetNewTestTransformationInstance().Run);
		}

		protected void DropTableIncludingDependencies(string tableName)
		{
			var resolver = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>();
			var table = resolver.GetTableSchema(tableName);

			foreach (var column in resolver.GetSchemaColumns(tableName))
			{
				new DbColumnDependencyRemover(table.SqlSchemaName, tableName, column.Name).DropRelateObjects(adminConnection);
			}

			adminConnection.ExecuteNonQuery($"DROP TABLE {table.SqlSchemaName}.{tableName}");
		}
	}
}
