using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingDependency
{
	public interface IAccountingDependencyFactory
	{
		ITransactionQRCodeDataProvider GetTransactionQRCodeDataProvider(InvoicingBase invoice);
		ILabelTranslator GetCountrySpecificLabelTranslator();
		IProcessor GetJobCostQueuePoster(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries);
		IProcessor GetJobRevenueQueuePoster(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries);
		IBranchLevelPostingHelper GetBranchLevelPostingHelper();
		ISingleActionPerTransactionOnDifferentLevels GetSingleActionPerTransactionOnDifferentLevels();
		IJobCostingPlugInHelpers GetJobCostingPlugInHelpers();
		IWithholdingJournalCreationManager GetWithholdingJournalCreationManager();
		ICollectionBatchFileGeneratorProvider GetCollectionBatchFileGeneratorProvider(AccCollectionBatch accCollectionBatch, IEnvironment environment);
	}
}
