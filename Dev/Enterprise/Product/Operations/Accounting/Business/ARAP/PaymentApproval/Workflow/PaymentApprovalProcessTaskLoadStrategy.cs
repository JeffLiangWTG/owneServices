using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		void IProcessTaskLoadStrategy.AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
			switch (workflowDescriptor.Code)
			{
				case WorkflowDescriptors.APPaymentApprovalWorkflowDescriptorCode:
					subQuery.AddToFilter(AccPaymentApprovalSchema.AV_Ledger, LedgerTypes.AccountsPayable);
					break;

				case WorkflowDescriptors.ARPaymentApprovalWorkflowDescriptorCode:
					subQuery.AddToFilter(AccPaymentApprovalSchema.AV_Ledger, LedgerTypes.AccountsReceivable);
					break;
			}
		}

		Type IProcessTaskLoadStrategy.GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			return typeof(PaymentApprovalProcessTask);
		}
	}
}
