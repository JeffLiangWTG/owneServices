using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class CustomsPaymentCreatorTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestErrorMessageWhenThereAreNoDataProviders()
		{
			CreatePayment("QWERTYUIOP");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreatePaymentThrowsArgumentExceptionWithInvalidArguments()
		{
			CustomsPaymentCreator paymentCreator = new CustomsPaymentCreator();
			paymentCreator.CreatePayment(null, null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestArgumentExceptionIsThrownIfDataProvidersAreFromDifferentCompanies()
		{
			CustomsJob2.fBranch = ObjCreator.NonCurrentCompanyBranch;
			CreatePayment("ABCD1234", ChargesProvider1, ChargesProvider2);
		}

		public void TestNoExceptionWhenCreditorIsNullButSendEmail()
		{
			var paymentDataProvider = new CustomsPaymentDataProvider();
			paymentDataProvider.EmailGroupPK = EmailRecipientGuid;
			paymentDataProvider.SendEmail = true;
			paymentDataProvider.BankAccountPK = ObjCreator.AUDBankAccount.PK;
			paymentDataProvider.Creditor = null;
			paymentDataProvider.Company = GlbCompany.CurrentCompany;

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertNoExceptionThrown(delegate
			{ CreatePayment(paymentDataProvider, "123", ChargesProvider1, ChargesProvider2); });

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void CreateAndPostCharge(Job job, AccChargeCode code, string transactionNo, string description, ZGuid companyPK, ZGuid branchPK, OrgHeader creditor, ZDateTime postDate)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = code.PK;
			charge.JR_Desc = description;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = ObjCreator.AUD.RX_Code;
			charge.JR_OSCostAmt = 16.0m;

			charge.JR_OH_SellAccount = ObjCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = ObjCreator.AUD.RX_Code;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge.JR_OSSellAmt = 16.0m;
			charge.JR_GE = ObjCreator.FESDepartment.PK;
			charge.JR_GB = branchPK;
			charge.ChargeCode.AC_GC = companyPK;

			var apInvoice = ObjCreator.CreateAPInvoice<APInvoice>(transactionNo, ObjCreator.AUD, 1.0m, 16m, 0m, 0m, 16m, 0m, 0m, creditor);
			apInvoice.AH_PostDate = postDate;
			var apline = ObjCreator.CreateAPInvoiceLine(apInvoice, job, ObjCreator.CC1, ObjCreator.AUD, 1.0m, description, 16.0m);

			apline.AL_AT = charge.JR_AT_CostGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_CostVATClass = ZGuid.Empty;
			charge.JR_AL_APLine = apline.PK;

			var arInvoice = ObjCreator.CreateARInvoice<ARInvoice>(transactionNo + "_AR", ObjCreator.AUD, 1.0m, ObjCreator.Debtor);
			var arline = ObjCreator.CreateARInvoiceLine(arInvoice, job, ObjCreator.CC1, ObjCreator.AUD, 1.0m, description, 16.0m);

			arline.AL_AT = charge.JR_AT_SellGSTRate = ZGuid.Empty;
			apline.AL_A9_VATClass = charge.JR_A9_SellVATClass = ZGuid.Empty;
			charge.JR_AL_ARLine = arline.PK;
		}

		[TestDate(2021, 05, 05, 0, 0, 0)]
		public void TestUpdateInvoiceDateFullyPaidDateAndPostingDateWithPaymentDate()
		{
			var dateToday = ZDateTime.Now;
			var job = ObjCreator.CreateJob("D0011", ObjCreator.Creditor1, 1.0M, ObjCreator.Agent, 1.0M);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_A_JOP = Env.Time.CurrentLocalDate.AddDays(-15);

			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = Env.CurrentCompany.PK;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.JH_JobNum = "B66666";
			CreateAndPostCharge(job, chargeCode1, "ABCD1234", "DESC002", Env.CurrentCompany.PK, Env.CurrentBranch.PK, ObjCreator.Creditor1, dateToday.AddDays(-2));

			var paymentDataProvider = new CustomsPaymentDataProvider();
			paymentDataProvider.EmailGroupPK = EmailRecipientGuid;
			paymentDataProvider.SendEmail = true;
			paymentDataProvider.BankAccountPK = ObjCreator.AUDBankAccount.PK;
			paymentDataProvider.Creditor = ObjCreator.Creditor1;
			paymentDataProvider.Company = GlbCompany.CurrentCompany;
			paymentDataProvider.PaymentDate = dateToday.AddDays(-1);
			Factory.Save();

			var apInvoice1 = FindPayment(ObjCreator.Creditor1, transactionType: TransactionTypes.Invoice);
			AssertEquals("Posting Date", dateToday.AddDays(-2), apInvoice1.AH_PostDate);

			var cusCharge1 = new CustomsCharge(null, "CusChg1", 32.0m, 0m, true, ObjCreator.Creditor1.PK);
			var chargeProvider1 = CreateMockCustomsChargesProvider(CustomsJob1, "ABCD1234", CreateMockCustomCharge(cusCharge1));
			CreatePayment(paymentDataProvider, "ABCD1234", chargeProvider1);
			var originalPayment = FindPayment(ObjCreator.Creditor1, transactionType: TransactionTypes.Payment, jobNumber: "ABCD1234");
			AssertEquals("Payment Fully Paid Date", dateToday, originalPayment.AH_FullyPaidDate);
			AssertEquals("Payment Posting Date", dateToday, originalPayment.AH_PostDate);
			AssertEquals("Payment Invoice Date", dateToday.AddDays(-1), originalPayment.AH_InvoiceDate);

			apInvoice1 = FindPayment(ObjCreator.Creditor1, transactionType: TransactionTypes.Invoice);
			AssertEquals("Invoice1 Fully Paid Date", dateToday, apInvoice1.AH_FullyPaidDate);
		}

		public void TestInWrongCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~AA";
			company.GC_RN_NKCountryCode = "AU";

			var branch = company.Branches.AddNew();
			Factory.Save();

			var paymentDataProvider = new CustomsPaymentDataProvider();

			paymentDataProvider.EmailGroupPK = EmailRecipientGuid;
			paymentDataProvider.SendEmail = true;
			paymentDataProvider.BankAccountPK = ObjCreator.AUDBankAccount.PK;
			paymentDataProvider.Creditor = null;
			paymentDataProvider.Company = company;

			try
			{
				CreatePayment(paymentDataProvider, "123", ChargesProvider1, ChargesProvider2);
				Fail("An exception is expected");
			}
			catch (CustomsInvoiceRaiseException e)
			{
				AssertContains("Auto-Billing is attempted in the company ", e.Message);
			}
		}

		public void TestCannotMakeAPaymentWhenCustomsJobDoesNotExist()
		{
			ChargesProvider1.CustomsJob = null;

			ICustomsPaymentCreationResult result = CreatePayment("ABCD1234", ChargesProvider1, ChargesProvider2);

			AssertContains("No customs declaration job exists in the system for " + ChargesProvider1.UniqueNumber, result.ErrorMessage);
		}

		[ExpectNoExceptions]
		public void TestArgumentExceptionIsNotThrownIfDataProvidersAreFromSameCompany()
		{
			CreatePayment("ABCD1234", ChargesProvider1, ChargesProvider2);
		}

		public void TestExistingPaymentDoesntLookAtReversedTransactions()
		{
			APInvoice invoice = CreateAPInvoice(ChargesProvider1, 110m);
			var result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Original Payment should be created", true, result.WasSuccessful);
			APPayment originalPayment = FindPayment(Creditor, jobNumber: "ABCD1234");

			result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Payment shouldn't be created (original payment isn't reversed)", false, result.WasSuccessful);

			originalPayment.AH_IsCancelled = true;
			((IMatching)originalPayment).Matchlinks.AddNew().AP_AH = originalPayment.PK;
			invoice.AH_OutstandingAmount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			((IMatching)invoice).CurrentMatchGroup.RemoveAndDeleteAll();
			((IMatching)originalPayment).CurrentMatchGroup.RemoveAndDeleteAll();

			result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Error Message (2nd Payment after reversing 1st Payment)", "", result.ErrorMessage);
			AssertEquals("Was Successful (2nd Payment after reversing 1st Payment)", true, result.WasSuccessful);

			APPayment secondPayment = FindPayment(Creditor, "ABCD1234");
			AssertNotEquals("Second payment should be different from first", originalPayment.PK, secondPayment.PK);
		}

		public void TestFindAPInvoiceWhenThereIsAReversedOne()
		{
			APInvoice reversedInvoice = CreateAPInvoice(ChargesProvider1, 110m);
			reversedInvoice.AH_IsCancelled = true;

			TransactionMatchLink matchLink = ((IMatching)reversedInvoice).CurrentMatchGroup.AddNew(); // to pass IsCancelled check
			matchLink.AP_AH = reversedInvoice.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLink);
			Factory.Save();

			APInvoice invoice = CreateAPInvoice(ChargesProvider1, 110m);
			invoice.AH_TransactionNum = ChargesProvider1.UniqueNumber + "/1";

			var result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("", result.ErrorMessage);
			AssertEquals("It should locate active AP Invoice", true, result.WasSuccessful);
			Assert("should have been fully paid", !invoice.AH_FullyPaidDate.IsEmpty);
			Assert("should NOT have paid for the reversed invoice", reversedInvoice.AH_FullyPaidDate.IsEmpty);
		}

		public void TestSuccessfulPaymentIsCreatedOnceAndIsNotDuplicated()
		{
			APInvoice invoice = CreateAPInvoice(ChargesProvider1, 110m);
			//Factory.Save();

			ICustomsPaymentCreationResult result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("ErrorMessage", "", result.ErrorMessage);
			AssertEquals("WasSuccessful", true, result.WasSuccessful);

			APPayment payment = FindPayment(Creditor, jobNumber: "ABCD1234");
			AssertNotNull("Payment", payment);
			AssertEquals("Creditor", Creditor.OH_Code, payment.Header.OH_Code);
			AssertEquals("Payment OSExTaxAmount", 110m, payment.AH_OSExTaxAmount);
			AssertEquals("Payment OSTaxAmount", 0m, payment.AH_OSTaxAmount);
			AssertEquals("Payment OSTotalAmount", 110m, payment.AH_OSTotalAmount);

			AssertTransactionIsFullyPaid(payment);
			AssertTransactionIsFullyPaid(invoice);

			result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertNotEquals("ErrorMessage", "", result.ErrorMessage);
			AssertEquals("WasSuccessful", false, result.WasSuccessful);
		}

		public void TestPaymentBranchAndDepartmentCopiedFromTheFirstAPInvoice()
		{
			var branch = GlbCompany.CurrentCompany.Branches.AddNew();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var invoice = CreateAPInvoice(ChargesProvider1, 110m);
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = department.PK;

			ICustomsPaymentCreationResult result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("ErrorMessage", "", result.ErrorMessage);
			AssertEquals("WasSuccessful", true, result.WasSuccessful);

			APPayment payment = FindPayment(Creditor, jobNumber: "ABCD1234");
			AssertNotNull("Payment", payment);

			AssertEquals("Branch", branch.PK, payment.AH_GB);
			AssertEquals("Department", department.PK, payment.AH_GE);
		}

		public void TestCreatePaymentWherePaymentAlreadyExistsInTheSameCompany()
		{
			APInvoice invoice = CreateAPInvoice(ChargesProvider1, 110m);
			ICustomsPaymentCreationResult result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);

			CustomsCharge charge4 = new CustomsCharge(null, "TEST4", 220.0m, 0m, true, Creditor.PK);
			CustomsCharge charge5 = new CustomsCharge(null, "TEST5", 220.0m, 0m, false, Creditor.PK);
			CustomsCharge1 = CreateMockCustomCharge(charge4, charge5);
			ChargesProvider1 = CreateMockCustomsChargesProvider(CustomsJob1, "ABCD1234", CustomsCharge1);

			APInvoice secondInvoice = CreateAPInvoice(ChargesProvider1, 110m);
			result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertContains("Error Message", "Payment ABCD1234 already exists", result.ErrorMessage);
			AssertNotEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", false, result.WasSuccessful);

			CustomsCharge charge6 = new CustomsCharge(null, "TEST6", 220, 0m, true, Creditor.PK);
			CustomsCharge1 = CreateMockCustomCharge(charge6);
			ChargesProvider1 = CreateMockCustomsChargesProvider(CustomsJob1, "ABCD1234", CustomsCharge1);

			APInvoice invoice2 = CreateAPInvoice(ChargesProvider1, 110m);
			result = CreatePayment("WXYZ1234", ChargesProvider1);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestCreatePaymentWherePaymentAlreadyExistsInADifferentCompany()
		{
			CreateAPInvoice(ChargesProvider1, 110m);
			ICustomsPaymentCreationResult result = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);

			APPayment payment = FindPayment(Creditor, jobNumber: "ABCD1234");
			AssertNotNull("Payment", payment);
			AssertEquals("payment created in correct Company", payment.Company.PK, GlbCompany.CurrentCompany.PK);

			CreateAPInvoice(ChargesProvider1, 110m);
			ICustomsPaymentCreationResult secondAttemptWithSameInvoiceNumber = CreatePayment("ABCD1234", ChargesProvider1);
			AssertEquals("Payment creation shouldn't be successful", false, secondAttemptWithSameInvoiceNumber.WasSuccessful);

			CustomsJob1.fBranch = ObjCreator.NonCurrentCompanyBranch;

			using (ObjCreator.NonCurrentCompanyBranch.SetAsTemporaryContext())
			{
				Creditor.OH_IsCreditor = true;
				Creditor.CompanyData.OB_AB_APDefaultBankAccount = ObjCreator.AUDBankAccount2.PK;
				RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor.PK.ToGuid());

				APInvoice invoiceInDifferentCompany = CreateAPInvoice(ChargesProvider1, 110m);

				ICustomsPaymentCreationResult resultFromDifferentCompany = CreatePayment("ABCD1234", ChargesProvider1);
				AssertEquals("Error Message", "", resultFromDifferentCompany.ErrorMessage);
				AssertEquals("Was Successful", true, resultFromDifferentCompany.WasSuccessful);

				APPayment paymentInDifferentCompany = FindPayment(Creditor, jobNumber: "ABCD1234");
				AssertNotNull("Payment Created in different company", paymentInDifferentCompany);
				AssertEquals("paymentInDifferentCompany created in correct Company", paymentInDifferentCompany.Company.PK, GlbCompany.CurrentCompany.PK);
			}
		}

		public void TestCreatePaymentWhenOneEntryHasNothingToPay()
		{
			MockCustomsJobProvider customsJob = CreateMockCustomsJobProvider(Factory, "BBB222");
			MockCustomsChargesProvider chargesProvider = CreateMockCustomsChargesProvider(customsJob, "ABCD1234");
			chargesProvider.CustomsCharges = Array.Empty<ICustomsCharges>();//nothing to pay

			MockCustomsJobProvider customsJob2 = CreateMockCustomsJobProvider(Factory, "BBB333");
			MockCustomsChargesProvider chargesProvider2 = CreateMockCustomsChargesProvider(customsJob, "ABCD1235");
			MockCustomCharge charge1 = CreateMockCustomCharge(new CustomsCharge(null, "TEST", 110.0m, 0m, true, Creditor.PK));
			chargesProvider2.CustomsCharges = new ICustomsCharges[] { charge1 };

			CreateAPInvoice(chargesProvider2, 110m);

			ICustomsPaymentCreationResult result = CreatePayment("ENT1234", chargesProvider, chargesProvider2);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestDoNotCreatePaymentWhenAllEntriesHaveNothingToPay()
		{
			MockCustomsJobProvider customsJob = CreateMockCustomsJobProvider(Factory, "BBB222");
			MockCustomsChargesProvider chargesProvider = CreateMockCustomsChargesProvider(customsJob, "ABCD1234");
			chargesProvider.CustomsCharges = Array.Empty<ICustomsCharges>();//nothing to pay

			ICustomsPaymentCreationResult result = CreatePayment("ENT1234", chargesProvider);
			AssertEquals("Error Message", "Daily Statement ENT1234:Nothing to pay. No payment has been created.", result.ErrorMessage);
			AssertEquals("Was Successful", false, result.WasSuccessful);

			APPayment payment = FindPayment(Creditor, jobNumber: "ENT1234");
			AssertNull("No payment should have been made", payment);
		}

		public void TestCreatePaymentWithMissingInvoices()
		{
			var result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertEquals("Payment shouldn't be created (no invoices to pay)", false, result.WasSuccessful);
			AssertContains("Unable to find unpaid AP Invoice ABCD1234", result.ErrorMessage);
			AssertContains("Unable to find unpaid AP Invoice ABCD1234", result.ErrorMessage);

			CreateAPInvoice(ChargesProvider1, 110m);

			result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertEquals("Payment shouldn't be created (one invoice is missing)", false, result.WasSuccessful);
			AssertNotContains("Unable to find unpaid AP Invoice ABCD1234", result.ErrorMessage);
			AssertContains("Unable to find unpaid AP Invoice WXYZ5678", result.ErrorMessage);

			CreateAPInvoice(ChargesProvider2, 360m);

			result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertNotContains("Unable to find unpaid AP Invoice ABCD1234", result.ErrorMessage);
			AssertNotContains("Unable to find unpaid AP Invoice WXYZ5678", result.ErrorMessage);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestErrorMessagesRelatingToCreditorAndDefaultBankAccount()
		{
			CreateAPInvoice(ChargesProvider1, 110m);

			// No Creditor
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty);
			var result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertEquals("Was Successful (Missing Creditor)", false, result.WasSuccessful);
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor.PK.ToGuid());

			// Not ticked as Payables
			Creditor.OH_IsCreditor = false;
			result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertEquals("Was Successful (Creditor not ticked as Payables)", false, result.WasSuccessful);
			Creditor.OH_IsCreditor = true;

			// No Bank Account
			Creditor.CompanyData.OB_AB_APDefaultBankAccount = ZGuid.Empty;
			result = CreatePayment("QWERTYUIOP", ZGuid.Empty, ChargesProvider1);
			AssertEquals("Was Successful (No Default Bank Account)", false, result.WasSuccessful);
			Creditor.CompanyData.OB_AB_APDefaultBankAccount = ObjCreator.AUDBankAccount.PK;

			// Foreign Currency Bank Account
			Creditor.CompanyData.OB_AB_APDefaultBankAccount = ObjCreator.USDBankAccount.PK;
			result = CreatePayment("QWERTYUIOP", ObjCreator.USDBankAccount.PK, ChargesProvider1);
			AssertEquals("Was Successful (Foreign Currency Bank Account)", false, result.WasSuccessful);
			Creditor.CompanyData.OB_AB_APDefaultBankAccount = ObjCreator.AUDBankAccount.PK;

			// Everything OK
			result = CreatePayment("QWERTYUIOP", ObjCreator.AUDBankAccount.PK, ChargesProvider1);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestErrorMessageWhenPayingForeignCurrencyInvoices()
		{
			APInvoice invoice = CreateAPInvoice(ChargesProvider1, 110m);

			// Foreign Currency Invoice shouldn't work
			invoice.AH_RX_NKTransactionCurrency = ObjCreator.USD.RX_Code;
			var result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertEquals("Was Successful", false, result.WasSuccessful);
			AssertContains("AP Invoice ABCD1234 is not in local currency", result.ErrorMessage);

			// Local Currency Invoice should work
			invoice.AH_RX_NKTransactionCurrency = ObjCreator.LocalCurrency.RX_Code;
			result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertEquals("Was Successful", true, result.WasSuccessful);
			AssertNotContains("AP Invoice ABCD1234 is not in local currency", result.ErrorMessage);
		}

		public void TestCreatePaymentWithDiscrepanciesBetweenCustomsAndInvoiceAmounts()
		{
			APInvoice invoice1 = CreateAPInvoice(ChargesProvider1, 115m); // Should be 110
			APInvoice invoice2 = CreateAPInvoice(ChargesProvider2, 225m); // Should be 250

			var result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertContains("Error Message", "AP Invoice ABCD1234 Amount (115) is different to Customs Amount (110.0)", result.ErrorMessage);
			AssertContains("Error Message", "AP Invoice WXYZ5678 Amount (225) is different to Customs Amount (360.0)", result.ErrorMessage);
			AssertNotEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", false, result.WasSuccessful);

			invoice1.Lines[0].AL_OSExTaxAmount = 110m;
			invoice2.Lines[0].AL_OSExTaxAmount = 360m;
			result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertEquals("ErrorMessage", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestCreatePaymentWhereInvoicesAreAlreadyPaid()
		{
			APInvoice invoice1 = CreateAPInvoice(ChargesProvider1, 110m);
			APInvoice invoice2 = CreateAPInvoice(ChargesProvider2, 360m);

			invoice1.AH_FullyPaidDate = ZDateTime.Today;
			invoice1.AH_OutstandingAmount = 0m;

			var result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertContains("Unable to find unpaid AP Invoice ABCD1234", result.ErrorMessage);
			AssertEquals("Was Successful", false, result.WasSuccessful);

			APInvoice invoice3 = CreateAPInvoice(ChargesProvider1, 110m);
			result = CreatePayment("QWERTYUIOP", ChargesProvider1, ChargesProvider2);
			AssertNotContains("AP Invoice ABCD1234 is already paid", result.ErrorMessage);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		public void TestCreatePaymentWithCreditNote()
		{
			var apInvoice = CreateAPInvoice(ChargesProvider1, 140m);
			var creditNotice1 = CreateAPCreditNote(ChargesProvider1, 25m);

			var result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertContains("Error Message", "AP Invoice ABCD1234, ABCD1234A Amount (115) is different to Customs Amount (110.0)", result.ErrorMessage);
			AssertEquals("Was Successful", false, result.WasSuccessful);

			var creditNotice2 = CreateAPCreditNote(ChargesProvider1, 5m);
			result = CreatePayment("QWERTYUIOP", ChargesProvider1);
			AssertEquals("Error Message", "", result.ErrorMessage);
			AssertEquals("Was Successful", true, result.WasSuccessful);
		}

		#region Implementation

		protected MockCustomCharge GetCustomCharge(ZDecimal amount, ZDecimal gST, ZBool paidByBroker)
		{
			return GetCustomCharge(null, amount, gST, paidByBroker);
		}

		protected MockCustomCharge GetCustomCharge(AccChargeCode chargeCode, ZDecimal amount, ZDecimal gST, ZBool paidByBroker)
		{
			CustomsCharge customCharge1 = new CustomsCharge(chargeCode, "TEST", amount, gST, paidByBroker, Creditor.PK);
			MockCustomCharge charge1 = new MockCustomCharge();
			charge1.fIsActive = true;
			charge1.fCustomsCharges = new CustomsCharge[] { customCharge1 };
			return charge1;
		}

		APPayment FindPayment(OrgHeader creditor, string jobNumber = "", string transactionType = "")
		{
			ZQuery apPaymentQuery = new ZQuery();
			apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, (!string.IsNullOrEmpty(transactionType) ? transactionType : TransactionTypes.Payment));
			apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, creditor.PK);
			if (!string.IsNullOrEmpty(jobNumber))
			{
				apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, jobNumber);
			}
			apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			apPaymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);

			return Factory.LoadTop1<APPayment>(apPaymentQuery);
		}

		APInvoice CreateAPInvoice(IAccInvoiceDataProvider dataProvider, ZDecimal invoiceAmount)
		{
			Job jobHeader = new Job.Loader(dataProvider.CustomsJob.TopLevelObjectForJobToReference).TryLoadOrCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = dataProvider.UniqueNumber;
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_OH = Creditor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;

			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = jobHeader.PK;
			line.AL_OSExTaxAmount = invoiceAmount;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_AG = ObjCreator.GLHeader1.PK;

			return invoice;
		}

		APCreditNote CreateAPCreditNote(IAccInvoiceDataProvider dataProvider, ZDecimal invoiceAmount)
		{
			var jobHeader = new Job.Loader(dataProvider.CustomsJob.TopLevelObjectForJobToReference).TryLoadOrCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var invoice = Factory.New<APCreditNote>();
			invoice.AH_TransactionNum = dataProvider.UniqueNumber + "A";
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.CreditNote;
			invoice.AH_OH = Creditor.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;

			var line = (APCreditNoteLine)invoice.Lines.AddNew();
			line.AL_JH = jobHeader.PK;
			line.AL_OSExTaxAmount = invoiceAmount;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			return invoice;
		}

		ICustomsPaymentCreationResult CreatePayment(ZString paymentNumber, params IAccInvoiceDataProvider[] chargesProviders)
		{
			return CreatePayment(paymentNumber, ObjCreator.AUDBankAccount.PK, chargesProviders);
		}

		ICustomsPaymentCreationResult CreatePayment(ZString paymentNumber, ZGuid bankAccount, params IAccInvoiceDataProvider[] chargesProviders)
		{
			CustomsPaymentDataProvider paymentDataProvider = new CustomsPaymentDataProvider();
			paymentDataProvider.EmailGroupPK = EmailRecipientGuid;
			paymentDataProvider.SendEmail = true;
			paymentDataProvider.BankAccountPK = bankAccount;
			paymentDataProvider.Creditor = Creditor;
			paymentDataProvider.Company = GlbCompany.CurrentCompany;

			return CreatePayment(paymentDataProvider, paymentNumber, chargesProviders);
		}

		ICustomsPaymentCreationResult CreatePayment(CustomsPaymentDataProvider paymentDataProvider, ZString paymentNumber, params IAccInvoiceDataProvider[] chargesProviders)
		{
			var paymentCreator = new CustomsPaymentCreator();

			var aPPaymentGroup = new MockAPPaymentGroup(chargesProviders);
			aPPaymentGroup.APPaymentNumber = paymentNumber;

			var list = new List<IAPPaymentGroup>();
			list.Add(aPPaymentGroup);
			return paymentCreator.CreatePayment(paymentDataProvider, list);
		}

		class CustomsPaymentDataProvider : ICustomsPaymentDataProvider
		{
			public ZString BankAccountUniqueID
			{
				get;
				set;
			}

			public ZGuid BankAccountPK
			{
				get; set;
			}

			public ZGuid EmailGroupPK
			{
				get; set;
			}

			public ZString HyperLinkText
			{
				get; set;
			}

			public bool SendEmail
			{
				get;
				set;
			}

			public OrgHeader Creditor
			{
				get;
				set;
			}

			public GlbCompany Company
			{
				get;
				set;
			}

			public ZDateTime PaymentDate
			{
				get;
				set;
			}
		}

		void AssertTransactionIsFullyPaid(TransactionHeader transaction)
		{
			ZString description = transaction.AH_Ledger + " " +
									transaction.AH_TransactionType + " " +
									transaction.AH_TransactionNum + " " +
									transaction.AH_TransactionReference + " ";

			AssertEquals(description + "OutstandingAmount", 0m, transaction.AH_OutstandingAmount);
			AssertEquals(description + "FullyPaidDate", ZDateTime.Today, transaction.AH_FullyPaidDate.Date);
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupStaffMemberEmailAddress();

			ObjCreator = new TestObjectCreator(Factory);
			Creditor = ObjCreator.Creditor1;

			CustomsCharge charge1 = new CustomsCharge(null, "TEST", 110.0m, 0m, true, Creditor.PK);
			CustomsCharge charge2 = new CustomsCharge(null, "TES2", 120.0m, 0m, false, Creditor.PK);
			CustomsCharge charge3 = new CustomsCharge(null, "TES3", 140.0m, 0m, true, Creditor.PK);

			CustomsCharge1 = CreateMockCustomCharge(charge1, charge2);
			CustomsCharge2 = CreateMockCustomCharge(charge1, charge3);

			CustomsJob1 = CreateMockCustomsJobProvider(Factory, "AAA111");
			CustomsJob2 = CreateMockCustomsJobProvider(Factory, "BBB222");

			ChargesProvider1 = CreateMockCustomsChargesProvider(CustomsJob1, "ABCD1234", CustomsCharge1);
			ChargesProvider2 = CreateMockCustomsChargesProvider(CustomsJob2, "WXYZ5678", CustomsCharge1, CustomsCharge2);

			Creditor.CompanyData.OB_AB_APDefaultBankAccount = ObjCreator.AUDBankAccount.PK;
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creditor.PK.ToGuid());
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			currentStaffMember.GS_EmailAddress = EmailAddress;

			GlbGroup group = staffMemberFactory.New<GlbGroup>();
			group.Staff.Add(currentStaffMember);
			EmailRecipientGuid = group.PK;

			staffMemberFactory.Save();
		}

		ZGuid EmailRecipientGuid;

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		MockCustomCharge CreateMockCustomCharge(params CustomsCharge[] charges)
		{
			var mockCustomCharge = new MockCustomCharge();
			mockCustomCharge.fIsActive = true;
			mockCustomCharge.fCustomsCharges = charges;
			return mockCustomCharge;
		}

		MockCustomsJobProvider CreateMockCustomsJobProvider(BusinessObjectFactory factory, ZString jobNumber)
		{
			var mockCustomsJobProvider = Factory.New<MockCustomsJobProvider>();
			mockCustomsJobProvider.JobNumber = jobNumber;
			return mockCustomsJobProvider;
		}

		MockCustomsChargesProvider CreateMockCustomsChargesProvider(MockCustomsJobProvider customsJobProvider,
			ZString invoiceNumber, params ICustomsCharges[] customsCharges)
		{
			var mockCustomsChargesProvider = new MockCustomsChargesProvider();
			mockCustomsChargesProvider.CustomsJob = customsJobProvider;
			mockCustomsChargesProvider.Factory = Factory;
			mockCustomsChargesProvider.CustomsCharges = customsCharges;
			mockCustomsChargesProvider.InvoiceNumber = invoiceNumber;
			return mockCustomsChargesProvider;
		}

		TestObjectCreator ObjCreator;
		OrgHeader Creditor;
		MockCustomsJobProvider CustomsJob1, CustomsJob2;
		MockCustomsChargesProvider ChargesProvider1, ChargesProvider2;
		MockCustomCharge CustomsCharge1, CustomsCharge2;

		#endregion
	}
}
