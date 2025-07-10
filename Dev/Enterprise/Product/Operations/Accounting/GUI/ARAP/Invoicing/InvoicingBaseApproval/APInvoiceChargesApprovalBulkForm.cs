using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.InvoicingApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class APInvoiceChargesApprovalBulkForm : APInvoiceChargesApprovalBulkFormForDesigner
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public APInvoiceChargesApprovalBulkForm()
		{
		}

		public APInvoiceChargesApprovalBulkForm(APInvoiceChargesApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				DetailsGrid.RemoveFromAvailableColumns("PlaceOfSupply");
			}
		}

		public override bool IsReasonDescriptionReadOnly
		{
			get { return base.IsReasonDescriptionReadOnly && ActionMode != TransactionApprovalFormModes.Reject; }
		}

		#region Posting

		protected override bool IsPostingSupported => true;

		protected override bool IsSaveButtonHidden => base.IsSaveButtonHidden || !Env.Security.APInvoiceApproval_Post.IsAllowed;

		protected override void SetPostContext(BusinessObjectFactory newFactory)
		{
			newFactory.SetContext(APInvoiceChargesApprovalRequest.Context.Posting);
		}

		protected override BusinessObject GetRequestParent(APInvoiceChargesApprovalRequest reloadedApproval, out ZString errorMessage)
		{
			var errorMessages = new ZStringBuilder();
			BusinessObject parent = null;
			if (reloadedApproval.IsTransactionRelated)
			{
				(parent, var restoreSavedDataResult) = reloadedApproval.GetLinkedInvoice();
				if (restoreSavedDataResult != null && restoreSavedDataResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
				{
					errorMessages.AppendLine(Res.GetString("0317EE3B-EE8B-491C-8D31-1D1F8D8E6423", "Request ({0}): {1}.", reloadedApproval.FormatedRequestId, restoreSavedDataResult.Error));
				}
			}
			else if (reloadedApproval.IsJobRelated)
			{
				parent = reloadedApproval.Factory.Load<Job>(reloadedApproval.XP_ParentID);
			}
			else if (reloadedApproval.IsConsolRelated)
			{
				parent = reloadedApproval.Factory.Load<GenericConsol>(reloadedApproval.XP_ParentID);
			}
			else
			{
				errorMessages.AppendLine(Res.GetString("441115E5-E4E5-410A-9F58-7CE953287F6D", "Request ({0}) - reference type {1} is invalid.", reloadedApproval.FormatedRequestId, reloadedApproval.ReferenceType));
			}

			errorMessage = errorMessages.ToString();

			return parent;
		}

		protected override ControllerID GetControllerIDForEditing(BusinessObject requestParent, out BusinessObject objectToEdit)
		{
			ControllerID result = null;
			objectToEdit = null;
			var transaction = requestParent as InvoicingBase;
			Job job;
			GenericConsol genericConsol;
			if (transaction != null)
			{
				result = new AccountingControllerIdDecider().GetControllerID(transaction.AH_TransactionType, transaction.AH_Ledger, ModuleIDs.APInvoiceApproval);
				objectToEdit = transaction;
			}
			else if ((job = requestParent as Job) != null)
			{
				var genericJob = (GenericJob)job.Factory.LoadGenericJob(job);
				objectToEdit = genericJob.Consumer as BusinessObject;
				result = genericJob.GetConsumerController();
			}
			else if ((genericConsol = requestParent as GenericConsol) != null)
			{
				result = genericConsol.GetParentConsolController();
				objectToEdit = genericConsol;
			}

			return result;
		}

		protected override void PostApprovalsAndRemovePosted(TransactionApprovalBulk<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails> approvalBulk)
		{
			((APInvoiceChargesApprovalBulk)approvalBulk).PostApprovalsAndRemovePosted(new BaseInvoicingFormApprovalGUIProvider(this));
		}

		#endregion
	}

#if DEBUG
	// This ZForm serves as base class only. It's not abstract so that it can be open in designer tool. Hence it has TestExcludeZWinFormsAllHaveFormBashers attribute applied.
	[TestExcludeZWinFormsAllHaveFormBashers]
#endif
	public class APInvoiceChargesApprovalBulkFormForDesigner : TransactionApprovalBulkForm<InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceChargesApprovalBulkFormForDesigner()
		{
		}

		public APInvoiceChargesApprovalBulkFormForDesigner(APInvoiceChargesApprovalBulk bo, TransactionApprovalFormModes actionMode)
			: base(bo, actionMode)
		{
		}
	}
}
