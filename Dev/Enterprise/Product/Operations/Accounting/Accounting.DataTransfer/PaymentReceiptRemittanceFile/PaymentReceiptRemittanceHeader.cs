using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class PaymentReceiptRemittanceHeader : IPaymentReceiptHeader
	{
		public PaymentReceiptRemittanceHeader(FlatFileDataRow row)
		{
			Argument.NotNull(row, nameof(row));
			Row = row;
		}

		readonly FlatFileDataRow Row;

		#region IPaymentReceiptHeader

		public ZString Type => Row[(int)PAYRECLine.Type];

		public ZString Ledger => Row[(int)PAYRECLine.Ledger];

		public ZString TransactionType => Row[(int)PAYRECLine.TransactionType];

		public ZDateTime InvoiceDate => Row.GetFieldAsZDateTime((int)PAYRECLine.ReceiptPaymentDate, "yyyyMMdd");

		public ZDateTime PostDate => Row.GetFieldAsZDateTime((int)PAYRECLine.PostDate, "yyyyMMdd");

		public ZString OrganizationCode => Row[(int)PAYRECLine.AccountCode];

		public ZString Description => Row[(int)PAYRECLine.Description];

		public ZString ReceiptPaymentType => Row[(int)PAYRECLine.ReceiptPaymentType];

		public ZString BankAccountCode => Row[(int)PAYRECLine.BankAccountCode];

		public ZString CheckBookCode => Row[(int)PAYRECLine.ChequeBook_PaymentOnly];

		public ZString CheckOrReference => Row[(int)PAYRECLine.ChequeOrReference];

		public ZString OSCurrencyCode => Row[(int)PAYRECLine.Currency];

		public ZString LocalCurrencyCode => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public ZDecimal OSTotal => Row.GetFieldAsZDecimal((int)PAYRECLine.OSAmount);

		public ZDecimal OSExGSTVATAmount => Row.GetFieldAsZDecimal((int)PAYRECLine.OSAmount);

		public ZDecimal LocalTotal => Row.GetFieldAsZDecimal((int)PAYRECLine.LocalAmount);

		public ZDecimal LocalExVATAmount => Row.GetFieldAsZDecimal((int)PAYRECLine.LocalAmount);

		public ZString BranchCode => Row[(int)PAYRECLine.Branch];

		public ZString DepartmentCode => Row[(int)PAYRECLine.Department];

		public ZString CheckDrawer => Row[(int)PAYRECLine.ChequeDrawer_ReceiptOnly];

		public ZString DrawerBank => Row[(int)PAYRECLine.ChequeDrawerBank_ReceiptOnly];

		public ZString DrawerBranch => Row[(int)PAYRECLine.ChequeDrawerBankBranch_ReceiptOnly];

		public ZString OverrideAddress => Row[(int)PAYRECLine.OverrideAddress_PaymentOnly];

		public ZString OverrideContact => Row[(int)PAYRECLine.OverrideContact_PaymentOnly];

		public bool IsContainOverrideAddress => Row.FieldCount > (int)PAYRECLine.OverrideAddress_PaymentOnly;
		public bool IsContainOverrideContact => Row.FieldCount > (int)PAYRECLine.OverrideContact_PaymentOnly;

		public ZString ThirdPartyReference => ZString.Empty;

		#endregion

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum PAYRECLine
		{
			Type,
			Ledger,
			TransactionType,
			ReceiptPaymentDate,
			PostDate,
			AccountCode,
			Description,
			ReceiptPaymentType,
			BankAccountCode,
			ChequeBook_PaymentOnly,
			ChequeOrReference,
			Currency,
			OSAmount,
			LocalAmount,
			Branch,
			Department,
			ChequeDrawer_ReceiptOnly,
			ChequeDrawerBank_ReceiptOnly,
			ChequeDrawerBankBranch_ReceiptOnly,
			OverrideAddress_PaymentOnly,
			OverrideContact_PaymentOnly
		}
	}
}
