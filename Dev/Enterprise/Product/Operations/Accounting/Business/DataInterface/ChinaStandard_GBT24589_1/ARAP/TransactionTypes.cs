using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ARAPTransactionTypes : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T302";
		public ZString TransactionTypesCode { get; set; }
		public ZString TransactionTypesName { get; set; }
	}

	public class ARAPTransactionTypesCollection : NonPersistentBusinessObjectCollection<ARAPTransactionTypes>	{
		public ARAPTransactionTypesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARAPTransactionTypes();
		}

		void AddDefaultElements()
		{
			ARAPTransactionTypes arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Invoice;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARInvoice;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARAdjustment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARCreditNote;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Payment;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARPayment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Receipt;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARReceipt;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Discount;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARDiscount;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.ExchangeDifference;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARExchangeDiff;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Overpayment;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.AROverpayment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Journal;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARJournal;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsReceivable + TransactionTypes.Transfer;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.ARTransfer;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = TransactionTypes.Contra;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.Contra;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Invoice;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APInvoice;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APAdjustment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.CreditNote;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APCreditNote;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Payment;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APPayment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Receipt;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APReceipt;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Discount;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APDiscount;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.ExchangeDifference;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APExchangeDiff;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Overpayment;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APOverpayment;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Journal;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APJournal;

			arapTransactionTypes = AddNew();
			arapTransactionTypes.TransactionTypesCode = LedgerTypes.AccountsPayable + TransactionTypes.Transfer;
			arapTransactionTypes.TransactionTypesName = TransactionDescription.APTransfer;
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China's Accounting fixed value")]
	public static class TransactionDescription
	{
		public const string APInvoice = "营业成本";
		public const string APCreditNote = "营业成本";
		public const string APAdjustment = "营业成本";
		public const string APReceipt = "银行收入";
		public const string APPayment = "银行支出";
		public const string APDiscount = "应付回扣";
		public const string APExchangeDiff = "应付汇兑损益";
		public const string APOverpayment = "应付预付账款";
		public const string APJournal = "应付凭证";
		public const string APTransfer = "应付转账";

		public const string ARInvoice = "营业收入";
		public const string ARCreditNote = "营业收入";
		public const string ARAdjustment = "营业收入";
		public const string ARReceipt = "银行收入";
		public const string ARPayment = "银行支出";
		public const string ARDiscount = "应收回扣";
		public const string ARExchangeDiff = "应收汇兑损益";
		public const string AROverpayment = "应收预付账款";
		public const string ARJournal = "应收凭证";
		public const string ARTransfer = "应收凭证转账";
		public const string Contra = "应收/应付抵账";
	}
}

