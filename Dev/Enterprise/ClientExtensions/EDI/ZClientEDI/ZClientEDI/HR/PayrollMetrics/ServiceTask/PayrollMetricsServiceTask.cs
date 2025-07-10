using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"PML",
	"Payroll Metrics Leave Sync",
	"CSP",
	typeof(Enterprise.Client.EDI.HR.PayrollMetrics.PayrollMetricsServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = false,
	MinimumPeriod = "1Hours",
	DefaultScheduleRunEvery = "1hour")
]

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	public class PayrollMetricsServiceTask : ServiceProviderImpl
	{
		#region Factory

		protected virtual BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;

		#endregion

		public override void RunTask(CancellationToken token)
		{
			try
			{
				var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					HRLogger = new HRSystemLogger(ServiceLogger, "PayrollMetrics");
					if (!EDIDataRegistry.Instance.AllowPayrollMetricsServiceTask.Value || !RegistryIsConfigured())
					{
						return;
					}

					const int MaxTries = 3;
					for (int tryCount = 1; tryCount <= MaxTries; ++tryCount)
					{
						try
						{
							RunOnce(token);
							break;
						}
						catch (HttpRequestException ex)
						{
							var logType = tryCount < MaxTries ? LogType.Information : LogType.Warning;
							HRLogger.Log(logType, "", ex);
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var aggregateEx = ex as AggregateException;
				bool isTaskCanceled = ex is TaskCanceledException
					|| ex is OperationCanceledException
					|| (aggregateEx != null && (aggregateEx.InnerException is TaskCanceledException || aggregateEx.InnerException is OperationCanceledException));

				if (!isTaskCanceled)
				{
					HRLogger.Log(LogType.Error, ex.Message, ex);
				}

				throw;
			}
			finally
			{
				HRLogger.SendNotificationEmail();
			}
		}

		void RunOnce(CancellationToken token)
		{
			var utcNowInWholeSeconds = ZDateTime.UtcNow.ToDateTime();
			utcNowInWholeSeconds = utcNowInWholeSeconds.Date.AddSeconds((int)utcNowInWholeSeconds.TimeOfDay.TotalSeconds);

			var lastSyncUtc = EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.Value;
			if (lastSyncUtc == DateTime.MinValue)
			{
				lastSyncUtc = utcNowInWholeSeconds.AddDays(-1);
			}

			DateTime fromUtc = lastSyncUtc.AddMinutes(-15);
			DateTime toUtc = utcNowInWholeSeconds.AddMinutes(15);
			DateTime apiFrom = ConvertToPayrollMetricsTime(fromUtc);
			DateTime apiTo = ConvertToPayrollMetricsTime(toUtc);

			var sync = CreateSynchronizer();

			using (var client = CreateClient())
			{
				foreach (var payrollName in EDIDataRegistry.Instance.PayrollMetricsPayrollNames.Value)
				{
					token.ThrowIfCancellationRequested();

					var param = new EssLeaveRequestStatusModifiedApiParam()
					{
						PayrollName = payrollName,
						StatusModifiedFrom = apiFrom,
						StatusModifiedTo = apiTo
					};

					HRLogger.Log(LogType.Information, "Payroll " + payrollName +
						" from " + new ZDateTime(param.StatusModifiedFrom).ToISO8601String() +
						" to " + new ZDateTime(param.StatusModifiedTo).ToISO8601String());

					var response = client.GetEssLeaveRequestStatusModified(param);
					if (response.Status == "0")
					{
						sync.Process(response.Output);
					}
					else
					{
						HRLogger.Log(LogType.Warning, FormattableString.Invariant($@"Payroll Metrics web service returned: {string.Join("\r\n", response.ErrorMessages)}. Request json: {response.RequestJson}"));
						return;
					}
				}
			}

			EDIDataRegistry.Instance.PayrollMetricsLastLeaveSyncUtc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, utcNowInWholeSeconds);
		}

		IPayrollMetricsWebServiceClient CreateClient()
		{
			if (Globals.IsTest && ClientForTest != null)
			{
				return ClientForTest;
			}
			return new PayrollMetricsWebServiceClient();
		}

		ILeaveSynchronizer CreateSynchronizer()
		{
			if (Globals.IsTest && LeaveSynchronizerForTest != null)
			{
				return LeaveSynchronizerForTest;
			}
			return new LeaveSynchronizer(null, HRLogger);
		}

		internal IPayrollMetricsWebServiceClient ClientForTest;
		internal ILeaveSynchronizer LeaveSynchronizerForTest;

		static DateTime ConvertToPayrollMetricsTime(DateTime utc)
		{
			const string TimeZoneId = "AUS Eastern Standard Time";
			var timeZone = SafeGetTimeZone(TimeZoneId);
			if (timeZone != null)
			{
				return System.TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
			}
			else
			{
				return utc.AddHours(10);
			}
		}

		static System.TimeZoneInfo SafeGetTimeZone(string id)
		{
			try
			{
				return System.TimeZoneInfo.FindSystemTimeZoneById(id);
			}
			catch (TimeZoneNotFoundException) { }
			catch (InvalidTimeZoneException) { }
			return null;
		}

		bool RegistryIsConfigured()
		{
			bool ok = true;
			if (string.IsNullOrEmpty(EDIDataRegistry.Instance.PayrollMetricsServiceUri.Value))
			{
				HRLogger.Log(Integration.LogType.Warning, "Registry setting blank: " + FullName(EDIDataRegistry.Instance.PayrollMetricsServiceUri));
				ok = false;
			}

			if (EDIDataRegistry.Instance.PayrollMetricsLeaveTypes.Value.Count == 0)
			{
				HRLogger.Log(Integration.LogType.Warning, "Registry is empty: " + FullName(EDIDataRegistry.Instance.PayrollMetricsLeaveTypes));
				ok = false;
			}

			return ok;
		}

		static string FullName(IRegistryItem regItem)
		{
			return string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;
		}

		HRSystemLogger HRLogger;
	}
}
