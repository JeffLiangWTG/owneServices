using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Accounting.Business.AccountingUtils;

[assembly: HostedService(
	PeriodClosureServiceTask.Code,
	"Period Closure Service Task",
	"ACC",
	typeof(PeriodClosureServiceTask),
	IsMandatory = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "15minutes",
	DefaultScheduleRunEvery = "1hour")
]

namespace Enterprise.Accounting.ServiceTasks
{
	public enum PeriodType
	{
		SubLedgerPeriod = 1,
		GLPeriod = 2,
		AdjustmentsSubLedgerPeriod = 3,
	}

	public class PeriodClosureServiceTask : ServiceProviderImpl
	{
		public const string Code = "PCS";

		string ErrorMessage;
		Period CurrentPeriod;
		PeriodType CurrentPeriodType;
		PeriodManager PeriodManager;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, "Period Closure starting.");
			try
			{
				foreach (var company in GetApplicableConfigurations())
				{
					using (DisposableEnvironment.ForCompany(company.GC_Code))
					{
						ServiceLogger.Log(LogType.Debug, $"Start processing company {GlbCompany.CurrentCompany.GC_Code}");

						PreparePeriodManager();

						CloseSubLedgerPeriod();
						CloseGLPeriod();
						CloseGLPeriodForAdjustments();

						ServiceLogger.Log(LogType.Debug, $"End processing company {GlbCompany.CurrentCompany.GC_Code}");
					}
				}
			}
			finally
			{
				ServiceLogger.Log(LogType.Information, "Period Closure completed.");
			}
		}

		protected void PreparePeriodManager()
		{
			PeriodManager = new PeriodManager(new BusinessObjectFactory());
			PeriodManager.OnCloseSubLedgerError += delegate(object sender, EventArgs e)
			{
				ErrorMessage = (string)sender;
			};
		}

		protected void CloseSubLedgerPeriod()
		{
			CurrentPeriodType = PeriodType.SubLedgerPeriod;
			CurrentPeriod = PeriodManager.NextUnClosedSubLedgerPeriod;

			var isPeriodClosed = true;
			while (isPeriodClosed
				&& CurrentPeriod != null
				&& ShouldClosePeriodByRegistry(CurrentPeriod.AM_EndDate))
			{
				PeriodManager.CloseSubLedgerPeriod();
				isPeriodClosed = CurrentPeriod.AM_IsSubLedgerClosed;
				Log(isPeriodClosed);

				CurrentPeriod = PeriodManager.NextUnClosedSubLedgerPeriod;
			}
		}

		protected void CloseGLPeriod()
		{
			CurrentPeriodType = PeriodType.GLPeriod;
			CurrentPeriod = PeriodManager.NextUnClosedGLPeriod;

			var isPeriodClosed = true;
			while (isPeriodClosed
				&& CurrentPeriod != null
				&& ShouldClosePeriodByRegistry(CurrentPeriod.AM_EndDate)
				&& (PeriodManager.NextUnClosedSubLedgerPeriod == null || CurrentPeriod.AM_Period < PeriodManager.NextUnClosedSubLedgerPeriod.AM_Period))
			{
				PeriodManager.CloseGLPeriod();
				isPeriodClosed = CurrentPeriod.AM_IsGeneralLedgerClosed;
				Log(isPeriodClosed);

				CurrentPeriod = PeriodManager.NextUnClosedGLPeriod;
			}
		}

		protected void CloseGLPeriodForAdjustments()
		{
			CurrentPeriodType = PeriodType.AdjustmentsSubLedgerPeriod;
			CurrentPeriod = PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod;

			var isPeriodClosed = true;
			while (isPeriodClosed
				&& CurrentPeriod != null
				&& ShouldClosePeriodByRegistry(CurrentPeriod.AM_EndDate)
				&& (PeriodManager.NextUnClosedGLPeriod == null || CurrentPeriod.AM_Period < PeriodManager.NextUnClosedGLPeriod.AM_Period))
			{
				PeriodManager.CloseGLPeriodForAdjustments();
				isPeriodClosed = CurrentPeriod.AM_IsSubledgerClosedForAdjustments;
				Log(isPeriodClosed);

				CurrentPeriod = PeriodManager.NextUnClosedForAdjustmentsSubLedgerPeriod;
			}
		}

		IEnumerable<GlbCompany> GetApplicableConfigurations()
		{
			var factory = new BusinessObjectFactory();
			var companies = AccountingUtils.GetAllActiveCompanies(factory);

			foreach (var company in companies)
			{
				if (IsEnableAutoPeriodClosureConfiguration(company.PK))
				{
					if (AccountingUtils.GetTopOneActiveBranchOfCompany(company.PK, factory) == null)
					{
						ServiceLogger.Log(LogType.Error, $"Cannot find an active branch for company {company.GC_Code}");
					}
					else if (IsSetAutoPeriodClosureNotifyGroup(company.PK))
					{
						yield return company;
					}
					else
					{
						ServiceLogger.Log(LogType.Error, $"Please add notify group for company {company.GC_Code} in {AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.Location()}");
					}
				}
			}
		}

		protected bool ShouldClosePeriodByRegistry(ZDateTime time)
		{
			int intervalValue = 0;
			ZDateTime timeSetting = time;

			switch (CurrentPeriodType)
			{
				case PeriodType.SubLedgerPeriod:
					intervalValue = AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.Value.SubLedgerInterval;
					break;
				case PeriodType.GLPeriod:
					intervalValue = AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.Value.GeneralLedgerInterval;
					break;
				case PeriodType.AdjustmentsSubLedgerPeriod:
					intervalValue = AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.Value.AdjustmentLedgerInterval;
					break;
			}

			switch (AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.Value.IntervalType)
			{
				case PeriodClosureConfigurationIntervalType.Minutes:
					timeSetting = time.AddMinutes(intervalValue);
					break;
				case PeriodClosureConfigurationIntervalType.Hours:
					timeSetting = time.AddHours(intervalValue);
					break;
				case PeriodClosureConfigurationIntervalType.Days:
					timeSetting = time.AddDays(intervalValue);
					break;
			}

			return intervalValue > 0 && ZDateTime.Now >= timeSetting;
		}

		protected void Log(bool isPeriodClosed)
		{
			if (!isPeriodClosed)
			{
				ServiceLogger.Log(LogType.Error, $"{GetPeriodTypeStr(CurrentPeriodType)} for Period {CurrentPeriod.AM_Period} of company {GlbCompany.CurrentCompany.GC_Code} cannot be auto closed. {ErrorMessage}");

				var subject = Res.GetString("053fbaee-facd-4a2a-95eb-6bfe4fb77a25", "{0} for Period {1} of company {2} cannot be auto closed.", GetPeriodTypeStr(CurrentPeriodType), CurrentPeriod.AM_Period, GlbCompany.CurrentCompany.GC_Code);
				new PeriodClosureEmailNotificationProcessorEmail(subject, ErrorMessage).Send();
			}
			else
			{
				ServiceLogger.Log(LogType.Debug, string.Format("{0} for Period {1} of company {2} has been auto closed.", GetPeriodTypeStr(CurrentPeriodType), CurrentPeriod.AM_Period, GlbCompany.CurrentCompany.GC_Code));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Logs should be in English Only")]
		protected string GetPeriodTypeStr(PeriodType periodType)
		{
			switch (periodType)
			{
				case PeriodType.SubLedgerPeriod:
					return "Sub Ledger";
				case PeriodType.GLPeriod:
					return "General Ledger";
				case PeriodType.AdjustmentsSubLedgerPeriod:
					return "Adjustment Ledger";
				default:
					return string.Empty;
			}
		}

		protected bool IsSetAutoPeriodClosureNotifyGroup(ZGuid companyPK)
		{
			return !AccountingConfigurationRegistry.Instance.AutoPeriodClosureNotifyGroup.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty).Equals(Guid.Empty);
		}

		protected bool IsEnableAutoPeriodClosureConfiguration(ZGuid companyPK)
		{
			var value = AccountingConfigurationRegistry.Instance.AutoPeriodClosureConfiguration.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			return !(value.SubLedgerInterval == 0 && value.GeneralLedgerInterval == 0 && value.AdjustmentLedgerInterval == 0);
		}
	}
}
