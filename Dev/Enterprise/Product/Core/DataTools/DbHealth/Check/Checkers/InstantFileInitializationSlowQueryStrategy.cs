using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	using static FormattableString;

	/// <summary>
	/// A strategy that checks for Instant File Intitialization via a test for functionality 
	/// followed by a read of server logs.
	/// </summary>
	class InstantFileInitializationSlowQueryStrategy : IInstantFileInitializationStrategy
	{
		readonly string query;
		readonly Func<DbConnection, int> getSpid;

		public InstantFileInitializationSlowQueryStrategy()
			: this(GetDefaultQuery(), conn => conn.SPID)
		{
		}

		internal InstantFileInitializationSlowQueryStrategy(string query, Func<DbConnection, int> getSpid)
		{
			this.query = query;
			this.getSpid = getSpid;
		}

		public bool? IsInstantFileInitializationEnabled(ILogger logger)
		{
			var logRecords = CheckSqlInstanceAndGetErrorLogRecords(logger);

			return
				logRecords.Any(x => x.StartsWith("Zeroing completed on ", StringComparison.OrdinalIgnoreCase)) &&
				logRecords.Any(x => x.StartsWith("FixupLogTail(progress) zeroing ", StringComparison.OrdinalIgnoreCase)) &&
				logRecords.Any(x => x.Equals("Starting up database '" + Db.DatabaseName + "_DbChecker" + "'.", StringComparison.OrdinalIgnoreCase)) &&
				logRecords.Any(x => x.Equals("Setting database option DISABLE_BROKER to ON for database '" + Db.DatabaseName + "_DbChecker" + "'.", StringComparison.OrdinalIgnoreCase));
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		List<string> CheckSqlInstanceAndGetErrorLogRecords(ILogger logger)
		{
			var result = new List<string>();

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var spid = "spid" + getSpid(connection);

				using (var reader = connection.Command(query).ExecuteReader())
				{
					try
					{
						while (reader.Read())
						{
							if (reader.GetString(1) == spid)
							{
								var logText = reader.GetString(2);
								if (!logText.StartsWith("DBCC TRACEON 3004", StringComparison.OrdinalIgnoreCase) &&
									!logText.StartsWith("DBCC TRACEON 3605", StringComparison.OrdinalIgnoreCase) &&
									!logText.StartsWith("DBCC TRACEOFF 3004", StringComparison.OrdinalIgnoreCase) &&
									!logText.StartsWith("DBCC TRACEOFF 3605", StringComparison.OrdinalIgnoreCase)
									)
								{
									result.Add(logText);
								}
							}
						}
					}
					catch (IndexOutOfRangeException ex)
					{
						logger.Log(LogType.Error, "Failed to read SQL Server error log.", ex);
					}
				}
			}
			return result;
		}

		static string GetDefaultQuery()
		{
			var dbCheckerName = Db.DatabaseName + "_DbChecker";
			return Invariant($@"
If EXISTS(Select null From sys.databases Where name = '{dbCheckerName}') 
Drop Database [{dbCheckerName}]
DBCC TRACEON(3004,3605)
Declare @dtFrom datetime = DATEADD(SECOND, -1, GETDATE())
Declare @dbPath nvarchar(max) = (Select top 1 physical_name From sys.master_files Where database_id = DB_ID('{Db.DatabaseName}') and type = 0) 

Set @dbPath = LEFT(@dbPath, len(@dbPath) - CHARINDEX('\', REVERSE(@dbPath)))
Declare @fileSize int = (Select size * 8 From sys.master_files Where database_id = DB_ID('model') AND type = 0) 

Declare @sql varchar(max) = 'CREATE DATABASE [{dbCheckerName}]
ON PRIMARY (NAME = N''{dbCheckerName}_Data'', FILENAME = ''' + @dbPath + '\{dbCheckerName}.mdf'', SIZE = ' + CAST(@fileSize as varchar(15)) + 'KB)
LOG ON (NAME = N''{dbCheckerName}_log'', FILENAME = ''' + @dbPath + '\{dbCheckerName}.ldf'' , SIZE = ' + CAST(@fileSize as varchar(15)) + 'KB)
'
EXEC(@sql)

Set @sql = 'ALTER DATABASE [{dbCheckerName}] SET DISABLE_BROKER'
EXEC(@sql)

DROP DATABASE [{dbCheckerName}]
DBCC TRACEOFF(3004,3605)

Declare @dtTo datetime = getdate()

EXEC master.dbo.xp_readerrorlog 0, 1, null, null, @dtFrom, @dtTo, N'ASC' ");
		}
	}
}
