using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestsSubclassesOf(typeof(BasePostManager))]
	public abstract partial class BasePostManagerTest : TestCaseWithFactory
	{
		#region TestAPPaymentApprovalAmountUpdater

		public void TestAPPaymentApprovalAmountUpdater()
		{
			AssertAPPaymentApprovalAmountUpdater(false);
		}

		public void TestAPPaymentApprovalAmountUpdater_AfterCriticalError()
		{
			AssertAPPaymentApprovalAmountUpdater(true);
		}

		protected virtual void AssertAPPaymentApprovalAmountUpdater(bool hasCriticalErrors)
		{
			var taxTestHelper = new AccountingTestObjectCreator(new BusinessObjectFactory());
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(GlbCompany.CurrentCompany, LedgerTypesList.Codes.AccountsPayable);

			var taxProcessorMock = new Mock<ITaxProcessor>();
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var chargeCode = TestObjectCreator.CC1;
			TestObjectCreator.CreateCharge(job, chargeCode, creditor: TestObjectCreator.Creditor1, invoiceNum: "INV1", paymentBankAccount: TestObjectCreator.AUDBankAccount);
			TestObjectCreator.CreateCharge(job, chargeCode, creditor: TestObjectCreator.Creditor2, invoiceNum: "INV2", paymentBankAccount: TestObjectCreator.AUDBankAccount);
			var postManager = GetPostManager(new[] { job });
			AssertType<APPaymentApprovalAmountUpdater>(postManager.APPaymentApprovalAmountUpdater_ExposedForTestOnly);

			var paymentUpdaterMock = new Mock<IAPPaymentApprovalAmountUpdater>(MockBehavior.Strict);
			paymentUpdaterMock.Setup(x => x.UpdateAmountsOnAllPaymentsAndInvoiceLinks(It.IsAny<TransactionCreatorHashtable>())).Callback<TransactionCreatorHashtable>(AssertTransactionCreatorHashtablePassedIntoUpdateAmountsOnAllPaymentsAndInvoiceLinks);
			postManager.SubstituteAPPaymentApprovalAmountUpdater_ForTestOnly(paymentUpdaterMock.Object);

			if (hasCriticalErrors)
			{
				taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("I'm tired");
				postManager.CreateTransactions(JobInvoicingPostingOption.All);
				paymentUpdaterMock.Verify(x => x.UpdateAmountsOnAllPaymentsAndInvoiceLinks(It.IsAny<TransactionCreatorHashtable>()), Times.Never);
			}
			else
			{
				taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("").Callback<ITaxRecordParent>(taxRecordParent => taxRecordParent.OSTaxAmount = 23M);
				var createdTransactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
				taxProcessorMock.Verify(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>()), "PostCondition: Tax Transactions must be calculated.");
				paymentUpdaterMock.Verify(x => x.UpdateAmountsOnAllPaymentsAndInvoiceLinks(createdTransactions), Times.Once);
			}

			void AssertTransactionCreatorHashtablePassedIntoUpdateAmountsOnAllPaymentsAndInvoiceLinks(TransactionCreatorHashtable transactions)
			{
				var payments = transactions.GetAllAPPaymentApprovals();
				AssertEquals("PostCondition: Number of payments passed in UpdateAmountsOnAllPaymentsAndInvoiceLinks method", 2, payments.Length);

				CombineAssertions(() =>
				{
					foreach (var payment in payments)
					{
						var items = new PaymentApprovalItemCollection(payment);
						items.Load();
						AssertEquals("PostCondition: Number of invoices attached for a payment", 1, items.Count);
						AssertEquals("PostCondition: AH_OSTaxAmountOtherTaxes", 23m, items[0].Header.AH_OSTaxAmountOtherTaxes);
						AssertEquals("Invoice total includes Tax Transactions on time of Payment amount update", -87m, items[0].Header.AH_OSTotal);
					}
				});
			}
		}

		#endregion

		public void TestCheckInvoiceOrCreditNoteLines_ValidateLines_KoreaSouth_EInvoicingEnabled()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			{
				AssertEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Pre-condition", true, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				AssertCheckInvoiceOrCreditNoteLines_ValidateLines(hasErrorWhenGreaterThan99: true, hasErrorWhenEqualTo99: false, hasErrorWhenLessThan99: false);
			}
		}

		public void TestCheckInvoiceOrCreditNoteLines_ValidateLines_KoreaSouth_EInvoicingDisabled()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
			{
				AssertEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Pre-condition", false, AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value);

				AssertCheckInvoiceOrCreditNoteLines_ValidateLines(false, false, false);
			}
		}

		public void TestCheckInvoiceOrCreditNoteLines_ValidateLines_NotKoreaSouth()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertNotEquals("Pre-condition", CountryCodes.KoreaSouth, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				AssertCheckInvoiceOrCreditNoteLines_ValidateLines(false, false, false);
			}
		}

		void AssertCheckInvoiceOrCreditNoteLines_ValidateLines(bool hasErrorWhenGreaterThan99, bool hasErrorWhenEqualTo99, bool hasErrorWhenLessThan99)
		{
			var expectedErrorMessage = "The Korea National Tax Service only accepts up to 99 transaction lines per invoice. Please split the charges into multiple invoices.";
			var shipment1 = TestObjectCreator.CreateShipment("S001");
			var shipment2 = TestObjectCreator.CreateShipment("S002");
			var shipment3 = TestObjectCreator.CreateShipment("S003");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var job3 = TestObjectCreator.CreateJob(shipment3, false);
			var organization = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.CC1;
			SetupDebtor(organization);

			for (var i = 0; i < 98; i++)
			{
				TestObjectCreator.CreateCharge(job1, chargeCode, debtor: organization);
				TestObjectCreator.CreateCharge(job2, chargeCode, debtor: organization);
				TestObjectCreator.CreateCharge(job3, chargeCode, debtor: organization);
			}
			TestObjectCreator.CreateCharge(job1, chargeCode, debtor: organization);
			TestObjectCreator.CreateCharge(job1, chargeCode, debtor: organization);
			TestObjectCreator.CreateCharge(job2, chargeCode, debtor: organization);

			AssertEquals("Pre-condition", 100, job1.Charges.Count);
			AssertEquals("Pre-condition", 99, job2.Charges.Count);
			AssertEquals("Pre-condition", 98, job3.Charges.Count);

			AssertHasError(job1, hasErrorWhenGreaterThan99);
			AssertHasError(job2, hasErrorWhenEqualTo99);
			AssertHasError(job3, hasErrorWhenLessThan99);

			void AssertHasError(Job job, bool hasError)
			{
				var lastErrorReported = "";

				var postManager = GetPostManager(new[] { job });
				postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
				postManager.CreateTransactions(GetRevenuePostingOption());

				AssertEquals(hasError, postManager.CancelPosting);
				if (hasError)
				{
					AssertContains(expectedErrorMessage, lastErrorReported);
				}
				else
				{
					AssertNotContains(expectedErrorMessage, lastErrorReported);
				}
			}
		}

		public void TestCheckTaxMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var organization = TestObjectCreator.TestOrganisation;
			var chargeCode = TestObjectCreator.CC1;
			SetupDebtor(organization);

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = AccTaxRate.Types.Rated;
			taxRate.AT_Code = "TaxRate";
			var taxMsg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg1.A9_Code = "TaxMsg01";
			var taxMsg2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg2.A9_Code = "TaxMsg02";
			Factory.Save();

			var config = TestObjectCreator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Cost, taxRate, taxMsg2));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, creditor: TestObjectCreator.Creditor1, debtor: organization, invoiceNum: "INV1");
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_A9_CostVATClass = taxMsg1.PK;

			var lastErrorReported = "";
			var postManager = GetPostManager(new[] { job });
			postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
			var expectedErrorMessage = @"Transaction cannot be saved/posted due to invalid Tax ID and Tax Message combination.
Line Type=CST, Tax ID=TaxRate, Tax Message=TaxMsg01";

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert("Posting is cancelled", postManager.CancelPosting);
			AssertContains(expectedErrorMessage, lastErrorReported);
		}

		public void TestInitializingAddressContact()
		{
			var localClient = TestObjectCreator.LocalClient;
			var localClientAddress = TestObjectCreator.CreateAddress(localClient);
			var localClientContact = TestObjectCreator.CreateContact(localClient);
			var localClientContact2 = TestObjectCreator.CreateContact(localClient, "Ben");
			var agent = TestObjectCreator.Agent;
			var agentAddress = TestObjectCreator.CreateAddress(agent);
			var agentContact = TestObjectCreator.CreateContact(agent);
			var someOtherDebtor = TestObjectCreator.Debtor;

			var shipment = TestObjectCreator.CreateShipment("S001", saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, localClient, 0, agent, 0);
			job.JH_OA_LocalChargesAddr = localClientAddress.PK;
			job.JH_OC_LocalBillingContact = localClientContact.PK;
			job.JH_OA_AgentCollectAddr = agentAddress.PK;

			var chargeLocalClient = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 10);
			chargeLocalClient.JR_OH_SellAccount = localClient.PK;
			AssertEquals("Precondition: chargeLocalClient.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeLocalClient.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: chargeLocalClient.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("Precondition: chargeLocalClient.DisplaySellInvoiceAddress", localClientAddress.PK, chargeLocalClient.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: chargeLocalClient.DisplaySellInvoiceContact", localClientContact.PK, chargeLocalClient.DisplaySellInvoiceContact);
			var chargeLocalClient2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 10);
			chargeLocalClient2.JR_OH_SellAccount = localClient.PK;
			chargeLocalClient2.DisplaySellInvoiceAddress = localClient.MainAddress.PK;
			chargeLocalClient2.DisplaySellInvoiceContact = localClientContact2.PK;
			AssertEquals("Precondition: chargeLocalClient.JR_OA_SellInvoiceAddress", localClient.MainAddress.PK, chargeLocalClient2.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: chargeLocalClient.JR_OC_SellInvoiceContact", localClientContact2.PK, chargeLocalClient2.JR_OC_SellInvoiceContact);
			AssertEquals("Precondition: chargeLocalClient.DisplaySellInvoiceAddress", localClient.MainAddress.PK, chargeLocalClient2.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: chargeLocalClient.DisplaySellInvoiceContact", localClientContact2.PK, chargeLocalClient2.DisplaySellInvoiceContact);
			var chargeAgent = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 10);
			chargeAgent.JR_OH_SellAccount = agent.PK;
			AssertEquals("Precondition: chargeAgent.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeAgent.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: chargeAgent.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
			AssertEquals("Precondition: chargeAgent.DisplaySellInvoiceAddress", agentAddress.PK, chargeAgent.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: chargeAgent.DisplaySellInvoiceContact", ZGuid.Empty, chargeAgent.DisplaySellInvoiceContact);
			var chargeAgent2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 10);
			chargeAgent2.JR_OH_SellAccount = agent.PK;
			chargeAgent2.DisplaySellInvoiceAddress = agent.MainAddress.PK;
			chargeAgent2.DisplaySellInvoiceContact = agentContact.PK;
			AssertEquals("Precondition: chargeAgent.JR_OA_SellInvoiceAddress", agent.MainAddress.PK, chargeAgent2.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: chargeAgent.JR_OC_SellInvoiceContact", agentContact.PK, chargeAgent2.JR_OC_SellInvoiceContact);
			AssertEquals("Precondition: chargeAgent.DisplaySellInvoiceAddress", agent.MainAddress.PK, chargeAgent2.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: chargeAgent.DisplaySellInvoiceContact", agentContact.PK, chargeAgent2.DisplaySellInvoiceContact);
			var chargeSomeOtherDebtor = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 0, 10);
			chargeSomeOtherDebtor.JR_OH_SellAccount = someOtherDebtor.PK;
			var expectedDisplaySellInvoiceAddressForChargeSomeOtherDebtor = someOtherDebtor.MainAddress.PK;
			AssertEquals("Precondition: chargeSomeOtherDebtor.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeSomeOtherDebtor.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: chargeSomeOtherDebtor.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeSomeOtherDebtor.JR_OC_SellInvoiceContact);
			AssertEquals("Precondition: chargeSomeOtherDebtor.DisplaySellInvoiceAddress", expectedDisplaySellInvoiceAddressForChargeSomeOtherDebtor, chargeSomeOtherDebtor.DisplaySellInvoiceAddress);
			AssertEquals("Precondition: chargeSomeOtherDebtor.DisplaySellInvoiceContact", ZGuid.Empty, chargeSomeOtherDebtor.DisplaySellInvoiceContact);

			var jobs = new[] { job };
			var postManager = GetPostManager(jobs);
			localClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();
			var transactions = postManager.CreateTransactions(GetRevenuePostingOption());

			bool isConsolPosting = postManager is ConsolInvoicingPostManager;
			AssertEquals("Posted ARTransactionsCount", isConsolPosting ? 4 : 5, transactions.ARTransactionsCount);

			Assert("chargeLocalClient.IsRevenuePosted", chargeLocalClient.IsRevenuePosted);
			AssertEquals("chargeLocalClient.JR_OA_SellInvoiceAddress", localClientAddress.PK, chargeLocalClient.JR_OA_SellInvoiceAddress);
			AssertEquals("chargeLocalClient.JR_OC_SellInvoiceContact", localClientContact.PK, chargeLocalClient.JR_OC_SellInvoiceContact);
			AssertEquals("chargeLocalClient2.JR_OA_SellInvoiceAddress", localClient.MainAddress.PK, chargeLocalClient2.JR_OA_SellInvoiceAddress);
			AssertEquals("chargeLocalClient2.JR_OC_SellInvoiceContact", localClientContact2.PK, chargeLocalClient2.JR_OC_SellInvoiceContact);
			Assert("chargeAgent.IsRevenuePosted", chargeAgent.IsRevenuePosted);
			if (isConsolPosting)
			{
				AssertEquals("chargeAgent.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeAgent.JR_OA_SellInvoiceAddress);
				AssertEquals("chargeAgent.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
				AssertEquals("chargeAgent2.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeAgent2.JR_OA_SellInvoiceAddress);
				AssertEquals("chargeAgent2.JR_OA_SellInvoiceAddress", ZGuid.Empty, chargeAgent2.JR_OA_SellInvoiceAddress);
			}
			else
			{
				AssertEquals("chargeAgent.JR_OA_SellInvoiceAddress", agentAddress.PK, chargeAgent.JR_OA_SellInvoiceAddress);
				AssertEquals("chargeAgent.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeAgent.JR_OC_SellInvoiceContact);
				AssertEquals("chargeAgent2.JR_OA_SellInvoiceAddress", agent.MainAddress.PK, chargeAgent2.JR_OA_SellInvoiceAddress);
				AssertEquals("chargeAgent2.JR_OC_SellInvoiceContact", agentContact.PK, chargeAgent2.JR_OC_SellInvoiceContact);
			}
			Assert("chargeSomeOtherDebtor.IsRevenuePosted", chargeSomeOtherDebtor.IsRevenuePosted);
			AssertEquals("chargeSomeOtherDebtor.JR_OA_SellInvoiceAddress. All charges with empty Address are defaulted to DisplaySellInvoiceAddress.", expectedDisplaySellInvoiceAddressForChargeSomeOtherDebtor, chargeSomeOtherDebtor.JR_OA_SellInvoiceAddress);
			AssertEquals("chargeSomeOtherDebtor.JR_OC_SellInvoiceContact", ZGuid.Empty, chargeSomeOtherDebtor.JR_OC_SellInvoiceContact);
		}

		[TestDate(2023, 03, 30)]
		public void TestChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDatingWithDifferentForeignCurrencys()
		{
			PostingExRateRegistryAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 1.3362m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 1.3462m, ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.BuyRate, 1.3562m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.BuyRate, 1.3662m, ZDateTime.Today.AddDays(-6), ZDateTime.Today.AddDays(-2));

			var testRatePK = TestObjectCreator.GST1.PK;
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);

			var chargeUSD = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "testUSD", TestObjectCreator.AUD, 0m, null, TestObjectCreator.USD, 100m, TestObjectCreator.LocalClient);
			chargeUSD.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var chargeEUR = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "testEUR", TestObjectCreator.AUD, 0m, null, TestObjectCreator.EUR, 100m, TestObjectCreator.LocalClient);
			chargeEUR.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var postManager = new InvoicingPostManager(job);
			postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			Factory.Save();

			AssertEquals("Precondition: BillInLocalCurrency", false, chargeUSD.BillInLocalCurrency);
			AssertEquals("Precondition: USD exchange rate should be today's rate before backing date", 1.3362m, chargeUSD.JR_OSSellExRate);
			AssertEquals("Precondition: EUR exchange rate should be today's rate before backing date", 1.3562m, chargeEUR.JR_OSSellExRate);

			var result = postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(ZDateTime.Today.AddDays(-3), ZDateTime.Today, Factory);
			AssertEquals("Update exchange rate successfully when exchange rate was setted", true, result);
			AssertEquals("USD exchange rate should be changed", 1.3462m, chargeUSD.JR_OSSellExRate);
			AssertEquals("EUR exchange rate should be changed", 1.3662m, chargeEUR.JR_OSSellExRate);

			result = postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(ZDateTime.Today.AddDays(-5), ZDateTime.Today, Factory);
			AssertEquals("Update exchange rate failed when exchange rate of USD is not setted", false, result);
		}

		public void TestPostingChargesWithSellInvoiceCurrencyWithPostingExchangeRateConfiguration()
		{
			AssertPostingChargesWithSellInvoiceCurrency(true);
		}

		public void TestPostingChargesWithSellInvoiceCurrencyWithoutPostingExchangeRateConfiguration()
		{
			AssertPostingChargesWithSellInvoiceCurrency(false);
		}

		[TestDate(2019, 08, 02)]
		public void TestChangeTransactionDateForBackDatingWhenGroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction()
		{
			PostingExRateRegistryAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);

			var taxRate = TestObjectCreator.CreateTaxRate("GST2", string.Empty, 7);
			TestObjectCreator.CC1.AC_AT_GSTRate = taxRate.PK;

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 4M);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD);
			invoice.AH_PostDate = new ZDateTime(2019, 08, 02);
			invoice.AH_InvoiceDate = new ZDateTime(2019, 08, 02);

			var line = TestObjectCreator.CreateInvoiceLine(invoice, 25m, TestObjectCreator.USD);
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			line.AL_LineAmount = 21m;
			line.AL_OSAmount = 4m;

			var charge = TestObjectCreator.CreateCharge(job, testObjectCreator.CC1, "test", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, invoice.Header);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			job.Charges.Load();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD);
			invoice1.AH_PostDate = new ZDateTime(2019, 08, 02);
			invoice1.AH_InvoiceDate = new ZDateTime(2019, 08, 02);

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, 25m, TestObjectCreator.USD);
			line1.AL_JH = job.PK;
			line1.AL_AT = taxRate.PK;
			line1.AL_LineAmount = 21m;
			line1.AL_OSAmount = 4m;

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, invoice1.Header);
			charge1.JR_AT_SellGSTRate = taxRate.PK;
			job.Charges.Load();

			var postManager = new InvoicingPostManager(job);
			postManager.Poster.PostedInvoices.Add(invoice);
			postManager.Poster.PostedInvoices.Add(invoice1);

			var company = Factory.NewWithValidTestData(typeof(GlbCompany));
			var company1 = Factory.NewWithValidTestData(typeof(GlbCompany));

			postManager.Poster.PostedInvoices[0].AH_GC = company.PK;
			postManager.Poster.PostedInvoices[1].AH_GC = company1.PK;

			postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(invoice.AH_InvoiceDate, invoice.AH_PostDate, Factory);
			CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction);
			postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(invoice.AH_InvoiceDate, invoice.AH_PostDate, Factory);
			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction);
			AssertContains("GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", collectedInfo);

			var postingExRateOptions = new InvoicePostingExRateOptionCollection();
			postingExRateOptions.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code, -1));
			postingExRateOptions.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, AccountingConstants.InvoicePostingExchangeRateOption.Default.Code, -1));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postingExRateOptions);

			postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(new ZDateTime(), new ZDateTime(), Factory);
			var collectedInfo1 = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(invoice.PK, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction);
			var result1 = string.Format(@"
GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction:
Group First IsLocalCurrencyTransaction = False, Group First AH_GC = {0}, Invoice IsLocalCurrencyTransaction = False, Invoice AH_GC = {0}", postManager.Poster.PostedInvoices[0].AH_GC);
			AssertContains(result1, collectedInfo1);

			invoice.Company.GC_RX_NKLocalCurrency = "USD";
			invoice1.Company.GC_RX_NKLocalCurrency = "AUD";

			postManager.ChangeTransactionDateOnAllARInvoicesAndCFXLinesAndUpdateChargesAndLinesExchangeRateForBackDating(invoice.AH_InvoiceDate, invoice.AH_PostDate, Factory);
			var collectedInfo2 = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(postManager.Poster.PostedInvoices[0].PK, CriticalValidationInfoCollectorServiceKeyType.GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction);
			var result2 = string.Format(@"
GroupedInvoicesHaveDifferentCompanyOrIsLocalCurrencyTransaction:
Group First IsLocalCurrencyTransaction = False, Group First AH_GC = {0}, Invoice IsLocalCurrencyTransaction = False, Invoice AH_GC = {0}
Group First IsLocalCurrencyTransaction = True, Group First AH_GC = {0}, Invoice IsLocalCurrencyTransaction = True, Invoice AH_GC = {0}", postManager.Poster.PostedInvoices[0].AH_GC );
			AssertContains(result2, collectedInfo2);
		}

		void AssertPostingChargesWithSellInvoiceCurrency(bool withPostingExchangeRateConfiguration)
		{
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.USD, TestObjectCreator.AUD, TestObjectCreator.EUR });
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.USD, TestObjectCreator.EUR, TestObjectCreator.AUD });
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.EUR, TestObjectCreator.AUD, TestObjectCreator.USD });
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.EUR, TestObjectCreator.USD, TestObjectCreator.AUD });
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.AUD, TestObjectCreator.USD, TestObjectCreator.EUR });
			AssertPostingChargesWithSellInvoiceCurrency(withPostingExchangeRateConfiguration, new[] { TestObjectCreator.AUD, TestObjectCreator.EUR, TestObjectCreator.USD });
		}

		void AssertPostingChargesWithSellInvoiceCurrency(bool withPostingExchangeRateConfiguration, RefCurrency[] currenciesOrder)
		{
			var localCurrency = TestObjectCreator.AUD;
			var foreignCurrencyUSD = TestObjectCreator.USD;
			var foreignCurrencyEUR = TestObjectCreator.EUR;
			var jobBuyExchangeRateUSD = 0.8m;
			var todayExchangeRateUSD = 0.6m;
			var jobBuyExchangeRateEUR = 1.3m;
			var todayExchangeRateEUR = 1.1m;
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var postingExchangeRateOption = withPostingExchangeRateConfiguration
				? AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code
				: AccountingConstants.InvoicePostingExchangeRateOption.Default.Code;
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingExchangeRateOption);

			TestObjectCreator.CreateExchangeRate(foreignCurrencyUSD, Constants.ExchangeRateTypes.Code.BuyRate, todayExchangeRateUSD);
			TestObjectCreator.CreateExchangeRate(foreignCurrencyEUR, Constants.ExchangeRateTypes.Code.BuyRate, todayExchangeRateEUR);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);

			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			job.PlugInData = shipment;
			TestObjectCreator.CreateExchangeRate(job, foreignCurrencyUSD, jobBuyExchangeRateUSD);
			TestObjectCreator.CreateExchangeRate(job, foreignCurrencyEUR, jobBuyExchangeRateEUR);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Revenue Transaction Test 1", TestObjectCreator.AUD, 0m, null,
				currenciesOrder[0], 1M, TestObjectCreator.LocalClient);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Revenue Transaction Test 2", TestObjectCreator.AUD, 0m, null,
				currenciesOrder[1], 1M, TestObjectCreator.LocalClient);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Revenue Transaction Test 3", TestObjectCreator.AUD, 0m, null,
				currenciesOrder[2], 1M, TestObjectCreator.LocalClient);

			var chargeUSD = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_RX_NKSellCurrency == TestObjectCreator.USD.RX_Code);
			var chargeEUR = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_RX_NKSellCurrency == TestObjectCreator.EUR.RX_Code);
			var localCharge = job.Charges.Cast<Charge>().FirstOrDefault(x => x.JR_RX_NKSellCurrency == localCurrency.RX_Code);

			chargeUSD.JR_RX_NKSellInvoiceCurrency = foreignCurrencyUSD.RX_Code;
			chargeEUR.JR_RX_NKSellInvoiceCurrency = foreignCurrencyUSD.RX_Code;
			localCharge.JR_RX_NKSellInvoiceCurrency = foreignCurrencyUSD.RX_Code;

			chargeUSD.JR_OSSellAmt = 125m;
			chargeEUR.JR_OSSellAmt = 7.69m;
			localCharge.JR_OSSellAmt = 50m;

			Factory.Save();

			var exRateUsd = job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.JF_RX_NKRateCurrency == foreignCurrencyUSD.RX_Code);
			var exRateEur = job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.JF_RX_NKRateCurrency == foreignCurrencyEUR.RX_Code);

			AssertEquals("job exchange rate of USD is 0.8", jobBuyExchangeRateUSD, exRateUsd.JF_BaseRate);
			AssertEquals("job today's exchange rate of USD is 0.6", todayExchangeRateUSD, exRateUsd.JF_TodayRate);

			AssertEquals("job exchange rate of EUR is 1.3", jobBuyExchangeRateEUR, exRateEur.JF_BaseRate);
			AssertEquals("job today's exchange rate of EUR is 1.1", todayExchangeRateEUR, exRateEur.JF_TodayRate);

			AssertEquals("localcurrency is current company local currency", localCurrency.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("All charges use the same invoice sell currency USD", job.Charges.Cast<Charge>().All(x => x.JR_RX_NKSellInvoiceCurrency == foreignCurrencyUSD.RX_Code));
			AssertEquals(3, job.Charges.Count);
			AssertEquals("First Currency in currenciesOrder need to be the first charge sell Currency", job.Charges[0].JR_RX_NKSellCurrency, currenciesOrder[0].RX_Code);

			AssertChargeValues(chargeUSD, 100m, 125m, 125m, jobBuyExchangeRateUSD);
			AssertChargeValues(localCharge, 50m, 50m, 62.5m, jobBuyExchangeRateUSD);
			AssertChargeValues(chargeEUR, 10m, 7.69m, 12.5m, jobBuyExchangeRateUSD);

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals("Should found one transaction", 1, transactions.Values.Count);
			Assert("All charges are posted", job.Charges.Cast<Charge>().All(x => x.IsRevenuePosted));
			Assert("Charges posting context was removed", job.Charges.Cast<Charge>().All(x => !x.HasContext(BusinessContext.PostingReceivableCharges)));

			if (withPostingExchangeRateConfiguration)
			{
				AssertChargeValues(chargeUSD, 75m, 125m, 125m, todayExchangeRateUSD);
				AssertChargeValues(localCharge, 50m, 50m, 83.33m, todayExchangeRateUSD);
				AssertChargeValues(chargeEUR, 8.46m, 7.69m, 14.10m, todayExchangeRateUSD);
			}
			else
			{
				AssertChargeValues(chargeUSD, 100m, 125m, 125m, jobBuyExchangeRateUSD);
				AssertChargeValues(localCharge, 50m, 50m, 62.5m, jobBuyExchangeRateUSD);
				AssertChargeValues(chargeEUR, 10m, 7.69m, 12.5m, jobBuyExchangeRateUSD);
			}
		}

		public void TestPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOptionLocalDefaultAndForeignNotDefault()
		{
			AssertPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOption(InvoicePostingExchangeRateOption.Default.Code, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		public void TestPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOptionLocalNotDefaultAndForeignDefault()
		{
			AssertPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOption(InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, InvoicePostingExchangeRateOption.Default.Code);
		}

		public void TestPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOptionBothDefault()
		{
			AssertPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOption(InvoicePostingExchangeRateOption.Default.Code, InvoicePostingExchangeRateOption.Default.Code);
		}

		public void TestPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOptionBothNotDefault()
		{
			AssertPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOption(InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code, InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		void AssertPostingChargesWithInvoiceCurrencyTypeAndInvoicePostingExRateOption(string localExRateOption, string foreignExRateOption)
		{
			var foreignCurrencyUSD = TestObjectCreator.USD;
			var todayExchangeRateUSD = 0.6m;
			TestObjectCreator.SetCurrentCompanyReciprocal(true);

			var collection = new InvoicePostingExRateOptionCollection();
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, foreignExRateOption, 0));
			collection.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, localExRateOption, 0));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			TestObjectCreator.CreateExchangeRate(foreignCurrencyUSD, Constants.ExchangeRateTypes.Code.BuyRate, todayExchangeRateUSD);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "local charge 1", TestObjectCreator.AUD, 0m, null, foreignCurrencyUSD, 1M, TestObjectCreator.LocalClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "foreign charge 2", TestObjectCreator.AUD, 0m, null, foreignCurrencyUSD, 1M, TestObjectCreator.LocalClient);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "foreign charge 3", TestObjectCreator.AUD, 0m, null, foreignCurrencyUSD, 1M, TestObjectCreator.LocalClient);
			charge2.JR_RX_NKSellInvoiceCurrency = foreignCurrencyUSD.RX_Code;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Factory.Save();

			Assert("charge1 has a local invoice currency type", charge1.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			Assert("charge2 has a foreign invoice currency type", !charge2.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			Assert("charge3 has a foreign invoice currency type", !charge3.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			AssertEquals("charge1 is local because it uses a FIN invoice type which is considered as a local invoice type", InvoiceTypesList.Codes.FinalInvoice, charge1.JR_InvoiceType);
			AssertEquals(InvoiceTypesList.Codes.FinalInvoice, charge2.JR_InvoiceType);
			AssertEquals("charge3 is foreign because it uses a foreign invoice type", InvoiceTypesList.Codes.ForeignCurrencyInvoice, charge3.JR_InvoiceType);
			Assert("all charges have USD sell currency", job.Charges.Cast<Charge>().All(x => x.JR_RX_NKSellCurrency == foreignCurrencyUSD.RX_Code));
			AssertEquals(string.Empty, charge1.JR_RX_NKSellInvoiceCurrency);
			AssertEquals("charge2 is foreign because the sell invoice currency is foreign", foreignCurrencyUSD.RX_Code, charge2.JR_RX_NKSellInvoiceCurrency);
			AssertEquals(string.Empty, charge3.JR_RX_NKSellInvoiceCurrency);

			var exRateUsd = job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(r => r.JF_RX_NKRateCurrency == foreignCurrencyUSD.RX_Code
				&& r.JF_OrgType == ExchangeRateOrgTypeEnum.Debtor.ToCode());

			AssertEquals(todayExchangeRateUSD, exRateUsd.JF_BaseRate);
			AssertEquals(todayExchangeRateUSD, exRateUsd.JF_TodayRate);
			Assert("all charges uses the BaseRate", job.Charges.Cast<Charge>().All(x => x.JR_OSSellExRate == todayExchangeRateUSD));

			exRateUsd.JF_BaseRate = 3m;
			Assert("all charges uses the overridden BaseRate", job.Charges.Cast<Charge>().All(x => x.JR_OSSellExRate == 3m));

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);
			AssertEquals(3, transactions.Values.Count);

			if (localExRateOption == InvoicePostingExchangeRateOption.Default.Code)
			{
				AssertEquals("With DEF invoice posting option so it should uses the overridden BaseRate", 3m, charge1.JR_OSSellExRate);
			}
			else
			{
				AssertEquals("Otherwise it should uses the today's rate", todayExchangeRateUSD, charge1.JR_OSSellExRate);
			}

			if (foreignExRateOption == InvoicePostingExchangeRateOption.Default.Code)
			{
				AssertEquals("With DEF invoice posting option so it should uses the overridden BaseRate", 3m, charge2.JR_OSSellExRate);
				AssertEquals("With DEF invoice posting option so it should uses the overridden BaseRate", 3m, charge3.JR_OSSellExRate);
			}
			else
			{
				AssertEquals("Otherwise it should uses the today's rate", todayExchangeRateUSD, charge2.JR_OSSellExRate);
				AssertEquals("Otherwise it should uses the today's rate", todayExchangeRateUSD, charge3.JR_OSSellExRate);
			}
		}

		#region Check duplicate invoice numbers

		public void TestPostingChargesWithDuplicateInvoiceNumbers_Standard()
		{
			AssertPostingChargesWithDuplicateInvoiceNumbers("TESTAP001", AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD, true);
		}

		public void TestPostingChargesWithDuplicateInvoiceNumbers_Standard_NoPermission()
		{
			AssertPostingChargesWithDuplicateInvoiceNumbers("TESTAP001", AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD, false, "AP Invoice number TESTAP001 is already used for the organization: ZCreditor1. Please use another transaction number.");
		}

		public void TestPostingChargesWithDuplicateInvoiceNumbers_Calendar()
		{
			AssertPostingChargesWithDuplicateInvoiceNumbers("TESTAP002", AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL, true);
		}

		public void TestPostingChargesWithDuplicateInvoiceNumbers_Calendar_NoPermission()
		{
			AssertPostingChargesWithDuplicateInvoiceNumbers("TESTAP002", AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL, false, "AP Invoice number TESTAP002 is already used for the organization: ZCreditor1. Please use another transaction number.");
		}

		void AssertPostingChargesWithDuplicateInvoiceNumbers(string invoiceNumber, string allowDuplicateInvoiceNumberRule, bool allowDuplicateInvoiceNumber, string expectedErrorMessage = null)
		{
			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = allowDuplicateInvoiceNumber;

				var organization = TestObjectCreator.TestOrganisation;

				var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNumber, TestObjectCreator.USD, 4m, TestObjectCreator.Creditor1);
				invoice.AH_PostDate = ZDateTime.Today.AddMonths(-AccountingUtils.DuplicateInvoiceNumberPeriodMonths);
				invoice.AH_InvoiceDate = ZDateTime.Today.AddMonths(-AccountingUtils.DuplicateInvoiceNumberPeriodMonths);
				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
				var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

				TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, creditor: TestObjectCreator.Creditor1, invoiceNum: invoiceNumber, paymentBankAccount: TestObjectCreator.AUDBankAccount, debtor: organization);
				Factory.Save();

				AssertHasError(job, !allowDuplicateInvoiceNumber);

				void AssertHasError(Job job, bool hasError)
				{
					var lastErrorReported = "";

					var postManager = GetPostManager(new[] { job });
					postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
					postManager.CreateTransactions(JobInvoicingPostingOption.All);

					AssertEquals(hasError, postManager.CancelPosting);
					if (hasError)
					{
						AssertContains(expectedErrorMessage, lastErrorReported);
					}
				}
			}
		}

		#endregion

		#region Prevent Posting of Credit Notes

		public void TestPreventPostingARCRD()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var debtor1 = TestObjectCreator.Debtor;
			var debtor2 = TestObjectCreator.Debtor1;
			SetupDebtor(debtor1);
			SetupDebtor(debtor2);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", null, 0M, null, TestObjectCreator.AUD, 100M, debtor1);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", null, 0M, null, TestObjectCreator.AUD, -200M, debtor1);

			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", null, 0M, null, TestObjectCreator.AUD, 300M, debtor2);

			Factory.Save();
			bool criticalErrorRised;
			BasePostManager postManager;

			Assert(true);
			Assert(false);

			void Assert(bool isCreditNotePrevented)
			{
				AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isCreditNotePrevented);
				CreatePostManager();
				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());
				AssertNotNull("At least 1 AR Credit note is generated", transactions.GetAllARCreditNotes().FirstOrDefault());
				AssertEquals("Posting was cancelled", isCreditNotePrevented, postManager.CancelPosting);
				AssertEquals(nameof(criticalErrorRised), isCreditNotePrevented, criticalErrorRised);
			}

			void CreatePostManager()
			{
				var newFactory = new BusinessObjectFactory();
				var jobInNewFactory = newFactory.Load<Job>(job.PK);
				postManager = GetPostManager(new[] { jobInNewFactory });
				postManager.OnCriticalPostError += PostManager_OnCriticalPostError_ForPreventCreditNoteTest;

				criticalErrorRised = false;
			}

			void PostManager_OnCriticalPostError_ForPreventCreditNoteTest(object sender, CriticalPostingErrorEventArgs e)
			{
				criticalErrorRised = true;

				var header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				AssertType<ARCreditNote>(header);
				AssertHasRowError(header, "Posting of Receivables Credit Notes is not permitted. The SELL Charges being posted would produce at least one Receivables Credit Note. Please review the prepared charges and correct appropriately. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Creation of Credit Notes.");
			}
		}

		public void TestPreventPostingAPCRD()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var creditor1 = TestObjectCreator.Creditor1;
			var creditor2 = TestObjectCreator.Creditor2;

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 100M, creditor1, "1001", null, 0M, null);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, -200M, creditor1, "1001", null, 0M, null);

			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", TestObjectCreator.AUD, 300M, creditor2, "1002", null, 0M, null);

			Factory.Save();

			bool criticalErrorRised;
			BasePostManager postManager;

			Assert(true);
			Assert(false);

			void Assert(bool isCreditNotePrevented)
			{
				AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isCreditNotePrevented);
				CreatePostManager();
				var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs);
				AssertNotNull("At least 1 AP Credit note is generated", transactions.GetAllAPCreditNotes().FirstOrDefault());
				AssertEquals("Posting was cancelled", isCreditNotePrevented, postManager.CancelPosting);
				AssertEquals(nameof(criticalErrorRised), isCreditNotePrevented, criticalErrorRised);
			}

			void CreatePostManager()
			{
				var newFactory = new BusinessObjectFactory();
				var jobInNewFactory = newFactory.Load<Job>(job.PK);
				postManager = GetPostManager(new[] { jobInNewFactory });
				postManager.OnCriticalPostError += PostManager_OnCriticalPostError_ForPreventCreditNoteTest;

				criticalErrorRised = false;
			}

			void PostManager_OnCriticalPostError_ForPreventCreditNoteTest(object sender, CriticalPostingErrorEventArgs e)
			{
				criticalErrorRised = true;

				var header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				AssertType<APCreditNote>(header);
				AssertHasRowError(header, "Posting of Payables Credit Notes is not permitted. The COST Charges being posted would produce at least one Payables Credit Note. Please review the prepared charges and correct appropriately. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.");
			}
		}

		#endregion

		static void AssertChargeValues(Charge charge, ZDecimal localSellAmt, ZDecimal oSSellAmt, ZDecimal oSSellInvoiceAmt, ZDecimal exchangeRate)
		{
			AssertEquals("charge invoice type must be final", InvoiceTypesList.Codes.FinalInvoice, charge.JR_InvoiceType);
			AssertEquals(exchangeRate, charge.JR_OSSellInvoiceExRate);
			AssertEquals(oSSellAmt, charge.JR_OSSellAmt);
			AssertEquals(oSSellInvoiceAmt, charge.JR_OSSellInvoiceAmt);
			AssertEquals(localSellAmt, charge.JR_LocalSellInvoiceAmt);
			AssertEquals(localSellAmt, charge.JR_LocalSellAmt);
		}

		public void TestAvoidFromDuplicationOfCharges()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();

			var jobs = new[] { job1, job2 };

			var job1Charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100, null, TestObjectCreator.AUD, 200, null);
			job2.Charges.Add(job1Charge1);

			DummyBasePostManagerForTest dummyForTestBasePostManager = new DummyBasePostManagerForTest(Factory, jobs);
			AssertEquals(1, dummyForTestBasePostManager.OnlyForTestListOfCharges.Length);
		}

		public void TestPreviewInvoiceRunValidationAfterCreatingLines()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var localClient = TestObjectCreator.CreateOrgHeader("ABC", true, true, true, true, true, true);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001002"));
			job.JH_OA_LocalChargesAddr = TestObjectCreator.CreateAddress(localClient).PK;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_IsActive = true;
			chargeCode.AC_ChargeType = "CMT";

			var charge = Factory.NewWithValidTestData<Charge>();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSSellAmt = 0;
			charge.JR_OH_SellAccount = localClient.PK;
			charge.JR_InvoiceType = "FIN";
			charge.JR_Desc = "Comment Charge";
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = TestObjectCreator.FIADepartment.PK;

			job.Charges.Add(charge);

			Factory.Save();

			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Revenue);

			AssertEquals("Precondition: Should found one transaction", 1, transactions.Values.Count);
			var previewInvoice = transactions.Values.Cast<TransactionHeader>().FirstOrDefault();
			AssertNoErrors("Should not have any error as CMT line is created with 0 amount", previewInvoice.AH_OSTotalAmountInfo);
		}

		public void TestNotTriggertJobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Shipment Job Charge", TestObjectCreator.EUR, 10M, TestObjectCreator.AALSHI, TestObjectCreator.EUR, 10M, TestObjectCreator.AALSHI);
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();

			charge.JR_LocalSellAmt = 100m;
			Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false;
			var postManager = new InvoicingPostManager(job);
			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			Assert(!job.NotificationsIncludingChildren.ContainsNotificationContaining(AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage));
		}

		public void TestRaiseDebtorsPECCodeIsNotDefinedError_WhenComplianceSubTypeIsEAR() =>
			TestRaiseDebtorsPECCodeIsNotDefinedError(@"An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)");

		public void TestRaiseDebtorsPECCodeIsNotDefinedError_WhenComplianceSubTypeIsNotEAR() =>
			TestRaiseDebtorsPECCodeIsNotDefinedError(@"A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)", true);

		void TestRaiseDebtorsPECCodeIsNotDefinedError(string expectedError, bool addVTECusCode = false)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC14, "charge1", TestObjectCreator.TRY, 100.3m, TestObjectCreator.CreditorTR, TestObjectCreator.TRY, 100m, TestObjectCreator.Debtor);
			charge.JR_OSSellExRate = 1m;
			charge.JR_AT_SellGSTRate = TestObjectCreator.KDV18.PK;
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(charge.JR_GB.ToGuid(), DateTime.Today.AddDays(-30)))
			{
				if (addVTECusCode)
				{
					TestObjectCreator.Debtor.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, "1234567890");
					TestObjectCreator.Debtor.Factory.Save();
				}
				var lastErrorReported = "";
				var postManager = new InvoicingPostManager(job);
				postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
				var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);

				AssertEquals("Precondition: Should found one transaction", 1, transactions.Values.Count);

				var invoice = transactions.Values.Cast<TransactionHeader>().FirstOrDefault();
				Assert(invoice.Notifications.ContainsNotificationContaining(expectedError));
				if (!addVTECusCode)
				{
					Assert(expectedError, lastErrorReported.Contains(expectedError));
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestReDistributeChargesWhileContainsSpecificInvoiceTypes()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var localClient = TestObjectCreator.CreateOrgHeader("ABC", true, true, true, true, true, true);
			localClient.OH_RL_NKClosestPort = "USLAX";
			localClient.OH_IsForwarder = true;
			localClient.MiscServ.OM_ARTreatDisbursementsAsStandardValue = 1000m;
			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = localClient.OH_RL_NKClosestPort;
			port.O5_IsHandlesAirAgent = ZBool.True;
			port.O5_AgentDirection = "BTH";
			localClient.AppointedAgentPorts.Add(port);

			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment, localClientOrg: localClient))
			{
				var chargeLocalClient = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10m, 10m);
				chargeLocalClient.JR_OH_SellAccount = localClient.PK;

				var postManager = GetPostManager(new Job[] { job });
				SetSendingForwarderAddressForConsol(localClient.MainAddress.PK);

				TransactionCreatorHashtable transactions = null;

				foreach (var invoiceType in ReDistributeChargesList)
				{
					postManager.reDistributeCharges_ForTest = false;
					chargeLocalClient.JR_InvoiceType = invoiceType;
					transactions = postManager.CreateTransactions(GetRevenuePostingOption());
					Assert($"Should Re-Distribute charges while invoice type is {invoiceType}", postManager.reDistributeCharges_ForTest);
					transactions.RemoveAndDeleteAll();
				}

				foreach (var invoiceType in NotReDistributeChargesList)
				{
					postManager.reDistributeCharges_ForTest = false;
					chargeLocalClient.JR_InvoiceType = invoiceType;
					transactions = postManager.CreateTransactions(GetRevenuePostingOption());
					Assert($"Should not Re-Distribute charges while invoice type is {invoiceType}", !postManager.reDistributeCharges_ForTest);
					transactions.RemoveAndDeleteAll();
				}
			}
		}

		public void TestCreateTransactions_CallingCheckJobConsolCostValidation()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var postManager = GetPostManager(new[] { job });

			AssertEquals("PreCondition", false, postManager.IsCallingCheckJobConsolCostValidation_ForTestOnly);
			postManager.CreateTransactions(JobInvoicingPostingOption.All);

			AssertEquals(true, postManager.IsCallingCheckJobConsolCostValidation_ForTestOnly);
		}

		public void TestCreateTransactions_WhenPostReceivablesCharges_WithAllowZeroValueARInvoices()
		{
			AssertCreateTransactions_WhenPostReceivablesCharges_WithZeroBalanceInvoice(true);
		}

		public void TestCreateTransactions_WhenPostReceivablesCharges_WithDisallowZeroValueARInvoices()
		{
			AssertCreateTransactions_WhenPostReceivablesCharges_WithZeroBalanceInvoice(false);
		}

		void AssertCreateTransactions_WhenPostReceivablesCharges_WithZeroBalanceInvoice(bool allowZeroValueARInvoices)
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, 0.6m);

			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "local charge 1", TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "local charge 2", TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, -100M, TestObjectCreator.LocalClient);

			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "local charge 3", TestObjectCreator.AUD, 0m, null, TestObjectCreator.USD, 120M, TestObjectCreator.LocalClient);
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "local charge 4", TestObjectCreator.AUD, 0m, null, TestObjectCreator.USD, -120M, TestObjectCreator.LocalClient);
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			Factory.Save();

			Assert("charge1 has a local invoice currency type", charge1.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			Assert("charge2 has a local invoice currency type", charge2.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			Assert("charge3 has a foreign invoice currency type", !charge3.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			Assert("charge4 has a foreign invoice currency type", !charge4.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));

			var postManager = new InvoicingPostManager(job);

			using (AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowZeroValueARInvoices))
			{
				var transactions = postManager.CreateTransactions(GetRevenuePostingOption());
				if (allowZeroValueARInvoices)
				{
					AssertEquals("Should have 2 transactions.", 2, transactions.Count);
				}
				else
				{
					AssertEquals("Should have 0 transactions.", 0, transactions.Count);
				}
			}
		}

		[MasterFiles.Business.Testing.SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestCheckJobConsolCostValidation()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S0001", consol);
			var job1 = TestObjectCreator.CreateJob(shipment1);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost.E6_InvoiceNum = "INV123";
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_AT_TaxRate = TestObjectCreator.FREEVAT.PK;
			consolCost.E6_ExchangeRate = 4m;
			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0001", TestObjectCreator.USD, 4m, TestObjectCreator.Creditor1);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			consolCost.E6_AH_APInvoice = invoice.PK;

			var charge1 = consolCost.ApportionmentCharges.FindChargeForJob(shipment1);
			charge1.JR_OH_SellAccount = TestObjectCreator.Debtor.PK;
			charge1.JR_OSCostExRate = 0m;

			var line1 = TestObjectCreator.CreateInvoiceLine(TransactionLineTypes.Cost, invoice, job1, TestObjectCreator.CC1, TestObjectCreator.USD, 4m, "", 100m, taxRate: TestObjectCreator.FREEVAT);
			charge1.Accrual.AL_ReverseDate = ZDateTime.Today;
			charge1.JR_AL_APLine = line1.PK;

			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPInvoice(invoice, invoice.Header.OH_Code, invoice.AH_TransactionNum);

			var lastErrorReported = "";
			var postManager = GetPostManager(new Job[] { job1 });
			postManager.OnCriticalPostError += (object sender, CriticalPostingErrorEventArgs e) => lastErrorReported = e.ErrorMessage;
			postManager.CheckJobConsolCostValidation_FortestOnly(transactions);
			AssertEquals("CancelPosting", true, postManager.CancelPosting);
			AssertEquals(
				"Local amount of apportion charge (Consol: C001, Charge Code: ZZCC1) cannot be zero when Overseas Cost amount is non zero. Please check relative Consol Cost exchange rate settings."
				, lastErrorReported);
		}

		#region TestAdjustPostedInvoiceHelper

		public void TestAdjustPostedInvoiceHelperType()
		{
			var basePostManager = GetPostManager(new Job[] { TestObjectCreator.Job1 });
			AssertType<AdjustPostedInvoiceHelper>(basePostManager.AdjustPostedInvoiceHelper_ExposedForTestOnly);
		}

		public void TestAdjustPostedInvoiceHelper_AddRoundingLine()
		{
			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false, true, TestObjectCreator.LocalClient);
			SetupDebtor(TestObjectCreator.LocalClient);
			CreateChargeForRounding(job);
			var postManager = GetPostManager(new[] { job });

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			var chargeCode = TestObjectCreator.CC1;
			var org = TestObjectCreator.LocalClient;
			taxTestHelper.SetupMinimumSettingsForTaxFramework(GlbCompany.CurrentCompany, org, chargeCode, LedgerTypes.AccountsReceivable);

			var taxProcessorMock = new Mock<ITaxProcessor>(MockBehavior.Strict);
			taxProcessorMock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("").Callback((ITaxRecordParent taxRecordParent) => taxRecordParent.OSTaxAmount = 23M);
			ObjectFactory.Substitute(taxProcessorMock.Object);

			var osTotalAmountBeforeRounding = 0M;

			var adjustPostedInvoiceHelper = new Mock<IAdjustPostedInvoiceHelper>(MockBehavior.Strict);
			adjustPostedInvoiceHelper.Setup(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>())).Callback((InvoicingBase invoice) => osTotalAmountBeforeRounding = invoice.AH_OSTotalAmount);
			postManager.SubstituteAdjustPostedInvoiceHelper_ForTestOnly(adjustPostedInvoiceHelper.Object);

			var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.All);
			adjustPostedInvoiceHelper.Verify(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>()), Times.Once);

			var osTotalAmountBeforeRoundingExpected = 133M; //133 = 110 + 23(other tax amount)
			AssertEquals(osTotalAmountBeforeRoundingExpected, osTotalAmountBeforeRounding);
		}

		public void TestAdjustPostedInvoiceHelper_AddRoundingLine_CancelPosting_True()
		{
			AssertAddRoundingLine_CancelPosting(true);
		}

		public void TestAdjustPostedInvoiceHelper_AddRoundingLine_CancelPosting_False()
		{
			AssertAddRoundingLine_CancelPosting(false);
		}

		void AssertAddRoundingLine_CancelPosting(bool cancelPosting)
		{
			var adjustPostedInvoiceHelper = new Mock<IAdjustPostedInvoiceHelper>(MockBehavior.Strict);
			adjustPostedInvoiceHelper.Setup(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>()));

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false, true, TestObjectCreator.LocalClient);
			SetupDebtor(TestObjectCreator.LocalClient);
			CreateChargeForRounding(job);

			var postManager = GetPostManager(new[] { job });
			postManager.SubstituteAdjustPostedInvoiceHelper_ForTestOnly(adjustPostedInvoiceHelper.Object);

			if (cancelPosting)
			{
				postManager.CheckForCriticalErrors_ForTestOnly = _ => true;
			}

			postManager.CreateTransactions(JobInvoicingPostingOption.All);
			adjustPostedInvoiceHelper.Verify(x => x.AdjustPostedInvoice(It.IsAny<InvoicingBase>()), Times.Exactly(cancelPosting ? 0 : 1));
		}

		protected virtual Charge CreateChargeForRounding(Job job)
		{
			return TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.AUD, osSellAmt: 100M, debtor: TestObjectCreator.LocalClient);
		}

		#endregion

		public void TestCheckForCriticalErrors_AddErrorIfInvoiceDateIsInTheFuture()
		{
			AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var shipment = TestObjectCreator.CreateShipment("S001001", false);
			shipment.JS_E_DEP = ZDateTime.Today.AddDays(1);
			shipment.JS_E_ARV = ZDateTime.Today.AddDays(1);

			var job = TestObjectCreator.CreateJob(shipment, false);

			var debtor = TestObjectCreator.Debtor;
			SetupDebtor(debtor);
			var arTerm = debtor.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm.PY_InvoiceTerm = Constants.InvoiceTerms.FromShipmentDate;
			arTerm.PY_InvoiceDays = 2;

			TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "", null, 0M, null, TestObjectCreator.AUD, 100M, debtor);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var jobInNewFactory = newFactory.Load<Job>(job.PK);
			var postManager = GetPostManager(new[] { jobInNewFactory });
			postManager.OnCriticalPostError += PostManager_OnCriticalPostError;
			postManager.CreateTransactions(GetRevenuePostingOption()).GetAllARTransactions().FirstOrDefault();

			void PostManager_OnCriticalPostError(object sender, CriticalPostingErrorEventArgs e)
			{
				var header = ((CriticalTransactionPostingErrorEventArgs)e).Header;
				AssertHasRowError(header, $@"Job=S001001, Debtor=ZDebtor, Invoice Date={ZDateTime.Today.AddDays(1)}, Term=SHP+2
The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.");
			}
		}

		#region Implementation

		protected virtual string[] ReDistributeChargesList
		{
			get
			{
				return AccTransactionHeader.DisbursementInvoiceTypes.Where(x => !InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Contains(x)).ToArray();
			}
		}

		protected virtual string[] NotReDistributeChargesList
		{
			get
			{
				var allInvoiceTypes = new InvoiceTypesList();
				return allInvoiceTypes.Cast<CodeDescriptionPair>().Select(x => x.Code).
					Where(x => InvoiceTypeCalculationProvider.DeferredInvoiceTypes.Contains(x) || !AccTransactionHeader.DisbursementInvoiceTypes.Contains(x)).ToArray();
			}
		}

		protected virtual void SetSendingForwarderAddressForConsol(ZGuid addressPK) { }

		protected abstract BasePostManager GetPostManager(IEnumerable<Job> jobs, GlbBranch taxBranch = null);

		protected virtual JobInvoicingPostingOption GetRevenuePostingOption()
		{
			return JobInvoicingPostingOption.Revenue;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		protected TestObjectCreator testObjectCreator;

		protected virtual void SetupDebtor(OrgHeader debtor)
		{
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion
	}

	public class InterruptPostingExceptionTest : TestCase
	{
		public void TestInterruptPostingException_Message()
		{
			try
			{
				throw new InterruptPostingException();
			}
			catch (Exception ex)
			{
				AssertEquals(string.Empty, ex.Message);
			}

			var errorMessage = "ERROR!!!";
			try
			{
				throw new InterruptPostingException(errorMessage);
			}
			catch (Exception ex)
			{
				AssertEquals(errorMessage, ex.Message);
			}

			try
			{
				var exception = new Exception();
				throw new InterruptPostingException(errorMessage, exception);
			}
			catch (Exception ex)
			{
				AssertEquals(errorMessage, ex.Message);
			}
		}
	}
}
