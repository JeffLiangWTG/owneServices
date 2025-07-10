using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	SystemLicenceService.Code,
	SystemLicenceService.Description,
	"SYS",
	typeof(SystemLicenceService),
	IsMandatory = true,
	MaximumPeriod = "5m",
	MinimumPeriod = "5m",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true
	)
]

namespace Enterprise.Licensing.ServiceTasks
{
	public class SystemLicenceService : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			if (!IsDeveloperEnvironment) // Ignore debug builds
			{
				var dateFromUtcInclusive = SystemDataRegistry.Instance.OnDemandLicenceUsageReportDate.Value;

				DateTime utcNow = ZDateTime.UtcNow.ToDateTime();
				var converter = new BillingTimeConverter();
				var dateFromInBillingTimeZone = converter.ConvertUtcToTimeInBillingTimeZone(dateFromUtcInclusive).Date;
				var billingNow = converter.ConvertUtcToTimeInBillingTimeZone(utcNow);
				var dateToInBillingTimeZone = billingNow.Date;

				if (dateFromInBillingTimeZone != dateToInBillingTimeZone
					&& IsTimeForThisDatabaseSoNotAllRunAtSameTime(billingNow))
				{
					if (dateFromUtcInclusive.Year == SystemDataRegistry.InitialLicenceUsageReportYear)
					{
						dateFromUtcInclusive = utcNow.AddHours(-48);
					}
					var dateToUtcExclusive = converter.ConvertTimeInBillingTimeZoneToUtc(dateToInBillingTimeZone).ToDateTime();

					var usageProcess = CreateLicenceUsageProcess();
					try
					{
						SendCurrentVersionReport(usageProcess, dateFromUtcInclusive, dateToUtcExclusive);
						ServiceLogger.Log(LogType.Information,
							dateFromUtcInclusive.ToString(JsonDateTimeFormat, CultureInfo.InvariantCulture)
							+ " - "
							+ utcNow.ToString(JsonDateTimeFormat, CultureInfo.InvariantCulture));
					}
					catch (SqlException ex)
					{
						if (new DbErrorMatch(ex).ExceptionType == DbErrorType.TimeoutExpired)
						{
							ServiceLogger.Log(LogType.Warning, dateFromUtcInclusive.ToString(JsonDateTimeFormat, CultureInfo.InvariantCulture) + " - retry next run", ex);
						}
						else
						{
							throw;
						}
					}
				}
			}
		}

		static internal bool IsTimeForThisDatabaseSoNotAllRunAtSameTime(ZDateTime billingNow)
		{
			const int StaggerRunTimeOverMinutes = 60;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var earliestMinuteInDayForThisDatabaseToRun = Math.Max(registrationKey.DatabaseNumber, 0) % StaggerRunTimeOverMinutes;
			var currentMinuteInDay = billingNow.Hour * 60 + billingNow.Minute;
			return currentMinuteInDay >= earliestMinuteInDayForThisDatabaseToRun;
		}

		public const string Code = "SLS";
		public const string Description = "System License Service";
		public const string JsonDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

		protected virtual ILicenceConsumptionLogProcess CreateLicenceUsageProcess()
		{
			return new LicenceConsumptionLogSenderProcess();
		}

		protected virtual void SendCurrentVersionReport(ILicenceConsumptionLogProcess usageProcess, DateTime dateFromUtcInclusive, DateTime dateToUtcExclusive)
		{
			var companyCode = DisposableEnvironment.GetActiveCompanies().OrderBy(x => x).FirstOrDefault() ?? GlbCompany.CurrentCompany.GC_Code.ToString();
			using (DisposableEnvironment.ForCompany(companyCode))
			{
				CreateVersionReportSender().SendCurrent(usageProcess.Execute(dateFromUtcInclusive, dateToUtcExclusive), false);
			}

			SystemDataRegistry.Instance.OnDemandLicenceUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateToUtcExclusive);
		}

		protected virtual IVersionReportSender CreateVersionReportSender()
		{
			return new VersionReportBuilderFactory();
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
