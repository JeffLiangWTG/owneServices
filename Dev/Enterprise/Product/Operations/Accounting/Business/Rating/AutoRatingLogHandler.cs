using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	class AutoRatingLogHandler : IAutoRatingLogHandler
	{
		#region CreateLogs

		const string LogParentPK = nameof(LogParentPK);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void CreateLogs()
		{
			if (CARLogsToCreate.Count > 0)
			{
				var logsParentsWithNonCancelledAutoRatingLogInDB = new HashSet<ZGuid>();

				using (var command = Db.Connection.Command(SQL))
				{
					command.AddTableValuedParameter("@LogParents", "dbo.TVP_AutoRatingCARLog", GetCARLogDataTable());
					command.AddParameterBasedOnDbColumn("@EventType", Events.ChargesHaveBeenAutoRatedCode, StmALogSchema.SL_SE_NKEvent);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							logsParentsWithNonCancelledAutoRatingLogInDB.Add((Guid)reader[LogParentPK]);
						}
					}
				}

				foreach (var (logsParent, eventInfo) in CARLogsToCreate)
				{
					if (logsParentsWithNonCancelledAutoRatingLogInDB.Contains(logsParent.LogsParentPK))
					{
						logsParent.Logs.CreateOrRecreateEventLog(eventInfo);
					}
					else
					{
						// no log in DB, just check in memory
						logsParent.Logs.CreateOrRecreateEventLog(eventInfo);
					}
				}
			}
		}

		static string SQL => @"
SELECT
	LogParentPK
FROM
	@LogParents
WHERE
	EXISTS
	(
		SELECT NULL
		FROM dbo.StmALog
		WHERE
			SL_Parent = LogParentPK
			AND SL_SE_NKEvent = @EventType
			AND SL_IsEstimate = 'N'
			AND SL_IsCancelled = 'N'
			AND SL_Reference = LogReference
	)";

		DataTable GetCARLogDataTable()
		{
			var dataTable = new DataTable("@LogParents");
			dataTable.Columns.Add(LogParentPK, typeof(Guid));
			dataTable.Columns.Add("LogReference", typeof(string));
			dataTable.Locale = CultureInfo.InvariantCulture;

			foreach (var (logParent, eventInfo) in CARLogsToCreate)
			{
				dataTable.Rows.Add(new object[]
				{
					logParent.LogsParentPK.ToGuid(),
					eventInfo.Reference
				});
			}

			return dataTable;
		}

		#endregion

		#region QueueCARLogToCreateOnceAutoRatingFinished

		void IAutoRatingLogHandler.QueueCARLogToCreateOnceAutoRatingFinished(IStmALogParent logsParent, ZDateTime eventTime, string reference)
		{
			var eventInfo = new EventValue(Events.ChargesHaveBeenAutoRated, isEstimate: false, eventTime: eventTime.ToOffset(), reference: reference);
			CARLogsToCreate.Add((logsParent, eventInfo));
		}

		List<(IStmALogParent LogParent, EventValue EventInfo)> CARLogsToCreate { get; } = new List<(IStmALogParent LogParent, EventValue EventInfo)>();

		#endregion
	}
}
