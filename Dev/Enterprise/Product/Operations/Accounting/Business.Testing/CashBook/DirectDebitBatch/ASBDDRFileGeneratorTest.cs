using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class ASBDDRFileGeneratorTest : DDRFileCreatorBaseTest
	{
		#region TestWriteDescriptiveHeaderRecordForASB

		public void TestWriteDescriptiveHeaderRecordForASB()
		{
			string bankNum = "35";
			string branchNum = "2356";
			string uniqueNum = "3567734";
			string suffix = "67";

			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = uniqueNum + suffix;
			testBank.AB_BSB = bankNum + branchNum;

			StringWriter writer = new StringWriter();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			string expectedCompanyName = "Goldstein Bagels    ";
			testBank.AB_BankAccountName = expectedCompanyName;

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);

			string entireHeaderRecordString = writer.ToString();
			string dueDatePadRight = ZDateTime.Today.ToString("ddMMyyyy").PadRight(13);
			string headerFillerEnd = DDRFileGenerator.RESERVED_ForTestOnly.PadRight(109);

			AssertEquals("File Type", "12", entireHeaderRecordString.Substring(0, 2));
			AssertEquals("Bank Number", bankNum, entireHeaderRecordString.Substring(2, 2));
			AssertEquals("Branch Number", branchNum, entireHeaderRecordString.Substring(4, 4));
			AssertEquals("Unique Number", uniqueNum, entireHeaderRecordString.Substring(8, 7));
			AssertEquals("Suffix", suffix + ASBDDRFileGenerator.Padding, entireHeaderRecordString.Substring(15, 3));
			AssertEquals("Due Date", dueDatePadRight, entireHeaderRecordString.Substring(18, 13));
			AssertEquals("Client Short Name", expectedCompanyName, entireHeaderRecordString.Substring(31, 20));
			AssertEquals("Filler", headerFillerEnd, entireHeaderRecordString.Substring(51, 109));
			AssertEquals("NewLine", System.Environment.NewLine, entireHeaderRecordString.Substring(160, 2));
			AssertEquals("Header Record should be 162 characters long (includes newline & carriage return)", 162, entireHeaderRecordString.Length);
		}

		#endregion

		#region TestWriteTransactionRecord

		public void TestWriteTransactionRecordForPayment()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			string payerBSB = "324525";
			string payerAccountNumber = "543255267";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = payerAccountNumber;
			testBank.AB_BSB = payerBSB;

			string payeeBankNumber = "34";
			string payeeBranchNumber = "3089";
			string payeeUniqueNumber = "3286876";
			string payeeSuffix = "66";
			string amountInCents = "0000049249";
			string payeeName = "Joe Bloggs";
			string payeeNamePadRight = payeeName.PadRight(20);

			string internalReference = ASBDDRFileGenerator.DetailInternalReference.PadRight(12);    // hard coded
			string payeeCode = "G Bagels";
			string payeeCodePadRight = payeeCode.PadRight(12);
			string payeeReference = "35819";    // AH_ChequeOrReference
			string payeeReferencePadRight = payeeReference.PadRight(12);
			string payeeParticulars = ASBDDRFileGenerator.DetailPayeeParticulars.PadRight(12);  // hard coded

			string payerName = "NameLongerThan20Characters";
			string payerNamePadRight = "NameLongerThan20Char";
			string payerCode = GlbCompany.CurrentCompany.GC_Code;
			string payerCodePadRight = payerCode.PadRight(12);
			string payerReference = ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(12);              // PayerReference is left empty
			string payerParticulars = "DDR " + payeeReference;
			string payerParticularsPadRight = payerParticulars.PadRight(12);
			string detailFillerEnd = ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(4);

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = payeeBankNumber + payeeBranchNumber;
			accountDetails.A1_BankAccount = payeeUniqueNumber + payeeSuffix;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = payeeName;

			aPPayment.AH_OSExTaxAmount = 492.49m;
			testOrg.OH_FullName = payeeCode;
			aPPayment.AH_ChequeOrReference = payeeReference;

			StringWriter writer = new StringWriter();

			testBank.AB_BankAccountName = payerName;
			GlbCompany.CurrentCompany.GC_Code = payerCode;

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(aPPayment, "1");

			string detailRecordString = writer.ToString();

			AssertEquals("Record Type", ASBDDRFileGenerator.BeginNewRecordFlag, detailRecordString.Substring(0, 2));
			AssertEquals("Bank Number", payeeBankNumber, detailRecordString.Substring(2, 2));
			AssertEquals("Branch Number", payeeBranchNumber, detailRecordString.Substring(4, 4));
			AssertEquals("Unique Number", payeeUniqueNumber, detailRecordString.Substring(8, 7));
			AssertEquals("Suffix", payeeSuffix + ASBDDRFileGenerator.Padding, detailRecordString.Substring(15, 3));
			AssertEquals("Transaction Code", ASBDDRFileGenerator.CreditTransactionCode, detailRecordString.Substring(18, 3));
			AssertEquals("Amount", amountInCents, detailRecordString.Substring(21, 10));
			AssertEquals("Payee Name", payeeNamePadRight, detailRecordString.Substring(31, 20));
			AssertEquals("Internal Reference", internalReference, detailRecordString.Substring(51, 12));
			AssertEquals("Payee Code", payeeCodePadRight, detailRecordString.Substring(63, 12));
			AssertEquals("Payee Reference", payeeReferencePadRight, detailRecordString.Substring(75, 12));
			AssertEquals("Payee Particulars", payeeParticulars, detailRecordString.Substring(87, 12));
			AssertEquals("Filler", ASBDDRFileGenerator.Padding, detailRecordString.Substring(99, 1));
			AssertEquals("Payer Name", payerNamePadRight, detailRecordString.Substring(100, 20));
			AssertEquals("Payer Code", payerCodePadRight, detailRecordString.Substring(120, 12));
			AssertEquals("Payer Reference", payerReference, detailRecordString.Substring(132, 12));
			AssertEquals("Payer Particulars", payerParticularsPadRight, detailRecordString.Substring(144, 12));
			AssertEquals("Filler", detailFillerEnd, detailRecordString.Substring(156, 4));
			AssertEquals("NewLine", System.Environment.NewLine, detailRecordString.Substring(160, 2));
			AssertEquals("Total length of detail string should be 162", 162, detailRecordString.Length);
		}

		public void TestWriteTransactionRecordForDirectPayment()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			string payerBSB = "324525";
			string payerAccountNumber = "543255267";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = payerAccountNumber;
			testBank.AB_BSB = payerBSB;

			string payeeBankNumber = "34";
			string payeeBranchNumber = "3089";
			string payeeUniqueNumber = "3286876";
			string payeeSuffix = "66";
			string amountInCents = "0000049249";
			string payeeName = "Joe Bloggs";
			string payeeNamePadRight = payeeName.PadRight(20);

			string internalReference = ASBDDRFileGenerator.DetailInternalReference.PadRight(12);    // hard coded
			string payeeCodePadRight = payeeName.PadRight(12);
			string payeeReference = "35819";    // AH_ChequeOrReference
			string payeeReferencePadRight = payeeReference.PadRight(12);
			string payeeParticulars = ASBDDRFileGenerator.DetailPayeeParticulars.PadRight(12);  // hard coded

			string payerName = "NameLongerThan20Characters";
			string payerNamePadRight = "NameLongerThan20Char";
			string payerCode = GlbCompany.CurrentCompany.GC_Code;
			string payerCodePadRight = payerCode.PadRight(12);
			string payerReference = ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(12);              // PayerReference is left empty
			string payerParticulars = "DDR " + payeeReference;
			string payerParticularsPadRight = payerParticulars.PadRight(12);
			string detailFillerEnd = ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(4);

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			directPayment.AH_DrawerBranch = payeeBankNumber + payeeBranchNumber;
			directPayment.AH_DrawerBank = payeeUniqueNumber + payeeSuffix;
			directPayment.AH_OSExTaxAmount = 492.49m;
			directPayment.AH_ChequeDrawer = payeeName;
			directPayment.AH_ChequeOrReference = payeeReference;

			StringWriter writer = new StringWriter();

			testBank.AB_BankAccountName = payerName;
			GlbCompany.CurrentCompany.GC_Code = payerCode;

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(directPayment, "1");

			string detailRecordString = writer.ToString();

			AssertEquals("Record Type", ASBDDRFileGenerator.BeginNewRecordFlag, detailRecordString.Substring(0, 2));
			AssertEquals("Bank Number", payeeBankNumber, detailRecordString.Substring(2, 2));
			AssertEquals("Branch Number", payeeBranchNumber, detailRecordString.Substring(4, 4));
			AssertEquals("Unique Number", payeeUniqueNumber, detailRecordString.Substring(8, 7));
			AssertEquals("Suffix", payeeSuffix + ASBDDRFileGenerator.Padding, detailRecordString.Substring(15, 3));
			AssertEquals("Transaction Code", ASBDDRFileGenerator.CreditTransactionCode, detailRecordString.Substring(18, 3));
			AssertEquals("Amount", amountInCents, detailRecordString.Substring(21, 10));
			AssertEquals("Payee Name", payeeNamePadRight, detailRecordString.Substring(31, 20));
			AssertEquals("Internal Reference", internalReference, detailRecordString.Substring(51, 12));
			AssertEquals("Payee Code", payeeCodePadRight, detailRecordString.Substring(63, 12));
			AssertEquals("Payee Reference", payeeReferencePadRight, detailRecordString.Substring(75, 12));
			AssertEquals("Payee Particulars", payeeParticulars, detailRecordString.Substring(87, 12));
			AssertEquals("Filler", ASBDDRFileGenerator.Padding, detailRecordString.Substring(99, 1));
			AssertEquals("Payer Name", payerNamePadRight, detailRecordString.Substring(100, 20));
			AssertEquals("Payer Code", payerCodePadRight, detailRecordString.Substring(120, 12));
			AssertEquals("Payer Reference", payerReference, detailRecordString.Substring(132, 12));
			AssertEquals("Payer Particulars", payerParticularsPadRight, detailRecordString.Substring(144, 12));
			AssertEquals("Filler", detailFillerEnd, detailRecordString.Substring(156, 4));
			AssertEquals("NewLine", System.Environment.NewLine, detailRecordString.Substring(160, 2));
			AssertEquals("Total length of detail string should be 162", 162, detailRecordString.Length);
		}

		#endregion

		#region TestWriteTransactionRecord_AccountNum10Chars

		public void TestWriteDescriptiveHeaderRecordForASB_AccountNum10CharsForPayment()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string payerBSB = "324324";
			string payerAccountNumber = "3435556666";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = payerAccountNumber;
			testBank.AB_BSB = payerBSB;

			string payeeBankNumber = "34";
			string payeeBranchNumber = "3089";
			string payeeUniqueNumber = "3286876";
			string payeeSuffix = "665";
			string payeeName = "Joe Citizen";

			string payeeCode = "Cheesecake";
			string payeeReference = "53253555";

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = payeeBankNumber + payeeBranchNumber;
			accountDetails.A1_BankAccount = payeeUniqueNumber + payeeSuffix;
			accountDetails.A1_AccountName = payeeName;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			aPPayment.AH_OSExTaxAmount = 492.49m;

			testOrg.OH_FullName = payeeCode;
			aPPayment.AH_ChequeOrReference = payeeReference;

			StringWriter writer = new StringWriter();

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(aPPayment, "1");

			string entireHeaderRecordString = writer.ToString();
			AssertEquals("Suffix", payeeSuffix, entireHeaderRecordString.Substring(15, 3));

			accountDetails.A1_BankAccount = payeeUniqueNumber + payeeSuffix + "4";
			writer = new StringWriter();

			try
			{
				fileCreator.WriteDetailRecord_ForTestOnly(aPPayment, "1");
				AssertEquals("DDR File error message", "Accounting: ASB DDR File Creation: Account number was 11 digits long; must be 9 or 10", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestWriteDescriptiveHeaderRecordForASB_AccountNum10CharsForDirectPayment()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string payerBSB = "324324";
			string payerAccountNumber = "3435556666";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = payerAccountNumber;
			testBank.AB_BSB = payerBSB;

			string payeeBankNumber = "34";
			string payeeBranchNumber = "3089";
			string payeeUniqueNumber = "3286876";
			string payeeSuffix = "665";
			string payeeName = "Joe Citizen";

			string payeeReference = "53253555";

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			directPayment.AH_DrawerBranch = payeeBankNumber + payeeBranchNumber;
			directPayment.AH_DrawerBank = payeeUniqueNumber + payeeSuffix;
			directPayment.AH_OSExTaxAmount = 492.49m;
			directPayment.AH_ChequeDrawer = payeeName;
			directPayment.AH_ChequeOrReference = payeeReference;

			StringWriter writer = new StringWriter();

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(directPayment, "1");

			string entireHeaderRecordString = writer.ToString();
			AssertEquals("Suffix", payeeSuffix, entireHeaderRecordString.Substring(15, 3));

			directPayment.AH_DrawerBank = payeeUniqueNumber + payeeSuffix + "4";
			writer = new StringWriter();

			try
			{
				fileCreator.WriteDetailRecord_ForTestOnly(directPayment, "1");
				AssertEquals("DDR File error message", "Accounting: ASB DDR File Creation: Account number was 11 digits long; must be 9 or 10", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWriteTrailerRecord

		public void TestWriteTrailerRecord()
		{
			string detailRecordCount = "5";
			string hashTotal = "395813";
			string hashTotalFormatted = hashTotal.PadLeft(11);
			ZDecimal totalAmount = 395.53m;
			string totalAmountFormatted = "39553".PadLeft(10, '0');

			StringWriter writer = new StringWriter();
			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.WriteFileTotalRecord_ForTestOnly(totalAmount, detailRecordCount, hashTotal);

			string trailerRecord = writer.ToString();
			AssertEquals("Record Type", ASBDDRFileGenerator.BeginNewRecordFlag, trailerRecord.Substring(0, 2));
			AssertEquals("Key Field", ASBDDRFileGenerator.TrailerRecordFlag, trailerRecord.Substring(2, 2));
			AssertEquals("Hash Total", hashTotalFormatted, trailerRecord.Substring(4, 11));
			AssertEquals("Filler", ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(6), trailerRecord.Substring(15, 6));
			AssertEquals("Total Amount", totalAmountFormatted, trailerRecord.Substring(21, 10));
			AssertEquals("Filler", ASBDDRFileGenerator.RESERVED_ForTestOnly.PadRight(129), trailerRecord.Substring(31, 129));
			AssertEquals("NewLine", System.Environment.NewLine, trailerRecord.Substring(160, 2));
			AssertEquals("Length of Trailer Record should be 162", 162, trailerRecord.Length);
		}

		#endregion

		#region TestFileCreation

		public void TestFileCreation()
		{
			string bankBSB = "568090";
			string bankAccountNum = "480098029";
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bankBSB;

			string payeeBSB_Detail1 = "348390";
			string payeeAccountNum_Detail1 = "385029571";
			string accountTitle_Detail1 = "George";
			string payeeCode_Detail1 = "GFF";
			string lodgementReference_Detail1 = "582305";

			string payeeBSB_Detail2 = "385925";
			string payeeAccountNum_Detail2 = "995839589";
			string accountTitle_Detail2 = "Edward";
			string lodgementReference_Detail2 = "33";

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			testOrg.OH_Code = "RGY$*E";

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = payeeBSB_Detail1;
			accountDetails.A1_BankAccount = payeeAccountNum_Detail1;
			accountDetails.A1_AccountName = accountTitle_Detail1;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_IsDefaultAccount = true;

			aPPayment.AH_OSExTaxAmount = 239.42m;
			testOrg.OH_FullName = payeeCode_Detail1;
			aPPayment.AH_ChequeOrReference = lodgementReference_Detail1;

			DirectPayment.DirectPayment testDirectPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			testDirectPayment.AH_AB = testBank.PK;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			testDirectPayment.AH_DrawerBranch = payeeBSB_Detail2;
			testDirectPayment.AH_DrawerBank = payeeAccountNum_Detail2;
			testDirectPayment.AH_ChequeDrawer = accountTitle_Detail2;
			testDirectPayment.AH_ChequeOrReference = lodgementReference_Detail2;
			testDirectPayment.AH_OH = testOrg.PK;

			testDirectPayment.Lines.AddNew();
			DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)testDirectPayment.Lines[0];
			testLine.AL_OSExTaxAmount = 5859.90m;

			Factory.Save();

			StringWriter writer = new StringWriter();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(writer, header);
			fileCreator.Create();

			string[] fileRecords = Regex.Split(writer.ToString(), System.Environment.NewLine);

			AssertEquals("4 rows should have been created", 4, fileRecords.Length - 1);

			Assert("First Row should start with 12", fileRecords[0].StartsWith("12"));
			Assert("Second Row should start with 13", fileRecords[1].StartsWith("13"));
			Assert("Third Row should also start with 13", fileRecords[2].StartsWith("13"));
			Assert("Fourth Row should also start with 13", fileRecords[3].StartsWith("13"));

			AssertEquals("File Net Total Amount Calculation", "0000609932", fileRecords[3].Substring(21, 10));
			//"   13823005";
			AssertEquals("Hash Total Amount Calculation", ASBDDRFileGenerator.DefaultHashTotal, fileRecords[3].Substring(4, 11));
		}

		#endregion

		#region TestGetFileHashTotal

		public void TestGetFileHashTotal()
		{
			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			ASBDDRFileGenerator fileCreator = new ASBDDRFileGenerator(new StringWriter(), header);
			AssertEquals("The hashing total should be the default hash total", ASBDDRFileGenerator.DefaultHashTotal, fileCreator.GetFileHashingTotal_ForTestOnly(header));
		}

		#endregion

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new ASBDDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "13385925995839589";
			string directPaymentText = ((int)(directPaymentAmount * 100)).ToString("D");
			string paymentText = ((int)(paymentAmount * 100)).ToString("D");
			string directPaymentLine = "13385925995839589 05100000" + directPaymentText + "Edward              REMITTANCE  Edward      33          REFER        Eagle Datamation IntEDI                     DDR 33          ";
			string paymentLine = "13348390385029571 05100000" + paymentText + "George              REMITTANCE  GFF         582305      REFER        Eagle Datamation IntEDI                     DDR 582305      ";
			string line2 = records[1].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string line3 = records[2].StartsWith(prefix) ? directPaymentLine : paymentLine;

			AssertEquals("Line 1", "12568090480098029 23082011     Eagle Datamation Int                                                                                                             ", records[0]);
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "139900000000000      00000" + ((int)((directPaymentAmount + paymentAmount) * 100)).ToString("D") + "                                                                                                                                 ", records[3]);
		}

		#endregion
	}
}
