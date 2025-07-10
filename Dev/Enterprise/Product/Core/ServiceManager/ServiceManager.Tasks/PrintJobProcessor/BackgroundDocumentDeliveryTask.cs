using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.PrintJobProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"BDD",
	"Background Document Delivery",
	"DOC",
	typeof(BackgroundDocumentDeliveryTask),
	IsMandatory = true,
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("BDD", StmDocumentDeliverySchema.Constants.TableName, new[] { StmDocumentDeliverySchema.Constants.SDL_IsProcessed + "=0" }, "BDD Job Delivery")]
namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class BackgroundDocumentDeliveryTask : ServiceProviderImpl
	{
		const int MaxBatchSize = 50;

		public override void RunTask(CancellationToken token)
		{
			RunTaskCore(token);
		}

		void RunTaskCore(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Background Document Delivery Service Task Run Started.");
			DocumentDeliveryManager.PurgeOld(ServiceLogger);

			using (var mutexes = new DisposableList(0))
			{
				var documentDeliveryJobsQueue = new DbOnlyBusinessObjectQueue<StmDocumentDelivery>(GetJobsToProcess());
				documentDeliveryJobsQueue.ProcessBatch((deliveryJobs, e) =>
				{
					e.Cancel |= token.IsCancellationRequested;
					if (!e.Cancel)
					{
						var lockedJobs = TryToLockJobs(deliveryJobs, mutexes, Db.Connection);
						try
						{
							DocumentDeliveryManager.Execute(lockedJobs, ServiceLogger);
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ServiceLogger.Error($"Background Document Delivery Service Task Run Failed. The exception type is {ex.GetType().FullName}. The exception message is: {ex.Message}. If this service task failed often, please raise an incident for further investigation.");
							ErrorReporter.ReportOnce("Service task BDD execute failed.", ex);
						}
					}
				}, MaxBatchSize, token);
			}
			var countProcessedJobs = DocumentDeliveryManager.CountProcessedJobs();
			ServiceLogger.Log(LogType.Information, $"{countProcessedJobs} document(s) processed.");
			ServiceLogger.Log(LogType.Information, $"Background Document Delivery Service Task Run Completed.");
		}

		ZNonPersistentDataQuery GetJobsToProcess()
		{
			const string pk = "UPPER(CAST(" + StmDocumentDeliverySchema.Constants.PK + " AS VARCHAR(36)))";

			var sql = $@"
select {StmDocumentDeliverySchema.Constants.PK}
from {StmDocumentDeliverySchema.Constants.SqlSchemaName}.{StmDocumentDeliverySchema.Constants.TableName}
where {StmDocumentDeliverySchema.Constants.SDL_IsProcessed} = 0
  and {StmDocumentDeliverySchema.Constants.SDL_RetryAttempts} < 3
  and APPLOCK_TEST('public', CONCAT('StmDocumentDeliveryManager:', {pk}), 'exclusive', 'session') = 1";
			return new ZNonPersistentDataQuery(sql);
		}

		StmDocumentDelivery[] TryToLockJobs(StmDocumentDelivery[] jobs, DisposableList mutexes, DbConnection connection)
		{
			try
			{
				SqlApplicationLock mutex;
				var lockedPks = new List<ZGuid>();
				foreach (var job in jobs)
				{
					if (TryGetLock(connection, job, out mutex))
					{
						mutexes.Add(mutex);
						lockedPks.Add(job.PK);
					}
				}

				return jobs.Where(job => JobStillExistsAndAvailable(job, lockedPks)).ToArray();
			}
			//If our db connection is dropped then we lose all our locks.
			//We should not process jobs unless we know we have locks on them.
			catch (SqlException sqlException)
			{
				ErrorReporter.ReportOnce("BDD: DB Connection error while grabbing process job mutexes", sqlException);
			}

			return Array.Empty<StmDocumentDelivery>();
		}

		static bool TryGetLock(DbConnection connection, StmDocumentDelivery job, out SqlApplicationLock appLock)
		{
			var key = "StmDocumentDeliveryManager:" + job.PK.ToString().ToUpperInvariant();
			return connection.TryGetLock(key, out appLock);
		}

		bool JobStillExistsAndAvailable(StmDocumentDelivery job, List<ZGuid> lockedPks)
		{
			var reloadedJob = ReloadJobFromDb(job);
			return lockedPks.Contains(job.PK)
					&& reloadedJob != null
					&& !reloadedJob.SDL_IsProcessed
					&& reloadedJob.SDL_RetryAttempts.ToZInt().CompareTo(3) < 0;
		}

		StmDocumentDelivery ReloadJobFromDb(StmDocumentDelivery job)
		{
			job.ReloadSafe();
			var reloadQuery = new ZDBOnlyQuery(job.GetType()) { ReLoadExistingRows = true };
			reloadQuery.AddToFilter(StmDocumentDeliverySchema.PK, job.PK);

			return job.Factory.LoadTop1<StmDocumentDelivery>(reloadQuery);
		}

		protected virtual DocumentDeliveryManager DocumentDeliveryManager
		{
			get
			{
				if (_documentDeliveryManager == null)
				{
					_documentDeliveryManager = new DocumentDeliveryManager();
				}

				return _documentDeliveryManager;
			}
		}
		DocumentDeliveryManager _documentDeliveryManager;
	}
}
