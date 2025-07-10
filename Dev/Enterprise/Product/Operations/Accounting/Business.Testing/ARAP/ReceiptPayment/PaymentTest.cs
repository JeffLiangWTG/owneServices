using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public abstract class PaymentTest : ReceiptPaymentBaseTest
	{
		#region Implementation

		protected new Payment ReceiptPaymentBase
		{
			get { return (Payment)Header; }
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(PaymentValidation); }
		}

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		#endregion

		#region TestDrawerDetailsNotSetOnPAYTransaction

		public void TestDrawerDetailsNotSetOnPAYTransaction()
		{
			string drawer = "Test Drawer";
			string drawerBank = "Test Bank";
			string drawerBranch = "Test Drawer";

			ARReceipt receipt = Factory.New<ARReceipt>();
			receipt.AH_OH = TestObjectCreator.AALSHI.PK;
			receipt.AH_ReceiptType = ReceiptTypes.Cheque;
			receipt.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			receipt.AH_ChequeDrawer = drawer;
			receipt.AH_DrawerBank = drawerBank;
			receipt.AH_DrawerBranch = drawerBranch;
			receipt.AH_InvoiceAmount = 100;
			receipt.AH_OutstandingAmount = 100;
			receipt.AH_OSTotal = 100;
			receipt.AH_ChequeOrReference = "17895";

			Factory.Save();

			AssertEquals("Precondition: Saving Receipt will record Last Drawer deatils", drawer, TestObjectCreator.AALSHI.CompanyData.OB_ARPreviousChequeDrawer);
			AssertEquals("Precondition: Saving Receipt will record Last Drawer deatils", drawerBank, TestObjectCreator.AALSHI.CompanyData.OB_ARPreviousChequeDrawerBank);
			AssertEquals("Precondition: Saving Receipt will record Last Drawer deatils", drawerBranch, TestObjectCreator.AALSHI.CompanyData.OB_ARPreviousChequeDrawerBankBranch);

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_OH = TestObjectCreator.AALSHI.PK;
			AssertEquals("PostCondition: Drawer details should NOT be defauled", "", ReceiptPaymentBase.AH_ChequeDrawer);
			AssertEquals("PostCondition: Drawer details should NOT be defauled", "", ReceiptPaymentBase.AH_DrawerBank);
			AssertEquals("PostCondition: Drawer details should NOT be defauled", "", ReceiptPaymentBase.AH_DrawerBranch);
		}

		#endregion

		#region TestUnmatch

		public void TestUnmatch()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatch_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatch(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatch(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			// Note: everything in DB is positive for both ledgers
			ReceiptPaymentBase.AH_LocalExTaxAmount = 29M;
			ReceiptPaymentBase.AH_LocalOutstandingAmount = 0M;
			ReceiptPaymentBase.AH_FullyPaidDate = ZDateTime.Today;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AssertEquals("Cannot unmatch -1", UnmatchingResult.DataErrorInvoiceAmountAndMatchLinkAmountHasOppositeSigns, ((IMatching)ReceiptPaymentBase).CanUnmatch(-1M));
			AssertEquals("Cannot unmatch 30", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)ReceiptPaymentBase).CanUnmatch(30M));

			AssertEquals("Can unmatch 29", UnmatchingResult.Success, ((IMatching)ReceiptPaymentBase).CanUnmatch(29M));
			((IMatching)ReceiptPaymentBase).Unmatch(29M, 29M);
			AssertEquals("Outstanding amt should be 29", 29M, ReceiptPaymentBase.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 29m : 0m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Fully paid date should be null", ZDateTime.Empty, ReceiptPaymentBase.AH_FullyPaidDate);
		}

		#endregion

		#region TestUnmatchWithTax

		public void TestUnmatchWithTax()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchWithTax_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchWithTax(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			ReceiptPaymentBase.AH_LocalExTaxAmount = 90M;
			ReceiptPaymentBase.AH_LocalOutstandingAmount = 40M;
			ReceiptPaymentBase.AH_LocalTaxAmount = 10M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 40m * ReceiptPaymentBase.Multiplier_ForTestOnly;
				ReceiptPaymentBase.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 40m, ReceiptPaymentBase.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AssertEquals("Cannot unmatch 61 since greater than tax amt + invoice amt - outstanding amt", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount,
				((IMatching)ReceiptPaymentBase).CanUnmatch(61M));

			AssertEquals("Can unmatch 60 since equals tax amt + invoice amt - outstanding amt", UnmatchingResult.Success,
				((IMatching)ReceiptPaymentBase).CanUnmatch(60M));

			((IMatching)ReceiptPaymentBase).Unmatch(60M, 60M);

			AssertEquals("Outstanding amt should be 100", 100M, ReceiptPaymentBase.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 100m : 0m, ReceiptPaymentBase.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, ReceiptPaymentBase.AH_IsOSOutstandingAmountApplicable);
		}

		#endregion

		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			APPayment payment = Factory.New<APPayment>();
			AssertNull("No contact org yet", payment.DocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader);
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			payment.AH_OH = header.PK;
			AssertEquals("Contact Org for Document not null", header.PK, payment.DocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader.PK);
		}

		#endregion

		#region TestBusinessContext

		public void TestBusinessContext()
		{
			APPayment payment = Factory.New<APPayment>();
			AssertEquals("BusinessContext", BusinessContext.APTransaction, payment.DocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestPayeeAcountDetails

		public void TestPayeeAcountDetails()
		{
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_BankBranchName = "ABC Branch";
			accountDetails.A1_BankAddress1 = "ABC Add1";
			accountDetails.A1_BankAddress2 = "ABC Add2";
			accountDetails.A1_BankAddress3 = "ABC Add3";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.IsCancelled = false;
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			Assert(testPayment.AccountDetailsFound);
			AssertEquals(accountDetails.A1_BankAccount, testPayment.PayeeBankAccountNumber);
			AssertEquals(accountDetails.A1_AccountName, testPayment.AccountTitle);
			AssertEquals(accountDetails.A1_IBANNumber, testPayment.PayeeIBANNumber);
			AssertEquals(accountDetails.A1_BankBsb, testPayment.PayeeBankBSB);
			AssertEquals(accountDetails.A1_BankBranchName, testPayment.PayeeBankBranchName);
			AssertEquals(accountDetails.A1_BankAddress1, testPayment.PayeeBankAddress1);
			AssertEquals(accountDetails.A1_BankAddress2, testPayment.PayeeBankAddress2);
			AssertEquals(accountDetails.A1_BankAddress3, testPayment.PayeeBankAddress3);
			AssertEquals(accountDetails.A1_BankName, testPayment.PayeeBankName);
			AssertEquals(accountDetails.A1_BankSwift, testPayment.PayeeBankSwift);
			AssertEquals(accountDetails.A1_RN_NKCountryCode, testPayment.PayeeCountryCode);
			AssertEquals(accountDetails.A1_RX_NKAccountCurrency, testPayment.AccountCurrency);
		}

		#endregion

		#region TestGetNextChequeNumberTwice

		public void TestGetNextChequeNumberTwice()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			//Factory.Save();

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;  // cheque number should default to 34

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;

			AssertEquals("Bank should remain", testBank.PK, ReceiptPaymentBase.AH_AB);
			AssertEquals("Cheque Book should be cleared", ZGuid.Empty, ReceiptPaymentBase.ChequeBook);

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", ReceiptPaymentBase.AH_ChequeOrReference);
		}

		public void TestGetChequeBookWithDecimalInChequeReference()
		{
			ReceiptPaymentBase.AH_ChequeOrReference = "15.5";
			AssertEquals("ChequeBook should be empty", ZGuid.Empty, ReceiptPaymentBase.ChequeBook);
		}

		#endregion

		#region TestCanIncrementChequeNumber

		public void TestCanIncrementChequeNumber()
		{
			AccChequeBook chequeBook = GetAutoPrintChequeBook();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			chequeBook.AK_CurrentNo = 93;

			Factory.Save();

			ReceiptPaymentBase.ChequeBook = chequeBook.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "94";

			Assert("CanIncrementChequeNumber should be false", !ReceiptPaymentBase.CanIncrementChequeBookNumber);

			chequeBook.AK_AutoPrintCheque = ZBool.False;

			Assert("CanIncrementChequeNumber should be true", ReceiptPaymentBase.CanIncrementChequeBookNumber);

			ReceiptPaymentBase.AH_ChequeOrReference = "AF";

			Assert("CanIncrementChequeNumber should be false", !ReceiptPaymentBase.CanIncrementChequeBookNumber);

			ReceiptPaymentBase.AH_ChequeOrReference = "9";

			Assert("CanIncrementChequeNumber should be false", !ReceiptPaymentBase.CanIncrementChequeBookNumber);

			ReceiptPaymentBase.AH_ChequeOrReference = "94";

			Assert("CanIncrementChequeNumber should be true", ReceiptPaymentBase.CanIncrementChequeBookNumber);

			ReceiptPaymentBase.AH_ChequeOrReference = "94.5";

			Assert("CanIncrementChequeNumber should be false", !ReceiptPaymentBase.CanIncrementChequeBookNumber);
		}

		#endregion

		#region TestFactory_Saved

		public void TestFactory_Saved()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			chequeBook.AK_CurrentNo = 93;

			Factory.Save();

			ReceiptPaymentBase.ChequeBook = chequeBook.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "89";
			ReceiptPaymentBase.OnFactorySaved_ForTestOnly(true);
			AssertEquals("Current number should be 93", 93m, chequeBook.AK_CurrentNo);

			ReceiptPaymentBase.AH_ChequeOrReference = "95";
			ReceiptPaymentBase.OnFactorySaved_ForTestOnly(true);
			AssertEquals("Current number should be 96", 96m, chequeBook.AK_CurrentNo);
		}

		#endregion

		#region TestSavingChequeNumberGreaterThanCurrentNumber

		public void TestSavingChequeNumberGreaterThanCurrentNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 66;

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;
			// Cheque num should default to 66
			ReceiptPaymentBase.AH_ChequeOrReference = "77";

			Factory.Save();

			testChequeBook.Reload();
			AssertEquals("Current number should become 1 plus the number entered by the user", 78m, testChequeBook.AK_CurrentNo);
		}

		#endregion

		#region TestSavingChequeNumberConcurrently

		[SuspendCriticalValidation]
		public void TestSavingChequeNumberConcurrently()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 45;

			Factory.Save();

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;  // Cheque number should default to 45

			AssertEquals("Cheque number should default to 00045", "00045", ReceiptPaymentBase.AH_ChequeOrReference);

			BusinessObjectFactory concurrentFactory = new BusinessObjectFactory();

			ARPayment testARPay = concurrentFactory.NewWithValidTestData<ARPayment>();
			testARPay.AH_AB = testBank.PK;
			testARPay.ChequeBook = testChequeBook.PK;   // Cheque number should default to 00045

			AssertEquals("Cheque number should default to 00045", "00045", ReceiptPaymentBase.AH_ChequeOrReference);

			Factory.Save();

			Assert("ReceiptPaymentBase should save to DB", ReceiptPaymentBase.IsInDatabase);

			testChequeBook.Reload();
			AssertEquals("The current number should increment on saving", 46m, testChequeBook.AK_CurrentNo);

			concurrentFactory.Save();

			testChequeBook.Reload();
			AssertEquals("The current number should not increment since TestARPay has a lower number than the current number in DB",
				46m, testChequeBook.AK_CurrentNo);
		}

		#endregion

		#region TestCurrentNumberStaysSameIfEnteredNumberBelongsToCancelledPayment

		[SuspendCriticalValidation]
		public void TestCurrentNumberStaysSameIfEnteredNumberBelongsToCancelledPayment()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			Factory.Save();

			// Create a cancelled payment
			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>();
			testAPPay.AH_AB = testBank.PK;
			testAPPay.ChequeBook = testChequeBook.PK;
			testAPPay.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)testAPPay).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = testAPPay.PK;
			matchLink.AP_Amount = testAPPay.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			Factory.Save();

			testChequeBook.Reload();

			AssertEquals("Cheque number should be 00034", "00034", testAPPay.AH_ChequeOrReference);

			AssertEquals("Current number should increment", 35m, testChequeBook.AK_CurrentNo);

			testChequeBook.AK_CurrentNo = 67;
			Factory.Save();

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "34";

			Factory.Save();

			testChequeBook.Reload();
			AssertEquals("Current number should not be incremented since the cheque number was for a cancelled payment",
				67m, testChequeBook.AK_CurrentNo);
		}

		#endregion

		#region TestDoesChequeNumberBelongToCancelledPayment

		public void TestDoesChequeNumberBelongToCancelledPayment()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;

			AccBankAccount anotherBank = Factory.NewWithValidTestData<AccBankAccount>();
			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompanyBranch.GB_GC = anotherCompany.PK;

			ARPayment testARPay = Factory.NewWithValidTestData<ARPayment>(); //AR Payment
			testARPay.AH_AB = testBank.PK;
			testARPay.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)testARPay).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = testARPay.PK;
			matchLink.AP_Amount = testARPay.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			testARPay.AH_ChequeOrReference = "45";

			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>(); //AP Payment + AnotherBank
			testAPPay.AH_AB = anotherBank.PK;
			testAPPay.AH_IsCancelled = true;
			matchLink = ((IMatching)testAPPay).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = testAPPay.PK;
			matchLink.AP_Amount = testAPPay.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			testAPPay.AH_ChequeOrReference = "35";

			APPayment testAPPay2 = Factory.NewWithValidTestData<APPayment>(); //AP Payment + TestBank + AnotherCompanyBranch
			testAPPay2.AH_AB = testBank.PK;
			testAPPay2.AH_IsCancelled = true;
			matchLink = ((IMatching)testAPPay2).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = testAPPay2.PK;
			matchLink.AP_Amount = testAPPay2.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			testAPPay2.AH_ChequeOrReference = "23";
			testAPPay2.AH_GB = anotherCompanyBranch.PK;
			testAPPay2.AH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "45";

			Assert("There is a cancelled Payment with the current bank and cheque number = 45", ReceiptPaymentBase.ChequeBookBizO.BankAccount.IsChequeNumberUsedByCancelledPayment(ReceiptPaymentBase.AH_ChequeOrReference));

			ReceiptPaymentBase.AH_ChequeOrReference = "46";

			Assert("There is no cancelled payment with the current bank and cheque number = 46", !ReceiptPaymentBase.ChequeBookBizO.BankAccount.IsChequeNumberUsedByCancelledPayment(ReceiptPaymentBase.AH_ChequeOrReference));

			ReceiptPaymentBase.AH_ChequeOrReference = "35";

			Assert("There is no cancelled payment from the current bank with cheque number = 35", !ReceiptPaymentBase.ChequeBookBizO.BankAccount.IsChequeNumberUsedByCancelledPayment(ReceiptPaymentBase.AH_ChequeOrReference));

			ReceiptPaymentBase.AH_ChequeOrReference = "23";

			Assert("There is a cancelled payment from the current company with cheque number = 23", ReceiptPaymentBase.ChequeBookBizO.BankAccount.IsChequeNumberUsedByCancelledPayment(ReceiptPaymentBase.AH_ChequeOrReference));
		}

		#endregion

		#region TestSetReceiptType

		public override void TestSetReceiptType()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			//Factory.Save();

			AssertEquals("Precondition: Default receipt type is cheque", ReceiptTypes.Cheque, ReceiptPaymentBase.AH_ReceiptType);

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals("Bank account should be left the same", testBank.PK, ReceiptPaymentBase.AH_AB);
			AssertEquals("Cheque/Reference number should change to CASH", ReceiptTypes.Cash, ReceiptPaymentBase.AH_ChequeOrReference);
			Assert("ChequeBook field should clear", ReceiptPaymentBase.ChequeBook.IsEmpty);
			Assert("ChequeBook should become readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			// Cheque
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			AssertEquals("Bank account should be left the same", testBank.PK, ReceiptPaymentBase.AH_AB);
			Assert("Cheque/Reference number should become empty", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			// only for payment
			Assert("ChequeBook field should be editable", !ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			// CreditCard
			ReceiptPaymentBase.AH_ChequeOrReference = "234";

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.CreditCard;
			Assert("ChequeOrReference should be cleared", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			// Direct Debit
			ReceiptPaymentBase.AH_ChequeOrReference = "345";

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Assert("ChequeOrReference should be cleared", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			// Periodic Payment
			ReceiptPaymentBase.AH_ChequeOrReference = "345";

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.EFT;
			Assert("ChequeOrReference should be cleared", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			ReceiptPaymentBase.AH_ChequeOrReference = "345";

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.ScheduledEFT;
			Assert("ChequeOrReference should be cleared", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);

			ReceiptPaymentBase.AH_ChequeOrReference = "345";

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.CollectionRequest;
			Assert("ChequeOrReference should be cleared", ReceiptPaymentBase.AH_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", ReceiptPaymentBase.ChequeBookInfo.ReadOnly);
		}

		#endregion

		#region TestChequeBooks

		public void TestChequeBooks()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			AccChequeBook currentBranchBook = Factory.NewWithValidTestData<AccChequeBook>();
			currentBranchBook.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook otherBranchBook = Factory.NewWithValidTestData<AccChequeBook>();
			otherBranchBook.AK_GB = branch.PK;

			Factory.Save();

			AssertEquals("Cheque Books collection should be of valid type", typeof(ActiveChequeBookCollection), ReceiptPaymentBase.ChequeBooks.GetType());
			ReceiptPaymentBase.ChequeBooks.Load();

			AssertEquals("There should be 1 chequebook in the list", 1, ReceiptPaymentBase.ChequeBooks.Count);
			Assert("The chequebook should be currentBranchBook", ReceiptPaymentBase.ChequeBooks.Contains(currentBranchBook));
		}

		#endregion

		#region TestGetChequeNumberStatus

		public void TestGetChequeNumberStatus()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 45;

			Factory.Save();

			var receiptPayment = PrepareTransactionHeaderForTest() as Payment;
			receiptPayment.AH_AB = testBank.PK;
			receiptPayment.ChequeBook = testChequeBook.PK;

			AssertEquals("Cheque Number is 00045", "00045", receiptPayment.AH_ChequeOrReference);

			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				receiptPayment.GetChequeNumberStatus());

			Factory.Save();

			AssertEquals("ChequeNumStatus is LessThanCurrentNum", ChequeNumberStatus.LessThanCurrentNum, receiptPayment.GetChequeNumberStatus());

			APPayment cancelledPayment = Factory.NewWithValidTestData<APPayment>();
			cancelledPayment.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)cancelledPayment).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = cancelledPayment.PK;
			matchLink.AP_Amount = cancelledPayment.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			cancelledPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			cancelledPayment.AH_AB = testBank.PK;
			cancelledPayment.AH_ChequeOrReference = "11";

			Factory.Save();

			receiptPayment.AH_ChequeOrReference = "11";
			AssertEquals("ChequeNumStatus is CancelledPayment", ChequeNumberStatus.CancelledPayment, receiptPayment.GetChequeNumberStatus());

			receiptPayment.AH_ChequeOrReference = "";
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, receiptPayment.GetChequeNumberStatus());

			receiptPayment.AH_ChequeOrReference = "111";
			receiptPayment.ChequeBook = ZGuid.Empty;
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, receiptPayment.GetChequeNumberStatus());

			receiptPayment.AH_ChequeOrReference = "46";
			receiptPayment.ChequeBook = testChequeBook.PK;
			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				receiptPayment.GetChequeNumberStatus());

			receiptPayment.AH_ChequeOrReference = "11.5";
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, receiptPayment.GetChequeNumberStatus());
		}

		public void TestGetChequeNumberStatusWithFactoryRefreshDisabled()
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			var testBank = newFactory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			var testChequeBook = newFactory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 45;

			newFactory.Save();

			var payment1 = (Payment)newFactory.NewWithValidTestData(GetExpectedBusinessObjectType());
			payment1.AH_AB = testBank.PK;
			payment1.ChequeBook = testChequeBook.PK;
			payment1.ChequeOrReference = "00048";

			var payment2 = (Payment)newFactory.NewWithValidTestData(GetExpectedBusinessObjectType());
			payment2.AH_AB = testBank.PK;
			payment2.ChequeBook = testChequeBook.PK;
			payment2.ChequeOrReference = "00045";

			newFactory.Save();

			AssertEquals("ChequeNumStatus is LessThanCurrentNum", ChequeNumberStatus.LessThanCurrentNum, payment2.GetChequeNumberStatus());

			var loadedChequeBook = new BusinessObjectFactory().Load<AccChequeBook>(testChequeBook.PK);
			AssertEquals(49, loadedChequeBook.AK_CurrentNo.ToZInt());
		}

		#endregion

		#region TestGetChequeBook
		public void TestGetChequeBook()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.AH_AB = testBank.PK;
			testPayment.AH_TransactionNum = "00001101";
			testPayment.AH_ReceiptType = "CHQ";

			AssertNull("Related payment approval should be null", testPayment.RelatedPaymentApproval);
			Assert("Cheque book should be empty", testPayment.ChequeBook.IsEmpty);

			AccPaymentApproval testPaymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			testPaymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			testPaymentApproval.AV_Ledger = testPayment.AH_Ledger;
			testPaymentApproval.AV_AB = testBank.PK;
			testPaymentApproval.AV_AK = testChequeBook.PK;
			testPaymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.Australia;
			testPaymentApproval.AV_ChequeOrReference = testChequeBook.AK_CurrentNo.ToString();
			testPaymentApproval.AV_Amount = 600m;
			testPaymentApproval.AV_AH = testPayment.PK;

			Factory.Save();

			AssertEquals("Cheque book should be the same as from TestPaymentApproval", testPayment.ChequeBook, testPaymentApproval.AV_AK);
		}

		public void TestGetChequeBookWithoutPaymentApproval()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook1.AK_AB = testBank.PK;
			testChequeBook1.AK_GB = GlbBranch.CurrentBranch.PK;
			testChequeBook1.AK_StartNo = 1;
			testChequeBook1.AK_LastNo = 100;
			testChequeBook1.AK_CurrentNo = 34;
			testChequeBook1.AK_IsActive = ZBool.True;

			AccChequeBook testChequeBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook2.AK_AB = testBank.PK;
			testChequeBook2.AK_GB = GlbBranch.CurrentBranch.PK;
			testChequeBook2.AK_StartNo = 101;
			testChequeBook2.AK_LastNo = 1000;
			testChequeBook2.AK_CurrentNo = 134;
			testChequeBook2.AK_IsActive = ZBool.True;
			Factory.Save();

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			testPayment.AH_AB = testBank.PK;
			testPayment.AH_TransactionNum = "00001101";
			testPayment.AH_ReceiptType = "CHQ";
			testPayment.AH_ChequeOrReference = "1200";
			Factory.Save();
			AssertEquals("Cheque book of the payment should be empty as there is no cheque book with number range which includes payment reference number", ZGuid.Empty, testPayment.ChequeBook);

			testPayment.AH_ChequeOrReference = ((decimal)int.MaxValue + 1).ToString();
			Factory.Save();
			AssertEquals("Cheque book of the payment should be empty as there is no cheque book with number range which includes payment reference number", ZGuid.Empty, testPayment.ChequeBook);

			testPayment.AH_ChequeOrReference = "120";
			Factory.Save();
			AssertEquals("Cheque book should be TestChequeBook2 as payment reference number falls into cheque book range and they have the same bank account", testChequeBook2.PK, testPayment.ChequeBook);
		}
		#endregion

		#region TestUpdateChequeNumber

		public void TestUpdateChequeNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.AH_AB = testBank.PK;
			testPayment.AH_TransactionNum = "00001101";
			testPayment.AH_ReceiptType = "CHQ";

			AssertNull("Related payment approval should be null", testPayment.RelatedPaymentApproval);
			Assert("Cheque book should be empty", testPayment.ChequeBook.IsEmpty);

			AccPaymentApproval testPaymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			testPaymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			testPaymentApproval.AV_Ledger = testPayment.AH_Ledger;
			testPaymentApproval.AV_AB = testBank.PK;
			testPaymentApproval.AV_AK = testChequeBook.PK;
			testPaymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.Australia;
			testPaymentApproval.AV_ChequeOrReference = testChequeBook.AK_CurrentNo.ToString();
			testPaymentApproval.AV_Amount = 600m;
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPaymentApproval.AV_AH = testPayment.PK;

			Factory.Save();

			AssertEquals("Cheque book should be the same as from TestPaymentApproval", testPayment.ChequeBook, testPaymentApproval.AV_AK);

			testPayment.UpdateChequeNumber("35", true);
			Factory.Save();

			AssertEquals("Cheque number on payment should equal", "00035", testPayment.AH_ChequeOrReference);
			AssertEquals("Cheque number on payment approval should be same as payment", testPayment.AH_ChequeOrReference, testPaymentApproval.AV_ChequeOrReference);
		}

		#endregion

		#region TestSetChequeBook

		public void TestSetChequeBook()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook();
			autoPrintChequeBook.AK_StartNo = 1;
			autoPrintChequeBook.AK_LastNo = 100;
			autoPrintChequeBook.AK_CurrentNo = 35;

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBook = testChequeBook.PK;
			AssertEquals("The chequebook should be equal to the assigned one", testChequeBook.PK, testPayment.ChequeBook);
			AssertEquals("The Cheque number should be defaulted", testChequeBook.AK_CurrentNo.ToString(), testPayment.AH_ChequeOrReference);
			Assert("Auto allocation mode should not be set", !((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);

			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("The Cheque number should be defaulted to CSH", "CSH", testPayment.AH_ChequeOrReference);

			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBook = ZGuid.Empty;
			testPayment.ChequeBook = autoPrintChequeBook.PK;
			Assert("Auto allocation mode should be set", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
			AssertEquals("The chequebook should be equal to the assigned one", autoPrintChequeBook.PK, testPayment.ChequeBook);
			Assert("The Cheque number should not be defaulted", testPayment.AH_ChequeOrReference.IsEmpty);
		}

		#endregion

		#region TestChangingBankAccountResetsChequeBook

		public void TestChangingBankAccountResetsChequeBook()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Factory.Save();

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.ChequeBook = chequeBook.PK;
			ReceiptPaymentBase.AH_AB = bank.PK;
			Assert("Cheque Book should reset to empty when bank account is changed", ReceiptPaymentBase.ChequeBook.IsEmpty);
		}

		#endregion

		#region TestChangingBankAccountResetsChequeBookCollection

		public void TestChangingBankAccountResetsChequeBookCollection()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount bank1 = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount bank2 = Factory.NewWithValidTestData<AccBankAccount>();

			bank1.AB_RX_NKAccountCurrency = currency1.RX_Code;
			bank2.AB_RX_NKAccountCurrency = currency2.RX_Code;

			AccChequeBook cheques1 = Factory.NewWithValidTestData<AccChequeBook>();
			AccChequeBook cheques2 = Factory.NewWithValidTestData<AccChequeBook>();

			cheques1.AK_AB = bank1.PK;
			cheques1.AK_GB = GlbBranch.CurrentBranch.PK;
			cheques2.AK_AB = bank2.PK;
			cheques2.AK_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			ReceiptPaymentBase.AH_AB = bank1.PK;
			ReceiptPaymentBase.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques1", ReceiptPaymentBase.ChequeBooks.Contains(cheques1));
			Assert("ChequeBook collection should not contain Cheques2", !ReceiptPaymentBase.ChequeBooks.Contains(cheques2));

			ReceiptPaymentBase.AH_AB = bank2.PK;
			ReceiptPaymentBase.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques2", ReceiptPaymentBase.ChequeBooks.Contains(cheques2));
			Assert("ChequeBook collection should not contain Cheques1", !ReceiptPaymentBase.ChequeBooks.Contains(cheques1));
		}

		#endregion

		#region TestValidationWhenInMatchingContext

		public override void TestValidationWhenInMatchingContext()
		{
			Assert("Should be Payment Validation", typeof(PaymentValidation).IsAssignableFrom(Header.Validation.GetType()));
			IMatchingCollection parentCollectionForMatching = new IMatchingCollection(Factory);
			parentCollectionForMatching.Add(Header);
			Assert("Should be still be Payment Validation" + " Header.Validation Type is " + Header.Validation.GetType().FullName, typeof(PaymentValidation).IsAssignableFrom(Header.Validation.GetType()));
		}

		public void TestValidationTypeWhenPaymentIsInDB()
		{
			Factory.Save();

			Payment paymentToLoad = (Payment)Factory.Load(GetExpectedBusinessObjectType(), Header.PK);

			IMatchingCollection parentCollectionForMatching = new IMatchingCollection(Factory);
			parentCollectionForMatching.Add(paymentToLoad);
			Assert("Should be Matching Validation" + " Header.Validation Type is " + paymentToLoad.Validation.GetType().FullName, typeof(MatchingValidation).IsAssignableFrom(paymentToLoad.Validation.GetType()));
		}

		#endregion

		#region Validation Tests

		#region TestValidateChequeBook

		public void TestValidateChequeBook()
		{
			Assert("Precondition: cheque book should have no errors", !ReceiptPaymentBase.ChequeBookInfo.HasErrors());
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.ChequeBook = ZGuid.Empty;
			Assert("ChequeBook should have errors since it cant be empty", ReceiptPaymentBase.ChequeBookInfo.HasErrors());

			ReceiptPaymentBase.ChequeBook = ZGuid.Invalid;
			Assert("ChequeBook should have errors since the guid is invalid", ReceiptPaymentBase.ChequeBookInfo.HasErrors());

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.CreditCard;
			ReceiptPaymentBase.ChequeBook = ZGuid.Empty;
			Assert("ChequeBook should not have errors since it's not required for CreditCard", !ReceiptPaymentBase.ChequeBookInfo.HasErrors());
		}

		#endregion

		#region TestValidateChequeOrReference

		public void TestValidateChequeOrReference()
		{
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;
			ReceiptPaymentBase.AH_ChequeOrReference = "QWER";
			Assert("Should be no errors, CASH allows alphabetical chars", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.CreditCard;
			ReceiptPaymentBase.AH_ChequeOrReference = "asdf";
			Assert("Should be no errors, CreditCard allows alphabetical chars", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectCredit;
			ReceiptPaymentBase.AH_ChequeOrReference = "ZXCV";
			Assert("Should be no errors, DirectCredit allows alphabetical chars", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cheque;
			ReceiptPaymentBase.AH_ChequeOrReference = "UIOP";
			Assert("Should give errors, Cheque allows numbers only", ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());
		}

		#endregion

		#region TestValidateUniqueChequeNumber

		public void TestValidateUniqueChequeNumber()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 10;
			testChequeBook.AK_CurrentNo = 6;

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_ChequeNumDigits = (ZByte)5;

			APPayment cancelledPayment = Factory.NewWithValidTestData<APPayment>();
			cancelledPayment.AH_IsCancelled = true;
			TransactionMatchLink matchLink = ((IMatching)cancelledPayment).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = cancelledPayment.PK;
			matchLink.AP_Amount = cancelledPayment.AH_OutstandingAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			cancelledPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			cancelledPayment.AH_AB = bank.PK;
			cancelledPayment.AH_ChequeOrReference = "3";    // must be set in this order

			Factory.Save();

			ReceiptPaymentBase.AH_AB = bank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;  // must be set in this order

			AssertEquals("Cheque Number should default to 00006", "00006", ReceiptPaymentBase.AH_ChequeOrReference);
			Assert("Cheque Number should have no errors", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());

			ReceiptPaymentBase.AH_ChequeOrReference = "5";
			Assert("Cheque Number should have no errors", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());

			ReceiptPaymentBase.AH_ChequeOrReference = "3";
			Assert("Cheque Number should have no errors because the number belongs to a cancelled payment", !ReceiptPaymentBase.AH_ChequeOrReferenceInfo.HasErrors());
		}

		#endregion

		#region TestValidateReceiptType

		public void TestValidateReceiptType()
		{
			Assert("Precondition: ReceiptType should have no errors", !ReceiptPaymentBase.AH_ReceiptTypeInfo.HasErrors());
			ReceiptPaymentBase.AH_ReceiptType = "#$#";
			Assert("Invalid ReceiptType should produce errors", ReceiptPaymentBase.AH_ReceiptTypeInfo.HasErrors());
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.Cash;
			Assert("Valid ReceiptType - should have no errors", !ReceiptPaymentBase.AH_ReceiptTypeInfo.HasErrors());
		}

		#endregion

		#region TestCheckAH_OSExTaxAmount

		public override void TestCheckAH_OSExTaxAmount()
		{
			Assert("Precondition: OSAmount should not have errors", !ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());

			ReceiptPaymentBase.AH_OSExTaxAmount = 0M;
			Assert("OSAmount should have errors since 0 is not allowed", ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());

			ReceiptPaymentBase.AH_OSExTaxAmount = -90M;
			Assert("OSAmount should have errors since cannot be negative", ReceiptPaymentBase.AH_OSExTaxAmountInfo.HasErrors());
		}

		#endregion

		#endregion

		#region Matching Tests

		public override void TestMatchingBaseObject()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			ReceiptPaymentBase.AH_OH = testOrg.PK;
			ReceiptPaymentBase.AH_LocalExTaxAmount = 46M;
			ReceiptPaymentBase.AH_OSExTaxAmount = 46M;
			ZDateTime expectedDate = ZDateTime.Now;
			ReceiptPaymentBase.CurrentMatchingDate = expectedDate;

			fMatchingBaseObject = ReceiptPaymentBase.MatchingBaseObject;
			Assert("Matching should not be for non-Payments", !fMatchingBaseObject.IsNotMatchingPayment);
			Assert("Matching should be for Receipt/Payment", fMatchingBaseObject.IsMatchingPaymentOrReceipt);
			AssertEquals("Matching object's MatchDate should have CurrentMatchingDate", expectedDate.Date,
				fMatchingBaseObject.MatchDate);
		}

		#endregion

		#region Testing Cheque Number Leading Zero
		public void TestAH_ChequeOrReference()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;
			testBank.AB_ChequeNumDigits = (ZByte)5;
			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "77";

			AssertEquals("Should be 00077", "00077", ReceiptPaymentBase.AH_ChequeOrReference);
		}
		#endregion

		#region Testing DoesChequeNumberAlreadyExist
		public void TestDoesChequeNumberAlreadyExist()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount testBank2 = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;

			Payment pay1 = (Payment)GetNewBusinessObject();
			pay1.AH_AB = testBank.PK;
			pay1.AH_ChequeOrReference = "11";

			Payment pay2 = (Payment)GetNewBusinessObject();
			pay2.AH_AB = testBank2.PK;
			pay2.AH_ChequeOrReference = "12";

			ARReceipt receipt1 = Factory.NewWithValidTestData<ARReceipt>();
			receipt1.AH_AB = testBank.PK;
			receipt1.AH_ChequeOrReference = "12";

			Factory.Save();

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.ChequeBook = testChequeBook.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "11";
			AssertEquals("Should be true", true, ReceiptPaymentBase.ChequeBookBizO.BankAccount.HasChequeNumberBeenUsed(ReceiptPaymentBase.AH_ChequeOrReference, ReceiptPaymentBase.PK));

			ReceiptPaymentBase.AH_AB = testBank2.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "11";
			AssertEquals("Should be false", false, ReceiptPaymentBase.BankAccount.HasChequeNumberBeenUsed(ReceiptPaymentBase.AH_ChequeOrReference, ReceiptPaymentBase.PK));

			ReceiptPaymentBase.AH_AB = testBank.PK;
			ReceiptPaymentBase.AH_ChequeOrReference = "12";
			AssertEquals("Should be false", false, ReceiptPaymentBase.BankAccount.HasChequeNumberBeenUsed(ReceiptPaymentBase.AH_ChequeOrReference, ReceiptPaymentBase.PK));
		}
		#endregion

		#region DirectDebitNumber Test

		public override void TestDirectDebitNumber()
		{
			ReceiptPaymentBase.AH_ReceiptBatchNo = "00001002";
			AssertEquals("DirectDebit number should be 00001002", "00001002", ReceiptPaymentBase.DirectDebitNumber);
		}

		#endregion

		#region AllowAutoDDR

		public void TestAllowAutoDDR()
		{
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			Assert(testPayment.AllowAutoDDR);

			accountDetails.A1_BankBsb = "";
			Assert(!testPayment.AllowAutoDDR);

			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;

			AssertNotNull(testPayment.AccountDetails);
		}

		#endregion

		#region AccountDetailsFound

		public void TestAccountDetailsFound()
		{
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			Assert("Should be true", testPayment.AccountDetailsFound);

			accountDetails.A1_IsDefaultAccount = false;
			testPayment.ResetAccountDetails();
			Assert("Should be false, AccountDetails is not Default", !testPayment.AccountDetailsFound);

			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewZealand;
			testPayment.ResetAccountDetails();
			Assert("Should be false, AccountDetails Currency of NZD is different from the Payment Currency AUD", !testPayment.AccountDetailsFound);

			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.ResetAccountDetails();
			Assert("Should be false, AccountDetails PaymentMethod of CHQ is different from Payment ReceiptType of DDR", !testPayment.AccountDetailsFound);
		}

		#endregion

		#region IChequeNumberAutoAllocation Members Tests

		public void TestIChequeNumberAutoAllocation_ChequeBookPK()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Payment testPayment = (Payment)GetNewBusinessObject();
			AssertNull("Should return empty cheque book", ((IChequeNumberAutoAllocation)testPayment).ChequeBook);
			testPayment.ChequeBook = testChequeBook.PK;
			AssertEquals("Should return TestChequeBook PK", testChequeBook, ((IChequeNumberAutoAllocation)testPayment).ChequeBook);
		}

		public virtual void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccChequeBook testChequeBook = newFactory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_IsActive = ZBool.True;
			newFactory.Save();

			Payment testPayment = (Payment)GetNewBusinessObject();
			testPayment.ChequeBook = testChequeBook.PK;
			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("123");
			AssertEquals("AH_ChequeOrReference should be set", "123", testPayment.AH_ChequeOrReference);
			APPaymentApprovalWithAuthorisation testPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			testPaymentApproval.AV_AH = testPayment.PK;
			Assert("AV_ChequeOrReference should be empty on payment approval", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("123");
			AssertEquals("AH_ChequeOrReference should be set", "123", testPayment.AH_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be set on the payment", "123", testPayment.AH_ChequeOrReference);

			testPayment.ChequeBookBizO.AK_IsActive = ZBool.False;
			((IChequeNumberAutoAllocation)testPayment).AllocationOrPrintingFailed();
			Assert("Cheque book should be reloaded", testPayment.ChequeBookBizO.AK_IsActive);
			Assert("AH_ChequeOrReference should be reset", testPayment.AH_ChequeOrReference.IsEmpty);
			Assert("AV_ChequeOrReference should be reset on payment approval", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
		}

		public void TestIChequeNumberAutoAllocation_IsAllocationPerformed()
		{
			Payment testPayment = (Payment)GetNewBusinessObject();
			Assert("Should return False by defualt", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			((IChequeNumberAutoAllocation)testPayment).AssignChequeNumber("123");
			Assert(((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
		}

		public void TestIChequeNumberAutoAllocation_Printing_ObjectPK()
		{
			Payment testPayment = (Payment)GetNewBusinessObject();
			AssertEquals(testPayment.PK, ((IChequeNumberAutoAllocation)testPayment).Printing_ObjectPK);
		}

		public void TestIChequeNumberAutoAllocation_Printing_PrinterPK()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			chequeBookFactory.Save();

			Payment testPayment = (Payment)GetNewBusinessObject();
			AssertEquals("Should return empty printer", ZGuid.Empty, ((IChequeNumberAutoAllocation)testPayment).Printing_PrinterPK);
			testPayment.ChequeBook = testBookWithAutoAllocation.PK;
			AssertEquals("Should return TestBookWithAutoAllocation.AK_SQ", testBookWithAutoAllocation.AK_SQ, ((IChequeNumberAutoAllocation)testPayment).Printing_PrinterPK);
		}

		public void TestIChequeNumberAutoAllocation_ChequeIsAutoPrinted()
		{
			Payment testPayment = (Payment)GetNewBusinessObject();
			Assert("Default value", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted = ZBool.True;
			Assert("value should be set", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted = ZBool.False;
			Assert("value should be changed back", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
		}
		#endregion

		public void TestHasAcceptedEPaymentDeal()
		{
			var payment1 = Factory.NewWithValidTestData<APPayment>();
			payment1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment1.AH_GC = GlbCompany.CurrentCompany.PK;
			AssertNull(payment1.RelatedPaymentApproval);
			Assert("Should be false because no RelatedPaymentApproval", !payment1.HasAcceptedEPaymentDeal);

			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Queued);
			var deal3 = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Accepted);
			Factory.Save();

			var approval2 = Factory.Load<PaymentApprovalBase>(deal2.Quote.PaymentApproval.PK);
			var payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_ReceiptType = ReceiptTypes.EPayment;
			payment2.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment2.AH_GC = GlbCompany.CurrentCompany.PK;
			approval2.AV_AH = payment2.PK;
			Assert("Should be false because status is queued", !payment2.HasAcceptedEPaymentDeal);

			var approval3 = Factory.Load<PaymentApprovalBase>(deal3.Quote.PaymentApproval.PK);
			var payment3 = Factory.NewWithValidTestData<APPayment>();
			payment3.AH_ReceiptType = ReceiptTypes.Cash;
			payment3.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment3.AH_GC = GlbCompany.CurrentCompany.PK;
			approval3.AV_AH = payment3.PK;
			Assert("Should be false because PaymentType is not EPayment", !payment3.HasAcceptedEPaymentDeal);

			payment3.AH_ReceiptType = ReceiptTypes.EPayment;
			Assert("Should be true", payment3.HasAcceptedEPaymentDeal);
		}

		public void TestSetReceiptTypeDoesNotChangeValueIfInDatabase()
		{
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			ReceiptPaymentBase.AH_ChequeOrReference = "TEST001";
			Factory.Save();
			AssertEquals("TEST001", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectDebit;
			AssertEquals("TEST001", ReceiptPaymentBase.AH_ChequeOrReference);
		}

		public void TestSetReceiptTypeDoesChangeValueIfNotInDatabase()
		{
			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			ReceiptPaymentBase.AH_ChequeOrReference = "TEST001";
			AssertEquals("TEST001", ReceiptPaymentBase.AH_ChequeOrReference);

			ReceiptPaymentBase.AH_ReceiptType = ReceiptTypes.DirectDebit;
			AssertEquals("", ReceiptPaymentBase.AH_ChequeOrReference);
		}

		public void TestIsENettPayment()
		{
			foreach (FieldInfo receiptTypeField in typeof(ReceiptTypes).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				string receiptType = (string)receiptTypeField.GetValue(null);
				ReceiptPaymentBase.AH_ReceiptType = receiptType;
				AssertEquals(receiptType, receiptType == ReceiptTypes.eNettDirectDebit, ReceiptPaymentBase.IsENettPayment);
			}
		}

		public override void TestGetWritableProperties()
		{
			AssertEquals("GetWritableProperties_ForTestOnly().Count", 4, ReceiptPaymentBase.GetWritableProperties_ForTestOnly().Count);
			AssertEquals("GetWritableProperties_ForTestOnly()[0]", "OSPartialPaymentAmount", ReceiptPaymentBase.GetWritableProperties_ForTestOnly()[0]);
			AssertEquals("GetWritableProperties_ForTestOnly()[1]", "MatchStatus", ReceiptPaymentBase.GetWritableProperties_ForTestOnly()[1]);
			AssertEquals("GetWritableProperties_ForTestOnly()[2]", "MatchStatusReasonCode", ReceiptPaymentBase.GetWritableProperties_ForTestOnly()[2]);
			AssertEquals("GetWritableProperties_ForTestOnly()[3]", "IncludeInTheBatch", ReceiptPaymentBase.GetWritableProperties_ForTestOnly()[3]);
		}

		public void TestDDLTypeDescription()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			APPayment payment1 = creator.CreateAPPayment(1.0M, 1.0M, ZDateTime.Now, ZDateTime.Now, creator.ABIGAS.PK, creator.AUDBankAccount.PK);
			AssertNull(payment1.PaymentMethods.GetDescriptionFromCode("DDL"));
			Factory.Save();
			AssertEquals("Direct Debit Line Description", "Direct Debit Line (rolled up into DDR Batch on Bank Rec.)", payment1.PaymentMethods.GetDescriptionFromCode("DDL"));
		}

		public void TestPaymentAuditDetails()
		{
			var accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			var bankCreateTime = new ZDateTime(2022, 11, 6, 16, 42, 0);
			var bankLastEditTime = new ZDateTime(2022, 11, 6, 17, 42, 0);
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_SystemCreateUser = "TST";
			accountDetails.A1_SystemCreateTimeUtc = bankCreateTime;
			accountDetails.A1_SystemLastEditUser = "EDT";
			accountDetails.A1_SystemLastEditTimeUtc = bankLastEditTime;

			var testPayment = (Payment)GetNewBusinessObject();
			testPayment.AH_OH = TestObjectCreator.AALSHI.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.AH_SystemCreateTimeUtc = new ZDateTime(2022, 11, 6, 16, 42, 0);

			AssertEquals(ZString.Empty, testPayment.BankCreateUser);
			AssertEquals(ZDateTime.Empty, testPayment.BankCreateTimeLocal);
			AssertEquals(ZString.Empty, testPayment.BankLastEditUser);
			AssertEquals(ZDateTime.Empty, testPayment.BankLastEditTimeLocal);

			testPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;

			AssertEquals("TST", testPayment.BankCreateUser);
			AssertEquals(bankCreateTime.ToLocalBranchTime(), testPayment.BankCreateTimeLocal);
			AssertEquals("EDT", testPayment.BankLastEditUser);
			AssertEquals(bankLastEditTime.ToLocalBranchTime(), testPayment.BankLastEditTimeLocal);
		}

		public void TestIncludeInTheBatch()
		{
			var payment = ReceiptPaymentBase;
			payment.AH_OSExTaxAmount = 11;
			payment.AH_LocalExTaxAmount = 10;

			var testHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			var testDDRCollection = new DirectDebitBatchLineCollection(Factory, testHeader);
			payment.SetDDRCollection_ForTestOnly(testDDRCollection);
			payment.IncludeInTheBatch = false;

			AssertEquals("No payment be included in batch, amount should zero.", 0m, payment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("No payment be included in batch, amount should zero.", 0m, payment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			payment.IncludeInTheBatch = false;

			AssertEquals("No payment include in batch, amount should not change when IncludeInTheBatch is not change.", 0m, payment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("No payment include in batch, amount should not change when IncludeInTheBatch is not change.", 0m, payment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			payment.IncludeInTheBatch = true;

			AssertEquals("Payment include in batch, amount should be payment AH_OSExTaxAmount.", 11m, payment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("Payment include in batch, amount should be payment AH_LocalExTaxAmount.", 10m, payment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);

			payment.IncludeInTheBatch = true;

			AssertEquals("Payment include in batch, amount should not change when IncludeInTheBatch is not change.", 11m, payment.DDRCollection.DDRHeader.AH_OSExTaxAmount);
			AssertEquals("Payment include in batch, amount should not change when IncludeInTheBatch is not change.", 10m, payment.DDRCollection.DDRHeader.AH_LocalExTaxAmount);
		}

		#region IDirectDebitBatchTransaction

		public void TestCode()
		{
			Assert(!ReceiptPaymentBase.Code.IsEmpty);
			Assert(ReceiptPaymentBase.Code == ReceiptPaymentBase.AH_Ledger);
		}

		public void TestPayeeBankAccountNumber()
		{
			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = aALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			ReceiptPaymentBase.AH_OH = aALSHI.PK;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			AssertEquals(accountDetails.A1_BankAccount, ReceiptPaymentBase.PayeeBankAccountNumber);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			AssertEquals("", ReceiptPaymentBase.PayeeBankAccountNumber);
		}

		public void TestPayeeBankAccountNumber_DeletedAccountDetails()
		{
			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = aALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			ReceiptPaymentBase.AH_OH = aALSHI.PK;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			Factory.Save();

			AssertNotNull(ReceiptPaymentBase.AccountDetails);

			var factoryforDeletion = new BusinessObjectFactory();
			var accDetailsToDelete = factoryforDeletion.Load<AccAPAccountDetails>(ReceiptPaymentBase.AccountDetails.PK);
			accDetailsToDelete.Delete();
			factoryforDeletion.Save();

			AssertNull(ReceiptPaymentBase.AccountDetails);
		}

		public void TestAccountTitle()
		{
			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = TestObjectCreator.AALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			ReceiptPaymentBase.AH_OH = aALSHI.PK;
			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertEquals(accountDetails.A1_AccountName, ReceiptPaymentBase.AccountTitle);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			AssertEquals("", ReceiptPaymentBase.AccountTitle);
		}

		public void TestPayeeBankBSB()
		{
			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails = aALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Account Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "123456";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			ReceiptPaymentBase.AH_OH = aALSHI.PK;
			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			AssertEquals(accountDetails.A1_BankBsb, ReceiptPaymentBase.PayeeBankBSB);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			AssertEquals("", ReceiptPaymentBase.PayeeBankBSB);
		}

		public void TestAccountDetails()
		{
			OrgHeader aALSHI = TestObjectCreator.AALSHI;
			AccAPAccountDetails accountDetails1 = aALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails1.A1_AccountName = "Account Name";
			accountDetails1.A1_BankAccount = "Bank";
			accountDetails1.A1_BankBsb = "123456";
			accountDetails1.A1_IsDefaultAccount = true;
			accountDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			AccAPAccountDetails accountDetails2 = aALSHI.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails2.A1_AccountName = "Account Name";
			accountDetails2.A1_BankAccount = "Bank";
			accountDetails2.A1_BankBsb = "123456";
			accountDetails2.A1_IsDefaultAccount = true;
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Afghanistan;

			ReceiptPaymentBase.AH_OH = aALSHI.PK;
			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			AssertNotNull(ReceiptPaymentBase.AccountDetails);
			AssertEquals("AccountDetails 1 should be retrieved", accountDetails2.PK, ReceiptPaymentBase.AccountDetails.PK);

			ReceiptPaymentBase.AH_RX_NKTransactionCurrency = ZString.Empty;
			AssertEquals("AccountDetails 2 should be retrieved", accountDetails1.PK, ReceiptPaymentBase.AccountDetails.PK);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertNull("No Account Details should be found", ReceiptPaymentBase.AccountDetails);

			ReceiptPaymentBase.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertNotNull("Account Details should be found", ReceiptPaymentBase.AccountDetails);

			ReceiptPaymentBase.AH_OH = ZGuid.Empty;
			AssertNull("No Account Details should be found", ReceiptPaymentBase.AccountDetails);
		}

		public void TestIncludeInTheBatchReadOnly()
		{
			ReceiptPaymentBase.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			ReceiptPaymentBase.AH_ReceiptType = "DDR";
			Factory.Save();

			DirectDebitBatchHeader directDebitBatchHeader = Factory.New<DirectDebitBatchHeader>();
			directDebitBatchHeader.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			AssertEquals(false, directDebitBatchHeader.Lines[0].IncludeInTheBatchInfo.ReadOnly);
		}

		#endregion

		#region FundingBankAccount

		public void TestFundingBankAccountPK()
		{
			var payment = (Payment)GetNewBusinessObject();
			payment.AH_ReceiptType = ReceiptTypes.EPayment;
			payment.AH_TransactionNum = "0001";
			AssertNull(payment.RelatedPaymentApproval);
			AssertEquals(ZGuid.Empty, payment.FundingBankAccountPK);
			AssertEquals("AUD", payment.FundingCurrency);

			var paymentApproval = Factory.NewWithValidTestData<AccPaymentApproval>();
			paymentApproval.AV_AH = payment.PK;
			paymentApproval.AV_Ledger = "AP";
			AssertEquals(ZGuid.Empty, payment.FundingBankAccountPK);
			AssertEquals("AUD", payment.FundingCurrency);

			paymentApproval.AV_AB_FundingBankAccount = TestObjectCreator.USDBankAccount.PK;
			AssertEquals(TestObjectCreator.USDBankAccount.PK, payment.FundingBankAccountPK);
			AssertEquals("USD", payment.FundingCurrency);
		}

		#endregion
	}
}
