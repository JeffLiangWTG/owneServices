using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Environment;
using WTG.Data.SqlDbSecuritySynchroniser;
using IntegrationLogging = Enterprise.Integration;

namespace Enterprise.SqlSecurity
{
	public class SqlSecurityManager : ISqlSecurityManager
	{
		internal SqlSecurityManager(IntegrationLogging.ILogger logger, ISqlDbSecuritySynchroniserProvider sqlDbSecuritySynchroniserProvider, string mainDatabaseName, bool allowTransaction = false)
		{
			_ = logger ?? throw new ArgumentNullException(nameof(logger));
			_ = sqlDbSecuritySynchroniserProvider ?? throw new ArgumentException(nameof(sqlDbSecuritySynchroniserProvider));

			this.logger = logger;
			this.mainDatabaseName = mainDatabaseName;
			this.sqlDbSecuritySynchroniserProvider = sqlDbSecuritySynchroniserProvider;

			#region Test

#if DEBUG
			if (!Globals.IsTest_ForTest.Value && allowTransaction)
			{
				throw new NotSupportedException("Transaction is not allowed in production code to prevent blocking of the Sql server.");
			}

			this.allowTransaction = allowTransaction;
#endif
			#endregion
		}

		#region Test

#if DEBUG

		public SqlSecurityManager(IntegrationLogging.ILogger logger, string mainDatabaseName, bool allowTransaction) : this(logger, new SqlDbSecuritySynchroniserProvider(), mainDatabaseName, allowTransaction)
		{
		}

#endif
		#endregion

		public SqlSecurityManager(IntegrationLogging.ILogger logger, string mainDatabaseName) : this(logger, new SqlDbSecuritySynchroniserProvider(), mainDatabaseName, allowTransaction: false)
		{
		}

		readonly IntegrationLogging.ILogger logger;
		readonly string mainDatabaseName;
		IStaffInfoProvider staffInfoProvider;
		readonly ISqlDbSecuritySynchroniserProvider sqlDbSecuritySynchroniserProvider;
		List<string> likeFiltersForSharedEnvironment;
		readonly bool allowTransaction;

		IStaffInfoProvider GetStaffInfoProvider(CancellationToken cancellationToken)
		{
			if (staffInfoProvider is null)
			{
				staffInfoProvider = new StaffInfoProvider(cancellationToken, logger);
			}

			return staffInfoProvider;
		}

		void LogProposedRecords(AdminConnection connection, SqlSecurityLogger sqlSecurityLogger, string proposedRecords, string topLogMessage)
		{
			var builder = new StringBuilder();

			connection.ExecuteReader(
				proposedRecords,
				dbRecord =>
				{
					if (builder.Length == 0)
					{
						builder.AppendLine(topLogMessage);
						builder.AppendLine(
							string.Join(",", EnumerateRecord(dbRecord)
							.Select(recordValue => recordValue.FieldName)));
					}

					builder.AppendLine(
						string.Join(",", EnumerateRecord(dbRecord)
						.Select(recordValue =>
						{
							if (recordValue.Value is null || recordValue.Value == DBNull.Value)
							{
								return string.Empty;
							}

							if (recordValue.FieldName.Contains("password", StringComparison.OrdinalIgnoreCase))
							{
								return "<password>";
							}

							if (recordValue.Value is byte[])
							{
								return DataUtils.BytesToHexString((byte[])recordValue.Value);
							}

							return recordValue.Value.ToString();
						})));
				});

			sqlSecurityLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Debug, builder.ToString());

			IEnumerable<(string FieldName, object Value)> EnumerateRecord(IDataRecord dbRecord)
			{
				for (var i = 0; i < dbRecord.FieldCount; i++)
				{
					yield return (dbRecord.GetName(i), dbRecord[i]);
				}
			}
		}

		void CheckAndThrowIfInTransactionWhenNotAllowed(AdminConnection connection)
		{
			if (!allowTransaction && connection.ExecuteScalar<int>("SELECT @@TRANCOUNT") > 0)
			{
				throw new InvalidOperationException("The method should not be called within a transaction.");
			}
		}

		(Exception Exception, bool IsSynchronised) GetSynchronisationResult(IDifferenceSynchroniser synchroniser, AdminConnection connectionToServer, bool trialRun)
		{
			try
			{
				return (null, synchroniser.Synchronise(((IDbConnectionInternals)connectionToServer).ADOConnection, trialRun));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return (ex, false);
			}
		}

		List<string> GetLikeFiltersForSharedEnvironment(IDatabaseInfoRetriever databaseInfoRetriever, bool ignoreStaffMembers)
		{
			if (likeFiltersForSharedEnvironment == null)
			{
				likeFiltersForSharedEnvironment = new List<string> { $"{DataUtils.ReplaceSqlLikeWildcard(mainDatabaseName)}[_]%" };

				if (!ignoreStaffMembers)
				{
					likeFiltersForSharedEnvironment.AddRange(databaseInfoRetriever.StaffPrefixes.Select(prefix => $"{DataUtils.ReplaceSqlLikeWildcard(prefix)}%"));

					// The following filter is a HACK that exists because we do not have a name convention for AD integrated logins
					// Once we have a convention, this filter should disappear
					if (databaseInfoRetriever.AllStaffADLoginNames != null)
					{
						likeFiltersForSharedEnvironment.AddRange(databaseInfoRetriever.AllStaffADLoginNames.Select(name => DataUtils.ReplaceSqlLikeWildcard(name)));
					}
				}
			}

			return likeFiltersForSharedEnvironment.ToList();
		}

		#region Server level

		void BuildServerSecurity(AdminConnection connection, IDatabaseInfoRetriever databaseInfoRetriever, SqlSecurityLogger synchroniserLogger, CancellationToken cancellationToken, bool ignoreStaffMembers, bool trialRun)
		{
			SqlSecurityManagerNew.BuildServerSecurity(
				connection: connection,
				databaseInfoRetriever: databaseInfoRetriever,
				logger: logger,
				synchroniserLogger: synchroniserLogger,
				cancellationToken: cancellationToken,
				mainDatabaseName: mainDatabaseName,
				ignoreStaffMembers: ignoreStaffMembers,
				trialRun: trialRun
			);
		}

		public void BuildServerSecurity(AdminConnection connection, bool trialRun)
		{
			var synchroniserLogger = new SqlSecurityLogger(logger);

			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, CancellationToken.None))
			{
				CheckAndThrowIfInTransactionWhenNotAllowed(connection);

				BuildServerSecurity(connection, databaseInfoRetriever, synchroniserLogger, CancellationToken.None, ignoreStaffMembers: false, trialRun: trialRun);
			}
		}

		public void Propagate(AdminConnection connection, IEnumerable<string> replicas, CancellationToken cancellationToken)
		{
			CheckAndThrowIfInTransactionWhenNotAllowed(connection);

			SqlSecurityManagerNew.Propagate(
				connection: connection,
				logger: logger,
				synchroniserLogger: new SqlSecurityLogger(logger),
				mainDatabaseName: mainDatabaseName,
				replicas: replicas,
				cancellationToken: cancellationToken
			);
		}

		#endregion Server level

		#region Database level

		void BuildDatabaseSecurity(AdminConnection mainConnection, IDatabaseInfoRetriever databaseInfoRetriever, string databaseName, SqlSecurityLogger synchroniserLogger, CancellationToken cancellationToken, bool ignoreStaffMembers, bool trialRun)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				var totalTimeStopWatch = new Stopwatch();
				totalTimeStopWatch.Start();

				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for database '{databaseName}'.");

				var staffInfoProvider = ignoreStaffMembers
					? new NoStaffInfoProvider()
					: GetStaffInfoProvider(cancellationToken);

				var databaseType = Helper.GetDatabaseType(mainDatabaseName, databaseInfoRetriever.SingleRefDbName, databaseName);
				var selfHostedOpen = !databaseInfoRetriever.IsHostedInWiseCloud && databaseInfoRetriever.IsDatabaseSecurityModeOpen;

				var likeFilters = new List<string>();
				var notLikeFilters = new List<string>();

				switch (databaseType)
				{
					case DatabaseType.SingleSharedRef:
						notLikeFilters.Add("%");
						break;
					case DatabaseType.SharedRef:
						likeFilters = GetLikeFiltersForSharedEnvironment(databaseInfoRetriever, ignoreStaffMembers);
						break;
					default:
						if (selfHostedOpen || ignoreStaffMembers)
						{
							likeFilters = GetLikeFiltersForSharedEnvironment(databaseInfoRetriever, ignoreStaffMembers);
							likeFilters.Add("cw%Role");
						}
						else
						{
							notLikeFilters.AddRange(new[]
							{
								"sys",
								"cdc",
								"dbo",
								"INFORMATION_SCHEMA",
								"public",
								"guest",
								"db[_]%",
							});
						}

						break;
				}

				var databaseConnection = mainConnection;
				if (databaseType == DatabaseType.Audit)
				{
					databaseConnection = databaseInfoRetriever.AuditServerConnection ?? mainConnection;
				}
				else if (databaseType == DatabaseType.EDW)
				{
					databaseConnection = databaseInfoRetriever.DataWarehouseServerConnection ?? mainConnection;
				}

				var isDedicateServer = databaseInfoRetriever.IsDedicatedServerInstance(databaseConnection.ServerInstanceName);
				var sqlSecurityBuilder = new SqlSecurityBuilder(mainDatabaseName, databaseInfoRetriever.IsHostedInWiseCloud, isDedicateServer, staffInfoProvider);

				using (((ICurrentDbControl)databaseConnection).UseDatabase(databaseName))
				{
					var proposedValuesStopWatch = new Stopwatch();
					proposedValuesStopWatch.Start();

					var sqlDbSecurityBuilderResult = sqlSecurityBuilder.GetDatabaseLevelInfo(mainConnection, databaseConnection, databaseType);
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Getting proposed values for database '{databaseName}' took {proposedValuesStopWatch.Elapsed}.");

					if (trialRun)
					{
						LogProposedRecords(databaseConnection, synchroniserLogger, sqlDbSecurityBuilderResult.ProposedPrincipalsAndMemberships, $"Expected principals and memberships for database '{databaseName}'");
						LogProposedRecords(databaseConnection, synchroniserLogger, sqlDbSecurityBuilderResult.ProposedPermissions, $"Expected permissions for database '{databaseName}'");
					}

					var synchroniser = sqlDbSecuritySynchroniserProvider.GetDatabaseSyncrhoniser(
						sqlDbSecurityBuilderResult.ProposedPrincipalsAndMemberships,
						sqlDbSecurityBuilderResult.ProposedPermissions,
						likeFilters: likeFilters.ToArray(),
						notLikeFilters: notLikeFilters.ToArray(),
						synchroniserLogger);

					var synchronisationResult = GetSynchronisationResult(synchroniser, databaseConnection, trialRun);
					if (!trialRun && !synchronisationResult.IsSynchronised)
					{
						if (synchronisationResult.Exception != null)
						{
							LogProposedRecords(databaseConnection, synchroniserLogger, sqlDbSecurityBuilderResult.ProposedPrincipalsAndMemberships, $"Expected principals and memberships for database '{databaseConnection.CurrentDatabase}'");
							LogProposedRecords(databaseConnection, synchroniserLogger, sqlDbSecurityBuilderResult.ProposedPermissions, $"Expected permissions for database '{databaseConnection.CurrentDatabase}'");

							logger.Log(IntegrationLogging.LogType.Error, $"Sql security for database '{databaseConnection.CurrentDatabase}' on server '{databaseConnection.ServerName}' is not synchronised.");
						}

						synchroniserLogger.ReportDeveloperExceptionOnce($"Sql security for database '{databaseConnection.CurrentDatabase}' on server '{databaseConnection.ServerName}' is not synchronised.", synchronisationResult.Exception);
					}

					totalTimeStopWatch.Stop();
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, $"Building Sql security for database '{databaseConnection.CurrentDatabase}' took {totalTimeStopWatch.Elapsed}.");
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Information, ".");
				}
			}
			catch (OperationCanceledException)
			{
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, $"Cancellation was requested before security build for database '{databaseName}' is started. Main database name '{mainDatabaseName}' on server '{mainConnection.ServerName}'.");
				throw;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, $"Failed to build Sql security for database '{databaseName}'. Main database name '{mainDatabaseName}' on server '{mainConnection.ServerName}'.", ex);
			}
		}

		public void BuildDatabaseSecurity(AdminConnection connection, string databaseName, bool trialRun)
		{
			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			{
				var synchroniserLogger = new SqlSecurityLogger(logger);

				using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, CancellationToken.None))
				{
					CheckAndThrowIfInTransactionWhenNotAllowed(connection);

					BuildDatabaseSecurity(connection, databaseInfoRetriever, databaseName, synchroniserLogger, CancellationToken.None, ignoreStaffMembers: false, trialRun: trialRun);
				}
			}
		}

		#endregion Database level

		#region Full synchronisation

		public void BuildSecurityForAllDatabases(AdminConnection connection, CancellationToken cancellationToken)
		{
			var synchroniserLogger = new SqlSecurityLogger(logger);
			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, cancellationToken))
			{
				CheckAndThrowIfInTransactionWhenNotAllowed(connection);

				IEnumerable<string> allDatabases = null;
				try
				{
					allDatabases = databaseInfoRetriever.AllDatabases;
				}
				catch (OperationCanceledException)
				{
					synchroniserLogger.LogToIntegrationLoger(IntegrationLogging.LogType.Error, $"Cancellation was requested when building Sql security for databases. Main database name '{mainDatabaseName}' on server '{connection.ServerName}'.");
					throw;
				}

				foreach (var database in allDatabases)
				{
					BuildDatabaseSecurity(connection, databaseInfoRetriever, database, synchroniserLogger, cancellationToken: cancellationToken, ignoreStaffMembers: false, trialRun: false);
				}
			}
		}

		public void BuildSecurity(AdminConnection connection, CancellationToken cancellationToken, bool trialRun = false)
		{
			var timer = new Stopwatch();
			timer.Start();

			var synchroniserLogger = new SqlSecurityLogger(logger);

			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, cancellationToken))
			{
				CheckAndThrowIfInTransactionWhenNotAllowed(connection);
				BuildServerSecurity(connection, databaseInfoRetriever, synchroniserLogger, cancellationToken, ignoreStaffMembers: false, trialRun: trialRun);

				foreach (var database in databaseInfoRetriever.AllDatabases)
				{
					BuildDatabaseSecurity(connection, databaseInfoRetriever, database, synchroniserLogger, cancellationToken, ignoreStaffMembers: false, trialRun: trialRun);
				}
			}

			timer.Stop();
			logger.Log(IntegrationLogging.LogType.Information, $"Building Sql security took {timer.Elapsed}.");
		}

		#endregion Full synchronisation

		#region Application logins and roles synchronisation

		public void BuildApplicationLoginsSecurity(AdminConnection connection, CancellationToken cancellationToken)
		{
			var synchroniserLogger = new SqlSecurityLogger(logger);

			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			using (var databaseInfoRetriever = new DatabaseInfoRetriever(connection, mainDatabaseName, synchroniserLogger, cancellationToken, ignoreStaffMembers: true))
			{
				CheckAndThrowIfInTransactionWhenNotAllowed(connection);

				BuildServerSecurity(connection, databaseInfoRetriever, synchroniserLogger, cancellationToken, ignoreStaffMembers: true, trialRun: false);

				foreach (var database in databaseInfoRetriever.AllDatabases)
				{
					BuildDatabaseSecurity(connection, databaseInfoRetriever, database, synchroniserLogger, cancellationToken, ignoreStaffMembers: true, trialRun: false);
				}
			}
		}

		#endregion Application logins and roles synchronisation

	}
}
