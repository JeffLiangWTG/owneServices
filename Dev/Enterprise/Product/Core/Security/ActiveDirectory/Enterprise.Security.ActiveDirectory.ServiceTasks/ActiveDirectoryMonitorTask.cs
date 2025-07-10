using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory.ServiceTasks;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ActiveDirectoryMonitorTask.Code,
	ActiveDirectoryMonitorTask.Description,
	"SYS",
	typeof(ActiveDirectoryMonitorTask),
	CanRunInAnyBranch = true,
	IsReadOnlyForWiseCloudClient = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "3hours",
	DefaultScheduleRandomStartOffset = "60minutes"
	)]
namespace Enterprise.Security.ActiveDirectory.ServiceTasks
{
	public class ActiveDirectoryMonitorTask : ServiceProviderImpl
	{
		public const string Code = "ADN";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Active Directory Monitor";
		const string LastProcessedMonitorPK = "LastProcessedMonitorPK";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log String")]
		public const string ServiceNotRequiredMessage = "This service task is not required to run for self-hosted system.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log String")]
		public const string ActiveDirectoryIsDisabledMessage = "This service task is not required to run for non-AD Integrated system.";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log String")]
		public const string AttributeNotSyncedMessage = "This service task is not required to run because GS_IsActive attribute is not synced.";

		[HostedServiceRequirement]
		public static string CheckIsHostedWithCargoWise() => (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI) ? string.Empty : ServiceNotRequiredMessage;

		[HostedServiceRequirement]
		public static string CheckActiveDirectoryIsDisabled() => ActiveDirectoryRegistry.Instance.IsIntegrationEnabled ? string.Empty : ActiveDirectoryIsDisabledMessage;

		[HostedServiceRequirement]
		public static string CheckAttributeGS_IsActiveNotSynced() => AttributeMap.Current.IsSynced(GlbStaffSchema.GS_IsActive) ? string.Empty : AttributeNotSyncedMessage;

		public override void RunTask(CancellationToken token)
		{
			Logger.Log(LogType.Information, Description + " started on PID " + Process.GetCurrentProcess().Id);
			Run(token);
			Logger.Log(LogType.Information, Description + " finished");
		}

		void Run(CancellationToken token)
		{
			if (ADMonitor == null)
			{
				token.ThrowIfCancellationRequested();
				ADMonitor = new ADMonitor(new BusinessObjectFactory { NameForDebugging = nameof(ActiveDirectoryMonitorTask) });
			}

			var lastProcessedPKString = DataUtils.LoadDbExtendedProperty(Db.Connection, LastProcessedMonitorPK);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var glbStaffCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, GlbStaffSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(chunkSize: 1000, glbStaffCount, lastProcessedPK);

			foreach (var chunk in chunks)
			{
				var stopWatch = Stopwatch.StartNew();
				try
				{
					ADMonitor.CheckSynchronisedStatusInRange(chunk.LowerBound, chunk.UpperBound);

					if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
					{
						DataUtils.SaveDbExtendedProperty(Db.Connection, LastProcessedMonitorPK, chunk.UpperBound.ToString());

						token.ThrowIfCancellationRequested();
						stopWatch.Restart();
					}
				}
				catch (Exception ex)
				{
					if (ex is NoDomainPrivilegeException ||
						ex is DirectoryServicesCOMException ||
						ex is ActiveDirectoryUserException)
					{
						Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error monitoring AD objects:{0}{0}{1}", System.Environment.NewLine, ex.Message));
					}

					if (ex is COMException)
					{
						Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error communicating with domain:{0}{0}{1}", System.Environment.NewLine, ex.Message));
					}

					if (ex is InvalidOUException invalidOUException || (ex is DirectoryServicesException && (invalidOUException = ex.Find<InvalidOUException>()) != null))
					{
						Logger.Log(LogType.Error, string.Format(
						CultureInfo.InvariantCulture,
						"The Organizational Unit {0} is invalid, please review the setting in Registry items: {1} - cannot run {2} task.",
						invalidOUException.InvalidOUPath,
						((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual,
						Code
						));
					}

					if (ex is UnauthorizedAccessException unauthorizedAccessException || (ex is DirectoryServicesException && (unauthorizedAccessException = ex.Find<UnauthorizedAccessException>()) != null))
					{
						throw new HostedServiceException("Error getting AD information. Please ensure the user account the service host is running on has appropriate permissions.", ex);
					}
				}
			}

			DataUtils.DropDbExtendedProperty(Db.Connection, LastProcessedMonitorPK);
		}

		public IADMonitor ADMonitor { get; set; }

		public ILogger Logger
		{
			get { return logger ?? (logger = ServiceLogger); }
			set { logger = value; }
		}
		ILogger logger;
	}
}
