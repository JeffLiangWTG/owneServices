using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching.Testing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalMatchingBaseTest : MatchingBaseTest
	{
		public void TestValidateZeroPaymentApprovalAmount()
		{
			PaymentTestMatchingBase.AddRowError("Test");

			PaymentTestMatchingBase.PaymentApproval.AV_Amount = 0m;
			PaymentTestMatchingBase.RunPreSaveValidation();
			AssertHasRowError(PaymentTestMatchingBase, "Test");
			AssertHasRowError(PaymentTestMatchingBase, "Overseas amount can not be zero");

			PaymentTestMatchingBase.PaymentApproval.AV_Amount = 1m;
			PaymentTestMatchingBase.RunPreSaveValidation();
			AssertHasRowError(PaymentTestMatchingBase, "Test");
			AssertNoRowError(PaymentTestMatchingBase, "Overseas amount can not be zero");
		}

		public void TestValidateZeroBalance()
		{
			PaymentTestMatchingBase.PaymentApproval.AV_Amount = 1m;
			AssertEquals("Percondition, balance not equal to zero", 1m, PaymentTestMatchingBase.Balance);

			AssertEquals(false, PaymentTestMatchingBase.PaymentApproval.IsSavingPaymentApprovalAsDraft);
			PaymentTestMatchingBase.RunPreSaveValidation();
			AssertHasError(PaymentTestMatchingBase.BalanceInfo, "The balance must equal 0");

			using (PaymentTestMatchingBase.Factory.SetTempContext(BusinessContext.SavingPaymentApprovalAsDraft))
			{
				AssertEquals(true, PaymentTestMatchingBase.PaymentApproval.IsSavingPaymentApprovalAsDraft);
				PaymentTestMatchingBase.RunPreSaveValidation();
				AssertNoError(PaymentTestMatchingBase.BalanceInfo, "The balance must equal 0");
			}
		}

		public virtual void TestMatchDateUsesMaxPostDateFromMatchedTransactions()
		{
			Assert("PaymentApprovalMatchingBase's MatchDate default value should be valid", !TestMatchingBase.MatchDate.IsEmpty && TestMatchingBase.MatchDate.IsValid);
		}

		public override void TestMatchingSuccessWithConcurrency()
		{
			Assert("PaymentApprovalMatchingBase overrides Match method, and so doesn't need to test with concurrency as there is no factory.Save() used", true);
		}

		public void TestDeleteTemporaryTransactionsWithPaymentApprovalHasChange()
		{
			Approval.AV_ExchangeDifference = 100m;
			Approval.AV_Discount = 200m;
			Factory.Save();
			AssertEquals("Temporary transactions get created and deleted when PaymentApproval has change", null, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("Temporary transactions get created and deleted when PaymentApproval has change", null, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertEquals("Temporary transactions set PaymentApproval's EXX and Discount Amount when PaymentApproval has change", 100m, Approval.AV_ExchangeDifference);
			AssertEquals("Temporary transactions set PaymentApproval's EXX and Discount Amount when PaymentApproval has change", 200m, Approval.AV_Discount);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			PaymentApprovalBase loadedApproval = newFactory.LoadTop1<PaymentApprovalBase>(new ZQuery(AccPaymentApprovalSchema.PK, Approval.PK));
			newFactory.Save();

			AssertEquals("Temporary transactions don't get created when PaymentApproval just gets loaded", null, loadedApproval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("Temporary transactions don't get created When PaymentApproval just gets loaded", null, loadedApproval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertEquals("PaymentApproval's EXX amount should remain the same when PaymentApproval just gets loaded", 100m, loadedApproval.AV_ExchangeDifference);
			AssertEquals("PaymentApproval's DSC amount should remain the same when PaymentApproval just gets loaded", 200m, loadedApproval.AV_Discount);
		}

		public void TestCreateTemporaryTransactions()
		{
			ZDecimal testExchangeDifferenceAmount = -100M;
			ZDecimal testDiscountAmount = -200M;
			var matchStatus = "UAC";
			var matchStatusReasonCode = "ADV";

			Approval.AV_Amount = 300m;

			Approval.AV_ExchangeDifference = testExchangeDifferenceAmount;
			Approval.AV_Discount = testDiscountAmount;
			Approval.AV_ExxMatchStatus = matchStatus;
			Approval.AV_ExxMatchStatusReasonCode = matchStatusReasonCode;
			Approval.AV_DscMatchStatus = matchStatus;
			Approval.AV_DscMatchStatusReasonCode = matchStatusReasonCode;

			Approval.PaymentMatchingBaseObject.CreateTemporaryTransactions();

			AssertEquals("Session Balance", 0m, Approval.PaymentMatchingBaseObject.Balance);
			AssertEquals("SessionBalancesToZero", true, Approval.PaymentMatchingBaseObject.SessionBalancesToZero);
			AssertEquals("Should be matchable", true, Approval.PaymentMatchingBaseObject.Match_ForTestOnly());

			AssertEquals("ExchangeDifferenceAmount", testExchangeDifferenceAmount, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertNotNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("ExchangeRate", 1m, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_ExchangeRate);
			AssertEquals("MatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);
			Assert("ExchangeDifferenceBizO_ForTestOnly should be matched", Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.PK));

			AssertEquals("DiscountAmount", testDiscountAmount, Approval.PaymentMatchingBaseObject.DiscountAmount);
			AssertNotNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertEquals("ExchangeRate", 1m, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_ExchangeRate);
			AssertEquals("MatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);
			Assert("DiscountBizO_ForTestOnly should be matched", Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.PK));

			AssertEquals("Exchange Difference Amount on Payment Approval", 0m, Approval.AV_ExchangeDifference);
			AssertEquals("Discount Amount on Payment Approval", 0m, Approval.AV_Discount);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			AssertEquals("Exchange Difference Amount on Payment Approval", testExchangeDifferenceAmount, Approval.AV_ExchangeDifference);
			AssertEquals("Discount Amount on Payment Approval", testDiscountAmount, Approval.AV_Discount);
			AssertEquals("Exchange Difference MatchStatus on Payment Approval", matchStatus, Approval.AV_ExxMatchStatus);
			AssertEquals("Exchange Difference MatchStatusReasonCode on Payment Approval", matchStatusReasonCode, Approval.AV_ExxMatchStatusReasonCode);
			AssertEquals("Discount MatchStatus on Payment Approval", matchStatus, Approval.AV_DscMatchStatus);
			AssertEquals("Discount MatchStatusReasonCode on Payment Approval", matchStatusReasonCode, Approval.AV_DscMatchStatusReasonCode);
			Approval.PaymentMatchingBaseObject.CreateTemporaryTransactions();

			AssertEquals("ExchangeDifferenceAmount", testExchangeDifferenceAmount, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertNotNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("MatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);
			AssertEquals("ExchangeRate", 1m, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_ExchangeRate);

			AssertEquals("DiscountAmount", testDiscountAmount, Approval.PaymentMatchingBaseObject.DiscountAmount);
			AssertNotNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertEquals("MatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("MatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);
			AssertEquals("ExchangeRate", 1m, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_ExchangeRate);
		}

		public void TestShouldCreateAndDeleteTemporaryTransactions()
		{
			SetUp();

			ZDecimal testExchangeDifferenceAmount = 10M;
			var matchStatus = "UAC";
			var matchStatusReasonCode = "ADV";

			APJournal testAPJournal = Factory.NewWithValidTestData<APJournal>();
			testAPJournal.AH_OH = TestOrgHeader.PK;
			testAPJournal.DebitCreditSign = "CR";
			testAPJournal.AH_LocalExTaxAmount = 90M;
			testAPJournal.AH_LocalOutstandingAmount = 90M;
			testAPJournal.AH_OSTotalAmount = 90M;

			Approval.PaymentMatchingBaseObject.AddToBalancingJournals(testAPJournal);
			Approval.AV_ExchangeDifference = testExchangeDifferenceAmount;
			Approval.AV_Discount = 10M;
			Approval.AV_ExxMatchStatus = matchStatus;
			Approval.AV_ExxMatchStatusReasonCode = matchStatusReasonCode;
			Approval.AV_DscMatchStatus = matchStatus;
			Approval.AV_DscMatchStatusReasonCode = matchStatusReasonCode;

			Approval.PaymentMatchingBaseObject.CreateTemporaryTransactions();

			AssertEquals("ExchangeDifferenceAmount", 0M, Approval.AV_ExchangeDifference);
			AssertEquals("ExchangeDifferenceAmount", testExchangeDifferenceAmount, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertNotNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertNotNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("ExxMatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("ExxMatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);
			AssertEquals("DscMatchStatus", matchStatus, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("DscmatchStatusReasonCode", matchStatusReasonCode, Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);

			Approval.PaymentMatchingBaseObject.DeleteTemporaryTransactions();

			AssertEquals("ExchangeDifferenceAmount", testExchangeDifferenceAmount, Approval.AV_ExchangeDifference);
			AssertEquals("ExchangeDifferenceAmount", 0M, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertEquals("ExxMatchStatus", matchStatus, Approval.AV_ExxMatchStatus);
			AssertEquals("ExxMatchStatusReasonCode", matchStatusReasonCode, Approval.AV_ExxMatchStatusReasonCode);
			AssertEquals("DscMatchStatus", matchStatus, Approval.AV_DscMatchStatus);
			AssertEquals("DscmatchStatusReasonCode", matchStatusReasonCode, Approval.AV_DscMatchStatusReasonCode);
		}

		public void TestConstructor()
		{
			Approval.AV_PayExRate = 1M;
			Approval.AV_Amount = 1000M;

			IMatching paymentDetailAsIMatching = PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly;

			AssertEquals("Payment Approval Detail", Approval.PK, PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.PK);
			AssertEquals("Payment Approval Detail IsProcessingPaymentDetail must be false", ZBool.False, Approval.IsProcessingPaymentDetail);
			AssertEquals("Payment Approval OS Payment Amount", paymentDetailAsIMatching.OSOutstandingAmount, paymentDetailAsIMatching.OSPartialPaymentAmount);
			AssertEquals("Payment Approval Local Payment Amount", paymentDetailAsIMatching.OutstandingAmount, paymentDetailAsIMatching.LocalPartialPaymentAmount);

			bool actual = PaymentTestMatchingBase.MatchedTransactions.MustTransactionBeMatched(Approval);
			AssertEquals("Payment Approval must be matched", true, actual);
		}

		public void TestConstructorSetIsLoadedFromGUIFlagForPaymentApprovalMatchingBaseFromPaymentApproval()
		{
			PaymentApprovalBase paymentApproval = GetNewPaymentApproval();
			paymentApproval.IsLoadedFromGUI = false;
			AssertEquals(false, paymentApproval.IsLoadedFromGUI);
			AssertEquals(false, paymentApproval.MatchingBaseObject.GetIsLoadedFromGUIForTest());

			paymentApproval = GetNewPaymentApproval();
			paymentApproval.IsLoadedFromGUI = true;
			AssertEquals(true, paymentApproval.IsLoadedFromGUI);
			AssertEquals(true, paymentApproval.MatchingBaseObject.GetIsLoadedFromGUIForTest());
		}

		protected abstract PaymentApprovalBase GetNewPaymentApproval();

		public void TestCreateAndDeleteTemporaryTransactions()
		{
			ZDecimal testExchangeDifferenceAmount = 10000M;
			ZDecimal testDiscountAmount = -5000M;
			ZDecimal testBankFeeAmount = -100M;
			ZDecimal newTestExchangeDifferenceAmount = 11000M;
			ZDecimal newTestDiscountAmount = -6000M;
			ZDecimal newTestBankFeeAmount = -200M;

			Approval.AV_ExchangeDifference = testExchangeDifferenceAmount;
			Approval.AV_Discount = testDiscountAmount;
			AssertEquals("PaymentApprovalBase", Approval.PK, PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.PK);

			AssertNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			AssertNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			AssertNull("OverpaymentBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.OverpaymentBizO_ForTestOnly);
			AssertNull("BankFeeBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.BankFeeBizO_ForTestOnly);

			// Create Before Showing the Form
			Approval.PaymentMatchingBaseObject.CreateTemporaryTransactions();

			AssertEquals("ExchangeDifferenceAmount", testExchangeDifferenceAmount, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertNotNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);
			Assert("ExchangeDifferenceBizO_ForTestOnly should be matched", Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.PK));

			AssertEquals("DiscountAmount", testDiscountAmount, Approval.PaymentMatchingBaseObject.DiscountAmount);
			AssertNotNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);
			Assert("DiscountBizO_ForTestOnly should be matched", Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.PK));

			AssertEquals("OSOverpaymentAmount", 0m, Approval.PaymentMatchingBaseObject.OSOverpaymentAmount);
			AssertNull("OverpaymentBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.OverpaymentBizO_ForTestOnly);

			AssertEquals("Exchange Difference Amount on Payment Approval", 0m, Approval.AV_ExchangeDifference);
			AssertEquals("Discount Amount on Payment Approval", 0m, Approval.AV_Discount);

			// User changes values using the form
			Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly.BindableInvoiceAmount = newTestExchangeDifferenceAmount;
			Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly.BindableInvoiceAmount = newTestDiscountAmount;

			AssertEquals("ExchangeDifferenceAmount", newTestExchangeDifferenceAmount, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertEquals("DiscountAmount", newTestDiscountAmount, Approval.PaymentMatchingBaseObject.DiscountAmount);

			// Users selects 'Match and Close' - Saves
			Approval.PaymentMatchingBaseObject.DeleteTemporaryTransactions();

			AssertEquals("ExchangeDifferenceAmount", 0m, Approval.PaymentMatchingBaseObject.ExchangeDifferenceAmount);
			AssertNull("ExchangeDifferenceBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.ExchangeDifferenceBizO_ForTestOnly);

			AssertEquals("DiscountAmount", 0m, Approval.PaymentMatchingBaseObject.DiscountAmount);
			AssertNull("DiscountBizO_ForTestOnly", Approval.PaymentMatchingBaseObject.DiscountBizO_ForTestOnly);

			AssertEquals("OSOverpaymentAmount", 0m, Approval.PaymentMatchingBaseObject.OSOverpaymentAmount);

			AssertEquals("Exchange Difference Amount on Payment Approval", newTestExchangeDifferenceAmount, Approval.AV_ExchangeDifference);
			AssertEquals("Discount Amount on Payment Approval", newTestDiscountAmount, Approval.AV_Discount);
		}

		public void TestMatchAndReloadPaymentApprovalItems()
		{
			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_OSExTaxAmount = 150M;

			APInvoice invoice2 = Factory.New<APInvoice>();
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_TransactionNum = "00001002";
			invoice2.AH_OSExTaxAmount = 150M;

			APInvoice invoice3 = Factory.New<APInvoice>();
			invoice3.AH_OH = TestOrgHeader.PK;
			invoice3.AH_TransactionNum = "00001003";
			invoice3.AH_OSExTaxAmount = 150M;

			Approval.AV_Amount = 300M;

			AssertEquals("Payment Approval OS Partial Payment Amount", 300M, ((IMatching)Approval).OSPartialPaymentAmount);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.AddRange(new BusinessObject[] { invoice1, invoice2, invoice3 });
			AssertEquals(3, invoices.Count);

			Approval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());

			((IMatching)invoice1).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 1 OS Partial Payment Amount", -100M, ((IMatching)invoice1).OSPartialPaymentAmount);
			((IMatching)invoice2).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 2 OS Partial Payment Amount", -100M, ((IMatching)invoice2).OSPartialPaymentAmount);
			((IMatching)invoice3).OSPartialPaymentAmount = -100M;
			AssertEquals("Invoice 3 OS Partial Payment Amount", -100M, ((IMatching)invoice3).OSPartialPaymentAmount);

			AssertEquals("Session Balance", 0m, Approval.PaymentMatchingBaseObject.Balance);
			AssertEquals("SessionBalancesToZero", true, Approval.PaymentMatchingBaseObject.SessionBalancesToZero);
			AssertEquals("Should be matchable", true, Approval.PaymentMatchingBaseObject.Match_ForTestOnly());

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(Factory);
			approvalItems.Load(new ZQuery());
			AssertEquals("Approval Items Count", 3, approvalItems.Count);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[0].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[0].A2_PaymentThisRun);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[1].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[1].A2_PaymentThisRun);

			AssertEquals("PaymentApprovalBase Item was created for one of the invoices", true, invoices.Contains(approvalItems[2].A2_AH));
			AssertEquals("PaymentApprovalBase This Run", -100M, approvalItems[2].A2_PaymentThisRun);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			AssertEquals("Matched Transactions", 4, Approval.PaymentMatchingBaseObject.MatchedTransactions.Count);

			AssertEquals("Payment Approval should be matched", true, Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(Approval.PK));

			AssertEquals("Invoice 1 should be matched", true, Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1.PK));
			AssertEquals("Invoice 1 OSMatched Amount", -100M, ((IMatching)invoice1).OSPartialPaymentAmount);

			AssertEquals("Invoice 2 should be matched", true, Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2.PK));
			AssertEquals("Invoice 2 OSMatched Amount", -100M, ((IMatching)invoice2).OSPartialPaymentAmount);

			AssertEquals("Invoice 3 should be matched", true, Approval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice3.PK));
			AssertEquals("Invoice 3 OSMatched Amount", -100M, ((IMatching)invoice3).OSPartialPaymentAmount);
		}

		public void TestCalculateOSBalanceExcludingPayment()
		{
			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OSExTaxAmount = 99.23M;
			TestAPInvoice1.AH_ExchangeRate = 0.78M;
			((IMatching)TestAPInvoice1).OSPartialPaymentAmount = -90.23M;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, 0.78M, 99.23M);

			TestARInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			TestARInvoice1.AH_OSExTaxAmount = 23.99M;
			TestARInvoice1.AH_ExchangeRate = 0.79M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 20.99M;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, TestARInvoice1.TransactionCurrency, 0.79M, 23.99M);

			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_PayExRate = 0.80m;
			TestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);
			TestMatchingBase.MatchedTransactions.Add(TestARInvoice1);
			AssertEquals("The OSBalance excluding Payment is 71.29", 71.29M, ((PaymentApprovalMatchingBase)TestMatchingBase).CalculateOSBalanceExcludingPayment());
		}

		public void TestMatchCurrentMatchingDateUpdate()
		{
			ZDateTime expectedDate = ZDateTime.Now;
			PaymentTestMatchingBase.MatchDate = expectedDate;
			PaymentTestMatchingBase.Match_ForTestOnly();
			AssertEquals("PaymentDetail.CurrentMatchingDate should be updated.", expectedDate.Date, PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.CurrentMatchingDate);
		}

		public void TestRecordCriticalValidationExceptionWithMiscTranasactionWhenBothMatchFails()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1001", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestOrgHeader);
			Factory.Save();

			Approval.AV_OH = ZGuid.Empty;
			Approval.AV_OH = TestOrgHeader.PK;
			Approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Approval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			Approval.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			Approval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			Approval.AV_Amount = 105;
			Approval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			Approval.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval.IsAllowedToPost = true;

			// 1st Match
			Assert(!Approval.PaymentMatchingBaseObject.Match_ForTestOnly());

			Approval.AV_ExchangeDifference = -7M;
			bool isHasException = false;
			try
			{
				// 2nd Match
				AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException ex)
			{
				isHasException = true;
				AssertContains("Non zero outstanding amount on miscellaneous transaction", ex.Message);
				AssertRecordCriticalValidationExceptionWithMiscTranasactionWhenBothMatchFails(ex.DeveloperErrorMessage);

				ErrorReporter.Clear();
			}

			Assert(isHasException);
		}

		public virtual void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhenBothMatchFails(string exceptionMessage)
		{
			throw new NotImplementedException();
		}

		public void TestRecordCriticalValidationExceptionWithMiscTranasactionWhen2ndMatchFails()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1001", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestOrgHeader);
			Factory.Save();

			Approval.AV_OH = ZGuid.Empty;
			Approval.AV_OH = TestOrgHeader.PK;
			Approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Approval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			Approval.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			Approval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			Approval.AV_Amount = 105M;
			Approval.AV_ExchangeDifference = -5M;
			Approval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			Approval.AV_Status = PaymentApprovalStatus.FullyApproved;
			Approval.IsAllowedToPost = true;

			// 1st Match
			Assert(Approval.PaymentMatchingBaseObject.Match_ForTestOnly());

			Approval.AV_ExchangeDifference = -7M;

			bool isHasException = false;
			try
			{
				// 2nd Match
				AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException ex)
			{
				isHasException = true;
				AssertContains("Non zero outstanding amount on miscellaneous transaction", ex.Message);
				AssertRecordCriticalValidationExceptionWithMiscTranasactionWhen2ndMatchFails(ex.DeveloperErrorMessage);

				ErrorReporter.Clear();
			}

			Assert(isHasException);
		}

		public void TestIsMatchTransactionsInDbChangedWithContextSavingPaymentApprovalAsDraft()
		{
			SetupForTestIsMatchTransactionsInDbChanged();

			AssertEquals("IsMatchTransactionsInDbChanged should be false as factory didn't contain context 'SavingPaymentApprovalAsDraft'", false, Approval.PaymentMatchingBaseObject.IsMatchTransactionsInDbChangedForDraft);

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				AssertEquals("IsMatchTransactionsInDbChanged should be true as factory contain context 'SavingPaymentApprovalAsDraft'", true, Approval.PaymentMatchingBaseObject.IsMatchTransactionsInDbChangedForDraft);
			}
		}

		public void TestIsMatchTransactionsInDbChangedWithCachedValueIsNull()
		{
			SetupForTestIsMatchTransactionsInDbChanged();

			IEnumerable<ZGuid> cachedValue;
			Factory.ClearCachedValue<IEnumerable<ZGuid>>(Approval.PaymentMatchingBaseObject.GetMatchedTransactionsCacheKey_ForTestOnly());
			Factory.TryGetValueFromCacheOnly(Approval.PaymentMatchingBaseObject.GetMatchedTransactionsCacheKey_ForTestOnly(), out cachedValue);
			AssertNull("Pre-condition: The cached value should be null.", cachedValue);

			AssertEquals("IsMatchTransactionsInDbChanged should be false as factory didn't contain context 'SavingPaymentApprovalAsDraft'", false, Approval.PaymentMatchingBaseObject.IsMatchTransactionsInDbChangedForDraft);

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft), () => Factory.RemoveContext(BusinessContext.SavingPaymentApprovalAsDraft)))
			{
				AssertEquals("IsMatchTransactionsInDbChanged should be true as factory contain context 'SavingPaymentApprovalAsDraft'", false, Approval.PaymentMatchingBaseObject.IsMatchTransactionsInDbChangedForDraft);
			}
		}

		void SetupForTestIsMatchTransactionsInDbChanged()
		{
			Approval.AV_Status = PaymentApprovalStatus.Draft;
			Approval.AV_Amount = 95m;
			Approval.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var creator = new TestObjectCreator(newFactory);
			var invoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "123456789", creator.USD, 2, 100M, 20, 200, 40);
			invoice1.AH_OH = Approval.AV_OH;
			newFactory.Save();

			var invoices = new InvoicingBaseCollection(newFactory);
			invoices.AddRange(new BusinessObject[] { invoice1 });
			newFactory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft);
			var reloadApproval = newFactory.Load<PaymentApprovalWithAuthorisation>(Approval.PK);
			reloadApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
			reloadApproval.PaymentMatchingBaseObject.MatchAndClearTransactions();
			newFactory.Save();
		}

		public virtual void AssertRecordCriticalValidationExceptionWithMiscTranasactionWhen2ndMatchFails(string exceptionMessage)
		{
			throw new NotImplementedException();
		}

		protected override int GetExpectedCountOfAllMatchedWhenManyTransactions(int count) => count + 1;

		#region Inherited Tests

		public override void TestTaxRealisationEnabler_CalledWithCorrectParameters()
		{
			Assert("This test is not  valid for PaymentApprovalMatchingBase as nothing is matched at this point", true);
		}

		public override void TestTaxRealisationEnablerIsCalledBeforeGLMovementProcessor()
		{
			Assert("This test is not  valid for PaymentApprovalMatchingBase as nothing is matched at this point", true);
		}

		public override void TestTaxRealisationEnabler_ReturnsErrorMessage()
		{
			Assert("This test is not  valid for PaymentApprovalMatchingBase as nothing is matched at this point", true);
		}

		public override void TestCreateMiscTransactionsFromPaymentApprovalDetails()
		{
			Assert("This method should never be called in PaymentApprovalMatchingBase - it is used to when creating a Payment", true);
		}

		public override void TestSettingPrimaryOrgClearsSelectedTransactions()
		{
			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();

			TestOrg1 = GetNewTestOrg();
			TestOrg2 = GetNewTestOrg();

			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentTestMatchingBase.MatchedTransactions.Add(TestAPInvoice1);

			PaymentTestMatchingBase.PrimaryOrganization = TestOrg2.PK;
			AssertEquals("Should be one transaction selected", 1, PaymentTestMatchingBase.MatchedTransactions.Count);
		}

		public override void TestPartialPaymentWithPartiallyPaidTransactions()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// AR Invoice has been partially paid, we want to
			// partially pay it
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 100M;
			TestARInvoice1.AH_LocalOutstandingAmount = 60M;
			IMatching aRINV_IMatching = TestARInvoice1;
			aRINV_IMatching.OSPartialPaymentAmount = 30M;

			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 40M;
			TestAPInvoice1.AH_LocalOutstandingAmount = 40M;
			IMatching aPINV_IMatching = TestAPInvoice1;
			aPINV_IMatching.OSPartialPaymentAmount = -30M;

			PaymentTestMatchingBase.AddIMatching(aRINV_IMatching);
			PaymentTestMatchingBase.AddIMatching(aPINV_IMatching);

			Assert("These transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());

			// Check that transactions are correctly matched
			AssertEquals("ARINV should still have 60 outstanding", 60M, TestARInvoice1.OutstandingAmountMatching);
			AssertEquals("APINV should still have 40 outstanding", 40M, TestAPInvoice1.AH_LocalOutstandingAmount);

			// Check dynamic transactions
			AssertEquals("No contras should be created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestPartialPaymentWithNoPartiallyPaidTransactions()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// This tests the case when none of the transactions in the 
			// collection have been partially paid, but one or more
			// must be partially paid for all amounts to balance to 0
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			IMatching aRINV_IMatching = TestARInvoice1;
			aRINV_IMatching.OSPartialPaymentAmount = 10M;

			// Credit note that has not been partially paid -
			// we want to partially pay it
			ARCRD1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARCRD1.AH_OH = TestOrg1.PK;
			ARCRD1.AH_LocalExTaxAmount = 30M;
			IMatching aRCRD_IMatching = ARCRD1;
			aRCRD_IMatching.OSPartialPaymentAmount = -10M;

			PaymentTestMatchingBase.AddIMatching(aRCRD_IMatching);
			PaymentTestMatchingBase.AddIMatching(aRINV_IMatching);

			Assert("Transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());

			AssertEquals("No dynamic transactions should be created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);
			AssertEquals("ARInvoice should have outstanding amount of 10", 10M, TestARInvoice1.OutstandingAmountMatching);
			AssertEquals("ARCRD1 should have outstanding amount of -30", -30M, ARCRD1.OutstandingAmountMatching);
		}

		public override void TestPartialPaymentWithARREC_ARPAY()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 100M;
			TestARReceipt.AH_OSExTaxAmount = 100M;
			TestARReceipt.AH_ExchangeRate = 1M;
			IMatching aRREC_IMatching = TestARReceipt;
			aRREC_IMatching.OSPartialPaymentAmount = -50M;

			ARPayment aRPAY = Factory.NewWithValidTestData<ARPayment>();
			aRPAY.AH_OH = TestOrg2.PK;
			aRPAY.AH_LocalExTaxAmount = 100M;
			aRPAY.AH_OSExTaxAmount = 100M;
			aRPAY.AH_ExchangeRate = 1M;
			IMatching aRPAY_IMatching = aRPAY;
			aRPAY_IMatching.OSPartialPaymentAmount = 50M;

			PaymentTestMatchingBase.AddIMatching(aRREC_IMatching);
			PaymentTestMatchingBase.AddIMatching(aRPAY_IMatching);

			Assert("These transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());
			AssertEquals("No Transfers should be created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);

			// Check that Transactions are correctly matched
			AssertEquals("OutstandingAmount on ARREC should be -100", -100M, TestARReceipt.OutstandingAmountMatching);
			AssertEquals("FullyPaid date on ARREC should be null", ZDateTime.Empty, TestARReceipt.AH_FullyPaidDate);
			AssertEquals("OutstandingAmount on ARPAY should be 100", 100M, aRPAY.OutstandingAmountMatching);
			AssertEquals("Fully Paid date on ARPAY should be null", ZDateTime.Empty, aRPAY.AH_FullyPaidDate);
		}

		public override void TestPartialPaymentWithAPJNL_APCTR_APTRF()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			// APJNL - signs are inverted
			APJournal aPJNL = Factory.NewWithValidTestData<APJournal>();
			aPJNL.AH_OH = TestOrg1.PK;
			aPJNL.AH_LocalExTaxAmount = 200M;
			aPJNL.AH_ExchangeRate = 1M;
			aPJNL.AH_OSTotalAmount = 200M;
			aPJNL.AH_LocalOutstandingAmount = 70M;
			((IMatching)aPJNL).OSPartialPaymentAmount = -70M;

			// APCTR - signs not inverted
			APContraRow aPCTR = Factory.NewWithValidTestData<APContraRow>();
			aPCTR.AH_OH = TestOrg1.PK;
			aPCTR.AH_LocalExTaxAmount = 300M;
			aPCTR.AH_ExchangeRate = 1M;
			aPCTR.AH_OSTotalAmount = 300M;
			((IMatching)aPCTR).OSPartialPaymentAmount = 35M;

			// APTRF From - signs not inverted
			APTransferFromRow aPTRF = Factory.NewWithValidTestData<APTransferFromRow>();
			aPTRF.AH_OH = TestOrg1.PK;
			aPTRF.AH_LocalExTaxAmount = 400M;
			aPTRF.AH_ExchangeRate = 1M;
			aPTRF.AH_LocalOutstandingAmount = 200M;
			((IMatching)aPTRF).OSPartialPaymentAmount = 35M;

			PaymentTestMatchingBase.AddIMatching(aPJNL);
			PaymentTestMatchingBase.AddIMatching(aPCTR);
			PaymentTestMatchingBase.AddIMatching(aPTRF);

			Assert("These transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());
			AssertEquals("There should be no dynamic transactions", 0, PaymentTestMatchingBase.DynamicTransactions.Count);

			// Check that original transactions are matched correctly
			AssertEquals("APCTR should have outstanding amt of 300", 300M, aPCTR.OutstandingAmountMatching);
			AssertEquals("APTRF should have outstanding amt of 200", 200M, aPTRF.OutstandingAmountMatching);
			AssertEquals("APJNL should have outstanding amt of -70", -70M, aPJNL.OutstandingAmountMatching);
		}

		public override void TestOverpaymentMatching()
		{
			Assert("You cant create an Overpayment from this matchingBzObj", true);
		}

		public override void TestOverpaymentAgainstSingleLedgerTransactions()
		{
			Assert("You cant create an Overpayment from this matchingBzObj", true);
		}

		public override void TestMoveFromUnMatchToMatchResetsPartialPaidAmount()
		{
			SetUpTestDataSet();
			Factory.Save(); // stops dirty table exception

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10M;
			TestARInvoice1.AH_OSTotalAmount = 10M;
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 10M;

			PaymentTestMatchingBase.UnmatchedTransactions.Add(TestARInvoice1);

			BusinessObject[] tmpSelected = new BusinessObject[1];
			tmpSelected[0] = TestARInvoice1;

			PaymentTestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);
			((IMatching)TestARInvoice1).OSPartialPaymentAmount = 5M;

			tmpSelected[0] = TestARInvoice1;
			PaymentTestMatchingBase.MoveFromMatchToUnmatch(tmpSelected);

			TestMatchingBase.MoveFromUnmatchToMatch(tmpSelected);

			AssertEquals("OSPartialPayment amt should be reset to 10", 10M, ((IMatching)TestARInvoice1).OSPartialPaymentAmount);
		}

		public override void TestMatchingWithJournal()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 100M;
			TestARInvoice1.AH_LocalTaxAmount = 10M;
			TestARInvoice1.AH_LocalOutstandingAmount = 110M;
			TestARInvoice1.AH_OSTotalAmount = 110M;

			ARJournal testARJournal = Factory.NewWithValidTestData<ARJournal>();
			testARJournal.AH_OH = TestOrg1.PK;
			testARJournal.AH_LocalExTaxAmount = -60M;
			testARJournal.AH_LocalOutstandingAmount = -60M;
			testARJournal.AH_OSTotalAmount = -60M;

			ARTransferFromRow testFromRow = Factory.NewWithValidTestData<ARTransferFromRow>();
			testFromRow.AH_OH = TestOrg1.PK;
			testFromRow.AH_LocalExTaxAmount = 50M;
			testFromRow.AH_LocalOutstandingAmount = 50M;
			testFromRow.AH_OSTotalAmount = 50M;

			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(testARJournal);
			PaymentTestMatchingBase.AddIMatching(testFromRow);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("These Transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());

			AssertEquals("Outstanding amount on AR Invoice should be -50", -50M, testFromRow.OutstandingAmountMatching);
			AssertEquals("Outstanding amount on Journal should be -60", -60M, testARJournal.OutstandingAmountMatching);
		}

		public override void TestMatchingWithAPJournal()
		{
			Assert("Not applicable to this class", true);
		}

		public override void TestMatchingINV_CRD_APPaymentFromBothLedgers()
		{
			SetUpTestDataSet();
			Factory.Save();
			SetUpTestDataSet1();

			// TestOrg 1
			TestAPPayment.AH_OH = TestOrg1.PK;
			TestAPPayment.AH_LocalExTaxAmount = 24.24M;
			TestAPPayment.AH_OSTotalAmount = 24.24M;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 4.26M;
			TestARInvoice1.AH_OSTotalAmount = 4.26M;

			PaymentTestMatchingBase.AddIMatching(ARCRD1);
			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestAPInvoice1);
			PaymentTestMatchingBase.AddIMatching(APCRD1);
			PaymentTestMatchingBase.AddIMatching(TestAPPayment);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			PaymentTestMatchingBase.Match_ForTestOnly();

			AssertEquals("There should be no Contras or Transfers should be created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestMatchingContrasAndInvoicesFromBothLedgers()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg3.PK;

			Contra testContra1 = Contra.New(Factory);
			testContra1.APRow.AH_OH = TestOrg1.PK;
			testContra1.APRow.AH_LocalExTaxAmount = 48M;
			testContra1.APRow.AH_OSTotalAmount = 48M;

			testContra1.ARRow.AH_OH = TestOrg2.PK;
			testContra1.ARRow.AH_LocalExTaxAmount = 48M;
			testContra1.ARRow.AH_OSTotalAmount = 48M;

			TestAPInvoice1.AH_OH = TestOrg3.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 80M;
			TestAPInvoice1.AH_OSTotalAmount = 80M;

			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 40M;
			TestARInvoice1.AH_OSTotalAmount = 40M;

			TestARInvoice2.AH_OH = TestOrg2.PK;
			TestARInvoice2.AH_LocalExTaxAmount = 40M;
			TestARInvoice2.AH_OSTotalAmount = 40M;

			PaymentTestMatchingBase.AddIMatching(testContra1);
			PaymentTestMatchingBase.AddIMatching(TestAPInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestARInvoice2);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should allow matching since balance is zero", PaymentTestMatchingBase.Match_ForTestOnly());
		}

		public override void TestMatchingARInvoiceAndAPInvoiceFromDifferentOrg()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 34.56M, 0m, 0m, 34.56M, 0m, 0m);

			TestAPInvoice1.AH_OH = TestOrg2.PK;
			TestAPInvoice1.AH_FullyPaidDate = ZDateTime.Empty;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 34.56M, 0m, 0m, 34.56M, 0m, 0m);

			Factory.Save(); // ** required for dirty table test
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestAPInvoice1);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			AssertEquals("Fully Paid Date should not be set", ZDateTime.Empty, TestARInvoice1.AH_FullyPaidDate);
			AssertEquals("Fully Paid Date should not be set", ZDateTime.Empty, TestAPInvoice1.AH_FullyPaidDate);

			Assert("Should match since balance is 0", PaymentTestMatchingBase.Match_ForTestOnly());

			AssertEquals("outstanding amount on ARInvoice should still be 34.56", 34.56M, TestARInvoice1.OutstandingAmountMatching);
			AssertEquals("outstanding amount on APInvoice should be -34.56", -34.56M, TestAPInvoice1.OutstandingAmountMatching);

			AssertEquals("Fully Paid Date should not be set", ZDateTime.Empty, TestARInvoice1.AH_FullyPaidDate);
			AssertEquals("Fully Paid Date should not be set", ZDateTime.Empty, TestAPInvoice1.AH_FullyPaidDate);
		}

		public override void TestMatchedTransactionsAreNotRematched()
		{
			SetUpTestDataSet();
			Factory.Save();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestARInvoice1.AH_LocalExTaxAmount = 10;
			TestARInvoice1.AH_OutstandingAmount = 0;
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Today;

			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_LocalExTaxAmount = 10;

			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestARReceipt);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("ARInvoice has been matched so matching should fail", !PaymentTestMatchingBase.Match_ForTestOnly());
		}

		public override void TestFullPaymentWithPartiallyPaidTransactions()
		{
			SetUpTestDataSet();
			Factory.Save();
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			// AP Invoice that has been partially paid - we want 
			// to fully pay it
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestAPInvoice1.AH_LocalExTaxAmount = 100M;
			TestAPInvoice1.AH_LocalOutstandingAmount = 30M;
			IMatching aPINV_IMatching = TestAPInvoice1;
			aPINV_IMatching.OSPartialPaymentAmount = -30M;

			APCRD1 = Factory.NewWithValidTestData<APCreditNote>();
			APCRD1.AH_OH = TestOrg1.PK;
			APCRD1.AH_LocalExTaxAmount = 30M;
			APCRD1.AH_LocalOutstandingAmount = 30M;
			IMatching aPCRD_IMatching = APCRD1;
			aPCRD_IMatching.OSPartialPaymentAmount = 30M;

			PaymentTestMatchingBase.AddIMatching(aPINV_IMatching);
			PaymentTestMatchingBase.AddIMatching(aPCRD_IMatching);

			Assert("The transactions should be matchable", PaymentTestMatchingBase.Match_ForTestOnly());
			AssertEquals("No dynamic transactions should be created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);

			// Check that transactions are matched correctly
			AssertEquals("Outstanding amt on AP Invoice should be -30", -30M, TestAPInvoice1.OutstandingAmountMatching);
			AssertEquals("Outstanding amt on ARCRD should be 30", 30M, APCRD1.OutstandingAmountMatching);
		}

		public override void TestDeleteCachedMiscTransactions()
		{
			TestOrg1 = GetNewTestOrg();
			Factory.Save();

			TestAPInvoice1 = Factory.NewWithValidTestData<APInvoice>();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 320M);
			Factory.Save();

			var matchStatus = "UAC";
			var matchStatusReasonCode = "ADV";

			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentTestMatchingBase.MoveAllFromUnmatchToMatch();

			AssertEquals("There should be 2 transactions selected for matching", 2, PaymentTestMatchingBase.MatchedTransactions.Count);

			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_Discount = 10000M;
			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_ExchangeDifference = -10000M;
			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_ExxMatchStatus = matchStatus;
			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_ExxMatchStatusReasonCode = matchStatusReasonCode;
			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_DscMatchStatus = matchStatus;
			PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly.AV_DscMatchStatusReasonCode = matchStatusReasonCode;

			PaymentTestMatchingBase.CreateTemporaryTransactions();

			AssertNull("OverpaymentBizO_ForTestOnly should be null", PaymentTestMatchingBase.OverpaymentBizO_ForTestOnly);

			AssertNotNull("DiscountBizO_ForTestOnly should not be null", PaymentTestMatchingBase.DiscountBizO_ForTestOnly);
			AssertEquals("Discount Amount", 10000M, PaymentTestMatchingBase.DiscountAmount);
			AssertEquals("Discount Amount", matchStatus, PaymentTestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("Discount Amount", matchStatusReasonCode, PaymentTestMatchingBase.DiscountBizO_ForTestOnly.AH_MatchStatusReasonCode);

			AssertNotNull("ExchangeDiffTmp should not be null", PaymentTestMatchingBase.ExchangeDiffTmp);
			AssertEquals("Exchange Difference Amount", -10000M, PaymentTestMatchingBase.ExchangeDifferenceAmount);
			AssertNotNull("ExchangeDifferenceBizO_ForTestOnly should not be null", PaymentTestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
			AssertEquals("Discount Amount", matchStatus, PaymentTestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatus);
			AssertEquals("Discount Amount", matchStatusReasonCode, PaymentTestMatchingBase.ExchangeDifferenceBizO_ForTestOnly.AH_MatchStatusReasonCode);

			PaymentTestMatchingBase.DeleteTemporaryTransactions();

			AssertNull("OverpaymentBizO_ForTestOnly should be null", PaymentTestMatchingBase.OverpaymentBizO_ForTestOnly);
			AssertNull("DiscountBizO_ForTestOnly should be null", PaymentTestMatchingBase.DiscountBizO_ForTestOnly);
			AssertNull("ExchangeDiffTmp should be null", PaymentTestMatchingBase.ExchangeDifferenceBizO_ForTestOnly);
		}

		public override void TestARMatchingInvoiceAndTransfer()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 20m, 0m, 0m, 20m, 0m, 0m);
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			ARTransferFromRow testARFromRow = Factory.NewWithValidTestData<ARTransferFromRow>();
			testARFromRow.AH_OH = TestOrg2.PK;
			testARFromRow.AH_LocalExTaxAmount = 20M;
			testARFromRow.AH_OSTotalAmount = 20M;
			testARFromRow.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(testARFromRow);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should match since balance is 0", PaymentTestMatchingBase.Match_ForTestOnly());
			AssertEquals("No Dynamic Transactions should have been created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestARMatching_TwoInvoiceAndOneReceiptFromDifferentOrg()
		{
			SetUpTestDataSet();
			TestARInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice1, GlbCompany.CurrentCompany.LocalCurrency, 1m, 50m, 0m, 0m, 50m, 0m, 0m);
			TestARInvoice1.AH_FullyPaidDate = ZDateTime.Empty;

			TestARInvoice2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestARInvoice2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 60m, 0m, 0m, 60m, 0m, 0m);
			TestARInvoice2.AH_FullyPaidDate = ZDateTime.Empty;

			TestARReceipt.AH_LocalExTaxAmount = 100;
			TestARReceipt.AH_OSTotalAmount = 100;
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_FullyPaidDate = ZDateTime.Empty;
			//TestARReceipt.AH_LocalOutstandingAmount = 100; // Outstanding amt not automatically set on Receipts

			/*
			 * Balance of TestOrg1 is -50
			 * Balance of TestOrg2 is 60
			 */

			Factory.Save(); // for DBOnlyQuery in PrimaryOrg
			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
			PaymentTestMatchingBase.AddIMatching(TestARInvoice2);
			PaymentTestMatchingBase.AddIMatching(TestARReceipt);
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			AssertEquals("Balance should be 10", 10M, PaymentTestMatchingBase.MatchedTransactions.Balance);
			Assert("Should not allow a match since balance is not 0", !PaymentTestMatchingBase.Match_ForTestOnly());

			TestARInvoice2.AH_LocalExTaxAmount = 50;
			TestARInvoice2.AH_OSTotalAmount = 50;
			PaymentTestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			Assert("Should allow a match since balance is 0", PaymentTestMatchingBase.Match_ForTestOnly());

			AssertEquals("No Transfers should be dynamically created", 0, PaymentTestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestARMatching_InvoiceAndReceiptFromSameOrg()
		{
			SetUpTestDataSet();
			Factory.Save();
			if (PaymentTestMatchingBase is ARPaymentApprovalMatching)
			{
				TestOrg1.OH_IsDebtor = true;
			}
			else
			{
				TestOrg1.OH_IsCreditor = true;
			}

			PaymentTestMatchingBase.PrimaryOrganization = TestOrg1.PK;

			TestARInvoice1.AH_LocalExTaxAmount = 50M;
			TestARInvoice1.AH_OSTotalAmount = 50M;
			TestARInvoice1.AH_OH = TestOrg1.PK;

			TestAPInvoice1.AH_LocalExTaxAmount = 50M;
			TestAPInvoice1.AH_OSTotalAmount = 50M;
			TestAPInvoice1.AH_OH = TestOrg1.PK;

			TestARReceipt.AH_LocalExTaxAmount = 50M;
			TestARReceipt.AH_OH = TestOrg1.PK;
			TestARReceipt.AH_OSTotalAmount = 50M;

			TestAPPayment.AH_LocalExTaxAmount = 50M;
			TestAPPayment.AH_OH = TestOrg1.PK;
			TestAPPayment.AH_OSTotalAmount = 50M;

			if (PaymentTestMatchingBase is ARPaymentApprovalMatching)
			{
				PaymentTestMatchingBase.AddIMatching(TestARInvoice1);
				PaymentTestMatchingBase.AddIMatching(TestARReceipt);
			}
			else
			{
				PaymentTestMatchingBase.AddIMatching(TestAPInvoice1);
				PaymentTestMatchingBase.AddIMatching(TestAPPayment);
			}

			AssertEquals("Session Balance should be zero", true, PaymentTestMatchingBase.SessionBalancesToZero);
		}

		public override void TestAPMatching_TwoInvoicesAndOnePaymentFromDifferentOrg()
		{
			SetUpTestDataSet();
			Factory.Save();
			TestAPInvoice1.AH_OH = TestOrg1.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice1, TestAPInvoice1.TransactionCurrency, TestAPInvoice1.AH_ExchangeRate, 38m, 0m, 0m, 38m, 0m, 0m);

			TestAPInvoice2.AH_OH = TestOrg2.PK;
			TestObjectCreator.CreateInvoiceLine(TestAPInvoice2, TestAPInvoice2.TransactionCurrency, TestAPInvoice2.AH_ExchangeRate, 42m, 0m, 0m, 42m, 0m, 0m);

			TestAPPayment.AH_LocalExTaxAmount = 81;
			TestAPPayment.AH_OSTotalAmount = 81;
			TestAPPayment.AH_OH = TestOrg2.PK;

			/*
			 * TestOrg1 Balance is -38
			 * TestOrg2 Balance is 39
			 */

			// ** required for Dirty Table test
			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(TestAPInvoice1);
			TestMatchingBase.AddIMatching(TestAPInvoice2);
			TestMatchingBase.AddIMatching(TestAPPayment);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			AssertEquals("The balance should be 1", 1M, TestMatchingBase.MatchedTransactions.Balance);
			Assert("Should not allow a match since balance is not 0", !PaymentTestMatchingBase.Match_ForTestOnly());

			TestAPPayment.AH_LocalExTaxAmount = 80;
			TestAPPayment.AH_OSTotalAmount = 80;
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();
			//TestAPPayment.AH_OutstandingAmount = 80;	// ** required because not set automatically
			Assert("Should allow a match since balance is 0", PaymentTestMatchingBase.Match_ForTestOnly());

			AssertEquals("No Dynamic transactions should be created", 0, TestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestAPMatchingReceiptAndContra()
		{
			SetUpTestDataSet();
			Factory.Save();
			APReceipt testAPReceipt = Factory.NewWithValidTestData<APReceipt>();
			testAPReceipt.AH_OH = TestOrg1.PK;
			testAPReceipt.AH_LocalExTaxAmount = 20M;
			testAPReceipt.AH_OSTotalAmount = 20M;

			APContraRow testAPContraRow = Factory.NewWithValidTestData<APContraRow>();
			testAPContraRow.AH_OH = TestOrg2.PK;
			testAPContraRow.AH_InvoiceAmount = 20M;
			testAPContraRow.AH_OutstandingAmount = 20M;
			testAPContraRow.AH_OSTotal = 20M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = TestOrg1.PK;
			TestMatchingBase.AddIMatching(testAPReceipt);
			TestMatchingBase.AddIMatching(testAPContraRow);
			TestMatchingBase.MatchedTransactions.SetPartialPaidAmount();

			Assert("Should match since balance is 0", PaymentTestMatchingBase.Match_ForTestOnly());
			AssertEquals("No Transfers should be dynamically created", 0, TestMatchingBase.DynamicTransactions.Count);
		}

		public override void TestValidateRows()
		{
			RefCurrency testCurrency1 = Factory.NewWithValidTestData<RefCurrency>();
			OrgHeader testOrg1 = TestObjectCreator.TestOrganisation;

			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			payment1.AH_OH = testOrg1.PK;
			payment1.AH_RX_NKTransactionCurrency = testCurrency1.RX_Code;
			payment1.AH_LocalOutstandingAmount = 30M;
			payment1.AH_ExchangeRate = 20;

			Approval.AV_Amount = 30M;

			Factory.Save();

			TestMatchingBase.PrimaryOrganization = testOrg1.PK;
			TestMatchingBase.UnmatchedTransactions.Load();

			TestMatchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { payment1 });
			AssertEquals("MatchedTransactions must contain two payments", 2, TestMatchingBase.MatchedTransactions.Count);
			//Assert("First Payment must not have error because it is default payment", !TestMatchingBase.MatchedTransactions[0].HasNotifications());
			Assert("First Payment must not have error because it is default payment. Errors: " + TestMatchingBase.MatchedTransactions[0].GetErrors().ToUniqueMessageListString(), !TestMatchingBase.MatchedTransactions[0].HasNotifications());
			Assert("Second Payment must have error", TestMatchingBase.MatchedTransactions[1].HasNotifications());

			BusinessObject[] tempArray = new BusinessObject[1];
			tempArray[0] = (BusinessObject)TestMatchingBase.MatchedTransactions[1];
			TestMatchingBase.MoveFromMatchToUnmatch(tempArray);
			AssertEquals("MatchedTransactions must contain one payment", 1, TestMatchingBase.MatchedTransactions.Count);
			//Assert("First Payment must not have error because it is default payment", !TestMatchingBase.MatchedTransactions[0].HasNotifications());
			Assert("First Payment must not have error because it is default payment. Errors: " + TestMatchingBase.MatchedTransactions[0].GetErrors().ToUniqueMessageListString(), !TestMatchingBase.MatchedTransactions[0].HasNotifications());
		}

		public override void TestDontDeleteMatchLinksIfDoNotSaveFactoryOnMatching()
		{
			Assert("The test is not applicable here.", true);
		}

		#endregion

		#region Implementation

		protected override ZBool ShouldTestPayLine => false;

		protected override void SetUp()
		{
			base.SetUp();
			TestOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			TestOrgHeader.CompanyData.OB_IsCreditor = true;
			TestOrgHeader.CompanyData.OB_IsDebtor = true;
			Factory.Save();
		}

		protected OrgHeader TestOrgHeader;

		protected PaymentApprovalBase Approval
		{
			get { return PaymentTestMatchingBase != null ? PaymentTestMatchingBase.PaymentApprovalDetail_ForTestOnly : null; }
		}

		protected PaymentApprovalMatchingBase PaymentTestMatchingBase
		{
			get { return (PaymentApprovalMatchingBase)TestMatchingBase; }
		}

		protected override Journal.Journal GetJournalFromDB()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			var journal = Factory.LoadTop1<APJournal>(query);
			return journal;
		}

		#endregion
	}
}
