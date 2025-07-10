using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Client.MFI.Data;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Test.Data
{
	public class EStatementConverterTest : TestCaseWithFactory
	{
		public void TestProcessInvoiceHeader()
		{
			CsvFlatFileFormat formater = new CsvFlatFileFormat();
			FlatFileDataRow invoiceHeaderRow = formater.ConvertToRow("8maiicu1,maiicu1-200137,ic,cartm,,,,,,,,,,,,,,");
			Xsd.TxnHeader txnHeader = new Xsd.TxnHeader();
			Converter.ProcessInvoiceHeader(invoiceHeaderRow, txnHeader);
			AssertEquals("TxnHeader.TxnNumber", "maiicu1-200137", txnHeader.TxnNumber);
			AssertNotNull("TxnHeader.DebtorOrCreditor", txnHeader.DebtorOrCreditor);
			AssertEquals("TxnHeader.DebtorOrCreditor.EDICode", "8maiicu1", txnHeader.DebtorOrCreditor.EDICode);
			AssertEquals("TxnHeader.TxnReference", "maiicu1-200137", txnHeader.TxnReference);
			AssertEquals("TxnHeader.Ledger", TxnHeaderMapper.GetTxnHeaderLedger(LedgerTypes.AccountsPayable), txnHeader.Ledger);
			AssertEquals("TxnHeader.TxnType", TxnHeaderMapper.GetTxnHeaderTxnType(TransactionTypes.Invoice), txnHeader.TxnType);
			AssertEquals("TxnHeader.OsInvoiceAmtExclTax.CurrencyCode", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, txnHeader.OsInvoiceAmtExclTax.CurrencyCode);
		}

		public void TestProcessInvoiceLines()
		{
			CsvFlatFileFormat formater = new CsvFlatFileFormat();
			FlatFileDataRow invoiceLineRow = formater.ConvertToRow("11/09/2001,MJ0901795,2017798,,-1,0,,TR,,fuel,FLETCHER ALUMINIUM,901795,6,5.07,6168,234.38,29.3,263.68");
			Xsd.TxnHeader txnHeader = new Xsd.TxnHeader();
			Converter.ProcessInvoiceLines(invoiceLineRow, txnHeader);
			Xsd.TxnLine txnLine = txnHeader.TxnLines[0];
			AssertEquals("TxnLine.Description:", "11/09/2001, FLETCHER ALUMINIUM, 6, 5.07, 6168", txnLine.Description);
			AssertEquals("TxnLine.OsInvoiceAmtExclTax:", -234.38m, txnLine.OsInvoiceAmtExclTax.Value);
			AssertEquals("TxnLine.OsTaxAmount:", -29.3m, txnLine.OsTaxAmount.Value);
			AssertEquals("TxnLine.OsInvoiceAmtInclTax:", -263.68m, txnLine.OsInvoiceAmtInclTax.Value);
			AssertEquals("TxnLine.ConsolOrJobNo", "901795", txnLine.ConsolOrJobNo);
			AssertEquals("TxnLine.ChargeCode", "FUEL", txnLine.ChargeCode);
			AssertEquals("TxnLine.IsFinalCharge", false, txnLine.IsFinalCharge);
		}

		public void TestSetDescription()
		{
			CsvFlatFileFormat formater = new CsvFlatFileFormat();
			FlatFileDataRow invoiceLineRow = formater.ConvertToRow("11/09/2001,,,,,,,,,,Description,,Items,Measure,Weight,,,");
			Xsd.TxnLine txnLine = new Xsd.TxnLine();
			Converter.SetDescription(invoiceLineRow, txnLine);
			AssertEquals("Description should be:", "11/09/2001, Description, Items, Measure, Weight", txnLine.Description);
			invoiceLineRow = formater.ConvertToRow("11/09/2001,,,,,,,,,,,,Items,Measure,Weight,,,");
			Converter.SetDescription(invoiceLineRow, txnLine);
			AssertEquals("Description should be:", "11/09/2001, Items, Measure, Weight", txnLine.Description);
			invoiceLineRow = formater.ConvertToRow("11/09/2001,,,,,,,,,,,,,Measure,Weight,,,");
			Converter.SetDescription(invoiceLineRow, txnLine);
			AssertEquals("Description should be:", "11/09/2001, Measure, Weight", txnLine.Description);
			invoiceLineRow = formater.ConvertToRow("11/09/2001,,,,,,,,,,,,,,Weight,,,");
			Converter.SetDescription(invoiceLineRow, txnLine);
			AssertEquals("Description should be:", "11/09/2001, Weight", txnLine.Description);
			invoiceLineRow = formater.ConvertToRow("11/09/2001,,,,,,,,,,,,,,,,");
			Converter.SetDescription(invoiceLineRow, txnLine);
			AssertEquals("Description should be:", "11/09/2001", txnLine.Description);
		}

		public void TestEmptyChargeLineIsDefaultedToDefaultCharge()
		{
			CsvFlatFileFormat formater = new CsvFlatFileFormat();
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			FlatFileDataRow invoiceHeaderRow = formater.ConvertToRow("8maiicu1,maiicu1-200137,ic,cart,,,,,,,,,,,,,,");
			dataRows.Add(invoiceHeaderRow);
			FlatFileDataRow docHeaderRow = formater.ConvertToRow("Date,,,,,,,,,,,,,,,,,");
			dataRows.Add(docHeaderRow);
			FlatFileDataRow invoiceLineRow1 = formater.ConvertToRow("11/09/2001,,,Y,,,,,,CHARGECODE,,,,,,,,");
			dataRows.Add(invoiceLineRow1);
			FlatFileDataRow invoiceLineRow2 = formater.ConvertToRow("11/09/2001,,,,,,,,,,,,,,,,,");
			dataRows.Add(invoiceLineRow2);
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			Converter.InternalMapImportTest(txnHeaderCollection, dataRows);
			AssertEquals("Collection Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];
			AssertEquals("Converter.DefaultCharge", "CART", Converter.DefaultCharge);
			AssertEquals("Preassertion:", 2, txnHeader.TxnLines.Count);
			Xsd.TxnLine txnLine1 = txnHeader.TxnLines[0];
			AssertEquals("TxnLine.IsFinalCharge", true, txnLine1.IsFinalCharge);
			AssertEquals("TxnLine.IsFinalChargeSpecified", true, txnLine1.IsFinalChargeSpecified);
			AssertEquals("TxnLine1.ChargeCode", "CHARGECODE", txnLine1.ChargeCode);
			Xsd.TxnLine txnLine2 = txnHeader.TxnLines[1];
			AssertEquals("TxnLine.IsFinalCharge", false, txnLine2.IsFinalCharge);
			AssertEquals("TxnLine.IsFinalChargeSpecified", false, txnLine2.IsFinalChargeSpecified);
			AssertEquals("TxnLine2.ChargeCode", "CART", txnLine2.ChargeCode);
		}

		public void TestLinesWithEmptyDateAreNotProcessed()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("DataImportExport.EmptyDateLines_Sample.csv");
				using (StreamReader reader = new StreamReader(testFilePath))
				{
					Converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
				}

				AssertEquals("Collection Count", 1, txnHeaderCollection.Count);
				Xsd.TxnHeader txnHeader = txnHeaderCollection[0];
				AssertEquals("TxnHeader.TxnLines.Count", 0, txnHeader.TxnLines.Count);
			}
		}

		public void TestIsDataValidWithValidFile()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("DataImportExport.ValidData_Sample.csv");
				FlatFileDataRowCollection fileLines = GetFileLines(testFilePath);
				AssertEquals("Should be valid", true, Converter.IsDataValid(fileLines));
			}
		}

		public void TestIsDataValidWithInvalidFile()
		{
			using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testFilePath = resourceRetriever.SaveResourceToFile("DataImportExport.InvalidData_Sample.csv");

				FlatFileDataRowCollection fileLines = GetFileLines(testFilePath);
				AssertEquals("Should be valid", false, Converter.IsDataValid(fileLines));
			}
		}

		public void TestMapImportGeneratesErrorWhenInvalidFile()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
				using (EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					string testFilePath = resourceRetriever.SaveResourceToFile("DataImportExport.InvalidData_Sample.csv");
					FlatFileDataRowCollection fileLines = GetFileLines(testFilePath);
					Converter.InternalMapImportTest(txnHeaderCollection, fileLines);
					AssertEquals("Collection Count", 1, txnHeaderCollection.Count);
					Xsd.TxnHeader txnHeader = txnHeaderCollection[0];
					AssertEquals("Should have errors", true, ((NotificationBuffer)Converter.InternalNotificationTest).ContainsNotificationType(ErrorType.InvalidFileFormat));
				}
		}

		public FlatFileDataRowCollection GetFileLines(string fileName)
		{
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			using (StreamReader reader = new StreamReader(fileName))
			{
				string dataLine;
				FlatFileFormat fileFormat = new CsvFlatFileFormat(false);
				while ((dataLine = reader.ReadLine()) != null)
				{
					FlatFileDataRow flatFileLine = Converter.InternalExtractRowFromFormatTest(fileFormat, dataLine);
					if (flatFileLine != null)
					{
						fileLines.Add(flatFileLine);
					}
				}
			}

			return fileLines;
		}

		EStatementConverter Converter;
		protected override void SetUp()
		{
			base.SetUp();
			NotificationBuffer notify = new NotificationBuffer();
			Converter = new EStatementConverter(notify, Factory);
		}
	}
}
