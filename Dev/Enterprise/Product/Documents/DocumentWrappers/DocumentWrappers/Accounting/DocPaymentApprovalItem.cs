using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocPaymentApprovalItem : DocumentWrapper, IDocMachedTransaction
	{
		protected DocPaymentApprovalItem(PaymentApprovalItem approvalItem, BusinessObjectFactory factory)
			: base(approvalItem, factory)
		{
			var transaction = approvalItem?.TransactionHeader;
			if (transaction != null)
			{
				TransactionType = transaction.AH_TransactionType;
				InvoiceDate = transaction.AH_InvoiceDate;
				TransactionNumber = transaction.AH_TransactionNum;
				CurrencyCode = transaction.AH_RX_NKTransactionCurrency;
				ExchangeRate = transaction.AH_ExchangeRate;
				OSAmount = approvalItem.OSAmountPaidThisRun;
				Desc = transaction.AH_Desc;
				Ledger = transaction.AH_Ledger;
				Amount = approvalItem.A2_PaymentThisRun;
				PreparedBy = transaction.Logs.CreatedByUserName;
				TransactionNumberPrefixed = transaction.TransactionNumberPrefixed;
				OrganizationCode = factory.Load<OrgHeader>(transaction.AH_OH)?.OH_Code ?? ZString.Empty;
				PaymentRequestedDate = transaction.AH_RequisitionDate;
				DueDate = transaction.AH_DueDate;
				Criticality = transaction.AH_RequisitionStatus;

				int invertAmount = TransactionType == ZArchitecture.Core.TransactionTypes.Payment || TransactionType == "UNA" ? 1 : -1;
				InvertedOriginalOSAmount = transaction.AH_OSTotal * invertAmount;
			}
		}

		protected DocPaymentApprovalItem()
			: base()
		{
		}

		public static DocPaymentApprovalItem New(PaymentApprovalItem approvalItem, BusinessObjectFactory factory)
		{
			return (approvalItem != null) ? new DocPaymentApprovalItem(approvalItem, factory) : null;
		}

		public static DocPaymentApprovalItem New()
		{
			return new DocPaymentApprovalItem();
		}

		public ZString OrganizationCode { get; }

		public ZString TransactionType { get; set; }

		public ZDateTime PaymentRequestedDate { get; }

		public ZDateTime InvoiceDate { get; }

		public ZDateTime DueDate { get; }

		public ZString Criticality { get; }

		public ZDecimal InvertedOriginalOSAmount { get; }

		public ZString TransactionNumber { get; }

		public ZString TransactionNumberPrefixed { get; }

		public ZString CurrencyCode { get; set; }

		public ZString Desc { get; set; }

		public ZString Ledger { get; }

		public ZDecimal OSAmount { get; set; }

		public ZDecimal ExchangeRate { get; set; }

		public ZDecimal Amount { get; set; }

		public ZString PreparedBy { get; set; }

		public DocPaymentApprovalMatchLink MatchLink
		{
			get { return fMatchLink ?? (fMatchLink = DocPaymentApprovalMatchLink.New(this, Factory)); }
		}
		DocPaymentApprovalMatchLink fMatchLink;

		#region Implementation

		public override string ToString()
		{
			return ZString.Empty;
		}

		#endregion
	}
}
