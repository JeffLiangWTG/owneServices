using System;
using System.Data;
using System.Data.Common;
using CargoWise.Data.SqlServer;
using NUnit.Framework;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.Data.Testing;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Fullname needed to identify SqlClient types")]
class SqlServerBulkCopyTest : TestCase
{
	[ExpectNoExceptions]
	public void TestSqlServerBulkCopyConstructor()
	{
		var testConnection = (Db.Connection as IDbConnectionInternals).SqlConnection;
		var bulkCopy = new SqlServerBulkCopy(testConnection, SqlBulkCopyOptions.TableLock, null);
		AssertEquals(bulkCopy.BulkCopyOptions, SqlBulkCopyOptions.TableLock);
	}

	public void TestSqlServerBulkCopyConstructorWithNullConnection()
	{
		AssertExceptionThrown<System.ArgumentNullException>(() => new SqlServerBulkCopy(null));
	}

	public void TestSqlServerBulkCopyConstructorWithInvalidConnection()
	{
		var invalidConnection = new TestInvalidConnection();
		AssertExceptionThrown<System.NotSupportedException>(() => new SqlServerBulkCopy(invalidConnection));
	}

	public void TestSqlServerBulkCopyDisposeWrappedObject()
	{
		var testConnection = (Db.Connection as IDbConnectionInternals).SqlConnection;
		var bulkCopy = new SqlServerBulkCopy(testConnection, SqlBulkCopyOptions.TableLock, null);

		var sqlBulkCopySys = GetPrivateFieldValue(bulkCopy, "sqlBulkCopySys") as System.Data.SqlClient.SqlBulkCopy;
		if (sqlBulkCopySys != null)
		{
			AssertNotNull(sqlBulkCopySys.ColumnMappings);
		}

#if NET
		var sqlBulkCopyMS = GetPrivateFieldValue(bulkCopy, "sqlBulkCopyMS") as Microsoft.Data.SqlClient.SqlBulkCopy;
		if (sqlBulkCopyMS != null)
		{
			AssertNotNull(sqlBulkCopyMS.ColumnMappings);
		}
#endif

		bulkCopy.Dispose();

		if (sqlBulkCopySys != null)
		{
			AssertNull(sqlBulkCopySys.ColumnMappings);
		}
#if NET
		if (sqlBulkCopyMS != null)
		{
			AssertNull(sqlBulkCopyMS.ColumnMappings);
		}
#endif
	}

	public void TestSqlServerBulkCopyCanSetAndGetProperties()
	{
		var testConnection = (Db.Connection as IDbConnectionInternals).SqlConnection;
		var bulkCopy = new SqlServerBulkCopy(testConnection, SqlBulkCopyOptions.TableLock, null);

		// Test setting and getting properties
		bulkCopy.DestinationTableName = "TestTable";
		AssertEquals(bulkCopy.DestinationTableName, "TestTable");

		bulkCopy.BatchSize = 1000;
		AssertEquals(bulkCopy.BatchSize, 1000);

		bulkCopy.BulkCopyTimeout = 30;
		AssertEquals(bulkCopy.BulkCopyTimeout, 30);

		bulkCopy.NotifyAfter = 500;
		AssertEquals(bulkCopy.NotifyAfter, 500);
	}

	public void TestPrepareMethodSetColumnMappingsAddedToWrappedObject()
	{
		var testConnection = (Db.Connection as IDbConnectionInternals).SqlConnection;
		var bulkCopy = new SqlServerBulkCopy(testConnection, SqlBulkCopyOptions.TableLock, null);
		bulkCopy.ColumnMappings.Add("SourceColumn1", "DestinationColumn1");
		bulkCopy.ColumnMappings.Add("SourceColumn2", "DestinationColumn2");

		var testTable = new DataTable();
		testTable.Columns.Add("SourceColumn1", typeof(string));
		testTable.Columns.Add("SourceColumn2", typeof(int));
		AssertExceptionThrown<Exception>(() => bulkCopy.WriteToServer(testTable)); // Triggers the Prepare method, ignore exception

		var sqlBulkCopySys = GetPrivateFieldValue(bulkCopy, "sqlBulkCopySys") as System.Data.SqlClient.SqlBulkCopy;
		if (sqlBulkCopySys != null)
		{
			AssertNotNull(sqlBulkCopySys.ColumnMappings);
			AssertEquals(sqlBulkCopySys.ColumnMappings.Count, 2);
			AssertEquals(sqlBulkCopySys.ColumnMappings[0].SourceColumn, "SourceColumn1");
			AssertEquals(sqlBulkCopySys.ColumnMappings[0].DestinationColumn, "DestinationColumn1");
			AssertEquals(sqlBulkCopySys.ColumnMappings[1].SourceColumn, "SourceColumn2");
			AssertEquals(sqlBulkCopySys.ColumnMappings[1].DestinationColumn, "DestinationColumn2");
		}
#if NET
		var sqlBulkCopyMS = GetPrivateFieldValue(bulkCopy, "sqlBulkCopyMS") as Microsoft.Data.SqlClient.SqlBulkCopy;
		if (sqlBulkCopyMS != null)
		{
			AssertNotNull(sqlBulkCopyMS.ColumnMappings);
			AssertEquals(sqlBulkCopyMS.ColumnMappings.Count, 2);
			AssertEquals(sqlBulkCopyMS.ColumnMappings[0].SourceColumn, "SourceColumn1");
			AssertEquals(sqlBulkCopyMS.ColumnMappings[0].DestinationColumn, "DestinationColumn1");
			AssertEquals(sqlBulkCopyMS.ColumnMappings[1].SourceColumn, "SourceColumn2");
			AssertEquals(sqlBulkCopyMS.ColumnMappings[1].DestinationColumn, "DestinationColumn2");
		}
#endif
	}

	object GetPrivateFieldValue(object obj, string fieldName)
	{
		var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field?.GetValue(obj);
	}

	class TestInvalidConnection : System.Data.Common.DbConnection
	{
		public override string ConnectionString { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
		public override string Database => throw new System.NotImplementedException();
		public override string DataSource => throw new System.NotImplementedException();
		public override string ServerVersion => throw new System.NotImplementedException();
		public override ConnectionState State => throw new System.NotImplementedException();
		public override void ChangeDatabase(string databaseName) => throw new System.NotImplementedException();
		public override void Close() => throw new System.NotImplementedException();
		public override void Open() => throw new System.NotImplementedException();
		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new System.NotImplementedException();
		protected override System.Data.Common.DbCommand CreateDbCommand() => throw new System.NotImplementedException();
	}
}
