using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	GeneralLedgerDataBacklogQueueServiceTask.Code,
	"General Ledger Data Backlog Queue Service Task",
	"ACC",
	typeof(GeneralLedgerDataBacklogQueueServiceTask),
	ActiveByDefault = true,
	IsMandatory = true,
	AllowsMultipleInstances = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour")
]
namespace Enterprise.Accounting.ServiceTasks
{
	public class GeneralLedgerDataBacklogQueueServiceTask : ServiceProviderImpl
	{
		public const string Code = "GLQ";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Debug($"General ledger data backlog queuing process starting.");

			if (!AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value)
			{
				return;
			}

			ClearRegistryCache();

			var processedCount = 0;
			foreach (var gldDates in GetCompanyWithDates())
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();

				try
				{
					ServiceLogger.Debug($"Start to queue transactions of company '{gldDates.Company.GC_Code}'.");

					var lastQueuedJournalEntryDate = gldDates.LastQueuedJournalEntryDate == DateTime.MinValue
						? gldDates.CDCStartDate
						: gldDates.LastQueuedJournalEntryDate;
					var startDate = GetBatchStartDate(lastQueuedJournalEntryDate, gldDates.GenerateJournalEntriesStartDate);
					var endDate = gldDates.LastQueuedJournalEntryDate;
					var isFinal = false;
					while (!isFinal)
					{
						youMustReactToThisToken.ThrowIfCancellationRequested();

						if (startDate <= gldDates.GenerateJournalEntriesStartDate)
						{
							isFinal = true;
						}

						ServiceLogger.Debug($"Start to backlog queue to '{startDate.ToString(ZDateTime.ISO8601ShortDateFormat)}'.");
						Db.Connection.RunInTransaction(() =>
						{
							GeneralLedgerDataQueue.QueueTransaction(Db.Connection
								, companyPK: gldDates.Company.PK.ToGuid()
								, startDate: startDate
								, endDate: endDate
								, endSystemCreateTime: gldDates.CDCStartDate
								, isCheckDuplicated: false
							);

							AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(gldDates.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, startDate);
						});

						endDate = startDate;
						startDate = GetBatchStartDate(startDate, gldDates.GenerateJournalEntriesStartDate);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Error($"Fail to queue transactions of company '{gldDates.Company.GC_Code}' - {ex.Message}");
				}

				processedCount++;
				ServiceLogger.Debug($"Finish to queue transactions of company '{gldDates.Company.GC_Code}'.");
			}

			if (processedCount > 0)
			{
				ServiceLogger.Information($"General ledger data backlog queuing process completed for {processedCount} companies.");
			}

			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("GLP");

			ServiceLogger.Debug($"Service Task GLP has been nudged.");
		}

		void ClearRegistryCache()
		{
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.Inner.ClearCache();
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Inner.ClearCache();
		}

		DateTime GetBatchStartDate(DateTime lastQueuedJournalEntryDate, DateTime generateJournalEntriesStartDate)
		{
			var previousMonth = lastQueuedJournalEntryDate.AddMonths(lastQueuedJournalEntryDate.Day == 01 ? -1 : 0);

			var previousMonthFirstDay = new DateTime(previousMonth.Year, previousMonth.Month, 01);

			return previousMonthFirstDay < generateJournalEntriesStartDate
				? generateJournalEntriesStartDate
				: previousMonthFirstDay;
		}

		IEnumerable<GeneralLedgerDataDates> GetCompanyWithDates()
		{
			foreach (var company in AccountingUtils.GetAllActiveCompanies(Factory).OrderBy(x => x.GC_Code))
			{
				var journalEntriesStartDate = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (journalEntriesStartDate != DateTime.MinValue)
				{
					var lastQueuedJournalEntryDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (lastQueuedJournalEntryDate == DateTime.MinValue || journalEntriesStartDate < lastQueuedJournalEntryDate)
					{
						var cdcStartDate = AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
						yield return new GeneralLedgerDataDates(company, cdcStartDate, journalEntriesStartDate, lastQueuedJournalEntryDate);
					}
				}
			}
		}

		IGeneralLedgerDataQueue GeneralLedgerDataQueue => generalLedgerDataQueue ?? (generalLedgerDataQueue = ObjectFactory.Get<IGeneralLedgerDataQueue>());
		IGeneralLedgerDataQueue generalLedgerDataQueue;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}

	struct GeneralLedgerDataDates
	{
		public GeneralLedgerDataDates(GlbCompany company, DateTime cdcStartDate, DateTime generateJournalEntriesStartDate, DateTime lastQueuedJournalEntryDate)
		{
			Company = company;
			CDCStartDate = cdcStartDate;
			GenerateJournalEntriesStartDate = generateJournalEntriesStartDate;
			LastQueuedJournalEntryDate = lastQueuedJournalEntryDate;
		}

		public GlbCompany Company { get; }
		public DateTime GenerateJournalEntriesStartDate { get; }
		public DateTime CDCStartDate { get; }
		public DateTime LastQueuedJournalEntryDate { get; }
	}
}
