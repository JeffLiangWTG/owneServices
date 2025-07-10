
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.SqlSecurity.Common;
using CargoWise.SqlSecurity.Server;
using Newtonsoft.Json;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity
{
	// ====================================================
	// NOTE
	// This module is currently being actively worked on, and
	// will soon be moved to CargoWise/Shared.
	// If making changes here, please keep Leonid Shchurov (LDS)
	// in the loop
	// ====================================================
	static class SqlSecurityManagerNew
	{
		static T WithTimeLog<T>(SqlSecurityLogger synchroniserLogger, string message, Func<T> action)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			var result = action();
			stopWatch.Stop();
			synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"{message} took {stopWatch.Elapsed}.");
			return result;
		}

		static void WithErrorHandling(SqlSecurityLogger synchroniserLogger, string actionName, Action action)
		{
			try
			{
				action();
			}
			catch (OperationCanceledException)
			{
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, $"Cancelled while {actionName}");
				throw;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, e.Message);
			}
		}

		static void ApplyProposedSecurityOntoServer(
			ISqlExecutionContext connectionCtx,
			SqlServerProposedEntities sqlDbSecurityProposed,
			string[] whitelistFilters,
			string environmentName,
			SqlSecurityLogger synchroniserLogger,
			CancellationToken cancellationToken)
		{
			WithErrorHandling(
				synchroniserLogger,
				$"Applying proposed security onto {environmentName}",
				() =>
				{
					var sqlDbSecurityExisting = WithTimeLog(
						synchroniserLogger,
						$"Getting existing values for {environmentName}",
						() => ServerExistingStateQuery.GetSqlServerExistingEntities(connectionCtx, whitelistFilters, Array.Empty<string>(), Array.Empty<string>()));
					var sqlDbSecurityDiff = WithTimeLog(
						synchroniserLogger,
						$"Getting execution commands for {environmentName}",
						() => ServerDiff.GenerateDiffCommands(sqlDbSecurityExisting, sqlDbSecurityProposed, Array.Empty<string>()));

					synchroniserLogger.LogToIntegrationLoger(
						IntegrationLogging.LogType.Information,
						$"Commands to be executed:\r\n{string.Join("\r\n", sqlDbSecurityDiff.Select(cmd => cmd.Command))}"
					);

					synchroniserLogger.LogToIntegrationLoger(
						IntegrationLogging.LogType.Debug,
						$"Synchronizer debug information for {environmentName}:\r\nProposed: {JsonConvert.SerializeObject(sqlDbSecurityProposed)}\r\nExisting: {JsonConvert.SerializeObject(sqlDbSecurityExisting)}\r\nDiff: {JsonConvert.SerializeObject(sqlDbSecurityDiff)}"
					);

					cancellationToken.ThrowIfCancellationRequested();

					var messages = WithTimeLog(
						synchroniserLogger,
						$"Executing commands for {environmentName}",
						() => DiffExecutor.ExecuteDiff(connectionCtx, sqlDbSecurityDiff));

					if (messages.Length > 0)
					{
						synchroniserLogger.LogToIntegrationLoger(
							IntegrationLogging.LogType.Error,
							$"There were errors executing commands.\r\n{string.Join("\r\n", messages.Select(PrettyPrintMessage))}"
						);
					}

					var sqlDbSecurityExistingAfterSync = WithTimeLog(
						synchroniserLogger,
						$"Getting server state after synchronization of {environmentName}",
						() => ServerExistingStateQuery.GetSqlServerExistingEntities(connectionCtx, whitelistFilters, Array.Empty<string>(), Array.Empty<string>()));
					var sqlDbSecurityDiffAfterSync = WithTimeLog(
						synchroniserLogger,
						$"Validating synchronization of {environmentName}",
						() => ServerDiff.GenerateDiffCommands(sqlDbSecurityExistingAfterSync, sqlDbSecurityProposed, Array.Empty<string>()));

					if (sqlDbSecurityDiffAfterSync.Any())
					{
						synchroniserLogger.LogToIntegrationLoger(
							IntegrationLogging.LogType.Debug,
							$"Validation synchronizer debug information for {environmentName}:\r\nProposed: {{unchanged}}\r\nExisting: {JsonConvert.SerializeObject(sqlDbSecurityExistingAfterSync)}\r\nDiff: {JsonConvert.SerializeObject(sqlDbSecurityDiffAfterSync)}"
						);

						var extraCommands = string.Join("\r\n", sqlDbSecurityDiffAfterSync.Select(cmd => cmd.Command));
						var message = $"Synchronization of {environmentName} incomplete. There were more commands generated after synchronization.";

						synchroniserLogger.LogToIntegrationLoger(
							IntegrationLogging.LogType.Error,
							$"{message}\r\n{extraCommands}"
						);

						// Exception to pass to error reporter
						var exceptionToReport = new Exception($"{message}\r\n{extraCommands}");
						synchroniserLogger.ReportDeveloperExceptionOnce(message, exceptionToReport);
					}
				});
		}

		static void TrialRunSecurityOntoServer(
			ISqlExecutionContext connectionCtx,
			SqlServerProposedEntities sqlDbSecurityProposed,
			string[] whitelistFilters,
			string environmentName,
			SqlSecurityLogger synchroniserLogger)
		{
			WithErrorHandling(
				synchroniserLogger,
				$"Trial running security for {environmentName}",
				() =>
				{
					var sqlDbSecurityExisting = WithTimeLog(
						synchroniserLogger,
						$"Getting existing values for {environmentName}",
						() => ServerExistingStateQuery.GetSqlServerExistingEntities(connectionCtx, whitelistFilters, Array.Empty<string>(), Array.Empty<string>()));
					var sqlDbSecurityDiff = WithTimeLog(
						synchroniserLogger,
						$"Getting execution commands for {environmentName}",
						() => ServerDiff.GenerateDiffCommands(sqlDbSecurityExisting, sqlDbSecurityProposed, Array.Empty<string>()));

					synchroniserLogger.LogToIntegrationLoger(
						IntegrationLogging.LogType.Debug,
						$"Synchronizer debug information for {environmentName}:\r\nProposed: {JsonConvert.SerializeObject(sqlDbSecurityProposed)}\r\nExisting: {JsonConvert.SerializeObject(sqlDbSecurityExisting)}\r\nDiff: {JsonConvert.SerializeObject(sqlDbSecurityDiff)}"
					);

					synchroniserLogger.LogToIntegrationLoger(
						IntegrationLogging.LogType.Information,
						$"Commands to be executed:\r\n{string.Join("\r\n", sqlDbSecurityDiff.Select(cmd => cmd.Command))}"
					);

					synchroniserLogger.LogToIntegrationLoger(
						IntegrationLogging.LogType.Information,
						$"Trial run completed successfully for {environmentName}. No commands will be executed."
					);
				});
		}

		static void BuildSecurityForSingleServer(
			AdminConnection connection,
			string serverName,
			string mainDatabaseName,
			bool isDedicatedServer,
			bool isHostedInWiseCloud,
			string[] whitelistFilters,
			IStaffInfoProvider staffInfoProvider,
			SqlSecurityLogger synchroniserLogger,
			CancellationToken cancellationToken,
			bool trialRun)
		{
			var connectionCtx = new SqlExecutionContext(((IDbConnectionInternals)connection).ADOConnection, null);

			var environmentName = $"server '{serverName}' with main database name '{mainDatabaseName}'"; // For logs

			using (connection.UseMasterDb())
			{
				WithErrorHandling(
					synchroniserLogger,
					$"building Sql security for {environmentName}'",
					() =>
					{
						var stopWatch = new Stopwatch();
						stopWatch.Start();

						synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for {environmentName}.");

						var sqlSecurityBuilder = new SqlSecurityBuilder(mainDatabaseName, isHostedInWiseCloud, isDedicatedServer, staffInfoProvider);

						var sqlDbSecurityProposed = WithTimeLog(
							synchroniserLogger,
							$"Getting proposed values for {environmentName}",
							() => sqlSecurityBuilder.GetServerLevelInfoWithNewBuilder(connection));

						if (trialRun)
						{
							TrialRunSecurityOntoServer(
								connectionCtx: connectionCtx,
								sqlDbSecurityProposed: sqlDbSecurityProposed,
								whitelistFilters: whitelistFilters,
								environmentName: environmentName,
								synchroniserLogger: synchroniserLogger);
						}
						else
						{
							ApplyProposedSecurityOntoServer(
								connectionCtx: connectionCtx,
								sqlDbSecurityProposed: sqlDbSecurityProposed,
								whitelistFilters: whitelistFilters,
								environmentName: environmentName,
								synchroniserLogger: synchroniserLogger,
								cancellationToken: cancellationToken);
						}

						stopWatch.Stop();
						synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for {environmentName} took {stopWatch.Elapsed}.");
					}
				);
			}
		}

		static string PrettyPrintMessage(SqlDiffExecutionMessage message)
		{
			var lines = new List<string> { $"{message.Command.Command}     - {message.SqlMessage}" };
			if (message.Command.FallbackCommands.Length > 0)
			{
				lines.Add("Fallback commands executed:");
				lines.AddRange(message.Command.FallbackCommands.Select(cmd => $" - {cmd.Command}"));
			}

			return string.Join("\r\n", lines);
		}

		public static void BuildServerSecurity(
			AdminConnection connection,
			IDatabaseInfoRetriever databaseInfoRetriever,
			IntegrationLogging.ILogger logger,
			SqlSecurityLogger synchroniserLogger,
			CancellationToken cancellationToken,
			string mainDatabaseName,
			bool ignoreStaffMembers,
			bool trialRun)
		{
			var cancellationRequestedMessage = "Cancellation was requested";

			try
			{
				var totalTimeStopWatch = new Stopwatch();
				totalTimeStopWatch.Start();

				IStaffInfoProvider staffInfoProvider = ignoreStaffMembers
					? new NoStaffInfoProvider()
					: new StaffInfoProvider(cancellationToken, logger);

				var whitelistFilters = GetServerWhitelistFiltersForSharedEnvironment(databaseInfoRetriever, staffInfoProvider, mainDatabaseName, ignoreStaffMembers);

				var isDedicatedServer = databaseInfoRetriever.IsDedicatedServerInstance(connection.ServerInstanceName);

				BuildSecurityForSingleServer(
					connection,
					connection.ServerName,
					mainDatabaseName,
					isDedicatedServer,
					databaseInfoRetriever.IsHostedInWiseCloud,
					whitelistFilters,
					staffInfoProvider,
					synchroniserLogger,
					cancellationToken,
					trialRun);

				totalTimeStopWatch.Stop();
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for server '{connection.ServerName}' took {totalTimeStopWatch.Elapsed}.");

				cancellationRequestedMessage = $"Cancellation was requested before building security for server '{connection.ServerName}'. Main database name '{mainDatabaseName}' on server '{connection.ServerName}'.";
				cancellationToken.ThrowIfCancellationRequested();

				var dataWareHouseServerConnection = databaseInfoRetriever.DataWarehouseServerConnection;
				var auditDatabaseServerConnection = databaseInfoRetriever.AuditServerConnection;

				if (auditDatabaseServerConnection != null)
				{
					totalTimeStopWatch.Start();
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for server '{auditDatabaseServerConnection.ServerName}'.");

					var isAuditDedicatedServer = auditDatabaseServerConnection != null ? databaseInfoRetriever.IsDedicatedServerInstance(auditDatabaseServerConnection.ServerInstanceName) : isDedicatedServer;

					BuildSecurityForSingleServer(
						connection,
						connection.ServerName,
						mainDatabaseName,
						isAuditDedicatedServer,
						databaseInfoRetriever.IsHostedInWiseCloud,
						whitelistFilters,
						staffInfoProvider,
						synchroniserLogger,
						cancellationToken,
						trialRun);

					totalTimeStopWatch.Stop();
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for server '{auditDatabaseServerConnection.ServerName}' took {totalTimeStopWatch.Elapsed}.");
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, ".");
				}

				if (dataWareHouseServerConnection != null && dataWareHouseServerConnection != auditDatabaseServerConnection)
				{
					totalTimeStopWatch.Start();
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for server '{dataWareHouseServerConnection.ServerName}'");

					var isEdwDedicatedServer = dataWareHouseServerConnection != null ? databaseInfoRetriever.IsDedicatedServerInstance(dataWareHouseServerConnection.ServerInstanceName) : isDedicatedServer;

					BuildSecurityForSingleServer(
						connection,
						connection.ServerName,
						mainDatabaseName,
						isEdwDedicatedServer,
						databaseInfoRetriever.IsHostedInWiseCloud,
						whitelistFilters,
						staffInfoProvider,
						synchroniserLogger,
						cancellationToken,
						trialRun);

					totalTimeStopWatch.Stop();
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for server '{dataWareHouseServerConnection.ServerName}' took {totalTimeStopWatch.Elapsed}.");
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, ".");
				}
			}
			catch (OperationCanceledException)
			{
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, cancellationRequestedMessage);
				throw;
			}
		}

		public static void Propagate(
			AdminConnection connection,
			string mainDatabaseName,
			IntegrationLogging.ILogger logger,
			SqlSecurityLogger synchroniserLogger,
			IEnumerable<string> replicas,
			CancellationToken cancellationToken)
		{
			var connectionCtx = new SqlExecutionContext(((IDbConnectionInternals)connection).ADOConnection, null);

			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, cancellationToken))
			{
				var totalTimeStopWatch = new Stopwatch();
				totalTimeStopWatch.Start();

				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Propagating Sql server logins from server '{connection.ServerName}', main database name '{mainDatabaseName}'.");

				var staffInfoProvider = new StaffInfoProvider(cancellationToken, logger);
				var whitelistFilters = GetServerWhitelistFiltersForSharedEnvironment(databaseInfoRetriever, staffInfoProvider, mainDatabaseName, ignoreStaffMembers: false);

				var isDedicatedServer = databaseInfoRetriever.IsDedicatedServerInstance(connection.ServerInstanceName);
				var sqlSecurityBuilder = new SqlSecurityBuilder(mainDatabaseName, databaseInfoRetriever.IsHostedInWiseCloud, isDedicatedServer, staffInfoProvider);

				using (connection.UseMasterDb())
				{
					var sqlDbSecurityProposed = WithTimeLog(
						synchroniserLogger,
						$"Getting proposed values for server '{connection.ServerName}' for system with main database '{mainDatabaseName}'",
						() => sqlSecurityBuilder.GetServerLevelInfoWithNewBuilder(connection));
					var sqlDbSecurityExisting = WithTimeLog(
						synchroniserLogger,
						$"Getting existing propagation values from primary server '{connection.ServerName}' for system with main database '{mainDatabaseName}'",
						() => ServerExistingStateQuery.GetSqlServerExistingEntities(connectionCtx, whitelistFilters, Array.Empty<string>(), Array.Empty<string>()));

					var proposedForPropagation = ServerEntityTransformations.GetSqlServerEntitiesForPropagation(sqlDbSecurityProposed, sqlDbSecurityExisting);

					Parallel.ForEach(replicas, (replicaServerName) =>
					{
						var synchroniserLogger = new SqlSecurityLogger(logger);

						var environmentName = $"server '{replicaServerName}' for system with main database '{mainDatabaseName}'"; // For logs

						WithErrorHandling(
							synchroniserLogger,
							$"propagating to ${environmentName}",
							() =>
							{
								synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Propagating Sql server security to server '{replicaServerName}' from primary server '{connection.ServerName}', main database name '{mainDatabaseName}'.");
								cancellationToken.ThrowIfCancellationRequested();

								using (var connectionToReplica = Db.NewAdminConnection(replicaServerName, Db.SqlMasterDb))
								{
									var connectionCtx = new SqlExecutionContext(((IDbConnectionInternals)connectionToReplica).ADOConnection, null);
									ApplyProposedSecurityOntoServer(
										connectionCtx: connectionCtx,
										sqlDbSecurityProposed: proposedForPropagation,
										whitelistFilters: whitelistFilters,
										environmentName: environmentName,
										synchroniserLogger: synchroniserLogger,
										cancellationToken: cancellationToken);
								}
							});
					});
				}

				totalTimeStopWatch.Stop();
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Propagating Sql security from primary server '{connection.ServerName}' for system with main database name '{mainDatabaseName}' took {totalTimeStopWatch.Elapsed}.");
			}
		}

		static string[] GetServerWhitelistFiltersForSharedEnvironment(IDatabaseInfoRetriever databaseInfoRetriever, IStaffInfoProvider staffInfoProvider, string mainDatabaseName, bool ignoreStaffMembers)
		{
			var filters = new List<string> { $"{DataUtils.ReplaceSqlLikeWildcard(mainDatabaseName)}[_]%" };

			if (!ignoreStaffMembers)
			{
				filters.AddRange(databaseInfoRetriever.StaffPrefixes.Select(prefix => $"{DataUtils.ReplaceSqlLikeWildcard(prefix)}%"));

				// Adding all staff AD logins into the like filters to ensure deleted windows logins can still appear in the existing list.
				// This will be deleted once we add marker roles.
				if (databaseInfoRetriever.AllStaffADLoginNames != null)
				{
					filters.AddRange(databaseInfoRetriever.AllStaffADLoginNames.Select(name => DataUtils.ReplaceSqlLikeWildcard(name)));
				}
			}

			return filters.ToArray();
		}
	}
}
