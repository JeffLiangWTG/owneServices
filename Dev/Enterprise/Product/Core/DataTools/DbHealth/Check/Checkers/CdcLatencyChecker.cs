using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check.Checkers
{
	public class CdcLatencyChecker : IChecker
	{
		public string Description => "Check that database scan for change data capture is progressing";

		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CheckCdcLatencyIsLessThan2Days(connection, warningList, logger);
			}
		}

		protected virtual TimeSpan AcceptableCdcLatencyOffset => TimeSpan.FromDays(2);

		void CheckCdcLatencyIsLessThan2Days(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			var lastLsnDateTime = GetLastLsnUtcDateTime(connection);

			if (lastLsnDateTime.IsEmpty)
			{
				var warning = new DatabaseWarning(
					connection.ServerNameReportedByDatabase,
					DatabaseWarning.CdcWarning,
					"Database does not contain any change data capture records.",
					"Check if CDC scan service task is running.");
				warningList.Add(warning);
			}
			else if (ZDateTime.UtcNow - lastLsnDateTime > AcceptableCdcLatencyOffset)
			{
				var warning = new DatabaseWarning(
					connection.ServerNameReportedByDatabase,
					DatabaseWarning.CdcWarning,
					"Database has not scanned any change data capture records for more than 2 days.",
					"Check if CDC scan service task is running.");
				warningList.Add(warning);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Query is to a system table")]
		ZDateTime GetLastLsnUtcDateTime(DbConnection connection)
		{
			var query = "select top 1 DATEADD(HOUR, DATEDIFF(HOUR, GETDATE(), GETUTCDATE()), tran_end_time) from cdc.lsn_time_mapping order by start_lsn desc";

			using (var cmd = connection.Command(query))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					return new ZDateTime(reader.GetDateTime(0));
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}
	}
}
