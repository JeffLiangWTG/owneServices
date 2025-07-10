using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.Core.Subscribers
{
	public class StaffDeactivatedSubscriber : ActualDataChangesAuditSubscriber
	{
		public override bool NotifyInsert => false;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "KDS";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "Kill database connection if staff is deactivated.";

		public override bool IsRequired()
		{
			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			for (var i = 0; i < 3; i++)
			{
				try
				{
					var statement = GetCommandStatement(changeTable);

					if (!string.IsNullOrWhiteSpace(statement))
					{
						using (var adminConnection = Db.NewAdminConnection())
						{
							adminConnection.ExecuteNonQuery(statement);
						}
					}

					break;
				}
				catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.NotAnActiveProcessId)
				{
					logger.Error($"Tried to kill the connection but the SPID is not an active process ID at {i + 1} attempts.");
					if (i == 2)
					{
						ErrorReporter.ReportOnce("Kill connection failed due to the SPID is not an active process ID", e);
					}
				}
			}
		}

		public string GetCommandStatement(DataTable changeTable)
		{
			var killCommands = new StringBuilder();

			var deactivatedStaffPks = new HashSet<Guid>();
			foreach (var row in changeTable.Rows.Cast<DataRow>())
			{
				if (!(bool)row[GlbStaffSchema.Constants.GS_IsActive])
				{
					deactivatedStaffPks.Add((Guid)row[GlbStaffSchema.Constants.PK]);
				}
			}

			if (deactivatedStaffPks.Any())
			{
				killCommands.AppendLine(@$"DELETE FROM dbo.StmServiceHeartBeat WHERE SV_ParentId IN ({string.Join(",", deactivatedStaffPks.Select(pk => $"'{pk.ToString()}'"))}) AND SV_ParentTableCode = 'GS'");

				using (var adminConnection = Db.NewAdminConnection())
				{
					var query = $@"
SELECT 'KILL ' + CONVERT(varchar, sp.session_id) AS cmd FROM sys.dm_exec_sessions sp 
LEFT JOIN dbo.StmServiceHeartBeat hb ON hb.SV_ProcessID = sp.host_process_id                                
WHERE hb.SV_ParentId IN (SELECT Value FROM @StaffPks) AND hb.SV_ParentTableCode = '{GlbStaffSchema.Constants.Prefix}' AND hb.SV_ProcessID <> 0 AND sp.session_id <> @@SPID";
					using (var cmd = adminConnection.Command(query))
					{
						cmd.AddTableValuedParameter("@StaffPks", "dbo.TVP_uniqueidentifier", deactivatedStaffPks);
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								killCommands.AppendLine(reader.GetString(0));
							}
						}
					}
				}
			}

			return killCommands.ToString();
		}

		public override ITableSchema Table => GlbStaffSchema.Instance;
		public override IEnumerable<SchemaColumn> SpecificColumns => new List<SchemaColumn>() { GlbStaffSchema.GS_IsActive };
	}
}
