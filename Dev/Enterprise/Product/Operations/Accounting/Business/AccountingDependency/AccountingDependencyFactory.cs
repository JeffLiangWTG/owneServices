using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.QRCodeData;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.AccountingDependency
{
	public class AccountingDependencyFactory : IAccountingDependencyFactory
	{
		ILabelTranslator IAccountingDependencyFactory.GetCountrySpecificLabelTranslator()
		{
			return new CountrySpecificLabelTranslator();
		}

		ITransactionQRCodeDataProvider IAccountingDependencyFactory.GetTransactionQRCodeDataProvider(InvoicingBase invoice)
		{
			return new TransactionQRCodeDataProvider(invoice);
		}

		IProcessor IAccountingDependencyFactory.GetJobCostQueuePoster(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries)
		{
			return new JobCostQueuePoster(plugIn, jobCharges, queueEntries);
		}

		IProcessor IAccountingDependencyFactory.GetJobRevenueQueuePoster(IJobInvoicingPlugIn plugIn, IEnumerable<Charge> jobCharges, IEnumerable<IJobChargePostingQueue> queueEntries)
		{
			return new JobRevenueQueuePoster(plugIn, jobCharges, queueEntries);
		}

		IBranchLevelPostingHelper IAccountingDependencyFactory.GetBranchLevelPostingHelper()
		{
			return new BranchLevelPostingHelper();
		}

		ISingleActionPerTransactionOnDifferentLevels IAccountingDependencyFactory.GetSingleActionPerTransactionOnDifferentLevels()
		{
			return new SingleActionPerTransactionOnDifferentLevels();
		}

		IJobCostingPlugInHelpers IAccountingDependencyFactory.GetJobCostingPlugInHelpers()
		{
			return new JobCostingPlugInHelpers();
		}

		IWithholdingJournalCreationManager IAccountingDependencyFactory.GetWithholdingJournalCreationManager()
		{
			return new WithholdingJournalCreationManager();
		}

		ICollectionBatchFileGeneratorProvider IAccountingDependencyFactory.GetCollectionBatchFileGeneratorProvider(AccCollectionBatch accCollectionBatch, IEnvironment environment)
		{
			switch (accCollectionBatch?.ACB_CollectionFileFormat)
			{
				case CollectionFileFormatList.Codes.itauBank:
					return new ITAFileGeneratorProvider(accCollectionBatch, environment);
			}
			return null;
		}
	}
}
