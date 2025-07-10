using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.MainDb
{
	[TestedType(typeof(CdcGetNetChanges))]
	class CdcGetNetChangesTest : DbCreateScriptTest
	{
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestUpdateMaskForLargeColumnNumber()
		{
			CdcDatabase.Enable((AdminConnection)TestConnection, Db.DatabaseName);
			CreateTestTable();
			AddTestData();
			var cdcTable = new CdcTable("dbo", "Test_CDC");
			cdcTable.EnableCdc(TestConnection);
			UpdateTestData();

			using (var cmd = TestConnection.Command("CdcGetNetChanges"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@from_lsn", SqlDbType.Binary, BitConverter.GetBytes(0x0));
				cmd.AddParameter("@to_lsn", SqlDbType.Binary, BitConverter.GetBytes(0xFFFFFFFFFFFFFFFF));
				cmd.AddParameter("@table_name", SqlDbType.VarChar, "Test_CDC");
				cmd.AddParameter("@schema_name", SqlDbType.VarChar, "dbo");
				cmd.AddParameter("@pk_name", SqlDbType.VarChar, "RecordID");
				cmd.AddParameter("@col_list", SqlDbType.VarChar, "RecordID,c1,c2,c3,c4,c5,c6,c7,c8,c9,c10,c11,c12,c13,c14,c15,c16,c17,c18,c19,c20,c21,c22,c23,c24,c25,c26,c27,c28,c29,c30,c31,c32,c33,c34,c35,c36,c37,c38,c39,c40,c41,c42,c43,c44,c45,c46,c47,c48,c49,c50,c51,c52,c53,c54,c55,c56,c57,c58,c59,c60,c61,c62,c63,c64,c65,c66,c67,c68,c69,c70,c71,c72,c73,c74,c75,c76,c77,c78,c79,c80,c81,c82,c83,c84,c85,c86,c87,c88,c89,c90,c91,c92,c93,c94,c95,c96,c97,c98,c99,c100,c101,c102,c103,c104,c105,c106,c107,c108,c109,c110,c111,c112,c113,c114,c115,c116,c117,c118,c119,c120,c121,c122,c123,c124,c125,c126,c127");
				using (var reader = cmd.ExecuteReader())
				{
					var rowCount = 0;
					while (reader.Read())
					{
						rowCount++;
						var c63Value = Convert.ToInt32(reader["c63"]);
						AssertEquals(999999, c63Value);
					}
					AssertEquals("CdcGetNetChanges should return 1 row", 1, rowCount);
				}
			}
		}

		void CreateTestTable()
		{
			TestConnection.ExecuteNonQuery(@"
CREATE TABLE dbo.Test_CDC
(
RecordID UNIQUEIDENTIFIER NOT NULL,
c1 int,c2 int,c3 int,c4 int,c5 int,c6 int,c7 int,c8 int,c9 int,c10 int,c11 int,c12 int,c13 int,c14 int,c15 int,c16 int,c17 int,c18 int,c19 int,c20 int,c21 int,c22 int,c23 int,c24 int,c25 int,c26 int,c27 int,c28 int,c29 int,c30 int,c31 int,c32 int,c33 int,c34 int,c35 int,c36 int,c37 int,c38 int,c39 int,c40 int,c41 int,c42 int,c43 int,c44 int,c45 int,c46 int,c47 int,c48 int,c49 int,c50 int,c51 int,c52 int,c53 int,c54 int,c55 int,c56 int,c57 int,c58 int,c59 int,c60 int,c61 int,c62 int,c63 int,c64 int,c65 int,c66 int,c67 int,c68 int,c69 int,c70 int,c71 int,c72 int,c73 int,c74 int,c75 int,c76 int,c77 int,c78 int,c79 int,c80 int,c81 int,c82 int,c83 int,c84 int,c85 int,c86 int,c87 int,c88 int,c89 int,c90 int,c91 int,c92 int,c93 int,c94 int,c95 int,c96 int,c97 int,c98 int,c99 int,c100 int,c101 int,c102 int,c103 int,c104 int,c105 int,c106 int,c107 int,c108 int,c109 int,c110 int,c111 int,c112 int,c113 int,c114 int,c115 int,c116 int,c117 int,c118 int,c119 int,c120 int,c121 int,c122 int,c123 int,c124 int,c125 int,c126 int,c127 int
)");
			TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.Test_CDC
ADD CONSTRAINT PK_RECORDID PRIMARY KEY NONCLUSTERED (RecordID)");
		}

		void AddTestData()
		{
			var id = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@$"
insert into dbo.Test_CDC (RecordID, C1)
values('{id}', 0)");
		}

		void UpdateTestData()
		{
			TestConnection.ExecuteNonQuery(@"
update dbo.Test_CDC
set c63 = 999999");

			TestConnection.ExecuteNonQuery("exec sys.sp_cdc_scan");
		}

		DbConnection testConnection;
		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection());
			}
		}
	}
}
