using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceValidationTest : InvoiceBaseValidationTest
	{
		public void TestCheckReceiptPaymentAK_AB()
		{
			string warningSamePrinterMessage = AccChequeBook.WarningChequeBookWithSamePrinterMessageStart + "'Book1'" + AccChequeBook.WarningSamePrinterMessageEnd;

			BusinessObject printer = (BusinessObject)Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = true;
			newInvoice.AH_InvoiceDate = ZDateTime.Now;
			newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			newInvoice.AH_AB = book2.AK_AB;
			newInvoice.ReceiptPaymentAK_AB = book2.PK;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAK_AB();
			Assert("Should have warning about another Cheque Book with the same printer", newInvoice.ReceiptPaymentAK_ABInfo.HasWarning(warningSamePrinterMessage));

			APInvoice invoiceWithEmptyCheckBook = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			invoiceWithEmptyCheckBook.SubmittedFromInvoicingForm = true;
			invoiceWithEmptyCheckBook.IsInvoiceReceiptPayment = true;
			invoiceWithEmptyCheckBook.AH_InvoiceDate = ZDateTime.Now;
			invoiceWithEmptyCheckBook.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

			invoiceWithEmptyCheckBook.ReceiptPaymentAK_AB = ZGuid.Empty;
			((InvoiceValidation)invoiceWithEmptyCheckBook.Validation).ValidateReceiptPaymentAK_AB();
			AssertHasError(invoiceWithEmptyCheckBook.ReceiptPaymentAK_ABInfo, "Please enter a Check Book.");

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			invoiceWithEmptyCheckBook.ReceiptPaymentAK_AB = testChequeBook.PK;
			((InvoiceValidation)invoiceWithEmptyCheckBook.Validation).ValidateReceiptPaymentAK_AB();
			AssertNoError(invoiceWithEmptyCheckBook.ReceiptPaymentAK_ABInfo, "Please enter a Check Book.");
		}

		[TestDate(2013, 10, 21)]
		public void TestCheckReceiptPaymentAH_PostDate()
		{
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			var newInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1) as Invoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = true;
			newInvoice.AH_InvoiceDate = ZDateTime.Now;
			newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			newInvoice.ReceiptPaymentAH_PostDate = ZDateTime.Empty;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAH_PostDate();
			Assert("Should have error about missing post date", newInvoice.ReceiptPaymentAH_PostDateInfo.HasError("Please enter a value."));
			newInvoice.ReceiptPaymentAH_PostDate = ZDateTime.Invalid;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAH_PostDate();
			Assert("Should have error about invalid post date", newInvoice.ReceiptPaymentAH_PostDateInfo.HasError("Please enter a valid Receipt Payment Post Date."));

			AssertReceiptPaymentAH_PostDateMessage(0, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, newInvoice, "The post date cannot be in the past", true);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			AssertReceiptPaymentAH_PostDateMessage(0, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, newInvoice, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;

			AssertReceiptPaymentAH_PostDateMessage(2, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(0, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, newInvoice, "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing", false);

			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.CashBookPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AssertReceiptPaymentAH_PostDateMessage(2, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(0, newInvoice, "", false);
			AssertReceiptPaymentAH_PostDateMessage(-2, newInvoice, "The post date cannot be in the past", true);
		}

		void AssertReceiptPaymentAH_PostDateMessage(int daysFromToday, Invoice invoice, string message, bool isError)
		{
			invoice.ReceiptPaymentAH_PostDate = ZDateTime.Now.AddDays(daysFromToday);
			((InvoiceValidation)invoice.Validation).ValidateReceiptPaymentAH_PostDate();
			var assertionMsg = string.Format("Should have {0} about post date is in {1}",
				string.IsNullOrEmpty(message) ? "no problem" : (isError ? "error" : "warning"),
				daysFromToday < 0 ? "past" : (daysFromToday == 0 ? "present" : "future"));
			bool result;
			if (!string.IsNullOrEmpty(message))
			{
				result = isError ? invoice.ReceiptPaymentAH_PostDateInfo.HasError(message) : invoice.ReceiptPaymentAH_PostDateInfo.HasWarning(message);
			}
			else
			{
				result = !invoice.ReceiptPaymentAH_PostDateInfo.HasErrors() && !invoice.ReceiptPaymentAH_PostDateInfo.HasWarnings();
			}
			Assert(assertionMsg, result);
		}

		[TestDate(2012, 10, 01)]
		public void TestCheckReceiptPaymentAH_PostDate_ValidateAll()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.AH_PostDate = ZDateTime.Now.AddDays(2);
			newInvoice.IsInvoiceReceiptPayment = true;
			newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			((InvoiceValidation)newInvoice.Validation).ValidateAll();

			Assert("Should have error about post date is in future", newInvoice.ReceiptPaymentAH_PostDateInfo.HasError(AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled));
		}

		[TestDate(2015, 03, 09)]
		public void TestCheckAH_InvoiceDate()
		{
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.AH_InvoiceDate = ZDate.Today.AddDays(1);
			AssertHasError(newInvoice.AH_InvoiceDateInfo, "Invoice date cannot be in the future.\r\nThis is determined by registry: Accounting -> Payable Defaults -> Default Settings -> Allow Forward Dating of AP Invoice Date");

			ARInvoice newARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			newARInvoice.AH_InvoiceDate = ZDate.Today.AddDays(1);
			AssertNoErrors(newARInvoice.AH_InvoiceDateInfo);
		}

		public void TestCheckReceiptPaymentAH_InvoiceDate()
		{
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = true;
			newInvoice.AH_InvoiceDate = ZDateTime.Now;
			newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			newInvoice.ReceiptPaymentAH_InvoiceDate = ZDateTime.Empty;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAH_InvoiceDate();
			Assert("Should have error about missing invoice date", newInvoice.ReceiptPaymentAH_InvoiceDateInfo.HasError("Please enter a Receipt/Payment Invoice Date."));
			newInvoice.ReceiptPaymentAH_InvoiceDate = ZDateTime.Invalid;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAH_InvoiceDate();
			Assert("Should have error about invalid invoice date", newInvoice.ReceiptPaymentAH_InvoiceDateInfo.HasError("Please enter a valid Receipt Payment Invoice Date."));

			AccountingPeriodTestHelper periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.SetupPeriods();
			newInvoice.ReceiptPaymentAH_InvoiceDate = periodManagementTestHelper.PreviousGLClosedPeriod.AM_EndDate.AddDays(-1).Date;
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentAH_InvoiceDate();
			Assert("Should not have any errors now", !newInvoice.ReceiptPaymentAH_InvoiceDateInfo.HasErrors());

			newInvoice.ReceiptPaymentAH_InvoiceDate = periodManagementTestHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
			Assert("Should not have error about closed subledger Date", !newInvoice.ReceiptPaymentAH_InvoiceDateInfo.HasErrors());
		}

		public void TestCheckAH_DueDate()
		{
			APInvoice newInvoice = Factory.NewWithValidTestData<APInvoice>();

			newInvoice.AH_InvoiceDate = ZDateTime.Now;
			newInvoice.AH_DueDate = ZDateTime.Now.AddDays(-1);

			InvoiceValidation testValidation = new APInvoiceValidation(newInvoice);
			testValidation.ValidateAH_DueDate();
			Assert(newInvoice.AH_DueDateInfo.HasError("Due date should be after or equal to Invoice Date"));

			newInvoice.AH_DueDate = ZDateTime.Now.AddDays(3);
			testValidation.ValidateAH_DueDate();
			Assert(!newInvoice.AH_DueDateInfo.HasError("Due date should be after or equal to Invoice Date"));
		}

		public void TestCheckAH_InvoiceTerm()
		{
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			InvoiceValidation testValidation = new APInvoiceValidation(newInvoice);

			testValidation.CheckAH_InvoiceTerm_ForTestOnly();
			Assert(!newInvoice.AH_InvoiceTermInfo.HasErrors());

			ARInvoice newARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testValidation = new InvoiceValidation(newInvoice);
			newARInvoice.AH_InvoiceTerm = ZString.Empty;
			testValidation.ValidateAH_InvoiceTerm();

			Assert(newARInvoice.AH_InvoiceTermInfo.HasErrors());

			newARInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			testValidation.ValidateAH_InvoiceTerm();

			Assert(!newARInvoice.AH_InvoiceTermInfo.HasErrors());

			newARInvoice.AH_OH = ZGuid.Empty;
			newARInvoice.AH_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			AssertHasError(newARInvoice.AH_InvoiceTermInfo, TermsAndDueDateCalculationProvider.MonthsFromInvoiceCycleDateError);
		}

		public void TestCheckAH_InvoiceTermDays()
		{
			bool oldAllowModifyInvoiceTerm = AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				APInvoice newInvoice = Factory.NewWithValidTestData<APInvoice>();
				InvoiceValidation testValidation = new APInvoiceValidation(newInvoice);

				testValidation.CheckAH_InvoiceTermDays_ForTestOnly();
				Assert(!newInvoice.AH_InvoiceTermDaysInfo.HasErrors());

				ARInvoice newARInvoice = Factory.NewWithValidTestData<ARInvoice>();
				testValidation = new InvoiceValidation(newInvoice);

				newARInvoice.AH_InvoiceTermDays = (ZByte)8;
				testValidation.ValidateAH_InvoiceTermDays();
				Assert(!newARInvoice.AH_InvoiceTermDaysInfo.HasErrors());

				newARInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.CashOnDelivery;
				newARInvoice.AH_InvoiceTermDays = 1;
				AssertHasError("Expected an error", newARInvoice.AH_InvoiceTermDaysInfo, "An Invoice Term Days should be 0 for this Invoice Term");

				newARInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.PaymentInAdvance;
				newARInvoice.AH_InvoiceTermDays = 1;
				AssertHasError("Expected an error", newARInvoice.AH_InvoiceTermDaysInfo, "An Invoice Term Days should be 0 for this Invoice Term");

				foreach (CodeDescriptionPair invoiceTerm in newARInvoice.InvoiceTerms_List)
				{
					if (Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate == invoiceTerm.Code)
					{
						continue; //Invoice term DLP requires Invoice.Job and tested in Test_DLP_InvoiceTerm_Days
					}
					newARInvoice.AH_InvoiceTerm = invoiceTerm.Code;
					newARInvoice.AH_InvoiceTermDays = ZByte.Zero;
					AssertNoErrors("Expected no errors", newARInvoice.AH_InvoiceTermDaysInfo);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldAllowModifyInvoiceTerm);
			}
		}

		public void Test_DLP_InvoiceTerm_Days()
		{
			bool oldAllowModifyInvoiceTerm = AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			try
			{
				var creator = new TestObjectCreator(Factory);

				var shipment = creator.CreateShipment("S0001");
				var job = creator.CreateJob(shipment);
				var charge = creator.CreateCharge(job, creator.FRT, "Desc", creator.AUD, 111.11m, creator.Creditor1, creator.AUD, 1000.11m, creator.Debtor);

				Factory.Save();

				var apInvoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1m) as APInvoice;
				apInvoice.AH_JH = job.PK;
				apInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;

				AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				InvoiceValidation testValidation = new APInvoiceValidation(apInvoice);
				testValidation.CheckAH_InvoiceTermDays_ForTestOnly();

				Assert(!apInvoice.AH_InvoiceTermDaysInfo.HasErrors());

				var arInvoice = creator.CreateInvoice(typeof(ARInvoice), creator.AUD, 1m) as ARInvoice;
				arInvoice.AH_JH = job.PK;
				arInvoice.AH_InvoiceTerm = Core.Constants.InvoiceTerms.FromDeliveryOrPickupDate;

				testValidation = new InvoiceValidation(arInvoice);
				testValidation.ValidateAH_InvoiceTermDays();

				arInvoice.AH_InvoiceTermDays = (ZByte)8;				
				Assert(!arInvoice.AH_InvoiceTermDaysInfo.HasErrors());

				arInvoice.AH_InvoiceTermDays = (ZByte)101;
				AssertHasError("Expected an error", arInvoice.AH_InvoiceTermDaysInfo, "Days must be between 0 and 99.");

				arInvoice.AH_InvoiceTermDays = ZByte.Zero;
				AssertNoErrors("Expected no errors", arInvoice.AH_InvoiceTermDaysInfo);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AllowModifyInvoiceTerm.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldAllowModifyInvoiceTerm);
			}
		}

		public void TestCheckReceiptPaymentBankAccount()
		{
			TestObjectCreator testDataCreator = new TestObjectCreator(Factory);
			ZGuid aUDBankAccount = testDataCreator.AUDBankAccount.PK;
			ZGuid uSDBankAccount = testDataCreator.USDBankAccount.PK;

			Invoice testInvoice = (Invoice)Factory.NewWithValidTestData(typeof(APInvoice));
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.IsInvoiceReceiptPayment = true;
			testInvoice.ReceiptPaymentAH_AB = uSDBankAccount;

			InvoiceValidation invoiceValidation = (InvoiceValidation)testInvoice.Validation;

			invoiceValidation.ValidateReceiptPaymentAH_AB();
			Assert("Should be an error because currency is AUD", testInvoice.ReceiptPaymentAH_ABInfo.HasErrors());
			Assert("Validation Error - message should be 'Bank Account currency must be currency of the invoice or local currency.'", testInvoice.ReceiptPaymentAH_ABInfo.HasError("Bank Account must be for the current company." + System.Environment.NewLine +
				"Bank Account currency must be currency of the invoice or local currency."));

			testInvoice.AH_RX_NKTransactionCurrency = testDataCreator.USD.RX_Code;
			invoiceValidation.ValidateReceiptPaymentAH_AB();
			Assert("Should no longer be an error because currencies match", !testInvoice.ReceiptPaymentAH_ABInfo.HasErrors());

			testInvoice.ReceiptPaymentAH_AB = aUDBankAccount;
			Assert("Should no longer be an error because currencies match", !testInvoice.ReceiptPaymentAH_ABInfo.HasErrors());

			testInvoice.AH_RX_NKTransactionCurrency = testDataCreator.AUD.RX_Code;
			testInvoice.ReceiptPaymentAH_AB = uSDBankAccount;
			invoiceValidation.ValidateReceiptPaymentAH_AB();
			Assert("Should be an error because currency is AUD", testInvoice.ReceiptPaymentAH_ABInfo.HasErrors());
			Assert("Validation Error - message should be 'Bank Account currency must be currency of the invoice or local currency.'", testInvoice.ReceiptPaymentAH_ABInfo.HasError("Bank Account must be for the current company." + System.Environment.NewLine +
				"Bank Account currency must be currency of the invoice or local currency."));
		}

		public virtual void TestValidationOnReceiptPaymentAH_ABWhenBankAccountBranchNotEqualInvoiceHeaderBranch()
		{
			var testBranch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var testBranch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_GB = testBranch1.PK;
			bankAccount2.AB_GB = testBranch2.PK;

			foreach (Type invoiceType in new[] { typeof(ARInvoice), typeof(APInvoice) })
			{
				var invoice = (Invoice)TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
				invoice.SubmittedFromInvoicingForm = true;
				invoice.IsInvoiceReceiptPayment = true;

				invoice.AH_GB = testBranch1.PK;
				invoice.ReceiptPaymentAH_AB = bankAccount2.PK;
				AssertNotNull("Precondition:", invoice.ReceiptPaymentBankAccount);
				AssertNotEquals("Precondition: ", invoice.ReceiptPaymentBankAccount.AB_GB, invoice.AH_GB);
				AssertHasError(invoice.ReceiptPaymentAH_ABInfo, "You cannot select a bank account that is different to the invoice branch (AAA)");

				invoice.AH_GB = testBranch2.PK;
				invoice.ReceiptPaymentAH_AB = bankAccount1.PK;
				AssertNotNull("Precondition:", invoice.ReceiptPaymentBankAccount);
				AssertNotEquals("Precondition: ", invoice.ReceiptPaymentBankAccount.AB_GB, invoice.AH_GB);
				AssertHasError(invoice.ReceiptPaymentAH_ABInfo, "You cannot select a bank account that is different to the invoice branch (BBB)");

				invoice.ReceiptPaymentAH_AB = ZGuid.Empty;
				AssertNull(invoice.ReceiptPaymentBankAccount);
				AssertNoError(invoice.ReceiptPaymentAH_ABInfo, "You cannot select a bank account that is different to the invoice branch (BBB)");

				invoice.ReceiptPaymentAH_AB = bankAccount2.PK;
				AssertEquals(invoice.ReceiptPaymentBankAccount.AB_GB, invoice.AH_GB);
				AssertNoErrors(invoice.ReceiptPaymentAH_ABInfo);

				invoice.AH_GB = testBranch1.PK;
				invoice.ReceiptPaymentAH_AB = bankAccount1.PK;
				AssertEquals(invoice.ReceiptPaymentBankAccount.AB_GB, invoice.AH_GB);
				AssertNoErrors(invoice.ReceiptPaymentAH_ABInfo);
			}
		}

		public virtual void TestValidationOnReceiptPaymentAH_AB_NoErrorWhenBankAccountBranchIsNull()
		{
			var testBranch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var testBranch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			var bankAccount1 = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount1.AB_GB = ZGuid.Empty;
			bankAccount2.AB_GB = testBranch2.PK;

			foreach (Type invoiceType in new[] { typeof(ARInvoice), typeof(APInvoice) })
			{
				var invoice = (Invoice)TestObjectCreator.CreateInvoice(invoiceType, TestObjectCreator.AUD, 1m);
				invoice.SubmittedFromInvoicingForm = true;
				invoice.IsInvoiceReceiptPayment = true;

				invoice.AH_GB = testBranch1.PK;
				invoice.ReceiptPaymentAH_AB = bankAccount2.PK;
				AssertNotNull("Precondition:", invoice.ReceiptPaymentBankAccount);
				AssertNotNull("Precondition:", invoice.ReceiptPaymentBankAccount.Branch);
				AssertNotEquals("Precondition: ", invoice.ReceiptPaymentBankAccount.AB_GB, invoice.AH_GB);
				AssertHasError(invoice.ReceiptPaymentAH_ABInfo, "You cannot select a bank account that is different to the invoice branch (AAA)");

				invoice.ReceiptPaymentAH_AB = bankAccount1.PK;
				AssertNotNull("Precondition:", invoice.ReceiptPaymentBankAccount);
				AssertNull("Precondition:", invoice.ReceiptPaymentBankAccount.Branch);
				AssertNoError(invoice.ReceiptPaymentAH_ABInfo, "You cannot select a bank account that is different to the invoice branch (AAA)");
			}
		}

		public void TestCheckReceiptType()
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.SubmittedFromInvoicingForm = true;
			aPInv.IsInvoiceReceiptPayment = true;

			aPInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			((InvoiceValidation)aPInv.Validation).ValidateReceiptPaymentAH_ReceiptType();
			Assert("There should not be any errors", !aPInv.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors());

			aPInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			((InvoiceValidation)aPInv.Validation).ValidateReceiptPaymentAH_ReceiptType();
			Assert("There should be an error", aPInv.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors());

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.SubmittedFromInvoicingForm = true;
			aRInv.IsInvoiceReceiptPayment = true;

			aRInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			((InvoiceValidation)aRInv.Validation).ValidateReceiptPaymentAH_ReceiptType();
			Assert("There should not be any errors", !aRInv.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors());

			aRInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			((InvoiceValidation)aRInv.Validation).ValidateReceiptPaymentAH_ReceiptType();
			Assert("There should be an error", aRInv.ReceiptPaymentAH_ReceiptTypeInfo.HasErrors());
		}

		public void TestCheckReceiptType_Security()
		{
			#region AR Invoice

			ARInvoice aRInv = (ARInvoice)TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			aRInv.SubmittedFromInvoicingForm = true;
			aRInv.IsInvoiceReceiptPayment = true;

			Env.Security.NewReceivablesPaymentCheque.IsAllowed = true;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertNoErrors(aRInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewReceivablesPaymentCash.IsAllowed = true;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors(aRInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = true;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CreditCard;
			AssertNoErrors(aRInv.ReceiptPaymentAH_ReceiptTypeInfo);

			Env.Security.NewReceivablesPaymentCheque.IsAllowed = false;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertHasError(aRInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewReceivablesPaymentCash.IsAllowed = false;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertHasError(aRInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewReceivablesPaymentCreditCard.IsAllowed = false;
			aRInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CreditCard;
			AssertHasError(aRInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

			#endregion

			#region AP Invoice

			APInvoice aPInv = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			aPInv.SubmittedFromInvoicingForm = true;
			aPInv.IsInvoiceReceiptPayment = true;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CreditCard;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.DirectDebit;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.EFT;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentSFT.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.ScheduledEFT;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = true;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CollectionRequest;
			AssertNoErrors(aPInv.ReceiptPaymentAH_ReceiptTypeInfo);

			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CreditCard;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.DirectDebit;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.EFT;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentSFT.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.ScheduledEFT;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");
			Env.Security.NewPayablesPaymentCRQ.IsAllowed = false;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.CollectionRequest;
			AssertHasError(aPInv.ReceiptPaymentAH_ReceiptTypeInfo, "You do not have appropriate security rights to select this payment type.");

			#endregion
		}

		public void TestAccountDetailsNotFound()
		{
			var testBank = TestObjectCreator.AUDBankAccount;
			testBank.AB_AllowAutoDDR = true;

			var orgWithAccountDetails = TestObjectCreator.Creditor1;
			AccountDetailsDependentCollection collection = new AccountDetailsDependentCollection(orgWithAccountDetails.CompanyDataCollection[0], Factory);
			AccAPAccountDetails accDetail = collection.AddNew();
			accDetail.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accDetail.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accDetail.A1_IsDefaultAccount = true;

			APInvoice aPInv = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			aPInv.SubmittedFromInvoicingForm = true;
			aPInv.IsInvoiceReceiptPayment = true;
			aPInv.ReceiptPaymentAH_AB = testBank.PK;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.DirectDebit;

			AssertNull(((InvoiceValidation)aPInv.Validation).AccountDetails_ForTestOnly);
			AssertHasError(aPInv.AH_OHInfo, @"An AP Bank Account could not be found with currency AUD and payment type DDR for the payee AALSHI.

Please set up an AP Account for the organization AALSHI under the AP Details tab, by right-clicking this grid and selecting ""Edit Payment Organization Detail"", with the currency AUD and payment type of DDR.");

			aPInv.AH_OH = orgWithAccountDetails.PK;
			aPInv.IsInvoiceReceiptPayment = true;
			aPInv.ReceiptPaymentAH_AB = testBank.PK;
			aPInv.ReceiptPaymentAH_ReceiptType = ReceiptTypes.DirectDebit;
			((InvoiceValidation)aPInv.Validation).ValidateReceiptPaymentAH_ReceiptType();
			AssertNotNull(((InvoiceValidation)aPInv.Validation).AccountDetails_ForTestOnly);
			AssertNoErrors(aPInv.AH_OHInfo);
		}

		public void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(1, 3, 2);
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = ZBool.True;
			newInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;

			newInvoice.ReceiptPaymentAH_AB = testChequeBook.AK_AB;
			newInvoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			newInvoice.ReceiptPaymentAH_ChequeOrReference = "BLAH!";
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentCommon_ForTestOnly();
			AssertHasError("Check Number should have an error", newInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, "Only numbers are allowed in this field.");

			newInvoice.ReceiptPaymentAH_AB = autoPrintChequeBook.AK_AB;
			newInvoice.ReceiptPaymentAK_AB = autoPrintChequeBook.PK;
			newInvoice.ReceiptPaymentAH_ChequeOrReference = "BLAH!";
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentCommon_ForTestOnly();
			AssertNoErrors("Should not have any errors as now in autoallocation mode", newInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo);
		}

		public void TestChequeOrReferenceValidationIsNumbersOrLettersForAutoAllocationMode()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = ZBool.True;
			newInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;

			newInvoice.ReceiptPaymentAH_AB = bankAccount.PK;
			newInvoice.ReceiptPaymentAH_ChequeOrReference = "BLAH!";
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentCommon_ForTestOnly();
			AssertHasError("Check Number should have an error", newInvoice.ReceiptPaymentAH_ChequeOrReferenceInfo, "Only numbers or letters are allowed in this field.");
		}

		public void TestReceiptPaymentAH_ChequeOrReferenceMustBeEntered()
		{
			var invoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			invoice.AH_InvoiceDate = ZDateTime.Empty;
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = ZBool.True;

			foreach (CodeDescriptionPair paymentMethod in invoice.PaymentMethods)
			{
				invoice.ReceiptPaymentAH_ReceiptType = paymentMethod.Code;
				((InvoiceValidation)invoice.Validation).ValidateReceiptPaymentCommon_ForTestOnly();
				AssertHasError("ReceiptPaymentAH_InvoiceDate should have an error", invoice.ReceiptPaymentAH_InvoiceDateInfo, "Please enter a Receipt/Payment Invoice Date.");
			}
		}

		public void TestReceiptPaymentAH_InvoiceDateIsPartOfCommonValidate()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.AH_InvoiceDate = ZDateTime.Empty;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = ZBool.True;
			newInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;

			newInvoice.ReceiptPaymentAH_AB = testChequeBook.AK_AB;
			newInvoice.ReceiptPaymentAK_AB = testChequeBook.PK;
			newInvoice.ReceiptPaymentAH_ChequeOrReference = "000003";
			((InvoiceValidation)newInvoice.Validation).ValidateReceiptPaymentCommon_ForTestOnly();
			AssertHasError("ReceiptPaymentAH_InvoiceDate should have an error", newInvoice.ReceiptPaymentAH_InvoiceDateInfo, "Please enter a Receipt/Payment Invoice Date.");
		}

		public void TestAV_AKHasAutoAllocationValidation()
		{
			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.SubmittedFromInvoicingForm = true;
			newInvoice.IsInvoiceReceiptPayment = ZBool.True;
			newInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AccChequeBook newChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			newInvoice.ReceiptPaymentAH_AB = newChequeBook.AK_AB;
			newInvoice.AH_GB = newChequeBook.AK_GB;
			newInvoice.ReceiptPaymentAK_AB = newChequeBook.PK;
			((InvoiceValidation)newInvoice.Validation).ValidatePayment_ForTestOnly();
			Assert("Should be no errors so far", !newInvoice.ReceiptPaymentAK_ABInfo.HasErrors());

			newChequeBook.AK_IsActive = ZBool.False;
			newInvoice.ReceiptPaymentAK_AB = newChequeBook.PK;
			((InvoiceValidation)newInvoice.Validation).ValidatePayment_ForTestOnly();
			AssertHasError(newInvoice.ReceiptPaymentAK_ABInfo, AccChequeBookAutoAllocationValidation.ChequeBookIsInActiveMessage);
		}

		public void TestCheckReceiptPaymentAH_PostDateNotInFuture()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			foreach (Type type in new[] { typeof(APInvoice), typeof(ARInvoice) })
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(type, "0001", TestObjectCreator.AUD, 1, 100, 0, 100, 0) as Invoice;
				invoice.SubmittedFromInvoicingForm = true;
				invoice.IsInvoiceReceiptPayment = true;
				invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;

				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				invoice.ReceiptPaymentAH_PostDate = ZDateTime.Now.AddDays(1);
				AssertHasError("ReceiptPaymentAH_PostDate", invoice.ReceiptPaymentAH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

				invoice.ReceiptPaymentAH_PostDate = ZDateTime.Now;
				AssertNoErrors("ReceiptPaymentAH_PostDate", invoice.ReceiptPaymentAH_PostDateInfo);

				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				TestObjectCreator.ResetSecurityCore();
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

				invoice.ReceiptPaymentAH_PostDate = ZDateTime.Now.AddDays(2);
				AssertHasError("ReceiptPaymentAH_PostDate", invoice.ReceiptPaymentAH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				invoice.ReceiptPaymentAH_PostDate = ZDateTime.Now.AddDays(3);
				AssertNoErrors("ReceiptPaymentAH_PostDate", invoice.ReceiptPaymentAH_PostDateInfo);
			}
		}

		public void TestReceiptPaymentAH_ReceiptTypeWithCashAccount()
		{
			var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var invoice = (Invoice)Factory.New(InvoiceType);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.IsInvoiceReceiptPayment = true;
			invoice.ReceiptPaymentAH_AB = bank.PK;

			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			AssertHasError("Cash receipt type is not valid for other than CSH accounts", invoice.ReceiptPaymentAH_ReceiptTypeInfo, "For Cash Account, please select CSH - Cash Receipt Type.");

			invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors("Setting receipt type to cash is valid for CSH accounts", invoice.ReceiptPaymentAH_ReceiptTypeInfo);
		}

		protected AccChequeBook GetAutoPrintChequeBook(ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			return chequeBook;
		}
	}
}
