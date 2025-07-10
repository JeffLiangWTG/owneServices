using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	class GhostRecordsChecker : IChecker
	{
		#region IChecker Memebers

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			try
			{
				CheckGhostRecords(connection, warningList);
			}
			catch (SqlException ex)
			{
				ErrorReporter.ReportOnce("GhostRecordsChecker.SqlException", "SQL Exception happened in GhostRecordsChecker", ex);
			}
		}

		string IChecker.Description
		{
			get { return "Ghost Records Cleanup Process Checker"; }
		}

		#endregion

		#region Implementation

		void CheckGhostRecords(DbConnection connection, DbHealthWarningList warningList)
		{
			connection.ExecuteNonQuery(Invariant($@"
if not exists(select name from sys.tables where name = '{TableName}')
begin
	create table {TableName} (data varchar(max));
end;"));

			int ghostRecordsCount = GetGhostRecordsCount(connection);
			if (ghostRecordsCount > 0)
			{
				Thread.Sleep(15000);
				ghostRecordsCount = GetGhostRecordsCount(connection);
			}
			if (ghostRecordsCount > 0)
			{
				DatabaseWarning warning = new DatabaseWarning(
					connection.ServerNameReportedByDatabase,
					DatabaseWarning.GhostRecordsWarning,
					"Ghost records were identified in database tables. It may indicate that Ms Sql Server Ghost Records Cleanup Process is not running correctly.",
					"Restart Sql Server when possible. This will restart the Ghost Records Cleanup Process.");
				warningList.Add(warning);
			}

			if (ghostRecordsCount == 0)
			{
				// Prepare ghost records for next run
				connection.ExecuteNonQuery(Invariant($@"
insert into {TableName} values (replicate('x', 12000))
insert into {TableName} values (replicate('x', 12000))
insert into {TableName} values (replicate('x', 12000))
insert into {TableName} values (replicate('x', 12000))
insert into {TableName} values (replicate('x', 12000))
update {TableName} set data = data + data + data + data
delete from {TableName}"));
			}
		}

		int GetGhostRecordsCount(DbConnection connection)
		{
			object ghostRecords = connection.ExecuteScalar(Invariant($@"
declare @ObjId int = object_id('{TableName}');
declare @DbId int = DB_ID();
select count(1)
from sys.dm_db_index_physical_stats(@DbId, @ObjId, NULL, NULL, 'DETAILED')
where ghost_record_count > 0"));

			if (ghostRecords != null && ghostRecords != DBNull.Value)
			{
				int ghostRecordsCount;
				if (int.TryParse(ghostRecords.ToString(), out ghostRecordsCount) && ghostRecordsCount > 0)
				{
					return ghostRecordsCount;
				}
			}

			return 0;
		}

		const string TableName = "GhostRecordCleanChecker";

		#endregion
	}
}
