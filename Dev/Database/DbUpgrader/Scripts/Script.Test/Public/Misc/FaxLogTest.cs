using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(FaxLog))]
	class FaxLogTest : DbCreateScriptTest
	{
		public void TestIncludesNormalFaxEntry()
		{
			CreateDSNLogEntry("+64 (9) 256-0071 - EDN - AKL - TAX INVOICE EDN00011139 RHCHUSAKL (01-Sep-10)");
			using (var reader = Db.Connection.Command(faxLogQuery).ExecuteReader())
			{
				Assert(reader.Read());
				AssertEquals("+64 (9) 256-0071", reader[1]);
				AssertEquals("EDN - AKL - TAX INVOICE EDN00011139 RHCHUSAKL (01-Sep-10)", reader[2]);
			}
		}

		public void TestExcludesNumericEmailEntry()
		{
			CreateDSNLogEntry("256007@fax.service.net - EDN - AKL - TAX INVOICE EDN00011139 RHCHUSAKL (01-Sep-10)");
			using (var reader = Db.Connection.Command(faxLogQuery).ExecuteReader())
			{
				Assert(!reader.Read());
			}
		}

		const string faxLogQuery = "select * from FaxLog(dateadd(hour, -1, getutcdate()), dateadd(hour, 1, getutcdate()))";

		void CreateDSNLogEntry(string reference)
		{
			const string sql = "insert into dbo.StmALog (SL_PK, SL_Parent, SL_Table, SL_Reference, SL_PostedTimeUtc, SL_EventTime, SL_SE_NKEvent) values (newid(), newid(), 'Dummy', @reference, getutcdate(), getdate(), 'DSN')";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("reference", SqlDbType.VarChar, reference);
				cmd.ExecuteNonQuery();
			}
		}
	}
}

