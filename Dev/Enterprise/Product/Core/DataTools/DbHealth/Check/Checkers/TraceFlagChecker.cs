using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	class TraceFlagChecker : IChecker
	{
		#region IChecker Memebers
		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			CheckTraceFlags(connection, warningList);
		}

		string IChecker.Description
		{
			get { return "Check required trace flags are enabled on the SQL Server"; }
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal virtual List<TraceFlag> GetTraceFlags(DbConnection connection)
		{
			var tfs = new List<TraceFlag>();
			using (var reader = connection.Command("dbcc tracestatus").ExecuteReader())
			{
				while (reader.Read())
				{
					tfs.Add(new TraceFlag(
						(Int16)reader[0],
						(Int16)reader[1],
						(Int16)reader[2],
						(Int16)reader[3]
						));
				}
			}

			return tfs;
		}

		internal void CheckTraceFlags(DbConnection connection, DbHealthWarningList warningList)
		{
			var traceFlagsToCheck = new List<TraceFlag>();
			traceFlagsToCheck.Add(new TraceFlag(1448, true));
			traceFlagsToCheck.Add(new TraceFlag(4199, true));
			traceFlagsToCheck.Add(new TraceFlag(15006, true));

			// See https://learn.microsoft.com/en-us/sql/t-sql/database-console-commands/dbcc-traceon-trace-flags-transact-sql?view=sql-server-ver16#tf12502
			// Tf12502 was introduced in https://learn.microsoft.com/en-us/troubleshoot/sql/releases/sqlserver-2022/cumulativeupdate5#2351584
			// See https://learn.microsoft.com/en-us/troubleshoot/sql/releases/sqlserver-2022/build-versions for full SQL Server version list
			var sql22Cu5 = new SqlServerVersionNumber("16.0.4045.3");
			if (connection.ServerVersionNumber.CompareTo(sql22Cu5) >= 0)
			{
				traceFlagsToCheck.Add(new TraceFlag(12502, true));
			}

			var traceFlags = GetTraceFlags(connection);

			foreach (var tf in traceFlagsToCheck)
			{
				CheckTraceFlag(tf, traceFlags, warningList);
			}
		}

		void CheckTraceFlag(TraceFlag tf, List<TraceFlag> traceFlags, DbHealthWarningList warningList)
		{
			var existingTF = traceFlags.Find(x => x.TFNumber == tf.TFNumber);

			if (
				(tf.IsEnabledGlobally && existingTF == null) ||
				((tf.IsEnabledGlobally && existingTF != null) && existingTF.IsEnabledGlobally != tf.IsEnabledGlobally) ||
				(!tf.IsEnabledGlobally && existingTF != null)
				)
			{
				var warningDesc = tf.IsEnabledGlobally ?
					"Trace Flag {0} is not enabled on the SQL Server" :
					"Trace Flag {0} is enabled on the SQL Server";

				var warningAction = tf.IsEnabledGlobally ?
					"Enable this trace flag" :
					"Disable this trace flag";

				var warning = new ServerWarning(
					Db.Connection.ServerNameReportedByDatabase
					, ServerWarning.ServerTraceFlagsWarning
					, String.Format(CultureInfo.InvariantCulture, warningDesc, tf.TFNumber)
					, warningAction);
				warningList.Add(warning);
			}
		}
	}

	class TraceFlag
	{
		public TraceFlag(int tfNumber, bool isEnabledGlobally)
		{
			this.TFNumber = tfNumber;
			this.IsEnabledGlobally = isEnabledGlobally;
		}

		public TraceFlag(int tfNumber, int tfStatus, int tfGlobal, int tfSession)
		{
			this.TFNumber = tfNumber;
			this.IsEnabledGlobally =
				(tfStatus == 1 &&
				tfGlobal == 1 &&
				tfSession == 0);
		}

		public int TFNumber { get; private set; }
		public bool IsEnabledGlobally { get; private set; }
	}
}
