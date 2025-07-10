using System;
using System.Data;
using System.IO;
using System.Xml;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class SqlTextFieldReaderTest : TransactionedTestCase
	{
		readonly DbConnection testConnection = Db.Connection;
		readonly string testTableName = "Test_" + Guid.NewGuid().ToString().Replace("-", "");
		const string testPKColumnName = "PK";
		const string VarCharColumnName = "VarCharMax";
		const string NVarCharColumnName = "NVarCharMax";
		readonly Guid testPKValue = Guid.NewGuid();
		const int bufferSize = 1024;
		readonly int testFileSize = 50 * 1024;

		public void TestNoTableArgumentException()
		{
			foreach (var sqlDbType in new SqlDbType[] { SqlDbType.VarChar, SqlDbType.NVarChar })
			{
				AssertExceptionThrown(sqlDbType.ToString(), typeof(ArgumentException), () => { new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, DataColumnName(sqlDbType), sqlDbType); });
			}
		}

		public void TestNoPKValueArgumentException()
		{
			CreateTableData();
			foreach (var sqlDbType in new SqlDbType[] { SqlDbType.VarChar, SqlDbType.NVarChar })
			{
				AssertExceptionThrown(sqlDbType.ToString(), typeof(SqlStreamReaderRowNotFoundException), () => { new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, DataColumnName(sqlDbType), sqlDbType); });
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
			string textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			try
			{
				Char[] buffer = new Char[bufferSize];
				Int32 read = 0;

				using (SqlTextFieldWriter writer = new SqlTextFieldWriter(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType))
				{
					using (FileStream readerStream = new FileStream(textFileName, FileMode.Open))
					{
						using (StreamReader reader = new StreamReader(readerStream))
						{
							read = reader.Read(buffer, 0, buffer.Length);
							writer.Write(buffer, 0, read);
						}
					}
				}

				using (SqlTextFieldReader charReader = new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType))
				{
					AssertEquals(true, charReader.CanRead);
					AssertEquals(true, charReader.CanSeek);
					AssertEquals(false, charReader.CanWrite);

					charReader.Position = 100;
					AssertEquals(buffer.Length, charReader.Length);
					AssertEquals(100, charReader.Position);
					AssertEquals(buffer.Length - 4, charReader.Seek(-4, SeekOrigin.End));
					AssertEquals(4, charReader.Seek(4, SeekOrigin.Begin));
					AssertEquals(8, charReader.Seek(4, SeekOrigin.Current));
					AssertEquals(4, charReader.Seek(-4, SeekOrigin.Current));
				}
			}
			finally
			{
				File.Delete(textFileName);
			}
		}

		public void TestReadWriteTextFile_VarChar()
		{
			// Arrange
			CreateTableData();
			InsertTestData();

			ReadWriteTextFileWarmUp(VarCharColumnName, SqlDbType.VarChar, LargeFileSize);

			// Act
			// Assert
			AssertReadWriteTextFile(VarCharColumnName, SqlDbType.VarChar, LargeFileSize, MaxExpectedMemorySize);
		}

		public void TestReadWriteTextFile_NVarChar()
		{
			// Arrange
			CreateTableData();
			InsertTestData();

			ReadWriteTextFileWarmUp(NVarCharColumnName, SqlDbType.NVarChar, LargeFileSize);

			// Act
			// Assert
			AssertReadWriteTextFile(NVarCharColumnName, SqlDbType.NVarChar, LargeFileSize, MaxExpectedMemorySize);
		}

		const int LargeFileSize = 10 * 1024 * 1024;    // 10MB
		const int MaxExpectedMemorySize =  200 * 1024; // 200KB

		void AssertReadWriteTextFile(string columnName, SqlDbType sqlDbType, int fileSize, long maxExpectedMemorySize)
		{
			string textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, fileSize);
			string outputFileName = TempForTest.GetTempFileName();
			string assertPrefix = columnName + " " + fileSize + " ";

			try
			{
				long memorySize1 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();
				int commandCount1 = testConnection.ExecutedCommandCount;

				SqlStreamTestHelper.WriteTextStream(textFileName, 1024, testConnection, testTableName, testPKColumnName, testPKValue, columnName, sqlDbType);

				long memorySize2 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();
				int commandCount2 = testConnection.ExecutedCommandCount;

				ReadTextStream(outputFileName, columnName, sqlDbType, testPKValue);

				long memorySize3 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();
				int commandCount3 = testConnection.ExecutedCommandCount;

				AssertLessThan(assertPrefix + "Writing consumes more memory than expectation", memorySize2 - memorySize1, maxExpectedMemorySize);
				AssertLessThan(assertPrefix + "Reading consumes more memory than expectation", memorySize3 - memorySize2, maxExpectedMemorySize);

				const int chunkSize = 80400;
				int chunks = (fileSize + chunkSize - 1) / chunkSize;
				AssertEquals(assertPrefix + "Write operation should consume 1 Command per SqlStreamWriter.writeChunkSize", commandCount1 + chunks, commandCount2);
				AssertEquals(assertPrefix + "Read operation should consume only 1 Command", commandCount2 + 1, commandCount3);

				Assert(assertPrefix + "Files not equal", SqlStreamTestHelper.TextFileAreEqual(textFileName, outputFileName));
			}
			finally
			{
				File.Delete(textFileName);
				File.Delete(outputFileName);
			}
		}

		void ReadWriteTextFileWarmUp(string columnName, SqlDbType sqlDbType, int fileSize)
		{
			var textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, fileSize);
			using (new DisposableAction(() => File.Delete(textFileName)))
			{
				SqlStreamTestHelper.WriteTextStream(textFileName, 1024, testConnection, testTableName, testPKColumnName, testPKValue, columnName, sqlDbType);
			}

			var outputFileName = TempForTest.GetTempFileName();
			using (new DisposableAction(() => File.Delete(outputFileName)))
			{
				ReadTextStream(outputFileName, columnName, sqlDbType, testPKValue);
			}

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void TestXmlReaderWriter()
		{
			CreateTableData();
			InsertTestData();
			AssertXmlReaderWriter(VarCharColumnName, SqlDbType.VarChar);
			AssertXmlReaderWriter(NVarCharColumnName, SqlDbType.NVarChar);
		}

		void AssertXmlReaderWriter(string columnname, SqlDbType sqlDbType)
		{
			string xmlFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Xml, testFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				SqlStreamTestHelper.WriteTextStreamUsingXmlWriter(xmlFileName, testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType);
				ReadTextStreamUsingXmlReader(outputFileName, columnname, sqlDbType);

				Assert("Files not equal", SqlStreamTestHelper.XmlFileAreEqual(xmlFileName, outputFileName));
			}
			finally
			{
				File.Delete(xmlFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestPeek()
		{
			CreateTableData();
			InsertTestData();
			AssertPeek(VarCharColumnName, SqlDbType.VarChar);
			AssertPeek(NVarCharColumnName, SqlDbType.NVarChar);
		}

		void AssertPeek(string columnname, SqlDbType sqlDbType)
		{
			string textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			try
			{
				SqlStreamTestHelper.WriteTextStream(textFileName, 1024, testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType);
				using (var reader = new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, columnname, sqlDbType))
				{
					for (int readLength = 1; readLength < 20; readLength++)
					{
						AssertEquals(reader.Peek(), reader.Read());
						reader.ReadBlock(new char[readLength], 0, readLength);
					}
				}
			}
			finally
			{
				File.Delete(textFileName);
			}
		}

		#region Implementation

		void ReadTextStream(string outputFilePath, string columnName, SqlDbType sqlDbType, Guid pk)
		{
			char[] buffer = new char[bufferSize];
			Int32 read = 0;

			using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					using (SqlTextFieldReader reader = new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, pk, columnName, sqlDbType))
					{
						do
						{
							read = reader.Read(buffer, 0, buffer.Length);
							writer.Write(buffer, 0, read);
						}
						while (read > 0);
					}
				}
			}
		}

		void ReadTextStreamUsingXmlReader(string outputFilePath, string columnName, SqlDbType sqlDbType)
		{
			using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (XmlWriter writer = XmlWriter.Create(fileStream))
				{
					using (SqlTextFieldReader readerStream = new SqlTextFieldReader(testConnection, testTableName, testPKColumnName, testPKValue, columnName, sqlDbType))
					{
						using (XmlReader reader = XmlReader.Create(readerStream))
						{
							SqlStreamTestHelper.CopyXmlStream(reader, writer);
						}
					}
				}
			}
		}

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
