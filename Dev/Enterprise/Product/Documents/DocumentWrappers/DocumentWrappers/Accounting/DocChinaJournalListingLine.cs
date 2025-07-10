using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocChinaJournalListingLine : DocumentWrapper
	{
		protected ChinaJournal ChinaJournal;

		DocChinaJournalListingLine(ChinaJournal chinaJournal)
			: base(chinaJournal, chinaJournal.Factory)
		{
			this.ChinaJournal = chinaJournal;
		}

		public static DocChinaJournalListingLine New(ChinaJournal chinaJournal, BusinessObjectFactory factory)
		{
			return chinaJournal != null ? new DocChinaJournalListingLine(chinaJournal) : null;
		}

		public ZDecimal OriginalAmount
		{
			get { return ChinaJournal.OriginalAmount; }
		}

		public ZString Currency
		{
			get { return ChinaJournal.Currency; }
		}

		public ZString ApprovedBy
		{
			get { return ChinaJournal.ApprovedBy; }
		}

		public ZString HandlingStaff
		{
			get { return ChinaJournal.HandlingStaff; }
		}

		public ZString Annotation
		{
			get { return ChinaJournal.Annotation; }
		}

		public ZString PaymentNumber
		{
			get { return ChinaJournal.PaymentNumber; }
		}

		public ZString BusinessContacts
		{
			get { return ChinaJournal.BusinessContacts; }
		}

		public ZInt SerialNumber
		{
			get { return ChinaJournal.SerialNumber; }
		}

		public ZString SystemModule
		{
			get { return ChinaJournal.SystemModule; }
		}

		public ZString BusinessDesc
		{
			get { return ChinaJournal.BusinessDesc; }
		}

		public ZString VoucherDate
		{
			get { return ChinaJournal.VoucherDate; }
		}

		public ZInt FinancialYear
		{
			get { return ChinaJournal.FinancialYear; }
		}

		public ZInt Period
		{
			get { return ChinaJournal.Period; }
		}

		public ZString VoucherTypeNumber
		{
			get { return ChinaJournal.VoucherTypeNumber; }
		}

		public ZString VoucherNumber
		{
			get { return ChinaJournal.VoucherNumber; }
		}

		public ZString VoucherLineNumber
		{
			get { return ChinaJournal.VoucherLineNumber; }
		}

		public ZString VoucherDescription
		{
			get { return ChinaJournal.VoucherDescription; }
		}

		public ZString GLAccountNumber
		{
			get { return ChinaJournal.GLAccountNumber; }
		}

		public ZString GLAccountNumAndDescription
		{
			get { return ChinaJournal.GLAccountNumAndDescription; }
		}

		public ZString CurrencyCode
		{
			get { return ChinaJournal.CurrencyCode; }
		}

		public ZString Unit
		{
			get { return ChinaJournal.Unit; }
		}

		public ZDecimal Quantity
		{
			get { return ChinaJournal.DebitQuantity + ChinaJournal.CreditQuantity; }
		}

		public ZDecimal DebitCurrencyAmount
		{
			get { return ChinaJournal.DebitCurrencyAmount; }
		}

		public ZDecimal DebitAmount
		{
			get { return ChinaJournal.DebitAmountLocalCurrency; }
		}

		public ZDecimal CreditCurrencyAmount
		{
			get { return ChinaJournal.CreditCurrencyAmount; }
		}

		public ZDecimal CreditAmount
		{
			get { return ChinaJournal.CreditAmountLocalCurrency; }
		}

		public ZString ExRateTypeNumber
		{
			get { return ChinaJournal.ExRateTypeNumber; }
		}

		public ZDecimal ExRate
		{
			get { return ChinaJournal.ExRate; }
		}

		public ZDecimal UnitPrice
		{
			get { return ChinaJournal.UnitPrice; }
		}

		public ZString VoucherHeaderExtendedFieldSchemasValue
		{
			get { return ChinaJournal.VoucherHeaderExtendedFieldSchemasValue; }
		}

		public ZString EntryLineExtendedFieldSchemas
		{
			get { return ChinaJournal.EntryLineExtendedFieldSchemas; }
		}

		public ZString PaymentTypeCode
		{
			get { return ChinaJournal.PaymentTypeCode; }
		}

		public ZString VoucherType
		{
			get { return ChinaJournal.VoucherType; }
		}

		public ZString VoucherDocNumber
		{
			get { return ChinaJournal.VoucherDocNumber; }
		}

		public ZString VoucherDocDate
		{
			get { return ChinaJournal.VoucherDocDate; }
		}

		public ZInt Attachments
		{
			get { return ChinaJournal.Attachments; }
		}

		public ZString Reviwer
		{
			get { return ChinaJournal.Reviwer; }
		}

		public ZString EnteredBy
		{
			get { return ChinaJournal.EnteredBy; }
		}

		public ZString Cashier
		{
			get { return ChinaJournal.Cashier; }
		}

		public ZString PreparedBy
		{
			get { return ChinaJournal.PreparedBy; }
		}

		public ZString AccountingFlag
		{
			get { return ChinaJournal.AccountingFlag.ToString(); }
		}

		public ZString VoidFlag
		{
			get { return ChinaJournal.VoidFlag.ToString(); }
		}

		public ZString VoucherSourceSystem
		{
			get { return ChinaJournal.VoucherSourceSystem; }
		}

		public ZString ReferenceInformation
		{
			get { return ChinaJournal.ReferenceInformation; }
		}
	}
}
