using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DatabaseWaiter : IDatabaseWaiter
	{
		void WaitUntilDatabaseReady(DbConnection connection, string dbName, Logger logger, CancellationToken token, List<string> validStates)
		{
			logger.LogMessage($"Waiting for database '{dbName}' to be in one of the following states: {string.Join(", ", validStates)} on server '{connection.ServerName}'.");

			while (!IsDatabaseInDesiredState(connection, dbName, logger, validStates))
			{
				token.ThrowIfCancellationRequested();
				Thread.Sleep(TimeSpan.FromSeconds(3)); // Retry delay
			}
		}

		public void WaitUntilDatabaseRemovedFromAvailabilityGroup(DbConnection connection, string dbName, Logger logger, CancellationToken token, IAlwaysOnHelper alwaysOnHelper)
		{
			while (alwaysOnHelper.IsDbPartOfAlwaysOn(connection, dbName))
			{
				logger.LogMessage($"Waiting for database {dbName} to be removed from availability group on {connection.ServerName}.");
				token.ThrowIfCancellationRequested();
				Thread.Sleep(TimeSpan.FromSeconds(3)); // Retry delay
			}

			logger.LogMessage($"Database {dbName} has been successfully removed from availability group.");
		}

		public void WaitUntilDatabaseReadyToRestore(DbConnection connection, string dbName, Logger logger, CancellationToken token)
		{
			WaitUntilDatabaseReady(connection, dbName, logger, token, new List<string> { "RESTORING" });
		}

		public void WaitUntilDatabaseIsOnline(DbConnection connection, string dbName, Logger logger, CancellationToken token)
		{
			WaitUntilDatabaseReady(connection, dbName, logger, token, new List<string> { "ONLINE" });
		}

		internal virtual bool IsDatabaseInDesiredState(DbConnection connection, string dbName, Logger logger, List<string> validStates)
		{
			try
			{
				var dbDescription = connection.DatabaseStateDescription(dbName);
				if (string.IsNullOrWhiteSpace(dbDescription) ||
					validStates.Any(state => string.Equals(dbDescription, state, StringComparison.OrdinalIgnoreCase)))
				{
					logger.LogMessage($"Database {dbName} state is ({dbDescription ?? "not found"}) on {connection.ServerName}.");
					return true;
				}
				else
				{
					logger.LogMessage($"Database {dbName} state is {dbDescription} on {connection.ServerName}. Retrying...");
					return false;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var waitPeriodInSeconds = TimeSpan.FromSeconds(5);
				logger.LogMessage($"Could not determine status of database {dbName} on {connection.ServerName}. Waiting {waitPeriodInSeconds.TotalSeconds} seconds before proceeding. Exception: {ex}");
				Thread.Sleep(waitPeriodInSeconds);
				return false;
			}
		}
	}
}
