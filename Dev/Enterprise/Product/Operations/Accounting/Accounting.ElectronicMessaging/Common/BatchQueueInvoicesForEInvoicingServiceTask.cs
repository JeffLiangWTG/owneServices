using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	BatchQueueInvoicesForEInvoicingServiceTask.Code,
	"Batch Queue Invoice For EInvoicing Service Task",
	"ACC",
	typeof(BatchQueueInvoicesForEInvoicingServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = false,
	IsScheduleReadOnly = false,
	MinimumPeriod = "20minutes",
	DefaultScheduleRunEvery = "1hour",
	AllowsMultipleInstances = false
	)]
namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class BatchQueueInvoicesForEInvoicingServiceTask : ServiceProviderImpl
	{
		public const string Code = "BQI";

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var mainFactory = CreateNewFactoryWithRefreshDisabled();
			var companyToProcess = GetPKsOfCompaniesThatRequireBatchQueue(mainFactory);

			var capacity = BatchSize;
			foreach (var companyWithDate in companyToProcess)
			{
				youMustReactToThisToken.ThrowIfCancellationRequested();

				if (capacity <= 0)
				{
					break;
				}

				var companyFactory = CreateNewFactoryWithRefreshDisabled();
				var invoices = CollectTransactionsForEInvoicing(companyFactory, companyWithDate.Company, companyWithDate.StartDate, capacity);

				if (invoices.Any())
				{
					ServiceLogger.Log(LogType.Information, $"Starting queue {invoices.Count()} transactions for {companyWithDate.Company.CompanyName}.");
					invoices.ForEach(x => new ElectronicInvoicingTransactionProxy(x).CreateNewPivot());
					companyFactory.Save();

					capacity -= invoices.Count();
				}
				else
				{
					AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.SetValue(companyWithDate.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
				}
			}

			ServiceLogger.Log(LogType.Debug, "Successfully batch queued transactions for eligible companies.");
		}

		protected IEnumerable<AccTransactionHeader> CollectTransactionsForEInvoicing(BusinessObjectFactory factory, GlbCompany company, DateTime startDate, int batchSize)
		{
			var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(company.GC_RN_NKCountryCode);

			if (countryFactory is not IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider> instanceProvider)
			{
				return new List<AccTransactionHeader>();
			}

			var provider = instanceProvider.Get();

			var invoicesToBeQueued = provider.GetTransactionsToBeQueued(factory, company.PK, startDate, batchSize);

			var invalidTransactions = invoicesToBeQueued.Where(x => !new ElectronicInvoicingTransactionProxy(x).IsEligibleToCreatePivot());
			if (invalidTransactions.Any())
			{
				var invalidTransaction = invalidTransactions.First();
				var hintMessage = $"Some transactions to be queued are not eligible for e-Invoicing. Etc, the one with transaction number {invalidTransaction.AH_TransactionNum}.";
				ServiceLogger.Log(LogType.Warning, $"{hintMessage} Please raise a support incident with CargoWise.");
				ExceptionReporter.Instance.ReportDeveloperException($"{hintMessage} Company code {company.GC_Code}.", null);
			}

			return invoicesToBeQueued.Except(invalidTransactions);
		}

		IReadOnlyCollection<(GlbCompany Company, DateTime StartDate)> GetPKsOfCompaniesThatRequireBatchQueue(BusinessObjectFactory factory)
		{
			var query = new ZQuery(GlbCompanySchema.GC_IsActive, true);
			query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, ApplicableCountryList);

			var countrySpecificCompanies = factory.Load<GlbCompany>(query);
			return countrySpecificCompanies
						.Where(c => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(c.PK.ToGuid(), Guid.Empty, Guid.Empty))
						.Select(c => (c, AccountingElectronicMessagingRegistry.Instance.QueueOldTransactionsFromDateReceivables.GetValueWithoutFallback(c.PK.ToGuid(), Guid.Empty, Guid.Empty)))
						.Where(o => o.Item2 != DateTime.MinValue)
						.ToHashSet();
		}

		BusinessObjectFactory CreateNewFactoryWithRefreshDisabled() => new BusinessObjectFactory() { RefreshEnabled = false };

		string[] ApplicableCountryList => QueueOldTransactionsForEInvoicingCountryHelper.GetQueueOldTransactionsCountryCodes().ToArray();

		int BatchSize
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && BatchSize_ForTestOnly.HasValue)
				{
					return BatchSize_ForTestOnly.Value;
				}
#endif
				return 100;
			}
		}

#if DEBUG
		public int? BatchSize_ForTestOnly;
#endif
	}
}
