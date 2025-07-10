using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZTextReaderNoTransactionTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestWithReconnect()
		{
			AddRow("Hello");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AdoTestUtils.KillConnection(connectionInfo.DbConnection);
					AssertEquals(textColumn.Name, "Hello", reader.ReadToEnd());
				}
			}
		}

		void AddRow(string text)
		{
			ZTextReaderTest.AddRow(text, pk);
		}

		protected override void SetUp()
		{
			connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			pk = ZGuid.NewZGuid();
		}

		ZSqlConnectionInfo connectionInfo;
		ZGuid pk;
	}

	sealed class ZTextReaderTest : TransactionedTestCase
	{
		public void TestMissingPK()
		{
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (TextReader reader = new ZTextReader(connectionInfo, textColumn, ZGuid.NewZGuid()))
				{
					AssertEquals(textColumn.Name, 0, reader.ReadToEnd().Length);
				}
			}
		}

		public void TestEmptyString()
		{
			AddRow("");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (TextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AssertEquals(textColumn.Name, 0, reader.ReadToEnd().Length);
				}
			}
		}

		public void TestReadBytes()
		{
			AddRow("Hello");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					char[] buffer = new char[3];
					AssertEquals(textColumn.Name, 3, reader.Read(buffer, 0, 3));
					AssertEquals(textColumn.Name, 'H', buffer[0]);
					AssertEquals(textColumn.Name, 'e', buffer[1]);
					AssertEquals(textColumn.Name, 'l', buffer[2]);
					AssertEquals(textColumn.Name, 2, reader.Read(buffer, 0, 3));
					AssertEquals(textColumn.Name, 'l', buffer[0]);
					AssertEquals(textColumn.Name, 'o', buffer[1]);
					AssertEquals(textColumn.Name, 'l', buffer[2]); // buffer should still have old data I think
				}
			}
		}

		public void TestSmallString()
		{
			AddRow("Hello");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AssertEquals(textColumn.Name, "Hello", reader.ReadToEnd());
				}
			}
		}

		public void TestSmallStringWithTrailingBlanks()
		{
			AddRow("Hello ");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AssertEquals(textColumn.Name, "Hello ", reader.ReadToEnd());
				}
			}
		}

		public void TestReadByteTillEnd()
		{
			AddRow("Hell ");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AssertEquals(textColumn.Name, 'H', reader.Read());
					AssertEquals(textColumn.Name, 'e', reader.Read());
					AssertEquals(textColumn.Name, 'l', reader.Read());
					AssertEquals(textColumn.Name, 'l', reader.Read());
					AssertEquals(textColumn.Name, ' ', reader.Read());
					AssertEquals(textColumn.Name, -1, reader.Read());
				}
			}
		}

		public void TestLargeString()
		{
			string largeString = new string('*', 1000000);
			AddRow(largeString);
			using (ZTextReader reader = new ZTextReader(connectionInfo, DummyBizoSchema.Z0_VarCharMax, pk))
			{
				AssertEquals(largeString, reader.ReadToEnd());
			}
			using (ZTextReader reader = new ZTextReader(connectionInfo, DummyBizoSchema.Z0_NVarCharMax, pk))
			{
				AssertEquals(largeString, reader.ReadToEnd());
			}
			using (ZTextReader reader = new ZTextReader(connectionInfo, DummyBizoSchema.Z0_Description, pk))
			{
				AssertEquals(largeString.Substring(0, DummyBizoSchema.Z0_Description.MaxLength), reader.ReadToEnd());
			}
		}

		public void TestRead_MoreCharactersThanAvailable()
		{
			AddRow("12345678 ");
			foreach (var textColumn in new SchemaStringColumn[] { DummyBizoSchema.Z0_VarCharMax, DummyBizoSchema.Z0_NVarCharMax })
			{
				char[] buffer = new char[100];
				using (ZTextReader reader = new ZTextReader(connectionInfo, textColumn, pk))
				{
					AssertEquals(textColumn.Name, 9, reader.Read(buffer, 0, 11));
					AssertEquals(textColumn.Name, "12345678 ", new string(buffer, 0, 9));
				}
			}
		}

		internal static void AddRow(string text, ZGuid pk)
		{
			string sql = string.Format("INSERT INTO dbo.DummyBizO (Z0_PK, Z0_VarCharMax, Z0_NVarCharMax, Z0_Description) values ('{0}', '{1}', '{1}', '{2}')",
				pk.ToString(), text,
				text.Length <= DummyBizoSchema.Z0_Description.MaxLength ? text : text.Substring(0, DummyBizoSchema.Z0_Description.MaxLength));
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		void AddRow(string text)
		{
			AddRow(text, pk);
		}

		protected override void SetUp()
		{
			connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
			pk = ZGuid.NewZGuid();
		}

		ZSqlConnectionInfo connectionInfo;
		ZGuid pk;
	}
}
