using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.Audit
{
	internal class BiAuditScriptHelperTest : TransactionedTestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestUpdateMaskForLargeColumnNumber()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				SetupLsnTimeMapping(new byte[] { 0x5 }, new DateTime(2021, 12, 01));
				var column64Bitmap = "0x8000000000000000";
				var column64Name = "GS_GE_LastLogonDepartment";
				var col64Value = "1E2C1BC2-1E57-4312-8A70-05B7B840B053";
				var operationType = 3; 
				var sqlText = @$"
Insert into dbo.GlbStaff (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, __$command_id, {column64Name})
VALUES (0x6, 0x0001BBE70000028B0005, {operationType}, {column64Bitmap}, 2112, 1, '{col64Value}')";
				TestConnection.ExecuteNonQuery(sqlText);
				var command = TestConnection.Command($"Select {column64Name} From biadmin.udf_GetDataBetweenLSN_dbo_GlbStaff ({operationType}, 0x5, 0x7, convert(bigint, {column64Bitmap}), 0, 0, 0)");
				var results = DataUtils.GetListOfValuesFromCommand(command);
				AssertEquals("Query should return a result for the changed column", 1, results.Count());
				AssertEquals(col64Value.ToUpper(), results.ElementAt(0).ToUpper());
			}
		}

		void SetupLsnTimeMapping(byte[] startLsn, DateTime tranEndTimeUtc)
		{
			var sqlText = "INSERT INTO [biadmin].[LsnTimeMapping] VALUES(@startLsn, @tranEndTimeUtc)";
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.AddParameter("@startLsn", SqlDbType.Binary, startLsn);
				cmd.AddParameter("@tranEndTimeUtc", SqlDbType.DateTime, tranEndTimeUtc);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
