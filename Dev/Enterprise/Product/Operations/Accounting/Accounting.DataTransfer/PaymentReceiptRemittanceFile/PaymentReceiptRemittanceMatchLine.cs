using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class PaymentReceiptRemittanceMatchLine : IPaymentReceiptMatchLine
	{
		public PaymentReceiptRemittanceMatchLine(FlatFileDataRow row)
		{
			Argument.NotNull(row, nameof(row));
			Row = row;
		}

		readonly FlatFileDataRow Row;

		public ZString Ledger => Row[(int)PTRLine.Ledger];

		public ZString TransactionType => Row[(int)PTRLine.TransactionType];

		public ZString TransactionNumber => Row[(int)PTRLine.TransactionNumber];

		public ZString OSCurrencyCode => Row[(int)PTRLine.Currency];

		public decimal AmountPaid => Row.GetFieldAsZDecimal((int)PTRLine.AmountPaid);

		public ZString OrganizationCode => Row[(int)PTRLine.Organisation];

		public ZString PaymentReference => Row[(int)PTRLine.PaymentReference];

		public ZString Description => Row[(int)PTRLine.TransactionDescription];

		public ZDateTime InvoiceDate => Row.GetFieldAsZDateTime((int)PTRLine.TransactionDate, "yyyyMMdd");

		public ZDateTime PostDate => Row.GetFieldAsZDateTime((int)PTRLine.PostDate, "yyyyMMdd");

		public ZDateTime DueDate => Row.GetFieldAsZDateTime((int)PTRLine.DueDate, "yyyyMMdd");

		public ZString LocalCurrencyCode => Row[(int)PTRLine.LocalCurrency];

		public decimal AmountPaidInLocalCurrency => Row.GetFieldAsZDecimal((int)PTRLine.AmountPaidInLocalCurrency);

		public ZString MatchStatus => Row[(int)PTRLine.MatchStatus];

		public ZString MatchStatusReasonCode => Row[(int)PTRLine.MatchStatusReasonCode];

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum PTRLine
		{
			Type,
			Ledger,
			TransactionType,
			TransactionNumber,
			AmountPaid,
			Organisation,
			PaymentReference,
			TransactionDescription,
			TransactionDate,
			PostDate,
			DueDate,
			Currency,
			AmountPaidInLocalCurrency,
			LocalCurrency,
			MatchStatus,
			MatchStatusReasonCode
		}
	}
}
