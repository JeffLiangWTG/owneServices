using System;
using System.Data;
using System.IO;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class SqlTextFieldWriterTest : TransactionedTestCase
	{
		readonly DbConnection testConnection = Db.Connection;
		readonly string testTableName = "Test_" + Guid.NewGuid().ToString().Replace("-", "");
		const string testPKColumnName = "PK";
		const string VarCharColumnName = "VarCharMax";
		const string NVarCharColumnName = "NVarCharMax";
		readonly Guid testPKValue = Guid.NewGuid();
		const int bufferSize = 256;

		class SqlStreamWriterForTest : SqlStreamWriter<char>
		{
			public SqlStreamWriterForTest(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, int writeChunkSize, bool append)
				: base(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType, writeChunkSize, append)
			{
			}

			internal override DbCommand CreateInitCommand(DbConnection connection, string table, string dataColumn, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataType, int writeChunkSize)
			{
				return null;
			}

			internal override DbCommand CreateWriteCommand(DbConnection connection, string table, string dataColumn, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataType, int writeChunkSize)
			{
				return null;
			}

			public void DisposeForTest()
			{
				DisposeOnError();
				base.Dispose(true);
			}
		}

		public void TestNoExceptionThrownOnDispose()
		{
			SqlStreamWriterForTest helper = new SqlStreamWriterForTest(testConnection, testTableName, testPKColumnName, testPKValue, DataColumnName(SqlDbType.VarChar), SqlDbType.VarChar, 80400, false);
			AssertNoExceptionThrown(() => helper.DisposeForTest());
		}

		public void TestNoTableArgumentException()
		{
			foreach (var sqlDbType in new SqlDbType[] { SqlDbType.VarChar, SqlDbType.NVarChar })
			{
				using (var writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, DataColumnName(sqlDbType), sqlDbType))
				{
					writer.Write(Array.Empty<char>(), 0, 0);
					AssertExceptionThrown(sqlDbType.ToString(), typeof(ArgumentException), () => { writer.Flush(); });
				}
			}
		}

		public void TestNoPKValueArgumentException()
		{
			CreateTableData();
			foreach (var sqlDbType in new SqlDbType[] { SqlDbType.VarChar, SqlDbType.NVarChar })
			{
				using (var writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, DataColumnName(sqlDbType), sqlDbType))
				{
					writer.Write(Array.Empty<char>(), 0, 0);
					AssertExceptionThrown(sqlDbType.ToString(), typeof(ArgumentException), () => { writer.Flush(); });
				}
			}
		}

		public void TestProperty()
		{
			CreateTableData();
			InsertTestData();
			AssertProperty(VarCharColumnName, SqlDbType.VarChar);
			AssertProperty(NVarCharColumnName, SqlDbType.NVarChar);
		}

		void AssertProperty(string columnname, SqlDbType sqlDbType)
		{
			var testFileSize = 50 * 1024;
			var textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);

			try
			{
				var buffer = new Char[bufferSize];
				var read = 0;

				var writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType);

				using (FileStream readerStream = new FileStream(textFileName, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(readerStream))
					{
						read = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, read);
					}
				}

				AssertEquals(false, writer.CanRead);
				AssertEquals(false, writer.CanSeek);
				AssertEquals(true, writer.CanWrite);

				AssertEquals(buffer.Length, writer.Length);
				AssertEquals(buffer.Length, writer.Position);
				AssertExceptionThrown(typeof(NotSupportedException), () => { writer.Position = 100; });
				AssertExceptionThrown(typeof(NotSupportedException), () => { writer.Seek(-4, SeekOrigin.End); });

				writer.Flush();
				writer.Dispose();
				writer.Close();
			}
			finally
			{
				File.Delete(textFileName);
			}
		}

		public void TestAppend()
		{
			CreateTableData();
			InsertTestData();
			AssertAppend(VarCharColumnName, SqlDbType.VarChar);
			AssertAppend(NVarCharColumnName, SqlDbType.NVarChar);
		}

		void AssertAppend(string columnname, SqlDbType sqlDbType)
		{
			string firstFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, 1000);
			string secondFileName = string.Empty;

			try
			{
				char[] buffer = new char[bufferSize];
				int read = 0;
				using (var writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType, true))
				using (var reader = new StreamReader(firstFileName))
				{
					do
					{
						read = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, read);
					}
					while (read > 0);
				}
				using (var writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType, true))
				using (var reader = new StreamReader(firstFileName))
				{
					do
					{
						read = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, read);
					}
					while (read > 0);
				}
				secondFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, 2000);
				using (var reader = new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType))
				{
					AssertFileSameAsString(secondFileName, reader.ReadToEnd());
				}
			}
			finally
			{
				File.Delete(firstFileName);
				if (!string.IsNullOrEmpty(secondFileName))
				{
					File.Delete(secondFileName);
				}
			}
		}

		#region Implementation

		void CreateTableData()
		{
			string sqlText = string.Format(@"
					CREATE TABLE {0} (
						{1} uniqueidentifier not null,
						{2} varchar(MAX) NULL,
						{3} nvarchar(MAX) NULL
						)", testTableName, testPKColumnName, VarCharColumnName, NVarCharColumnName);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void InsertTestData()
		{
			string sqlText = "INSERT " + testTableName + "(" + testPKColumnName + ") VALUES ('" + testPKValue.ToString() + "')";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		static string DataColumnName(SqlDbType sqlDbType)
		{
			return sqlDbType == SqlDbType.VarChar ? VarCharColumnName : NVarCharColumnName;
		}

		#endregion
	}
}
