using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class YusenDDRFileGenerator_InnerTest : DDRFileCreatorBaseTest
	{
		public void TestWriteDescriptiveHeaderRecord()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "038-304";
			bank.AB_AccountNum = "273482130";

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_AB = bank.PK;

			StringWriter writer = new StringWriter();
			YusenDDRFileGenerator generator = new YusenDDRFileGenerator(writer, dDRHeader);
			generator.WriteDescriptiveHeaderRecord_ForTestOnly(dDRHeader);

			string headerString = writer.ToString();

			AssertEquals("DescriptiveHeaderRecord should not be written", string.Empty, writer.ToString());
		}

		[TestDate(2018, 07, 14)]
		public void TestWriteDetailRecord()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "5138345";
			accountDetails.A1_BankAccount = "309932005";
			accountDetails.A1_AccountName = "Bank of Australia";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Payables.ToString();
			document.OD_FilterShipmentMode = Core.Constants.TransportModes.All;
			document.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "1385-999";
			bank.AB_AccountNum = "5988662002";
			bank.AB_BankAccountName = "Goldblatt Resources Ltd";

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = org.PK;
			payment.AH_AB = bank.PK;
			payment.AH_OSExTaxAmount = 4000.00m;
			payment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			payment.AH_ChequeOrReference = "284908";
			payment.AH_OutstandingAmount = 0.00m;
			payment.AH_FullyPaidDate = ZDateTime.Today;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
			line.FillWithValidTestData();
			line.AL_OSExTaxAmount = 4000.00m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice.AH_OutstandingAmount = 0.00m;
			invoice.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink matchLink1 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice.PK;
			matchLink1.AP_MatchGroupNum = "98765";
			matchLink1.AP_Amount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;

			TransactionMatchLink matchLink2 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = payment.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			matchLink2.AP_Amount = payment.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();

			AssertEquals("invoice line amount", 4000.00m, invoice.Lines[0].AL_OSExTaxAmount);

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_AB = bank.PK;
			dDRHeader.AH_ChequeOrReference = "20091120BLAH";
			Factory.Save();

			StringWriter writer = new StringWriter();
			YusenDDRFileGenerator generator = new YusenDDRFileGenerator(writer, dDRHeader);
			generator.WriteDetailRecord_ForTestOnly(payment, "1");

			string[] rows = writer.ToString().Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
			AssertEquals("3 rows should have been created (including an empty one)", 3, rows.Length);

			AssertDetailsForHeaderRecord(rows[0]);
			AssertDetailsForAdviceRecord(rows[1]);
			AssertEquals("row3 should be empty", string.Empty, rows[2]);
		}

		public void TestWriteTrailerRecord()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			StringWriter writer = new StringWriter();

			YusenDDRFileGenerator generator = new YusenDDRFileGenerator(writer, dDRHeader);
			generator.WriteFileTotalRecord_ForTestOnly(99.33m, "0", "blah");
			string trailer = writer.ToString();
			AssertEquals("WriteTotalRecord should not do anything", string.Empty, trailer);
		}

		[TestDate(2018, 07, 14)]
		public void TestWriteAllRecords()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "5138345";
			accountDetails.A1_BankAccount = "309932005";
			accountDetails.A1_AccountName = "Bank of Australia";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_IsDefaultAccount = true;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Payables.ToString();
			document.OD_FilterShipmentMode = Core.Constants.TransportModes.All;
			document.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "1385-999";
			bank.AB_AccountNum = "5988662002";
			bank.AB_BankAccountName = "Goldblatt Resources Ltd";

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_OH = org.PK;
			payment.AH_AB = bank.PK;
			payment.AH_OSExTaxAmount = 4000.00m;
			payment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			payment.AH_ChequeOrReference = "284908";
			payment.AH_OutstandingAmount = 0.00m;
			payment.AH_FullyPaidDate = ZDateTime.Today;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine line = invoice.Lines.AddNew() as APInvoiceLine;
			line.FillWithValidTestData();
			line.AL_OSExTaxAmount = 4000.00m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice.AH_OutstandingAmount = 0.00m;
			invoice.AH_FullyPaidDate = ZDateTime.Today;

			TransactionMatchLink matchLink1 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice.PK;
			matchLink1.AP_MatchGroupNum = "98765";
			matchLink1.AP_Amount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;

			TransactionMatchLink matchLink2 = ((IMatching)invoice).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = payment.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			matchLink2.AP_Amount = payment.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);

			Factory.Save();

			AssertEquals("invoice line amount", 4000.00m, invoice.Lines[0].AL_OSExTaxAmount);

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_TransactionNum = ZString.Empty;
			dDRHeader.AH_AB = bank.PK;
			dDRHeader.AH_ChequeOrReference = "20091120BLAH";
			Factory.Save();

			StringWriter writer = new StringWriter();
			YusenDDRFileGenerator generator = new YusenDDRFileGenerator(writer, dDRHeader);
			generator.Create();

			string dDRFileContents = writer.ToString();
			string expectedPostDate = dDRHeader.AH_PostDate.ToString("yyyyMMdd");

			string[] rows = dDRFileContents.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
			AssertEquals("3 rows should have been created (including an empty one)", 3, rows.Length);
			AssertDetailsForHeaderRecord(rows[0]);
			AssertDetailsForAdviceRecord(rows[1]);
			AssertEquals("row3", "", rows[2]);
		}

		static void AssertDetailsForHeaderRecord(string row1AsString)
		{
			string[] row1 = row1AsString.Split(',');
			int count = 0;
			string emptyField = "\"\"";

			AssertEquals("row1 length", 41, row1.Length);

			AssertEquals(" 1. Serial Number", emptyField, row1[count++]);
			AssertEquals(" 2. Payment Method", "\"GI\"", row1[count++]);
			AssertEquals(" 3. Transaction Code", "\"MIS\"", row1[count++]);
			AssertEquals(" 4. Record Type", "\"T\"", row1[count++]);
			AssertEquals(" 5. Value Date", "\"20091120\"", row1[count++]);
			AssertEquals(" 6. Account No. with BTM", "\"59886620\"", row1[count++]);
			AssertEquals(" 7. Currency", "\"AUD\"", row1[count++]);
			AssertEquals(" 8. Amount", "\"4000.00\"", row1[count++]);
			AssertEquals(" 9. Beneficiary Name 1", "\"Bank of Australia\"", row1[count++]);
			AssertEquals("10. Beneficiary Name 2", emptyField, row1[count++]);
			AssertEquals("11. Beneficiary Attention", emptyField, row1[count++]);
			AssertEquals("12. Contact", emptyField, row1[count++]);
			AssertEquals("13. Beneficiary Address 1", emptyField, row1[count++]);
			AssertEquals("14. Beneficiary Address 2", emptyField, row1[count++]);
			AssertEquals("15. Beneficiary Address 3", emptyField, row1[count++]);
			AssertEquals("16. Beneficiary Country", emptyField, row1[count++]);
			AssertEquals("17. Beneficiary Account No.", "\"309932005\"", row1[count++]);
			AssertEquals("18. Beneficiary Bank Name", emptyField, row1[count++]);
			AssertEquals("19. Beneficiary Bank Code", "\"5138\"", row1[count++]);
			AssertEquals("20. Beneficiary Branch Name", emptyField, row1[count++]);
			AssertEquals("21. Beneficiary Branch Code", "\"345\"", row1[count++]);
			AssertEquals("22. Beneficiary Bank Address 1", emptyField, row1[count++]);
			AssertEquals("23. Beneficiary Bank Address 2", emptyField, row1[count++]);
			AssertEquals("24. Beneficiary Bank Address 3", emptyField, row1[count++]);
			AssertEquals("25. Beneficiary Bank Country", emptyField, row1[count++]);
			AssertEquals("26. Beneficiary Bank BIC", emptyField, row1[count++]);
			AssertEquals("27. Intermediary Bank Name", emptyField, row1[count++]);
			AssertEquals("28. Intermediary Bank Branch Name", emptyField, row1[count++]);
			AssertEquals("29. Intermediary Bank Address1", emptyField, row1[count++]);
			AssertEquals("30. Intermediary Bank Address2", emptyField, row1[count++]);
			AssertEquals("31. Intermediary Bank Address3", emptyField, row1[count++]);
			AssertEquals("32. Intermediary Bank Country", emptyField, row1[count++]);
			AssertEquals("33. Intermediary Bank BIC", emptyField, row1[count++]);
			AssertEquals("34. Charge Account No.", emptyField, row1[count++]);
			AssertEquals("35. Message to Beneficiary", emptyField, row1[count++]);
			AssertEquals("36. Message to Bank", emptyField, row1[count++]);
			AssertEquals("37. Reference ", "\"284908\"", row1[count++]);
			AssertEquals("38. Charge Code", emptyField, row1[count++]);
			AssertEquals("39. Contract No", emptyField, row1[count++]);
			AssertEquals("40. Delivery Mode", emptyField, row1[count++]);
			AssertEquals("41. Special Flag", emptyField, row1[count++]);
		}

		static void AssertDetailsForAdviceRecord(string row2AsString)
		{
			string[] row2 = row2AsString.Split(',');

			AssertEquals("row2 length", 5, row2.Length);
			AssertEquals("Serial No.", "\"\"", row2[0]);
			AssertEquals("Payment Method", "\"GI\"", row2[1]);
			AssertEquals("Transaction Code", "\"MIS\"", row2[2]);
			AssertEquals("Record Type", "\"A\"", row2[3]);
			AssertEquals("Transaction Advice", "\"20091120/PI/CL/AP INVOICE/AUD/4000.00\"", row2[4]);
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new YusenDDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string contains = "Edward";
			string directPaymentText = directPaymentAmount.ToString("N2");
			string paymentText = paymentAmount.ToString("N2");
			string currency = directPaymentAmount == 550.00m ? Core.Constants.CurrencyCodes.UnitedStates : Core.Constants.CurrencyCodes.Australia;
			string directPaymentLine = "\"\",\"GI\",\"MIS\",\"T\",\"\",\"48009802\",\"" + currency + "\",\"" + directPaymentText + "\",\"Edward\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"995839589\",\"\",\"3859\",\"\",\"25\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"33\",\"\",\"\",\"\",\"\"";
			string paymentLine = "\"\",\"GI\",\"MIS\",\"T\",\"\",\"48009802\",\"" + currency + "\",\"" + paymentText + "\",\"George\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"385029571\",\"\",\"3483\",\"\",\"90\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"582305\",\"\",\"\",\"\",\"\"";
			string line1 = records[0].Contains(contains) ? directPaymentLine : paymentLine;
			string line2 = records[1].Contains(contains) ? directPaymentLine : paymentLine;

			AssertEquals("Line 1", line1, records[0]);
			AssertEquals("Line 2", line2, records[1]);
		}

		#endregion
	}
}
