using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.Security.ActiveDirectory.ServiceTasks;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Constants.ActiveDirectorySynchronisationTask.Code,
	ActiveDirectorySynchronisationTask.Description,
	"SYS",
	typeof(ActiveDirectorySynchronisationTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours",
	ActiveByDefault = true
	)]

// Below predicate rules are taken from GetStaffToSynchroniseQuery and GetGroupsToSynchroniseQuery functions of EventSynchroniser.cs
[assembly: HostedServiceBusinessObjectBinding(Constants.ActiveDirectorySynchronisationTask.Code, GlbStaffSchema.Constants.TableName, new[] { GlbStaffSchema.Constants.GS_IsSystemAccount + "=N", GlbStaffSchema.Constants.GS_IsResource + "=N", GlbStaffSchema.Constants.GS_IsActive + "=Y" }, null)]
[assembly: HostedServiceBusinessObjectBinding(Constants.ActiveDirectorySynchronisationTask.Code, GlbStaffSchema.Constants.TableName, new[] { GlbStaffSchema.Constants.GS_IsSystemAccount + "=N", GlbStaffSchema.Constants.GS_IsResource + "=N", GlbStaffSchema.Constants.GS_ActiveDirectoryObjectGuid + "!=00000000-0000-0000-0000-000000000000" }, null)]
[assembly: HostedServiceBusinessObjectBinding(Constants.ActiveDirectorySynchronisationTask.Code, GlbGroupSchema.Constants.TableName, new[] { GlbGroupSchema.Constants.GG_IsSystemDefined + "=N", GlbGroupSchema.Constants.GG_IsActive + "=Y" }, null)]
[assembly: HostedServiceBusinessObjectBinding(Constants.ActiveDirectorySynchronisationTask.Code, GlbGroupSchema.Constants.TableName, new[] { GlbGroupSchema.Constants.GG_IsSystemDefined + "=N", GlbGroupSchema.Constants.GG_ActiveDirectoryObjectGuid + "!=00000000-0000-0000-0000-000000000000" }, null)]

namespace Enterprise.Security.ActiveDirectory.ServiceTasks
{
	public class ActiveDirectorySynchronisationTask : ServiceProviderImpl, IServiceTaskConfigurationUser
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Description")]
		public const string Description = "Active Directory Synchronizer";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log String")]
		public const string ActiveDirectoryIsDisabledMessage = Description + " not started, Active Directory integration is disabled";

		[HostedServiceRequirement]
		public static string CheckIsADIntegrationEnabled() => ActiveDirectoryRegistry.Instance.IsIntegrationEnabled ? string.Empty : ActiveDirectoryIsDisabledMessage;

		public override void RunTask(CancellationToken token)
		{
			if (ActiveDirectoryRegistry.Instance.IsIntegrationEnabled)
			{
				Logger.Log(LogType.Information, Description + " started on PID " + Process.GetCurrentProcess().Id);
				Run(token);
				Logger.Log(LogType.Information, Description + " finished");
			}
			else
			{
				Logger.Log(LogType.Information, ActiveDirectoryIsDisabledMessage);
			}
		}

		void Run(CancellationToken token)
		{
			var startTime = ZDateTime.UtcNow;
			try
			{
				if (Synchroniser == null)
				{
					token.ThrowIfCancellationRequested();
					Synchroniser = new EntitySynchroniser(new BusinessObjectFactory { NameForDebugging = nameof(ActiveDirectorySynchronisationTask) });
				}
				Synchroniser.ProgressUpdated += UpdateLog;
				Synchroniser.Synchronise(preferredSyncMode: ADConfig.GetSyncModeFromCode(ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value));
				Synchroniser.Save();
				ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				RecordLastSuccessfulSyncTime();
				RecordEntitiesWithSyncErrors();
			}
			catch (ZSaveException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error saving after synchronise:{0}{0}{1}", System.Environment.NewLine, ex.ToString()));
			}
			catch (Exception ex) when (ex is NoDomainPrivilegeException ||
					ex is DirectoryServicesCOMException ||
					ex is ActiveDirectoryUserException ||
					ex is ZCannotSaveException)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error synchronising directory objects:{0}{0}{1}", System.Environment.NewLine, ex.Message));
			}
			catch (COMException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error communicating with domain:{0}{0}{1}", System.Environment.NewLine, ex.Message));
			}
			catch (Exception ex) when (ex is InvalidOUException invalidOUException || (ex is DirectoryServicesException && (invalidOUException = ex.Find<InvalidOUException>()) != null))
			{
				Logger.Log(LogType.Error, string.Format(
				CultureInfo.InvariantCulture,
				"The Organizational Unit {0} is invalid, please review the setting in Registry items: {1} - cannot run {2} task.",
				invalidOUException.InvalidOUPath,
				((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.DomainCredentialsCollection).LocationMultilingual,
				Constants.ActiveDirectorySynchronisationTask.Code
				));
			}
			catch (Exception ex) when (ex is UnauthorizedAccessException unauthorizedAccessException || (ex is DirectoryServicesException && (unauthorizedAccessException = ex.Find<UnauthorizedAccessException>()) != null))
			{
				throw new HostedServiceException("Error saving after synchronise. Please ensure the user account the service host is running on has appropriate read/write permissions.", ex);
			}

			Synchroniser.ProgressUpdated -= UpdateLog;
			Synchroniser = null;

			void RecordLastSuccessfulSyncTime() => ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, startTime.ToDateTime());

			void RecordEntitiesWithSyncErrors()
			{
				if (Synchroniser?.EntitiesWithErrors == null)
				{
					return;
				}

				var failedStaff = new List<Guid>();
				var failedGroups = new List<Guid>();
				foreach (var entity in Synchroniser.EntitiesWithErrors)
				{
					if (entity is ADUser)
					{
						failedStaff.Add(((BusinessObject)entity.EnterpriseEntity).PK.ToGuid());
					}
					else if (entity is ADGroup)
					{
						failedGroups.Add(((BusinessObject)entity.EnterpriseEntity).PK.ToGuid());
					}
				}
				ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, failedStaff.Distinct().ToArray());
				ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, failedGroups.Distinct().ToArray());
			}
		}

		public void UpdateLog(object sender, SyncProgressEventArgs e)
		{
			var logMessage = GetLogMessage(e);
			if (!string.IsNullOrEmpty(logMessage))
			{
				Logger.Log(e.LogType, logMessage);
			}
		}

		static string GetLogMessage(SyncProgressEventArgs updateInfo)
		{
			if (string.IsNullOrEmpty(updateInfo.TaskName) && string.IsNullOrEmpty(updateInfo.TaskDetails))
			{
				return null;
			}
			var message = new StringBuilder();
			if (updateInfo.OverallPercentComplete.HasValue)
			{
				message.Append(updateInfo.OverallPercentComplete.Value + "% ");
			}
			message.Append(updateInfo.TaskName);
			if (!(string.IsNullOrWhiteSpace(updateInfo.TaskName) || string.IsNullOrWhiteSpace(updateInfo.TaskDetails)))
			{
				message.Append(" - ");
			}
			message.Append(updateInfo.TaskDetails);
			return message.ToString();
		}

		string IServiceTaskConfigurationUser.ConfigString { get; set; }

		public IEntitySynchroniser Synchroniser { get; set; }

		public ILogger Logger
		{
			get { return logger ?? (logger = ServiceLogger); }
			set { logger = value; }
		}
		ILogger logger;
	}
}
