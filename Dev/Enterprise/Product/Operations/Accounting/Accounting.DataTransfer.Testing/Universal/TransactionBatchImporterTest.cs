using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public class TransactionBatchImporterTest : TestCaseWithUniversalObjectFactory
	{
		public void TestNoLinkedTransactionIDCollection()
		{
			SetUpTransactionInfo(TransactionInfo);
			var linkedTransactionIDCollection = TransactionBatch.TransactionCollection[0].MatchLineCollection[0].LinkedTransactionIDCollection;
			linkedTransactionIDCollection.Remove(LinkedTransactionID);
			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert("It should have error, because there is no <LinkedTransactionID>.", Logger.GetErrors().Contains("Match line should have one <LinkedTransactionID> for each <LinkedTransactionIDCollection>."));

			Logger.ClearLogs();
			TransactionBatch.TransactionCollection[0].MatchLineCollection[0].LinkedTransactionIDCollection = null;
			result = importer.ImportTransactionBatch();
			Assert("The result of importing is false", !result);
			Assert("It should have error, because there is no <LinkedTransactionIDCollection>.", Logger.GetErrors().Contains("Match line should have one <LinkedTransactionID> for each <LinkedTransactionIDCollection>."));
		}

		public void TestKeyOfLinkedTransactionIDShouldHaveValue()
		{
			SetUpTransactionInfo(TransactionInfo);
			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert("It should have error, because Key of <LinkedTransactionID> doesn't have value.", Logger.GetErrors().Contains("The <Key> of <LinkedTransactionID> should have value."));
		}

		public void TestKeyOfLinkedTransactionIDShouldBeValidFormat()
		{
			SetUpTransactionInfo(TransactionInfo);
			LinkedTransactionID.Key = "xx c";
			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert("It should have error, because Key of <LinkedTransactionID> doesn't have value.", Logger.GetErrors().Contains("The <Key> of <LinkedTransactionID> should consist of ledger, transaction type and transaction number separated by one space. E.g. AP INV 12345"));
		}

		[TestDate(2021, 03, 15)]
		public void TestImportSuccessfully()
		{
			TransactionInfo.MatchLineCollection = null;
			TransactionInfo.OrganizationsTransactionID = "Test Third Party Num";
			SetUpTransactionInfo(TransactionInfo);
			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is true", result);
			AssertEquals(@"Information - Matching '':- Matched to 'BAROPT' by code, main address used.
Information - Begin processing Transaction AR REC BAROPT ZHSBCAUD CASH: 
Information -   Completed Processing Transaction.", Logger.Logs);
			AssertEquals("The count of SaveInTransactionActions should be 1 after importing successfully.", 1, Factory.BOFactory.SaveInTransactionActions.Count);
		}

		[TestDate(2021, 3, 15)]
		public void TestUniversalTransactionBatchImportWithMatch_SetDepartment()
		{
			new AccountingPeriodTestHelper(BusinessObjectFactory).SetupPeriods();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "11111", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);
			apInvoice.Factory.Save();
			AssertEquals("Precondition: AH_OutstandingAmount", -100m, apInvoice.AH_OutstandingAmount);

			var transactionInfo = CreateTransactionInfo(AccountingConstants.PaymentReceiptXUBTypes.AccountingPayment);
			SetUpTransactionInfo(transactionInfo, LedgerTypes.AccountsPayable, TransactionType.PAY, "AP PAY");
			transactionInfo.Department = new Department() { Code = TestObjectCreator.NonCurrentDepartment.GE_Code, Name = TestObjectCreator.NonCurrentDepartment.GE_Desc };
			transactionInfo.OrganizationAddress = new OrganizationAddress() { OrganizationCode = TestObjectCreator.AALSHI.OH_Code };
			MatchLine.OrganizationAddress = transactionInfo.OrganizationAddress;
			MatchLine.OSPaidAmount = -100m;
			MatchLine.MatchDate = ZDate.Today;
			var transactionBatch = new TransactionBatch();
			transactionBatch.TransactionCollection = new List<TransactionInfo>();
			transactionBatch.TransactionCollection.Add(transactionInfo);
			LinkedTransactionID.Key = "AP INV 11111";
			LinkedTransactionID.Type = "AccountingInvoice";

			var importer = new TransactionBatchImporter(transactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();
			Assert("Postcondition: The result of importing is true", result);
			Factory.SaveAtEndOfImport(Logger);

			var payment = BusinessObjectFactory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals(1, payment.Length);
			AssertEquals("Department", TestObjectCreator.NonCurrentDepartment.PK, payment[0].AH_GE);
		}

		[TestDate(2021, 3, 15)]
		public void TestWhenNoUniversalTransactionBatchImportContextWithPTRAndMatchCreateAPWithholdingJournals()
		{
			AssertUniversalTransactionBatchImportWithPTRWithMatch(true, Times.Once);
		}

		[TestDate(2021, 3, 15)]
		public void TestWhenHasUniversalTransactionBatchImportContextWithPTRAndMatchDoNotCreateAPWithholdingJournals()
		{
			AssertUniversalTransactionBatchImportWithPTRWithMatch(false, Times.Never);
		}

		public void AssertUniversalTransactionBatchImportWithPTRWithMatch(bool shouldCreateAPPBWJounal, Func<Times> times)
		{
			new AccountingPeriodTestHelper(BusinessObjectFactory).SetupPeriods();

			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "11111", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1m, 100m, 0m, 0m);
			apInvoice.Factory.Save();
			AssertEquals("Precondition: AH_OutstandingAmount", -100m, apInvoice.AH_OutstandingAmount);

			var accountingTestObjectCreator = new AccountingTestObjectCreator(BusinessObjectFactory);
			accountingTestObjectCreator.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypes.AccountsPayable, TaxSuperTypeList.StandardPaymentRetention.Code);

			var transactionInfo = CreateTransactionInfo(AccountingConstants.PaymentReceiptXUBTypes.AccountingPayment);
			SetUpTransactionInfo(transactionInfo, LedgerTypes.AccountsPayable, TransactionType.PAY, "AP PAY");
			transactionInfo.OrganizationAddress = new OrganizationAddress() { OrganizationCode = TestObjectCreator.AALSHI.OH_Code };
			MatchLine.OrganizationAddress = transactionInfo.OrganizationAddress;
			MatchLine.OSPaidAmount = -100m;
			MatchLine.MatchDate = ZDate.Today;
			var transactionBatch = new TransactionBatch();
			transactionBatch.TransactionCollection = new List<TransactionInfo>();
			transactionBatch.TransactionCollection.Add(transactionInfo);
			LinkedTransactionID.Key = "AP INV 11111";
			LinkedTransactionID.Type = "AccountingInvoice";

			var accountingDependencyFactoryMock = new Mock<IAccountingDependencyFactory>();
			var withholdingJournalCreationManagerMock = new Mock<IWithholdingJournalCreationManager>();

			ObjectFactory.Substitute(accountingDependencyFactoryMock.Object);
			accountingDependencyFactoryMock.Setup(x => x.GetWithholdingJournalCreationManager()).Returns(withholdingJournalCreationManagerMock.Object);

			withholdingJournalCreationManagerMock.Setup(x => x.ShouldAPWithholdingJournalsBeCreated(It.Is<BusinessObjectFactory>(factory => factory.HasContext(BusinessContext.UniversalTransactionBatchImport)))).Returns(shouldCreateAPPBWJounal);

			var importer = new TransactionBatchImporter(transactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();
			Assert("Postcondition: The result of importing is true", result);
			Factory.SaveAtEndOfImport(Logger);

			withholdingJournalCreationManagerMock.Verify(x => x.CreateAPWithholdingJournalsIfApplicable(It.IsAny<IMatching>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<ZDate>()), times);

			var payment = BusinessObjectFactory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals(1, payment.Length);
			AssertEquals("Amount", 100m, payment[0].AH_OSTotal);
			AssertEquals("After Import: AH_OutstandingAmount", 0m, payment[0].AH_OutstandingAmount);

			AssertEquals("Amount", 100m, apInvoice.AH_OSTotalAmount);
			AssertEquals("After Import: AH_OutstandingAmount", 0m, apInvoice.AH_OutstandingAmount);
		}

		public void TestProcessPaidTransactionsWithoutDataContextCollection()
		{
			var transactionBatch = new TransactionBatch();
			var firstTransactionInfo = CreateTransactionInfo(AccountingConstants.PaymentReceiptXUBTypes.AccountingMatching);
			firstTransactionInfo.MatchLineCollection.Add(new MatchLine());
			var secondTransactionInfo = new TransactionInfo();
			secondTransactionInfo.DataContext = new DataContext();
			firstTransactionInfo.PostDate = new ZDateTime(2000, 03, 15);
			secondTransactionInfo.PostDate = new ZDateTime(2000, 03, 15);
			LinkedTransactionID.Key = "AR INV 00001327";
			transactionBatch.TransactionCollection = new List<TransactionInfo>();
			transactionBatch.TransactionCollection.Add(firstTransactionInfo);
			transactionBatch.TransactionCollection.Add(secondTransactionInfo);

			var importer = new TransactionBatchImporter(transactionBatch, Logger, Factory);
			AssertNoExceptionThrown(() => importer.ImportTransactionBatch());
		}

		public void TestMatchOnly_WithNoMatchLines()
		{
			var transactionInfo = CreateTransactionInfo(AccountingConstants.PaymentReceiptXUBTypes.AccountingMatching);
			transactionInfo.MatchLineCollection.Clear();
			var transactionBatch = new TransactionBatch();
			transactionBatch.TransactionCollection = new List<TransactionInfo>();
			transactionBatch.TransactionCollection.Add(transactionInfo);

			var importer = new TransactionBatchImporter(transactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert(Logger.Logs.Contains("The Accounting Matching type must be accompanied by at least two match lines."));
		}

		[TestDate(2021, 03, 15)]
		public void TestImport_DuplicateOrganizationsTransactionIDInSameFile()
		{
			TransactionInfo.MatchLineCollection = null;
			TransactionInfo.OrganizationsTransactionID = "111";
			SetUpTransactionInfo(TransactionInfo);

			var transactionInfo2 = CreateTransactionInfo();
			transactionInfo2.MatchLineCollection = null;
			transactionInfo2.OrganizationsTransactionID = "111";
			SetUpTransactionInfo(transactionInfo2);
			TransactionBatch.TransactionCollection.Add(transactionInfo2);

			var transactionInfo3 = CreateTransactionInfo();
			transactionInfo3.MatchLineCollection = null;
			transactionInfo3.OrganizationsTransactionID = "222";
			SetUpTransactionInfo(transactionInfo3);
			TransactionBatch.TransactionCollection.Add(transactionInfo3);

			var transactionInfo4 = CreateTransactionInfo();
			transactionInfo4.MatchLineCollection = null;
			transactionInfo4.OrganizationsTransactionID = "222";
			SetUpTransactionInfo(transactionInfo4);
			TransactionBatch.TransactionCollection.Add(transactionInfo4);

			var transactionInfo5 = CreateTransactionInfo();
			transactionInfo5.MatchLineCollection = null;
			transactionInfo5.OrganizationsTransactionID = "222";
			SetUpTransactionInfo(transactionInfo5, LedgerTypes.AccountsPayable);
			TransactionBatch.TransactionCollection.Add(transactionInfo5);

			var transactionInfo6 = CreateTransactionInfo();
			transactionInfo6.MatchLineCollection = null;
			transactionInfo6.OrganizationsTransactionID = "222";
			SetUpTransactionInfo(transactionInfo6, LedgerTypes.AccountsPayable);
			TransactionBatch.TransactionCollection.Add(transactionInfo6);

			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert("Should contain duplicate OTI error.", Logger.Logs.Contains("Error - Transaction ID 111, 222 is already in use by another transaction in the XML file with the same organization, ledger and transaction type."));
		}

		[TestDate(2021, 03, 15)]
		public void TestImport_DuplicateChequeNumberInSameFile()
		{
			SetUpForTestChequeNumber(TransactionInfo, "1");

			var transactionInfo2 = CreateTransactionInfo();
			SetUpForTestChequeNumber(transactionInfo2, "1");
			TransactionBatch.TransactionCollection.Add(transactionInfo2);

			var transactionInfo3 = CreateTransactionInfo();
			SetUpForTestChequeNumber(transactionInfo3, "2");
			TransactionBatch.TransactionCollection.Add(transactionInfo3);

			var transactionInfo4 = CreateTransactionInfo();
			SetUpForTestChequeNumber(transactionInfo4, "2");
			TransactionBatch.TransactionCollection.Add(transactionInfo4);

			var transactionInfo5 = CreateTransactionInfo();
			SetUpForTestChequeNumber(transactionInfo5, "2");
			transactionInfo5.CheckBookCode = TestObjectCreator.AUDChequeBook2.AK_Code;
			TransactionBatch.TransactionCollection.Add(transactionInfo5);

			var transactionInfo6 = CreateTransactionInfo();
			SetUpForTestChequeNumber(transactionInfo6, "2");
			transactionInfo6.CheckBookCode = TestObjectCreator.AUDChequeBook2.AK_Code;
			TransactionBatch.TransactionCollection.Add(transactionInfo6);

			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false", !result);
			Assert("Should contain duplicate cheque number error.", Logger.Logs.Contains("Error - Cheque number 1, 2 is already in use by another transaction in the XML file."));
		}

		void SetUpForTestChequeNumber(TransactionInfo transactionInfo, string chequeNumber)
		{
			transactionInfo.MatchLineCollection = null;
			SetUpTransactionInfo(transactionInfo);
			transactionInfo.TransactionType = TransactionType.PAY;
			transactionInfo.PaymentOrReceiptType = PaymentOrReceiptType.CHQ;
			transactionInfo.CheckBookCode = TestObjectCreator.AUDChequeBook.AK_Code;
			transactionInfo.CheckNumberOrPaymentRef = chequeNumber;
		}

		public void TestNoSaveInTransactionActionInBoFactoryWhenFailToImport()
		{
			TransactionInfo.MatchLineCollection = null;
			SetUpTransactionInfo(TransactionInfo);
			TransactionInfo.PostDate = new ZDateTime(2000, 03, 15);

			var importer = new TransactionBatchImporter(TransactionBatch, Logger, Factory);
			var result = importer.ImportTransactionBatch();

			Assert("The result of importing is false due to the previous post date.", !result);
			AssertEquals("The count of SaveInTransactionActions should be 0 after importing unsuccessfully.", 0, Factory.BOFactory.SaveInTransactionActions.Count);
		}

		void SetUpTransactionInfo(TransactionInfo transactionInfo, string ledger = LedgerTypes.AccountsReceivable, TransactionType transactionType = TransactionType.REC, string description = "AR REC")
		{
			transactionInfo.Ledger = ledger;
			transactionInfo.TransactionType = transactionType;
			transactionInfo.TransactionDate = new ZDateTime(2021, 03, 15);
			transactionInfo.PostDate = new ZDateTime(2021, 03, 15);
			transactionInfo.OrganizationAddress = new OrganizationAddress() { OrganizationCode = "BAROPT" };
			transactionInfo.Description = description;
			transactionInfo.PaymentOrReceiptType = PaymentOrReceiptType.CSH;
			transactionInfo.BankAccount = TestObjectCreator.AUDBankAccount.AB_Code;
			transactionInfo.CheckBookCode = "";
			transactionInfo.CheckNumberOrPaymentRef = "CASH";
			transactionInfo.OSCurrency = new Currency();
			transactionInfo.OSCurrency.Code = TestObjectCreator.AUD.Code;
			transactionInfo.OSExGSTVATAmount = 100M;
			transactionInfo.OSTotal = 100M;
			transactionInfo.LocalCurrency = new Currency();
			transactionInfo.LocalCurrency.Code = TestObjectCreator.AUD.Code;
			transactionInfo.LocalExVATAmount = 100M;
			transactionInfo.LocalTotal = 100M;
		}

		TestObjectCreator TestObjectCreator => (testObjectCreator ??= new TestObjectCreator(BusinessObjectFactory));
		TestObjectCreator testObjectCreator;

		BusinessObjectFactory BusinessObjectFactory => (businessObjectFactory ??= new BusinessObjectFactory());
		BusinessObjectFactory businessObjectFactory;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 03, 01));
			_ = TestObjectCreator.AUDBankAccount.AB_Code;
			BusinessObjectFactory.Save();

			LinkedTransactionID = new LinkedTransactionID();
			MatchLine = new MatchLine();
			MatchLine.LinkedTransactionIDCollection = new List<LinkedTransactionID>();
			MatchLine.LinkedTransactionIDCollection.Add(LinkedTransactionID);

			TransactionInfo = CreateTransactionInfo();

			TransactionBatch = new TransactionBatch();
			TransactionBatch.TransactionCollection = new List<TransactionInfo>();
			TransactionBatch.TransactionCollection.Add(TransactionInfo);
			Logger = new TestErrorLogger();
		}

		TransactionInfo CreateTransactionInfo(string dataTargetType = "AccountingReceipt")
		{
			var transactionInfo = new TransactionInfo();
			transactionInfo.MatchLineCollection = new List<MatchLine>();
			transactionInfo.MatchLineCollection.Add(MatchLine);
			transactionInfo.Branch = Branch.New(GlbBranch.CurrentBranch);
			transactionInfo.Department = Department.New(GlbDepartment.CurrentDepartment);

			var context = new DataContext();
			var dataTargetCollection = new List<DataTarget>();
			dataTargetCollection.Add(new DataTarget() { Type = dataTargetType, Key = "" });
			context.DataTargetCollection = dataTargetCollection;
			transactionInfo.DataContext = context;

			return transactionInfo;
		}

		LinkedTransactionID LinkedTransactionID;
		MatchLine MatchLine;
		TransactionInfo TransactionInfo;
		TransactionBatch TransactionBatch;
		TestErrorLogger Logger;
	}
}
