using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(APPaymentApprovalWithAuthorisation))]
	public class APPaymentApprovalWithAuthorisationTest : PaymentApprovalWithAuthorisationTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var eDocsParsingSupport = TestPaymentApproval as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestProcessEPaymentSecurity()
		{
			Env.Security.NewPayablesPaymentProcessEPayment.IsAllowed = false;
			Assert(!Env.Security.NewPayablesPaymentProcessEPayment.IsAllowed);
			Env.Security.NewPayablesPaymentProcessEPayment.IsAllowed = true;
			Assert(Env.Security.NewPayablesPaymentProcessEPayment.IsAllowed);
		}

		public void TestDeleteClearsPaymentDetailsOnCharges()
		{
			APPaymentApprovalWithAuthorisation approval = (APPaymentApprovalWithAuthorisation)GetNewBusinessObjectForDeleteTest(Factory);
			PaymentApprovalItemCollection items = new PaymentApprovalItemCollection(approval);

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();

			PaymentApprovalItem item = items.AddNew();
			item.A2_AV = approval.PK;
			item.A2_AH = invoice.PK;

			InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
			InvoicingLineBase line2 = (InvoicingLineBase)invoice.Lines.AddNew();
			Charge charge1 = job.Charges.AddNew();
			Charge charge2 = job.Charges.AddNew();
			JobConsolCost cost1 = Factory.NewWithValidTestData<JobConsolCost>();
			JobConsolCost cost2 = Factory.NewWithValidTestData<JobConsolCost>();
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost2.PK;
			charge1.JR_AL_APLine = line1.PK;
			charge2.JR_AL_APLine = line2.PK;

			approval.AV_PaymentType = cost1.E6_PaymentType = charge1.JR_PaymentType = "CHQ";
			approval.AV_AB = cost1.E6_AB_BankAccount = charge1.JR_AB = ZGuid.NewZGuid();
			approval.AV_AK = cost1.E6_AK_ChequeBook = charge1.JR_AK = ZGuid.NewZGuid();
			approval.AV_ChequeOrReference = cost1.E6_ChequeOrReference = charge1.JR_ChequeNo = "12345";

			cost2.E6_PaymentType = charge2.JR_PaymentType = "EFT";
			cost2.E6_AB_BankAccount = charge2.JR_AB = ZGuid.NewZGuid();
			cost2.E6_AK_ChequeBook = charge2.JR_AK = ZGuid.NewZGuid();
			cost2.E6_ChequeOrReference = charge2.JR_ChequeNo = "54321";

			approval.Delete();
			AssertPaymentDetailsCleared(charge1, true);
			AssertPaymentDetailsCleared(charge2, false);
		}

		public void TestPaymentApprovalSecurity()
		{
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCheque, false, ReceiptTypes.Cheque, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCash, false, ReceiptTypes.Cash, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCreditCard, false, ReceiptTypes.CreditCard, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewDirectDebit, false, ReceiptTypes.DirectDebit, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewEFT, false, ReceiptTypes.EFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewSFT, false, ReceiptTypes.ScheduledEFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCRQ, false, ReceiptTypes.CollectionRequest, false);

			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCheque, true, ReceiptTypes.Cheque, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCash, true, ReceiptTypes.Cash, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCreditCard, true, ReceiptTypes.CreditCard, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewDirectDebit, true, ReceiptTypes.DirectDebit, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewEFT, true, ReceiptTypes.EFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewSFT, true, ReceiptTypes.ScheduledEFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewCRQ, true, ReceiptTypes.CollectionRequest, true);

			AssertPaymentApprovalSecurityCase(Env.Security.APPaymentProcessingNewDirectDebit, false, ReceiptTypes.DirectCredit, true);
		}

		public override void TestSetExchangeRate()
		{
			TestPaymentApproval.AV_PayExRate = 2m;
			AssertEquals("Pre-condition", 2m, TestPaymentApproval.AV_PayExRate);
			AssertEquals(false, TestPaymentApproval.IsLoadedFromPaymentBatch);

			TestPaymentApproval.SetExchangeRate(1.5m);

			AssertEquals("Non payment batch approval should have pay ex-rate unchanged.", 2m, TestPaymentApproval.AV_PayExRate);

			TestPaymentApproval.InitializeForPaymentBatch();
			AssertEquals("Pre-condition", 2m, TestPaymentApproval.AV_PayExRate);
			AssertEquals(true, TestPaymentApproval.IsLoadedFromPaymentBatch);

			TestPaymentApproval.SetExchangeRate(1.5m);

			AssertEquals("Payment batch approval should have pay ex-rate updated.", 1.5m, TestPaymentApproval.AV_PayExRate);
		}

		void AssertPaymentDetailsCleared(Charge charge, bool shouldBeCleared)
		{
			AssertEquals("JR_PaymentType", shouldBeCleared, charge.JR_PaymentType.IsEmpty);
			AssertEquals("E6_PaymentType", shouldBeCleared, charge.ParentConsolCost.E6_PaymentType.IsEmpty);
			AssertEquals("JR_AB", shouldBeCleared, charge.JR_AB.IsEmpty);
			AssertEquals("E6_AB_BankAccount", shouldBeCleared, charge.ParentConsolCost.E6_AB_BankAccount.IsEmpty);
			AssertEquals("JR_AK", shouldBeCleared, charge.JR_AK.IsEmpty);
			AssertEquals("E6_AK_ChequeBook", shouldBeCleared, charge.ParentConsolCost.E6_AK_ChequeBook.IsEmpty);
			AssertEquals("JR_ChequeNo", shouldBeCleared, charge.JR_ChequeNo.IsEmpty);
			AssertEquals("E6_ChequeOrReference", shouldBeCleared, charge.ParentConsolCost.E6_ChequeOrReference.IsEmpty);
		}

		[TestDate(2020, 01, 01)]
		public void TestExchangeRateTolerance_SpecifyCurrency()
		{
			AssertExchangeRateToleranceCore(CurrencyCodes.NewZealand);
		}

		[TestDate(2020, 01, 01)]
		public void TestExchangeRateTolerance_AllCurrency()
		{
			AssertExchangeRateToleranceCore(ExchangeRateToleranceLookups.AllCurrencyCode);
		}

		void AssertExchangeRateToleranceCore(string currency)
		{
			var paymentAuthoSettings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(paymentAuthoSettings, RangeCodes.Above, 0, AuthorisationCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentAuthoSettings);

			var exchangeRateTolerancesConfiguration = new ExchangeRateToleranceConfiguration();
			exchangeRateTolerancesConfiguration.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			exchangeRateTolerancesConfiguration.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance() { Currency = currency, ExchangeRateTolerancePercentage = 10m });
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exchangeRateTolerancesConfiguration);

			var approval = CreateApproval(150m, 100m);
			AssertEquals("PreCondition Local Amount", 100m, approval.AV_Calc_LocalAmount);
			AssertEquals("PreCondition Company's Reciprocal", false, Env.CurrentCompany.IsReciprocal);

			UpdateApprovalStatus(approval);
			AssertEquals("PreCondition not in database", false, approval.IsInDatabase);
			CombineAssertions("Test Before Save", () => {
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since data is not saved", approval, approval.AV_Calc_LocalAmountInfo, 180m, false);

				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since data is not saved", approval, approval.AV_PayExRateInfo, 0.5, false);
			});

			Factory.Save();

			UpdateApprovalStatus(approval);
			AssertEquals("PreCondition in database", true, approval.IsInDatabase);
			CombineAssertions("Test After Save", () => {
				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status reset by pay more 10%", approval, approval.AV_Calc_LocalAmountInfo, 111m, true);

				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since pay more in 10%", approval, approval.AV_Calc_LocalAmountInfo, 110m, false);

				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since pay less", approval, approval.AV_Calc_LocalAmountInfo, 50m, false);

				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status reset by pay more 10%", approval, approval.AV_PayExRateInfo, 1, true);

				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since pay more in 10%", approval, approval.AV_PayExRateInfo, 1.40, false);

				UpdateApprovalStatus(approval);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status not reset since pay less", approval, approval.AV_PayExRateInfo, 3, false);
			});

			approval.AV_Amount += 1;
			CombineAssertions("Test After Save & AV_Amount changed, it keep original behavior that not reset since authorizer is current user", () => {
				AssertEquals("PreCondition AV_Amount changed", true, approval.AV_AmountInfo.HasChanges);

				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("pay more 10% via updating amount", approval, approval.AV_Calc_LocalAmountInfo, 120m, false);

				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("pay more 10% via updating exchange rate", approval, approval.AV_PayExRateInfo, 1m, false);
			});
			approval.AV_Amount -= 1;

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			var defaultUserPK = Env.CurrentUser.PK;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				CombineAssertions("Test After Save", () => {
					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status reset by pay more 10%", approval, approval.AV_Calc_LocalAmountInfo, 111m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status not reset since pay more in 10%", approval, approval.AV_Calc_LocalAmountInfo, 110m, false);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status not reset since pay less", approval, approval.AV_Calc_LocalAmountInfo, 80m, false);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status reset by pay more 10%", approval, approval.AV_PayExRateInfo, 1, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status not reset since pay more in 10%", approval, approval.AV_PayExRateInfo, 1.40, false);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("status not reset since pay less", approval, approval.AV_PayExRateInfo, 3, false);
				});

				approval.AV_Amount += 1;
				CombineAssertions("Test After Save & AV_Amount changed, it keep original behavior that reset since it is not original authorizer", () => {
					AssertEquals("PreCondition AV_Amount changed", true, approval.AV_AmountInfo.HasChanges);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("reset status even pay less", approval, approval.AV_Calc_LocalAmountInfo, 80m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
					AssertExchangeRateTolerance("reset status even pay less", approval, approval.AV_PayExRateInfo, 3, true);
				});
				approval.AV_Amount -= 1;
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestExchangeRateTolerance_AuthorizationLevelChange()
		{
			var paymentAuthoSettings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(paymentAuthoSettings, RangeCodes.UpTo, 100, AuthorisationCodes.FirstApprovalRequiredOnly);
			MakeNewAuthorisationSetting(paymentAuthoSettings, RangeCodes.Above, 100, AuthorisationCodes.FirstAndSecondApprovalRequired);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, paymentAuthoSettings);

			var exchangeRateTolerancesConfiguration = new ExchangeRateToleranceConfiguration();
			exchangeRateTolerancesConfiguration.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance() { Currency = "NZD", ExchangeRateTolerancePercentage = 10 });
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exchangeRateTolerancesConfiguration);

			var approval = CreateApproval(142.5m, 95m);
			AssertEquals("PreCondition Local Amount", 95m, approval.AV_Calc_LocalAmount);
			AssertEquals("PreCondition Company's Reciprocal", false, Env.CurrentCompany.IsReciprocal);

			UpdateApprovalStatus(approval);
			Factory.Save();

			AssertEquals("PreCondition in database", true, approval.IsInDatabase);

			approval.AV_Amount += 1;
			UpdateApprovalStatus(approval);
			AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
			AssertExchangeRateTolerance("status reset by authorization level changed, payment amount is changed, keep current user's authorization", approval, approval.AV_Calc_LocalAmountInfo, 101M, true, false);
			approval.AV_Amount -= 1;

			UpdateApprovalStatus(approval);
			AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
			AssertEquals("PreCondition AV_Amount is not changed", false, approval.AV_AmountInfo.HasChanges);
			AssertExchangeRateTolerance("status reset by authorization level changed, payment amount is not changed, keep current user's authorization", approval, approval.AV_Calc_LocalAmountInfo, 101M, true, false);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			var defaultUserPK = Env.CurrentUser.PK;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				approval.AV_Amount += 1;
				UpdateApprovalStatus(approval, defaultUserPK);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertExchangeRateTolerance("status reset by authorization level changed, clear other user's authorization", approval, approval.AV_Calc_LocalAmountInfo, 101M, true, true);
				approval.AV_Amount -= 1;

				UpdateApprovalStatus(approval, defaultUserPK);
				AssertEquals("PreCondition AV_Status", "APP", approval.AV_Status);
				AssertEquals("PreCondition AV_Amount is not changed", false, approval.AV_AmountInfo.HasChanges);
				AssertExchangeRateTolerance("status reset by authorization level changed, clear other user's authorization", approval, approval.AV_Calc_LocalAmountInfo, 101M, true, true);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestExchangeRateTolerance_ZeroTolerance()
		{
			var authorisationSettings = new PaymentAuthorisationSettingsCollection();
			MakeNewAuthorisationSetting(authorisationSettings, RangeCodes.Above, 0, AuthorisationCodes.FirstApprovalRequiredOnly);
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, authorisationSettings);

			var exRateToleranceConfig = new ExchangeRateToleranceConfiguration();
			exRateToleranceConfig.ExchangeRateToleranceCollection.RemoveAndDeleteAll();
			exRateToleranceConfig.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance { Currency = ExchangeRateToleranceLookups.AllCurrencyCode, ExchangeRateTolerancePercentage = 5m });
			exRateToleranceConfig.ExchangeRateToleranceCollection.Add(new ExchangeRateTolerance { Currency = CurrencyCodes.NewZealand, ExchangeRateTolerancePercentage = 0m });
			AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, exRateToleranceConfig);

			var approval = CreateApproval(150m, 139m);
			AssertEquals("Pre-condition", 139m, approval.AV_Calc_LocalAmount);
			AssertEquals(false, Env.CurrentCompany.IsReciprocal);

			UpdateApprovalStatus(approval);
			AssertEquals("Pre-condition: not in DB", false, approval.IsInDatabase);
			CombineAssertions("Test before save", () => {
				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status is NOT reset because the payment approval is not saved.", approval, approval.AV_Calc_LocalAmountInfo, 160m, false);

				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status is NOT reset because the payment approval is not saved.", approval, approval.AV_PayExRateInfo, 0.9m, false);
			});

			Factory.Save();

			UpdateApprovalStatus(approval);
			AssertEquals("Pre-condition: saved", true, approval.IsInDatabase);
			CombineAssertions("Test login with authorizer.", () => {
				UpdateApprovalStatus(approval);
				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled.", approval, approval.AV_Calc_LocalAmountInfo, 160m, false);

				UpdateApprovalStatus(approval);
				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled", approval, approval.AV_Calc_LocalAmountInfo, 100m, false);

				UpdateApprovalStatus(approval);
				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled", approval, approval.AV_PayExRateInfo, 0.9m, false);

				UpdateApprovalStatus(approval);
				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled", approval, approval.AV_PayExRateInfo, 2m, false);
			});

			approval.AV_Amount += 50m;
			CombineAssertions("Test login with authorizer and AV_Amount changed.", () => {
				AssertEquals("PreCondition AV_Amount changed", true, approval.AV_AmountInfo.HasChanges);

				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled.", approval, approval.AV_Calc_LocalAmountInfo, 160m, false);

				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled.", approval, approval.AV_Calc_LocalAmountInfo, 100m, false);

				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled.", approval, approval.AV_PayExRateInfo, 0.9m, false);

				AssertEquals("APP", approval.AV_Status);
				AssertExchangeRateTolerance("Status should NOT be reset. NZD exchange rate tolerance is disabled.", approval, approval.AV_PayExRateInfo, 2m, false);
			});
			approval.AV_Amount -= 50m;

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_EmailAddress = "waiting@outsi.de";
			Factory.Save();

			var defaultUserPK = Env.CurrentUser.PK;
			using (Env.SetTemporaryUserContext(newStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				CombineAssertions("Test login with another user.", () => {
					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_Calc_LocalAmountInfo, 150m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_Calc_LocalAmountInfo, 100m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_PayExRateInfo, 0.9m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_PayExRateInfo, 2m, true);
				});

				approval.AV_Amount += 50m;
				CombineAssertions("Test AV_Amount changed and login with another user. The status is reset, because the login user is NOT the original authorizer.", () => {
					AssertEquals("PreCondition AV_Amount changed", true, approval.AV_AmountInfo.HasChanges);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_Calc_LocalAmountInfo, 150m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_Calc_LocalAmountInfo, 100m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_PayExRateInfo, 0.9m, true);

					UpdateApprovalStatus(approval, defaultUserPK);
					AssertEquals("APP", approval.AV_Status);
					AssertExchangeRateTolerance("Reset Status. Modified by non-authorizer.", approval, approval.AV_PayExRateInfo, 2m, true);
				});
				approval.AV_Amount -= 50m;
			}
		}

		public void TestGetNewValidation()
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			AssertType<APPaymentApprovalWithAuthorisationValidation>(paymentApproval.Validation);
		}

		#region Implementation

		APPaymentApprovalWithAuthorisation CreateApproval(ZDecimal osAMount, ZDecimal localAMount)
		{
			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001001";
			invoice1.AH_RX_NKTransactionCurrency = "NZD";
			invoice1.AH_OSExTaxAmount = osAMount;
			invoice1.AH_LocalExTaxAmount = localAMount;

			var approval = (APPaymentApprovalWithAuthorisation)GetNewBusinessObjectForDeleteTest(Factory);
			approval.AV_Amount = osAMount;
			approval.AV_Calc_LocalAmount = localAMount;
			approval.AV_RX_NKPaymentCurrency = "NZD";
			AssertEquals("Payment Approval OS Partial Payment Amount", osAMount, ((IMatching)approval).OSPartialPaymentAmount);
			AssertEquals("Payment Approval OS Partial Payment Amount", localAMount, ((IMatching)approval).LocalPartialPaymentAmount);

			InvoicingBaseCollection invoices = new InvoicingBaseCollection(Factory);
			invoices.AddRange(new BusinessObject[] { invoice1 });
			AssertEquals(1, invoices.Count);

			approval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());

			((IMatching)invoice1).OSPartialPaymentAmount = -osAMount;

			AssertEquals("Invoice 1 OS Partial Payment Amount", -osAMount, ((IMatching)invoice1).OSPartialPaymentAmount);
			AssertEquals("Session Balance", 0m, approval.PaymentMatchingBaseObject.Balance);
			AssertEquals("SessionBalancesToZero", true, approval.PaymentMatchingBaseObject.SessionBalancesToZero);
			AssertEquals("Should be matchable", true, approval.PaymentMatchingBaseObject.MatchAndClearTransactions());

			return approval;
		}

		void UpdateApprovalStatus(APPaymentApprovalWithAuthorisation aPPaymentApprovalWithAuthorisation, Guid? staffPK = null)
		{
			using (Env.SetTemporaryUserContext(staffPK ?? Env.CurrentUserPK, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				aPPaymentApprovalWithAuthorisation.ApproveFirstApproval();

				var buffer = new NotificationBuffer();
				aPPaymentApprovalWithAuthorisation.TryAuthorisePayment(buffer);
				AssertEquals(false, buffer.HasErrors && !buffer.AsString.Contains("This Payment is already Fully Approved"));
			}
		}

		void AssertExchangeRateTolerance(string comment , APPaymentApprovalWithAuthorisation aPPaymentApprovalWithAuthorisation, ZPropertyInfo prop, ZDecimal updatedValue, bool isResetAuthorization, bool isClear1stAuth = true)
		{
			var autho1 = aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval1st;
			var autho2 = aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval2nd;
			var autho3 = aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval3rd;

			var previouseValue = prop.Value;
			prop.SetValueFromString(updatedValue.ToString());

			if (isResetAuthorization)
			{
				if (isClear1stAuth)
				{
					AssertEquals($"{comment} {nameof(aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval1st)}", ZString.Empty, aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval1st);
				}
				AssertEquals($"{comment} {nameof(aPPaymentApprovalWithAuthorisation.AV_Status)}", "AWA", aPPaymentApprovalWithAuthorisation.AV_Status);
			}
			else
			{
				AssertEquals($"{comment} {nameof(aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval1st)}", autho1, aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval1st);
				AssertEquals($"{comment} {nameof(aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval2nd)}", autho2, aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval2nd);
				AssertEquals($"{comment} {nameof(aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval3rd)}", autho3, aPPaymentApprovalWithAuthorisation.AV_GS_NKApproval3rd);
			}

			prop.SetValueFromString(previouseValue.ToString());
		}

		protected override ZString ExpectedDefaultLedger
		{
			get { return LedgerTypes.AccountsPayable; }
		}

		protected override SecurityCheckpoint FirstApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingThirdApproval; }
		}

		protected override SecurityCheckpoint CancelApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingCancelApproval; }
		}

		protected override SecurityCheckpoint CancelEPaymentCheckPoint
		{
			get { return Env.Security.APPaymentProcessingCancelEPayment; }
		}

		protected override (string, string) GetExpectedCreatorNotification(ZGuid approvalPK)
		{
			var expectedSubject = "AP Payment Request Approved - ZOrg AUD 1000.00";
			var expectedBody = $@"<br/><p>The following AP Payment Request was approved:</p>
<p><a href=""edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=APPaymentProcessing&BusinessEntityPK={approvalPK}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash=%2bMHt%2fmIG%2fGWzG8LrjocCRAo4YNtUsCOq2"">ZOrg - 14-May-20 - AP PAYMENT</a></p>
<p><span style=""font-weight:bold;"">Created on:</span> 14 May 2020 10:16</p>
<p><span style=""font-weight:bold;"">Payment Amount:</span> 1000.00 AUD</p>
<p><span style=""font-weight:bold;"">Organization:</span> ZOrg</p>";

			return (expectedSubject, expectedBody);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestPaymentApproval = (APPaymentApprovalWithAuthorisation)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		APPaymentApprovalWithAuthorisation TestPaymentApproval;

		#endregion
	}
}
