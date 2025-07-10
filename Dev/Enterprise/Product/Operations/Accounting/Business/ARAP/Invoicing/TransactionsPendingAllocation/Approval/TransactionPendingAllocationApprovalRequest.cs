using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalRequest : InvoicingBaseApprovalRequest<TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void Initialize(TransactionPendingAllocation transaction)
		{
			Argument.NotNull(transaction, "transaction");

			var latestApprovalRequest = transaction.TransactionApprovalRequest; //get previous request before linking new one

			base.Initialize(transaction.PK, transaction.TablePrefix);

			XP_ReasonDescription = transaction.AH_Desc;

			transactionToLink = transaction;

			PostingDetails.MaxAmountToApprove = transaction.AH_LocalExTaxAmount + transaction.AH_LocalTaxAmount;

			PostingDetails.TransactionDate = transaction.AH_InvoiceDate;
			PostingDetails.PostDate = transaction.AH_PostDate;
			PostingDetails.TransactionNumber = transaction.AH_TransactionNum;
			PostingDetails.CreditorPK = transaction.AH_OH;
			PostingDetails.DueDate = transaction.AH_DueDate;
			PostingDetails.CurrencyCode = transaction.AH_RX_NKTransactionCurrency;
			PostingDetails.ExRate = transaction.AH_ExchangeRate;
			PostingDetails.OSExTaxAmount = transaction.AH_OSExTaxAmount;
			PostingDetails.OSTaxAmount = transaction.AH_OSTaxAmount;
			PostingDetails.LocalExTaxAmount = transaction.AH_LocalExTaxAmount;
			PostingDetails.LocalTaxAmount = transaction.AH_LocalTaxAmount;
			PostingDetails.Description = transaction.AH_Desc;
			PostingDetails.BranchPK = transaction.AH_GB;
			PostingDetails.DepartmentPK = transaction.AH_GE;
			PostingDetails.AddressPK = transaction.AH_OA_InvoiceAddressOverride;
			PostingDetails.ContactPK = transaction.AH_OC_InvoiceContactOverride;
			PostingDetails.NumberOfSupportingDocuments = transaction.AH_NumberOfSupportingDocuments;
			PostingDetails.PlaceOfSupply = transaction.AH_PlaceOfSupply;
			PostingDetails.PlaceOfSupplyType = transaction.AH_PlaceOfSupplyType;
			if (!transaction.AH_InvoiceTerm.IsEmpty)
			{
				PostingDetails.InvoiceTerm = transaction.AH_InvoiceTerm;
				PostingDetails.InvoiceTermDays = transaction.AH_InvoiceTermDays;
			}

			if (latestApprovalRequest != null)
			{
				PostingDetails.SourceXML = latestApprovalRequest.PostingDetails.SourceXML;
				PostingDetails.IsCrossLedgerImportFromXML = latestApprovalRequest.PostingDetails.IsCrossLedgerImportFromXML;
			}
		}

		public void Initialize(TransactionPendingAllocation transaction, ZString sourceXML, bool isCrossLedgerImportFromXML)
		{
			Initialize(transaction);

			PostingDetails.SourceXML = sourceXML;
			PostingDetails.IsCrossLedgerImportFromXML = isCrossLedgerImportFromXML;
		}

		public TransactionPendingAllocation LinkedTransaction
		{
			get { return transactionToLink ?? Factory.Load<TransactionPendingAllocation>(XP_ParentID); }
		}

		public bool HasUniversalTransaction
		{
			get { return !PostingDetails.SourceXML.IsEmpty; }
		}

		public override bool TransactionIsAlreadyPosted => LinkedTransaction != null && LinkedTransaction.IsPosted;

		public override void OnSaving()
		{
			base.OnSaving();

			EvaluateEInvoicingEligibilityAndQueue();
		}

		public override ZString XP_ApprovalStatus
		{
			get { return base.XP_ApprovalStatus; }
			set
			{
				if (value == GenApprovalRequestApprovalStatus.Cancelled && IsInDatabase && LinkedTransactionToAddLog != null)
				{
					var allCancelApprovalRequestContexts = new List<BusinessContext> {
						BusinessContext.CancelApprovalRequestByUser,
						BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction,
						BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted
					};

					if (!this.GetContexts<BusinessContext>().Intersect(allCancelApprovalRequestContexts).Any())
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK,
							CriticalValidationInfoCollectorServiceKeyType.ApprovalRequestCancelledWithoutAnyBusinessContext,
							() => { return GetApprovalRequestInfo(); },
							CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);
					}
				}
				base.XP_ApprovalStatus = value;
			}
		}

		#region EInvoicing

		internal IEInvoicingTransaction EInvoicingTransaction
		{
			get
			{
				if (EInvoicingTransactionValue == null)
				{
					var transaction = Factory.Load<TransactionPendingAllocation>(XP_ParentID);
					if (transaction != null)
					{
						EInvoicingTransactionValue = ObjectFactory.Get<IEInvoicingTransactionProxyFactory>().GetProxy(transaction);
					}
				}
				return EInvoicingTransactionValue;
			}
		}

		IEInvoicingTransaction EInvoicingTransactionValue;

		void EvaluateEInvoicingEligibilityAndQueue()
		{
			if (IsInDatabase)
			{
				EInvoicingTransaction?.EvaluateEligibilityAndQueue();
			}
		}

		public bool IsTransactionEligibleToCreateRejectionRequest(ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider rejectionSupporter) =>
			!(rejectionSupporter != null
			&& AccountingMasterFilesRegistry.Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
			&& LinkedTransaction.AH_Ledger == LedgerTypes.TransactionsPendingAllocation
			&& !rejectionSupporter.IsTransactionEligibleToCreateRejectionRequest(LinkedTransaction));

		#endregion

		protected override bool IsPostingActionTheSameCore(TransactionApprovalRequest<TransactionPendingAllocationApprovalDetails> request) => request is TransactionPendingAllocationApprovalRequest; //only one posting action is allowed for this type

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			XP_ApprovalType = Constants.GenApprovalRequestApprovalType.TransactionPendingAllocation;
		}

		protected override GenApprovalRequestLookups GetNewLookups()
		{
			return new TransactionPendingAllocationApprovalRequestLookups(this);
		}

		protected override ZString PostingOptionCore
		{
			get { return ZString.Empty; }
		}

		[ResourceStringData("JobNumberAsRefNumber", Caption = "Reference Number", ShortCaption = "Ref. Num.", MediumCaption = "Ref. Number")]
		public override ZString JobNumber
		{
			get
			{
				if (!JobNumber_cached.HasValue)
				{
					var invoice = LinkedTransaction;
					if (invoice != null)
					{
						JobNumber_cached = invoice.AH_TransactionNum;
					}
				}

				return base.JobNumber;
			}
		}

		protected override Type ApprovingTransactionTypeCore
		{
			get { return typeof(TransactionPendingAllocation); }
		}

		protected override TransactionPendingAllocationApprovalDetails CreatePostingApprovalDetails()
		{
			return new TransactionPendingAllocationApprovalDetails(Factory);
		}

		protected override TransactionApprovalRequestEmail CreateEmail()
		{
			return null;
		}

		protected override bool ShouldEmailBeSent
		{
			get { return false; }
		}

		protected override void FinalizeCancellingCore()
		{
			base.FinalizeCancellingCore();
			if (LinkedTransaction != null && !LinkedTransaction.IsPosted)
			{
				CancelAndIncrementTransactionCount(LinkedTransaction);
			}
		}

		protected override ZString RequestApprovedReference => (NoResString)"Approved For Allocation";
		protected override ZString RequestCancelledByUserReference => (NoResString)"Cancelled By User";
		protected override ZString RequestCancelledForEditReference => (NoResString)"Cancelled Due To Edit/Update";
		protected override ZString RequestRejectedReference => (NoResString)"Rejected By User";
		protected override ZString RequestCancelledAsTransactionAlreadyCancelledOrPostedReference => (NoResString)"Cancelled As Transaction Already Cancelled/Posted";

		TransactionPendingAllocation transactionToLink;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new TransactionPendingAllocationApprovalRequestFetchStrategy(this);
	}
}
