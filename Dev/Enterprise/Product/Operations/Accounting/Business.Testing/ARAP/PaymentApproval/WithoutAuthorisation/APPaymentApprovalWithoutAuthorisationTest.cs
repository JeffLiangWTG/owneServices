using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithoutAuthorisation))]
	public class APPaymentApprovalWithoutAuthorisationTest : PaymentApprovalWithoutAuthorisationTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var eDocsParsingSupport = TestPaymentApproval as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestProcessEPaymentSecurity()
		{
			Env.Security.APPaymentProcessingProcessEPayment.IsAllowed = false;
			Assert(!Env.Security.APPaymentProcessingProcessEPayment.IsAllowed);
			Env.Security.APPaymentProcessingProcessEPayment.IsAllowed = true;
			Assert(Env.Security.APPaymentProcessingProcessEPayment.IsAllowed);
		}

		public override void TestAmountsAndExchangeRates()
		{
			var originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				TestPaymentApproval.AV_OH = TestOrgHeader.PK;
				TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
				TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
				TestPaymentApproval.AV_PayExRate = 0.75m;

				TestPaymentApproval.AV_Amount = 100m;
				AssertEquals("if the payment amount is changed, the local amount will recalculate based on the exchange rate", 75m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(100m, TestPaymentApproval.AV_Amount);
				AssertEquals(0.75m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_Calc_LocalAmount = 150m;
				AssertEquals("if local amount is changed, the payment amount remains static ", 100m, TestPaymentApproval.AV_Amount);
				AssertEquals(150m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("if local amount is changed, exchange rate recalculates, should be 150/100", 1.5m, TestPaymentApproval.AV_PayExRate);

				if (!TestPaymentApproval.PostsOnSave)
				{
					TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				}

				TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, TestPaymentApproval, 150m);

				TestPaymentApproval.CreateNewPayment();

				AssertNotNull("Payment", TestPaymentApproval.TransactionHeader);
				AssertEquals("Payment OS Amount", 100m, TestPaymentApproval.TransactionHeader.AH_OSTotal);
				AssertEquals("Payment Local Amount", 150m, TestPaymentApproval.TransactionHeader.AH_InvoiceAmount);

				AssertEquals("Payment Exchange Rate", 1.5m, TestPaymentApproval.TransactionHeader.AH_ExchangeRate);
				AssertEquals("Payment Approval Exchange Rate", 1.5m, TestPaymentApproval.AV_PayExRate);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public override void TestAmountsTogether()
		{
			var originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			Factory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				TestPaymentApproval.AV_Amount = 0;
				TestPaymentApproval.AV_PayExRate = 1;

				AssertEquals(0m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(0m, TestPaymentApproval.AV_Amount);
				AssertEquals(1m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_Amount = 1.5;
				AssertEquals(1.5m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(1.5m, TestPaymentApproval.AV_Amount);
				AssertEquals(1m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_Amount = 2;
				AssertEquals(2m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(2m, TestPaymentApproval.AV_Amount);
				AssertEquals(1m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_Calc_LocalAmount = 4;
				AssertEquals(4m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(2m, TestPaymentApproval.AV_Amount);
				AssertEquals("2/4 , any change to the local amount will recalculate the exchange rate", 0.5m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_PayExRate = 3;
				AssertEquals(0.67m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(2m, TestPaymentApproval.AV_Amount);
				AssertEquals(3m, TestPaymentApproval.AV_PayExRate);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public override void TestCurrencyAndExchangeRateFields()
		{
			var expectedRate = TestPaymentApproval.IsPayables ? USDBuyRate.RE_SellRate : USDSellRate.RE_SellRate;
			var exchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(TestObjectCreator.USD.RX_Code, TestPaymentApproval.GetRateType_ForTestOnly(), ZDateTime.Now.ToDateTime());
			AssertEquals("Precondition: ExchangeRate from standard code", expectedRate, exchangeRate);

			TestPaymentApproval.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Exchange Rate", 1m, TestPaymentApproval.AV_PayExRate);
			Assert("Exchange Rate Readonly", TestPaymentApproval.AV_PayExRateInfo.ReadOnly);

			TestPaymentApproval.AV_PostDate = ZDateTime.Now;
			TestPaymentApproval.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			AssertEquals("Exchange Rate", expectedRate, TestPaymentApproval.AV_PayExRate);
			Assert("Exchange Rate not Readonly", !TestPaymentApproval.AV_PayExRateInfo.ReadOnly);

			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert("if bank account local then currency is not Readonly", !TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);

			TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
			Assert("if bank account foreign then currency is Readonly", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);

			AssertEquals("RXDecimals", TestPaymentApproval.PaymentCurrency.Decimals, TestPaymentApproval.RXDecimals);
			AssertEquals("LocalRXDecimals", GlbCompany.CurrentCompany.LocalCurrency.Decimals, TestPaymentApproval.LocalRXDecimals);

			AssertEquals("AV_Calc_LocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPaymentApproval.AV_Calc_LocalCurrency);
			AssertEquals("AV_Calc_LocalCurrencyInfo.ReadOnly", true, TestPaymentApproval.AV_Calc_LocalCurrencyInfo.ReadOnly);

			AssertEquals("OSCurrencyForDisplay", TestPaymentApproval.AV_RX_NKPaymentCurrency, TestPaymentApproval.OSCurrencyForDisplay);
			AssertEquals("OSCurrencyForDisplayInfo.ReadOnly", true, TestPaymentApproval.OSCurrencyForDisplayInfo.ReadOnly);
		}

		public void TestAV_RX_NKPaymentCurrency()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
			AssertEquals("exRate updated to 1", 1m, TestPaymentApproval.AV_PayExRate);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			AssertEquals("exRate updated to Today's rate", 0.75m, TestPaymentApproval.AV_PayExRate);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.EUR.RX_Code;
			AssertEquals("exRate default is zero if there is no today's rate", 0m, TestPaymentApproval.AV_PayExRate);
		}

		public void TestExRateSettingsWhenCurrencyIsUpdated()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
			Assert("exRate is read only when currency is local", TestPaymentApproval.AV_PayExRate_ReadOnly_ForTestOnly);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			Assert("exRate is editable when currency is foreign", !TestPaymentApproval.AV_PayExRate_ReadOnly_ForTestOnly);
		}

		public override void TestIMatchingProperties()
		{
			AssertEquals(TestPaymentApproval.AV_Calc_LocalAmountInfo.Name, ((IMatching)TestPaymentApproval).LocalPartialPaymentAmountInfo.Name);
		}

		public override void TestShouldPostPaymentDefaultValue()
		{
			using (AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Post as approvals, not invoices.", false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
			}

			using (AccountingConfigurationRegistry.Instance.PayInvoicesDefaultPostPaymentsAsPaymentApprovals.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Post as invocies", true, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
			}
		}

		public override void TestShouldPostPaymentWhenPostActionReturnTrue()
		{
			TestPaymentApproval.InitializeForPaymentBatch(() => true);
			AssertEquals("ShouldPost action returns true", true, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
		}

		public void TestIsNotCreatedFromPaymentBatchPoster()
		{
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;

			AssertEquals("Payment Approval Without Authorisation by default should allow posted", true, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
			AssertEquals(false, TestPaymentApproval.AV_ABInfo.ReadOnly);
			AssertEquals(false, TestPaymentApproval.AV_OHInfo.ReadOnly);
			AssertEquals(false, TestPaymentApproval.AV_PaymentDateInfo.ReadOnly);
			AssertEquals(false, TestPaymentApproval.AV_PaymentTypeInfo.ReadOnly);
			AssertEquals(false, TestPaymentApproval.AV_AKInfo.ReadOnly);
			AssertEquals(typeof(APPaymentApprovalMatching), TestPaymentApproval.PaymentMatchingBaseObject.GetType());
		}

		public void TestIsCreatedFromPaymentBatchPoster()
		{
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.InitializeForPaymentBatch(() => true);

			AssertEquals("Payment Approval Without Authorisation by default should allow posted", true, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
			AssertEquals("Assert special readonly logic", true, TestPaymentApproval.AV_ABInfo.ReadOnly);
			AssertEquals(true, TestPaymentApproval.AV_OHInfo.ReadOnly);
			AssertEquals(true, TestPaymentApproval.AV_PaymentDateInfo.ReadOnly);
			AssertEquals(true, TestPaymentApproval.AV_PaymentTypeInfo.ReadOnly);
			AssertEquals(true, TestPaymentApproval.AV_AKInfo.ReadOnly);
			AssertEquals(typeof(APPaymentBatchApprovalMatching), TestPaymentApproval.PaymentMatchingBaseObject.GetType());

			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals(false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
		}

		public void TestPaymentApprovalSecurity()
		{
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCheque, false, ReceiptTypes.Cheque, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCash, false, ReceiptTypes.Cash, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCreditCard, false, ReceiptTypes.CreditCard, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentDirectDebit, false, ReceiptTypes.DirectDebit, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentEFT, false, ReceiptTypes.EFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentSFT, false, ReceiptTypes.ScheduledEFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCRQ, false, ReceiptTypes.CollectionRequest, false);

			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCheque, true, ReceiptTypes.Cheque, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCash, true, ReceiptTypes.Cash, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCreditCard, true, ReceiptTypes.CreditCard, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentDirectDebit, true, ReceiptTypes.DirectDebit, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentEFT, true, ReceiptTypes.EFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentSFT, true, ReceiptTypes.ScheduledEFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentCRQ, true, ReceiptTypes.CollectionRequest, true);

			AssertPaymentApprovalSecurityCase(Env.Security.NewPayablesPaymentDirectDebit, false, ReceiptTypes.DirectCredit, true);
		}

		public override void TestAV_AK_ReadOnly()
		{
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);
			foreach (var type in TestPaymentApproval.Lookups.PaymentMethods.GetAllCodes())
			{
				TestPaymentApproval.AV_PaymentType = type;
				AssertEquals(!TestPaymentApproval.IsCheque, TestPaymentApproval.AV_AKInfo.ReadOnly);
			}

			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			foreach (var type in TestPaymentApproval.Lookups.PaymentMethods.GetAllCodes())
			{
				TestPaymentApproval.AV_PaymentType = type;
				AssertEquals(true, TestPaymentApproval.AV_AKInfo.ReadOnly);
			}

			Factory.Save();
			TestPaymentApproval.ReadOnly = false;
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			foreach (var type in TestPaymentApproval.Lookups.PaymentMethods.GetAllCodes())
			{
				TestPaymentApproval.AV_PaymentType = type;
				AssertEquals(!TestPaymentApproval.IsCheque, TestPaymentApproval.AV_AKInfo.ReadOnly);
			}
		}

		public void TestAV_AB_ReadOnly()
		{
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_ABInfo.ReadOnly);

			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(true, TestPaymentApproval.AV_ABInfo.ReadOnly);

			Factory.Save();
			TestPaymentApproval.ReadOnly = false;
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_ABInfo.ReadOnly);
		}

		public void TestAV_PaymentDate_ReadOnly()
		{
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_PaymentDateInfo.ReadOnly);

			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(true, TestPaymentApproval.AV_PaymentDateInfo.ReadOnly);

			Factory.Save();
			TestPaymentApproval.ReadOnly = false;
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_PaymentDateInfo.ReadOnly);
		}

		public void TestAV_PaymentType_ReadOnly()
		{
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_PaymentTypeInfo.ReadOnly);

			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals("Percondition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(true, TestPaymentApproval.AV_PaymentTypeInfo.ReadOnly);

			Factory.Save();
			TestPaymentApproval.ReadOnly = false;
			AssertEquals("Percondition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Percondition", true, TestPaymentApproval.IsLoadedFromPaymentBatch);
			AssertEquals(false, TestPaymentApproval.AV_PaymentTypeInfo.ReadOnly);
		}

		protected override ZString ExpectedDefaultLedger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestPaymentApproval = (APPaymentApprovalWithoutAuthorisation)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		APPaymentApprovalWithoutAuthorisation TestPaymentApproval;

		#endregion
	}
}
