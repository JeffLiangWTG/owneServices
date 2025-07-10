using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.PrintProcessing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"DOD",
	"Documents delivery",
	"DOC",
	typeof(Enterprise.ServiceManager.Tasks.PrintJobProcessor.DocumentJobTask),
	IsMandatory = true,
	AllowsMultipleInstances = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("DOD", StmPrintJobQueueSchema.Constants.TableName, new[] { StmPrintJobQueueSchema.Constants.SPQ_EDocsProcessed + "=0" }, "Document Delivery - EDocsProcessed = '0'")]

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	/// <summary>
	/// Service task to processes print jobs with eDocsProcessed not true.
	/// </summary>
	/// <remarks>Other print jobs should be processed by other service tasks.</remarks>
	class DocumentJobTask : PrintJobTaskCore
	{
		public override void RunTask(CancellationToken token)
		{
			if (RunTaskForJobTypes(requiresPrintServer: false, token))
			{
				CleanupStmDeliveryGroup(isProcessed: true);
			}
		}

		protected override PrintJobManager GetPrintJobManagerCore()
		{
			return new EDocPrintJobManager();
		}

		/// <summary>
		/// Specifies if selected print jobs should be locked in processed only if locks were acquired.
		/// </summary>
		/// <remarks>
		/// Locking is done with SP_SB_DeliveryGroup and SP_ParentGuid fields.
		/// If any error occur:
		/// Please check value AllowsMultipleInstances or AllowsMultipleInstancesOnSameHost in Subclasses of PrintJobTaskCore.
		/// </remarks>
		internal override StmPrintJob[] TryToLockJobs(ICollection<StmPrintJob> printJobs, DisposableList mutexes, DbConnection connection)
		{
			try
			{
				var myJobs = new List<StmPrintJob>();

				foreach (var job in printJobs)
				{
					if (TryGetLockOnDeliveryGroupGuid(connection, job, out var deliveryGroupMutex))
					{
						mutexes.Add(deliveryGroupMutex);

						if (TryGetLockOnParentGuid(connection, job, out var parentGuidMutex))
						{
							mutexes.Add(parentGuidMutex);

							if (JobStillExistsAndAvailable(job))
							{
								myJobs.Add(job);
							}
						}
					}
				}

				return myJobs.OrderBy(job => job.SP_Group).ThenBy(job => job.SP_Sequence).ToArray();
			}
			// If our db connection is dropped then we lose all our locks. 
			// We should not process jobs unless we know we have locks on them.
			catch (SqlException sqlException)
			{
				ErrorReporter.ReportOnce("DB Connection error while grabbing print job mutexes", sqlException);

				return Array.Empty<StmPrintJob>();
			}
		}

		internal static bool TryGetLockOnParentGuid(DbConnection connection, StmPrintJob job, out SqlApplicationLock appLock)
		{
			var key = (NoResString)"PrintJobTaskCore:" + (job.SP_ParentGuid.IsValid ? job.SP_ParentGuid : job.PK).ToString().ToUpperInvariant();
			return connection.TryGetLock(key, out appLock);
		}

		protected override bool JobStillExistsAndAvailable(StmPrintJob job)
		{
			var reloadedJob = ReloadJobFromDb(job);

			return reloadedJob != null
				&& !reloadedJob.SP_EDocsProcessed
				&& reloadedJob.Factory.ExistsInDatabase(StmPrintJobQueueSchema.Constants.TableName, new ZQuery(StmPrintJobQueueSchema.SPQ_SP_PrintJob, reloadedJob.PK));
		}

		protected internal override bool TryGetJobsOfGivenTypesQuery(PrintJobType[] jobTypes, ZGuid? printServerPK, out ZNonPersistentDataQuery query)
		{
			const string sqlText = "dbo.GetNextPrintJobForSaveToEDocs";
			query = new ZNonPersistentDataQuery(sqlText);

			return true;
		}

		#region Delete Statements

		internal static void CleanupStmDeliveryGroup(bool isProcessed)
		{
			const int topRowCount = 1000;

			// The first NOT EXISTS subQuery is for deleted rows and the second one if updated rows : SP_SB_DeliveryGroup from NULL to VALUE
			var sql = string.Format(CultureInfo.InvariantCulture, @"
DELETE TOP ({0})
	dbo.StmDeliveryGroup WITH (READPAST, READCOMMITTEDLOCK)
WHERE 1=1
	AND SB_IsProcessed = @isProcessed
	AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmPrintJob
		WHERE SP_SB_DeliveryGroup = SB_PK
			AND SP_SB_DeliveryGroup is NOT NULL
	)
	AND NOT EXISTS
	(
		SELECT NULL
		FROM dbo.StmPrintJob WITH (NOLOCK)
		WHERE SP_SB_DeliveryGroup = SB_PK
			AND SP_SB_DeliveryGroup is NOT NULL
	)
"
				, topRowCount);

			if (!isProcessed)
			{
				sql += @"
	AND SB_SystemCreateTimeUtc < CONVERT(smalldatetime, dateadd(day, -1, getutcdate()))
";
			}

			try
			{
				if (Db.Connection.TryGetLock("DeleteDeliveryGroups", out var deliveryGroupsMutex))
				{
					using (deliveryGroupsMutex)
					using (var cmd = Db.Connection.Command(sql))
					{
						cmd.AddParameterBasedOnDbColumn("@isProcessed", isProcessed, StmDeliveryGroupSchema.SB_IsProcessed);

						var retryPolicy = new RetryPolicy<ErrorDetectionStrategy>(new FixedInterval(3));
						var result = topRowCount;

						while (result == topRowCount)
						{
							result = retryPolicy.ExecuteAction(() => cmd.ExecuteNonQuery());
						}
					}
				}
			}
			catch (SqlException ex)
			{
				ErrorReporter.ReportOnce("StmDeliveryGroupDeleteFailedDuringCleanup", "StmDeliveryGroup delete failed during cleanup.", ex);
			}
		}

		#endregion
	}

	class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
	{
		public bool IsTransient(Exception ex)
		{
			return ex is SqlException;
		}
	}
}
