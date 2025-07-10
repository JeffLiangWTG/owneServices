using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Client.EDI.WTGADUserSynchronization.SynchronizeEDIStaffToActiveDirectoryTask;
using SynchronizeEDIStaffToActiveDirectoryTask = Enterprise.Client.EDI.WTGADUserSynchronization.SynchronizeEDIStaffToActiveDirectoryTask;

[assembly: HostedService(
	Code, 
	Description, 
	"SYS", 
	typeof(SynchronizeEDIStaffToActiveDirectoryTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "6Hours",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Code, GlbStaffSchema.Constants.TableName, new[] { GlbStaffSchema.Constants.GS_IsSystemAccount + "=N", GlbStaffSchema.Constants.GS_IsResource + "=N", GlbStaffSchema.Constants.GS_IsActive + "=Y" }, null)]

namespace Enterprise.Client.EDI.WTGADUserSynchronization
{
	public class SynchronizeEDIStaffToActiveDirectoryTask : ServiceProviderImpl, IServiceTaskConfigurationUser
	{
		public SynchronizeEDIStaffToActiveDirectoryTask()
		{
		}

		internal SynchronizeEDIStaffToActiveDirectoryTask(IDirectorySearcher directorySearcher)
		{
			this.directorySearcher = directorySearcher;
		}

		public const string Code = "ADE";
		public const string Description = "Synchronize EDI Staffs to Active Directory";

		bool CanRunServiceTask => EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.Value.IsEnabled && !EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup.Value.IsNullOrEmpty();

		public override void RunTask(CancellationToken token)
		{
			if (CanRunServiceTask)
			{
				Logger.Log(LogType.Information, Description + " started on PID " + Process.GetCurrentProcess().Id);
				Run(token);
				Logger.Log(LogType.Information, Description + " finished");
			}
			else
			{
				Logger.Log(LogType.Information, string.Format(
					CultureInfo.InvariantCulture,
					"{0} not started, WTG Active Directory Credentials is disabled or the security group value in Registry item {1} is empty.",
					Description,
					((IMultilingualRegistryItem)EDIDataRegistry.Instance.WTGWiseCloudAccessSecurityGroup).LocationMultilingual)
				);
			}
		}

		void Run(CancellationToken token)
		{
			try
			{
				var synchroniser = new WTGADUserSynchronizer(DirectorySearcher, Logger, token);

				synchroniser.ProgressUpdated += (sender, e) =>
				{
					var logMessage = GetLogMessage(e);
					if (!string.IsNullOrEmpty(logMessage))
					{
						logger.Log(e.LogType, logMessage);
					}
				};

				synchroniser.Synchronise();

				RecordLastSuccessfulSyncTime();
			}
			catch (NoDomainPrivilegeException ex)
			{
				Logger.Log(LogType.Error, string.Format(
					CultureInfo.InvariantCulture,
					"Domain user does not have read and write privileges in the Organizational Units set in Registry items: {0} - cannot run {1} task.\r\nError Details: {2}",
					((IMultilingualRegistryItem)EDIDataRegistry.Instance.WTGActiveDirectoryCredentials).LocationMultilingual,
					Code,
					ex.ToString()
				));
			}
			catch (Exception ex) when (ex is InvalidOUException invalidOUException || (ex is DirectoryServicesException && (invalidOUException = ex.Find<InvalidOUException>()) != null))
			{
				Logger.Log(LogType.Error, string.Format(
					CultureInfo.InvariantCulture,
					"The Organizational Unit {0} is invalid, please review the setting in Registry items: {1} - cannot run {2} task.",
					invalidOUException.InvalidOUPath,
					((IMultilingualRegistryItem)EDIDataRegistry.Instance.WTGActiveDirectoryCredentials).LocationMultilingual,
					Code
				));
			}
			catch (DirectoryServicesCOMException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error synchronizing directory objects:{0}{0}{1}", System.Environment.NewLine, ex.ToString()));
			}
			catch (COMException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error communicating with domain:{0}{0}{1}", System.Environment.NewLine, ex.Message));
			}
			catch (ActiveDirectoryUserException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error synchronizing directory objects:{0}{0}{1}", System.Environment.NewLine, ex.Message));
			}
			catch (ZSaveException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error saving after synchronise:{0}{0}{1}", System.Environment.NewLine, ex.ToString()));
			}
			catch (UnauthorizedAccessException ex)
			{
				throw new HostedServiceException("Error saving after synchronise. Please ensure the user account the service host is running on has appropriate read/write permissions.", ex);
			}
			catch (ZCannotSaveException ex)
			{
				Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error synchronizing directory objects:{0}{0}{1}", System.Environment.NewLine, ex.Message));
			}
		}

		void RecordLastSuccessfulSyncTime() => EDIDataRegistry.Instance.LastSuccessfulSyncForEDIUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());

		IDirectorySearcher DirectorySearcher => directorySearcher ?? (directorySearcher = GetDirectorySearcher());
		IDirectorySearcher directorySearcher;

		IDirectorySearcher GetDirectorySearcher()
		{
			var domainCredentials = EDIDataRegistry.Instance.WTGActiveDirectoryCredentials.Value;
			var directorySearcher = new DirectorySearcherWrapper(domainCredentials.DomainUserName, domainCredentials.DomainUserPassword, domainCredentials.DomainName);
			return directorySearcher;
		}

		static string GetLogMessage(SyncProgressEventArgs updateInfo)
		{
			var message = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(updateInfo.TaskName))
			{
				message.Append(updateInfo.TaskName);
			}
			else
			{
				if (updateInfo.OverallPercentComplete.HasValue)
				{
					message.Append(updateInfo.OverallPercentComplete.Value + "%");
				}
				if (!string.IsNullOrWhiteSpace(updateInfo.TaskDetails))
				{
					message.Append(" - " + updateInfo.TaskDetails);
				}
			}
			return message.ToString();
		}

		string IServiceTaskConfigurationUser.ConfigString { get; set; }

		public ILogger Logger => logger ?? (logger = ServiceLogger);
		ILogger logger;
	}
}
