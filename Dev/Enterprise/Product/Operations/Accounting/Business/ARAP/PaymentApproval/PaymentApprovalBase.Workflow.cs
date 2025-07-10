using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalBase : IWorkflowProvider
	{
		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				return null;
			}
		}

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new PaymentApprovalProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, AV_OH, ZGuid.Empty);
			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				if (AV_Ledger == LedgerTypes.AccountsReceivable)
				{
					return WorkflowDescriptors.ARPaymentApprovalWorkflowDescriptorCode;
				}
				else
				{
					return WorkflowDescriptors.APPaymentApprovalWorkflowDescriptorCode;
				}
			}
		}
	}
}
