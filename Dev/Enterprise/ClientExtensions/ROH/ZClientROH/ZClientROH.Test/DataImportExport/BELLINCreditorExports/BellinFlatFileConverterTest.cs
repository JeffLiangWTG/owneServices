using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.Bellin.Testing
{
	public class BellinFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestExport()
		{
			BellinFlatFileConverterTestClass converter = new BellinFlatFileConverterTestClass(new NotificationBuffer(), Factory);
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.TxnNumber = "000011111";
			header.DebtorOrCreditor.EDICode = "EDIORG";
			ZDateTime invoiceDate = header.InvoiceDate = new ZDateTime(2005, 12, 29, 7, 50, 4);
			ZDateTime dueDate = header.DueDate = new ZDateTime(2005, 12, 30, 12, 5, 29);
			header.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)2233.23m, Core.Constants.CurrencyCodes.NewZealand);
			FlatFileDataRowCollection rows = converter.MapExport(header);
			AssertEquals("One row should be created", 1, rows.Count);
			BellinFlatFileDataRow row = (BellinFlatFileDataRow)rows[0];
			AssertEquals("Invoice Number", "000011111", row.InvoiceNumber);
			AssertEquals("Counterpart Number/ Creditor EDICode", "EDIORG", row.CounterpartNumber);
			AssertEquals("Invoice Date", invoiceDate.Date, row.InvoiceDate);
			AssertEquals("Maturity Date/ Due Date", dueDate.Date, row.MaturityDate);
			AssertEquals("Amount", -2233.23m, row.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.NewZealand, row.Currency);
			AssertEquals("Invoice Flag", ZString.Empty, row.InvoiceFlag);
			AssertEquals("Comment", ZString.Empty, row.Comment);
			AssertEquals("Delivery Note Number", ZString.Empty, row.DeliveryNoteNumber);
			AssertEquals("External Document Number", ZString.Empty, row.ExternalDocumentNumber);
			AssertEquals("Booking Date", ZDateTime.Empty, row.BookingDate);
			AssertEquals("Remark", ZString.Empty, row.Remark);
		}

		public void TestAmountMultipliedByNegativeOne()
		{
			BellinFlatFileConverterTestClass converter = new BellinFlatFileConverterTestClass(new NotificationBuffer(), Factory);
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)100m, Core.Constants.CurrencyCodes.NewZealand);
			FlatFileDataRowCollection rows = converter.MapExport(header);
			AssertEquals("One row should be created", 1, rows.Count);
			BellinFlatFileDataRow row = (BellinFlatFileDataRow)rows[0];
			AssertEquals("Amount", -100m, row.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.NewZealand, row.Currency);
			header.OsInvoiceAmtInclTax = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)(-100m), Core.Constants.CurrencyCodes.NewZealand);
			rows = converter.MapExport(header);
			AssertEquals("One row should be created", 1, rows.Count);
			row = (BellinFlatFileDataRow)rows[0];
			AssertEquals("Amount", 100m, row.Amount);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.NewZealand, row.Currency);
		}

		public void TestCheckThatAllTransactionsAreExported()
		{
			BellinFlatFileConverterTestClass converter = new BellinFlatFileConverterTestClass(new NotificationBuffer(), Factory);
			AssertEquals("Check That ALl Transactions Are Exported should be true", ZBool.True, converter.CheckThatAllTransactionsAreExported);
		}

		public void TestIncrementTransactionsProcessed()
		{
			BellinFlatFileConverterTestClass converter = new BellinFlatFileConverterTestClass(new NotificationBuffer(), Factory);
			ProcessTransaction(converter, Xsd.TxnType.ADJ);
			AssertEquals("Number of adjustment notes process should be 1", 1, converter.NumberOfAdjustmentNotesProcessed);
			ProcessTransaction(converter, Xsd.TxnType.CRD);
			AssertEquals("Number of credit notes notes process should be 1", 1, converter.NumberOfAdjustmentNotesProcessed);
			ProcessTransaction(converter, Xsd.TxnType.INV);
			AssertEquals("Number of invoices notes process should be 1", 1, converter.NumberOfAdjustmentNotesProcessed);
		}

		void ProcessTransaction(BellinFlatFileConverterTestClass converter, Xsd.TxnType txnType)
		{
			Xsd.TxnHeader header = new Xsd.TxnHeader();
			header.TxnType = txnType;
			converter.MapExport(header);
		}

		#region Implementation
		class BellinFlatFileConverterTestClass : BellinFlatFileConverter
		{
			public BellinFlatFileConverterTestClass(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
			{
			}

			public new FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				return base.MapExport(valueObject);
			}
		}
		#endregion
	}
}
