using System;
using System.Data;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.Statistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.Statistics.Testing
{
	[TestedType(typeof(StatisticsServiceSummarise))]
	class StatisticsServiceSummariseTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestConvertDecimalCorrectly_IncSec_WithPrecision12AndScale3()
		{
			var usageActions = GenerateUsageActions(incSec: 123456789.123m);
			InsertStmUsageWithActions(usageActions);
			ExecuteStatisticsServiceSummarise();
		}

		[ExpectNoExceptions]
		public void TestConvertDecimalCorrectly_ExcSec_WithPrecision12AndScale3()
		{
			var usageActions = GenerateUsageActions(excSec: 123456789.123m);
			InsertStmUsageWithActions(usageActions);
			ExecuteStatisticsServiceSummarise();
		}

		string GenerateUsageActions(decimal incSec = 0, decimal excSec = 0)
		{
			return $@"
<Usages>
	<Usage>
	    <N>ActionName</N>
	    <SN>SubActionName</SN>
	    <SDT>2023-01-01T00:00:00</SDT>
	    <EDT>2023-01-01T01:00:00</EDT>
	    <IncSec>{incSec}</IncSec>
	    <ExcSec>{excSec}</ExcSec>
	    <AC>1</AC>
	</Usage>
</Usages>
";
		}

		void InsertStmUsageWithActions(string usageActions)
		{
			var sql = @"
INSERT dbo.StmUsage (XW_PK, XW_MachineName, XW_CompanyCode, XW_GB_NKBranchCode, XW_GS_NKStaffCode, XW_ExeVersion, XW_StartTimeUtc, XW_EndTimeUtc, XW_UsageActions)
SELECT @XW_PK, @XW_MachineName, @XW_CompanyCode, @XW_GB_NKBranchCode, @XW_GS_NKStaffCode, @XW_ExeVersion, @XW_StartTimeUtc, @XW_EndTimeUtc, @XW_UsageActions
;
";

			using var cmd = TestConnection.Command(sql);
			cmd.AddParameter("@XW_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
			cmd.AddParameter("@XW_MachineName", SqlDbType.VarChar, Environment.MachineName);
			cmd.AddParameter("@XW_CompanyCode", SqlDbType.VarChar, "EDI");
			cmd.AddParameter("@XW_GB_NKBranchCode", SqlDbType.VarChar, "BNE");
			cmd.AddParameter("@XW_GS_NKStaffCode", SqlDbType.VarChar, "E");
			cmd.AddParameter("@XW_ExeVersion", SqlDbType.VarChar, "1.1.1.1");
			cmd.AddParameter("@XW_StartTimeUtc", SqlDbType.DateTime, DateTime.UtcNow);
			cmd.AddParameter("@XW_EndTimeUtc", SqlDbType.DateTime, DateTime.UtcNow);
			cmd.AddParameter("@XW_UsageActions", SqlDbType.VarBinary, Encoding.UTF8.GetBytes(usageActions));

			_ = cmd.ExecuteNonQuery();
		}

		void ExecuteStatisticsServiceSummarise()
		{
			using var cmd = TestConnection.Command(@"StatisticsServiceSummarise");
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.AddParameter("@TopRowCount", SqlDbType.BigInt, 1000);
			_ = cmd.ExecuteScalar();
		}
	}
}
