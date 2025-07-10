using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	abstract class PrintJobTaskCore : ServiceProviderImpl, IDisposable
	{
		protected virtual int MaxBatchSize => BaseMaxBatchSize;

		internal const int BaseMaxBatchSize = 50;

		protected virtual bool RunTaskForJobTypes(bool requiresPrintServer, CancellationToken token, params PrintJobType[] jobTypesToProcess)
		{
			var printServerPK = requiresPrintServer ? GetPrintServer()?.PK : null;

			return TryGetJobsOfGivenTypesQuery(jobTypesToProcess, printServerPK, out var query) && RunTaskCore(query, token);
		}

		protected virtual StmPrintServer GetPrintServer()
		{
			return new BusinessObjectFactory().LoadTop1<StmPrintServer>(new ZQuery(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName));
		}

		protected bool RunTaskCore(ZNonPersistentDataQuery nonPersistentDataQuery, CancellationToken token)
		{
			LogForDebug("Starting RunTask");

			var hasProcessedAnyJobs = false;
			var hasJobsToProcess = false;

			do
			{
				token.ThrowIfCancellationRequested();
				var hasProcessedJobsThisRound = hasJobsToProcess = false;

				var printJobsQueue = new DbOnlyBusinessObjectQueue<StmPrintJob>(nonPersistentDataQuery, true);

				printJobsQueue.ProcessBatch((printJobs, e) =>
				{
					using (var mutexes = new DisposableList(0))
					{
						LogForDebug("Processing Batch.");
						e.Cancel |= token.IsCancellationRequested;

						if (e.Cancel)
						{
							ServiceLogger.Log(LogType.Warning, "Cancellation was requested.");

							return;
						}

						hasJobsToProcess = true;

						var lockedJobs = TryToLockJobs(printJobs, mutexes, Db.Connection);
						LogForDebug(Invariant($"Post lock. Job count: {lockedJobs.Length}, mutex count: {mutexes.Count}"));

						if (lockedJobs.Length > 0)
						{
							LogForDebug("Processing Jobs");

							hasProcessedAnyJobs = hasProcessedJobsThisRound = true;

							PrintJobManager.ProcessPrintJobs(lockedJobs);
						}
					}
				}, MaxBatchSize, token);

				if (hasProcessedJobsThisRound)
				{
					ServiceLogger.Log(LogType.Information, "Finished processing queue");
				}
			} while (hasJobsToProcess);

			return hasProcessedAnyJobs;
		}

		#region Locking

		/// <summary>
		/// Specifies if selected print jobs should be locked in processed only if locks were acquired.
		/// 
		/// 
		/// </summary>
		/// <remarks>
		/// Locking is done with SP_SB_DeliveryGroup field.
		/// If any error occur:
		/// Please check value AllowsMultipleInstances or AllowsMultipleInstancesOnSameHost in Subclasses of PrintJobTaskCore.
		/// </remarks>
		internal virtual StmPrintJob[] TryToLockJobs(ICollection<StmPrintJob> printJobs, DisposableList mutexes, DbConnection connection)
		{
			try
			{
				var firstPrintJob = printJobs.FirstOrDefault();

				if (firstPrintJob != default && TryGetLockOnDeliveryGroupGuid(connection, firstPrintJob, out var mutex))
				{
					mutexes.Add(mutex);

					return printJobs.Where(JobStillExistsAndAvailable).OrderBy(job => job.SP_Group).ThenBy(job => job.SP_Sequence).ToArray();
				}
			}
			catch (SqlException sqlException)
			{
				// If db connection is dropped then all locks would be lost.
				// Jobs should not be processed further unless having locks on them.
				ErrorReporter.ReportOnce("DB Connection error while grabbing print job mutexes", sqlException);
			}

			return Array.Empty<StmPrintJob>();
		}

		[Conditional("DEBUG")]
		void LogForDebug(string log)
		{
#if DEBUG
			LoggerForTest.Log(LogType.Information, log);
#endif
		}

#if DEBUG
		internal LoggerForTest LoggerForTest { get; } = new ();

		internal Action ActionInAnotherInstance { get; set; }
#endif
		internal static bool TryGetLockOnDeliveryGroupGuid(DbConnection connection, StmPrintJob job, out SqlApplicationLock appLock)
		{
			var key = (NoResString)"PrintJobTaskCore:" + job.SP_SB_DeliveryGroup.ToString().ToUpperInvariant();
			return connection.TryGetLock(key, out appLock);
		}

		protected virtual bool JobStillExistsAndAvailable(StmPrintJob job)
		{
			var reloadedJob = ReloadJobFromDb(job);
			return reloadedJob != null && !reloadedJob.SP_JobType.EqualsIgnoringCase(nameof(PrintType.DDS)) && reloadedJob.Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, reloadedJob.PK));
		}

		protected StmPrintJob ReloadJobFromDb(StmPrintJob job)
		{
#if DEBUG
			ActionInAnotherInstance?.Invoke();
#endif
			job.ReloadSafe();
			var reloadQuery = new ZDBOnlyQuery(job.GetType()) { ReLoadExistingRows = true };
			reloadQuery.AddToFilter(StmPrintJobSchema.PK, job.PK);

			return job.Factory.LoadTop1<StmPrintJob>(reloadQuery);
		}

		protected bool IsMaster(string taskName)
		{
			if (masterMutex != null)
			{
				if (masterMutex.IsHoldingLock())
				{
					return true;
				}

				masterMutex.Dispose();
				masterMutex = null;

				// This is an error condition.
				// I am no longer master but thought I was.
				// This would starve the process if I still hold the lock.
				// Since I never normally release the lock, drop the connection.
				((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection();
			}

			if (lastMasterCheck.IsEmpty || ZDateTime.UtcNow - lastMasterCheck > TimeSpan.FromMinutes(1))
			{
				lastMasterCheck = ZDateTime.UtcNow;
				return Db.Connection.TryGetLock(taskName, out masterMutex);
			}

			return false;
		}
		SqlApplicationLock masterMutex;
		ZDateTime lastMasterCheck;

		void IDisposable.Dispose()
		{
			masterMutex?.Dispose();
		}

		#endregion

		protected internal virtual bool TryGetJobsOfGivenTypesQuery(PrintJobType[] jobTypes, ZGuid? printServerPK, out ZNonPersistentDataQuery query)
		{
			const string sqlText = "dbo.GetNextPrintJob";
			var queryParameters = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@JobTypes", jobTypes.Select(t => t.ToString()).ToList(), StmPrintJobSchema.SP_JobType, true)
			};

			if (printServerPK.HasValue)
			{
				queryParameters.Add(ZSqlParameter.New("@PrintServerPK", printServerPK, Schema.GenericGuidSchemaColumn));
			}

			query = new ZNonPersistentDataQuery(sqlText, queryParameters);

			return true;
		}

		internal static string GetLockConditionOnDeliveryGroupGuid()
		{
			return string.Format(CultureInfo.InvariantCulture, "APPLOCK_TEST(/* StringLiteral */ 'public', CONCAT(/* StringLiteral */ 'PrintJobTaskCore:', UPPER(CAST({0} AS VARCHAR(36)))), /* StringLiteral */ 'exclusive', /* StringLiteral */ 'session') = 1", StmPrintJobSchema.Constants.SP_SB_DeliveryGroup);
		}

		internal static string GetLockConditionOnParentGuid()
		{
			const string parentKeyExpression = "UPPER(CAST(COALESCE(" + StmPrintJobSchema.Constants.SP_ParentGuid + ", " + StmPrintJobSchema.Constants.PK + ") AS VARCHAR(36)))";
			return "APPLOCK_TEST(/* StringLiteral */ 'public', CONCAT(/* StringLiteral */ 'PrintJobTaskCore:', " + parentKeyExpression + "), /* StringLiteral */ 'exclusive', /* StringLiteral */ 'session') = 1";
		}

		protected PrintJobManager PrintJobManager
		{
			get
			{
				if (printJobManager == null)
				{
					printJobManager = GetPrintJobManagerCore();
					printJobManager.OnProgress += Log;
					printJobManager.OnNotification += Notify;
				}

				return printJobManager;
			}
		}

		protected virtual PrintJobManager GetPrintJobManagerCore()
		{
			return new PrintJobManager();
		}

		PrintJobManager printJobManager;

		protected virtual void Log(TraceEventType eventType, string message)
		{
			ServiceLogger.Log(GetLogTypeFromTraceEventType(eventType), message);
		}

		protected virtual void Notify(bool successful, string message)
		{
		}

		static LogType GetLogTypeFromTraceEventType(TraceEventType eventType)
		{
			LogType logType;
			switch (eventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
					logType = LogType.Error;
					break;

				case TraceEventType.Warning:
					logType = LogType.Warning;
					break;

				case TraceEventType.Verbose:
					logType = LogType.Debug;
					break;

				default:
					logType = LogType.Information;
					break;
			}

			return logType;
		}
	}
}
