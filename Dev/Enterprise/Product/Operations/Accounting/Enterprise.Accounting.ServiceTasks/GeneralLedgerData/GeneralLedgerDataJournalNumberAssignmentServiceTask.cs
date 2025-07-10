using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
		GeneralLedgerDataJournalNumberAssignmentServiceTask.Code,
		"General Ledger Journal Number Assignment Service Task",
		"ACC",
		typeof(GeneralLedgerDataJournalNumberAssignmentServiceTask),
		IsMandatory = false,
		ActiveByDefault = true,
		AllowsMultipleInstances = false,
		CanRunInAnyBranch = true,
		MinimumPeriod = "6hour",
		DefaultScheduleRunEvery = "6hour")
]
namespace Enterprise.Accounting.ServiceTasks
{
	public class GeneralLedgerDataJournalNumberAssignmentServiceTask : ServiceProviderImpl
	{
		public const string Code = "GLN";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Information($"General Ledger Journal Number Assignment Starting.");

			foreach (var company in GetToBeHandledCompanies())
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();

				try
				{
					ServiceLogger.Information($"General Ledger Journal Number Assignment Process Start: {company.GC_Code}.");

					GeneralLedgerDataAssignJournalNumberHelper.AssignJournalNumberForGLD(company);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Error($"General Ledger Journal Number Assignment Handle Failed -> company : {company.GC_Code}, Error: {ex.Message}.\r\nStackTrace: {ex.StackTrace}");
				}
				finally
				{
					ServiceLogger.Information($"General Ledger Journal Number Assignment Process End: {company.GC_Code}.");
				}
			}

			ServiceLogger.Information($"General Ledger Journal Number Assignment End.");
		}

		IEnumerable<GlbCompany> GetToBeHandledCompanies()
		{
			var allActiveCompanies = AccountingUtils.GetAllActiveCompanies(Factory);
			foreach (var company in allActiveCompanies)
			{
				var journalEntriesNumberStartDate = AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var lastProcessdDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var journalEntriesNumberCustomisationRegistry = AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (lastProcessdDate > DateTime.MinValue && journalEntriesNumberStartDate > DateTime.MinValue && lastProcessdDate <= journalEntriesNumberStartDate && journalEntriesNumberCustomisationRegistry.AllocationOption == AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN)
				{
					yield return company;
				}
			}
		}

		GeneralLedgerDataAssignJournalNumberHelper GeneralLedgerDataAssignJournalNumberHelper => generalLedgerDataAssignJournalNumberHelper ?? (generalLedgerDataAssignJournalNumberHelper = new GeneralLedgerDataAssignJournalNumberHelper());
		GeneralLedgerDataAssignJournalNumberHelper generalLedgerDataAssignJournalNumberHelper;

		ReadOnlyBusinessObjectFactory Factory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;
	}
}
