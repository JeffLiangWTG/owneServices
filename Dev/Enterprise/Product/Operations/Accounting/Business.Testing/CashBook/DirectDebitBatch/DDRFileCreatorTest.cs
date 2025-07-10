using System;
using System.IO;
using System.Text.RegularExpressions;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class DDRFileCreatorTest : DDRFileCreatorBaseTest
	{
		public void TestWriteDescriptiveHeaderRecord()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			string expectedCompanyName = testBank.AB_BankAccountName.PadRight(26);

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);
			string entireHeaderRecordString = writer.ToString();

			AssertEquals("String Length", 122, entireHeaderRecordString.Length);

			AssertEquals("Record Type", "0", entireHeaderRecordString.Substring(0, 1));
			AssertEquals("Reserved Field", "".PadRight(17), entireHeaderRecordString.Substring(1, 17));
			AssertEquals("Sequence Number", "01", entireHeaderRecordString.Substring(18, 2));
			AssertEquals("Name Of Financial Institution", bankCode, entireHeaderRecordString.Substring(20, 3));
			AssertEquals("Reserved Field", "".PadRight(7), entireHeaderRecordString.Substring(23, 7));
			AssertEquals("CurrentCompanyName", expectedCompanyName, entireHeaderRecordString.Substring(30, 26));
			AssertEquals("User ID Number", userID.PadLeft(6, '0'), entireHeaderRecordString.Substring(56, 6));
			AssertEquals("Description of Entries on file", "PAYMENTS".PadRight(12), entireHeaderRecordString.Substring(62, 12));
			AssertEquals("Today's Date", Env.Time.CurrentLocalDate.ToString("ddMMyy"), entireHeaderRecordString.Substring(74, 6));
			AssertEquals("Reserved Space", "".PadRight(40), entireHeaderRecordString.Substring(80, 40));
			AssertEquals("NewLine", System.Environment.NewLine, entireHeaderRecordString.Substring(120));
		}

		public void TestWriteDetailRecordsForPayment()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string transactionCode = "50";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = testBank.AB_BankAccountName.Substring(0, 16);

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testOrg.OH_Code = "TEURYH";
			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPayment.AH_ExchangeRate = 2m;
			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;

			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;
			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			AssertEquals(1, header.Lines.Count);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertEquals("String Length", 122, detailRecord.Length);

			AssertEquals("Record Type", "1", detailRecord.Substring(0, 1));
			AssertEquals("Payee BSB", aPBankBSB, detailRecord.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", aPBankAccountNum, detailRecord.Substring(8, 9));
			AssertEquals("Indicator", " ", detailRecord.Substring(17, 1));
			AssertEquals("Transaction Code", transactionCode, detailRecord.Substring(18, 2));
			AssertEquals("Amount", "0000025000", detailRecord.Substring(20, 10));
			AssertEquals("Title Of Account to be Credited", accountTitle.PadRight(32), detailRecord.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), detailRecord.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, detailRecord.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, detailRecord.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, detailRecord.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", detailRecord.Substring(112, 8));
			AssertEquals("NewLine", System.Environment.NewLine, detailRecord.Substring(120, 2));
		}

		public void TestWriteDetailRecordsForDDRPaymentLine()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string transactionCode = "50";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = testBank.AB_BankAccountName.Substring(0, 16);

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testOrg.OH_Code = "TEURYH";
			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPayment.AH_ExchangeRate = 2m;
			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;
			aPPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebitLine;

			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;
			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;
			header.Lines.Add(aPPayment);

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			AssertEquals(1, header.Lines.Count);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertEquals("String Length", 122, detailRecord.Length);

			AssertEquals("Record Type", "1", detailRecord.Substring(0, 1));
			AssertEquals("Payee BSB", aPBankBSB, detailRecord.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", aPBankAccountNum, detailRecord.Substring(8, 9));
			AssertEquals("Indicator", " ", detailRecord.Substring(17, 1));
			AssertEquals("Transaction Code", transactionCode, detailRecord.Substring(18, 2));
			AssertEquals("Amount", "0000025000", detailRecord.Substring(20, 10));
			AssertEquals("Title Of Account to be Credited", accountTitle.PadRight(32), detailRecord.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), detailRecord.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, detailRecord.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, detailRecord.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, detailRecord.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", detailRecord.Substring(112, 8));
			AssertEquals("NewLine", System.Environment.NewLine, detailRecord.Substring(120, 2));
		}

		public void TestWriteDetailRecordsFor2Payments()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			OrgHeader testOrg1 = Factory.New(typeof(OrgHeader)) as OrgHeader;
			OrgHeader testOrg2 = Factory.New(typeof(OrgHeader)) as OrgHeader;

			APPayment aPPayment1 = Factory.New<APPayment>();
			aPPayment1.AH_AB = testBank.PK;
			aPPayment1.AH_OH = testOrg1.PK;
			aPPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;

			APPayment aPPayment2 = Factory.New<APPayment>();
			aPPayment2.AH_AB = testBank.PK;
			aPPayment2.AH_OH = testOrg2.PK;
			aPPayment2.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string aPBankBSB1 = "1234-56";
			string aPBankAccountNum1 = "123456789";
			string aPBankBSB2 = "6543-21";
			string aPBankAccountNum2 = "987654321";
			string transactionCode = "50";
			string accountTitle1 = "Account Title 1";
			string accountTitle2 = "Account Title 2";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = testBank.AB_BankAccountName.Substring(0, 16);

			AccAPAccountDetails accountDetails1 = testOrg1.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails1.A1_BankBsb = aPBankBSB1;
			accountDetails1.A1_BankAccount = aPBankAccountNum1;
			accountDetails1.A1_AccountName = accountTitle1;
			accountDetails1.A1_IsDefaultAccount = true;
			accountDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			AccAPAccountDetails accountDetails2 = testOrg2.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails2.A1_BankBsb = aPBankBSB2;
			accountDetails2.A1_BankAccount = aPBankAccountNum2;
			accountDetails2.A1_AccountName = accountTitle2;
			accountDetails2.A1_IsDefaultAccount = true;
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			accountDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			testOrg1.OH_Code = "TEURYH";
			aPPayment1.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPayment1.AH_ExchangeRate = 2m;
			aPPayment1.AH_OSExTaxAmount = 500m;
			aPPayment1.AH_ChequeOrReference = lodgementRef;

			testOrg2.OH_Code = "blahgl";
			aPPayment2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPayment2.AH_ExchangeRate = 2m;
			aPPayment2.AH_OSExTaxAmount = 500m;
			aPPayment2.AH_ChequeOrReference = lodgementRef;

			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			AssertEquals(2, header.Lines.Count);
			fileCreator.Create();
			string file = writer.ToString();
			string[] fileLines = file.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
			string detailRecord1 = fileLines[1];
			string detailRecord2 = fileLines[2];

			if (detailRecord1.Substring(1, 7) != aPBankBSB1)
			{
				detailRecord2 = fileLines[1];
				detailRecord1 = fileLines[2];
			}

			Assert("Two bsb numbers should be different", detailRecord1.Substring(1, 7) != detailRecord2.Substring(1, 7));
			Assert("Two account numbers should be different", detailRecord1.Substring(8, 9) != detailRecord2.Substring(8, 9));

			AssertEquals("String Length", 120, detailRecord1.Length);

			AssertEquals("Record Type", "1", detailRecord1.Substring(0, 1));
			AssertEquals("Payee BSB", aPBankBSB1, detailRecord1.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", aPBankAccountNum1, detailRecord1.Substring(8, 9));
			AssertEquals("Indicator", " ", detailRecord1.Substring(17, 1));
			AssertEquals("Transaction Code", transactionCode, detailRecord1.Substring(18, 2));
			AssertEquals("Amount", "0000025000", detailRecord1.Substring(20, 10));
			AssertEquals("Title Of Account to be Credited", accountTitle1.PadRight(32), detailRecord1.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), detailRecord1.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, detailRecord1.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, detailRecord1.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, detailRecord1.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", detailRecord1.Substring(112, 8));

			AssertEquals("String Length", 120, detailRecord2.Length);

			AssertEquals("Record Type", "1", detailRecord2.Substring(0, 1));
			AssertEquals("Payee BSB", aPBankBSB2, detailRecord2.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", aPBankAccountNum2, detailRecord2.Substring(8, 9));
			AssertEquals("Indicator", " ", detailRecord2.Substring(17, 1));
			AssertEquals("Transaction Code", transactionCode, detailRecord2.Substring(18, 2));
			AssertEquals("Amount", "0000025000", detailRecord2.Substring(20, 10));
			AssertEquals("Title Of Account to be Credited", accountTitle2.PadRight(32), detailRecord2.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), detailRecord2.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, detailRecord2.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, detailRecord2.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, detailRecord2.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", detailRecord2.Substring(112, 8));
		}

		public void TestWriteDetailRecordsForDirectPayment()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string transactionCode = "50";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = testBank.AB_BankAccountName.Substring(0, 16);

			directPayment.AH_DrawerBranch = aPBankBSB;
			directPayment.AH_DrawerBank = aPBankAccountNum;
			directPayment.AH_ChequeDrawer = accountTitle;
			directPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			directPayment.AH_ExchangeRate = 4m;
			directPayment.AH_OSExTaxAmount = 500m;
			directPayment.AH_OSTaxAmount = 50m;
			directPayment.AH_ChequeOrReference = lodgementRef;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_OSExTaxAmount = 500m;
			directPayment.Lines[0].AL_GSTVAT = -12.5m;
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			AssertEquals(1, header.Lines.Count);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertEquals("String Length", 122, detailRecord.Length);

			AssertEquals("Record Type", "1", detailRecord.Substring(0, 1));
			AssertEquals("Payee BSB", aPBankBSB, detailRecord.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", aPBankAccountNum, detailRecord.Substring(8, 9));
			AssertEquals("Indicator", " ", detailRecord.Substring(17, 1));
			AssertEquals("Transaction Code", transactionCode, detailRecord.Substring(18, 2));
			AssertEquals("Amount", "0000013750", detailRecord.Substring(20, 10));
			AssertEquals("Title Of Account to be Credited", accountTitle.PadRight(32), detailRecord.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), detailRecord.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, detailRecord.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, detailRecord.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, detailRecord.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", detailRecord.Substring(112, 8));
			AssertEquals("NewLine", System.Environment.NewLine, detailRecord.Substring(120, 2));
		}

		public void TestWriteFileTotalRecord()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";

			directPayment.AH_DrawerBranch = aPBankBSB;
			directPayment.AH_DrawerBank = aPBankAccountNum;
			directPayment.AH_ChequeDrawer = accountTitle;

			directPayment.AH_OSExTaxAmount = 500m;
			directPayment.AH_ChequeOrReference = lodgementRef;
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);

			fileCreator.WriteFileTotalRecord_ForTestOnly(1000.00m, "2", "");

			string fileTotalRecord = writer.ToString();

			AssertEquals("String Length", 122, fileTotalRecord.Length);

			AssertEquals("Record Type", "7", fileTotalRecord.Substring(0, 1));
			AssertEquals("Reserved Field", "999-999", fileTotalRecord.Substring(1, 7));
			AssertEquals("Reserved Field", "".PadRight(12), fileTotalRecord.Substring(8, 12));
			AssertEquals("File Net Total Amount", "0000100000", fileTotalRecord.Substring(20, 10));
			AssertEquals("File Credit Total Amount", "0000100000", fileTotalRecord.Substring(30, 10));
			AssertEquals("Reserved Field", "0000000000", fileTotalRecord.Substring(40, 10));
			AssertEquals("Reserved Field", "".PadRight(24), fileTotalRecord.Substring(50, 24));
			AssertEquals("File Total Count", "000002", fileTotalRecord.Substring(74, 6));
			AssertEquals("Reserved Field", "".PadRight(40), fileTotalRecord.Substring(80, 40));
		}

		public void TestGetFormattedBankAccountNumber()
		{
			string accountNumber = "123456789";
			StringWriter writer = new StringWriter();
			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			string actual = fileCreator.GetFormattedBankAccountNumber_ForTestOnly(accountNumber);
			AssertEquals("Bank Account Number", accountNumber, actual);

			accountNumber = "123-456-7";
			actual = fileCreator.GetFormattedBankAccountNumber_ForTestOnly(accountNumber);
			AssertEquals("Number with dashes", accountNumber, actual);

			accountNumber = "123456-789";
			actual = fileCreator.GetFormattedBankAccountNumber_ForTestOnly(accountNumber);
			AssertEquals("Account Num requiring dashes to be removed", "123456789", actual);

			accountNumber = "abc12 123";
			actual = fileCreator.GetFormattedBankAccountNumber_ForTestOnly(accountNumber);
			AssertEquals("Account Num including alpha chars and spaces", accountNumber, actual);

			accountNumber = "123";
			actual = fileCreator.GetFormattedBankAccountNumber_ForTestOnly(accountNumber);
			AssertEquals("Account Num requiring left padding", accountNumber.PadLeft(9), actual);
		}

		public void TestFormatAmount()
		{
			string amountString = "500";
			StringWriter writer = new StringWriter();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);

			string actual = fileCreator.GetFormattedAmountString_ForTestOnly(amountString);
			AssertEquals("Amount String", "0000050000", actual);

			amountString = "120.00";
			actual = fileCreator.GetFormattedAmountString_ForTestOnly(amountString);
			AssertEquals("Amount String", "0000012000", actual);
		}

		public void TestFitToWidthCore()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			StringWriter writer = new StringWriter();
			DDRFileGenerator generator = new DDRFileGenerator(writer, dDRHeader);
			AssertEquals("Should pad right with '4'", "bug4444444", generator.FitToWidthCore_ForTestOnly("bug", 10, '4'));
		}

		public void TestFormatAmountCore()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			StringWriter writer = new StringWriter();
			DDRFileGenerator generator = new DDRFileGenerator(writer, dDRHeader);
			AssertEquals("Should be 6 numbers to left of decimal place", "00854034", generator.GetFormattedAmountStringCore_ForTestOnly("8540.34", 6, '0'));
			AssertEquals("Should be 8 numbers to left of decimal place", "0000854034", generator.GetFormattedAmountStringCore_ForTestOnly("8540.34", 8, '0'));
			AssertEquals("Should be 10 numbers to left of decimal place", "000000854034", generator.GetFormattedAmountStringCore_ForTestOnly("8540.34", 10, '0'));

			AssertEquals("Should be padded with space", "  854034", generator.GetFormattedAmountStringCore_ForTestOnly("8540.34", 6, ' '));
		}

		public void TestFitToWidthPadLeft()
		{
			StringWriter writer = new StringWriter();
			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);
			AssertEquals("Should pad left with spaces", "   pop", fileCreator.FitToWidthPadLeft_ForTestOnly("pop", 6));
			AssertEquals("Should return the final 6 chars", "adLeft", fileCreator.FitToWidthPadLeft_ForTestOnly("PadLeft", 6));
		}

		public void TestFileCreation()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			testOrg.OH_Code = "PDJERS";
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";

			directPayment.AH_DrawerBranch = aPBankBSB;
			directPayment.AH_DrawerBank = aPBankAccountNum;
			directPayment.AH_ChequeDrawer = accountTitle;

			directPayment.AH_OSExTaxAmount = 500m;
			directPayment.AH_ChequeOrReference = lodgementRef;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_OSExTaxAmount = 500m;
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_IsDefaultAccount = true;

			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			Factory.Save();

			StringWriter writer = new StringWriter();

			DDRFileGenerator fileCreator = new DDRFileGenerator(writer, header);

			fileCreator.Create();
			string[] fileRecords = Regex.Split(writer.ToString(), System.Environment.NewLine);

			AssertEquals("4 rows should have been created", 4, fileRecords.Length - 1);

			Assert("First Row should start with an 0", fileRecords[0].StartsWith("0"));
			Assert("Second Row should start with a 1", fileRecords[1].StartsWith("1"));
			Assert("Third Row should also start with a 1", fileRecords[2].StartsWith("1"));
			Assert("Fourth Row should start with a 7", fileRecords[3].StartsWith("7"));

			AssertEquals("File Net Total Amount Calculation", "0000100000", fileRecords[3].Substring(20, 10));
			AssertEquals("File Credit Total Amount Calculation", "0000100000", fileRecords[3].Substring(30, 10));
			AssertEquals("File Total Record Count", "000002", fileRecords[3].Substring(74, 6));
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new DDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "1385925 995839589 5000000";
			string directPaymentText = ((int)(directPaymentAmount * 100)).ToString("D");
			string paymentText = ((int)(paymentAmount * 100)).ToString("D");
			string directPaymentLine = "1385925 995839589 5000000" + directPaymentText + "Edward                          33                568090 480098029Eagle Datamation00000000";
			string paymentLine = "1348390 385029571 5000000" + paymentText + "George                          582305            568090 480098029Eagle Datamation00000000";
			string line2 = records[1].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string line3 = records[2].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string totalAmount = ((int)((directPaymentAmount + paymentAmount) * 100)).ToString("D");

			AssertEquals("Line 1", "0                 01", records[0].Substring(0, 20));
			AssertEquals("Line 1", "       Eagle Datamation Internati00", records[0].Substring(23, 35));
			AssertEquals("Line 1", "PAYMENTS    230811                                        ", records[0].Substring(62));
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "7999-999            00000" + totalAmount + "00000" + totalAmount + "0000000000                        000002                                        ", records[3]);
		}

		#endregion
	}
}
