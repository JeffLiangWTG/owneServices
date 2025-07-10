using System;
using System.Data;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class BinaryStreamTest : TransactionedTestCase
	{
		readonly DbConnection testConnection = Db.Connection;
		readonly string testTableName = "Test_" + Guid.NewGuid().ToString().Replace("-", "");
		const string testPKColumnName = "PK";
		const string testDataColumnName = "binary";
		readonly Guid testPKValue = Guid.NewGuid();
		const int bufferSize = 1024;
		const long testSmallFileSize = 1000;
		readonly long testFileSize = 100 * 1024;

		public void TestNoTableArgumentException()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => { SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary); });
			using (var writer = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
			{
				writer.Write(Array.Empty<byte>(), 0, 0);
				AssertExceptionThrown(typeof(ArgumentException), () => { writer.Flush(); });
			}
		}

		public void TestNoPKValueArgumentException()
		{
			CreateTableData();
			AssertExceptionThrown(typeof(SqlStreamReaderRowNotFoundException), () => { SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary); });
			using (var writer = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
			{
				writer.Write(Array.Empty<byte>(), 0, 0);
				AssertExceptionThrown(typeof(ArgumentException), () => { writer.Flush(); });
			}
		}

		public void TestProperty()
		{
			CreateTableData();
			InsertTestData();
			string binaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			try
			{
				Byte[] buffer = new Byte[bufferSize];
				Int32 read = 0;

				SqlBinaryFieldStream writerStream = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary);
				AssertEquals(false, writerStream.CanRead);
				AssertEquals(false, writerStream.CanSeek);
				AssertEquals(true, writerStream.CanWrite);

				FileStream readerFileStream = new FileStream(binaryFileName, FileMode.Open);
				BinaryReader reader = new BinaryReader(readerFileStream);

				read = reader.Read(buffer, 0, buffer.Length);
				writerStream.Write(buffer, 0, read);

				AssertEquals(buffer.Length, writerStream.Length);
				AssertExceptionThrown(typeof(NotSupportedException), () => { writerStream.Position = 100; });
				AssertExceptionThrown(typeof(NotSupportedException), () => { writerStream.Seek(-4, SeekOrigin.End); });
				writerStream.Flush();
				SqlBinaryFieldStream readerStream = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary);
				AssertEquals(true, readerStream.CanRead);
				AssertEquals(true, readerStream.CanSeek);
				AssertEquals(false, readerStream.CanWrite);

				AssertEquals(buffer.Length, readerStream.Length);

				readerStream.Position = 100;
				AssertEquals(100, readerStream.Position);
				AssertEquals(buffer.Length - 4, readerStream.Seek(-4, SeekOrigin.End));
				AssertEquals(4, readerStream.Seek(4, SeekOrigin.Begin));
				AssertEquals(8, readerStream.Seek(4, SeekOrigin.Current));
				AssertEquals(4, readerStream.Seek(-4, SeekOrigin.Current));

				read = readerStream.Read(buffer, 0, buffer.Length);
				AssertEquals(buffer.Length, readerStream.Position);

				readerFileStream.Close();
				writerStream.Flush();
				writerStream.Dispose();
				writerStream.Close();

				readerStream.Flush();
				readerStream.Dispose();
				readerStream.Close();
			}
			finally
			{
				File.Delete(binaryFileName);
			}
		}

		public void TestReadNullData()
		{
			CreateTableData();
			InsertTestData();

			AssertNoExceptionThrown("Should not throw exception when read null data", () =>
			{
				using (SqlBinaryFieldStream readerStream = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
				{
					AssertNotNull(readerStream);
					AssertEquals(readerStream.Length, 0);
					AssertEquals(true, readerStream.CanRead);
					AssertEquals(true, readerStream.CanSeek);
					AssertEquals(false, readerStream.CanWrite);
				}
			});
		}

		public void TestBinaryReadWriteSmallFile()
		{
			CreateTableData();
			InsertTestData();
			string binaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testSmallFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				int commandCount1 = testConnection.ExecutedCommandCount;
				WriteBinaryStream(binaryFileName);
				int commandCount2 = testConnection.ExecutedCommandCount;
				ReadBinaryStream(outputFileName);
				int commandCount3 = testConnection.ExecutedCommandCount;

				AssertEquals("Write operation should consume only 1 Command", commandCount1 + 1, commandCount2);
				AssertEquals("Read operation should consume only 1 Command", commandCount2 + 1, commandCount3);

				Assert("Files not equal", SqlStreamTestHelper.BinaryFileAreEqual(binaryFileName, outputFileName));
			}
			finally
			{
				File.Delete(binaryFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestBinaryReadWriteFile()
		{
			CreateTableData();
			InsertTestData();
			string binaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				int commandCount1 = testConnection.ExecutedCommandCount;
				WriteBinaryStream(binaryFileName);
				int commandCount2 = testConnection.ExecutedCommandCount;
				ReadBinaryStream(outputFileName);
				int commandCount3 = testConnection.ExecutedCommandCount;

				AssertEquals("Write operation should consume only 1 Command", commandCount1 + 2, commandCount2);
				AssertEquals("Read operation should consume only 1 Command", commandCount2 + 1, commandCount3);

				Assert("Files not equal", SqlStreamTestHelper.BinaryFileAreEqual(binaryFileName, outputFileName));
			}
			finally
			{
				File.Delete(binaryFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestBinaryReadWriteLargeFile()
		{
			CreateTableData();
			InsertTestData();
			var testLargeFileSize = 100 * 1024 * 1024;
			var textLargeFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testLargeFileSize);
			var outputFileName = TempForTest.GetTempFileName();

			try
			{
				var memorySize1 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();
				WriteBinaryStream(textLargeFileName);
				var memorySize2 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();
				ReadBinaryStream(outputFileName);
				var memorySize3 = SqlStreamTestHelper.GetCurrentApplicationMemorySizeByte();

				Assert("Writing consumes more than 2 mb of memory ", memorySize2 - memorySize1 < 1024 * 1024 * 2);
				Assert("Reading consumes more than 2 mb of memory ", memorySize3 - memorySize2 < 1024 * 1024 * 2);
			}
			finally
			{
				File.Delete(textLargeFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestBinaryReaderWriter()
		{
			CreateTableData();
			InsertTestData();
			string binaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				WriteBinaryStreamUsingBinaryWriter(binaryFileName);
				ReadBinaryStreamUsingBinaryReader(outputFileName);

				Assert("Files not equal", SqlStreamTestHelper.BinaryFileAreEqual(binaryFileName, outputFileName));
			}
			finally
			{
				File.Delete(binaryFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestStreamReaderWriterWithoutCache()
		{
			CreateTableData();
			InsertTestData();
			string textFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Text, testFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				WriteTextStreamUsingStreamWriterWithoutCache(textFileName);
				ReadTextStreamUsingStreamReaderWithoutCache(outputFileName);

				Assert("Files not equal", SqlStreamTestHelper.TextFileAreEqual(textFileName, outputFileName));
			}
			finally
			{
				File.Delete(textFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestXmlReaderWriter()
		{
			CreateTableData();
			InsertTestData();
			string xmlFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Xml, testFileSize);
			string outputFileName = TempForTest.GetTempFileName();

			try
			{
				WriteTextStreamUsingXmlWriter(xmlFileName);
				ReadTextStreamUsingXmlReader(outputFileName);

				Assert("Files not equal", SqlStreamTestHelper.XmlFileAreEqual(xmlFileName, outputFileName));
			}
			finally
			{
				File.Delete(xmlFileName);
				File.Delete(outputFileName);
			}
		}

		public void TestAppend()
		{
			CreateTableData();
			InsertTestData();
			string firstBinaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Binary, testSmallFileSize);
			string secondBinaryFileName = string.Empty;

			try
			{
				byte[] buffer = new byte[bufferSize];
				int read = 0;
				using (var writer = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary, true))
				using (var reader = File.OpenRead(firstBinaryFileName))
				{
					do
					{
						read = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, read);
					}
					while (read > 0);
				}
				using (var writer = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary, true))
				using (var reader = File.OpenRead(firstBinaryFileName))
				{
					do
					{
						read = reader.Read(buffer, 0, buffer.Length);
						writer.Write(buffer, 0, read);
					}
					while (read > 0);
				}
				secondBinaryFileName = SqlStreamTestHelper.CreateTestFile(SqlStreamTestHelper.FileType.Binary, testSmallFileSize * 2);
				using (var reader1 = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
				using (var reader2 = File.OpenRead(secondBinaryFileName))
				{
					Assert(SqlStreamTestHelper.StreamAreEqual(reader1, reader2));
				}
			}
			finally
			{
				File.Delete(firstBinaryFileName);
				if (!string.IsNullOrEmpty(secondBinaryFileName))
				{
					File.Delete(secondBinaryFileName);
				}
			}
		}

		#region implementation

		void WriteBinaryStream(string fileName, string column = testDataColumnName)
		{
			Byte[] buffer = new Byte[bufferSize];
			Int32 read = 0;

			using (SqlBinaryFieldStream writer = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, column, SqlDbType.VarBinary))
			{
				using (FileStream readerStream = new FileStream(fileName, FileMode.Open))
				{
					using (BinaryReader reader = new BinaryReader(readerStream))
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

		void WriteBinaryStreamUsingBinaryWriter(string fileName)
		{
			Byte[] buffer = new Byte[bufferSize];
			Int32 read = 0;

			using (SqlBinaryFieldStream writerStream = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
			{
				using (BinaryWriter writer = new BinaryWriter(writerStream))
				{
					using (FileStream readerStream = new FileStream(fileName, FileMode.Open))
					{
						using (BinaryReader reader = new BinaryReader(readerStream))
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
		}

		void WriteTextStreamUsingStreamWriterWithoutCache(string fileName)
		{
			char[] buffer = new char[bufferSize];
			Int32 read = 0;

			using (SqlBinaryFieldStream writerStream = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary, false))
			{
				using (StreamWriter writer = new StreamWriter(writerStream))
				{
					using (FileStream readerStream = new FileStream(fileName, FileMode.Open))
					{
						using (StreamReader reader = new StreamReader(readerStream))
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
		}

		void WriteTextStreamUsingXmlWriter(string fileName)
		{
			using (SqlBinaryFieldStream writerStream = SqlBinaryFieldStream.OpenWriter(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
			{
				using (XmlWriter writer = XmlWriter.Create(writerStream))
				{
					using (FileStream readerStream = new FileStream(fileName, FileMode.Open))
					{
						using (XmlReader reader = XmlReader.Create(readerStream))
						{
							SqlStreamTestHelper.CopyXmlStream(reader, writer);
						}
					}
				}
			}
		}

		void ReadBinaryStream(string outputFilePath, string columnName = testDataColumnName)
		{
			Byte[] buffer = new Byte[bufferSize];
			Int32 read = 0;

			using (FileStream writerStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (BinaryWriter writer = new BinaryWriter(writerStream))
				{
					using (SqlBinaryFieldStream reader = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, columnName, SqlDbType.VarBinary))
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

		void ReadBinaryStreamUsingBinaryReader(string outputFilePath)
		{
			Byte[] buffer = new Byte[bufferSize];
			Int32 read = 0;

			using (FileStream writerStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (BinaryWriter writer = new BinaryWriter(writerStream))
				{
					using (SqlBinaryFieldStream readerStream = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
					{
						using (BinaryReader reader = new BinaryReader(readerStream))
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
		}

		void ReadTextStreamUsingStreamReaderWithoutCache(string outputFilePath)
		{
			char[] buffer = new char[bufferSize];
			Int32 read = 0;

			using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					using (SqlBinaryFieldStream readerStream = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary, bufferSize / 2))
					{
						using (StreamReader reader = new StreamReader(readerStream))
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
		}

		void ReadTextStreamUsingXmlReader(string outputFilePath)
		{
			using (FileStream fileStream = new FileStream(outputFilePath, FileMode.Create))
			{
				using (XmlWriter writer = XmlWriter.Create(fileStream))
				{
					using (SqlBinaryFieldStream readerStream = SqlBinaryFieldStream.OpenReader(testConnection, testTableName, testPKColumnName, testPKValue, testDataColumnName, SqlDbType.VarBinary))
					{
						using (XmlReader reader = XmlReader.Create(readerStream))
						{
							SqlStreamTestHelper.CopyXmlStream(reader, writer);
						}
					}
				}
			}
		}

		#endregion

		#region Setup Data

		void CreateTableData(string columnName = testDataColumnName)
		{
			string sqlText = string.Format(@"
					CREATE TABLE {0} (
						{1} uniqueidentifier not null,
						{2} varbinary(MAX) NULL
						)", testTableName, testPKColumnName, columnName);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void InsertTestData()
		{
			string sqlText = string.Format(@"INSERT {0} SELECT '{1}', NULL ", testTableName, testPKValue);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		#endregion
	}
}
