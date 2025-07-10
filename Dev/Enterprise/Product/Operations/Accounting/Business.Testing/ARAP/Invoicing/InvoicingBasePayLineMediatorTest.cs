using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBasePayLineMediator))]
	internal class InvoicingBasePayLineMediatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLines()
		{
			AssertEquals(0m, mediator.Lines[0].PaidAmountInChargeCurrency);
			AssertEquals(0m, mediator.Lines[1].PaidAmountInChargeCurrency);
		}

		public void TestLineTotalPaidAmount()
		{
			mediator.Lines[0].PaidAmount = 10m;
			mediator.Lines[1].PaidAmount = 20m;
			AssertEquals(30m, mediator.LineTotalPaidAmount);
		}

		public void TestLineTotalLocalPaidAmount()
		{
			mediator.Lines[0].PaidAmount = 10m;
			mediator.Lines[1].PaidAmount = 20m;
			AssertEquals(30m, mediator.LineTotalLocalPaidAmount);
		}

		[TestDate(2019, 05, 15)]
		public void TestOutstandingAmountInvoice_Decreased_BySecondPayment_With_Paylines()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			// 1. Create an AP invoice with more than 3 lines
			BusinessObjectFactory businessObjectFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreator = new TestObjectCreator(businessObjectFactory);
			testObjectCreator.GLHeader1.AG_Description = "Test";
			businessObjectFactory.Save();

			AccBankAccount accBankAccount = testObjectCreator.AUDBankAccount;
			AccChequeBook accChequeBook = testObjectCreator.AUDChequeBook;
			OrgHeader orgHeader = testObjectCreator.AALSHI;
			RefCurrency refCurrency = testObjectCreator.AUD;

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), refCurrency, 1m, orgHeader, ZDateTime.Now);
			var line1 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 209.96m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line2 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 598.01m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line3 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 301.80m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line4 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 231.99m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line5 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 440.96m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line6 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 397.90m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line7 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 299.87m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			var line8 = testObjectCreator.CreateInvoiceLine(invoice, refCurrency, 1m, 90.55m, 0m, 0m, testObjectCreator.GLHeader1.PK);
			AssertNoExceptionThrown(() => businessObjectFactory.Save());

			// 2. Create an AP payment with lesser amount than the invoice and partially pay against the invoice
			APPaymentApprovalWithoutAuthorisation paymentApproval = testObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithoutAuthorisation), "PAY", accBankAccount, accChequeBook) as APPaymentApprovalWithoutAuthorisation;
			paymentApproval.AV_OH = orgHeader.PK;
			paymentApproval.AV_Amount = 553.21m;
			paymentApproval.AV_ChequeOrReference = ReceiptTypes.Cash;
			paymentApproval.AV_PaymentType = ReceiptTypes.Cash;

			IMatchingCollection matchingCollectionToForceCorrectValidationClassForInvoice = new IMatchingCollection(businessObjectFactory, MatchingCollectionTypes.MatchedTransactions);
			matchingCollectionToForceCorrectValidationClassForInvoice.Add(invoice);
			MatchingBase matchingBase = paymentApproval.PaymentMatchingBaseObject;
			matchingBase.AddIMatching(invoice);
			matchingBase.AddIMatching(paymentApproval);
			matchingBase.MatchedTransactions.SetPartialPaidAmount();

			InvoicingBasePayLineMediator payLineMediator = new InvoicingBasePayLineMediator(matchingBase, invoice);
			payLineMediator.Lines[0].PaidAmount = -160.86m;
			payLineMediator.Lines[1].PaidAmount = 0m;
			payLineMediator.Lines[2].PaidAmount = -301.80m;
			payLineMediator.Lines[2].IsFullyPay = true;
			payLineMediator.Lines[3].PaidAmount = 0m;
			payLineMediator.Lines[4].PaidAmount = 0m;
			payLineMediator.Lines[5].PaidAmount = 0m;
			payLineMediator.Lines[6].PaidAmount = 0m;
			payLineMediator.Lines[7].PaidAmount = -90.55m;
			payLineMediator.Lines[7].IsFullyPay = true;
			payLineMediator.ConveyData();

			paymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			paymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions();

			AssertEquals(-553.21m, payLineMediator.LineTotalLocalPaidAmount);

			// 3.Save
			AssertNoExceptionThrown(() => businessObjectFactory.Save());

			// Assert: Outstanding amount of the invoice is decreased by the payment amount
			var businessObjectFactoryForPayment2 = new BusinessObjectFactory();
			var invoiceLoaded = businessObjectFactoryForPayment2.Load<APInvoice>(invoice.PK);
			AssertEquals(-2017.83m, invoiceLoaded.AH_OutstandingAmount);

			// 4.Create another AP payment and again partially pay against the same invoice
			TestObjectCreator testObjectCreatorForPayment2 = new TestObjectCreator(businessObjectFactoryForPayment2);
			APPaymentApprovalWithoutAuthorisation paymentApproval2 = businessObjectFactoryForPayment2.New<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval2.AV_PaymentType = "PAY";
			paymentApproval2.AV_AB = accBankAccount.PK;
			paymentApproval2.AV_AK = accChequeBook.PK;
			paymentApproval2.AV_OH = orgHeader.PK;
			paymentApproval2.AV_Amount = 549.88m;
			paymentApproval2.AV_ChequeOrReference = ReceiptTypes.Cash;
			paymentApproval2.AV_PaymentType = ReceiptTypes.Cash;

			IMatchingCollection matchingCollectionPayment2 = new IMatchingCollection(businessObjectFactoryForPayment2, MatchingCollectionTypes.MatchedTransactions);
			matchingCollectionPayment2.Add(invoiceLoaded);
			MatchingBase matchingBasePayment2 = paymentApproval2.PaymentMatchingBaseObject;
			matchingBasePayment2.AddIMatching(invoiceLoaded);
			matchingBasePayment2.AddIMatching(paymentApproval2);
			matchingBasePayment2.MatchedTransactions.SetPartialPaidAmount();

			InvoicingBasePayLineMediator payLineMediatorPayment2 = new InvoicingBasePayLineMediator(matchingBasePayment2, invoiceLoaded);
			payLineMediatorPayment2.Lines[0].PaidAmount = 0m;
			payLineMediatorPayment2.Lines[1].PaidAmount = -18.02m;
			payLineMediatorPayment2.Lines[2].PaidAmount = 0m;
			payLineMediatorPayment2.Lines[3].IsFullyPay = true;
			payLineMediatorPayment2.Lines[4].PaidAmount = 0m;
			payLineMediatorPayment2.Lines[5].PaidAmount = 0m;
			payLineMediatorPayment2.Lines[6].IsFullyPay = true;
			payLineMediatorPayment2.Lines[7].PaidAmount = 0m;
			payLineMediatorPayment2.ConveyData();

			paymentApproval2.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			paymentApproval2.PaymentMatchingBaseObject.MatchAndClearTransactions();

			AssertEquals(-549.88m, payLineMediatorPayment2.LineTotalLocalPaidAmount);

			// 5.Save
			AssertNoExceptionThrown(() => businessObjectFactoryForPayment2.Save());

			// We will also assert that the outstanding amount of the invoice has decreased by the second payment.
			var businessObjectFactoryForPayment3 = new BusinessObjectFactory();
			var invoiceLoaded2 = businessObjectFactoryForPayment3.Load<APInvoice>(invoice.PK);
			AssertEquals(-1467.95m, invoiceLoaded2.AH_OutstandingAmount);
		}

		public void TestLineTotalLocalPaidAmount_With_Changing_PaidAmountInChargeCurrency_And_Having_ExchangeRate()
		{
			//Simulate WI00032217
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			MatchingBase matchingBase = new ARPaymentApprovalMatching(Factory, paymentApproval);
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_ExchangeRate = 1.8448m;
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line3 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line4 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line5 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line6 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line7 = (APInvoiceLine)invoice.Lines.AddNew();
			var mediatorWithExchangeRate = new InvoicingBasePayLineMediator(matchingBase, invoice);

			mediatorWithExchangeRate.Lines[0].PaidAmount = 7;
			mediatorWithExchangeRate.Lines[1].PaidAmount = 7.86m;
			mediatorWithExchangeRate.Lines[2].PaidAmount = 5m;
			mediatorWithExchangeRate.Lines[3].PaidAmount = 22.48m;
			mediatorWithExchangeRate.Lines[4].PaidAmount = 20m;
			mediatorWithExchangeRate.Lines[5].PaidAmount = 50m;
			mediatorWithExchangeRate.Lines[6].PaidAmount = 45m;

			AssertEquals(85.29m, mediatorWithExchangeRate.LineTotalLocalPaidAmount);
		}

		public void TestConveyData()
		{
			new IMatchingCollection(Factory) { mediator.MasterInvoice };
			mediator.Lines[0].PaidAmount = 10m;
			mediator.Lines[1].PaidAmount = 20m;
			AssertEquals(30m, mediator.LineTotalLocalPaidAmount);
			AssertEquals(30m, mediator.LineTotalPaidAmount);
			AssertEquals(0m, ((ISupportMatchingOfMyLines)mediator.MasterInvoice).LineTotalPaidAmount);
			AssertEquals(0m, ((ISupportMatchingOfMyLines)mediator.MasterInvoice).LineTotalLocalPaidAmount);
			mediator.ConveyData();
			AssertEquals(30m, ((ISupportMatchingOfMyLines)mediator.MasterInvoice).LineTotalPaidAmount);
			AssertEquals(30m, ((ISupportMatchingOfMyLines)mediator.MasterInvoice).LineTotalLocalPaidAmount);
			AssertEquals(10m, mediator.Lines[0].OriginalPaidAmount);
			AssertEquals(20m, mediator.Lines[1].OriginalPaidAmount);
		}

		public void TestCancelEdits()
		{
			AssertEquals(0m, mediator.Lines[0].PaidAmount);
			AssertEquals(0m, mediator.Lines[1].PaidAmount);
			mediator.Lines[0].PaidAmount = 10m;
			mediator.Lines[1].PaidAmount = 20m;
			AssertEquals(10m, mediator.Lines[0].PaidAmount);
			AssertEquals(20m, mediator.Lines[1].PaidAmount);
			mediator.CancelEdits();
			AssertEquals(0m, mediator.Lines[0].PaidAmount);
			AssertEquals(0m, mediator.Lines[1].PaidAmount);
		}

		public void TestSetSelectedLinesToFullyPaid()
		{
			Assert(mediator.Lines[0].IsFullyPay);
			Assert(mediator.Lines[1].IsFullyPay);
			mediator.SetSelectedLinesToFullyPaid(mediator.Lines[0], false);
			Assert(!mediator.Lines[0].IsFullyPay);
			Assert(mediator.Lines[1].IsFullyPay);
		}

		public void TestSetAllLinesToFullyPaid()
		{
			Assert(mediator.Lines[0].IsFullyPay);
			Assert(mediator.Lines[1].IsFullyPay);
			mediator.SetAllLinesToFullyPaid(false);
			Assert(!mediator.Lines[0].IsFullyPay);
			Assert(!mediator.Lines[1].IsFullyPay);
		}

		public void TestSetAllLinesMatchingFiltersToFullyPaid()
		{
			Assert(mediator.Lines[0].IsFullyPay);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			mediator.ChargeCodeFilter = chargeCode.PK;
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(!mediator.Lines[0].IsFullyPay);
			((APInvoiceLine)mediator.Lines[0]).AL_AC = chargeCode.PK;
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(mediator.Lines[0].IsFullyPay);

			mediator.ChargeGroupFilter = "TST";
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(!mediator.Lines[0].IsFullyPay);
			chargeCode.AC_ChargeGroup = "TST";
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(mediator.Lines[0].IsFullyPay);

			mediator.ChargeTypeFilter = "TST";
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(!mediator.Lines[0].IsFullyPay);
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_AL_APLine = ((APInvoiceLine)mediator.Lines[0]).PK;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_ChargeType = "TST";
			jobCharge.JR_RX_NKCostCurrency = "USD";
			AccountingConfigurationRegistry.Instance.PopupImportAccrualsScreenOnAPInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			((APInvoiceLine)mediator.Lines[0]).AL_JH = jobHeader.PK;
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(mediator.Lines[0].IsFullyPay);

			mediator.ChargeCurrencyFilter = "AUD";
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(!mediator.Lines[0].IsFullyPay);
			jobCharge.JR_RX_NKCostCurrency = "AUD";
			mediator.SetAllLinesMatchingFiltersToFullyPaid();
			Assert(mediator.Lines[0].IsFullyPay);
		}

		InvoicingBasePayLineMediator mediator;

		protected override void SetUp()
		{
			base.SetUp();
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			MatchingBase matchingBase = new ARPaymentApprovalMatching(Factory, paymentApproval);
			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			APInvoiceLine line2 = (APInvoiceLine)invoice.Lines.AddNew();
			mediator = new InvoicingBasePayLineMediator(matchingBase, invoice);
			AssertEquals(2, mediator.Lines.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			MatchingBase matchingBase = new ARPaymentApprovalMatching(Factory, paymentApproval);
			APInvoice invoice = Factory.New<APInvoice>();
			return new InvoicingBasePayLineMediator(matchingBase, invoice);
		}
	}
}
