using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class APInvoiceReversing : InvoicingBaseReversing
	{
		public APInvoiceReversing(InvoicingBase aPInv)
			: base(aPInv)
		{
		}

		public new InvoicingBase OriginalTransaction => (InvoicingBase)base.OriginalTransaction;

		#region ReversingPayment

		public IPayablesAndReceivables ReversingPayment
		{
			get { return fReversingPayment; }
		}

		IPayablesAndReceivables fReversingPayment;

		#endregion

		#region PaymentToReverse

		public IPayablesAndReceivables PaymentToReverse
		{
			get
			{
				if (fPaymentToReverse == null)
				{
					APInvoice apInvoice = OriginalTransaction as APInvoice;
					if (apInvoice != null)
					{
						fPaymentToReverse = apInvoice.PaymentForThisCashInvoice;
					}
				}
				return fPaymentToReverse;
			}
		}

		protected bool isPaymentToReverseExistAndCleared
		{
			get
			{
				return PaymentToReverse != null && PaymentToReverse.IsClearedInCashbook;
			}
		}

		IPayablesAndReceivables fPaymentToReverse;
		DirectDebitBatchHeader fDirectDebitBatchHeaderToReverse;

		#endregion

		#region Implementation

		public override bool ShouldShowReverseConfirmationMessage()
		{
			return base.ShouldShowReverseConfirmationMessage() || IsInvoiceGeneratedFromPurchaseOrder();
		}

		public override ZString GetReverseConfirmationMessage()
		{
			var message = base.GetReverseConfirmationMessage();
			if (message.IsEmpty)
			{
				var filter = new ZQuery(AccPayableOrderHeaderSchema.APH_AH, InvoiceToReverse.PK);
				var purchaseOrder = InvoiceToReverse.Factory.LoadTop1<AccPayableOrderHeader>(filter);
				if (purchaseOrder != null)
				{
					message = string.Format(Res.GetString("eef06369-aa42-4e9a-9ed3-d92a91cbfb0d", "You are about to reverse an invoice generated from a purchase order. Do you want to proceed?\r\n\r\nRelated Purchase Order Details:\r\n\r\n{0}", GetPurchaseOrderDetails(purchaseOrder)));
				}
			}

			return message;
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() && !isPaymentToReverseExistAndCleared && !ShouldPreventInvoiceReversing;
		}

		bool ShouldPreventInvoiceReversing
		{
			get
			{
				bool result = false;
				if (OriginalTransaction.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions)
				{
					result = AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsPayable, OriginalTransaction.AH_GC);
				}
				return result;
			}
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			ZString result = base.GenerateCantReverseErrorMessage();
			if (string.IsNullOrEmpty(result))
			{
				if (isPaymentToReverseExistAndCleared)
				{
					result = ClearedInCashBookErrorMessage;
				}
				else if (ShouldPreventInvoiceReversing)
				{
					result = AccountingMasterFilesUtils.APInvoiceReversalDisallowedMessage;
				}
			}
			return result;
		}

		protected override void DoReverseTransaction()
		{
			base.DoReverseTransaction();
			if (PaymentToReverse != null)
			{
				SetTransactionCount();
			}

			if (IsInvoiceGeneratedFromPurchaseOrder())
			{
				UpdatePurchaseOrderWhenReversingLinkedInvoice();
			}
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			if (PaymentToReverse != null)
			{
				PaymentToReverse.GenerateReverseTransaction(true);
				fReversingPayment = PaymentToReverse.ReverseTransaction as IPayablesAndReceivables;
			}

			if (PaymentToReverse is IPayment)
			{
				fDirectDebitBatchHeaderToReverse = DirectDebitBatchHeader.ReverseDDRBatch(PaymentToReverse as Payment);
			}
		}

		protected override void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			base.SetOtherNumberFountainFields(factory);
			if (fDirectDebitBatchHeaderToReverse != null)
			{
				ZString nextDDRBatchNo = AccountingNumberFountainWrapperFactory.Instance.DDRBatchNo.GetNext(fDirectDebitBatchHeaderToReverse.Factory);
				Payment revPayment = (Payment)PaymentToReverse.ReverseTransaction;
				revPayment.AH_ReceiptBatchNo = nextDDRBatchNo;
				fDirectDebitBatchHeaderToReverse.AH_TransactionNum = nextDDRBatchNo;
				fDirectDebitBatchHeaderToReverse.AH_ReceiptBatchNo = nextDDRBatchNo;
			}
		}

		protected override void SetCancellationFlagOnTransactionsToReverse()
		{
			base.SetCancellationFlagOnTransactionsToReverse();
			if (PaymentToReverse != null)
			{
				PaymentToReverse.SetCancellationFlag(true);
				ReversingPayment.SetCancellationFlag(true);
			}
		}

		protected override void SetReversingDescriptionOnTransactions()
		{
			base.SetReversingDescriptionOnTransactions();
			if (PaymentToReverse != null)
			{
				ZString reversingPaymentDesc = String.Format(
					  AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated,
											Res.GetString("cef0510a-da7f-4002-abfb-49f9e6f1abd9", "Reversal related to")) + " {0}", ((ITransaction)PaymentToReverse).TransactionNumber);
				ReversingPayment.SetDescription(reversingPaymentDesc);
				ReversingPayment.SetNumberOfSupportingDocuments(AccountingConfigurationRegistry.Instance.GetVoucherNoOfAttchmentsFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated, 0));
			}
		}

		protected override void SetTransactionBelongsToGroupOnTransactionsToReverse()
		{
			if (PaymentToReverse != null)
			{
				ZGuid groupingGuid = OriginalTransaction.AH_TransactionBelongsToGroup;
				ReversingAPCreditNote.AH_TransactionBelongsToGroup = groupingGuid;
				ReversingAPPayment.AH_TransactionBelongsToGroup = groupingGuid;
			}
			else
			{
				base.SetTransactionBelongsToGroupOnTransactionsToReverse();
			}
		}

		protected override void SetMatchGroupNumberAndMatchDateOnSaving(BusinessObjectFactory factory)
		{
			SetReversingPaymentPostDate();
			base.SetMatchGroupNumberAndMatchDateOnSaving(factory);
		}

		protected override void FullyPayBothTransactions()
		{
			// the original payment and invoice are not unmatched, only fully pay the reversing transactions
			base.FullyPayBothTransactions();
			if (PaymentToReverse != null)
			{
				ReversingPayment.FullyPay(((ITransaction)ReversingPayment).PostDate);
				PaymentToReverse.FullyPay(((ITransaction)ReversingPayment).PostDate);
			}
		}

		protected void SetReversingPaymentPostDate()
		{
			if (ReversingAPCreditNote != null && ReversingAPPayment != null)
			{
				ReversingAPPayment.AH_PostDate = ReversingAPCreditNote.AH_PostDate;
			}
		}

		protected override void GetMatchLinksFromTransactions()
		{
			if (PaymentToReverse != null)
			{
				ReversePayablesAndReceivables.GenerateMatchLinks();
				ReversingPayment.GenerateMatchLinks();

				ReverseTransactionMatchLinks = new TransactionMatchLinkCollection(InvoiceToReverse.Factory);
				ReverseTransactionMatchLinks.AddRange(ReversePayablesAndReceivables.CurrentMatchGroup);
				ReverseTransactionMatchLinks.AddRange(ReversingPayment.CurrentMatchGroup);
				ReversePayablesAndReceivables.CurrentMatchGroup.RemoveAll();
				ReversingPayment.CurrentMatchGroup.RemoveAll();

				OriginalTransactionMatchLinks = new TransactionMatchLinkCollection(InvoiceToReverse.Factory);
				if (InvoiceToReverse.LatestMatchLink != null)
				{
					TransactionMatchLink invoiceMatchLink = OriginalTransactionMatchLinks.AddNew();
					invoiceMatchLink.CopyPersistentValuesFrom(InvoiceToReverse.LatestMatchLink);
					InvoiceToReverse.LatestMatchLink.Delete();
				}
				if (APPaymentToReverse.LatestMatchLink != null)
				{
					TransactionMatchLink paymentMatchLink = OriginalTransactionMatchLinks.AddNew();
					paymentMatchLink.CopyPersistentValuesFrom(APPaymentToReverse.LatestMatchLink);
					APPaymentToReverse.LatestMatchLink.Delete();
				}
				((IMatching)InvoiceToReverse).CurrentMatchGroup.AddRange(OriginalTransactionMatchLinks);
				((IMatching)InvoiceToReverse).CurrentMatchGroup.AddRange(ReverseTransactionMatchLinks);
			}
			else
			{
				base.GetMatchLinksFromTransactions();
			}
		}

		protected override ZString GetMatchGroupNumber()
		{
			ZString matchGroupNumber = ZString.Empty;
			matchGroupNumber = (OriginalTransactionMatchLinks != null && OriginalTransactionMatchLinks.Count > 0 && !OriginalTransactionMatchLinks[0].AP_MatchGroupNum.IsEmpty ?
				OriginalTransactionMatchLinks[0].AP_MatchGroupNum : base.GetMatchGroupNumber());
			return matchGroupNumber;
		}

		void SetTransactionCount()
		{
			if (ReversingAPCreditNote != null && ReversingAPPayment != null)
			{
				ReversingAPCreditNote.AH_TransactionCount = 3;
				ReversingAPPayment.AH_TransactionCount = 4;
			}
		}

		protected override bool IsTransactionMatched
		{
			get
			{
				return
				(
					(OriginalPayablesAndReceivables.IsMatched || OriginalPayablesAndReceivables.IsReversed) &&
						!((OriginalTransaction as IInvoiceAssociatedToCashAdvanceRequest)?.IsOutstandingAmountPaidOnlyViaCashAdvance() ?? false)
				);
			}
		}

		APPayment ReversingAPPayment
		{
			get { return ReversingPayment as APPayment; }
		}

		APPayment APPaymentToReverse
		{
			get { return PaymentToReverse as APPayment; }
		}

		APCreditNote ReversingAPCreditNote
		{
			get { return ReverseTransaction as APCreditNote; }
		}

		APInvoice InvoiceToReverse
		{
			get { return OriginalTransaction as APInvoice; }
		}

		#endregion

		#region Reversing AP Invoice Linked To A Purchase Order

		void UpdatePurchaseOrderWhenReversingLinkedInvoice()
		{
			var filter = new ZQuery(AccPayableOrderHeaderSchema.APH_AH, InvoiceToReverse.PK);
			var purchaseOrder = InvoiceToReverse.Factory.LoadTop1<AccPayableOrderHeader>(filter);
			if (purchaseOrder != null)
			{
				ReversePurchaseOrder(purchaseOrder);
			}

			if (purchaseOrder.OrderSplitSiblings.Count > 0)
			{
				foreach (var siblingOrder in purchaseOrder.OrderSplitSiblings)
				{
					ReversePurchaseOrder(siblingOrder);
				}
			}
		}

		void ReversePurchaseOrder(AccPayableOrderHeader purchaseOrder)
		{
			if (purchaseOrder != null)
			{
				var sIVEventLogs = purchaseOrder.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ServiceInvoicePosted.Code));
				foreach (var log in sIVEventLogs)
				{
					log.Cancel();
				}
				purchaseOrder.APH_AH = ZGuid.Empty;
				purchaseOrder.APH_Disposition = Constants.PayableOrderDisposition.APInvoiceToBePosted;
				purchaseOrder.APH_InvoiceNumber = ZString.Empty;
			}
		}

		bool IsInvoiceGeneratedFromPurchaseOrder()
		{
			var result = false;
			if (InvoiceToReverse != null)
			{
				var filter = new ZQuery(AccPayableOrderHeaderSchema.APH_AH, InvoiceToReverse.PK);
				var purchaseOrder = InvoiceToReverse.Factory.LoadTop1<AccPayableOrderHeader>(filter);
				result = purchaseOrder != null;
			}
			return result;
		}

		string GetPurchaseOrderDetails(AccPayableOrderHeader purchaseOrder)
		{
			var orderNumber = purchaseOrder.APH_OrderNumber;
			var stage = purchaseOrder.APH_Stage_List.GetDescriptionFromCode(purchaseOrder.APH_Stage);
			var disposition = purchaseOrder.APH_Disposition_List.GetDescriptionFromCode(purchaseOrder.APH_Disposition);
			var createdTime = purchaseOrder.APH_SystemCreateTimeUtc;
			var filter = new ZQuery(GlbStaffSchema.GS_Code, purchaseOrder.APH_SystemCreateUser);
			var createdUser = purchaseOrder.Factory.LoadTop1<GlbStaff>(filter);
			var userName = createdUser != null ? createdUser.GS_FullName : ZString.Empty;
			var detailsString = string.Format(Res.GetString("2c5e5c13-8f6c-4ca2-870a-6b9667233456","Purchase Order Number : {0}\r\nStage : {1}\r\nDisposition : {2}\r\nCreated Time : {3}\r\nCreated By : {4}", orderNumber, stage, disposition, createdTime, userName));
			return detailsString;
		}

		#endregion
	}
}
