using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(IMatchingCollection))]
	public class IMatchingCollectionTestCase : BusinessObjectCollectionTestCase
	{
		#region TestGetOrganizationBalanceAmount

		public void TestGetOrganizationBalanceAmount()
		{
			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 380.58m, 0m, 0m, 380.58m, 0m, 0m);

			APCreditNote testAPCrd = Factory.NewWithValidTestData<APCreditNote>();
			testAPCrd.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(testAPCrd, testAPCrd.TransactionCurrency, testAPCrd.AH_ExchangeRate, 409.79m, 0m, 0m, 409.79m, 0m, 0m);

			APReceipt testAPRec = Factory.NewWithValidTestData<APReceipt>();
			testAPRec.AH_OH = TestOrg.PK;
			testAPRec.AH_LocalExTaxAmount = 190.56M;
			testAPRec.AH_OSTotalAmount = 190.56M;

			APPayment testAPPay = Factory.NewWithValidTestData<APPayment>();
			testAPPay.AH_OH = TestOrg2.PK;
			testAPPay.AH_LocalExTaxAmount = 90.78M;
			testAPPay.AH_OSTotalAmount = 90.78M;

			Factory.Save();

			TestTransactions.Add(testAPInv);
			TestTransactions.Add(testAPCrd);
			TestTransactions.Add(testAPRec);
			TestTransactions.Add(testAPPay);
			TestTransactions.SetPartialPaidAmount();

			AssertEquals("Should be the balance for TestOrg", -571.14M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsPayable));
			AssertEquals("Should be the balance for TestOrg2", 500.57M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsPayable));
			AssertEquals("Should be zero since there are no AR Transactions", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsReceivable));
			AssertEquals("Should be zero since there are no AR Transactions", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsReceivable));
		}

		#endregion

		#region TestGetOrganizationBalanceAmountWithTransfer

		public void TestGetOrganizationBalanceAmountWithTransfer()
		{
			APInvoice testAPInv = Factory.New<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 39.87M;
			testAPInv.AH_OSTotalAmount = 39.87M;

			APTransfer testAPTransfer = (APTransfer)Transfer.New(typeof(APTransfer), Factory);

			testAPTransfer.TransferFrom.AH_OH = TestOrg.PK;
			testAPTransfer.TransferFrom.AH_InvoiceAmount = -30M;
			testAPTransfer.TransferFrom.AH_OutstandingAmount = -30M;
			testAPTransfer.TransferFrom.AH_OSTotal = -30M;
			testAPTransfer.TransferTo.AH_OH = TestOrg2.PK;
			testAPTransfer.TransferTo.AH_InvoiceAmount = 30M;
			testAPTransfer.TransferTo.AH_OutstandingAmount = 30M;
			testAPTransfer.TransferTo.AH_OSTotal = 30M;

			APReceipt testAPRec = Factory.New<APReceipt>();
			testAPRec.AH_OH = TestOrg2.PK;
			testAPRec.AH_LocalExTaxAmount = 60.88M;
			testAPRec.AH_OSTotalAmount = 60.88M;

			TestTransactions.Add(testAPInv);
			TestTransactions.Add(testAPTransfer.TransferFrom);
			TestTransactions.Add(testAPTransfer.TransferTo);
			TestTransactions.Add(testAPRec);
			TestTransactions.SetPartialPaidAmount();

			AssertEquals("Should be the balance for TestOrg", -69.87M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsPayable));
			AssertEquals("Should be the balance for TestOrg2", -30.88M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsPayable));
			AssertEquals("Should be zero since there are no AR Transactions", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsReceivable));
			AssertEquals("Should be zero since there are no AR Transactions", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsReceivable));
		}

		#endregion

		#region TestGetOrganizationBalanceAmountWithContra

		public void TestGetOrganizationBalanceAmountWithContra()
		{
			APInvoice testAPInv = Factory.New<APInvoice>();
			testAPInv.AH_OH = TestOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 14.56M;
			testAPInv.AH_OSTotalAmount = 14.56M;

			ARInvoice testARInv = Factory.New<ARInvoice>();
			testARInv.AH_OH = TestOrg2.PK;
			testARInv.AH_LocalExTaxAmount = 54.32M;
			testARInv.AH_OSTotalAmount = 54.32M;

			Contra testContra = Contra.New(Factory);

			testContra.APRow.AH_OH = TestOrg.PK;
			testContra.APRow.AH_LocalExTaxAmount = 34.56M;
			testContra.APRow.AH_OSTotalAmount = 34.56M;
			testContra.ARRow.AH_OH = TestOrg2.PK;
			testContra.ARRow.AH_LocalExTaxAmount = 34.56M;
			testContra.ARRow.AH_OSTotalAmount = 34.56M;

			TestTransactions.Add(testAPInv);
			TestTransactions.Add(testARInv);
			TestTransactions.Add(testContra);
			TestTransactions.SetPartialPaidAmount();

			// Contra in Database - subtracts from AR and adds to AP
			AssertEquals("Balance for TestOrg in AR should be 0", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsReceivable));
			AssertEquals("Balance for TestOrg in AP should be 20", 20M, TestTransactions.GetOrganizationBalanceAmount(TestOrg.PK, LedgerTypes.AccountsPayable));
			AssertEquals("Balance for TestOrg2 in AR should be 19.76", 19.76M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsReceivable));
			AssertEquals("Balance for TestOrg2 in AP should be 0", 0M, TestTransactions.GetOrganizationBalanceAmount(TestOrg2.PK, LedgerTypes.AccountsPayable));
		}

		#endregion

		#region TestGetLedgerBalanceAmount

		public void TestGetLedgerBalanceAmount()
		{
			APInvoice testAPInvoice = Factory.New<APInvoice>();
			testAPInvoice.AH_OH = TestOrg.PK;
			testAPInvoice.AH_LocalExTaxAmount = 39.73M;
			testAPInvoice.AH_OSTotalAmount = 39.73M;

			Contra testContra = Contra.New(Factory);
			testContra.APRow.AH_OH = TestOrg.PK;
			testContra.APRow.AH_LocalExTaxAmount = 87.23M;
			testContra.APRow.AH_OSTotalAmount = 87.23M;
			testContra.ARRow.AH_OH = TestOrg2.PK;
			testContra.ARRow.AH_LocalExTaxAmount = 87.23M;
			testContra.ARRow.AH_OSTotalAmount = 87.23M;

			ARTransfer testARTransfer = (ARTransfer)Transfer.New(typeof(ARTransfer), Factory);
			testARTransfer.TransferFrom.AH_OH = TestOrg2.PK;
			testARTransfer.TransferFrom.AH_LocalExTaxAmount = 51.09M;
			testARTransfer.TransferFrom.AH_OSTotalAmount = 51.09M;
			testARTransfer.TransferTo.AH_OH = TestOrg.PK;
			testARTransfer.TransferTo.AH_LocalExTaxAmount = 51.09M;
			testARTransfer.TransferTo.AH_OSTotalAmount = 51.09M;

			TestTransactions.Add(testAPInvoice);
			TestTransactions.Add(testARTransfer);
			TestTransactions.Add(testContra);
			TestTransactions.SetPartialPaidAmount();
			AssertEquals("Balance of AR should be -87.23", -87.23M, TestTransactions.GetLedgerBalanceAmount(LedgerTypes.AccountsReceivable));
			AssertEquals("Balance of AP should be 47.5", 47.5M, TestTransactions.GetLedgerBalanceAmount(LedgerTypes.AccountsPayable));
		}

		#endregion

		#region TestContainsTransactionFromSpecifiedOrg

		public void TestContainsTransactionFromSpecifiedOrg()
		{
			APInvoice testAPINV = Factory.NewWithValidTestData<APInvoice>();
			testAPINV.AH_OH = TestOrg.PK;

			TestTransactions.Add(testAPINV);

			Assert("Contains TestOrg", TestTransactions.ContainsTransactionFromSpecifiedOrg(TestOrg));
			Assert("Does not contain TestOrg2", !TestTransactions.ContainsTransactionFromSpecifiedOrg(TestOrg2));
		}

		#endregion

		#region TestContainsBusinessObjects

		public void TestContainsBusinessObjects()
		{
			ARInvoice aRINV1 = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice aRINV2 = Factory.NewWithValidTestData<ARInvoice>();

			TestTransactions.Add(aRINV1);
			TestTransactions.Add(aRINV2);

			BusinessObject[] bizOs = new BusinessObject[] { aRINV1, aRINV2 };

			Assert("Test Transactions should contain ARINV1 and ARINV2", TestTransactions.ContainsBusinessObjects(bizOs));
		}

		#endregion

		#region TestTransactionsMustBeMatched

		public void TestTransactionsMustBeMatched()
		{
			APInvoice testAPInv = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARInvoice testARInv = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			Factory.Save();

			TestTransactions.AddTransactionThatMustBeMatched(testAPInv);
			TestTransactions.Add(testARInv);
			Assert("Collection should contain TestAPInv", TestTransactions.Contains(testAPInv));
			Assert("Internal collection should contain TestAPInv", TestTransactions.MustBeMatched_ForTestOnly.Contains(testAPInv));
			Assert("Internal collection should not contain TestARInv", !TestTransactions.MustBeMatched_ForTestOnly.Contains(testARInv));

			TestTransactions.Remove(testAPInv);
			Assert("Collection should contain TestAPInv", TestTransactions.Contains(testAPInv));
			Assert("Internal collection should contain TestAPInv", TestTransactions.MustBeMatched_ForTestOnly.Contains(testAPInv));
			Assert("Collection should contain TestARInv", TestTransactions.Contains(testARInv));

			TestTransactions.RemoveAll();
			AssertEquals("Collection should contain 1 object", 1, TestTransactions.Count);
			Assert("Collection should contain TestAPInv", TestTransactions.Contains(testAPInv));

			TestTransactions.Remove(null);
			AssertEquals("Collection should contain 1 object", 1, TestTransactions.Count);
			Assert("Collection should contain TestAPInv", TestTransactions.Contains(testAPInv));
		}

		#endregion

		#region TestMustTransactionBeMatched

		public void TestMustTransactionBeMatched()
		{
			APInvoice testAPInv = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			Assert("Transaction is not a must have", !TestTransactions.MustTransactionBeMatched(testAPInv));

			TestTransactions.AddTransactionThatMustBeMatched(testAPInv);
			Assert("TestAPInv is a must have", TestTransactions.MustTransactionBeMatched(testAPInv));
		}

		#endregion

		#region TestIsSettingChequeNumReadOnlyOnAdd

		public void TestIsSettingChequeNumReadOnlyOnAdd()
		{
			Collection.IsSettingChequeNumReadOnlyOnAdd = false;
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			((IMatching)aPPay).ChequeOrReference_ReadOnly = false;
			Collection.Add(aPPay);
			Assert("AH_ChequeOrReference should not be readonly", !aPPay.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("ChequeOrReference should not be readonly", !((IMatching)aPPay).ChequeOrReferenceInfo.ReadOnly);

			Collection.IsSettingChequeNumReadOnlyOnAdd = true;
			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			((IMatching)aRRec).ChequeOrReference_ReadOnly = false;
			Collection.Add(aRRec);
			Assert("AH_ChequeOrReference should be readonly", aRRec.AH_ChequeOrReferenceInfo.ReadOnly);
			Assert("ChequeOrReference should be readonly", ((IMatching)aRRec).ChequeOrReferenceInfo.ReadOnly);
		}

		#endregion

		#region Test RemoveAllFromAllCollections

		public void TestRemoveAllFromAllCollections()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			Collection.Add(aPPay);

			Assert("Precondition: Collection should contain APPayment", Collection.Contains(aPPay));
			Collection.RemoveAllFromAllCollections();
			Assert("Collection should not contain APPayment", !Collection.Contains(aPPay));

			Collection.AddTransactionThatMustBeMatched(aPPay);
			Assert("Collection should contain APPay", Collection.Contains(aPPay));
			Assert("MustBeMatched should contain APPay", Collection.MustBeMatched_ForTestOnly.Contains(aPPay));
			Collection.RemoveAllFromAllCollections();
			Assert("Collection should not contain APPay", !Collection.Contains(aPPay));
			Assert("MustBeMatched should not contain APPay", !Collection.MustBeMatched_ForTestOnly.Contains(aPPay));
		}

		#endregion

		#region TestRemoveBusinessElement

		public void TestRemoveBusinessElement()
		{
			APPayment apPay1 = Factory.NewWithValidTestData<APPayment>();
			APPayment apPay2 = Factory.NewWithValidTestData<APPayment>();
			APPayment apPay3 = Factory.NewWithValidTestData<APPayment>();
			Collection.Add(apPay1);
			Collection.Add(apPay2);

			Assert("Precondition: Collection should contain APPayment", Collection.Contains(apPay2));
			Assert("Precondition: Collection should not contain APPayment", !Collection.Contains(apPay3));
			AssertNoExceptionThrown("Collection should not throw Exception When Retrieve APPayment which not belongs to Collection", () => Collection.Remove(apPay3));
		}

		#endregion

		#region TestAddMatchingBusinessObject

		public void TestAddMatchingBusinessObject()
		{
			APPayment apPay1 = Factory.NewWithValidTestData<APPayment>();
			APPayment apPay2 = Factory.NewWithValidTestData<APPayment>();
			JobCharge chrg = Factory.NewWithValidTestData<JobCharge>();

			Collection.Add(apPay1);
			Collection.Add(apPay2);

			AssertEquals("Precondition: Collection Count should be 2", 2, Collection.Count);
			Collection.Add(chrg);
			AssertEquals("Collection Count Should Still Be 2 Because JobCharge Not Added to Collection", 2, Collection.Count);
		}

		#endregion

		#region ContainsTransactionWithSpecifiedCurrency Test

		public void TestContainsTransactionWithSpecifiedCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			ARInvoice aRInv_LocalCurr = Factory.NewWithValidTestData<ARInvoice>();
			aRInv_LocalCurr.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Collection.Add(aRInv_LocalCurr);
			Assert("Collection does not contain transactions for specified currency", !Collection.ContainsTransactionWithSpecifiedCurrency(currency.RX_Code));
			Collection.Add(aRInv);
			Assert("Collection does contain transaction for the specified currency", Collection.ContainsTransactionWithSpecifiedCurrency(currency.RX_Code));
		}

		#endregion

		#region TotalOSAmountForSpecifiedCurrencyAndTransactionType Test

		public void TestTotalOSAmountForSpecifiedCurrencyAndTransactionType()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			ARInvoice aRInv = GetARInvoice(currency, 1m, 70m, 34m);
			ARCreditNote aRCrd = GetARCreditNote(currency, 1m, 30m, -20m);
			ARCreditNote aRCrd2 = GetARCreditNote(GlbCompany.CurrentCompany.LocalCurrency, 1m, 100m, -90m);

			Collection.Add(aRInv);
			Collection.Add(aRCrd);
			Collection.Add(aRCrd2);

			AssertEquals("Total amount for Currency and Invoice", 34m, Collection.TotalOSAmountForSpecifiedCurrencyAndTransactionType(currency.RX_Code, TransactionTypes.Invoice));
			AssertEquals("Total amount for Currency and CreditNote", -20m, Collection.TotalOSAmountForSpecifiedCurrencyAndTransactionType(currency.RX_Code, TransactionTypes.CreditNote));
			AssertEquals("Total amount for Local Currency and CreditNote", -90m, Collection.TotalOSAmountForSpecifiedCurrencyAndTransactionType(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TransactionTypes.CreditNote));
			AssertEquals("Total amount for Currency and Receipt", 0m, Collection.TotalOSAmountForSpecifiedCurrencyAndTransactionType(currency.RX_Code, TransactionTypes.Receipt));
		}

		#endregion

		public void TestCalculateBalanceWithBalanceSuspended()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			APInvoice aPInv = GetAPInvoice(creator.AUD, 1m, 180m, -180m);
			Collection.Add(aPInv);

			AssertEquals("Collection balance", -180m, Collection.Balance);

			using (IDisposable suspender = Collection.SuspendBalanceCalculation())
			{
				AssertEquals("Balance calculation should become zero", 0m, Collection.Balance);
			}
			AssertEquals("Balance should be back to 180", -180m, Collection.Balance);
		}

		public void TestTotalLocalAmountForSpecifiedCurrency()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();

			ARReceipt aRRec_Curr1 = GetARReceipt(currency1, 4m, 100m, -80m);    // local = -20
			ARCreditNote aRCrd_Curr1 = GetARCreditNote(currency1, 5, 200m, -140m);  // local = -28

			ARInvoice aRInv_Curr2 = GetARInvoice(currency2, 5m, 200m, 190m);    // local = 38
			APInvoice aPInv_Curr2 = GetAPInvoice(currency2, 6m, 180m, -144m);   // local = -24

			Collection.Add(aRRec_Curr1);
			Collection.Add(aRCrd_Curr1);
			Collection.Add(aRInv_Curr2);
			Collection.Add(aPInv_Curr2);

			AssertEquals("LocalTotal for Currency1 should be -48", -48m, Collection.TotalLocalAmountForSpecifiedCurrency(currency1.RX_Code));
			AssertEquals("LocalTotal for Currency2 should be 14", 14m, Collection.TotalLocalAmountForSpecifiedCurrency(currency2.RX_Code));
		}

		public void TestTotalOSAmountForSpecifiedCurrency()
		{
			RefCurrency currency1 = Factory.NewWithValidTestData<RefCurrency>();
			RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();

			ARInvoice aRInv_Curr1 = GetARInvoice(currency1, 2m, 100m, 150m);
			APInvoice aPInv_Curr1 = GetAPInvoice(currency1, 2m, 900m, -400m);
			ARReceipt aRRec_Curr2 = GetARReceipt(currency2, 3m, 600m, -60m);

			Collection.Add(aRInv_Curr1);
			Collection.Add(aPInv_Curr1);
			Collection.Add(aRRec_Curr2);

			AssertEquals("OS Balance for Currency1 should be -250", -250m, Collection.TotalOSAmountForSpecifiedCurrency(currency1.RX_Code));
			AssertEquals("OS Balance for Currency2 should be -60", -60m, Collection.TotalOSAmountForSpecifiedCurrency(currency2.RX_Code));
			AssertEquals("OS Balance for local currency should be 0", 0m, Collection.TotalOSAmountForSpecifiedCurrency(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
		}

		public void TestCountOfPayments()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			ARPayment aRPay = Factory.NewWithValidTestData<ARPayment>();
			APPaymentApprovalWithAuthorisation aRPay2 = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();

			AssertEquals("Count must be 0", 0, Collection.CountOfPayments);

			Collection.Add(aRPay);
			Collection.Add(aRInv);
			AssertEquals("Count must be 1", 1, Collection.CountOfPayments);

			Collection.Add(aRPay2);
			AssertEquals("Count must be 2", 2, Collection.CountOfPayments);
		}

		public void TestPayForPartlyPaidInvoiceWithPaymentApproval()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			APInvoice invoiceInNewFactory = testObjectCreatorInNewFactory.CreateAPInvoice<APInvoice>("INV", testObjectCreatorInNewFactory.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M);
			invoiceInNewFactory.AH_OH = testObjectCreatorInNewFactory.AALSHI.PK;
			invoiceInNewFactory.AH_OutstandingAmount = -100M;
			APPaymentApprovalWithAuthorisation paymentInNewFactory = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			paymentInNewFactory.AV_Amount = 50M;
			paymentInNewFactory.AV_AH = invoiceInNewFactory.PK;
			paymentInNewFactory.AV_OH = invoiceInNewFactory.AH_OH;
			PaymentApprovalItem paymentItemInNewFactory = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			paymentItemInNewFactory.A2_AH = invoiceInNewFactory.PK;
			paymentItemInNewFactory.A2_AV = paymentInNewFactory.PK;
			paymentItemInNewFactory.A2_PaymentThisRun = -50M;

			newFactory.Save();

			APInvoice invoice = Factory.Load<APInvoice>(invoiceInNewFactory.PK);
			AssertEquals("Precondition: ", -100M, invoice.AH_OutstandingAmount);
			AssertEquals("Precondition: ", -50M, invoice.OutstandingAmountMatching);

			APCreditNote creditNote = TestObjectCreator.CreateAPCreditNote("CRD", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1M, "Desc");
			TestObjectCreator.CreateAPCreditNoteLine(creditNote, null, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, "Desc", 50M);
			TestObjectCreator.AttachJobToAPLine(creditNote.Lines[0]);
			creditNote.AH_OutstandingAmount = 50M;
			TestObjectCreator.AttachChargeToAPLine(creditNote.Lines[0]);

			((IMatching)invoice).OSPartialPaymentAmount = -50M;
			((IMatching)creditNote).OSPartialPaymentAmount = 50M;
			Collection.Add(invoice);
			Collection.Add(creditNote);
			Collection.Pay(ZDateTime.Today);
			TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
			matchLinkGroup.AddRange(Collection.GenerateMatchLinkRows());
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);

			Factory.Save();

			Assert("Must be saved without CriticalValidation exception", true);
			AssertEquals("All match links should be added to one TransactionMatchLinkGroup only.", true, matchLinkGroup.All(matchLink =>
				((IBusinessObjectInternals)matchLink).ParentCollections.OfType<TransactionMatchLinkGroup>().Count() == 1));
		}

		public void TestValidatePaymentApproval()
		{
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			using (paymentApproval.GetValidationSuspender())
			{
				paymentApproval.AV_Amount = -1110m;
			}
			Collection.Add(paymentApproval);

			Assert(!paymentApproval.HasErrors);
			Collection.ValidatePaymentApproval();
			Assert(paymentApproval.HasErrors);
		}

		public void TestFetchStrategy()
		{
			AssertNotNull("FetchStrategy must be an instance of IMatchingCollectionFetchStrategy", Collection.FetchStrategy as IMatchingCollectionFetchStrategy);
		}

		#region Implementation

		protected IMatchingCollection TestTransactions;
		protected OrgHeader TestOrg;
		protected OrgHeader TestOrg2;

		protected override void SetUp()
		{
			base.SetUp();
			TestOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			TestTransactions = new IMatchingCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IMatchingCollection(Factory);
		}

		protected new IMatchingCollection Collection
		{
			get { return (IMatchingCollection)base.Collection; }
		}

		ARInvoice GetARInvoice(RefCurrency currency, decimal exchangeRate, decimal oSExTaxAmount, decimal oSPartialAmount)
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			aRInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRInv.AH_ExchangeRate = exchangeRate;
			aRInv.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)aRInv).OSPartialPaymentAmount = oSPartialAmount;
			return aRInv;
		}

		APInvoice GetAPInvoice(RefCurrency currency, decimal exRate, decimal oSExTaxAmount, decimal oSPartialAmount)
		{
			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aPInv.AH_ExchangeRate = exRate;
			aPInv.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)aPInv).OSPartialPaymentAmount = oSPartialAmount;
			return aPInv;
		}

		ARReceipt GetARReceipt(RefCurrency currency, decimal exRate, decimal oSExTaxAmount, decimal oSPartialAmount)
		{
			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRRec.AH_ExchangeRate = exRate;
			aRRec.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)aRRec).OSPartialPaymentAmount = oSPartialAmount;
			return aRRec;
		}

		ARCreditNote GetARCreditNote(RefCurrency currency, decimal exRate, decimal oSExTaxAmount, decimal oSPartialAmount)
		{
			ARCreditNote aRCrd = Factory.NewWithValidTestData<ARCreditNote>();
			aRCrd.AH_RX_NKTransactionCurrency = currency.RX_Code;
			aRCrd.AH_ExchangeRate = exRate;
			aRCrd.AH_OSExTaxAmount = oSExTaxAmount;
			((IMatching)aRCrd).OSPartialPaymentAmount = oSPartialAmount;
			return aRCrd;
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
