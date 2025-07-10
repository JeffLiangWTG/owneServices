using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.Accounting.Netting;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class PaymentReceiptRemittanceFileConverterTest : TestCaseWithFactory
	{
		NotificationTestHelper NotificationTestHelper
		{
			get { return notificationTestHelper ?? (notificationTestHelper = new NotificationTestHelper()); }
		}
		NotificationTestHelper notificationTestHelper;

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertCSVContentToXsd()
		{
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT.csv"))
			{
				var csvContent = reader.ReadToEnd();

				NotificationBuffer notificationBuffer = new NotificationBuffer();
				PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
				var xsd = converter.ConvertCSVContentToXsd(csvContent);

				AssertNotNull(xsd);
				AssertType(typeof(FinancialInvoices), xsd);

				TxnHeaderCollection txnHeaderCollection = ((FinancialInvoices)xsd).TxnHeader;
				AssertEquals(7, txnHeaderCollection.Count);
			}
		}

		public void TestJournalConverter()
		{
			NettingObjectCreator creator = new NettingObjectCreator(Factory);
			var nettingSystem = creator.CreateNettingSystem("NS", "Netting System", GlbCompany.CurrentCompany);
			var org = creator.AALSHI;
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDAU");

			creator.CreateNettingOrganisation(nettingSystem, org, "FUL");

			Factory.Save();

			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("NCL,AP,JNL,20150115,20150116,AALSHI,Description,AUD,500,AUD,500,SYD,FEA");
				writer.WriteLine("PTR,AP,INV,00001003,-300,AALSHI");
				writer.WriteLine("PTR,AP,INV,00001004,-200,AALSHI");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(TxnLedgerType.AP, headerCollection[0].Ledger);
					AssertEquals(TxnType.JNL, headerCollection[0].TxnType);
					AssertEquals(new ZDateTime(2015, 01, 15), headerCollection[0].PostDate);
					AssertEquals(new ZDateTime(2015, 01, 16), headerCollection[0].DueDate);

					AssertEquals("AALSHI", headerCollection[0].DebtorOrCreditor.EDICode);
					AssertEquals("Description", headerCollection[0].Description);

					AssertEquals(500M, headerCollection[0].OsInvoiceAmtInclTax.Value);
					AssertEquals("AUD", headerCollection[0].OsInvoiceAmtInclTax.CurrencyCode);

					AssertEquals("SYD", headerCollection[0].Branch);
					AssertEquals("FEA", headerCollection[0].Department);

					AssertEquals(2, headerCollection[0].PaidTransactions.Count);
					AssertEquals(TxnLedgerType.AP, headerCollection[0].PaidTransactions[0].Ledger);
					AssertEquals(TxnType.INV, headerCollection[0].PaidTransactions[0].TxnType);
					AssertEquals("00001003", headerCollection[0].PaidTransactions[0].TxnNumber);
					AssertEquals(300M, headerCollection[0].PaidTransactions[0].AmountPaidThisPayment.Value);
					AssertEquals("AALSHI", headerCollection[0].PaidTransactions[0].DebtorOrCreditor.EDICode);

					AssertEquals(TxnLedgerType.AP, headerCollection[0].PaidTransactions[1].Ledger);
					AssertEquals(TxnType.INV, headerCollection[0].PaidTransactions[1].TxnType);
					AssertEquals("00001004", headerCollection[0].PaidTransactions[1].TxnNumber);
					AssertEquals(200M, headerCollection[0].PaidTransactions[1].AmountPaidThisPayment.Value);
					AssertEquals("AALSHI", headerCollection[0].PaidTransactions[1].DebtorOrCreditor.EDICode);
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20090115,20090116,ABIGAS,TEST PMT,CSH,AUD,AUDC,1,AUD,500,800,SYD,FEA,Drw,Bnk,Brn");
				writer.WriteLine("PTR,AP,INV,APINV2,100");
				writer.WriteLine("REC,AR,REC,20090115,20090117,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.WriteLine("PTR,AR,INV,00001003,1500,AALSHI,TEST REF,TEST DESCRIPTION,20090117,20090118,20090119,EUR,90,UAH");
				writer.WriteLine("PTR,AR,CRD,00001007,-800,ABIGAS,TEST REF,TEST DESCRIPTION,20090117,20090118,20090119,,90,");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(2, headerCollection.Count);

					AssertEquals(TxnLedgerType.AP, headerCollection[0].Ledger);
					AssertEquals(TxnType.PAY, headerCollection[0].TxnType);
					AssertEquals(new ZDateTime(2009, 01, 15), headerCollection[0].InvoiceDate);
					AssertEquals(new ZDateTime(2009, 01, 16), headerCollection[0].PostDate);
					AssertEquals("ABIGAS", headerCollection[0].DebtorOrCreditor.EDICode);
					AssertEquals("TEST PMT", headerCollection[0].Description);
					AssertEquals(TxnHeaderReceiptPaymentType.CSH, headerCollection[0].ReceiptPaymentType);
					AssertEquals("AUD", headerCollection[0].BankCode);
					AssertEquals("AUDC", headerCollection[0].ChequeBook);
					AssertEquals("1", headerCollection[0].ChequeOrReference);
					AssertEquals(500M, headerCollection[0].OsInvoiceAmtInclTax.Value);
					AssertEquals("AUD", headerCollection[0].OsInvoiceAmtInclTax.CurrencyCode);
					AssertEquals(500M, headerCollection[0].OsInvoiceAmtExclTax.Value);
					AssertEquals("AUD", headerCollection[0].OsInvoiceAmtExclTax.CurrencyCode);
					AssertEquals(800M, headerCollection[0].LocalInvoiceAmtInclTax.Value);
					AssertEquals("AUD", headerCollection[0].LocalInvoiceAmtInclTax.CurrencyCode);
					AssertEquals(800M, headerCollection[0].LocalInvoiceAmtExclTax.Value);
					AssertEquals("AUD", headerCollection[0].LocalInvoiceAmtExclTax.CurrencyCode);
					AssertEquals("SYD", headerCollection[0].Branch);
					AssertEquals("FEA", headerCollection[0].Department);
					AssertEquals("", headerCollection[0].ChequeDrawer);
					AssertEquals("", headerCollection[0].DrawerBank);
					AssertEquals("", headerCollection[0].DrawerBankBranch);

					AssertEquals(1, headerCollection[0].PaidTransactions.Count);
					AssertEquals(TxnLedgerType.AP, headerCollection[0].PaidTransactions[0].Ledger);
					AssertEquals(TxnType.INV, headerCollection[0].PaidTransactions[0].TxnType);
					AssertEquals("APINV2", headerCollection[0].PaidTransactions[0].TxnNumber);
					AssertEquals(-100M, headerCollection[0].PaidTransactions[0].AmountPaidThisPayment.Value);
					AssertEquals("", headerCollection[0].PaidTransactions[0].AmountPaidThisPayment.CurrencyCode);
					AssertEquals(-100M, headerCollection[0].PaidTransactions[0].OsInvoiceAmtInclTax.Value);
					AssertEquals("", headerCollection[0].PaidTransactions[0].OsInvoiceAmtInclTax.CurrencyCode);
					AssertEquals(false, headerCollection[0].PaidTransactions[0].DebtorOrCreditor.IsSpecified);

					AssertEquals(TxnLedgerType.AR, headerCollection[1].Ledger);
					AssertEquals(TxnType.REC, headerCollection[1].TxnType);
					AssertEquals(new ZDateTime(2009, 01, 15), headerCollection[1].InvoiceDate);
					AssertEquals(new ZDateTime(2009, 01, 17), headerCollection[1].PostDate);
					AssertEquals("AALSHI", headerCollection[1].DebtorOrCreditor.EDICode);
					AssertEquals("TEST RCT", headerCollection[1].Description);
					AssertEquals(TxnHeaderReceiptPaymentType.CHQ, headerCollection[1].ReceiptPaymentType);
					AssertEquals("USD", headerCollection[1].BankCode);
					AssertEquals("", headerCollection[1].ChequeBook);
					AssertEquals("103", headerCollection[1].ChequeOrReference);
					AssertEquals(-1000M, headerCollection[1].OsInvoiceAmtInclTax.Value);
					AssertEquals("USD", headerCollection[1].OsInvoiceAmtInclTax.CurrencyCode);
					AssertEquals(-1500M, headerCollection[1].LocalInvoiceAmtInclTax.Value);
					AssertEquals("AUD", headerCollection[1].LocalInvoiceAmtInclTax.CurrencyCode);
					AssertEquals("SYD", headerCollection[1].Branch);
					AssertEquals("FEA", headerCollection[1].Department);
					AssertEquals("Drw", headerCollection[1].ChequeDrawer);
					AssertEquals("Bnk", headerCollection[1].DrawerBank);
					AssertEquals("Brn", headerCollection[1].DrawerBankBranch);

					AssertEquals(2, headerCollection[1].PaidTransactions.Count);
					AssertEquals(TxnLedgerType.AR, headerCollection[1].PaidTransactions[0].Ledger);
					AssertEquals(TxnType.INV, headerCollection[1].PaidTransactions[0].TxnType);
					AssertEquals("00001003", headerCollection[1].PaidTransactions[0].TxnNumber);
					AssertEquals(1500M, headerCollection[1].PaidTransactions[0].AmountPaidThisPayment.Value);
					AssertEquals("", headerCollection[1].PaidTransactions[0].AmountPaidThisPayment.CurrencyCode);
					AssertEquals(1500M, headerCollection[1].PaidTransactions[0].OsInvoiceAmtInclTax.Value);
					AssertEquals("EUR", headerCollection[1].PaidTransactions[0].OsInvoiceAmtInclTax.CurrencyCode);
					AssertEquals("AALSHI", headerCollection[1].PaidTransactions[0].DebtorOrCreditor.EDICode);
					AssertEquals("TEST REF", headerCollection[1].PaidTransactions[0].PaymentReference);
					AssertEquals("TEST DESCRIPTION", headerCollection[1].PaidTransactions[0].Description);
					AssertEquals(new ZDateTime(2009, 01, 17), headerCollection[1].PaidTransactions[0].InvoiceDate);
					AssertEquals(new ZDateTime(2009, 01, 18), headerCollection[1].PaidTransactions[0].PostDate);
					AssertEquals(new ZDateTime(2009, 01, 19), headerCollection[1].PaidTransactions[0].DueDate);
					AssertEquals(90M, headerCollection[1].PaidTransactions[0].LocalInvoiceAmtInclTax.Value);
					AssertEquals("UAH", headerCollection[1].PaidTransactions[0].LocalInvoiceAmtInclTax.CurrencyCode);

					AssertEquals(TxnLedgerType.AR, headerCollection[1].PaidTransactions[1].Ledger);
					AssertEquals(TxnType.CRD, headerCollection[1].PaidTransactions[1].TxnType);
					AssertEquals("00001007", headerCollection[1].PaidTransactions[1].TxnNumber);
					AssertEquals(800M, headerCollection[1].PaidTransactions[1].AmountPaidThisPayment.Value);
					AssertEquals("", headerCollection[1].PaidTransactions[1].AmountPaidThisPayment.CurrencyCode);
					AssertEquals(800M, headerCollection[1].PaidTransactions[1].OsInvoiceAmtInclTax.Value);
					AssertEquals("", headerCollection[1].PaidTransactions[1].OsInvoiceAmtInclTax.CurrencyCode);
					AssertEquals("ABIGAS", headerCollection[1].PaidTransactions[1].DebtorOrCreditor.EDICode);
					AssertEquals("TEST REF", headerCollection[1].PaidTransactions[1].PaymentReference);
					AssertEquals("TEST DESCRIPTION", headerCollection[1].PaidTransactions[1].Description);
					AssertEquals(new ZDateTime(2009, 01, 17), headerCollection[1].PaidTransactions[1].InvoiceDate);
					AssertEquals(new ZDateTime(2009, 01, 18), headerCollection[1].PaidTransactions[1].PostDate);
					AssertEquals(new ZDateTime(2009, 01, 19), headerCollection[1].PaidTransactions[1].DueDate);
					AssertEquals(0M, headerCollection[1].PaidTransactions[1].LocalInvoiceAmtInclTax.Value);
					AssertEquals("", headerCollection[1].PaidTransactions[1].LocalInvoiceAmtInclTax.CurrencyCode);

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter_IncorrectLineOrder()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PTR,AP,INV,APINV2,100");
				writer.WriteLine("REC,AR,REC,20090115,20090117,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.WriteLine("PTR,AR,INV,00001003,1500,AALSHI");
				writer.WriteLine("PTR,AR,CRD,00001007,-800,ABIGAS");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(2, headerCollection[0].PaidTransactions.Count);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "'PTR' line type can't be the first line in a file.");
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter_LessThanTwoPTRLinesForMHR()
		{
			var headerCollection = new TxnHeaderCollection();
			var testStream1 = new MemoryStream();
			var testStream2 = new MemoryStream();
			using (var writer1 = new StreamWriter(testStream1))
			{
				writer1.WriteLine("MHR,20090302");
				writer1.Flush();
				testStream1.Position = 0;

				using (var reader = new StreamReader(testStream1))
				{
					var notificationBuffer = new NotificationBuffer();
					var converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(0, headerCollection.Count);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "The MHR line type must be accompanied by at least two PTR lines.");
				}
			}

			using (var writer2 = new StreamWriter(testStream2))
			{
				writer2.WriteLine(@"MHR,20090302
MHR,20090115
PTR,AP,INV,APINV45647,-100,AALSHI,,,,,,,,,");
				writer2.Flush();
				testStream2.Position = 0;

				using (var reader = new StreamReader(testStream2))
				{
					var notificationBuffer = new NotificationBuffer();
					var converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(0, headerCollection[0].PaidTransactions.Count);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "The MHR line type must be accompanied by at least two PTR lines.");
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter_IncorrectLineType()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("REC,AR,REC,20090115,20090117,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.WriteLine("XXX,AR,INV,00001003,1500,AALSHI");
				writer.WriteLine("PTR,AR,CRD,00001007,-800,ABIGAS");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(1, headerCollection[0].PaidTransactions.Count);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Unrecognized row type was detected. Only 'MHR', 'REC', 'PAY' and 'PTR' row types are used for import. The following row will be ignored: 'XXX'.");
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter_IncorrectTransactionType()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("REC,AR,REC,20090115,20090117,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.WriteLine("PTR,AR,INV,00001003,1500,AALSHI");
				writer.WriteLine("PTR,AR,ADJ,00001005,500,AALSHI");
				writer.WriteLine("PTR,AR,JNL,00001002,1000,AALSHI");
				writer.WriteLine("PTR,AR,CRD,00001007,-800,ABIGAS");
				writer.WriteLine("PTR,AR,REC,00001008,-300,ABIGAS");
				writer.WriteLine("PTR,AR,PAY,00001009,200,AALSHI");
				writer.WriteLine("PTR,AR,TRF,00001010,400,AALSHI");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(6, headerCollection[0].PaidTransactions.Count);

					Assert(!notificationBuffer.HasErrors);
					Assert(notificationBuffer.HasWarnings);

					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Unrecognized transaction type was detected. Only 'INV', 'CRD', 'ADJ', 'JNL', 'REC', 'PAY, 'EXX', 'DSC' and 'OVP' types are used for import. The following transaction type will be ignored: 'TRF'.");
				}
			}
		}

		[TestDate(2009, 03, 02)]
		public void TestConverter_IncorrectDate()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("REC,AR,REC,090115,17/01/2009,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Invalid Date format. Only YYYYMMDD format is supported.");
				}
			}
		}
		public void TestConverter_DateOutofRange()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("REC,AR,REC,22090115,21090115,AALSHI,TEST RCT,CHQ,USD,AUDC,103,USD,1000,1500,SYD,FEA,Drw,Bnk,Brn");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);

					Assert(notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
					NotificationTestHelper.AssertNotificationsContainsErrorMessage(notificationBuffer, "Date out of range");
				}
			}
		}

		[TestDate(2011, 09, 07)]
		public void TestPaidTransactionAmountsShouldNotRound()
		{
			GlbCompany.CurrentCompany.SetCurrency("AUD");
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20090115,20090115,AALSHI,Payment (JPY bank currency),CHQ,JPY,JPYC,1,JPY,500.5555555555,250.5555555555,SYD,FEA,,,");
				writer.WriteLine("PTR,AP,INV,APINV112,-525.5555555555,AALSHI,,,,,,,,,,,,,");
				writer.WriteLine("PTR,AP,INV,APINV113,-525.5555555555,AALSHI,,,,,,EUR,90.5555555555,AUD");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					AssertEquals(2, headerCollection[0].PaidTransactions.Count);
					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);

					AssertEquals(0, headerCollection[0].OsInvoiceAmtInclTax.Value.DecimalPlaces);
					AssertEquals(0, headerCollection[0].OsInvoiceAmtExclTax.Value.DecimalPlaces);
					AssertEquals(2, headerCollection[0].LocalInvoiceAmtInclTax.Value.DecimalPlaces);
					AssertEquals(2, headerCollection[0].LocalInvoiceAmtExclTax.Value.DecimalPlaces);

					AssertEquals(RefCurrency.MAX_DECIMAL, headerCollection[0].PaidTransactions[0].AmountPaidThisPayment.Value.DecimalPlaces);
					AssertEquals(RefCurrency.MAX_DECIMAL, headerCollection[0].PaidTransactions[0].OsInvoiceAmtInclTax.Value.DecimalPlaces);

					AssertEquals(RefCurrency.MAX_DECIMAL, headerCollection[0].PaidTransactions[1].AmountPaidThisPayment.Value.DecimalPlaces);
					AssertEquals(RefCurrency.MAX_DECIMAL, headerCollection[0].PaidTransactions[1].OsInvoiceAmtInclTax.Value.DecimalPlaces);
					AssertEquals(2, headerCollection[0].PaidTransactions[1].LocalInvoiceAmtInclTax.Value.DecimalPlaces);
				}
			}
		}

		public void TestConverter_TestCurrencyConversion()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20111121,20111121,HANTOKALV,1,CSH,UBHHECOR,UBHHECOR,1,JPY,17162,216.6,,,,,");
				writer.WriteLine("PTR,AP,INV,45,-17162,UBHHECOR,1,,,,,,,,,,,,,");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					Assert(!notificationBuffer.HasWarnings);

					ZDecimal expected = 216.6;
					AssertEquals(expected, headerCollection[0].LocalInvoiceAmtInclTax.Value);
				}
			}
		}

		public void TestConverter_PaymentOverrideAddressAndOverrideContact()
		{
			TxnHeaderCollection headerCollection = new TxnHeaderCollection();
			MemoryStream testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20151014,20151014,AALSHI,Payment1,CSH,BRT,,1,AUD,100,100,SYD,FEA,,,,AddrShortCode123,ContactName123");
				writer.WriteLine("PTR,AP,INV,INV8001,-100");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					NotificationBuffer notificationBuffer = new NotificationBuffer();
					PaymentReceiptRemittanceFileConverter converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);

					var header = headerCollection[0];
					AssertEquals(header.TxnOverrideAddress.AddressCode, "AddrShortCode123");
					AssertEquals(header.TxnOverrideContact.Name, "ContactName123");

					Assert(!notificationBuffer.HasErrors);
					Assert(!notificationBuffer.HasWarnings);
				}
			}
		}

		public void TestConverter_MatchStatus()
		{
			var headerCollection = new TxnHeaderCollection();
			var testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20111121,20111121,HANTOKALV,1,CSH,UBHHECOR,UBHHECOR,1,JPY,17162,216.6,,,,,");
				writer.WriteLine("PTR,AP,INV,45,-17162,UBHHECOR,1,,,,,,,,UAC,");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					var notificationBuffer = new NotificationBuffer();
					var converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					Assert(!notificationBuffer.HasWarnings);

					var expected = AccountingConstants.MatchStatusTypes.Unallocated.Code;
					AssertEquals(expected, headerCollection[0].PaidTransactions[0].MatchStatus);
				}
			}
		}

		public void TestConverter_MatchStatusReasonCode()
		{
			var headerCollection = new TxnHeaderCollection();
			var testStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(testStream))
			{
				writer.WriteLine("PAY,AP,PAY,20111121,20111121,HANTOKALV,1,CSH,UBHHECOR,UBHHECOR,1,JPY,17162,216.6,,,,,");
				writer.WriteLine("PTR,AP,INV,45,-17162,UBHHECOR,1,,,,,,,,UAC,ADV");
				writer.Flush();
				testStream.Position = 0;

				using (StreamReader reader = new StreamReader(testStream))
				{
					var notificationBuffer = new NotificationBuffer();
					var converter = new PaymentReceiptRemittanceFileConverter(notificationBuffer, Factory);
					converter.ImportFlatFile(headerCollection, new CsvFlatFileFormat(), reader);

					AssertEquals(1, headerCollection.Count);
					Assert(!notificationBuffer.HasWarnings);

					var expected = AccountingConstants.MatchStatusReasonCodeTypes.InAdvance.Code;
					AssertEquals(expected, headerCollection[0].PaidTransactions[0].MatchStatusReasonCode);
				}
			}
		}
	}
}
