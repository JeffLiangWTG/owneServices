using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.Audit
{
	[TestedType(typeof(usp_GetDeletedDataBetweenLSN))]
	class usp_GetDeletedDataBetweenLSNTest : BiCreateScriptTest
	{
		public void TestGetDeletedDataBetweenLSN()
		{
			using (var conn = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.AuditDatabaseName))
			{
				try
				{
					conn.BeginTransaction();

					conn.ExecuteNonQuery("INSERT INTO [biadmin].[LsnTimeMapping] (StartLsn, TranEndTimeUtc) VALUES (0x01, '2095-10-1')");

					var expectedPK = Guid.NewGuid();
					var deferredUpdatePK = Guid.NewGuid();
					conn.ExecuteNonQuery($@"INSERT [dbo].[GlbStaff]
					(
						[__$lsn_period], [__$command_id], [__$start_lsn], [__$seqval], [__$operation], [__$update_mask], [GS_PK], [GS_Code]
					) VALUES
					( 9412, 1, 0x02, 0x01, 1, 0x0, NEWID(), 'A' ),
					( 9512, 1, 0x02, 0x02, 1, 0x0, '{expectedPK}', 'B' ),
					( 9512, 1, 0x03, 0x03, 2, 0x0, NEWID(), 'C' ),
					( 9512, 1, 0x04, 0x04, 3, 0x0, NEWID(), 'D' ),
					( 9512, 1, 0x05, 0x05, 4, 0x0, NEWID(), 'E' ),
					( 9512, 1, 0x05, 0x06, 1, 0x0, '{deferredUpdatePK}', 'F' ),
					( 9512, 1, 0x05, 0x07, 2, 0x0, '{deferredUpdatePK}', 'G' ),
					( 9512, 1, 0x06, 0x08, 1, 0x0, NEWID(), 'H' )");

					var sqlText = "EXEC [biadmin].[usp_GetDeletedDataBetweenLSN] 'dbo', 'GlbStaff', 'GS_PK', 0x01, 0x05";
					var dataTable = DataUtils.GetDataTableFromQuery(conn, sqlText);

					AssertEquals("Result should have 1 row", 1, dataTable.Rows.Count);
					AssertEquals(expectedPK, dataTable.Rows[0][0]);
				}
				finally
				{
					conn.RollbackTransaction();
				}
			}
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}
