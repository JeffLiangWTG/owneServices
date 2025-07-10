using System;
using System.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	sealed class SqlDataReaderExtensionsTest : TestCase
	{
		public void TestHasColumn()
		{
			using (var command = Db.Connection.Command("SELECT Z0_PK, Z0_Code FROM dbo.DummyBizo"))
			using (var reader = command.ExecuteReader())
			{
				Assert("Should have column 'Z0_PK'", reader.HasColumn("Z0_PK"));
				Assert("Should have column 'Z0_Code'", reader.HasColumn("Z0_Code"));
				Assert("Should not have column 'Z0_Description'", !reader.HasColumn("Z0_Description"));
			}
		}

		[UseSnapshotProtection]
		public void TestReadAllBytes()
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command($"INSERT INTO dbo.DummyBizo(Z0_PK, Z0_VarBinaryMax, Z0_VarCharMax) VALUES('{pk}', 0x123456, 'ABCDEF')"))
			{
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command($"SELECT Z0_PK, Z0_VarBinaryMax, Z0_VarCharMax FROM dbo.DummyBizo WHERE Z0_PK = @PK"))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader.ReadAllBytes(1);
						AssertEquals("bytes Length of binary", 3, bytes.Length);
						AssertEquals("binary bytes[0]", (short)0x12, bytes[0]);
						AssertEquals("binary bytes[1]", (short)0x34, bytes[1]);
						AssertEquals("binary bytes[2]", (short)0x56, bytes[2]);

						bytes = reader.ReadAllBytes(2);
						AssertEquals("bytes Length of string", 6, bytes.Length);
						AssertEquals("string bytes[0]", (short)'A', bytes[0]);
						AssertEquals("string bytes[1]", (short)'B', bytes[1]);
						AssertEquals("string bytes[2]", (short)'C', bytes[2]);
						AssertEquals("string bytes[3]", (short)'D', bytes[3]);
						AssertEquals("string bytes[4]", (short)'E', bytes[4]);
						AssertEquals("string bytes[5]", (short)'F', bytes[5]);
					}
				}
			}
		}
	}
}
