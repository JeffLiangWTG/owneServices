using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ComplianceRisk.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Integration.Forwarding;

[assembly: HostedService(
	ComplianceJobEntityCacheServiceTask.Code,
	ComplianceJobEntityCacheServiceTask.Description,
	ComplianceJobEntityCacheServiceTask.Category,
	typeof(ComplianceJobEntityCacheServiceTask),
	IsMandatory = false,
	ActiveByDefault = true,
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "10minutes")]

namespace Enterprise.ComplianceRisk.ServiceTasks
{
	public class ComplianceJobEntityCacheServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal const string Description = "Compliance Job Entity Cache Transformation";
		internal const string Category = "CPW";
		public const string Code = "JEC";
		const int BatchSize = 50;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string ErrorKey = "An error occurred while attempting to create the compliance job entity cache during the transformation process.";

		/// <summary>
		/// Indicates whether the compliance job entity cache service task should be nudged (activated),
		/// based on whether job entity caching is currently enabled in the compliance risk helper settings.
		/// </summary>
		/// <returns>True if job entity caching is enabled; otherwise, false.</returns>
		[HostedServiceNudged]
		public static bool CheckIsNudged() => ComplianceRiskHelper.IsJobEntitiesCachingEnabled;

		/// <summary>
		/// Executes the compliance job entity cache synchronization task.
		/// Checks if job entity caching is enabled, then repeatedly processes batches of shipment, consolidation, and booking-with-quote jobs,
		/// updating their compliance job entity cache until no more jobs require processing or the operation is canceled.
		/// Logs the start and end of the synchronization, handles errors, and deactivates the service task upon completion.
		/// </summary>
		/// <param name="youMustReactToThisToken">A cancellation token to monitor for cancellation requests.</param>
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				if (!ComplianceRiskHelper.IsJobEntitiesCachingEnabled)
				{
					ServiceLogger?.Log(LogType.Information, (NoResString)"Registry: Compliance Job Entity Cache Synchronization is disabled.");
					return;
				}

				try
				{
					ServiceLogger?.Log(LogType.Information, (NoResString)"Started: Compliance Job Entity Cache Synchronization");

					var factoryProvider = new BusinessObjectFactoryProvider();
					var isProcessedBookingWithQuote = true;
					var isProcessedShipment = true;
					var isProcessedConsol = true;
					var isTransformationDone = false;

					do
					{
						isProcessedShipment = ProcessJobShipmentEntityCache(factoryProvider, youMustReactToThisToken);
						isProcessedConsol = ProcessJobConsolEntityCache(factoryProvider, youMustReactToThisToken);
						isProcessedBookingWithQuote = ProcessBookingWithQuoteEntityCache(factoryProvider, youMustReactToThisToken);
						isTransformationDone = !(isProcessedShipment || isProcessedConsol || isProcessedBookingWithQuote);
						youMustReactToThisToken.ThrowIfCancellationRequested();
					} while (!isTransformationDone);

					ServiceLogger?.Log(LogType.Information, (NoResString)"Finished: Compliance Job Entity Cache Synchronization");

					DeactivateSerivceTask(factoryProvider);

					ServiceLogger?.Log(LogType.Information, (NoResString)"Deactivated Service Task");
				}
				catch (Exception exception) when (!exception.IsCriticalException() && !(exception is OperationCanceledException))
				{
					ErrorReporter.ReportOnce(ErrorKey, exception);
					ServiceLogger?.Log(LogType.Error, ErrorKey, exception);
				}
			}
		}

		/// <summary>
		/// Deactivates the current service task by setting its active status to false
		/// using the service manager governor.
		/// </summary>
		/// <param name="factoryProvider">The business object factory provider (not used in this method).</param>
		void DeactivateSerivceTask(BusinessObjectFactoryProvider factoryProvider)
		{
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskIsActive(Code, isActive: false);
		}

		/// <summary>
		/// Synchronizes the compliance job entity cache for the specified set of compliance risk providers.
		/// For each provider, updates the entity cache to match the current in-memory state, respecting cancellation requests.
		/// After processing, saves all changes and reclaims memory using the provided factory provider.
		/// </summary>
		/// <param name="providers">The array of compliance item risk status providers to synchronize.</param>
		/// <param name="factoryProvider">The business object factory provider used for saving and memory management.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		void SynchronizeJobEntityCacheWithMemory(IComplianceItemRiskStatusProvider[] providers, BusinessObjectFactoryProvider factoryProvider, CancellationToken cancellationToken)
		{
			if (providers.Length > 0)
			{
				foreach (var complianceRiskPlugInBizO in ComplianceRiskPlugInBusinessObject.GetComplianceRiskPlugInBusinessObjects(providers))
				{
					cancellationToken.ThrowIfCancellationRequested();
					complianceRiskPlugInBizO.JobEntityCacheSyncCore(forceResynchronization: true);
				}

				factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
		}

		/// <summary>
		/// Loads all forwarding shipment jobs that require entity cache synchronization, based on their primary keys,
		/// and updates their compliance job entity cache to reflect the current in-memory state.
		/// </summary>
		/// <param name="factoryProvider">The business object factory provider used to load and save entities.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// <returns>True if any shipment jobs were processed; otherwise, false.</returns>
		bool ProcessJobShipmentEntityCache(BusinessObjectFactoryProvider factoryProvider, CancellationToken cancellationToken)
		{
			if (GetJobPrimaryKeys(JobShipmentSchema.Constants.Prefix, out var jobPKs))
			{
				var jobShipments = factoryProvider.Current
				.Load(ObjectFactory.GetType<IForwardingShipment>(),
					new ZQuery(JobShipmentSchema.PK, jobPKs) { IgnoreActiveFilter = true })
				.OfType<IComplianceItemRiskStatusProvider>()
				.ToArray();

				SynchronizeJobEntityCacheWithMemory(jobShipments, factoryProvider, cancellationToken);
			}

			return jobPKs.Count > 0;
		}

		/// <summary>
		/// Loads all forwarding consolidation jobs that require entity cache synchronization, based on their primary keys,
		/// and updates their compliance job entity cache to reflect the current in-memory state.
		/// </summary>
		/// <param name="factoryProvider">The business object factory provider used to load and save entities.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// /// <returns>True if any consolidation jobs were processed; otherwise, false.</returns>
		bool ProcessJobConsolEntityCache(BusinessObjectFactoryProvider factoryProvider, CancellationToken cancellationToken)
		{
			if (GetJobPrimaryKeys(JobConsolSchema.Constants.Prefix, out var jobPKs))
			{
				var jobConsols = factoryProvider.Current
					.Load(ObjectFactory.GetType<IForwardingConsol>(),
						new ZQuery(JobConsolSchema.PK, jobPKs) { IgnoreActiveFilter = true })
					.OfType<IComplianceItemRiskStatusProvider>()
					.ToArray();

				SynchronizeJobEntityCacheWithMemory(jobConsols, factoryProvider, cancellationToken);
			}

			return jobPKs.Count > 0;
		}

		/// <summary>
		/// Loads all booking-with-quote jobs that require entity cache synchronization, based on their primary keys,
		/// and updates their compliance job entity cache to reflect the current in-memory state.
		/// </summary>
		/// <param name="factoryProvider">The business object factory provider used to load and save entities.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		/// /// <returns>True if any booking-with-quote jobs were processed; otherwise, false.</returns>
		bool ProcessBookingWithQuoteEntityCache(BusinessObjectFactoryProvider factoryProvider, CancellationToken cancellationToken)
		{
			if (GetJobPrimaryKeys(RatingHeaderSchema.Constants.Prefix, out var jobPKs))
			{
				var jobQuotedBookings = factoryProvider.Current.
					Load(ObjectFactory.GetType<IViewQuotedBooking>(),
						new ZQuery(ViewQuotedBookingSchema.PK, jobPKs) { IgnoreActiveFilter = true })
					.OfType<IViewComplianceRiskStatusProvider>()
					.Select(booking => booking.GetProviderBusinessObject())
					.WhereNotNull()
					.ToArray();

				SynchronizeJobEntityCacheWithMemory(jobQuotedBookings, factoryProvider, cancellationToken);
			}

			return jobPKs.Count > 0;
		}

		/// <summary>
		/// Executes a SQL query to retrieve job primary keys for the specified table prefix.
		/// Parses the results into a list of <see cref="ZGuid"/> values, handling missing or invalid data gracefully.
		/// Returns <c>true</c> if any valid job primary keys are found; otherwise, returns <c>false</c>.
		/// </summary>
		/// <param name="tablePrefix">The table prefix identifying the job type.</param>
		/// <param name="jobPKs">Outputs the list of valid job primary keys found.</param>
		/// <returns><c>true</c> if one or more job primary keys are found; otherwise, <c>false</c>.</returns>

		bool GetJobPrimaryKeys(string tablePrefix, out List<ZGuid> jobPKs)
		{
			var sqlScript = GetSqlScript(tablePrefix);
			var result = DataUtils.GetDataTableFromQuery(Db.Connection, sqlScript);
			if (result == null || !result.Columns.Contains("JobPK"))
			{
				jobPKs = [];
			}
			else
			{
				jobPKs = result.Rows
					.Cast<DataRow>()
					.Select(row => ZGuid.TryParse(row["JobPK"]?.ToString(), out var guid) ? guid : ZGuid.Empty)
					.Where(guid => guid != ZGuid.Empty)
					.ToList();
			}
			return jobPKs.Count > 0;
		}

		/// <summary>
		/// Generates the SQL query used to retrieve job primary keys for entity cache synchronization,
		/// based on the specified table prefix. The query filters jobs by recent activity, parent table code,
		/// and ensures only jobs without an existing entity cache are selected. For booking-with-quote jobs,
		/// a join to <c>ViewQuotedBooking</c> is performed; for other jobs, a direct query on <c>ComplianceRiskStatus</c> is used.
		/// </summary>
		/// <param name="tablePrefix">The table prefix identifying the job type.</param>
		/// <returns>A SQL query string for selecting job primary keys requiring cache synchronization.</returns>
		string GetSqlScript(string tablePrefix)
		{
			string sqlScript;
			var jobEndDate = $@"
(COR_JobEndDate >= DATEADD(day, -{OrganisationsDataRegistry.Instance.ComplianceJobEndDateLimit.Value}, CAST(SYSDATETIMEOFFSET() AS DATE)))";

			if (tablePrefix == RatingHeaderSchema.Constants.Prefix)
			{
				sqlScript = $@"
SELECT TOP {BatchSize}
	VB_PK AS JobPK
FROM
	ComplianceRiskStatus CRS
	JOIN ViewQuotedBooking ON VB_TH = CRS.COR_ParentID AND COR_ParentTableCode = '{tablePrefix}'
	JOIN JobShipment ON JS_PK = VB_JS
WHERE
	{jobEndDate}
	AND JS_IsBooking = 1 AND JS_IsForwardRegistered = 0
	AND NOT EXISTS (
		SELECT 1
		FROM ComplianceJobEntityCache CJEC
		WHERE CJEC.CJE_COR_ComplianceRisk = CRS.COR_PK
	)";
			}
			else
			{
				sqlScript = $@"
SELECT TOP {BatchSize}
	COR_ParentID AS JobPK
FROM
	ComplianceRiskStatus CRS
WHERE
	{jobEndDate}
	AND COR_ParentTableCode = '{tablePrefix}'
	AND NOT EXISTS (
		SELECT 1
		FROM ComplianceJobEntityCache CJEC
		WHERE CJEC.CJE_COR_ComplianceRisk = CRS.COR_PK
	)";
			}

			return sqlScript;
		}
	}
}

