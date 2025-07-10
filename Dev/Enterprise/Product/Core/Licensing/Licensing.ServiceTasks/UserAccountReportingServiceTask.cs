using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Licensing.ServiceTasks;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	UserAccountReportingServiceTask.Code,
	UserAccountReportingServiceTask.Description,
	"SYS",
	typeof(UserAccountReportingServiceTask),
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Licensing.ServiceTasks
{
	public class UserAccountReportingServiceTask : ServiceProviderImpl
	{
		public const string Code = "UAR";
		public const string Description = "User Account Reporting Service";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, Description + " start.");
			try
			{
				if (!IsDeveloperEnvironment)
				{
					var lastRunTime = SystemDataRegistry.Instance.UserAccountLastReportTime.Value;
					IUserAccountReportSender sender = CreateUserAccountReportSender();
					var userAccountReport = sender.SendReport(lastRunTime);
					if (userAccountReport != null && !userAccountReport.StaffList.IsNullOrEmpty())
					{
						ServiceLogger.Log(LogType.Information, Description + $" {userAccountReport.StaffList.Count} staff info have been sent");
					}
					SystemDataRegistry.Instance.UserAccountLastReportTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
				}
				ServiceLogger.Log(LogType.Information, Description + " complete.");
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, Description + " process error ", e);
			}
		}

		protected virtual IUserAccountReportSender CreateUserAccountReportSender()
		{
			return new UserAccountReportSender();
		}

		protected virtual bool IsDeveloperEnvironment
		{
#if DEBUG
			get { return true; }
#else
			get { return false; }
#endif
		}
	}
}
