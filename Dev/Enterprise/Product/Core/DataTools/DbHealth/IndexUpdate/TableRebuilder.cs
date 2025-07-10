using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate
{
	/// <summary>
	/// Rebuilds Tables ONLINE (Enterprise Edition only) that had one or more columns dropped in the last schema upgrade.
	/// For other SQL Server editions the table is rebuilt in offline mode during the actual upgrade process.
	/// </summary>
	class TableRebuilder
	{
		public TableRebuilder(ILogger logger)
		{
			this.logger = logger;
			registrySettings = new IndexUpdateSettings();
		}

		readonly IndexUpdateSettings registrySettings;

		public void Run(AdminConnection connection)
		{
			var persister = new TableRebuildPersister(connection);
			foreach (var table in persister.GetTablesToRebuild())
			{
				try
				{
					logger.Information(Invariant($"Rebuilding table {table}"));
					RebuildTable(connection, table);
					persister.MarkTableAsNotRequiringRebuild(table);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					logger.Error(Invariant($"Error rebuilding table: {table}"), ex);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RebuildTable(AdminConnection connection, DbSchemaTable tableToRebuild)
		{
			var sql = Invariant($@"ALTER TABLE {tableToRebuild} REBUILD WITH ({Invariant($@"MAXDOP={registrySettings.Rebuild_MaxDop}, ONLINE=ON (WAIT_AT_LOW_PRIORITY (MAX_DURATION={registrySettings.Rebuild_MaxWaitInMinutes} MINUTES, ABORT_AFTER_WAIT={registrySettings.Rebuild_AbortAfterWait}))")});");

			_ = connection.ExecuteNonQuery(sql, DbCommand.Timeout.Infinite);
		}

		readonly ILogger logger;
	}
}
