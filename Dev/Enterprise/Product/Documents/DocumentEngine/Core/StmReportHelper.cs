using System;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:DoNotUseDbConnectionMethods", Justification = "Necessary")]
	public static class StmReportHelper
	{
		public static int SQLCPUTime(DbConnection connection)
		{
			using (connection.UseMasterDb())
			{
				var sql = "select CPU_Time from sys.dm_exec_sessions where session_id = @@SPID;";
				return connection.ExecuteScalar<int>(sql);
			}
		}
	}

	public class ReportStatistics
	{
		public int StartSQLCPU { get; set; }
		public int EndSQLCPU { get; set; }
		public TimeSpan StartCPU { get; set; }
		public ZDateTime Start { get; set; }
	}
}
