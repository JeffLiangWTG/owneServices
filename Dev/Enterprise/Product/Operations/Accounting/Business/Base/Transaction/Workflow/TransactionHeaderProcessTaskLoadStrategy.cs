using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.ARInvoiceCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					break;

				case WorkflowDescriptors.APInvoiceCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
					break;

				case WorkflowDescriptors.APReceiptWorkflowDescriptorCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
					break;
				case WorkflowDescriptors.ARReceiptWorkflowDescriptorCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					break;
				case WorkflowDescriptors.APPaymentWorkflowDescriptorCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
					break;
				case WorkflowDescriptors.ARPaymentWorkflowDescriptorCode:
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment);
					subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					break;
			}
		}

		Type IProcessTaskLoadStrategy.GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return typeof(TransactionHeaderProcessTask);
		}
	}
}
