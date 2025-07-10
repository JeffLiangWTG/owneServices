using System;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.FR.ServiceTasks.FRVATReportServiceTask.Code,
	Enterprise.Customs.FR.ServiceTasks.FRVATReportServiceTask.Description,
	"FRC",
	typeof(Enterprise.Customs.FR.ServiceTasks.FRVATReportServiceTask),
	MinimumPeriod = "1Day",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.France
	+ "," + Enterprise.Core.Constants.CountryCodes.FrenchGuyana
	+ "," + Enterprise.Core.Constants.CountryCodes.Guadeloupe
	+ "," + Enterprise.Core.Constants.CountryCodes.Martinique
	+ "," + Enterprise.Core.Constants.CountryCodes.Mayotte
	+ "," + Enterprise.Core.Constants.CountryCodes.Reunion
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintMartin
	+ "," + Enterprise.Core.Constants.CountryCodes.SaintBarthelemy,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1Day",
	DefaultScheduleStartAtLocal = "3Hours"
)]
namespace Enterprise.Customs.FR.ServiceTasks
{
	public class FRVATReportServiceTask : CustomsServiceTask
	{
		public const string Code = "FRV";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string Description = "FR Customs VAT Report Sender";

		protected override void RunTaskCore(CancellationToken token)
		{
			if (Today.Day > StandByDays)
			{
				var deadlineDate = GetDeadlineDate();
				if (deadlineDate >= Today)
				{
					var totalRunsThisSession = 0;
					var maxRunsPerSession = GetMaxRunsPerSession();
					foreach (var company in FrenchCompanies)
					{
						token.ThrowIfCancellationRequested();

						using (DisposableEnvironment.ForCompany(company.GC_Code))
						{
							if (Env.Instance.IsProductionSystem || FRCustomsDataRegistry.Instance.RunFRVATReportInUAT.Value)
							{
								if (ReportNotSendInThisMonth(company))
								{
									if (totalRunsThisSession < maxRunsPerSession || ThisIsTheLastSessionBeforeDeadline())
									{
										ExecuteReport(company);
										UpdateLastRunTime(company);
										totalRunsThisSession++;
									}
								}
							}
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Export")]
		protected virtual void ExecuteReport(GlbCompany company)
		{
			var factory = new BusinessObjectFactory();
			var allFees = VATReportSenderHelper.GetRecords(company.PK.ToGuid(), FirstDayOfThisMonth.AddMonths(-1).ToDateTime(), new ZDateTime(FirstDayOfThisMonth).AddSeconds(-1).ToDateTime()).ToList();

			var importers = allFees.Select(x => x.Key);
			foreach (var importerCode in importers)
			{
				var fees = allFees.First(x => x.Key == importerCode);

				var fileDisplayName = string.Format("VATReport_{0}.xls", importerCode);
				var filePathAndName = Path.Combine(Env.TempPath, string.Format("VATReport_{0}_{1}.xls", importerCode, ZDateTime.Now.Ticks));
				VATReportSenderHelper.ConvertReportToFile(fees, filePathAndName, company, factory);

				var importer = OrgHeader.LoadFromCode(factory, importerCode);
				var sender = new VATReportEmailSender(factory, Logger);
				sender.DoEverything(importer, company, filePathAndName, fileDisplayName);
			}
			factory.Save();
		}

		public static GlbCompany[] FrenchCompanies
		{
			get
			{
				var frenchDepartmentsAndTerritories = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.ToHashSet();
				return GlbCompany.GetActiveCompanies(x => frenchDepartmentsAndTerritories.Contains(x.GC_RN_NKCountryCode));
			}
		}

		ZInt GetMaxRunsPerSession()
		{
			var totalCompaniesCount = FrenchCompanies.Length;
			var totalBusinessDays = GetDeadlineDate().Day - StandByDays;
			var companiesPerDay = (ZInt)Math.Ceiling(totalCompaniesCount / (double)totalBusinessDays);
			return StaggeringFactor > 0 ? companiesPerDay * StaggeringFactor : totalCompaniesCount;
		}

		ZDate GetDeadlineDate()
		{
			var totalDaysInThisMonth = DateTime.DaysInMonth(Today.Year, Today.Month);
			var deadlineDay = DeadlineDay < totalDaysInThisMonth ? DeadlineDay : (ZInt)totalDaysInThisMonth;
			return new ZDate(Today.Year, Today.Month, deadlineDay);
		}

		ZBool ThisIsTheLastSessionBeforeDeadline()
		{
			var lastDayBeforeDeadline = GetDeadlineDate();
			return lastDayBeforeDeadline == Today;
		}

		ZBool ReportNotSendInThisMonth(GlbCompany company)
		{
			var lastRunTime = GetLastRunTime(company);
			return lastRunTime.IsEmpty || lastRunTime.Year < Today.Year || lastRunTime.Month < Today.Month;
		}

		protected void UpdateLastRunTime(GlbCompany company)
		{
			FRCustomsDataRegistry.Instance.FRVATReportLastRunTime.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Now.ToDateTime());
		}

		public ZDateTime GetLastRunTime(GlbCompany company) => FRCustomsDataRegistry.Instance.FRVATReportLastRunTime.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		protected ZInt StaggeringFactor => FRCustomsDataRegistry.Instance.FRVATReportStaggeringFactor.Value;
		protected ZInt DeadlineDay => FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.Value;
		protected virtual ZDateTime Now => ZDateTime.Now;
		protected ZDate Today => Now.Date;
		protected ZDate FirstDayOfThisMonth => new ZDate(Today.Year, Today.Month, 1);

		const int StandByDays = 2;
	}
}
