#define CODE_ANALYSIS

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public abstract partial class PostManagerGUIWrapper
	{
		protected virtual ParentInfo GetParentInfoForPostingAction()
		{
			var job = Jobs.First();
			return new ParentInfo() { Id = job.PK, Name = job.JH_JobNum, TableCode = job.TablePrefix };
		}

		public IPostingJobTransactionsApprovalGUIProvider ARCreditNoteApprovalGUIProvider
		{
			get { return arCreditNoteApprovalGUIProvider ?? (arCreditNoteApprovalGUIProvider = new ApprovalGUIProvider(this, wholePostingCanBeCancelled: true)); }
		}
		IPostingJobTransactionsApprovalGUIProvider arCreditNoteApprovalGUIProvider;

		protected IPostingJobTransactionsApprovalGUIProvider APInvoiceApprovalGUIProvider
		{
			get { return apInvoiceApprovalGUIProvider ?? (apInvoiceApprovalGUIProvider = new ApprovalGUIProvider(this, wholePostingCanBeCancelled: false)); }
		}
		IPostingJobTransactionsApprovalGUIProvider apInvoiceApprovalGUIProvider;

		BusinessObjectFactory FactoryForARCreditNoteApprovalRequests
		{
			get { return ARCreditNoteApprovalGUIProvider.FactoryForApprovalRequests; }
		}

#if DEBUG
		public bool ShowLoginFormForTest = true;

		public string SecurityItemForTest;

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Form ParentFormForInvoicePreview_ForTestOnly;
#endif

		class ApprovalGUIProvider : IPostingJobTransactionsApprovalGUIProvider
		{
			public ApprovalGUIProvider(PostManagerGUIWrapper postManagerGUIWrapper, bool wholePostingCanBeCancelled)
			{
				this.postManagerGUIWrapper = postManagerGUIWrapper;
				this.wholePostingCanBeCancelled = wholePostingCanBeCancelled;
			}

			readonly PostManagerGUIWrapper postManagerGUIWrapper;
			readonly bool wholePostingCanBeCancelled;

			public BusinessObjectFactory FactoryForApprovalRequests
			{
				get { return factoryForApprovalRequests ?? (factoryForApprovalRequests = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factoryForApprovalRequests;

			void ResetFactoryForApprovalRequests()
			{
				factoryForApprovalRequests = null;
			}

			#region IPostingJobTransactionsApprovalGUIProvider

#if DEBUG
			bool IPostingTransactionApprovalGUIProvider.ShowLoginFormForTest
			{
				get { return postManagerGUIWrapper.ShowLoginFormForTest; }
			}

			string IPostingTransactionApprovalGUIProvider.SecurityItemForTest
			{
				get { return postManagerGUIWrapper.SecurityItemForTest; }
			}
#endif

			bool IPostingTransactionApprovalGUIProvider.IsBulkPosting
			{
				get { return postManagerGUIWrapper.IsBulkPosting; }
			}

			ZDialogResult IPostingTransactionApprovalGUIProvider.ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
			{
				if (postManagerGUIWrapper.IsSilentAction(postManagerGUIWrapper.action))
				{
					postManagerGUIWrapper.RequestPreviewOrBulkPostErrorMessages = messageText;

					return dialogResult;
				}
				else
				{
					return Globals.Message.Show(messageText, messageCaption, messageBoxButtons, messageBoxIcon, dialogResult);
				}
			}

			ZDialogResult IPostingTransactionApprovalGUIProvider.ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
			{
				ZForm formToShow = null;

				if (approvingRequest is ARCreditNoteApprovalRequest)
				{
					var arCreditNoteApproval = approvingRequest as ARCreditNoteApprovalRequest;
					formToShow = new ARCreditNoteApprovalBulkForm(new ARCreditNoteApprovalBulk(arCreditNoteApproval.Factory, new InteractiveSecurityOverrideProvider(), arCreditNoteApproval), TransactionApprovalFormModes.SetDescription);
				}

				return (ZDialogResult)(formToShow != null ? ZFormModaliser.ShowDialogAndDispose(formToShow) : DialogResult.None);
			}

			void IPostingTransactionApprovalGUIProvider.NotifyBulkPostingIsNotAuthorized(string message)
			{
				if (wholePostingCanBeCancelled)
				{
					postManagerGUIWrapper.NotifyPostValidationError(message);
				}
				else
				{
					postManagerGUIWrapper.NotifyPostingWarning(message);
				}
			}

			BusinessObjectFactory IPostingTransactionApprovalGUIProvider.FactoryForApprovalRequests
			{
				get { return FactoryForApprovalRequests; }
			}

			void IPostingTransactionApprovalGUIProvider.ResetFactoryForApprovalRequests()
			{
				ResetFactoryForApprovalRequests();
			}

			Tuple<ZGuid, ZString> IPostingTransactionApprovalGUIProvider.GetParentIdAndTableCodeForJobPostingAction()
			{
				var parentInfo = postManagerGUIWrapper.GetParentInfoForPostingAction();

				return Tuple.Create(parentInfo.Id, parentInfo.TableCode);
			}

			void IPostingTransactionApprovalGUIProvider.RollbackPosting()
			{
				postManagerGUIWrapper.PostManager.RollbackPosting();
			}

			ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover)
			{
				return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, keepLoginFormResultAfterFirstUserAnswer: true, supportMultipleApprover: supportMultipleApprover);
			}

			ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, ARCreditNoteApprovalRequest[] approvalRequests)
			{
				return new InvoicingSecurityOverrideProvider(showApprovalRequestButton, keepLoginFormResultAfterFirstUserAnswer: true, supportMultipleApprover: supportMultipleApprover, aRCreditNoteApprovalRequests: approvalRequests);
			}

			JobInvoicingPostingOption IPostingTransactionApprovalGUIProvider.PostingOption
			{
				get
				{
					return postManagerGUIWrapper.PostingOption;
				}
			}

			APInvoiceChargesApprovalRequest IPostingJobTransactionsApprovalGUIProvider.RequestToCompare
			{
				get { return postManagerGUIWrapper.requestToCompare; }
			}

			ZDialogResult IPostingJobTransactionsApprovalGUIProvider.ShowPostingConfirmationForm(APInvoiceCharges[] apInvoiceCharges)
			{
				var formBizo = new APInvoiceChargesCollection();
				try
				{
					formBizo.AddRange(apInvoiceCharges);
					var parentInfo = postManagerGUIWrapper.GetParentInfoForPostingAction();

					return (ZDialogResult)ZFormModaliser.ShowDialogAndDispose(new APInvoicePostingWithApprovalRequestSummaryForm(formBizo, currentPostingJobNumber: parentInfo.Name));
				}
				finally
				{
					formBizo.RemoveAll();
				}
			}

			public Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode)
			{
				var reversingHolder = new TransactionReasonHolder(existingReasonCode);
				ZFormModaliser.ShowDialogAndDispose(new TransactionReasonForm(reversingHolder, Res.GetString("c44a73a5-5451-4595-811d-44f4087682cf", "Please enter the reason for creating this credit note reversal approval request"), Res.GetString("7ac53ec5-cdda-4d2c-b738-6f595820fa78", "Reversing Reason")));
				return Tuple.Create(reversingHolder.Code, reversingHolder.Reason);
			}

			bool IPostingJobTransactionsApprovalGUIProvider.IsForPreviewOnly
			{
				get { return postManagerGUIWrapper.action == PostManagerAction.Preview || postManagerGUIWrapper.action == PostManagerAction.PrepareTransactionsForPreview; }
			}

			#endregion
		}

		[SuppressMessage("Microsoft.Performance", "CA1815: Override equals and operator equals on value types", Justification = "Instances of the type is not be compared to each other.")]
#if DEBUG
		internal
#endif
		protected struct ParentInfo
		{
			public ZGuid Id;
			public ZString Name;
			public ZString TableCode;
		}
	}
}
