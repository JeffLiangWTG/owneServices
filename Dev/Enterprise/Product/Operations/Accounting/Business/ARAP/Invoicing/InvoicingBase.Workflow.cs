using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract partial class InvoicingBase : IWorkflowProvider
	{
		void CreateTasksAndMilestonesFromTemplate()
		{
			if (!IsInDatabase && !((IWorkflowProvider)this).WorkflowType.IsEmpty)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return WorkflowInformationProvider;
		}

		WorkflowInformationProvider WorkflowInformationProvider
		{
			get
			{
				if (workflowInformationProvider == null)
				{
					workflowInformationProvider = new WorkflowInformationProvider(new ZGuid[] { AH_GC });
				}
				workflowInformationProvider.Origin = ZString.Empty;
				workflowInformationProvider.Destination = ZString.Empty;
				workflowInformationProvider.BusinessContext = TrackingConstants.BusinessContext.Transaction;

				return workflowInformationProvider;
			}
		}
		WorkflowInformationProvider workflowInformationProvider;

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new TransactionHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, AH_OH, ZGuid.Empty);
			return result;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				var allowedTransactionTypes = new[]
				{
					TransactionTypes.Invoice, TransactionTypes.IncompleteInvoice, TransactionTypes.InvoicePendingAllocation,
					TransactionTypes.CreditNote, TransactionTypes.IncompleteCreditNote, TransactionTypes.CreditNotePendingAllocation,
					TransactionTypes.AdjustmentNote, TransactionTypes.IncompleteAdjustmentNote
				};
				if (allowedTransactionTypes.Contains(AH_TransactionType.ToString()))
				{
					if (AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						return WorkflowDescriptors.ARInvoiceCode;
					}
					else
					{
						return WorkflowDescriptors.APInvoiceCode;
					}
				}

				return ZString.Empty;
			}
		}
	}
}
