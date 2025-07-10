using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.APInvoice;
using static Enterprise.MasterFiles.Business.AccPaymentApproval;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public abstract class PaymentApprovalBaseTest : AccPaymentApprovalTest
	{
		public void TestAV_Calc_Sequence()
		{
			var dateTime = ZDateTime.Now;
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();

			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			batch.APB_PaymentType = ReceiptTypes.Cash;
			batch.APB_PaymentDate = dateTime;
			batch.APB_AB = bankAccount.PK;
			batch.APB_AK = chequeBook.PK;

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment1.AV_APB_PaymentBatch = batch.PK;
			var payment2 = CreatePaymentApproval(batch);
			var payment3 = CreatePaymentApproval(batch);
			var payment4 = CreatePaymentApproval(batch);

			AssertEquals("Percondition", true, payment1.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", 0, payment1.AV_Calc_Sequence);

			payment2.AV_Status = PaymentApprovalStatus.FullyApproved;
			AssertEquals("Percondition", false, payment2.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment2.IsFullyApproved);
			AssertEquals("Percondition", 3, payment2.AV_Calc_Sequence);

			payment3.AV_Status = PaymentApprovalStatus.Cancelled;
			AssertEquals("Percondition", false, payment3.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment3.IsCancelled);
			AssertEquals("Percondition", 2, payment3.AV_Calc_Sequence);

			payment4.AV_Status = PaymentApprovalStatus.Posted;
			AssertEquals("Percondition", false, payment4.IsDiscrepancyWithPaymentBatch);
			AssertEquals("Percondition", true, payment4.IsPosted);
			AssertEquals("Percondition", 1, payment4.AV_Calc_Sequence);
		}

		PaymentApprovalBase CreatePaymentApproval(AccPaymentBatch batch)
		{
			var paymentApproval = (PaymentApprovalBase)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());

			paymentApproval.AV_APB_PaymentBatch = batch.PK;
			paymentApproval.AV_PaymentType = batch.APB_PaymentType;
			paymentApproval.AV_PaymentDate = batch.APB_PaymentDate;
			paymentApproval.AV_AB = batch.APB_AB;
			paymentApproval.AV_AK = batch.APB_AK;

			return paymentApproval;
		}

		public void TestAV_EPaymentReasonCodeReadonlyness()
		{
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EPayment, ofxBankAccount, TestObjectCreator.USDChequeBook);
			Assert(paymentApproval.IsEPayment);
			Assert("When payment type is EPA, payment reason is not read only.", !paymentApproval.AV_EPaymentReasonCodeInfo.ReadOnly);

			paymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			Assert(!paymentApproval.IsEPayment);
			Assert("When bank account type is not EPA, payment reason is read only.", paymentApproval.AV_EPaymentReasonCodeInfo.ReadOnly);
		}

		public void TestPaymentReasonIsUpdatedWhenCreditorIsChanged()
		{
			var creditor1 = TestObjectCreator.Creditor1;
			creditor1.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail1.A1_IsDefaultAccount = true;
			usdAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;

			var creditor2 = TestObjectCreator.Creditor2;
			creditor2.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail2 = creditor2.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail2.A1_IsDefaultAccount = true;
			usdAccountDetail2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail2.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail2.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase;

			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EPayment, ofxBankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval.AV_OH = creditor1.PK;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_OH = creditor2.PK;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.GoodsPaymentPurchase, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_OH = ZGuid.Empty;
			AssertEquals(ZString.Empty, paymentApproval.AV_EPaymentReasonCode);
		}

		public void TestPaymentReasonIsUpdatedWhenCurrencyIsChanged()
		{
			var creditor1 = TestObjectCreator.Creditor1;
			creditor1.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail1.A1_IsDefaultAccount = true;
			usdAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			var eurAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			eurAccountDetail1.A1_IsDefaultAccount = true;
			eurAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			eurAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices;

			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EPayment, ofxBankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval.AV_OH = creditor1.PK;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.BusinessConsultancyAndPRSevices, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_RX_NKPaymentCurrency = ZString.Empty;
			AssertEquals(ZString.Empty, paymentApproval.AV_EPaymentReasonCode);
		}

		public void TestPaymentReasonIsUpdatedWhenBankAccountIsChanged()
		{
			var creditor1 = TestObjectCreator.Creditor1;
			creditor1.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail1.A1_IsDefaultAccount = true;
			usdAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();

			var paymentApproval = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EPayment, TestObjectCreator.USDBankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval.AV_OH = creditor1.PK;
			paymentApproval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(ZString.Empty, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_AB = ofxBankAccount.PK;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
			AssertEquals(ZString.Empty, paymentApproval.AV_EPaymentReasonCode);
			paymentApproval.AV_EPaymentReasonCode = "AAA";
			paymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
			AssertEquals("Payment reason not updated because previous AV_AB is same as current AV_AB", "AAA", paymentApproval.AV_EPaymentReasonCode);
		}

		public void TestPaymentReasonIsUpdatedWhenPaymentTypeIsChanged()
		{
			var creditor1 = TestObjectCreator.Creditor1;
			creditor1.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail1.A1_IsDefaultAccount = true;
			usdAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();

			var paymentApproval1 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, ofxBankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval1.AV_OH = creditor1.PK;
			paymentApproval1.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(ZString.Empty, paymentApproval1.AV_EPaymentReasonCode);
			paymentApproval1.AV_PaymentType = ReceiptTypes.EPayment;
			AssertEquals(EPaymentReasonCodes.OFXReasonCodes.AccountingServices, paymentApproval1.AV_EPaymentReasonCode);

			var paymentApproval2 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.USDBankAccount, TestObjectCreator.USDChequeBook);
			paymentApproval2.AV_OH = creditor1.PK;
			paymentApproval2.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(ZString.Empty, paymentApproval2.AV_EPaymentReasonCode);
			Factory.Save();

			var newFactory1 = new BusinessObjectFactory();
			var reloadedApproval1 = newFactory1.Load<APPaymentApprovalWithAuthorisation>(paymentApproval1.PK);
			AssertEquals("Existing Payment Reason should not be updated when loading it in new factory.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval1.AV_EPaymentReasonCode);
			reloadedApproval1.AV_PaymentType = ReceiptTypes.Cheque;
			Assert("When existing payment type is changed from EPA to CHQ, payment reason field should be empty.", reloadedApproval1.AV_EPaymentReasonCode.IsEmpty);
			reloadedApproval1.AV_PaymentType = ReceiptTypes.EPayment;
			AssertEquals("When existing account detail payment method is changed back to EPA from CHQ, original payment reason value should be populated.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval1.AV_EPaymentReasonCode);
			var reloadedApproval2 = newFactory1.Load<APPaymentApprovalWithAuthorisation>(paymentApproval2.PK);
			Assert("Existing Payment Reason should not be updated when loading it in new factory.", reloadedApproval2.AV_EPaymentReasonCode.IsEmpty);
			reloadedApproval2.AV_AB = ofxBankAccount.PK;
			reloadedApproval2.AV_PaymentType = ReceiptTypes.EPayment;
			AssertEquals("When existing payment type is changed to EPA, payment reason field should be defaulted from creditor account details.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval2.AV_EPaymentReasonCode);
		}

		public void TestPaymentReasonIsNotUpdatedWhenEPaymentFunctionalityIsDiabled()
		{
			var creditor1 = TestObjectCreator.Creditor1;
			creditor1.CompanyData.AccountDetailsCollection.RemoveAndDeleteAll();
			var usdAccountDetail1 = creditor1.CompanyData.AccountDetailsCollection.AddNew();
			usdAccountDetail1.A1_IsDefaultAccount = true;
			usdAccountDetail1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			usdAccountDetail1.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			usdAccountDetail1.A1_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices;
			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				var paymentApproval1 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.Cheque, TestObjectCreator.USDBankAccount, TestObjectCreator.USDChequeBook);
				paymentApproval1.AV_OH = creditor1.PK;
				paymentApproval1.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				Assert("When E-Payment Functionality is disabled, payment reason field should be empty.", paymentApproval1.AV_EPaymentReasonCode.IsEmpty);
				var paymentApproval2 = TestObjectCreator.CreatePaymentApproval(ReceiptTypes.EPayment, ofxBankAccount, TestObjectCreator.USDChequeBook);
				paymentApproval2.AV_OH = creditor1.PK;
				paymentApproval2.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				Assert("When E-Payment Functionality is disabled, payment reason field should be empty.", paymentApproval2.AV_EPaymentReasonCode.IsEmpty);
				paymentApproval2.AV_EPaymentReasonCode = EPaymentReasonCodes.OFXReasonCodes.AccountingServices; //mimick approval created before disabling E-Payment functionality
				Factory.Save();

				var newFactory1 = new BusinessObjectFactory();
				var reloadedApproval1 = newFactory1.Load<APPaymentApprovalWithAuthorisation>(paymentApproval1.PK);
				Assert("When E-Payment Functionality is disabled, payment reason is not updated.", reloadedApproval1.AV_EPaymentReasonCode.IsEmpty);
				reloadedApproval1.AV_PaymentType = ReceiptTypes.EPayment;
				reloadedApproval1.AV_AB = ofxBankAccount.PK;
				Assert("When E-Payment Functionality is disabled, payment reason is not updated.", reloadedApproval1.AV_EPaymentReasonCode.IsEmpty);
				var reloadedApproval2 = newFactory1.Load<APPaymentApprovalWithAuthorisation>(paymentApproval2.PK);
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval2.AV_EPaymentReasonCode);
				reloadedApproval2.AV_PaymentType = ReceiptTypes.Cheque;
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval2.AV_EPaymentReasonCode);
				reloadedApproval2.AV_PaymentType = EPaymentMethods.EPaymentViaOFX;
				AssertEquals("When E-Payment Functionality is disabled, payment reason is not updated, it will retain its current value.", EPaymentReasonCodes.OFXReasonCodes.AccountingServices, reloadedApproval2.AV_EPaymentReasonCode);
			}
		}

		public void TestHasActiveDeal()
		{
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued);
			Factory.Save();

			var paymentApproval = Factory.Load<PaymentApprovalBase>(deal1.Quote.PaymentApproval.PK);
			AssertEquals(deal1.PK, paymentApproval.CurrentDeal.PK);
			Assert(paymentApproval.HasActiveDeal);

			deal1.AED_Status = DealStatusCodes.Cancelled;
			Factory.Save();
			AssertEquals(deal1.PK, paymentApproval.CurrentDeal.PK);
			Assert(!paymentApproval.HasActiveDeal);

			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, paymentApproval);
			Factory.Save();
			AssertEquals(deal2.PK, paymentApproval.CurrentDeal.PK);
			Assert(paymentApproval.HasActiveDeal);
		}

		public void TestCheckIfDealIsConfirmedByProvider()
		{
			var errorMessageWhenNoDealIsAvailable = "Payment type is an E-Payment, but it doesn't have an E-Payment Deal yet. Please process an E-Payment before posting.";
			var errorMessageWhenDealIsNotConfirmed = "Payment type is an E-Payment, but it doesn't have a confirmed E-Payment Deal yet. You can post the payment once the deal has been 'Accepted' by the provider and the final exchange rate has been confirmed.";

			var ofxBankAccount = TestObjectCreator.CreateEPaymentBankAccount();
			var nonOFXBankAccount = TestObjectCreator.USDBankAccount;

			var paymentApproval = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
			paymentApproval.AV_Amount = 1000m;
			paymentApproval.AV_PayExRate = 1m;
			paymentApproval.AV_OH = TestObjectCreator.Creditor1.PK;
			paymentApproval.AV_AB = nonOFXBankAccount.PK;
			Factory.Save();

			AssertNull(paymentApproval.CurrentDeal);
			Assert(paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage1));
			Assert(errorMessage1.IsEmpty);

			paymentApproval.AV_AB = ofxBankAccount.PK;
			Factory.Save();

			AssertNull(paymentApproval.CurrentDeal);
			Assert(!paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage2));
			AssertEquals(errorMessageWhenNoDealIsAvailable, errorMessage2);

			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, paymentApproval);
			Factory.Save();

			AssertEquals(deal1.PK, paymentApproval.CurrentDeal.PK);
			Assert(!paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage3));
			AssertEquals(errorMessageWhenDealIsNotConfirmed, errorMessage3);

			deal1.AED_Status = DealStatusCodes.Requested;
			Factory.Save();
			AssertEquals(deal1.PK, paymentApproval.CurrentDeal.PK);
			Assert(!paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage4));
			AssertEquals(errorMessageWhenDealIsNotConfirmed, errorMessage4);

			deal1.AED_Status = DealStatusCodes.Accepted;
			deal1.AED_ProviderReference = deal1.PK.ToString();
			deal1.AED_LastResponseReceivedUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals(deal1.PK, paymentApproval.CurrentDeal.PK);
			Assert(paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage5));
			Assert(errorMessage5.IsEmpty);
			AssertEquals(deal1.PK.ToString().Substring(0, 8), paymentApproval.DealProviderReference);

			deal1.AED_Status = DealStatusCodes.Failed;
			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, paymentApproval);
			deal2.AED_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(2); //to ensure it is newer than the previous deal.
			Factory.Save();

			AssertEquals(deal2.PK, paymentApproval.CurrentDeal.PK);
			Assert(!paymentApproval.CheckIfDealIsConfirmedByProvider(out var errorMessage6));
			AssertEquals(errorMessageWhenDealIsNotConfirmed, errorMessage6);
		}

		public abstract void TestProcessEPaymentSecurity();

		public void TestDraftPaymentApprovalDoesNotPostOnSave()
		{
			var approvalWithoutAuthorisation = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			approvalWithoutAuthorisation.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Assert("Approval without authorisation is not in draft status, approval should post on save.", approvalWithoutAuthorisation.PostsOnSave);

			approvalWithoutAuthorisation.AV_Status = PaymentApprovalStatus.Draft;
			Assert("Approval without authorisation is in draft status and NOT linked to payment batch, approval should not post on save.", !approvalWithoutAuthorisation.PostsOnSave);

			var paymentBatch = Factory.New<AccPaymentBatch>();
			approvalWithoutAuthorisation.AV_APB_PaymentBatch = paymentBatch.PK;
			Assert("Approval without authorisation is in draft status and linked to payment batch, approval should post on save.", approvalWithoutAuthorisation.PostsOnSave);

			var approvalWithAuthorisation = Factory.New<APPaymentApprovalWithAuthorisation>();
			approvalWithAuthorisation.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Assert("Approval with authorisation is not in draft status, approval should not post on save.", !approvalWithAuthorisation.PostsOnSave);

			approvalWithAuthorisation.AV_Status = PaymentApprovalStatus.Draft;
			Assert("Approval with authorisation is in draft status and NOT linked to payment batch, approval should not post on save.", !approvalWithAuthorisation.PostsOnSave);

			approvalWithAuthorisation.AV_APB_PaymentBatch = paymentBatch.PK;
			Assert("Approval with authorisation is in draft status and linked to payment batch, approval should not post on save.", !approvalWithAuthorisation.PostsOnSave);
		}

		public void TestAccPaymentBatchStatusIsUpdatedToCompletedWhenCurrentApprovalIsPostedAndAllOtherApprovalsArePosted()
		{
			var approval1 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval1.AV_PostDate = ZDateTime.Today;
			approval1.AV_OH = TestObjectCreator.AALSHI.PK;
			approval1.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval1.AV_Amount = 100m;
			approval1.AV_Status = PaymentApprovalStatus.Draft;

			var approval2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval2.AV_PostDate = ZDateTime.Today;
			approval2.AV_OH = TestObjectCreator.AALSHI.PK;
			approval2.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval2.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval2.AV_Amount = 200m;
			approval2.AV_Status = PaymentApprovalStatus.Posted;

			var approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval3.AV_PostDate = ZDateTime.Today;
			approval3.AV_OH = TestObjectCreator.AALSHI.PK;
			approval3.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval3.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval3.AV_Amount = 300m;
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			var accPaymentBatch = Factory.New<AccPaymentBatch>();
			accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval3.AV_APB_PaymentBatch = accPaymentBatch.PK;

			accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

			Factory.Save();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in AwaitingApproval status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in Rejected status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();
			AssertEquals("Should update batch status to Completed because all approvals are posted.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch.APB_Status);
		}

		public void TestAccPaymentBatchStatusIsUpdatedToCompletedWhenCurrentApprovalIsPostedAndSomeApprovalsArePostedAndOtherApprovalsAreCancelled()
		{
			var approval1 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval1.AV_PostDate = ZDateTime.Today;
			approval1.AV_OH = TestObjectCreator.AALSHI.PK;
			approval1.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval1.AV_Amount = 100m;
			approval1.AV_Status = PaymentApprovalStatus.Draft;

			var approval2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval2.AV_PostDate = ZDateTime.Today;
			approval2.AV_OH = TestObjectCreator.AALSHI.PK;
			approval2.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval2.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval2.AV_Amount = 200m;
			approval2.AV_Status = PaymentApprovalStatus.Posted;

			var approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval3.AV_PostDate = ZDateTime.Today;
			approval3.AV_OH = TestObjectCreator.AALSHI.PK;
			approval3.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval3.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval3.AV_Amount = 300m;
			approval3.AV_Status = PaymentApprovalStatus.Cancelled;

			var accPaymentBatch = Factory.New<AccPaymentBatch>();
			accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval3.AV_APB_PaymentBatch = accPaymentBatch.PK;

			accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

			Factory.Save();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in AwaitingApproval status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in Rejected status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();
			AssertEquals("Should update batch status to Completed because two approvals are posted and one approval is cancelled.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch.APB_Status);
		}

		public void TestAccPaymentBatchStatusIsUpdatedToCompletedWhenCurrentApprovalIsCancelledAndSomeApprovalsArePostedAndOtherApprovalsAreCancelled()
		{
			var approval1 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval1.AV_PostDate = ZDateTime.Today;
			approval1.AV_OH = TestObjectCreator.AALSHI.PK;
			approval1.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval1.AV_Amount = 100m;
			approval1.AV_Status = PaymentApprovalStatus.Draft;

			var approval2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval2.AV_PostDate = ZDateTime.Today;
			approval2.AV_OH = TestObjectCreator.AALSHI.PK;
			approval2.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval2.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval2.AV_Amount = 200m;
			approval2.AV_Status = PaymentApprovalStatus.Posted;

			var approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval3.AV_PostDate = ZDateTime.Today;
			approval3.AV_OH = TestObjectCreator.AALSHI.PK;
			approval3.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval3.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval3.AV_Amount = 300m;
			approval3.AV_Status = PaymentApprovalStatus.Cancelled;

			var accPaymentBatch = Factory.New<AccPaymentBatch>();
			accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval3.AV_APB_PaymentBatch = accPaymentBatch.PK;

			accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

			Factory.Save();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in AwaitingApproval status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in Rejected status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();
			AssertEquals("Should update batch status to Completed because two approvals are cancelled and one approval is posted.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch.APB_Status);
		}

		public void TestAccPaymentBatchStatusIsUpdatedWhenCurrentApprovalIsDeleted()
		{
			AssertWorking();
			AssertCancelled();
			AssertCompleted();

			void AssertWorking()
			{
				var accPaymentBatch = Factory.New<AccPaymentBatch>();
				accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;

				var approvalDraft = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalDraft.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalDraft.AV_Status = PaymentApprovalStatus.Draft;

				var approvalPosted = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalPosted.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalPosted.AV_Status = PaymentApprovalStatus.Posted;

				var approvalCancelled = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalCancelled.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalCancelled.AV_Status = PaymentApprovalStatus.Cancelled;

				accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

				Factory.Save();
				AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

				approvalCancelled.Delete();

				Factory.Save();
				AssertEquals("Should remain batch status to Working because remaining approvals are not all posted or canceled", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);
			}

			void AssertCompleted()
			{
				var accPaymentBatch = Factory.New<AccPaymentBatch>();
				accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;

				var approvalDraft = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalDraft.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalDraft.AV_Status = PaymentApprovalStatus.Draft;

				var approvalPosted = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalPosted.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalPosted.AV_Status = PaymentApprovalStatus.Posted;

				var approvalCancelled = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalCancelled.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalCancelled.AV_Status = PaymentApprovalStatus.Cancelled;

				accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

				Factory.Save();
				AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

				approvalDraft.Delete();

				Factory.Save();
				AssertEquals("Should update batch status to Completed because remaining approvals are posted or canceled", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch.APB_Status);
			}

			void AssertCancelled()
			{
				var accPaymentBatch = Factory.New<AccPaymentBatch>();
				accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;

				var approvalDraft = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalDraft.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalDraft.AV_Status = PaymentApprovalStatus.Draft;

				var approvalPosted = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalPosted.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalPosted.AV_Status = PaymentApprovalStatus.Posted;

				var approvalCancelled = TestObjectCreator.CreatePaymentApproval(typeof(APPaymentApprovalWithAuthorisation), "AP", TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook);
				approvalCancelled.AV_APB_PaymentBatch = accPaymentBatch.PK;
				approvalCancelled.AV_Status = PaymentApprovalStatus.Cancelled;

				accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

				Factory.Save();
				AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

				approvalDraft.Delete();
				approvalPosted.Delete();

				Factory.Save();
				AssertEquals("Should update batch status to Canceled because remaining approvals are canceled", Core.Constants.AccPaymentBatchStatus.Cancelled, accPaymentBatch.APB_Status);
			}
		}

		public void TestAccPaymentBatchStatusIsUpdatedToCancelledWhenCurrentApprovalIsCancelledAndAllOtherApprovalsAreCancelled()
		{
			var approval1 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval1.AV_PostDate = ZDateTime.Today;
			approval1.AV_OH = TestObjectCreator.AALSHI.PK;
			approval1.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval1.AV_Amount = 100m;
			approval1.AV_Status = PaymentApprovalStatus.Draft;

			var approval2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval2.AV_PostDate = ZDateTime.Today;
			approval2.AV_OH = TestObjectCreator.AALSHI.PK;
			approval2.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval2.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval2.AV_Amount = 200m;
			approval2.AV_Status = PaymentApprovalStatus.Cancelled;

			var approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval3.AV_PostDate = ZDateTime.Today;
			approval3.AV_OH = TestObjectCreator.AALSHI.PK;
			approval3.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			approval3.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			approval3.AV_Amount = 300m;
			approval3.AV_Status = PaymentApprovalStatus.Cancelled;

			var accPaymentBatch = Factory.New<AccPaymentBatch>();
			accPaymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			approval1.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval3.AV_APB_PaymentBatch = accPaymentBatch.PK;

			accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

			Factory.Save();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in AwaitingApproval status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Rejected;
			Factory.Save();
			AssertEquals("Should not update batch status because approval 1 is still in Rejected status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch.APB_Status);

			approval1.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();
			AssertEquals("Should update batch status to Cancelled because all approvals are cancelled.", Core.Constants.AccPaymentBatchStatus.Cancelled, accPaymentBatch.APB_Status);
		}

		public void TestMatchedTransactionsBalanceWithPaymentAmount()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 94M;
			testAPInv.AH_OSExTaxAmount = 94M;
			testAPInv.AH_Desc = "For Payment Approval 1";

			TestPaymentApproval.AV_OH = testOrg.PK;

			AssertEquals("Balance should be thousand", 1000m, TestPaymentApproval.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(TestPaymentApproval.MatchingBaseObject.Balance, TestPaymentApproval.MatchedTransactionsBalanceWithPaymentAmount);

			TestPaymentApproval.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			TestPaymentApproval.MatchingBaseObject.UnmatchedTransactions.Add(testAPInv);
			TestPaymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(new BusinessObject[1] { testAPInv });

			AssertNotEquals("Balance should not be thousand", 1000m, TestPaymentApproval.MatchedTransactionsBalanceWithPaymentAmount);
			AssertEquals(TestPaymentApproval.MatchingBaseObject.Balance, TestPaymentApproval.MatchedTransactionsBalanceWithPaymentAmount);
		}

		public void TestGetPropertyReadonlyness()
		{
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			Factory.Save();

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			var payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			var payment3 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			var payment4 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment1.AV_APB_PaymentBatch = batch.PK;
			payment2.AV_APB_PaymentBatch = batch.PK;
			payment3.AV_APB_PaymentBatch = batch.PK;
			payment4.AV_APB_PaymentBatch = batch.PK;

			payment1.InitializeForPaymentBatch(() => false);
			payment2.InitializeForPaymentBatch(() => true);
			payment3.InitializeForPaymentBatch(() => true);

			payment1.AV_Status = PaymentApprovalStatus.Cancelled;
			payment2.AV_Status = PaymentApprovalStatus.Posted;
			payment3.AV_Status = PaymentApprovalStatus.FullyApproved;
			payment4.AV_Status = PaymentApprovalStatus.Cancelled;

			AssertEquals("Percondition", true, payment1.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment2.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", true, payment3.IsEditOrViewPaymentBatch);
			AssertEquals("Percondition", false, payment4.IsEditOrViewPaymentBatch);

			AssertEquals(true, payment1.GetPropertyReadonlyness_ForTestOnly(payment1.AV_AmountInfo.PropertyDescriptor));
			AssertEquals(true, payment2.GetPropertyReadonlyness_ForTestOnly(payment2.AV_AmountInfo.PropertyDescriptor));
			AssertEquals(false, payment3.GetPropertyReadonlyness_ForTestOnly(payment3.AV_AmountInfo.PropertyDescriptor));
			AssertEquals(false, payment4.GetPropertyReadonlyness_ForTestOnly(payment4.AV_AmountInfo.PropertyDescriptor));
		}

		void AssertIsDiscrepancyWithPaymentBatch(Action<PaymentApprovalBase> updateAction)
		{
			var dateTime = ZDateTime.Now;
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();

			var payment1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment1.InitializeForPaymentBatch(() => false);
			payment1.AV_APB_PaymentBatch = batch.PK;

			var payment2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment2.InitializeForPaymentBatch(() => false);
			payment2.AV_APB_PaymentBatch = batch.PK;

			batch.APB_PaymentType = payment1.AV_PaymentType = payment2.AV_PaymentType = ReceiptTypes.Cash;
			batch.APB_PaymentDate = payment1.AV_PaymentDate = payment2.AV_PaymentDate = dateTime;
			batch.APB_AB = payment1.AV_AB = payment2.AV_AB = bankAccount.PK;
			batch.APB_AK = payment1.AV_AK = payment2.AV_AK = chequeBook.PK;

			Factory.Save();

			AssertEquals(false, payment1.IsDiscrepancyWithPaymentBatch);
			AssertEquals(false, payment2.IsDiscrepancyWithPaymentBatch);

			updateAction(payment1);

			AssertEquals(true, payment1.IsDiscrepancyWithPaymentBatch);
			AssertEquals(false, payment2.IsDiscrepancyWithPaymentBatch);
		}

		public void TestIsDiscrepancyWithPaymentBatch()
		{
			AssertIsDiscrepancyWithPaymentBatch((payment1) => {
				payment1.AV_PaymentDate = ZDateTime.Now;
			});

			AssertIsDiscrepancyWithPaymentBatch((payment1) => {
				payment1.AV_PaymentType = ReceiptTypes.CreditCard;
			});

			AssertIsDiscrepancyWithPaymentBatch((payment1) => {
				var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
				payment1.AV_AB = bankAccount.PK;
			});

			AssertIsDiscrepancyWithPaymentBatch((payment1) => {
				var chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
				payment1.AV_AK = chequeBook.PK;
			});
		}

		public virtual void TestSetExchangeRate()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			TestPaymentApproval.AV_PayExRate = 1.5m;
			AssertEquals(1.5m, TestPaymentApproval.AV_PayExRate);
			TestPaymentApproval.SetExchangeRate(1.22m);
			AssertEquals(1.22m, TestPaymentApproval.AV_PayExRate);
		}

		public void TestPaymentItemsTotalAmount()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = Accounting.Business.TestObjectCreator.GetRandomString(10);
			testOrg.CompanyData.OB_IsCreditor = true;
			testOrg.APSettlementGroupPK = testOrg.PK;

			APInvoice testAPInv = Factory.NewWithValidTestData<APInvoice>();
			testAPInv.AH_OH = testOrg.PK;
			testAPInv.AH_LocalExTaxAmount = 94M;
			testAPInv.AH_OSExTaxAmount = 94M;
			testAPInv.AH_Desc = "For Payment Approval 1";

			TestPaymentApproval.AV_OH = testOrg.PK;
			AssertEquals("TestPaymentItemsTotalAmount should be thousand", 1000m, TestPaymentApproval.PaymentItemsTotalAmount);

			TestPaymentApproval.MatchingBaseObject.MatchedTransactions.RemoveAllFromAllCollections();
			TestPaymentApproval.MatchingBaseObject.UnmatchedTransactions.Add(testAPInv);
			TestPaymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(new BusinessObject[1] { testAPInv });

			AssertNotEquals("TestPaymentItemsTotalAmount should be changed", -94.0, TestPaymentApproval.PaymentItemsTotalAmount);
		}

		public void TestIsCancelledOrIsPosted()
		{
			AssertEquals("IsCancelledOrIsPosted should be false", TestPaymentApproval.IsCancelledOrIsPosted, false);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			AssertEquals("IsCancelledOrIsPosted should be true because it's posted", TestPaymentApproval.IsCancelledOrIsPosted, true);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Cancelled;
			AssertEquals("IsCancelledOrIsPosted should be true because it's cancelled", TestPaymentApproval.IsCancelledOrIsPosted, true);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			AssertEquals("IsCancelledOrIsPosted should be false", TestPaymentApproval.IsCancelledOrIsPosted, false);
		}

		public void TestIsEditOrViewPaymentBatch()
		{
			AssertEquals("IsEditOrViewPaymentBatch should be false because it does not belong to a payment batch", TestPaymentApproval.IsEditOrViewPaymentBatch, false);

			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			TestPaymentApproval.AV_APB_PaymentBatch = batch.PK;
			AssertEquals("IsEditOrViewPaymentBatch should be false because it's not saved", TestPaymentApproval.IsEditOrViewPaymentBatch, false);

			Factory.Save();
			AssertEquals("IsEditOrViewPaymentBatch should be false because it's not created from poster", TestPaymentApproval.IsEditOrViewPaymentBatch, false);

			TestPaymentApproval.InitializeForPaymentBatch(() => true);
			AssertEquals("IsEditOrViewPaymentBatch should be true", TestPaymentApproval.IsEditOrViewPaymentBatch, true);
		}

		public void TestInitializeForPaymentBatch()
		{
			AssertEquals("Pre-condition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);

			var batch = Factory.NewWithValidTestData<AccPaymentBatch>();
			TestPaymentApproval.AV_APB_PaymentBatch = batch.PK;
			AssertEquals("Pre-condition", false, TestPaymentApproval.IsLoadedFromPaymentBatch);

			TestPaymentApproval.InitializeForPaymentBatch(null);
			AssertEquals(true, TestPaymentApproval.IsLoadedFromPaymentBatch);
		}

		#region E-Payment Quote

		public void TestPaymentQuotes()
		{
			var quote1 = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			quote1.QU_Status = QuoteStatusCodes.Discarded;
			quote1.QU_LastResponseReceivedUtc = ZDateTime.Now;
			quote1.QU_ToAmount = quote1.QU_FromAmount = TestPaymentApproval.AV_Amount;
			quote1.QU_ExchangeRate = quote1.QU_ExchangeRateInverted = 1;
			Factory.Save();

			var quote2 = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals(2, TestPaymentApproval.PaymentQuotes.Count);
			Assert(TestPaymentApproval.PaymentQuotes.Contains(quote1));
			Assert(TestPaymentApproval.PaymentQuotes.Contains(quote2));
		}

		public void TestCreateQuote_NoExistingActiveQuote()
		{
			var quote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals(TestPaymentApproval.PK, quote.QU_AV);
			AssertEquals(EPaymentProviderCodes.Codes.OFX, quote.QU_ProviderCode);
			AssertEquals(TestPaymentApproval.AV_Amount, quote.QU_ToAmount);
			AssertEquals(TestPaymentApproval.AV_RX_NKPaymentCurrency, quote.QU_RX_NKToCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, quote.QU_RX_NKFromCurrency);
			AssertEquals(GlbCompany.CurrentCompany.PK, quote.QU_GC);
		}

		public void TestCreateQuote_HasExistingActiveQuote()
		{
			var paymentAmount = TestPaymentApproval.AV_Amount;
			TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			var (quote2, error, quoteStatus2) = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			AssertNull("Quote", quote2);
			AssertEquals("ErrorMessage", "Exchange rate already requested", error);

			TestPaymentApproval.AV_Amount = 250M;
			Factory.Save();

			var (quote4, error4, quoteStatus4) = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			AssertNotNull("Quote", quote4);
			AssertEquals("ErrorMessage", string.Empty, error4);

			TestPaymentApproval.AV_Amount = paymentAmount;
			TestPaymentApproval.AV_RX_NKPaymentCurrency = "EUR";
			Factory.Save();

			var (quote5, error5, quoteStatus5) = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			AssertNotNull("Quote", quote5);
			AssertEquals("ErrorMessage", string.Empty, error5);
		}

		public void TestCreateQuote_FundingCurrencyFromPaymentBatch()
		{
			var accBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			accBankAccount.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.China;
			Factory.Save();

			var aPPaymentBatchPoster = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			aPPaymentBatchPoster.APB_AB_FundingBankAccount = accBankAccount.PK;
			Factory.Save();

			var paymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			paymentApproval.InitializeForPaymentBatch(() => false);
			paymentApproval.AV_APB_PaymentBatch = aPPaymentBatchPoster.PK;
			Factory.Save();

			var quote = paymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;

			AssertEquals(accBankAccount.AB_RX_NKAccountCurrency, quote.QU_RX_NKFromCurrency);
		}

		public void TestCreateQuote_FundingCurrencyFromSinglePayment()
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			paymentApproval.AV_OH = TestObjectCreator.Debtor.PK;
			Factory.Save();

			var quote = paymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			AssertEquals("USD", quote.QU_RX_NKFromCurrency);
		}

		public void TestFundingBankAccount()
		{
			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			AssertEquals(ZGuid.Empty, paymentApproval.AV_AB_FundingBankAccount);
			AssertEquals(ZGuid.Empty, paymentApproval.FundingBankAccountPK);
			AssertEquals(Env.CurrentCompany.LocalCurrency.Code, paymentApproval.FundingCurrency);

			paymentApproval.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			AssertEquals(TestObjectCreator.USDBankAccount.PK, paymentApproval.AV_AB_FundingBankAccount);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, paymentApproval.FundingBankAccountPK);
			AssertEquals("USD", paymentApproval.FundingCurrency);

			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_BatchNumber = "0001";
			paymentApproval.AV_APB_PaymentBatch = paymentBatch.PK;

			var expectError = $@"This payment belongs to Payment Batch {paymentBatch.APB_BatchNumber}.
Funding Bank Account needs to be changed for the batch in the Manage > Payables > Payment Batches module.";
			AssertEquals(TestObjectCreator.USDBankAccount.PK, paymentApproval.AV_AB_FundingBankAccount);
			AssertEquals(TestObjectCreator.USDBankAccount.PK, paymentApproval.FundingBankAccountPK);
			AssertEquals("USD", paymentApproval.FundingCurrency);
			paymentApproval.RunPreSaveValidation();
			AssertHasError(paymentApproval.FundingBankAccountPKInfo,expectError);

			paymentApproval.FundingBankAccountPK = paymentBatch.APB_AB_FundingBankAccount;
			AssertEquals(paymentBatch.FundingBankAccountCurrency, paymentApproval.FundingCurrency);
			AssertNoErrors(paymentApproval.FundingBankAccountPKInfo);
		}

		public void TestDiscardActiveFXQuotes_PaymentDetailsDoesNotChange()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = "USD";
			var failedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			failedQuote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			var errorQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			errorQuote.QU_Status = QuoteStatusCodes.Error;
			errorQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			Factory.Save();

			var acceptedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			acceptedQuote.QU_Status = QuoteStatusCodes.Accepted;
			acceptedQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			acceptedQuote.QU_ExchangeRate = 2.0M;
			acceptedQuote.QU_ExchangeRateInverted = 0.5M;
			acceptedQuote.QU_FeeAmount = 20M;
			acceptedQuote.QU_RX_NKFeeCurrency = "AUD";
			acceptedQuote.QU_FromAmount = 2000M;
			acceptedQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var expiredQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			expiredQuote.QU_Status = QuoteStatusCodes.Expired;
			expiredQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			expiredQuote.QU_ExchangeRate = 2.0M;
			expiredQuote.QU_ExchangeRateInverted = 0.5M;
			expiredQuote.QU_FeeAmount = 20M;
			expiredQuote.QU_RX_NKFeeCurrency = "AUD";
			expiredQuote.QU_FromAmount = 2000M;
			expiredQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var queuedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			queuedQuote.QU_Status = QuoteStatusCodes.Queued;
			Factory.Save();

			TestPaymentApproval.DiscardActiveFXQuotes(EPaymentProviderCodes.Codes.OFX, discardOnlyIfPaymentDetailsAreChanged: false);
			AssertEquals(nameof(failedQuote), QuoteStatusCodes.Discarded, failedQuote.QU_Status);
			AssertEquals(nameof(errorQuote), QuoteStatusCodes.Discarded, errorQuote.QU_Status);
			AssertEquals(nameof(acceptedQuote), QuoteStatusCodes.Discarded, acceptedQuote.QU_Status);
			AssertEquals(nameof(expiredQuote), QuoteStatusCodes.Discarded, expiredQuote.QU_Status);
			AssertEquals(nameof(queuedQuote), QuoteStatusCodes.Queued, queuedQuote.QU_Status);
		}

		public void TestDiscardActiveFXQuotes_PaymentDetailsChanged_Case1()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = "USD";
			var failedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			failedQuote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			var errorQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			errorQuote.QU_Status = QuoteStatusCodes.Error;
			errorQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			Factory.Save();

			var acceptedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			acceptedQuote.QU_Status = QuoteStatusCodes.Accepted;
			acceptedQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			acceptedQuote.QU_ExchangeRate = 2.0M;
			acceptedQuote.QU_ExchangeRateInverted = 0.5M;
			acceptedQuote.QU_FeeAmount = 20M;
			acceptedQuote.QU_RX_NKFeeCurrency = "AUD";
			acceptedQuote.QU_FromAmount = 2000M;
			acceptedQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var expiredQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			expiredQuote.QU_Status = QuoteStatusCodes.Expired;
			expiredQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			expiredQuote.QU_ExchangeRate = 2.0M;
			expiredQuote.QU_ExchangeRateInverted = 0.5M;
			expiredQuote.QU_FeeAmount = 20M;
			expiredQuote.QU_RX_NKFeeCurrency = "AUD";
			expiredQuote.QU_FromAmount = 2000M;
			expiredQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var queuedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			queuedQuote.QU_Status = QuoteStatusCodes.Queued;
			Factory.Save();

			TestPaymentApproval.AV_Amount = 2589M;
			Factory.Save();

			TestPaymentApproval.DiscardActiveFXQuotes(EPaymentProviderCodes.Codes.OFX, discardOnlyIfPaymentDetailsAreChanged: false);
			AssertEquals(nameof(failedQuote), QuoteStatusCodes.Discarded, failedQuote.QU_Status);
			AssertEquals(nameof(errorQuote), QuoteStatusCodes.Discarded, errorQuote.QU_Status);
			AssertEquals(nameof(acceptedQuote), QuoteStatusCodes.Discarded, acceptedQuote.QU_Status);
			AssertEquals(nameof(expiredQuote), QuoteStatusCodes.Discarded, expiredQuote.QU_Status);
			AssertEquals(nameof(queuedQuote), QuoteStatusCodes.Discarded, queuedQuote.QU_Status);
		}

		public void TestDiscardActiveFXQuotes_PaymentDetailsChanged_Case2()
		{
			TestPaymentApproval.AV_RX_NKPaymentCurrency = "USD";
			var failedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			failedQuote.QU_Status = QuoteStatusCodes.Failed;
			Factory.Save();

			var errorQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			errorQuote.QU_Status = QuoteStatusCodes.Error;
			errorQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			Factory.Save();

			var acceptedQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			acceptedQuote.QU_Status = QuoteStatusCodes.Accepted;
			acceptedQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			acceptedQuote.QU_ExchangeRate = 2.0M;
			acceptedQuote.QU_ExchangeRateInverted = 0.5M;
			acceptedQuote.QU_FeeAmount = 20M;
			acceptedQuote.QU_RX_NKFeeCurrency = "AUD";
			acceptedQuote.QU_FromAmount = 2000M;
			acceptedQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			var expiredQuote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			expiredQuote.QU_Status = QuoteStatusCodes.Expired;
			expiredQuote.QU_LastResponseReceivedUtc = ZDateTime.Now;
			expiredQuote.QU_ExchangeRate = 2.0M;
			expiredQuote.QU_ExchangeRateInverted = 0.5M;
			expiredQuote.QU_FeeAmount = 20M;
			expiredQuote.QU_RX_NKFeeCurrency = "AUD";
			expiredQuote.QU_FromAmount = 2000M;
			expiredQuote.QU_ProviderReference = "PRF001";
			Factory.Save();

			TestPaymentApproval.AV_Amount = 2589M;
			Factory.Save();

			TestPaymentApproval.DiscardActiveFXQuotes(EPaymentProviderCodes.Codes.OFX, discardOnlyIfPaymentDetailsAreChanged: false);
			AssertEquals(nameof(failedQuote), QuoteStatusCodes.Discarded, failedQuote.QU_Status);
			AssertEquals(nameof(errorQuote), QuoteStatusCodes.Discarded, errorQuote.QU_Status);
			AssertEquals(nameof(acceptedQuote), QuoteStatusCodes.Discarded, acceptedQuote.QU_Status);
			AssertEquals(nameof(expiredQuote), QuoteStatusCodes.Discarded, expiredQuote.QU_Status);
		}

		public void TestCurrentDeal()
		{
			var deal1 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, TestPaymentApproval);
			Factory.Save();
			AssertEquals(deal1.PK, TestPaymentApproval.CurrentDeal.PK);

			var deal2 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, TestPaymentApproval);
			deal2.AED_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			Factory.Save();
			AssertEquals(deal2.PK, TestPaymentApproval.CurrentDeal.PK);

			var deal3 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, TestPaymentApproval);
			deal3.AED_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			AssertEquals(deal2.PK, TestPaymentApproval.CurrentDeal.PK);

			var deal4 = TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Queued, TestPaymentApproval);
			deal4.AED_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			Factory.Save();
			AssertEquals(deal4.PK, TestPaymentApproval.CurrentDeal.PK);
		}

		#endregion

		public void TestSetValues()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", TestObjectCreator.USD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.Debtor);
			charge.JR_AB = TestObjectCreator.USDBankAccount.PK;
			charge.JR_AK = TestObjectCreator.USDChequeBook.PK;
			charge.JR_ChequeNo = "CHQ1234";
			var apInvoice = CreateInvoice(Factory, TestObjectCreator.Creditor1, typeof(APInvoice), 10m, "00001011", TestObjectCreator.USD);
			apInvoice.AH_ExchangeRate = 1.2m;
			var postingTime = ZDateTime.Now.AddDays(2);

			ClearPaymentApprovalFieldsForTest();

			using (AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Precondition: Payment Comment (Description)", ZString.Empty, TestPaymentApproval.AV_PaymentComment);
				AssertEquals("Precondition: PaymentDate", ZDateTime.Empty, TestPaymentApproval.AV_PaymentDate);
				AssertEquals("Precondition: Currency", ZString.Empty, TestPaymentApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Precondition: Payment Type", ZString.Empty, TestPaymentApproval.AV_PaymentType);
				AssertEquals("Precondition: Organisation", ZGuid.Empty, TestPaymentApproval.AV_OH);
				AssertEquals("Precondition: Bank Account", ZGuid.Empty, TestPaymentApproval.AV_AB);
				AssertEquals("Precondition: Cheque Book", ZGuid.Empty, TestPaymentApproval.AV_AK);
				AssertEquals("Precondition: Cheque Reference", ZString.Empty, TestPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Precondition: Exchange Rate", 0M, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Precondition: Branch", ZGuid.Empty, TestPaymentApproval.AV_GB);
				AssertEquals("Precondition: Company", ZGuid.Empty, TestPaymentApproval.AV_GC);

				TestPaymentApproval.SetValues(apInvoice, job, charge, postingTime);

				AssertEquals("Postcondition: Payment Comment (Description)", APPayment.DefaultDescriptionForJobRelatedPayment + " " + job.JH_JobNum, TestPaymentApproval.AV_PaymentComment);
				AssertEquals("Postcondition: PaymentDate", postingTime, TestPaymentApproval.AV_PaymentDate);
				AssertEquals("Postcondition: Currency", TestObjectCreator.USD.Code, TestPaymentApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Postcondition: Payment Type", "", TestPaymentApproval.AV_PaymentType);
				AssertEquals("Postcondition: Organisation", TestObjectCreator.Creditor1.PK, TestPaymentApproval.AV_OH);
				AssertEquals("Postcondition: Bank Account", TestObjectCreator.USDBankAccount.PK, TestPaymentApproval.AV_AB);
				AssertEquals("Postcondition: Cheque Book", TestObjectCreator.USDChequeBook.PK, TestPaymentApproval.AV_AK);
				AssertEquals("Postcondition: Cheque Reference", "CHQ1234", TestPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Postcondition: Exchange Rate", 1.2M, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Postcondition: Branch", GlbBranch.CurrentBranch.PK, TestPaymentApproval.AV_GB);
				AssertEquals("Postcondition: Company", GlbCompany.CurrentCompany.PK, TestPaymentApproval.AV_GC);
			}

			ClearPaymentApprovalFieldsForTest();
			TestPaymentApproval.RelatedCharges.Clear();
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;

			using (AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Precondition: Payment Comment (Description)", ZString.Empty, TestPaymentApproval.AV_PaymentComment);
				AssertEquals("Precondition: PaymentDate", ZDateTime.Empty, TestPaymentApproval.AV_PaymentDate);
				AssertEquals("Precondition: Currency", ZString.Empty, TestPaymentApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Precondition: Payment Type", ZString.Empty, TestPaymentApproval.AV_PaymentType);
				AssertEquals("Precondition: Organisation", ZGuid.Empty, TestPaymentApproval.AV_OH);
				AssertEquals("Precondition: Bank Account", ZGuid.Empty, TestPaymentApproval.AV_AB);
				AssertEquals("Precondition: Cheque Book", ZGuid.Empty, TestPaymentApproval.AV_AK);
				AssertEquals("Precondition: Cheque Reference", ZString.Empty, TestPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Precondition: Exchange Rate", 0M, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Precondition: Branch", ZGuid.Empty, TestPaymentApproval.AV_GB);
				AssertEquals("Precondition: Company", ZGuid.Empty, TestPaymentApproval.AV_GC);

				TestPaymentApproval.SetValues(apInvoice, job, charge, postingTime);

				AssertEquals("Postcondition: Payment Comment (Description)", APPayment.DefaultDescriptionForJobRelatedPayment + " " + job.JH_JobNum, TestPaymentApproval.AV_PaymentComment);
				AssertEquals("Postcondition: PaymentDate", postingTime, TestPaymentApproval.AV_PaymentDate);
				AssertEquals("Postcondition: Currency", TestObjectCreator.USD.Code, TestPaymentApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Postcondition: Payment Type", "", TestPaymentApproval.AV_PaymentType);
				AssertEquals("Postcondition: Organisation", TestObjectCreator.Creditor1.PK, TestPaymentApproval.AV_OH);
				AssertEquals("Postcondition: Bank Account", TestObjectCreator.USDBankAccount.PK, TestPaymentApproval.AV_AB);
				AssertEquals("Postcondition: Cheque Book", TestObjectCreator.USDChequeBook.PK, TestPaymentApproval.AV_AK);
				AssertEquals("Postcondition: Cheque Reference", "CHQ1234", TestPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Postcondition: Exchange Rate", 1.2M, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Postcondition: Branch", TestObjectCreator.NonCurrentBranch.PK, TestPaymentApproval.AV_GB);
				AssertEquals("Postcondition: Company", TestObjectCreator.NonCurrentBranch.GB_GC, TestPaymentApproval.AV_GC);
			}

			void ClearPaymentApprovalFieldsForTest()
			{
				TestPaymentApproval.AV_PaymentComment = ZString.Empty;
				TestPaymentApproval.AV_PaymentDate = ZDateTime.Empty;
				TestPaymentApproval.AV_RX_NKPaymentCurrency = ZString.Empty;
				TestPaymentApproval.AV_PaymentType = ZString.Empty;
				TestPaymentApproval.AV_OH = ZGuid.Empty;
				TestPaymentApproval.AV_AB = ZGuid.Empty;
				TestPaymentApproval.AV_AK = ZGuid.Empty;
				TestPaymentApproval.AV_ChequeOrReference = ZString.Empty;
				TestPaymentApproval.AV_PayExRate = 0M;
				TestPaymentApproval.AV_GB = ZGuid.Empty;
				TestPaymentApproval.AV_GC = ZGuid.Empty;
			}
		}

		public void TestSetFieldsFromPayment()
		{
			var testDate = ZDateTime.Now.AddDays(2);
			var isARLedger = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable;
			var payment = isARLedger ?
				(Payment)TestObjectCreator.CreateARPayment(1.5m, 15m, testDate, testDate, TestObjectCreator.ActiveOrg.PK, TestObjectCreator.USDBankAccount.PK) :
				TestObjectCreator.CreateAPPayment(1.5m, 15m, testDate, testDate, TestObjectCreator.ActiveOrg.PK, TestObjectCreator.USDBankAccount.PK);

			payment.AH_InvoiceDate = testDate;
			payment.AH_PostDate = testDate;
			payment.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			payment.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			payment.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			payment.AH_OH = TestObjectCreator.Creditor1.PK;
			payment.AH_Desc = "Test Description";
			payment.AH_ReceiptType = ReceiptTypes.Cheque;
			payment.AH_AB = TestObjectCreator.USDBankAccount.PK;
			payment.ChequeBook = TestObjectCreator.USDChequeBook.PK;
			payment.AH_ChequeOrReference = "CHQ123";
			payment.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			payment.AH_OSExTaxAmount = 15m;
			payment.AH_LocalExTaxAmount = 10m;
			payment.AH_ExchangeRate = 1.5m;
			payment.AH_NumberOfSupportingDocuments = 2;
			payment.AH_OA_InvoiceAddressOverride = TestObjectCreator.ABIGAS.MainAddress.PK;
			payment.AH_OC_InvoiceContactOverride = TestObjectCreator.AALSHI.MainAddress.PK;

			TestPaymentApproval.AV_PaymentDate = ZDateTime.Empty;
			TestPaymentApproval.AV_PostDate = ZDateTime.Empty;
			TestPaymentApproval.AV_GB = ZGuid.Empty;
			TestPaymentApproval.DepartmentForImport_ForTestOnly = ZGuid.Empty;
			TestPaymentApproval.AV_GC = ZGuid.Empty;
			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_PaymentComment = ZString.Empty;
			TestPaymentApproval.AV_PaymentType = ZString.Empty;
			TestPaymentApproval.AV_AB = ZGuid.Empty;
			TestPaymentApproval.AV_AK = ZGuid.Empty;
			TestPaymentApproval.AV_ChequeOrReference = ZString.Empty;
			TestPaymentApproval.AV_RX_NKPaymentCurrency = ZString.Empty;
			TestPaymentApproval.AV_Amount = 0m;
			TestPaymentApproval.AV_Calc_LocalAmount = 0m;
			TestPaymentApproval.AV_PayExRate = 0m;
			TestPaymentApproval.AH_NumberOfSupportingDocuments = 0;
			TestPaymentApproval.AV_OA_AddressOverride = ZGuid.Empty;
			TestPaymentApproval.AV_OC_ContactOverride = ZGuid.Empty;

			AssertEquals("Precondition: PaymentDate", ZDateTime.Empty, TestPaymentApproval.AV_PaymentDate);
			AssertEquals("Precondition: PostDate", ZDateTime.Empty, TestPaymentApproval.AV_PostDate);
			AssertEquals("Precondition: Branch", ZGuid.Empty, TestPaymentApproval.AV_GB);
			AssertEquals("Precondition: DepartmentForImport", ZGuid.Empty, TestPaymentApproval.DepartmentForImport_ForTestOnly);
			AssertEquals("Precondition: Company", ZGuid.Empty, TestPaymentApproval.AV_GC);
			AssertEquals("Precondition: Organisation", ZGuid.Empty, TestPaymentApproval.AV_OH);
			AssertEquals("Precondition: Payment Comment (Description)", ZString.Empty, TestPaymentApproval.AV_PaymentComment);
			AssertEquals("Precondition: Payment Type", ZString.Empty, TestPaymentApproval.AV_PaymentType);
			AssertEquals("Precondition: Bank Account", ZGuid.Empty, TestPaymentApproval.AV_AB);
			AssertEquals("Precondition: Cheque Book", ZGuid.Empty, TestPaymentApproval.AV_AK);
			AssertEquals("Precondition: Cheque Reference", ZString.Empty, TestPaymentApproval.AV_ChequeOrReference);
			AssertEquals("Precondition: Currency", ZString.Empty, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Precondition: Exchange Rate", 0M, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Precondition: Amount", 0M, TestPaymentApproval.AV_Amount);
			AssertEquals("Precondition: Calc_Local_Amount", 0m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Precondition: NumberOfSupportingDocuments", 0, TestPaymentApproval.AH_NumberOfSupportingDocuments.ToZInt());
			AssertEquals("Precondition: AV_OA_AddressOverride", ZGuid.Empty, TestPaymentApproval.AV_OA_AddressOverride);
			AssertEquals("Precondition: AV_OC_ContactOverride", ZGuid.Empty, TestPaymentApproval.AV_OC_ContactOverride);

			TestPaymentApproval.SetFieldsFromPayment(payment);

			AssertEquals("Postcondition: PaymentDate", testDate, TestPaymentApproval.AV_PaymentDate);
			AssertEquals("Postcondition: PostDate", testDate, TestPaymentApproval.AV_PostDate);
			AssertEquals("Postcondition: Branch", TestObjectCreator.NonCurrentBranch.PK, TestPaymentApproval.AV_GB);
			AssertEquals("Postcondition: DepartmentForImport", TestObjectCreator.NonCurrentDepartment.PK, TestPaymentApproval.DepartmentForImport_ForTestOnly);
			AssertEquals("Postcondition: Company", TestObjectCreator.NonCurrentCompany.PK, TestPaymentApproval.AV_GC);
			AssertEquals("Postcondition: Organisation", TestObjectCreator.Creditor1.PK, TestPaymentApproval.AV_OH);
			AssertEquals("Postcondition: Payment Comment (Description)", "Test Description", TestPaymentApproval.AV_PaymentComment);
			AssertEquals("Postcondition: Payment Type", ReceiptTypes.Cheque, TestPaymentApproval.AV_PaymentType);
			AssertEquals("Postcondition: Bank Account", TestObjectCreator.USDBankAccount.PK, TestPaymentApproval.AV_AB);
			AssertEquals("Postcondition: Cheque Book", TestObjectCreator.USDChequeBook.PK, TestPaymentApproval.AV_AK);
			AssertEquals("Postcondition: Cheque Reference", "CHQ123", TestPaymentApproval.AV_ChequeOrReference);
			AssertEquals("Postcondition: Currency", TestObjectCreator.USD.Code, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Postcondition: Exchange Rate", 1.5M, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Postcondition: Amount", 15M, TestPaymentApproval.AV_Amount);
			AssertEquals("Postcondition: Calc_Local_Amount", 10m, TestPaymentApproval.AV_Calc_LocalAmount);
			AssertEquals("Postcondition: NumberOfSupportingDocuments", 2, TestPaymentApproval.AH_NumberOfSupportingDocuments.ToZInt());
			AssertEquals("Postcondition: AV_OA_AddressOverride", TestObjectCreator.ABIGAS.MainAddress.PK, TestPaymentApproval.AV_OA_AddressOverride);
			AssertEquals("Postcondition: AV_OC_ContactOverride", TestObjectCreator.AALSHI.MainAddress.PK, TestPaymentApproval.AV_OC_ContactOverride);
		}

		public void TestPaymentApprovalReferenceIsSetOnSaving()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CC1");
			var branch1 = TestObjectCreator.CreateBranch("BB1", company1);
			var branch2 = TestObjectCreator.CreateBranch("BB2", company1);

			var company2 = TestObjectCreator.CreateNewCompany("CC2");
			var branch3 = TestObjectCreator.CreateBranch("BB3", company2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval1 = CreatePaymentApproval();
				var approval2 = CreatePaymentApproval();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { approval1, approval2 }.Select(x => x.AV_PaymentApprovalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval3 = CreatePaymentApproval();
				var approval4 = CreatePaymentApproval();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001002", "00001003" }, new[] { approval3, approval4 }.Select(x => x.AV_PaymentApprovalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval5 = CreatePaymentApproval();
				var approval6 = CreatePaymentApproval();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { approval5, approval6 }.Select(x => x.AV_PaymentApprovalReference));
			}

			PaymentApprovalBase CreatePaymentApproval()
			{
				var approval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
				approval.AV_PostDate = ZDateTime.Today;
				approval.AV_OH = TestObjectCreator.AALSHI.PK;
				approval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				approval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
				approval.AV_Amount = 300m;
				return approval;
			}
		}

		public void TestCorrectFountainIsUsedToSetPaymentApprovalReference()
		{
			var company1 = TestObjectCreator.CreateNewCompany("CC1");
			var branch1 = TestObjectCreator.CreateBranch("BB1", company1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var apApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
				apApproval.AV_PostDate = ZDateTime.Today;
				apApproval.AV_OH = TestObjectCreator.AALSHI.PK;
				apApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				apApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
				apApproval.AV_Amount = 300m;

				var arApproval = Factory.New<ARPaymentApprovalWithAuthorisation>();
				arApproval.AV_PostDate = ZDateTime.Today;
				arApproval.AV_OH = TestObjectCreator.AALSHI.PK;
				arApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				arApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
				arApproval.AV_Amount = 300m;

				Factory.Save();

				AssertEquals("00001000", apApproval.AV_PaymentApprovalReference);
				AssertEquals("00001000", arApproval.AV_PaymentApprovalReference);
			}
		}

		public virtual void TestIMatchingProperties()
		{
			AssertEquals("LocalPartialPaymentAmount", ((IMatching)TestPaymentApproval).LocalPartialPaymentAmountInfo.Name);
		}

		public void TestPaymentCurrencyCodeInfo()
		{
			AssertEquals(TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.Name, ((IMatching)TestPaymentApproval).PaymentCurrencyCodeInfo.Name);
		}

		public void TestInvoiceTransactionReference()
		{
			Assert("Invoice Transaction Reference should be empty", ((IMatching)TestPaymentApproval).InvoiceTransactionReference.IsEmpty);
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesPaymentApprovalBase()
		{
			PaymentApprovalBase testPayment = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;

			var localList = new List<string>
			{
				nameof(testPayment.OutstandingAmount),
				nameof(testPayment.OriginalOutstandingAmount),
				nameof(testPayment.LocalPartialPaymentAmount),
				nameof(testPayment.AV_Calc_LocalAmount),
				nameof(testPayment.AV_Discount),
				nameof(testPayment.AV_ExchangeDifference)
			};

			var osList = new List<string>
			{
				nameof(testPayment.OSPartialPaymentAmount),
				nameof(testPayment.OverseasTotalAmount),
				nameof(testPayment.OSOutstandingAmount),
				nameof(testPayment.AV_Amount)
			};

			var exList = new List<string>
			{
				nameof(testPayment.ExchangeRateAmount)
			};

			var tester = new DecimalPlacesAttributeTester(testPayment);
			tester.CheckLocalCurrency(localList, nameof(testPayment.LocalRXDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(testPayment.RXDecimals), nameof(testPayment.AV_RX_NKPaymentCurrency), testPayment);
			tester.CheckExchangeRate(exList, nameof(testPayment.ExchangeRateDecimals));
		}

		public void TestDefaultPaymentType()
		{
			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			AssertEquals(ReceiptTypes.Cheque, testPaymentApproval.AV_PaymentType);

			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			AssertEquals(ReceiptTypes.Cash, testPaymentApproval.AV_PaymentType);
		}

		public void TestAV_RX_NKPaymentCurrency_ReadOnly()
		{
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert(!TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);

			TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
			Assert(TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);
		}

		public void TestDefaultPaymentTypeReferenceNumber()
		{
			AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			AccountingConfigurationRegistry.Instance.DefaultReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			AssertEquals(ReceiptTypes.CreditCard, testPaymentApproval.AV_PaymentType);
			AssertEquals("Test1", testPaymentApproval.AV_ChequeOrReference);

			testPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, testPaymentApproval.AV_PaymentType);
			AssertEquals("CSH", testPaymentApproval.AV_ChequeOrReference);
		}

		public void TestCreateNewPaymentWillUseMaxPostDateForMatchDate()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			testBookWithAutoAllocation.AK_IsActive = ZBool.True;
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			ZDateTime approvalPostDate = ZDateTime.Today.AddDays(-10);
			testPaymentApproval.AV_PostDate = approvalPostDate;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 300m;

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("00001001", TestObjectCreator.AUD, 1m, 150m, 0m, 0m, 150m, 0m, 0m);
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_PostDate = approvalPostDate.AddDays(5);
			invoice1.Lines[0].GenericCharge = TestObjectCreator.GLHeader1.PK;
			invoice1.Lines[0].AL_Desc = "test";

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("00001002", TestObjectCreator.AUD, 1m, 150m, 0m, 0m, 150m, 0m, 0m);
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_PostDate = approvalPostDate;
			invoice2.Lines[0].GenericCharge = TestObjectCreator.GLHeader1.PK;
			invoice2.Lines[0].AL_Desc = "test";

			PaymentApprovalItem approvalItem1 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			approvalItem1.A2_AV = testPaymentApproval.PK;
			approvalItem1.A2_AH = invoice1.PK;
			approvalItem1.A2_PaymentThisRun = -150m;

			PaymentApprovalItem approvalItem2 = Factory.NewWithValidTestData<PaymentApprovalItem>();
			approvalItem2.A2_AV = testPaymentApproval.PK;
			approvalItem2.A2_AH = invoice2.PK;
			approvalItem2.A2_PaymentThisRun = -150m;

			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 300m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			Factory.Save();

			ZQuery paymentMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, testPaymentApproval.NewPayment_ForTestOnly.PK);
			TransactionMatchLinkCollection newPaymentMatchLinks = new TransactionMatchLinkCollection(Factory, paymentMatchLinkQuery);
			newPaymentMatchLinks.Load();
			ZString matchGroupNum = newPaymentMatchLinks[0].AP_MatchGroupNum;
			ZQuery matchLinksQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory, matchLinksQuery);
			matchLinks.Load();
			for (int i = 0; i < matchLinks.Count; i++)
			{
				AssertEquals("Match Date on the match links should be set to the max post date of the matched transactions", invoice1.AH_PostDate, matchLinks[i].AP_MatchDate);
			}
		}

		public void TestPaymentMatchingBaseObjectNotCreatedIfNullAndThereAreNoChanges()
		{
			Factory.Save();
			var matchingFactory = new BusinessObjectFactory();
			var approval = matchingFactory.Load<PaymentApprovalBase>(TestPaymentApproval.PK);
			AssertEquals("IsPostWithoutMatching", false, approval.IsPostWithoutMatching);
			AssertEquals("Payment approval object has no changes", false, approval.HasChanges);
			AssertNull("fPaymentMatchingBaseObject_ForTestOnly", approval.PaymentMatchingBaseObject_ForTestOnly);
			matchingFactory.Save();
			AssertNull("fPaymentMatchingBaseObject_ForTestOnly should still be null after saving", approval.PaymentMatchingBaseObject_ForTestOnly);
		}

		public void TestDeleteTemporaryTransactionsWhenSavingWhereApprovalMatchingHasNoChange()
		{
			APInvoice apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 200m, 0m, 0m, 200m, 0m, 0m, TestObjectCreator.AALSHI);
			TestPaymentApproval.AV_OH = TestObjectCreator.AALSHI.PK;
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = "AUD";
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 200m;
			Factory.Save();

			var invoicesToMatch = new BusinessObject[] { apInv };

			TestPaymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(invoicesToMatch.ToArray());
			AssertEquals("Matched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions should contain apInv", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(apInv));
			Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			TestPaymentApproval.MatchingBaseObject.MatchAndClearTransactions();
			Factory.Save();

			var apDiscount = TestObjectCreator.CreateAPDiscount(200m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestPaymentApproval.MatchingBaseObject.MoveAllFromMatchToUnmatch();
			TestPaymentApproval.MatchingBaseObject.AddMiscellaneousTransaction(apDiscount);
			AssertEquals("Matched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions should contain apDiscount", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(apDiscount));
			Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			TestPaymentApproval.MatchingBaseObject.MatchAndClearTransactions();
			AssertNoExceptionThrown("Matched Successfully", delegate
			{ Factory.Save(); });
		}

		[SuspendCriticalValidation]
		public void TestHasReversedTransaction()
		{
			BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
			TestObjectCreator testInvoiceCreator = new TestObjectCreator(invoiceCreationFactory);

			APInvoice invoice1 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001100", testInvoiceCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			invoice1.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			invoiceCreationFactory.Save();
			invoice1 = Factory.Load<APInvoice>(invoice1.PK);

			APInvoice invoice2 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001101", testInvoiceCreator.AUD, 1m, 200m, 0m, 200m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			invoice2.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			invoiceCreationFactory.Save();
			invoice2 = Factory.Load<APInvoice>(invoice2.PK);

			AccBankAccount bankAccount = TestObjectCreator.AUDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			AccChequeBook chequeBook = TestObjectCreator.AUDChequeBook;

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.AV_AB = bankAccount.PK;
			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = "AUD";
			TestPaymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 300m;

			AssertEquals("Unmatched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
			Assert("Unmatched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

			TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1));
			Assert("Matched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2));
			Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
			Factory.Save();

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("Approval Items Created", 2, approvalItems.Count);

			AssertEquals(false, TestPaymentApproval.HasReversedTransaction);

			invoice1.AH_IsCancelled = true;
			AssertEquals(true, TestPaymentApproval.HasReversedTransaction);
		}

		#region Test Display Hot Cheques Event

		public void TestDisplayHotChequesEvent()
		{
			AccHotCheque hotCheque01 = CreateHotCheque(TestOrgHeader, "001001", false);
			AccHotCheque hotCheque02 = CreateHotCheque(TestOrgHeader, "001002", false);
			AccHotCheque hotCheque03 = CreateHotCheque(TestOrgHeader, "001003", true);
			AccHotCheque hotCheque04 = CreateHotCheque(TestOrgHeader2, "001004", false);

			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
			AccHotChequeCollection activeHotCheques = TestPaymentApproval.GetActiveHotCheques_ForTestOnly();

			AssertEquals("Active Hot Cheques Count", 1, activeHotCheques.Count);

			AssertEquals("Active Hot Cheques contains Cheque01", false, activeHotCheques.Contains(hotCheque01));
			AssertEquals("Active Hot Cheques contains Cheque02", false, activeHotCheques.Contains(hotCheque02));
			AssertEquals("Active Hot Cheques contains Cheque03", false, activeHotCheques.Contains(hotCheque03));
			AssertEquals("Active Hot Cheques contains Cheque04", true, activeHotCheques.Contains(hotCheque04));

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			activeHotCheques = TestPaymentApproval.GetActiveHotCheques_ForTestOnly();

			AssertEquals("Active Hot Cheques Count", 2, activeHotCheques.Count);
			AssertEquals("Active Hot Cheques contains Cheque01", true, activeHotCheques.Contains(hotCheque01));
			AssertEquals("Active Hot Cheques contains Cheque02", true, activeHotCheques.Contains(hotCheque02));
			AssertEquals("Active Hot Cheques contains Cheque03", false, activeHotCheques.Contains(hotCheque03));
			AssertEquals("Active Hot Cheques contains Cheque04", false, activeHotCheques.Contains(hotCheque04));

			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
			TestPaymentApproval.DisplayHotCheques += new PaymentApprovalBase.HotChequeSelectedHandler(TestPaymentApproval_DisplayHotCheques);

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			Expected = TestPaymentApproval.IsPayables ? 1 : 0;
			AssertEquals("DisplayHotChequesEvent should have been called", Expected, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestPaymentApproval_DisplayHotCheques(object sender, HotChequeLink link)
		{
			NumberOfTimesEventWasCalledDuringTest++;
			AssertNotNull("Event Sender should be of type PaymentApprovalBase");

			PaymentApprovalBase sourceOfEvent = sender as PaymentApprovalBase;
			AssertEquals("Event Sender", TestPaymentApproval.PK, sourceOfEvent.PK);
		}

		AccHotCheque CreateHotCheque(OrgHeader header, ZString chequeNumber, bool cancelled)
		{
			AccHotCheque newHotCheque = Factory.New<AccHotCheque>();
			newHotCheque.AQ_OH = header.PK;
			newHotCheque.AQ_AK = TestObjectCreator.AUDChequeBook.PK;
			newHotCheque.AQ_Cancelled = cancelled;
			newHotCheque.AQ_AH = ZGuid.Empty;
			newHotCheque.AQ_ChequeNumber = chequeNumber;
			return newHotCheque;
		}

		public void TestShouldAllocateChequeNumber()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = true;

			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, 1000m);

			TestPaymentApproval.CreateNewPayment();
			TestPaymentApproval.NewPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			TestPaymentApproval.NewPayment.ChequeBook = chequeBook.PK;
			TestPaymentApproval.NewPayment.AH_ChequeOrReference = ZString.Empty;
			Assert(TestPaymentApproval.ShouldAllocateChequeNumber_ForTestOnly());

			TestPaymentApproval.NewPayment.AH_ChequeOrReference = "0001";
			Assert(!TestPaymentApproval.ShouldAllocateChequeNumber_ForTestOnly());
		}

		#endregion

		#region Test Notify User Payment Uneditable Event

		public void TestNotifyUserPaymentUneditableEvent()
		{
			AccHotCheque hotCheque = Factory.New<AccHotCheque>();
			hotCheque.AQ_OH = TestOrgHeader.PK;
			hotCheque.AQ_Description = "AP PAYMENT DESCRIPTION";
			hotCheque.AQ_AK = TestObjectCreator.AUDChequeBook.PK;
			hotCheque.AQ_Amount = 1000M;

			TestPaymentApproval.FImportedHotCheque_ForTestOnly = hotCheque;
			TestPaymentApproval.NotifyUserPaymentUneditable += new PaymentApprovalBase.PaymentFieldsUneditableHandler(TestPaymentApproval_NotifyUserPaymentUneditable);

			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_OHError;
			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;

			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_ABError;
			TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;

			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_AKError;
			TestPaymentApproval.AV_AK = TestObjectCreator.USDChequeBook.PK;

			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_PaymentTypeError;
			TestPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;

			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_ChequeOrReferenceError;
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.GetRandomString(6);

			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;
			hotCheque.AQ_Amount = 50M;
			ExpectedEventMessageForTest = PaymentApprovalBase.HotChequeErrorMessages.AV_ActualAmountError;
			TestPaymentApproval.AV_Amount = 500M;

			hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Max;
			ExpectedEventMessageForTest = HotChequeErrorMessages.GetMaximumError(hotCheque.AQ_Amount);
			TestPaymentApproval.AV_Amount = 500M;

			AssertEquals("Event should have been called 7 times", 7, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestPaymentApproval_NotifyUserPaymentUneditable(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
			AssertEquals("Event Message", ExpectedEventMessageForTest, message);
			ExpectedEventMessageForTest = ZString.Empty;
		}

		#endregion

		#region Test Log Creation

		public void TestCreateAddLog()
		{
			Factory.Save();
			StmALog[] logs = Factory.Load<StmALog>(QueryToLoadEvents(AutoEvents.AddedARecordToTheSystem, TestPaymentApproval));
			AssertEquals("Logs Length", 1, logs.Length);
			AssertNotNull("ADD Log", logs[0]);
			AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.AddedARecordToTheSystem, ZString.Empty);
		}

		public virtual void TestCreatePaymentPostedLog()
		{
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();

			StmALog[] logs = Factory.Load<StmALog>(QueryToLoadEvents(AutoEvents.TransactionPosted, TestPaymentApproval));
			AssertEquals("Logs Length", 1, logs.Length);
			AssertNotNull("Log", logs[0]);
			ZString expectedDescription = TestPaymentApproval.PostsOnSave ? PaymentApprovalBase.UserCreatedAndPostedTransactionLogText : string.Empty;
			AssertLogIsCreatedCorrectly(logs[0], TestPaymentApproval, AutoEvents.TransactionPosted, expectedDescription);
		}

		protected void AssertLogIsCreatedCorrectly(StmALog log, PaymentApprovalBase approval, Event @event, ZString reference)
		{
			AssertLogIsCreatedCorrectly(log, approval, @event, reference, GlbStaff.CurrentUser);
		}

		protected void AssertLogIsCreatedCorrectly(StmALog log, PaymentApprovalBase approval, Event @event, ZString reference, GlbStaff staff)
		{
			AssertZDatesWithin5Minutes("Log EventTime", ZDateTime.Now, log.SL_EventTime);
			AssertEquals("Log Table", AccPaymentApprovalSchema.Constants.TableName, log.SL_Table);
			AssertEquals("Log Parent", approval.PK, log.SL_Parent);
			AssertEquals("Log NKEvent", @event.Code, log.SL_SE_NKEvent);
			AssertEquals("Log Reference", reference, log.SL_Reference);
			AssertEquals("Log User Code", staff.GS_Code, log.SL_GS_NKUser);
		}

		protected ZQuery QueryToLoadEvents(Event @event, PaymentApprovalBase approval)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, approval.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			return query;
		}

		protected ZQuery QueryToLoadEvents(Event @event, PaymentApprovalBase approval, ZString reference)
		{
			ZQuery query = QueryToLoadEvents(@event, approval);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);
			return query;
		}

		#endregion

		public virtual void TestShouldPostPaymentDefaultValue()
		{
			AssertEquals("Payment Approval by default should NOT allow post", false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
		}

		public void TestShouldPostPaymentWhenPostActionReturnFalse()
		{
			TestPaymentApproval.InitializeForPaymentBatch(() => false);
			AssertEquals("ShouldPost action returns false", false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);

			TestPaymentApproval.InitializeForPaymentBatch();
			AssertEquals("The value is unchanged.", false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
		}

		public virtual void TestShouldPostPaymentWhenPostActionReturnTrue()
		{
			TestPaymentApproval.InitializeForPaymentBatch(() => true);
			AssertEquals("Payment Approval Base Type should always NOT allow post", false, TestPaymentApproval.ShouldPostPayment_ForTestOnly);
		}

		public void TestPaymentAutomaticallyPostedIfFullyApprovedAndRegistrySet()
		{
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			TestPaymentApproval.AV_ChequeOrReference = "CSH";
			TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, 1000m);
			Factory.Save();

			if (TestPaymentApproval.IsAllowedToPost)
			{
				AssertEquals(PaymentApprovalStatus.Posted, TestPaymentApproval.AV_Status);
				AssertNotNull("A Payment must be posted as that's payment without authorisation.", TestPaymentApproval.NewPayment);
			}
			else
			{
				AssertEquals(PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);
				AssertNull("A Payment must not be posted, because payment with authorisation must be posted only through Post menu in the module or from Billing Screen.", TestPaymentApproval.NewPayment);
			}
		}

		public void TestMatchStatusAndReasonCodeProperties()
		{
			var matchStatus = "UAC";
			var matchStatusReasonCode = "ADV";

			TestPaymentApproval.MatchStatus = matchStatus;
			TestPaymentApproval.MatchStatusReasonCode = matchStatusReasonCode;

			Factory.Save();

			AssertEquals(matchStatus, TestPaymentApproval.MatchStatus);
			AssertEquals(matchStatusReasonCode, TestPaymentApproval.MatchStatusReasonCode);
		}

		#region TestIsCreatedForPosting

		public void TestIsCreatedForPosting()
		{
			TestPaymentApproval.IsAllowedToPost = true;
			AssertEquals("IsCreatedForPosting must have the same value as has been set.", GetIsCreatedForPostingValue(TestPaymentApproval), TestPaymentApproval.IsAllowedToPost);

			TestPaymentApproval.IsAllowedToPost = false;
			AssertEquals("IsCreatedForPosting must have the same value as has been set.", GetIsCreatedForPostingValue(TestPaymentApproval), TestPaymentApproval.IsAllowedToPost);
		}

		protected abstract bool GetIsCreatedForPostingValue(PaymentApprovalBase paymentApproval);

		#endregion

		public void TestAV_RXAllowsForeignCurrencyPaymentFromLocalCurrencyBankAccounts()
		{
			AssertEquals("Precondition: Local Currency should be AUD", TestObjectCreator.AUD.RX_Code, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
			AssertHasErrors("AUD payment from USD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			AssertHasErrors("GBP payment from USD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			AssertNoErrors("USD payment from USD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);

			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
			AssertNoErrors("AUD payment from AUD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			AssertNoErrors("GBP payment from AUD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);

			TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			AssertNoErrors("USD payment from AUD bank account", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo);
		}

		public void TestIsENettPayment()
		{
			foreach (FieldInfo receiptTypeField in typeof(ReceiptTypes).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				string receiptType = (string)receiptTypeField.GetValue(null);
				TestPaymentApproval.AV_PaymentType = receiptType;
				AssertEquals(receiptType, receiptType == ReceiptTypes.eNettDirectDebit, TestPaymentApproval.IsENettPayment);
			}
		}

		public void TestUseExchangeRateFromENettWebService()
		{
			EnettRegistrationCode originalRegistrationCode = AccountingConfigurationRegistry.Instance.ENettRegistration.Value;
			ENettRegisteredBankAccountCollection originalBankAccountCollection = AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.Value;
			try
			{
				OrgHeader orgHeader = TestObjectCreator.AALSHI;
				EnettRegistrationCode newRegistrationCode = new EnettRegistrationCode();
				newRegistrationCode.RegistrationCode = "123123";
				newRegistrationCode.OrganisationPK = orgHeader.PK;
				AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newRegistrationCode);
				Assert("ENettRegistration", !AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty);

				AccBankAccount usdBankAccount = TestObjectCreator.USDBankAccount;
				Factory.Save();
				ENettRegisteredBankAccountCollection bankAccountCollection = new ENettRegisteredBankAccountCollection();
				ENettRegisteredBankAccount account = bankAccountCollection.AddNew();
				account.BankAccountPK = usdBankAccount.PK;
				AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bankAccountCollection);

				Assert("BankAccountEnettRegistered", eNettHelper.IsBankAccountEnettRegistered(usdBankAccount));

				OrgCusCode orgCusCode = orgHeader.CustomsCodes.AddNew();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
				orgCusCode.OK_CustomsRegNo = "123123";
				TestPaymentApproval.AV_OH = orgHeader.PK;

				Assert("OrganisationEnettRegistered", eNettHelper.IsOrganisationeNettRegistered(orgHeader));

				TestPaymentApproval.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
				TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
				TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
				Assert("AUD/AUD", !TestPaymentApproval.UseExchangeRateFromENettWebService);
				TestPaymentApproval.AV_AB = usdBankAccount.PK;
				TestPaymentApproval.AV_RX_NKPaymentCurrency = TestObjectCreator.AUD.RX_Code;
				Assert("USD/AUD", TestPaymentApproval.UseExchangeRateFromENettWebService);
				TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
				Assert("Not an eNett type", !TestPaymentApproval.UseExchangeRateFromENettWebService);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistrationCode);
				AccountingConfigurationRegistry.Instance.ENettRegisteredBankAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalBankAccountCollection);
			}
		}

		public void TestAV_ChequeOrReferenceMaxLength()
		{
			AssertEquals(Schema.AV_ChequeOrReferenceMaxLength, TestPaymentApproval.AV_ChequeOrReferenceInfo.MaxLength);
			AssertEquals(Schema.AV_ChequeOrReferenceMaxLength, TestPaymentApproval.ChequeOrReferenceInfo.MaxLength);
		}

		public void TestSaveReloadAndPostPaymentApprovalItemsWithSameForeignCurrencyTransactions()
		{
			ZDecimal invoice1OSAmount = 216m;
			ZDecimal invoice1ExchangeRate = 0.72m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = TestObjectCreator.USD;

			ZDecimal invoice2OSAmount = 375m;
			ZDecimal invoice2ExchangeRate = 0.75m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = TestObjectCreator.USD;

			ZDecimal approvalOSAmount = 600m;
			ZDecimal approvalExchangeRate = 0.75m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = TestObjectCreator.USD;

			TestSaveReloadAndPostPaymentApprovalItems_Core(
						invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
						invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
						approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency);
		}

		public void TestSaveReloadAndPostPaymentApprovalItemsWithMixedForeignCurrencyTransactions()
		{
			ZDecimal invoice1OSAmount = 216m;
			ZDecimal invoice1ExchangeRate = 0.72m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = TestObjectCreator.USD;

			ZDecimal invoice2OSAmount = 225m;
			ZDecimal invoice2ExchangeRate = 0.45m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = TestObjectCreator.GBP;

			ZDecimal approvalOSAmount = 600m;
			ZDecimal approvalExchangeRate = 0.75m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = TestObjectCreator.USD;

			TestSaveReloadAndPostPaymentApprovalItems_Core(
						invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
						invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
						approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency);
		}

		public void TestSaveReloadAndPostPaymentApprovalItemsWithLocalCurrencyTransactions()
		{
			ZDecimal invoice1OSAmount = 300m;
			ZDecimal invoice1ExchangeRate = 1m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = TestObjectCreator.AUD;

			ZDecimal invoice2OSAmount = 500m;
			ZDecimal invoice2ExchangeRate = 1m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = TestObjectCreator.AUD;

			ZDecimal approvalOSAmount = 800m;
			ZDecimal approvalExchangeRate = 1m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = TestObjectCreator.AUD;

			TestSaveReloadAndPostPaymentApprovalItems_Core(
				invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
				invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
				approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public virtual void TestAmountsTogether()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			CurrentCompany.GC_IsReciprocal = false;
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
				AssertEquals(0.5m, TestPaymentApproval.AV_PayExRate);

				TestPaymentApproval.AV_PayExRate = 3;
				AssertEquals(0.67m, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals(2m, TestPaymentApproval.AV_Amount);
				AssertEquals(3m, TestPaymentApproval.AV_PayExRate);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public virtual void TestAmountsAndExchangeRates()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			ZString originalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CurrentCompany = newFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			CurrentCompany.GC_IsReciprocal = true;
			CurrentCompany.GC_RX_NKLocalCurrency = "JPY";
			newFactory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				ZDecimal oSAmount = 4839785M;
				ZDecimal localAmount = 344976M;
				ZDecimal exchangeRate = localAmount / oSAmount;

				TestPaymentApproval.BankAccount.AB_RX_NKAccountCurrency = "KRW";
				TestPaymentApproval.AV_RX_NKPaymentCurrency = "KRW";
				TestPaymentApproval.AV_Amount = oSAmount;
				TestPaymentApproval.AV_Calc_LocalAmount = localAmount;
				AssertEquals(Math.Round(exchangeRate, AccPaymentApprovalSchema.AV_PayExRate.Scale), TestPaymentApproval.AV_PayExRate);

				if (!TestPaymentApproval.PostsOnSave)
				{
					TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				}
				TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, localAmount);
				TestPaymentApproval.CreateNewPayment();

				AssertNotNull("Payment", TestPaymentApproval.TransactionHeader);
				AssertEquals("Payment OS Amount", oSAmount, TestPaymentApproval.TransactionHeader.AH_OSTotal);
				AssertEquals("Payment Local Amount", localAmount, TestPaymentApproval.TransactionHeader.AH_InvoiceAmount);

				AssertEquals("Payment Exchange Rate", 0.071279M, TestPaymentApproval.TransactionHeader.AH_ExchangeRate);
				AssertEquals("Payment Approval Exchange Rate", Math.Round(exchangeRate, AccPaymentApprovalSchema.AV_PayExRate.Scale), TestPaymentApproval.AV_PayExRate);
			}
			finally
			{
				CurrentCompany.GC_IsReciprocal = originalIsReciprocal;
				GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = originalCurrency;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		public void TestSetIsLoadedFromGUIDefaultValue()
		{
			AssertEquals("Precondition: IsLoadedFromGUI should be true ", TestPaymentApproval.IsLoadedFromGUI, true);
			TestPaymentApproval.IsLoadedFromGUI = false;
			AssertEquals("Precondition: IsLoadedFromGUI should be false ", TestPaymentApproval.IsLoadedFromGUI, false);
		}

		public void TestResetPaymentMatchingBaseObject()
		{
			AssertEquals(null, TestPaymentApproval.PaymentMatchingBaseObject_ForTestOnly);
			PaymentApprovalMatchingBase mathcingBase = TestPaymentApproval.PaymentMatchingBaseObject;
			AssertNotEquals(null, TestPaymentApproval.PaymentMatchingBaseObject_ForTestOnly);
			TestPaymentApproval.ResetPaymentMatchingBaseObject();
			AssertEquals(null, TestPaymentApproval.PaymentMatchingBaseObject_ForTestOnly);
		}

		public void TestCreateNewPaymentDoesNotReLoadTransactionFromDB()
		{
			ZDecimal invoice1OSAmount = 300m;
			ZDecimal invoice1ExchangeRate = 1m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = TestObjectCreator.AUD;

			ZDecimal invoice2OSAmount = 500m;
			ZDecimal invoice2ExchangeRate = 1m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = TestObjectCreator.AUD;

			ZDecimal invoice3OSAmount = 700m;
			ZDecimal invoice3ExchangeRate = 1m;
			RefCurrency invoice3Currency = TestObjectCreator.AUD;

			ZDecimal approvalOSAmount = 800m;
			ZDecimal approvalExchangeRate = 1m;
			RefCurrency approvalCurrency = TestObjectCreator.AUD;

			BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreatorForInvoiceCreationFactory = new TestObjectCreator(invoiceCreationFactory);

			APInvoice invoice1 = invoiceCreationFactory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001100";
			invoice1.AH_RX_NKTransactionCurrency = invoice1Currency.RX_Code;
			invoice1.AH_ExchangeRate = invoice1ExchangeRate;
			InvoicingLineBase line = testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice1, invoice1Currency, invoice1ExchangeRate, invoice1OSAmount);
			line.AL_AT = testObjectCreatorForInvoiceCreationFactory.GSTFREE1.PK;
			invoiceCreationFactory.Save();

			invoice1 = Factory.Load<APInvoice>(invoice1.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice1OSAmount, invoice1.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice1ExchangeRate, invoice1.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice1LocalAmount, invoice1.AH_LocalExTaxAmount);

			APInvoice invoice2 = invoiceCreationFactory.New<APInvoice>();
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_TransactionNum = "00001101";
			invoice2.AH_RX_NKTransactionCurrency = invoice2Currency.RX_Code;
			invoice2.AH_ExchangeRate = invoice2ExchangeRate;
			invoice2.AH_OSExTaxAmount = invoice2OSAmount;
			line = testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice2, invoice2Currency, invoice2ExchangeRate, invoice2OSAmount);
			line.AL_AT = testObjectCreatorForInvoiceCreationFactory.GSTFREE1.PK;
			invoiceCreationFactory.Save();

			invoice2 = Factory.Load<APInvoice>(invoice2.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice2OSAmount, invoice2.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice2ExchangeRate, invoice2.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice2LocalAmount, invoice2.AH_LocalExTaxAmount);
			invoiceCreationFactory.Save();

			AccBankAccount bankAccount = approvalExchangeRate == 1m ? TestObjectCreator.AUDBankAccount : TestObjectCreator.USDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = approvalCurrency.RX_Code;
			AccChequeBook chequeBook = approvalExchangeRate == 1m ? TestObjectCreator.AUDChequeBook : TestObjectCreator.USDChequeBook;

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			TestPaymentApproval.AV_AB = bankAccount.PK;
			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = approvalCurrency.RX_Code;
			TestPaymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = approvalOSAmount;
			TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, approvalOSAmount);
			Factory.Save();

			APInvoice invoice3 = invoiceCreationFactory.New<APInvoice>();
			invoice3.AH_OH = TestOrgHeader.PK;
			invoice3.AH_TransactionNum = "00001102";
			invoice3.AH_RX_NKTransactionCurrency = invoice3Currency.RX_Code;
			invoice3.AH_ExchangeRate = invoice3ExchangeRate;
			invoice3.AH_OSExTaxAmount = invoice3OSAmount;
			line = testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice3, invoice3Currency, invoice3ExchangeRate, invoice3OSAmount);
			line.AL_AT = testObjectCreatorForInvoiceCreationFactory.GSTFREE1.PK;
			invoiceCreationFactory.Save();

			if (!TestPaymentApproval.PostsOnSave)
			{
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				int beforeHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				TestPaymentApproval.CreateNewPayment();
				int afterHit = Factory.GetTableHitCount(AccTransactionHeaderSchema.Constants.TableName);
				AssertEquals("AccTransactionHeader table should not be hit during CreateNewPayment()", 1, afterHit - beforeHit);
			}
			Factory.Save();

			AssertEquals("New Payment's fIsLoadedFromGUI should be false", false, TestPaymentApproval.NewPayment.IsLoadedFromGUI);
			ZQuery loadAPInvQuery = new ZQuery();
			loadAPInvQuery.FetchOnlyFromLocalCache = true;
			loadAPInvQuery.AddToFilter(AccTransactionHeaderSchema.PK, invoice3.PK);
			var x = Factory.Load<APInvoice>(loadAPInvQuery);
			AssertEquals("inv3 should not be loaded from cache", 0, x.Length);
		}

		protected void TestSaveReloadAndPostPaymentApprovalItems_Core(
			ZDecimal invoice1OSAmount, ZDecimal invoice1ExchangeRate, ZDecimal invoice1LocalAmount, RefCurrency invoice1Currency,
			ZDecimal invoice2OSAmount, ZDecimal invoice2ExchangeRate, ZDecimal invoice2LocalAmount, RefCurrency invoice2Currency,
			ZDecimal approvalOSAmount, ZDecimal approvalExchangeRate, ZDecimal approvalLocalAmount, RefCurrency approvalCurrency)
		{
			BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
			TestObjectCreator testObjectCreatorForInvoiceCreationFactory = new TestObjectCreator(invoiceCreationFactory);

			APInvoice invoice1 = invoiceCreationFactory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader2.PK;
			invoice1.AH_TransactionNum = "00001100";
			invoice1.AH_RX_NKTransactionCurrency = invoice1Currency.RX_Code;
			invoice1.AH_ExchangeRate = invoice1ExchangeRate;
			InvoicingLineBase line = testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice1, invoice1Currency, invoice1ExchangeRate, invoice1OSAmount);
			line.AL_AT = testObjectCreatorForInvoiceCreationFactory.GSTFREE1.PK;
			invoiceCreationFactory.Save();

			invoice1 = Factory.Load<APInvoice>(invoice1.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice1OSAmount, invoice1.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice1ExchangeRate, invoice1.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice1LocalAmount, invoice1.AH_LocalExTaxAmount);

			APInvoice invoice2 = invoiceCreationFactory.New<APInvoice>();
			invoice2.AH_OH = TestOrgHeader2.PK;
			invoice2.AH_TransactionNum = "00001101";
			invoice2.AH_RX_NKTransactionCurrency = invoice2Currency.RX_Code;
			invoice2.AH_ExchangeRate = invoice2ExchangeRate;
			invoice2.AH_OSExTaxAmount = invoice2OSAmount;
			line = testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice2, invoice2Currency, invoice2ExchangeRate, invoice2OSAmount);
			line.AL_AT = testObjectCreatorForInvoiceCreationFactory.GSTFREE1.PK;
			invoiceCreationFactory.Save();

			invoice2 = Factory.Load<APInvoice>(invoice2.PK);
			AssertEquals("Precondition: OS Invoice Amount", invoice2OSAmount, invoice2.AH_OSExTaxAmount);
			AssertEquals("Precondition: Invoice Exchange Rate", invoice2ExchangeRate, invoice2.AH_ExchangeRate);
			AssertEquals("Precondition: Local Invoice Amount", invoice2LocalAmount, invoice2.AH_LocalExTaxAmount);
			invoiceCreationFactory.Save();

			AccBankAccount bankAccount = approvalExchangeRate == 1m ? TestObjectCreator.AUDBankAccount : TestObjectCreator.USDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = approvalCurrency.RX_Code;
			AccChequeBook chequeBook = approvalExchangeRate == 1m ? TestObjectCreator.AUDChequeBook : TestObjectCreator.USDChequeBook;

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
			TestPaymentApproval.AV_AB = bankAccount.PK;
			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = approvalCurrency.RX_Code;
			TestPaymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = approvalOSAmount;
			TestPaymentApproval.PaymentMatchingBaseObject.MatchDate = ZDateTime.Now;
			AssertEquals("Precondition: Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
			AssertEquals("Precondition: Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Precondition: Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

			AssertEquals("Unmatched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
			Assert("Unmatched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

			TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions shoulrd contain Payment Approval", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			Assert("transaction must be matched", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.MustTransactionBeMatched(TestPaymentApproval));

			Assert("Matched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1));
			AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
			AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

			Assert("Matched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2));
			AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2).OSPartialPaymentAmount);
			AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2).LocalPartialPaymentAmount);

			Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			AssertEquals("Payment Approval OsPartialPaymentAmount", approvalOSAmount, ((IMatching)TestPaymentApproval).OSPartialPaymentAmount);
			AssertEquals("Payment Approval LocalPartialPaymentAmount", approvalLocalAmount, ((IMatching)TestPaymentApproval).LocalPartialPaymentAmount);

			AssertEquals("Payment Matching Session Balances To Zero", true, TestPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);

			TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("Approval Items Created", 2, approvalItems.Count);

			ZQuery invoice1PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, TestPaymentApproval.PK);
			invoice1PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice1.PK);
			PaymentApprovalItem invoice1ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice1PaymentItemQuery);
			AssertNotNull("Invoice1ApprovalItem", invoice1ApprovalItem);
			AssertEquals("Invoice1ApprovalItem PaymentApprovalBase", invoice1ApprovalItem.PaymentApproval.PK, TestPaymentApproval.PK);
			AssertEquals("Invoice1ApprovalItem TransactionHeader", invoice1ApprovalItem.TransactionHeader.PK, invoice1.PK);
			AssertEquals("Invoice1ApprovalItem PaymentThisRun", -invoice1LocalAmount, invoice1ApprovalItem.A2_PaymentThisRun);

			ZQuery invoice2PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, TestPaymentApproval.PK);
			invoice2PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice2.PK);
			PaymentApprovalItem invoice2ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice2PaymentItemQuery);
			AssertNotNull("Invoice2ApprovalItem", invoice2ApprovalItem);
			AssertEquals("Invoice2ApprovalItem PaymentApprovalBase", invoice2ApprovalItem.PaymentApproval.PK, TestPaymentApproval.PK);
			AssertEquals("Invoice2ApprovalItem TransactionHeader", invoice2ApprovalItem.TransactionHeader.PK, invoice2.PK);
			AssertEquals("Invoice2ApprovalItem PaymentThisRun", -invoice2LocalAmount, invoice2ApprovalItem.A2_PaymentThisRun);

			PaymentApprovalBase reloadedApproval = newFactory.Load<PaymentApprovalBase>(TestPaymentApproval.PK);
			AssertEquals("Approval Currency", approvalCurrency.RX_Code, reloadedApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Approval OS Amount", approvalOSAmount, reloadedApproval.AV_Amount);
			AssertEquals("Approval Local Amount", approvalLocalAmount, reloadedApproval.AV_Calc_LocalAmount);

			AssertEquals("Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
			AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

			AssertEquals("Reloaded Approval Matched Transactions Count", 3, reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions Contains TestPaymentApproval", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(reloadedApproval.PK));
			Assert("Matched Transactions Contains Invoice1", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1.PK));
			Assert("Matched Transactions Contains Invoice2", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2.PK));

			APInvoice invoice1Reloaded = newFactory.Load<APInvoice>(invoice1.PK);
			AssertNotNull(invoice1Reloaded);
			AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
			AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

			APInvoice invoice2Reloaded = newFactory.Load<APInvoice>(invoice2.PK);
			AssertNotNull(invoice2Reloaded);
			AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2Reloaded).OSPartialPaymentAmount);
			AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2Reloaded).LocalPartialPaymentAmount);

			if (!TestPaymentApproval.PostsOnSave)
			{
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				TestPaymentApproval.CreateNewPayment();
			}

			Factory.Save();
			newFactory.Save();

			AssertEquals("Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
			AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

			Assert("PaymentCreationErrorMessages should be empty. PaymentCreationErrorMessages: " +
				TestPaymentApproval.PaymentCreationErrorMessages.ToMessageListString(), !TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());

			Assert("New Payment should be created", !TestPaymentApproval.AV_AH.IsEmpty);

			AssertNotNull("New Payment", TestPaymentApproval.TransactionHeader);
			TransactionHeader newPayment = Factory.Load<TransactionHeader>(TestPaymentApproval.AV_AH);

			AssertEquals("New Payment AH_OSExTaxAmount", approvalOSAmount, newPayment.AH_OSExTaxAmount);
			AssertEquals("New Payment AH_LocalExTaxAmount", approvalLocalAmount, newPayment.AH_LocalExTaxAmount);

			ZQuery newPaymentMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newPayment.PK);
			TransactionMatchLinkCollection newPaymentMatchLinks = new TransactionMatchLinkCollection(newFactory, newPaymentMatchLinkQuery);
			newPaymentMatchLinks.Load();
			AssertEquals("NewPaymentMatchLinks Count", 1, newPaymentMatchLinks.Count);
			AssertEquals("NewPayment MatchLink Amount", approvalLocalAmount, newPaymentMatchLinks[0].AP_Amount);
			Assert("NewPaymentMatchLink Match Group Number should not be empty", !newPaymentMatchLinks[0].AP_MatchGroupNum.IsEmpty);
			ZString matchGroupNum = newPaymentMatchLinks[0].AP_MatchGroupNum;

			ZQuery matchLinksQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(newFactory, matchLinksQuery);
			matchLinks.Load();
			AssertEquals("MatchLinks Count", TestPaymentApproval.IsPayables ? 3 : 5, matchLinks.Count);

			ZQuery invoice1MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK);
			invoice1MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
			TransactionMatchLink invoice1Link = Factory.LoadTop1<TransactionMatchLink>(invoice1MatchLinkQuery);
			AssertNotNull("Invoice1Link", invoice1Link);
			AssertEquals("Invoice 1 MatchLink Amount", -invoice1LocalAmount, invoice1Link.AP_Amount);

			ZQuery invoice2MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK);
			invoice2MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
			TransactionMatchLink invoice2Link = Factory.LoadTop1<TransactionMatchLink>(invoice2MatchLinkQuery);
			AssertNotNull("Invoice2Link", invoice2Link);
			AssertEquals("Invoice 2 MatchLink Amount", -invoice2LocalAmount, invoice2Link.AP_Amount);

			AssertEquals("Payment Date", TestPaymentApproval.AV_PaymentDate, newPayment.AH_InvoiceDate);
			AssertEquals("Post Date", TestPaymentApproval.AV_PostDate, newPayment.AH_PostDate);
			AssertEquals("Payment Branch", TestPaymentApproval.AV_GB, newPayment.AH_GB);
			AssertEquals("Payment Department", GlbDepartment.CurrentDepartment.PK, newPayment.AH_GE);
			AssertEquals("Payment Creditor", TestPaymentApproval.AV_OH, newPayment.AH_OH);
			AssertEquals("Payment Description", TestPaymentApproval.AV_PaymentComment, newPayment.AH_Desc);
			AssertEquals("Payment ReceiptType", TestPaymentApproval.AV_PaymentType, newPayment.AH_ReceiptType);
			AssertEquals("Payment BankAccount", TestPaymentApproval.AV_AB, newPayment.AH_AB);
			AssertEquals("Payment Currency", TestPaymentApproval.AV_RX_NKPaymentCurrency, newPayment.AH_RX_NKTransactionCurrency);
			AssertEquals("Payment ChequeOrReference", TestPaymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
			AssertEquals("Payment ExchangeRate", TestPaymentApproval.AV_PayExRate, newPayment.AH_ExchangeRate);
			AssertEquals("Payment OSExTaxAmount", TestPaymentApproval.AV_Amount, newPayment.AH_OSExTaxAmount);
			AssertEquals("Payment LocalExTaxAmount", TestPaymentApproval.AV_Calc_LocalAmount, newPayment.AH_LocalExTaxAmount);
		}

		public void TestCriticalValidationExceptionWithMiscTranasaction_BalanceNotZero()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("1001", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, TestOrgHeader2);
			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("1002", TestObjectCreator.AUD, 1M, 200M, 0M, 0M, 200M, 0M, 0M, TestOrgHeader2);

			Factory.Save();

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 305M;

			TestPaymentApproval.AV_ExchangeDifference = -5M;

			AssertEquals("Unmatched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
			Assert("Unmatched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

			TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions should contain Payment Approval", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			Assert("transaction must be matched", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.MustTransactionBeMatched(TestPaymentApproval));

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestPaymentApproval.IsAllowedToPost = true;

			try
			{
				AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
				Fail("Critical validation exception is thrown");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Non zero outstanding amount on miscellaneous transaction", ex.Message);

				var expectedError = $@"Balance is not zero, Balance is 300.0

PaymentApprovalBaseMatchDetails: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
MatchingBase.Match()
Current company's currency Code: AUD, Decimals: 2
Ledger: {TestPaymentApproval.Ledger}, Transaction type: PAY, Payment Amount: 305, Outstanding Amount: 305, OS Payment Amount: 305, OS Outstanding Amount: 305, Currency: AUD, Exchange Rate Amount: 1
Ledger: {TestPaymentApproval.Ledger}, Transaction type: EXX, Payment Amount: -5, Outstanding Amount: -5, OS Payment Amount: -5, OS Outstanding Amount: -5, Currency: AUD, Exchange Rate Amount: 1

There are matching errors:
Balance of transaction match result is not zero.

MatchedTransactions contains transaction from primary organization: true

Match group info: There is no data collected.";
				AssertContains(expectedError, ex.DeveloperErrorMessage);

				ErrorReporter.Clear();
			}
		}

		public void TestCriticalValidationExceptionWithMiscTranasaction_TransactionAlreadyPaid()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.PaymentApprovalBaseMatchDetails);

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "1001", TestObjectCreator.AUD, 1M, -100M, 0M, -100M, 0M, TestOrgHeader2, TestObjectCreator.GLHeader1.PK);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "1002", TestObjectCreator.AUD, 1M, -200M, 0M, -200M, 0M, TestOrgHeader2, TestObjectCreator.GLHeader1.PK);

			Factory.Save();

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestOrgHeader2.PK;
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 305M;

			TestPaymentApproval.AV_ExchangeDifference = -5M;

			AssertEquals("Unmatched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
			Assert("Unmatched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

			TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions shoulrd contain Payment Approval", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			Assert("transaction must be matched", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.MustTransactionBeMatched(TestPaymentApproval));

			AssertEquals("Payment Matching Session Balances To Zero", true, TestPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			TestPaymentApproval.IsAllowedToPost = true;
			TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui

			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice1, ZDateTime.Today); //this is to simulate invoice already paid

			try
			{
				AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Factory.Save();
				Fail("Critical validation exception is thrown");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Non zero outstanding amount on miscellaneous transaction", ex.Message);
				AssertTransactionAlreadyPaidHasExpectedError(TestPaymentApproval, ex.DeveloperErrorMessage);
				ErrorReporter.Clear();
			}
		}

		protected virtual void AssertTransactionAlreadyPaidHasExpectedError(PaymentApprovalBase approval, string exceptionMessage)
		{
			throw new NotImplementedException();
		}

		public void TestCriticalValidationExceptionWithMiscTranasaction_DynamicTransactionsHaveIncorrect()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.NonZeroOutstandingAmountOnMiscTransaction);

			Factory.Save();
			((IDbConnected)Factory).Connection.ExecuteNonQuery(@"
								Delete From dbo.AccPaymentApproval
								Delete From dbo.AccTransactionLines
								Delete From dbo.AccTransactionHeader");

			var newFactory = new BusinessObjectFactory();
			var newTestObjectCreator = new TestObjectCreator(newFactory);

			var currentBranch = newFactory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { newTestObjectCreator.NonCurrentDepartment });

			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newTestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.HeaderBranch.Code);

			var invoice1 = newTestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "1001", newTestObjectCreator.AUD, 1M, -100M, 0M, -100M, 0M, TestOrgHeader2, newTestObjectCreator.GLHeader1.PK);
			var invoice2 = newTestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "1002", newTestObjectCreator.AUD, 1M, -200M, 0M, -200M, 0M, TestOrgHeader, newTestObjectCreator.GLHeader1.PK);

			newFactory.Save();

			var testPaymentApproval = (PaymentApprovalBase)newFactory.New(GetExpectedBusinessObjectType());
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = TestOrgHeader2.PK;

			OrgLedgerFilter orgFilter = testPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			orgFilter.Organization = TestOrgHeader.PK;
			orgFilter.APLedger = true;
			orgFilter.ARLedger = true;

			OrgLedgerFilter org2Filter = testPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			org2Filter.Organization = TestOrgHeader2.PK;
			org2Filter.APLedger = true;
			org2Filter.ARLedger = true;

			testPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			testPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			testPaymentApproval.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			testPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			testPaymentApproval.AV_Amount = 305M;

			testPaymentApproval.AV_ExchangeDifference = -5M;

			testPaymentApproval.MatchingBaseObject.ReloadSettlementOrgTransactions();

			AssertEquals("Unmatched Transactions Count", 2, testPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
			Assert("Unmatched Transactions should contain Invoice1", testPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
			Assert("Unmatched Transactions should contain Invoice2", testPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

			testPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			AssertEquals("Matched Transactions Count", 3, testPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions shoulrd contain Payment Approval", testPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(testPaymentApproval));
			Assert("transaction must be matched", testPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.MustTransactionBeMatched(testPaymentApproval));

			AssertEquals("Payment Matching Session Balances To Zero", true, testPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);

			testPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
			testPaymentApproval.IsAllowedToPost = true;
			testPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui

			try
			{
				using (new DisposableAction(() => newFactory.SuspendValidation(), () => newFactory.ResumeValidation()))
				{
					AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					newFactory.Save();
				}

				Fail("Critical validation exception is thrown");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Non zero outstanding amount on miscellaneous transaction", ex.Message);
				var expectedError = @"There are matching errors:
Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.
Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.
Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.
Cannot create the Clearing Journal - The department BRN cannot be used with the branch BNE.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.";
				AssertContains(expectedError, ex.DeveloperErrorMessage);

				ErrorReporter.Clear();
			}
		}

		public void TestSaveReloadAndPostPaymentApprovalItemsWithHotCheque()
		{
			if (TestPaymentApproval.IsPayables)
			{
				ZDecimal invoice1OSAmount = 216m;
				ZDecimal invoice1ExchangeRate = 0.72m;
				ZDecimal invoice1LocalAmount = 300m;
				RefCurrency invoice1Currency = TestObjectCreator.USD;

				ZDecimal invoice2OSAmount = 375m;
				ZDecimal invoice2ExchangeRate = 0.75m;
				ZDecimal invoice2LocalAmount = 500m;
				RefCurrency invoice2Currency = TestObjectCreator.USD;

				ZDecimal approvalOSAmount = 600m;
				ZDecimal approvalExchangeRate = 0.75m;
				ZDecimal approvalLocalAmount = 800m;
				RefCurrency approvalCurrency = TestObjectCreator.USD;

				BusinessObjectFactory invoiceCreationFactory = new BusinessObjectFactory();
				TestObjectCreator testObjectCreatorForInvoiceCreationFactory = new TestObjectCreator(invoiceCreationFactory);

				AccHotCheque hotCheque = invoiceCreationFactory.New<AccHotCheque>();
				hotCheque.AQ_OH = TestOrgHeader.PK;
				hotCheque.AQ_ActualOrMaxIndicator = ActualOrMaxIndicator.Actual;
				hotCheque.AQ_AK = new TestObjectCreator(invoiceCreationFactory).USDChequeBook.PK;
				hotCheque.AQ_Amount = 600m;
				hotCheque.AQ_ChequeDate = ZDateTime.Today.AddDays(-5);
				hotCheque.AQ_ChequeNumber = "000050";

				APInvoice invoice1 = invoiceCreationFactory.New<APInvoice>();
				invoice1.AH_OH = TestOrgHeader.PK;
				invoice1.AH_TransactionNum = "00001100";
				invoice1.AH_RX_NKTransactionCurrency = invoice1Currency.RX_Code;
				invoice1.AH_ExchangeRate = invoice1ExchangeRate;
				testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice1, invoice1Currency, invoice1ExchangeRate, invoice1OSAmount, 0m, 0m);
				invoiceCreationFactory.Save();

				invoice1 = Factory.Load<APInvoice>(invoice1.PK);
				AssertEquals("Precondition: OS Invoice Amount", invoice1OSAmount, invoice1.AH_OSExTaxAmount);
				AssertEquals("Precondition: Invoice Exchange Rate", invoice1ExchangeRate, invoice1.AH_ExchangeRate);
				AssertEquals("Precondition: Local Invoice Amount", invoice1LocalAmount, invoice1.AH_LocalExTaxAmount);

				APInvoice invoice2 = invoiceCreationFactory.New<APInvoice>();
				invoice2.AH_OH = TestOrgHeader.PK;
				invoice2.AH_TransactionNum = "00001101";
				invoice2.AH_RX_NKTransactionCurrency = invoice2Currency.RX_Code;
				invoice2.AH_ExchangeRate = invoice2ExchangeRate;
				testObjectCreatorForInvoiceCreationFactory.CreateInvoiceLine(invoice2, invoice2Currency, invoice2ExchangeRate, invoice2OSAmount, 0m, 0m);
				invoiceCreationFactory.Save();

				invoice2 = Factory.Load<APInvoice>(invoice2.PK);
				AssertEquals("Precondition: OS Invoice Amount", invoice2OSAmount, invoice2.AH_OSExTaxAmount);
				AssertEquals("Precondition: Invoice Exchange Rate", invoice2ExchangeRate, invoice2.AH_ExchangeRate);
				AssertEquals("Precondition: Local Invoice Amount", invoice2LocalAmount, invoice2.AH_LocalExTaxAmount);
				invoiceCreationFactory.Save();

				hotCheque = Factory.Load<AccHotCheque>(hotCheque.PK);

				TestPaymentApproval.AV_OH = hotCheque.AQ_OH;
				TestPaymentApproval.ImportSelectedHotCheque(hotCheque);

				AssertEquals("Payment Approval Branch", GlbBranch.CurrentBranch.PK, TestPaymentApproval.Branch.PK);
				AssertEquals("Payment Approval Creditor", hotCheque.AQ_OH, TestPaymentApproval.AV_OH);
				AssertEquals("Payment Approval Is Cheque", true, TestPaymentApproval.IsCheque);
				AssertEquals("Payment Approval BankAccount", hotCheque.ChequeBook.BankAccount.PK, TestPaymentApproval.AV_AB);
				AssertEquals("Payment Approval Currency", hotCheque.AQ_Calc_RX_NK, TestPaymentApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Payment Approval ChequeOrReference", hotCheque.AQ_ChequeNumber, TestPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
				AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);
				AssertEquals("Hot Cheque has been imported", true, TestPaymentApproval.HotChequeIsAlreadyImported);

				AssertEquals("Unmatched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Count);
				Assert("Unmatched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice1));
				Assert("Unmatched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.UnmatchedTransactions.Contains(invoice2));

				TestPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
				AssertEquals("Matched Transactions Count", 3, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
				Assert("Matched Transactions should contain Payment Approval", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
				Assert("transaction must be matched", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.MustTransactionBeMatched(TestPaymentApproval));

				Assert("Matched Transactions should contain Invoice1", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1));
				AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
				AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

				Assert("Matched Transactions should contain Invoice2", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2));
				AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2).OSPartialPaymentAmount);
				AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2).LocalPartialPaymentAmount);

				Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
				AssertEquals("Payment Approval OsPartialPaymentAmount", approvalOSAmount, ((IMatching)TestPaymentApproval).OSPartialPaymentAmount);
				AssertEquals("Payment Approval LocalPartialPaymentAmount", approvalLocalAmount, ((IMatching)TestPaymentApproval).LocalPartialPaymentAmount);

				AssertEquals("Payment Matching Session Balances To Zero", true, TestPaymentApproval.PaymentMatchingBaseObject.SessionBalancesToZero);

				TestPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
				Factory.Save();

				BusinessObjectFactory newFactory = new BusinessObjectFactory();

				PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
				approvalItems.Load();
				AssertEquals("Approval Items Created", 2, approvalItems.Count);

				ZQuery invoice1PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, TestPaymentApproval.PK);
				invoice1PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice1.PK);
				PaymentApprovalItem invoice1ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice1PaymentItemQuery);
				AssertNotNull("Invoice1ApprovalItem", invoice1ApprovalItem);
				AssertEquals("Invoice1ApprovalItem PaymentApprovalBase", invoice1ApprovalItem.PaymentApproval.PK, TestPaymentApproval.PK);
				AssertEquals("Invoice1ApprovalItem TransactionHeader", invoice1ApprovalItem.TransactionHeader.PK, invoice1.PK);
				AssertEquals("Invoice1ApprovalItem PaymentThisRun", -invoice1LocalAmount, invoice1ApprovalItem.A2_PaymentThisRun);

				ZQuery invoice2PaymentItemQuery = new ZQuery(AccPaymentApprovalItemSchema.A2_AV, TestPaymentApproval.PK);
				invoice2PaymentItemQuery.AddToFilter(AccPaymentApprovalItemSchema.A2_AH, invoice2.PK);
				PaymentApprovalItem invoice2ApprovalItem = Factory.LoadTop1<PaymentApprovalItem>(invoice2PaymentItemQuery);
				AssertNotNull("Invoice2ApprovalItem", invoice2ApprovalItem);
				AssertEquals("Invoice2ApprovalItem PaymentApprovalBase", invoice2ApprovalItem.PaymentApproval.PK, TestPaymentApproval.PK);
				AssertEquals("Invoice2ApprovalItem TransactionHeader", invoice2ApprovalItem.TransactionHeader.PK, invoice2.PK);
				AssertEquals("Invoice2ApprovalItem PaymentThisRun", -invoice2LocalAmount, invoice2ApprovalItem.A2_PaymentThisRun);

				PaymentApprovalBase reloadedApproval = newFactory.Load<PaymentApprovalBase>(TestPaymentApproval.PK);
				AssertEquals("Approval Currency", approvalCurrency.RX_Code, reloadedApproval.AV_RX_NKPaymentCurrency);
				AssertEquals("Approval OS Amount", approvalOSAmount, reloadedApproval.AV_Amount);
				AssertEquals("Approval Local Amount", approvalLocalAmount, reloadedApproval.AV_Calc_LocalAmount);

				AssertEquals("Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
				AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

				AssertEquals("Reloaded Approval Matched Transactions Count", 3, reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
				Assert("Matched Transactions Contains TestPaymentApproval", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(reloadedApproval.PK));
				Assert("Matched Transactions Contains Invoice1", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice1.PK));
				Assert("Matched Transactions Contains Invoice2", reloadedApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(invoice2.PK));

				APInvoice invoice1Reloaded = newFactory.Load<APInvoice>(invoice1.PK);
				AssertNotNull(invoice1Reloaded);
				AssertEquals("Invoice1 OsPartialPaymentAmount", -invoice1OSAmount, ((IMatching)invoice1).OSPartialPaymentAmount);
				AssertEquals("Invoice1 LocalPartialPaymentAmount", -invoice1LocalAmount, ((IMatching)invoice1).LocalPartialPaymentAmount);

				APInvoice invoice2Reloaded = newFactory.Load<APInvoice>(invoice2.PK);
				AssertNotNull(invoice2Reloaded);
				AssertEquals("Invoice2 OsPartialPaymentAmount", -invoice2OSAmount, ((IMatching)invoice2Reloaded).OSPartialPaymentAmount);
				AssertEquals("Invoice2 LocalPartialPaymentAmount", -invoice2LocalAmount, ((IMatching)invoice2Reloaded).LocalPartialPaymentAmount);

				if (!TestPaymentApproval.PostsOnSave)
				{
					TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
					TestPaymentApproval.CreateNewPayment();
				}

				Factory.Save();
				newFactory.Save();

				AssertEquals("Payment Approval OS Amount", approvalOSAmount, TestPaymentApproval.AV_Amount);
				AssertEquals("Payment Approval Exchange Rate", approvalExchangeRate, TestPaymentApproval.AV_PayExRate);
				AssertEquals("Payment Approval OS Amount", approvalLocalAmount, TestPaymentApproval.AV_Calc_LocalAmount);

				Assert("PaymentCreationErrorMessages should be empty. PaymentCreationErrorMessages: " +
					TestPaymentApproval.PaymentCreationErrorMessages, !TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());

				Assert("New Payment should be created", !TestPaymentApproval.AV_AH.IsEmpty);

				AssertNotNull("New Payment", TestPaymentApproval.TransactionHeader);
				TransactionHeader newPayment = Factory.Load<TransactionHeader>(TestPaymentApproval.AV_AH);

				AssertEquals("New Payment AH_OSExTaxAmount", approvalOSAmount, newPayment.AH_OSExTaxAmount);
				AssertEquals("New Payment AH_LocalExTaxAmount", approvalLocalAmount, newPayment.AH_LocalExTaxAmount);

				ZQuery newPaymentMatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, newPayment.PK);
				TransactionMatchLinkCollection newPaymentMatchLinks = new TransactionMatchLinkCollection(newFactory, newPaymentMatchLinkQuery);
				newPaymentMatchLinks.Load();
				AssertEquals("NewPaymentMatchLinks Count", 1, newPaymentMatchLinks.Count);
				AssertEquals("NewPayment MatchLink Amount", approvalLocalAmount, newPaymentMatchLinks[0].AP_Amount);
				Assert("NewPaymentMatchLink Match Group Number should not be empty", !newPaymentMatchLinks[0].AP_MatchGroupNum.IsEmpty);
				ZString matchGroupNum = newPaymentMatchLinks[0].AP_MatchGroupNum;

				ZQuery matchLinksQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(newFactory, matchLinksQuery);
				matchLinks.Load();
				AssertEquals("MatchLinks Count", TestPaymentApproval.IsPayables ? 3 : 5, matchLinks.Count);

				ZQuery invoice1MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice1.PK);
				invoice1MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLink invoice1Link = Factory.LoadTop1<TransactionMatchLink>(invoice1MatchLinkQuery);
				AssertNotNull("Invoice1Link", invoice1Link);
				AssertEquals("Invoice 1 MatchLink Amount", -invoice1LocalAmount, invoice1Link.AP_Amount);

				ZQuery invoice2MatchLinkQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, invoice2.PK);
				invoice2MatchLinkQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_MatchGroupNum, matchGroupNum);
				TransactionMatchLink invoice2Link = Factory.LoadTop1<TransactionMatchLink>(invoice2MatchLinkQuery);
				AssertNotNull("Invoice2Link", invoice2Link);
				AssertEquals("Invoice 2 MatchLink Amount", -invoice2LocalAmount, invoice2Link.AP_Amount);

				AssertEquals("Payment Branch", TestPaymentApproval.AV_GB, newPayment.AH_GB);
				AssertEquals("Payment Department", GlbDepartment.CurrentDepartment.PK, newPayment.AH_GE);
				AssertEquals("Payment Creditor", TestPaymentApproval.AV_OH, newPayment.AH_OH);
				AssertEquals("Payment Description", TestPaymentApproval.AV_PaymentComment, newPayment.AH_Desc);
				AssertEquals("Payment ReceiptType", TestPaymentApproval.AV_PaymentType, newPayment.AH_ReceiptType);
				AssertEquals("Payment BankAccount", TestPaymentApproval.AV_AB, newPayment.AH_AB);
				AssertEquals("Payment Currency", TestPaymentApproval.AV_RX_NKPaymentCurrency, newPayment.AH_RX_NKTransactionCurrency);
				AssertEquals("Payment ChequeOrReference", TestPaymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
				AssertEquals("Payment ExchangeRate", TestPaymentApproval.AV_PayExRate, newPayment.AH_ExchangeRate);
				AssertEquals("Payment OSExTaxAmount", TestPaymentApproval.AV_Amount, newPayment.AH_OSExTaxAmount);
				AssertEquals("Payment LocalExTaxAmount", TestPaymentApproval.AV_Calc_LocalAmount, newPayment.AH_LocalExTaxAmount);

				hotCheque = Factory.Load<AccHotCheque>(hotCheque.PK);
				AssertEquals("Hot Cheque should be linked to Payment", TestPaymentApproval.AV_AH, hotCheque.AQ_AH);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMatchedHotChequeMarkedAsPostedWhilePaymentPosting()
		{
			if (TestPaymentApproval.IsPayables)
			{
				SetUpRegistryForTest();

				GlbStaff newUser = Factory.New<GlbStaff>();
				newUser.GS_Code = "ZAC";

				SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newUser);
				TestPaymentApproval.AV_PaymentDate = ZDateTime.Now.AddDays(-3);

				AccHotCheque hotCheque = CreateHotCheque(TestOrgHeader, "000001", false);
				hotCheque.AQ_Amount = 100m;

				TestPaymentApproval.AV_OH = TestOrgHeader.PK; //!!!
				TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
				TestPaymentApproval.AV_AK = hotCheque.AQ_AK;
				TestPaymentApproval.AV_AB = hotCheque.ChequeBook.BankAccount.PK;
				TestPaymentApproval.AV_ChequeOrReference = hotCheque.AQ_ChequeNumber;
				TestPaymentApproval.AV_Amount = hotCheque.AQ_Amount;

				AssertEquals("Approval Status", PaymentApprovalStatus.FullyApproved, TestPaymentApproval.AV_Status);

				TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, TestPaymentApproval, 100m);
				if (!TestPaymentApproval.PostsOnSave)
				{
					TestPaymentApproval.CreateNewPayment();
				}

				Factory.Save();

				AssertEquals("Hot Cheque should be linked to Payment", TestPaymentApproval.AV_AH, hotCheque.AQ_AH);
				AssertEquals("Hot Cheque should be Posted", AccHotCheque.POSTED, hotCheque.AQ_Calc_ChequeStatus);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDelete()
		{
			TestPaymentApproval.AV_OH = TestOrgHeader.PK;

			APInvoice invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = TestOrgHeader.PK;
			invoice1.AH_TransactionNum = "00001010";

			APInvoice invoice2 = Factory.New<APInvoice>();
			invoice2.AH_OH = TestOrgHeader.PK;
			invoice2.AH_TransactionNum = "00001011";
			Factory.Save();

			PaymentApprovalItem approvalItem1 = invoice1.GetPaymentApprovalItem(TestPaymentApproval);
			AssertNotNull("ApprovalItem1", approvalItem1);
			AssertNotNull("ApprovalItem1.Approval", approvalItem1.Approval);
			AssertEquals("ApprovalItem1.Approval", TestPaymentApproval.PK, approvalItem1.Approval.PK);

			PaymentApprovalItem approvalItem2 = invoice2.GetPaymentApprovalItem(TestPaymentApproval);
			AssertNotNull("ApprovalItem2", approvalItem2);
			AssertNotNull("ApprovalItem2.Approval", approvalItem2.Approval);
			AssertEquals("ApprovalItem2.Approval", TestPaymentApproval.PK, approvalItem2.Approval.PK);

			PaymentApprovalItemCollection approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("ApprovalItems.Count", 2, approvalItems.Count);

			if (!TestPaymentApproval.PostsOnSave)
			{
				Factory.Save();
			}

			TestPaymentApproval.Delete();

			AssertEquals("TestPaymentApproval.IsDeleted", true, TestPaymentApproval.IsDeleted);
			AssertEquals("ApprovalItem1.IsDeleted", true, approvalItem1.IsDeleted);
			AssertEquals("ApprovalItem2.IsDeleted", true, approvalItem2.IsDeleted);

			approvalItems = new PaymentApprovalItemCollection(TestPaymentApproval);
			approvalItems.Load();
			AssertEquals("ApprovalItems.Count", 0, approvalItems.Count);
		}

		public void TestDeleteWithQuotes()
		{
			var paymentApprovalDraft = GetNewBusinessObject() as PaymentApprovalBase;
			paymentApprovalDraft.AV_Status = PaymentApprovalStatus.Draft;
			paymentApprovalDraft.AV_OH = TestOrgHeader.PK;
			paymentApprovalDraft.AV_Amount = 1200m;
			paymentApprovalDraft.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);

			var paymentApprovalAwaitingApproval = GetNewBusinessObject() as PaymentApprovalBase;
			paymentApprovalAwaitingApproval.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			paymentApprovalAwaitingApproval.AV_OH = TestOrgHeader.PK;
			paymentApprovalAwaitingApproval.AV_Amount = 1200m;
			paymentApprovalAwaitingApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);

			var paymentApprovalRejected = GetNewBusinessObject() as PaymentApprovalBase;
			paymentApprovalRejected.AV_Status = PaymentApprovalStatus.Rejected;
			paymentApprovalRejected.AV_OH = TestOrgHeader.PK;
			paymentApprovalRejected.AV_Amount = 1200m;
			paymentApprovalRejected.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);

			var paymentApprovalCancelled = GetNewBusinessObject() as PaymentApprovalBase;
			paymentApprovalCancelled.AV_Status = PaymentApprovalStatus.Cancelled;
			paymentApprovalCancelled.AV_OH = TestOrgHeader.PK;
			paymentApprovalCancelled.AV_Amount = 1200m;
			paymentApprovalCancelled.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);

			TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
			Factory.Save();

			TestPaymentApproval.Delete();
			paymentApprovalDraft.Delete();
			paymentApprovalAwaitingApproval.Delete();
			paymentApprovalRejected.Delete();
			paymentApprovalCancelled.Delete();
			Factory.Save();

			AssertEquals("Payment Approval with quotes in approved status is deleted.", true, TestPaymentApproval.IsDeleted);
			AssertEquals("Payment Approval with quotes in draft status is deleted", true, paymentApprovalDraft.IsDeleted);
			AssertEquals("Payment Approval with quotes in awaiting approval status is deleted", true, paymentApprovalAwaitingApproval.IsDeleted);
			AssertEquals("Payment Approval with quotes in rejected status is deleted", true, paymentApprovalRejected.IsDeleted);
			AssertEquals("Payment Approval with quotes in cancelled status is deleted", true, paymentApprovalCancelled.IsDeleted);
		}

		public void TestDeleteWithQuotesWithDeals()
		{
			var quote = TestPaymentApproval.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX).Quote;
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Cancelled, TestPaymentApproval, quote);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.SubmissionFailed, TestPaymentApproval, quote);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Declined, TestPaymentApproval, quote);
			TestObjectCreator.CreateValidEPaymentDealForStatus(DealStatusCodes.Failed, TestPaymentApproval, quote);
			Factory.Save();

			TestPaymentApproval.Delete();
			Factory.Save();

			AssertEquals("Payment Approval with inactvie deals is deleted.", true, TestPaymentApproval.IsDeleted);
		}

		public void TestDefaultValues()
		{
			SetUpRegistryForTest();
			ZString expectedStatus = TestPaymentApproval.PostsOnSave ? PaymentApprovalStatus.FullyApproved : PaymentApprovalStatus.AwaitingApproval;

			TestPaymentApproval.AV_PaymentDate = ZDateTime.Empty;
			TestPaymentApproval.AV_PostDate = ZDateTime.Empty;
			TestPaymentApproval.AV_Ledger = ZString.Empty;
			TestPaymentApproval.AV_GB = ZGuid.Empty;
			TestPaymentApproval.AV_GC = ZGuid.Empty;
			TestPaymentApproval.AV_PayExRate = 0M;
			TestPaymentApproval.AV_RX_NKPaymentCurrency = ZString.Empty;
			TestPaymentApproval.AV_Status = ZString.Empty;
			TestPaymentApproval.AV_PaymentComment = ZString.Empty;
			TestPaymentApproval.AV_PaymentType = ZString.Empty;

			AssertEquals("Precondition: PaymentDate", ZDateTime.Empty, TestPaymentApproval.AV_PaymentDate);
			AssertEquals("Precondition: PostDate", ZDateTime.Empty, TestPaymentApproval.AV_PostDate);
			AssertEquals("Precondition: Ledger", ZString.Empty, TestPaymentApproval.AV_Ledger);
			AssertEquals("Precondition: Branch", ZGuid.Empty, TestPaymentApproval.AV_GB);
			AssertEquals("Precondition: Company", ZGuid.Empty, TestPaymentApproval.AV_GC);
			AssertEquals("Precondition: Exchange Rate", 0M, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Precondition: Currency", ZString.Empty, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Precondition: Payment Comment (Description)", ZString.Empty, TestPaymentApproval.AV_PaymentComment);
			AssertEquals("Precondition: Payment Type", ZString.Empty, TestPaymentApproval.AV_PaymentType);

			TestPaymentApproval.SetDefaultValues_ForTestOnly();

			AssertZDatesWithin5Minutes("Default: PaymentDate", ZDateTime.Now, TestPaymentApproval.AV_PaymentDate);
			AssertZDatesWithin5Minutes("Default: PostDate", ZDateTime.Now, TestPaymentApproval.AV_PostDate);
			AssertEquals("Default: Ledger", ExpectedDefaultLedger, TestPaymentApproval.AV_Ledger);
			AssertEquals("Default: Branch", GlbBranch.CurrentBranch.GB_Code, TestPaymentApproval.Branch.GB_Code);
			AssertEquals("Default: Company", GlbCompany.CurrentCompany.GC_Code, TestPaymentApproval.Company.GC_Code);
			AssertEquals("Default: Exchange Rate", 1M, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Default: Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Default: Status", expectedStatus, TestPaymentApproval.AV_Status);
			AssertEquals("Default: Payment Comment (Description)", ExpectedDefaultLedger + " PAYMENT", TestPaymentApproval.AV_PaymentComment);
			AssertEquals("Default: Payment Type", ReceiptTypes.Cheque, TestPaymentApproval.AV_PaymentType);
		}

		public virtual void TestDefaultPostDateReadOnly()
		{
			bool receivablesAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool payablesAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				fTestPaymentApproval = GetNewBusinessObject() as PaymentApprovalBase;
				Assert("AV_PostDate should not be readonly", !TestPaymentApproval.AV_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				fTestPaymentApproval = GetNewBusinessObject() as PaymentApprovalBase;
				Assert("AV_PostDate should not be readonly", !TestPaymentApproval.AV_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				fTestPaymentApproval = GetNewBusinessObject() as PaymentApprovalBase;
				Assert("AV_PostDate should not be readonly", !TestPaymentApproval.AV_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				fTestPaymentApproval = GetNewBusinessObject() as PaymentApprovalBase;
				Assert("AV_PostDate should not be readonly", !TestPaymentApproval.AV_PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = receivablesAllowed;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablesAllowed;
			}
		}

		public void TestDefaultCurrency()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			Factory.Save();

			AssertEquals("The Payment currency should default to the Current Company's Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("Currency is readonly", IsCurrencyReadOnly, TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);

			TestPaymentApproval.AV_AB = testBankAccount.PK;
			AssertEquals("The Payment currency should default to the Bank Account's currency", testCurrency.RX_Code, TestPaymentApproval.AV_RX_NKPaymentCurrency);
			Assert("The currency should still be readonly", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);
		}

		protected abstract bool IsCurrencyReadOnly { get; }

		public void TestLocalCurrencySettings()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			AccBankAccount testLocalBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testLocalBankAccount.AB_RX_NKAccountCurrency = TestPaymentApproval.AV_Calc_LocalCurrency;

			AccBankAccount testBankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			testBankAccount.AB_RX_NKAccountCurrency = testCurrency.RX_Code;

			TestPaymentApproval.AV_AB = testBankAccount.PK;
			TestPaymentApproval.AV_PayExRate = 0.78M;
			AssertEquals("Precondition: Exchange Rate is not 1", 0.78M, TestPaymentApproval.AV_PayExRate);

			TestPaymentApproval.AV_AB = testLocalBankAccount.PK;
			Assert("Exchange rate should be readonly because currency is local", TestPaymentApproval.AV_PayExRateInfo.ReadOnly);
			AssertEquals("Exchange rate should be 1 because currency is local", 1M, TestPaymentApproval.AV_PayExRate);

			TestPaymentApproval.AV_AB = testBankAccount.PK;
			Assert("Exchange rate should not be readonly", !TestPaymentApproval.AV_PayExRateInfo.ReadOnly);
		}

		public void TestLocalPartialPaymentAmount()
		{
			TestPaymentApproval.AV_Amount = 377.13M;
			TestPaymentApproval.AV_PayExRate = 0.6873M;

			ZDecimal amountWithMultiplier = 377.13M;
			((IMatching)TestPaymentApproval).OSPartialPaymentAmount = amountWithMultiplier;

			amountWithMultiplier = 548.71M;
			AssertEquals("LocalPartialPayment amount should be 548.71",
				amountWithMultiplier, ((IMatching)TestPaymentApproval).LocalPartialPaymentAmount);
		}

		public void TestAV_PayExRateDecimals()
		{
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, 1000m);
			TestPaymentApproval.CreateNewPayment();
			TestPaymentApproval.AV_Amount = 7394.88M;
			TestPaymentApproval.AV_Calc_LocalAmount = 12382.39M;
			AssertEquals(0.597209424M, TestPaymentApproval.AV_PayExRate);

			var payment = Factory.Load<TransactionHeader>(TestPaymentApproval.AV_AH);
			payment.AH_OSExTaxAmount = 7394.88M;
			payment.AH_LocalExTaxAmount = 12382.39M;
			TestPaymentApproval.SetFieldsFromPayment(payment as Payment);
			AssertEquals(0.597209424M, TestPaymentApproval.AV_PayExRate);
		}

		public void TestCurrencySettingsWhenBankAccountIsUpdated()
		{
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			Assert("payment currency is editable when bank account is local", !TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);

			TestPaymentApproval.AV_AB = TestObjectCreator.USDBankAccount.PK;
			Assert("payment currency is read only when bank account is foreign", TestPaymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);
		}

		public virtual void TestCurrencyAndExchangeRateFields()
		{
			ZDecimal expectedRate = TestPaymentApproval.IsPayables ? USDBuyRate.RE_SellRate : USDSellRate.RE_SellRate;
			ZDecimal exchangeRate = Env.CurrentCompany.ExchangeRate.GetRate(TestObjectCreator.USD.RX_Code, TestPaymentApproval.GetRateType_ForTestOnly(), ZDateTime.Now.ToDateTime());
			AssertEquals("Precondition: ExchangeRate from standard code", expectedRate, exchangeRate);

			TestPaymentApproval.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Exchange Rate", 1m, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Exchange Rate: Currency Readonly", false, TestPaymentApproval.ExchangeRate.CurrencyInfo.ReadOnly);

			TestPaymentApproval.AV_PostDate = ZDateTime.Now;
			TestPaymentApproval.ExchangeRate.Currency = TestObjectCreator.USD.RX_Code;
			AssertEquals("Exchange Rate", expectedRate, TestPaymentApproval.AV_PayExRate);
			AssertEquals("Exchange Rate: Currency Readonly", false, TestPaymentApproval.ExchangeRate.CurrencyInfo.ReadOnly);

			// ReadOnly Fields
			AssertEquals("RXDecimals", TestPaymentApproval.PaymentCurrency.Decimals, TestPaymentApproval.RXDecimals);
			AssertEquals("LocalRXDecimals", GlbCompany.CurrentCompany.LocalCurrency.Decimals, TestPaymentApproval.LocalRXDecimals);

			AssertEquals("AV_Calc_LocalCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPaymentApproval.AV_Calc_LocalCurrency);
			AssertEquals("AV_Calc_LocalCurrencyInfo.ReadOnly", true, TestPaymentApproval.AV_Calc_LocalCurrencyInfo.ReadOnly);

			AssertEquals("OSCurrencyForDisplay", TestPaymentApproval.AV_RX_NKPaymentCurrency, TestPaymentApproval.OSCurrencyForDisplay);
			AssertEquals("OSCurrencyForDisplayInfo.ReadOnly", true, TestPaymentApproval.OSCurrencyForDisplayInfo.ReadOnly);
		}

		public void TestGetContactOrganisation()
		{
			TestPaymentApproval.AV_OH = ZGuid.Empty;
			AssertNull("No contact org yet", TestPaymentApproval.PaymentApprovalDocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader);
			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			AssertEquals("Contact Org for Document not null", TestOrgHeader.PK, TestPaymentApproval.PaymentApprovalDocumentSupporter.GetContactOrganisation("", ContactType.Payables, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", CargoWise.Definitions.BusinessContext.PaymentApproval, TestPaymentApproval.PaymentApprovalDocumentSupporter.BusinessContext);
		}

		public void TestPopulateChequeNumberFromChequeBook()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_ChequeNumDigits = (ZByte)5;
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 34;

			// Factory.Save();

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;  // cheque number should default to 34
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", TestPaymentApproval.AV_ChequeOrReference);

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;

			AssertEquals("Bank should remain", testBank.PK, TestPaymentApproval.AV_AB);
			AssertEquals("Cheque Book should be cleared", ZGuid.Empty, TestPaymentApproval.AV_AK);

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque Number should default to the current number in the DB for the selected cheque book",
				"00034", TestPaymentApproval.AV_ChequeOrReference);
		}

		public void TestCanIncrementChequeNumber()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			chequeBook.AK_CurrentNo = 93;

			Factory.Save();

			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "AF";

			Assert("CanIncrementChequeNumber should be false", !TestPaymentApproval.CanIncrementChequeBookNumber);

			TestPaymentApproval.AV_ChequeOrReference = "9";

			Assert("CanIncrementChequeNumber should be false", !TestPaymentApproval.CanIncrementChequeBookNumber);

			TestPaymentApproval.AV_ChequeOrReference = "94";

			Assert("CanIncrementChequeNumber should be true", TestPaymentApproval.CanIncrementChequeBookNumber);

			TestPaymentApproval.AV_ChequeOrReference = "94.5";

			Assert("CanIncrementChequeNumber should be false", !TestPaymentApproval.CanIncrementChequeBookNumber);
		}

		public void TestFactory_Saved()
		{
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			chequeBook.AK_CurrentNo = 93;

			Factory.Save();

			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "89";
			TestPaymentApproval.OnFactorySaved_ForTestOnly(true);
			AssertEquals("Current number should be 93", 93m, chequeBook.AK_CurrentNo);

			TestPaymentApproval.AV_ChequeOrReference = "95";
			TestPaymentApproval.OnFactorySaved_ForTestOnly(true);
			AssertEquals("Current number should be 96", 96m, chequeBook.AK_CurrentNo);
		}

		public void TestSavingChequeNumberGreaterThanCurrentNumber()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;
			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 100;
			testChequeBook.AK_CurrentNo = 66;

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			// Cheque num should default to 66
			TestPaymentApproval.AV_ChequeOrReference = "77";

			Factory.Save();

			testChequeBook.Reload();
			AssertEquals("Current number should become 1 plus the number entered by the user", 78m, testChequeBook.AK_CurrentNo);
		}

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

			TestPaymentApproval.AV_Status = PaymentApprovalStatus.Posted;
			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;  // Cheque number should default to 45
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque number should default to 00045", "00045", TestPaymentApproval.AV_ChequeOrReference);

			BusinessObjectFactory concurrentFactory = new BusinessObjectFactory();

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;  // Cheque number should default to 00045
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque number should default to 00045", "00045", TestPaymentApproval.AV_ChequeOrReference);

			Factory.Save();

			Assert("TestPaymentApproval should save to DB", TestPaymentApproval.IsInDatabase);

			testChequeBook.Reload();
			AssertEquals("The current number should increment on saving", 46m, testChequeBook.AK_CurrentNo);

			concurrentFactory.Save();

			testChequeBook.Reload();
			AssertEquals("The current number should not increment since TestARPay has a lower number than the current number in DB",
				46m, testChequeBook.AK_CurrentNo);
		}

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

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "34";

			Factory.Save();

			testChequeBook.Reload();
			AssertEquals("Current number should not be incremented since the cheque number was for a cancelled payment",
				67m, testChequeBook.AK_CurrentNo);
		}

		public void TestIsChequeNumberUsedByCancelledPayment()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;

			AccBankAccount anotherBank = Factory.NewWithValidTestData<AccBankAccount>();
			GlbCompany anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			anotherCompanyBranch.GB_GC = GlbCompany.CurrentCompany.PK;

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

			Factory.Save();

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "45";

			Assert("There is a cancelled Payment with the current bank and cheque number = 45",
						TestPaymentApproval.ChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(TestPaymentApproval.AV_ChequeOrReference));

			TestPaymentApproval.AV_ChequeOrReference = "46";

			Assert("There is no cancelled payment with the current bank and cheque number = 46",
						!TestPaymentApproval.ChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(TestPaymentApproval.AV_ChequeOrReference));

			TestPaymentApproval.AV_ChequeOrReference = "35";

			Assert("There is no cancelled payment from the current bank with cheque number = 35",
						!TestPaymentApproval.ChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(TestPaymentApproval.AV_ChequeOrReference));

			TestPaymentApproval.AV_ChequeOrReference = "23";

			Assert("There is a cancelled payment from the current company with cheque number = 23",
						TestPaymentApproval.ChequeBook.BankAccount.IsChequeNumberUsedByCancelledPayment(TestPaymentApproval.AV_ChequeOrReference));
		}

		public void TestSetReceiptType()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			//Factory.Save();

			AssertEquals("Precondition: Default receipt type is cheque", ReceiptTypes.Cheque, TestPaymentApproval.AV_PaymentType);

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			AssertEquals("Bank account should be left the same", testBank.PK, TestPaymentApproval.AV_AB);
			AssertEquals("Cheque/Reference number should change to CASH", ReceiptTypes.Cash, TestPaymentApproval.AV_ChequeOrReference);
			Assert("ChequeBook field should clear", TestPaymentApproval.AV_AK.IsEmpty);
			Assert("ChequeBook should become readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);

			// Cheque
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			AssertEquals("Bank account should be left the same", testBank.PK, TestPaymentApproval.AV_AB);
			Assert("Cheque/Reference number should become empty", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			// only for payment
			Assert("ChequeBook field should be editable", !TestPaymentApproval.AV_AKInfo.ReadOnly);

			// CreditCard
			TestPaymentApproval.AV_ChequeOrReference = "234";

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.CreditCard;
			Assert("ChequeOrReference should be cleared", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);

			// Direct Debit
			TestPaymentApproval.AV_ChequeOrReference = "345";

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("ChequeOrReference should be cleared", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);

			// Periodic Payment
			TestPaymentApproval.AV_ChequeOrReference = "345";

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.EFT;
			Assert("ChequeOrReference should be cleared", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.ScheduledEFT;
			Assert("ChequeOrReference should be cleared", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.CollectionRequest;
			Assert("ChequeOrReference should be cleared", TestPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("ChequeBook should be readonly", TestPaymentApproval.AV_AKInfo.ReadOnly);
		}

		public void TestChequeBooks()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			AccChequeBook currentBranchBook = Factory.NewWithValidTestData<AccChequeBook>();
			currentBranchBook.AK_AB = bankAccount.PK;
			currentBranchBook.AK_GB = GlbBranch.CurrentBranch.PK;

			AccChequeBook otherBranchBook = Factory.NewWithValidTestData<AccChequeBook>();
			otherBranchBook.AK_AB = bankAccount.PK;
			otherBranchBook.AK_GB = branch.PK;

			Factory.Save();

			TestPaymentApproval.AV_AB = bankAccount.PK;
			TestPaymentApproval.Lookups.ChequeBooks.Load();

			AssertEquals("There should be 1 chequebook in the list", 1, TestPaymentApproval.Lookups.ChequeBooks.Count);
			Assert("The chequebook should be currentBranchBook", TestPaymentApproval.Lookups.ChequeBooks.Contains(currentBranchBook));
		}

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

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque Number is 00045", "00045", TestPaymentApproval.AV_ChequeOrReference);

			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				TestPaymentApproval.GetChequeNumberStatus());

			Factory.Save();

			AssertEquals("ChequeNumStatus is LessThanCurrentNum", ChequeNumberStatus.LessThanCurrentNum, TestPaymentApproval.GetChequeNumberStatus());

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

			TestPaymentApproval.AV_ChequeOrReference = "11";
			AssertEquals("ChequeNumStatus is CancelledPayment", ChequeNumberStatus.CancelledPayment, TestPaymentApproval.GetChequeNumberStatus());

			TestPaymentApproval.AV_AK = ZGuid.Empty;
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, TestPaymentApproval.GetChequeNumberStatus());

			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "46";

			AssertEquals("ChequeNumStatus is GreaterThanOrEqualToCurrentNum", ChequeNumberStatus.GreaterThanOrEqualToCurrentNum,
				TestPaymentApproval.GetChequeNumberStatus());

			TestPaymentApproval.AV_ChequeOrReference = "11.5";
			AssertEquals("ChequeNumStatus is NoChequeNumber", ChequeNumberStatus.NoChequeNumber, TestPaymentApproval.GetChequeNumberStatus());
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

			var testPaymentApproval1 = (PaymentApprovalBase)newFactory.New(GetExpectedBusinessObjectType());
			testPaymentApproval1.AV_AB = testBank.PK;
			testPaymentApproval1.AV_AK = testChequeBook.PK;
			testPaymentApproval1.AV_OH = TestOrgHeader.PK;
			testPaymentApproval1.AV_PaymentComment = "AP PAYMENT DESCRIPTION1";
			testPaymentApproval1.AV_PaymentType = ReceiptTypes.Cheque;
			testPaymentApproval1.AV_Amount = 2000M;
			testPaymentApproval1.AV_ChequeOrReference = "00048";

			var testPaymentApproval2 = (PaymentApprovalBase)newFactory.New(GetExpectedBusinessObjectType());
			testPaymentApproval2.AV_AB = testBank.PK;
			testPaymentApproval2.AV_AK = testChequeBook.PK;
			testPaymentApproval2.AV_OH = TestOrgHeader.PK;
			testPaymentApproval2.AV_PaymentComment = "AP PAYMENT DESCRIPTION2";
			testPaymentApproval2.AV_PaymentType = ReceiptTypes.Cheque;
			testPaymentApproval2.AV_Amount = 1000M;
			testPaymentApproval2.AV_ChequeOrReference = "00045";

			newFactory.Save();

			AssertEquals("ChequeNumStatus is LessThanCurrentNum", ChequeNumberStatus.LessThanCurrentNum, testPaymentApproval2.GetChequeNumberStatus());

			var loadedChequeBook = new BusinessObjectFactory().Load<AccChequeBook>(testChequeBook.PK);
			AssertEquals(49, loadedChequeBook.AK_CurrentNo.ToZInt());
		}

		public void TestChangingBankAccountResetsChequeBook()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			Factory.Save();

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_AK = chequeBook.PK;
			TestPaymentApproval.AV_AB = bank.PK;
			Assert("Cheque Book should reset to empty when bank account is changed", TestPaymentApproval.AV_AK.IsEmpty);
		}

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

			TestPaymentApproval.AV_AB = bank1.PK;
			TestPaymentApproval.Lookups.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques1", TestPaymentApproval.Lookups.ChequeBooks.Contains(cheques1));
			Assert("ChequeBook collection should not contain Cheques2", !TestPaymentApproval.Lookups.ChequeBooks.Contains(cheques2));

			TestPaymentApproval.AV_AB = bank2.PK;
			TestPaymentApproval.Lookups.ChequeBooks.Load();
			Assert("ChequeBook collection should contain Cheques2", TestPaymentApproval.Lookups.ChequeBooks.Contains(cheques2));
			Assert("ChequeBook collection should not contain Cheques1", !TestPaymentApproval.Lookups.ChequeBooks.Contains(cheques1));
		}

		public void TestAV_ChequeOrReference()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;
			testBank.AB_ChequeNumDigits = (ZByte)5;
			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_ChequeOrReference = "77";

			AssertEquals("Should be 00077", "00077", TestPaymentApproval.AV_ChequeOrReference);
		}

		public void TestDoesChequeNumberAlreadyExist()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccBankAccount testBank2 = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_AB = testBank.PK;

			Payment pay1 = Factory.New<APPayment>();
			pay1.AH_AB = testBank.PK;
			pay1.AH_ChequeOrReference = "11";

			Payment pay2 = Factory.New<APPayment>();
			pay2.AH_AB = testBank2.PK;
			pay2.AH_ChequeOrReference = "12";

			ARReceipt receipt1 = Factory.NewWithValidTestData<ARReceipt>();
			receipt1.AH_AB = testBank.PK;
			receipt1.AH_ChequeOrReference = "12";

			Factory.Save();

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = "11";
			AssertEquals("HasChequeNumberBeenUsed", true, TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_AB = testBank2.PK;
			TestPaymentApproval.AV_ChequeOrReference = "11";
			AssertEquals("HasChequeNumberBeenUsed", false, TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_ChequeOrReference = "12";
			AssertEquals("HasChequeNumberBeenUsed", false, TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestAllowAutoDDR()
		{
			AccAPAccountDetails accountDetails = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			Factory.Save();

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;

			Assert(TestPaymentApproval.AllowAutoDDR);

			accountDetails.A1_BankBsb = "";
			Assert(!TestPaymentApproval.AllowAutoDDR);
		}

		public void TestAccountDetailsFound()
		{
			AccAPAccountDetails accountDetails = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;
			Assert("Should be true", TestPaymentApproval.AccountDetailsFound);

			accountDetails.A1_IsDefaultAccount = false;
			TestPaymentApproval.ResetAccountDetails();
			Assert("Should be false, AccountDetails is not Default", !TestPaymentApproval.AccountDetailsFound);

			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewZealand;
			TestPaymentApproval.ResetAccountDetails();
			Assert("Should be false, AccountDetails Currency of NZD is different from the Payment TransactionCurrency AUD", !TestPaymentApproval.AccountDetailsFound);

			accountDetails.A1_PaymentMethod = ReceiptTypes.Cheque;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.ResetAccountDetails();
			Assert("Should be false, AccountDetails PaymentMethod of CHQ is different from Payment ReceiptType of DDR", !TestPaymentApproval.AccountDetailsFound);
		}

		public void TestAccountDetailsUpdateWithCache()
		{
			var accountDetails = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;

			Factory.Save();

			Assert("Should be true", TestPaymentApproval.AccountDetailsFound);
			AssertEquals("should be current bsb", "bsb", TestPaymentApproval.PayeeBankBSB);

			var otherFactory = new BusinessObjectFactory();
			var accountDetailsInOtherFactory = otherFactory.Load<AccAPAccountDetails>(accountDetails.PK);
			accountDetailsInOtherFactory.A1_BankBsb = "newBSB";
			otherFactory.Save();

			AssertEquals("should be current bsb", "newBSB", TestPaymentApproval.PayeeBankBSB);
		}

		public void TestAccountDetailsCache_DifferentOrg()
		{
			var accountDetails1 = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails1.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails1.A1_AccountName = "Name1";
			accountDetails1.A1_BankAccount = "Bank1";
			accountDetails1.A1_BankBsb = "bsb1";
			accountDetails1.A1_IsDefaultAccount = true;
			accountDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			var accountDetails2 = TestOrgHeader2.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails2.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails2.A1_AccountName = "Name2";
			accountDetails2.A1_BankAccount = "Bank2";
			accountDetails2.A1_BankBsb = "bsb2";
			accountDetails2.A1_IsDefaultAccount = true;
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;

			Assert(TestPaymentApproval.AccountDetailsFound);
			AssertEquals("bsb1", TestPaymentApproval.PayeeBankBSB);

			var testPaymentApproval2 = (PaymentApprovalBase)Factory.New(GetExpectedBusinessObjectType());
			testPaymentApproval2.AV_OH = TestOrgHeader2.PK;
			testPaymentApproval2.AV_PaymentType = ReceiptTypes.DirectDebit;
			testPaymentApproval2.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.Australia;

			Assert(testPaymentApproval2.AccountDetailsFound);
			AssertEquals("bsb2", testPaymentApproval2.PayeeBankBSB);
		}

		public void TestGetAccountDetails_ShouldThrowException()
		{
			TestPaymentApproval.AV_PaymentType = "I|C"; // '|' character is currently used as key separator.
			AssertExceptionThrown<InvalidOperationException>(() => _ = TestPaymentApproval.AccountDetailsFound);
		}

		public void TestAccountDetailsFound_PaymentTypeEPayment()
		{
			AccAPAccountDetails accountDetails = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankAccount = "Bank";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.EPayment;
			Assert("Should be false, still needs E-Payment provider", !TestPaymentApproval.AccountDetailsFound);

			testBank.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;
			Assert("Should be true", TestPaymentApproval.AccountDetailsFound);
		}

		public void TestAccountDetailsProperties()
		{
			var bankCreateTime = new ZDateTime(2022, 11, 6, 16, 42, 0);
			var bankLastEditTime = new ZDateTime(2022, 11, 6, 17, 42, 0);
			var ePayLastEditTime = new ZDateTime(2022, 11, 6, 19, 42, 0);
			var accountDetails = TestOrgHeader.CompanyData.AccountDetailsCollection.AddNew();
			var ePayBeneficiary = Factory.NewWithValidTestData<AccEPaymentBeneficiary>();
			ePayBeneficiary.ABF_SystemLastEditTimeUtc = ePayLastEditTime;

			accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
			accountDetails.A1_EPaymentBeneficiaryId = ePayBeneficiary.PK;
			accountDetails.A1_AccountName = "Name";
			accountDetails.A1_BankName = "Bank Name";
			accountDetails.A1_BankAccount = "000111000";
			accountDetails.A1_BankBsb = "bsb";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails.A1_SystemCreateUser = "TST";
			accountDetails.A1_SystemCreateTimeUtc = bankCreateTime;
			accountDetails.A1_SystemLastEditUser = "EDT";
			accountDetails.A1_SystemLastEditTimeUtc = bankLastEditTime;

			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_PaymentProvider = EPaymentProviderCodes.Codes.OFX;

			TestPaymentApproval.AV_AB = testBank.PK;
			TestPaymentApproval.AV_OH = TestOrgHeader.PK;
			TestPaymentApproval.ExchangeRate.Currency = Core.Constants.CurrencyCodes.Australia;
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.EPayment;
			Assert("Precondition: AccountDetails is found", TestPaymentApproval.AccountDetailsFound);

			// Payee Details
			AssertEquals("Bank Name", TestPaymentApproval.PayeeBankName);
			AssertEquals("Name", TestPaymentApproval.AccountTitle);
			AssertEquals(Core.Constants.CountryCodes.Australia, TestPaymentApproval.PayeeBankAccountCountry);
			AssertEquals("bsb", TestPaymentApproval.PayeeBankBSB);
			AssertEquals("000111000", TestPaymentApproval.PayeeBankAccountNumber);
			// Audit Details
			AssertEquals("TST", TestPaymentApproval.BankCreateUser);
			AssertEquals(bankCreateTime.ToLocalBranchTime(), TestPaymentApproval.BankCreateTimeLocal);
			AssertEquals("EDT", TestPaymentApproval.BankLastEditUser);
			AssertEquals(bankLastEditTime.ToLocalBranchTime(), TestPaymentApproval.BankLastEditTimeLocal);
			//EPayment Beneficiary details
			AssertEquals(ePayLastEditTime.ToLocalBranchTime(), TestPaymentApproval.EPaymentBeneficiaryLastEditTimeLocal);

			//Reset so that AccountDetails is null and retest properties.
			accountDetails.A1_IsDefaultAccount = false;
			TestPaymentApproval.ResetAccountDetails();
			Assert("Precondition: AccountDetails is null", !TestPaymentApproval.AccountDetailsFound);

			// Payee Details
			AssertEquals(ZString.Empty, TestPaymentApproval.PayeeBankName);
			AssertEquals(ZString.Empty, TestPaymentApproval.AccountTitle);
			AssertEquals(ZString.Empty, TestPaymentApproval.PayeeBankAccountCountry);
			AssertEquals(ZString.Empty, TestPaymentApproval.PayeeBankBSB);
			AssertEquals(ZString.Empty, TestPaymentApproval.PayeeBankAccountNumber);
			// Audit Details
			AssertEquals(ZString.Empty, TestPaymentApproval.BankCreateUser);
			AssertEquals(ZDateTime.Empty, TestPaymentApproval.BankCreateTimeLocal);
			AssertEquals(ZString.Empty, TestPaymentApproval.BankLastEditUser);
			AssertEquals(ZDateTime.Empty, TestPaymentApproval.BankLastEditTimeLocal);
			//EPayment Beneficiary details
			AssertEquals(ZDateTime.Empty, TestPaymentApproval.EPaymentBeneficiaryLastEditTimeLocal);
		}

		public void TestEPaymentRecipientListLastUpdatedTimeLocal()
		{
			var latestRequestReceived = new ZDateTime(2021, 8, 16);
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request.ABR_LastResponseReceivedUtc = latestRequestReceived;
			Factory.Save();

			AssertNull("Precondition: PaymentBatch should be null", TestPaymentApproval.PaymentBatch);
			AssertEquals("Should be empty, because PaymentBatch is null", ZDateTime.Empty, TestPaymentApproval.EPaymentRecipientListLastUpdatedTimeLocal);

			var paymentBatch = Factory.NewWithValidTestData<APPaymentBatchPoster>();
			TestPaymentApproval.AV_APB_PaymentBatch = paymentBatch.PK;

			AssertNotNull("Precondition: PaymentBatch should not be null", TestPaymentApproval.PaymentBatch);
			AssertEquals("Date should be correct", latestRequestReceived.ToLocalBranchTime(), TestPaymentApproval.EPaymentRecipientListLastUpdatedTimeLocal);
		}

		public void TestSetReceiptTypeDoesChangeValueIfNotInDatabase()
		{
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebitLine;
			TestPaymentApproval.AV_ChequeOrReference = "TEST001";
			AssertEquals("TEST001", TestPaymentApproval.AV_ChequeOrReference);

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectDebit;
			AssertEquals("", TestPaymentApproval.AV_ChequeOrReference);
		}

		public void TestValidationWhenInMatchingContext()
		{
			Assert("Should be Payment Approval Validation", typeof(PaymentApprovalValidation).IsAssignableFrom(TestPaymentApproval.Validation.GetType()));
			IMatchingCollection parentCollectionForMatching = new IMatchingCollection(Factory);
			parentCollectionForMatching.Add(TestPaymentApproval);
			Assert("Should be Payment Approval Validation" + " Header.Validation Type is " + TestPaymentApproval.Validation.GetType().FullName, typeof(PaymentApprovalValidation).IsAssignableFrom(TestPaymentApproval.Validation.GetType()));
		}

		public void TestValidationTypeWhenPaymentIsInDB()
		{
			Factory.Save();

			PaymentApprovalBase paymentApprovalToLoad = (PaymentApprovalBase)Factory.Load(GetExpectedBusinessObjectType(), TestPaymentApproval.PK);

			IMatchingCollection parentCollectionForMatching = new IMatchingCollection(Factory);
			parentCollectionForMatching.Add(paymentApprovalToLoad);
			Assert("Should be Matching Validation. Validation Type is " + paymentApprovalToLoad.Validation.GetType().FullName, typeof(PaymentApprovalValidation).IsAssignableFrom(paymentApprovalToLoad.Validation.GetType()));
		}

		public void TestValidateChequeBook()
		{
			Assert("Precondition: cheque book should have no errors", !TestPaymentApproval.AV_AKInfo.HasErrors());
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_AK = ZGuid.Empty;
			Assert("ChequeBook should have errors since it cant be empty", TestPaymentApproval.AV_AKInfo.HasErrors());

			TestPaymentApproval.AV_AK = ZGuid.Invalid;
			Assert("ChequeBook should have errors since the guid is invalid", TestPaymentApproval.AV_AKInfo.HasErrors());

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.CreditCard;
			TestPaymentApproval.AV_AK = ZGuid.Empty;
			Assert("ChequeBook should not have errors since it's not required for CreditCard", !TestPaymentApproval.AV_AKInfo.HasErrors());
		}

		public void TestValidateChequeOrReference()
		{
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			TestPaymentApproval.AV_ChequeOrReference = "QWER";
			Assert("Should be no errors, CASH allows alphabetical chars", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.CreditCard;
			TestPaymentApproval.AV_ChequeOrReference = "asdf";
			Assert("Should be no errors, CreditCard allows alphabetical chars", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.DirectCredit;
			TestPaymentApproval.AV_ChequeOrReference = "ZXCV";
			Assert("Should be no errors, DirectCredit allows alphabetical chars", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_ChequeOrReference = "UIOP";
			Assert("Should give errors, Cheque allows numbers only", TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());
		}

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

			TestPaymentApproval.AV_AB = bank.PK;
			TestPaymentApproval.AV_AK = testChequeBook.PK;  // must be set in this order\
			TestPaymentApproval.PopulateChequeNumberFromChequeBook();

			AssertEquals("Cheque Number should default to 00006", "00006", TestPaymentApproval.AV_ChequeOrReference);
			Assert("Cheque Number should have no errors", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_ChequeOrReference = "5";
			Assert("Cheque Number should have no errors", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			TestPaymentApproval.AV_ChequeOrReference = "3";
			Assert("Cheque Number should have no errors because the number belongs to a cancelled payment", !TestPaymentApproval.AV_ChequeOrReferenceInfo.HasErrors());
		}

		public void TestValidatePaymentType()
		{
			Assert("Precondition: ReceiptType should have no errors", !TestPaymentApproval.AV_PaymentTypeInfo.HasErrors());
			TestPaymentApproval.AV_PaymentType = "#$#";
			Assert("Invalid ReceiptType should produce errors", TestPaymentApproval.AV_PaymentTypeInfo.HasErrors());
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			Assert("Valid ReceiptType - should have no errors", !TestPaymentApproval.AV_PaymentTypeInfo.HasErrors());
		}

		public void TestCheckAV_Amount()
		{
			TestPaymentApproval.IsProcessingPaymentDetail = true;
			TestPaymentApproval.AV_Amount = 10M;
			Assert("OSAmount should not have errors", !TestPaymentApproval.AV_AmountInfo.HasErrors());

			TestPaymentApproval.AV_Amount = 0M;
			Assert("OSAmount should not have errors", !TestPaymentApproval.AV_AmountInfo.HasErrors());

			TestPaymentApproval.AV_Amount = -90M;
			Assert("OSAmount should have errors since cannot be negative", TestPaymentApproval.AV_AmountInfo.HasErrors());

			TestPaymentApproval.IsProcessingPaymentDetail = false;
			TestPaymentApproval.AV_Amount = 0m;
			Assert("OSAmount should have errors since cannot be negative or zero", TestPaymentApproval.AV_AmountInfo.HasErrors());

			TestPaymentApproval.AV_Amount = -1m;
			Assert("OSAmount should have errors since cannot be negative or zero", TestPaymentApproval.AV_AmountInfo.HasErrors());

			TestPaymentApproval.AV_Amount = 10m;
			Assert("OSAmount should not have errors", !TestPaymentApproval.AV_AmountInfo.HasErrors());
		}

		public void TestAV_Calc_ChequeReferenceLabel()
		{
			ZString chequeNumber = "Check Number";
			ZString referenceNumber = "Reference Number";

			TestPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("AV_Calc_ChequeReferenceLabel", chequeNumber, TestPaymentApproval.AV_Calc_ChequeReferenceLabel);

			TestPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("AV_Calc_ChequeReferenceLabel", referenceNumber, TestPaymentApproval.AV_Calc_ChequeReferenceLabel);

			TestPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("AV_Calc_ChequeReferenceLabel", chequeNumber, TestPaymentApproval.AV_Calc_ChequeReferenceLabel);
		}

		public void TestBackpostedPaymentHasCorrectPostDate()
		{
			ZDecimal invoice1OSAmount = 216m;
			ZDecimal invoice1ExchangeRate = 0.72m;
			ZDecimal invoice1LocalAmount = 300m;
			RefCurrency invoice1Currency = TestObjectCreator.USD;

			ZDecimal invoice2OSAmount = 375m;
			ZDecimal invoice2ExchangeRate = 0.75m;
			ZDecimal invoice2LocalAmount = 500m;
			RefCurrency invoice2Currency = TestObjectCreator.USD;

			ZDecimal approvalOSAmount = 600m;
			ZDecimal approvalExchangeRate = 0.75m;
			ZDecimal approvalLocalAmount = 800m;
			RefCurrency approvalCurrency = TestObjectCreator.USD;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestPaymentApproval.AV_PaymentDate = ZDateTime.Now.AddDays(-1);
			TestPaymentApproval.AV_PostDate = ZDateTime.Now.AddDays(-5);

			TestSaveReloadAndPostPaymentApprovalItems_Core(
						invoice1OSAmount, invoice1ExchangeRate, invoice1LocalAmount, invoice1Currency,
						invoice2OSAmount, invoice2ExchangeRate, invoice2LocalAmount, invoice2Currency,
						approvalOSAmount, approvalExchangeRate, approvalLocalAmount, approvalCurrency);
		}

		public void TestIsBackDatePostingNotAllowedAndPostDateNotToday()
		{
			TestPaymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-5);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Assert("BackDatePosting should be allowed", !TestPaymentApproval.IsBackDatePostingNotAllowedAndPostDateNotToday);

			TestPaymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-5);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Assert("BackDatePosting should not be allowed", TestPaymentApproval.IsBackDatePostingNotAllowedAndPostDateNotToday);

			TestPaymentApproval.AV_PostDate = ZDateTime.Today.AddDays(-5);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			Assert("BackDatePosting should not be allowed", TestPaymentApproval.IsBackDatePostingNotAllowedAndPostDateNotToday);
		}

		public void TestCreatePaymentAndMatchWithTransactionsFromManyOrgsAndAccrossLedgers()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency aUD = TestObjectCreator.AUD;

			Invoice aPInvoice1 = CreateInvoice(newFactory, TestOrgHeader, typeof(APInvoice), 1000m, "00001001", aUD);
			Invoice aPInvoice2 = CreateInvoice(newFactory, TestOrgHeader2, typeof(APInvoice), 1000m, "00001002", aUD);
			Invoice aPInvoice3 = CreateInvoice(newFactory, TestOrgHeader3, typeof(APInvoice), 1000m, "00001003", aUD);
			Invoice aRInvoice1 = CreateInvoice(newFactory, TestOrgHeader3, typeof(ARInvoice), 1000m, "00001001", aUD);
			Invoice aRInvoice2 = CreateInvoice(newFactory, TestOrgHeader4, typeof(ARInvoice), 1000m, "00001002", aUD);

			aPInvoice1 = Factory.Load<APInvoice>(aPInvoice1.PK);
			aPInvoice2 = Factory.Load<APInvoice>(aPInvoice2.PK);
			aPInvoice3 = Factory.Load<APInvoice>(aPInvoice3.PK);
			aRInvoice1 = Factory.Load<ARInvoice>(aRInvoice1.PK);
			aRInvoice2 = Factory.Load<ARInvoice>(aRInvoice2.PK);

			AssertEquals("APInvoice1.AH_OutstandingAmount", -1000m, aPInvoice1.AH_OutstandingAmount);
			AssertEquals("APInvoice2.AH_OutstandingAmount", -1000m, aPInvoice2.AH_OutstandingAmount);
			AssertEquals("APInvoice3.AH_OutstandingAmount", -1000m, aPInvoice3.AH_OutstandingAmount);
			AssertEquals("ARInvoice1.AH_OutstandingAmount", 1000m, aRInvoice1.AH_OutstandingAmount);
			AssertEquals("ARInvoice2.AH_OutstandingAmount", 1000m, aRInvoice2.AH_OutstandingAmount);

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;

			OrgLedgerFilter org2Filter = TestPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			org2Filter.Organization = TestOrgHeader2.PK;
			org2Filter.APLedger = true;
			org2Filter.ARLedger = true;
			OrgLedgerFilter org3Filter = TestPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			org3Filter.Organization = TestOrgHeader3.PK;
			org3Filter.APLedger = true;
			org3Filter.ARLedger = true;

			OrgLedgerFilter org4Filter = TestPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			org4Filter.Organization = TestOrgHeader4.PK;
			org4Filter.APLedger = true;
			org4Filter.ARLedger = true;

			TestPaymentApproval.MatchingBaseObject.ReloadSettlementOrgTransactions();

			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 1000m;

			BusinessObject[] invoicesToMatch = new BusinessObject[] { aPInvoice1, aPInvoice2, aPInvoice3, aRInvoice1, aRInvoice2 };
			TestPaymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(invoicesToMatch);
			((IMatching)aPInvoice1).OSPartialPaymentAmount = ((IMatching)aPInvoice1).OSOutstandingAmount;
			((IMatching)aPInvoice2).OSPartialPaymentAmount = ((IMatching)aPInvoice2).OSOutstandingAmount;
			((IMatching)aPInvoice3).OSPartialPaymentAmount = ((IMatching)aPInvoice3).OSOutstandingAmount;
			((IMatching)aRInvoice1).OSPartialPaymentAmount = ((IMatching)aRInvoice1).OSOutstandingAmount;
			((IMatching)aRInvoice2).OSPartialPaymentAmount = ((IMatching)aRInvoice2).OSOutstandingAmount;

			TestPaymentApproval.MatchingBaseObject.MatchAndClearTransactions(); // Called by Gui
			Factory.Save();

			if (!TestPaymentApproval.PostsOnSave)
			{
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				TestPaymentApproval.CreateNewPayment();
			}

			Factory.Save();

			AssertNotNull("Payment", TestPaymentApproval.NewPayment);
			AssertEquals("Payment OSExTaxAmount", TestPaymentApproval.AV_Amount, TestPaymentApproval.NewPayment.AH_OSExTaxAmount);
			AssertEquals("Payment LocalExTaxAmount", TestPaymentApproval.AV_Calc_LocalAmount, TestPaymentApproval.NewPayment.AH_LocalExTaxAmount);

			AssertEquals("NewPayment.AH_OutstandingAmount", 0m, TestPaymentApproval.NewPayment.AH_OutstandingAmount);
			AssertEquals("APInvoice1.AH_OutstandingAmount", 0m, aPInvoice1.AH_OutstandingAmount);
			AssertEquals("APInvoice2.AH_OutstandingAmount", 0m, aPInvoice2.AH_OutstandingAmount);
			AssertEquals("APInvoice3.AH_OutstandingAmount", 0m, aPInvoice3.AH_OutstandingAmount);
			AssertEquals("ARInvoice1.AH_OutstandingAmount", 0m, aRInvoice1.AH_OutstandingAmount);
			AssertEquals("ARInvoice2.AH_OutstandingAmount", 0m, aRInvoice2.AH_OutstandingAmount);
		}

		public void TestTransactionTypeForMatching()
		{
			ZString expectedTransactionType = TestPaymentApproval.PostsOnSave ? ZArchitecture.Core.TransactionTypes.Payment : "UNA";
			AssertEquals("TransactionType for Matching", expectedTransactionType, TestPaymentApproval.TransactionType);
		}

		public void TestTransactionCategoryForMatching()
		{
			AssertEquals("TransactionCategory for Matching", ZString.Empty, TestPaymentApproval.TransactionCategory);
		}

		public void TestUserAllowedToBackPost()
		{
			bool receivablesAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool payablesAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AssertEquals("UserAllowedToBackPost", false, TestPaymentApproval.UserAllowedToBackPost);

				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals("UserAllowedToBackPost", TestPaymentApproval.AV_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable, TestPaymentApproval.UserAllowedToBackPost);

				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				AssertEquals("UserAllowedToBackPost", TestPaymentApproval.AV_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable, TestPaymentApproval.UserAllowedToBackPost);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = receivablesAllowed;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablesAllowed;
			}
		}

		public void TestDefaultBankAccountFromOrg()
		{
			TestOrgHeader.CompanyData.OB_AB_APDefaultBankAccount = Factory.New<AccBankAccount>().PK;

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			ZGuid expectedBank = (testPaymentApproval.AV_Ledger == LedgerTypes.AccountsPayable) ? TestOrgHeader.CompanyData.OB_AB_APDefaultBankAccount : ZGuid.Empty;
			AssertEquals("AV_AB", expectedBank, testPaymentApproval.AV_AB);
		}

		public virtual void TestAV_AK_ReadOnly()
		{
			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			foreach (var type in testPaymentApproval.Lookups.PaymentMethods.GetAllCodes())
			{
				testPaymentApproval.AV_PaymentType = type;
				AssertEquals(!testPaymentApproval.IsCheque, testPaymentApproval.AV_AKInfo.ReadOnly);
			}
		}

		public void TestOSPartialPaymentAmount_ReadOnly()
		{
			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			Assert("OSPartialPaymentAmount should be read only", testPaymentApproval.OSPartialPaymentAmount_ReadOnly);
		}

		public void TestOSPartialPaymentAmountIsDefaultedFromAV_Amount()
		{
			AssertEquals("OSPartialPaymentAmount should be equal to AV_Amount", TestPaymentApproval.AV_Amount, TestPaymentApproval.OSPartialPaymentAmount);
			TestPaymentApproval.OSPartialPaymentAmount = TestPaymentApproval.AV_Amount - 1;
			AssertEquals("OSPartialPaymentAmount should be as assigned", TestPaymentApproval.AV_Amount - 1, TestPaymentApproval.OSPartialPaymentAmount);

			Factory.Save();
			PaymentApprovalBase reloadedPaymentApproval = new BusinessObjectFactory().Load(GetExpectedBusinessObjectType(), TestPaymentApproval.PK) as PaymentApprovalBase;
			AssertEquals("Reloaded OSPartialPaymentAmount should be equal to AV_Amount", TestPaymentApproval.AV_Amount, reloadedPaymentApproval.OSPartialPaymentAmount);
		}

		public void TestAV_Calc_ChequeNumberIsAutoAllocatedLabel()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			Assert("Label should be empty so far", testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testPaymentApproval.AV_AK = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Auto allocation is disabled", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);

			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel);
			Assert("Auto Allocation is enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Auto allocation is disabled", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel);
			Assert("Auto Allocation is enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			testPaymentApproval.FImportedHotCheque_ForTestOnly = Factory.NewWithValidTestData<AccHotCheque>();
			Assert("Hot cheque is imported, won't auto allocate", testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
			Assert("Auto allocation is disabled", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			testPaymentApproval.FImportedHotCheque_ForTestOnly = null;
			AssertEquals("Hot cheque is not imported, should auto allocate", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel);
			Assert("Auto Allocation is enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			testPaymentApproval.AV_ChequeOrReference = "123";
			Assert(testPaymentApproval.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("AV_ChequeOrReference is not empty, label should be empty", testPaymentApproval.AV_Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestAV_Calc_ChequeIsAutoPrintedLabel()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			Assert("Label should be empty so far", testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testPaymentApproval.AV_AK = testChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel.IsEmpty);

			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel);
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("Payment type not 'Cheque'", testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			testPaymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Pyment type is 'Cheque' again", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel);
			testPaymentApproval.FImportedHotCheque_ForTestOnly = Factory.NewWithValidTestData<AccHotCheque>();
			Assert("Hot cheque is imported, won't auto print", testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			testPaymentApproval.FImportedHotCheque_ForTestOnly = null;
			AssertEquals("Hot cheque is not imported, should auto print", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel);
			testPaymentApproval.AV_ChequeOrReference = "123";
			Assert(testPaymentApproval.IsChequeNumberAutoAllocated_ForTestOnly);
			Assert("AV_ChequeOrReference is not empty, label should be empty", testPaymentApproval.AV_Calc_ChequeIsAutoPrintedLabel.IsEmpty);
		}

		public virtual void TestSetAV_AK()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			testBookWithAutoAllocation.AK_CurrentNo = 2;
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			testChequeBook.AK_CurrentNo = 2;

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;

			testPaymentApproval.AV_AK = testChequeBook.PK;
			AssertEquals("Cheque Number should be populated", "2", testPaymentApproval.AV_ChequeOrReference);
			Assert("Cheque Number should not be readonly", !testPaymentApproval.AV_ChequeOrReferenceInfo.ReadOnly);

			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			Assert("Cheque number should be reset", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("Cheque number should be read only", testPaymentApproval.AV_ChequeOrReferenceInfo.ReadOnly);
		}

		[TestDate(2012, 4, 10)]
		public virtual void TestCreateNewPaymentCoreWhenBackDatingAndPostDateIsNotToday()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			AccChequeBook testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			testPaymentApproval.AV_PostDate = new ZDateTime(2012, 3, 18);
			testPaymentApproval.MatchingBaseObject.MatchDate = ZDateTime.Today;
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			AssertEquals("Payment.AH_PostDate should be today's date", ZDateTime.Today, testPaymentApproval.NewPayment_ForTestOnly.AH_PostDate);
		}

		[TestDate(2015, 8, 06, 10, 30, 00)]
		public void TestSameDatePostingDoesNotChangePostDateAndAuthorizedUser()
		{
			if (!TestPaymentApproval.PostsOnSave)
			{
				Assert(true);
			}
			else
			{
				TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
				TestPaymentApproval.AV_ChequeOrReference = "CSH";
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;
				TestPaymentApproval.IsAllowedToPost = true;
				var postDate = ZDateTime.Now.AddMinutes(-30);
				TestPaymentApproval.AV_PostDate = postDate;
				SetUpRegistryForTest();
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				try
				{
					GlbStaff newStaff = Factory.New<GlbStaff>();
					newStaff.GS_Code = "XYZ";

					SetUpAsFullyAuthorisedForTests(TestPaymentApproval, newStaff);
					TestObjectCreator.FillPaymentMatchTransactions("2", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, 7001m);

					Factory.Save();
					AssertEquals("Should be posted", PaymentApprovalStatus.Posted, TestPaymentApproval.AV_Status);
					AssertEquals("Post date should not be changed after posting", postDate, TestPaymentApproval.AV_PostDate);
					AssertEquals("approvals should remain same", newStaff.GS_Code, TestPaymentApproval.AV_GS_NKApproval1st);
				}
				finally
				{
					ResetRegistryForTest();
				}
			}
		}

		protected void CheckPostDateAndMatchDate(ZDateTime setDate, ZDateTime expectedDate, ZDateTime expectedMatchDate, string matachTransactionNumber)
		{
			var testPaymentApproval = (PaymentApprovalBase)Factory.New(GetExpectedBusinessObjectType());
			testPaymentApproval.AV_PostDate = setDate;
			testPaymentApproval.AV_PaymentComment = "AP PAYMENT DESCRIPTION";
			testPaymentApproval.AV_PaymentType = ReceiptTypes.Cash;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsPayable ? TestObjectCreator.Creditor1.PK : TestObjectCreator.Debtor.PK;
			testPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			testPaymentApproval.ExchangeRate.Currency = "AUD";
			TestPaymentApproval.AV_ChequeOrReference = "CSH";
			testPaymentApproval.AV_Amount = 200m;
			testPaymentApproval.MatchingBaseObject.MatchDate = expectedMatchDate;
			TestObjectCreator.FillPaymentMatchTransactions(matachTransactionNumber, typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsPayable ? TestObjectCreator.Creditor1 : TestObjectCreator.Debtor, testPaymentApproval, 200m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();

			AssertEquals("AV_PostDate should be expected date", expectedDate.Date, testPaymentApproval.AV_PostDate.Date);
			AssertEquals("Matching Date should be expected date", expectedMatchDate, testPaymentApproval.NewPayment.MatchingBaseObject.MatchDate.Date);
		}

		public void TestAV_PostDateWhenMatching()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();

			var pastPostDate = ZDateTime.Today.AddDays(-3);
			var todayPostDate = ZDateTime.Today;
			var futurePostDate = ZDateTime.Today.AddDays(3);

			bool futurePost = Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed;
			bool payablePost = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool receivePost = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				#region BackDateIsAllowed = FALSE, (FuturePosting = TRUE/FALSE)

				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

				CheckPostDateAndMatchDate(futurePostDate, todayPostDate, todayPostDate, "1");
				CheckPostDateAndMatchDate(todayPostDate, todayPostDate, todayPostDate, "2");
				CheckPostDateAndMatchDate(pastPostDate, todayPostDate, todayPostDate, "3");

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				CheckPostDateAndMatchDate(futurePostDate, futurePostDate, futurePostDate, "4");
				CheckPostDateAndMatchDate(todayPostDate, todayPostDate, todayPostDate, "5");
				CheckPostDateAndMatchDate(pastPostDate, todayPostDate, todayPostDate, "6");

				#endregion

				#region BackDateIsAllowed = TRUE, (FuturePosting = TRUE/FALSE)

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

				CheckPostDateAndMatchDate(futurePostDate, todayPostDate, todayPostDate, "7");
				CheckPostDateAndMatchDate(todayPostDate, todayPostDate, todayPostDate, "8");
				CheckPostDateAndMatchDate(pastPostDate, pastPostDate, todayPostDate, "9");

				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
				CheckPostDateAndMatchDate(futurePostDate, futurePostDate, futurePostDate, "10");
				CheckPostDateAndMatchDate(todayPostDate, todayPostDate, todayPostDate, "11");
				CheckPostDateAndMatchDate(pastPostDate, pastPostDate, todayPostDate, "12");

				#endregion
			}
			finally
			{
				Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = futurePost;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = payablePost;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = receivePost;
			}
		}

		public virtual void TestCreateNewPaymentCore_AutoAllocation()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			AccChequeBook testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			Assert("Payment.AH_ChequeOrReference should be empty as it is going to be autoallocated on posting", testPaymentApproval.NewPayment_ForTestOnly.AH_ChequeOrReference.IsEmpty);
			Assert("AV_ChequeOrReference should be empty as it is going to be autoallocated on posting", testPaymentApproval.AV_ChequeOrReference.IsEmpty);

			testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testChequeBook.PK;
			testPaymentApproval.AV_Amount = 100m;
			TestObjectCreator.FillPaymentMatchTransactions("2", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			AssertEquals("Payment.AH_ChequeOrReference should not be empty as it won't be autoallocated on posting", "2", testPaymentApproval.NewPayment_ForTestOnly.AH_ChequeOrReference);
			AssertEquals("AV_ChequeOrReference should not be empty as it won't be autoallocated on posting", "2", testPaymentApproval.AV_ChequeOrReference);
		}

		public virtual void TestSetFieldsOnPayment_AH_GE()
		{
			var chequeBookFactory = new BusinessObjectFactory();
			var testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			var bankAccount = testBookWithAutoAllocation.BankAccount;
			var testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			testPaymentApproval.DepartmentForImport_ForTestOnly = TestObjectCreator.NonCurrentDepartment.PK;

			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			AssertEquals(TestObjectCreator.NonCurrentDepartment.PK, testPaymentApproval.NewPayment_ForTestOnly.AH_GE);
		}

		public virtual void TestCreateNewPaymentCore_WithErrors()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			AccChequeBook testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			testPaymentApproval.AddRowError("Error to prevent creating payment");
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNull("New payment should not be created", testPaymentApproval.NewPayment_ForTestOnly);
			AssertContains("AccPaymentApproval: Error to prevent creating payment", testPaymentApproval.PaymentCreationErrorMessages.ToMessageListString());

			testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			((IMatching)testPaymentApproval).CurrentMatchGroup.AddNew().AP_Amount = 50M;
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment was created. It is still unclear wheter is it right or not", testPaymentApproval.NewPayment_ForTestOnly);
		}

		public virtual void TestUnsuccessfulPost_WithBusinessContextPostDraftPaymentApproval_ShouldRevertToDraftStatus_()
		{
			var chequeBookFactory = new BusinessObjectFactory();
			var testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			var bankAccount = testBookWithAutoAllocation.BankAccount;
			var testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			using (new DisposableAction(() => testPaymentApproval.SetContext(Enterprise.Integration.Accounting.BusinessContext.PostDraftPaymentApproval), () => testPaymentApproval.RemoveContext(Enterprise.Integration.Accounting.BusinessContext.PostDraftPaymentApproval)))
			{
				testPaymentApproval.AV_OH = TestOrgHeader.PK;
				testPaymentApproval.AV_AB = bankAccount.PK;
				testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
				testPaymentApproval.AV_Amount = 100m;
				testPaymentApproval.AddRowError("Error to prevent creating payment");

				AssertNotEquals(PaymentApprovalStatus.Draft, testPaymentApproval.AV_Status);
				testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
				AssertNull("New payment should not be created", testPaymentApproval.NewPayment_ForTestOnly);
				AssertContains("AccPaymentApproval: Error to prevent creating payment", testPaymentApproval.PaymentCreationErrorMessages.ToMessageListString());
				AssertEquals("Payment type should have reverted to Draft", PaymentApprovalStatus.Draft, testPaymentApproval.AV_Status);
			}
		}

		#region IChequeNumberAutoAllocation Members Tests

		public void TestIChequeNumberAutoAllocation_ChequeBookPK()
		{
			AccChequeBook testChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			AssertNull("Should return empty cheque book", ((IChequeNumberAutoAllocation)testPaymentApproval).ChequeBook);
			testPaymentApproval.AV_AK = testChequeBook.PK;
			AssertEquals("Should return TestChequeBook PK", testChequeBook, ((IChequeNumberAutoAllocation)testPaymentApproval).ChequeBook);
		}

		public void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			testBookWithAutoAllocation.AK_IsActive = ZBool.True;
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			Assert("Payment.AH_ChequeOrReference should be empty as it is going to be autoallocated on posting", testPaymentApproval.NewPayment_ForTestOnly.AH_ChequeOrReference.IsEmpty);
			Assert("AV_ChequeOrReference should be empty as it is going to be autoallocated on posting", testPaymentApproval.AV_ChequeOrReference.IsEmpty);

			((IChequeNumberAutoAllocation)testPaymentApproval).AssignChequeNumber("123");
			AssertEquals("AV_ChequeOrReference should be set", "123", testPaymentApproval.AV_ChequeOrReference);
			AssertEquals("AH_ChequeOrReference should be set on the payment", "123", testPaymentApproval.NewPayment.AH_ChequeOrReference);

			testPaymentApproval.ChequeBook.AK_IsActive = ZBool.False;
			((IChequeNumberAutoAllocation)testPaymentApproval).AllocationOrPrintingFailed();
			Assert("Cheque book should be reloaded", testPaymentApproval.ChequeBook.AK_IsActive);
			Assert("AV_ChequeOrReference should be reset", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
			Assert("AH_ChequeOrReference should be reset on the payment", testPaymentApproval.NewPayment.AH_ChequeOrReference.IsEmpty);
		}

		public void AssertIChequeNumberAutoAllocationCore(bool isSaveAsDraft)
		{
			var testBookWithAutoAllocation = GetAutoPrintChequeBook(Factory, 1, 3, 2);
			testBookWithAutoAllocation.AK_IsActive = ZBool.True;
			var bankAccount = testBookWithAutoAllocation.BankAccount;
			Factory.Save();

			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			AssertEquals("Should return False by defualt", false, ((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);

			if (isSaveAsDraft)
			{
				testPaymentApproval.AV_Status = PaymentApprovalStatus.Draft;
				testPaymentApproval.Factory.SetContext(Enterprise.Integration.Accounting.BusinessContext.SavingPaymentApprovalAsDraft);
			}

			testPaymentApproval.IsPostWithoutMatching = true;
			testPaymentApproval.IsAllowedToPost = true;
			var allocator = new PaymentChequeNumberAllocatorBase(testPaymentApproval, testPaymentApproval.Factory);
			BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving());

			testBookWithAutoAllocation.Reload();
			if (isSaveAsDraft)
			{
				AssertNull(testPaymentApproval.NewPayment);
				AssertEquals(true, testPaymentApproval.IsInDatabase);
				AssertEquals(2m, testBookWithAutoAllocation.AK_CurrentNo);
				AssertNullOrEmpty(testPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Should return False since no NewPayment", false, ((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
			}
			else
			{
				AssertNotNull(testPaymentApproval.NewPayment);
				AssertEquals(true, testPaymentApproval.IsInDatabase);
				AssertEquals(3m, testBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("2", testPaymentApproval.AV_ChequeOrReference);
				AssertEquals("Should return true since created NewPayment", true, ((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
			}
		}

		public void TestIChequeNumberAutoAllocation_AllocationPerformed()
		{
			AssertIChequeNumberAutoAllocationCore(false);
		}

		public void TestIChequeNumberAutoAllocation_AllocationNotPerformed()
		{
			AssertIChequeNumberAutoAllocationCore(true);
		}

		public void TestIChequeNumberAutoAllocation_Printing_ObjectPK()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccBankAccount bankAccount = chequeBookFactory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook testChequeBook = chequeBookFactory.NewWithValidTestData<AccChequeBook>();

			testChequeBook.AK_StartNo = 1;
			testChequeBook.AK_LastNo = 3;
			testChequeBook.AK_CurrentNo = 2;
			testChequeBook.AK_AB = bankAccount.PK;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testChequeBook.PK;
			testPaymentApproval.AV_Amount = 100m;

			AssertEquals(Guid.Empty, ((IChequeNumberAutoAllocation)testPaymentApproval).Printing_ObjectPK);
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertEquals(testPaymentApproval.NewPayment.PK, ((IChequeNumberAutoAllocation)testPaymentApproval).Printing_ObjectPK);
		}

		public void TestIChequeNumberAutoAllocation_Printing_PrinterPK()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			AssertEquals("Should return empty printer", ZGuid.Empty, ((IChequeNumberAutoAllocation)testPaymentApproval).Printing_PrinterPK);
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Should return TestBookWithAutoAllocation.AK_SQ", testBookWithAutoAllocation.AK_SQ, ((IChequeNumberAutoAllocation)testPaymentApproval).Printing_PrinterPK);
		}

		public void TestIChequeNumberAutoAllocation_ChequeIsAutoPrinted()
		{
			BusinessObjectFactory chequeBookFactory = new BusinessObjectFactory();
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook(chequeBookFactory, 1, 3, 2);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;
			chequeBookFactory.Save();

			PaymentApprovalBase testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;

			Assert("Default value", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted = ZBool.True;
			Assert("Default value should not change", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPaymentCore_ForTestOnly();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment_ForTestOnly);
			Assert("Default value should not change", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted = ZBool.True;
			Assert("Value should be changed", ((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
		}
		#endregion

		public void TestTransactionHeaderUseTypeDecider()
		{
			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4 : TestOrgHeader, TestPaymentApproval, 1000m);
			TestPaymentApproval.CreateNewPayment();

			Factory.Save();

			AssertNotNull("PaymentApproval.TransactionHeader should be Payment", TestPaymentApproval.TransactionHeader as Payment);
		}

		public void TestForeignCurrencyPayment()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APPayment payment = newFactory.LoadTop1<APPayment>(new ZQuery().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull("There shouldn't be an AP Payment in the database.", payment);

			AssertEquals("GlbCompany.Currency", Core.Constants.CurrencyCodes.Australia, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			APPaymentApprovalWithoutAuthorisation approval = newFactory.New<APPaymentApprovalWithoutAuthorisation>();
			approval.IsPostWithoutMatching = true;

			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);

			AccBankAccount bankAccount = testObjectCreator.USDBankAccount;
			OrgHeader orgHeader = testObjectCreator.AALSHI;

			approval.AV_OH = orgHeader.PK;
			approval.AV_AB = bankAccount.PK;
			approval.AV_RX_NKPaymentCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			approval.ExchangeRate.Rate = 2.0m;
			approval.AV_Amount = 250.00m;
			approval.AV_PaymentType = ReceiptTypes.EFT;
			approval.AV_ChequeOrReference = "123456";

			newFactory.Save();

			BusinessObjectFactory newFactory2 = new BusinessObjectFactory();
			ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, Core.Constants.CurrencyCodes.UnitedStates);
			query.AddToFilter(AccTransactionHeaderSchema.AH_AB, bankAccount.PK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ReceiptType, ReceiptTypes.EFT);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			payment = newFactory2.LoadTop1<APPayment>(query);
			AssertNotNull("There should be an AP Payment in the database.", payment);
			AssertEquals("AH_OSExTaxAmount", 250.00m, payment.AH_OSExTaxAmount);
		}

		public void TestTransactionDate()
		{
			ZDateTime transactionDate = ZDateTime.Now.AddDays(10);
			TestPaymentApproval.TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, TestPaymentApproval.TransactionDate);
		}

		public void TestAV_PostDate_ReadOnly()
		{
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;

			AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AV_PostDate_ReadOnly", TestPaymentApproval));
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			AssertEquals("Allowed to post future so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AV_PostDate_ReadOnly", TestPaymentApproval));

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;
			if (TestPaymentApproval.GetType().Name.StartsWith("AR"))
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			}
			else
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			}

			AssertEquals("Allowed to post past so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AV_PostDate_ReadOnly", TestPaymentApproval));

			if (TestPaymentApproval.GetType().Name.StartsWith("AR"))
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			}
			else
			{
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			}
			AssertEquals("Allowed to post future or past, so editable", false, MasterFilesTestHelper.GetNonPublicPropertyValue<bool>("AV_PostDate_ReadOnly", TestPaymentApproval));
		}

		public void TestDisplayInvoiceAddressOverride()
		{
			string dataMember = "DisplayInvoiceAddressOverride";

			PropertyDescriptor property = ZCustomTypeDescriptor.GetProperties(typeof(PaymentApprovalBase))[dataMember];
			AssertNotNull(property);

			string listMember = Enterprise.ZArchitecture.ComponentModel.MetadataAccessor.GetListMember("", property, dataMember);
			Assert(!string.IsNullOrEmpty(listMember));
		}

		public void TestMatchingValidationWhenCreateNewPayment()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			RefCurrency aUD = TestObjectCreator.AUD;

			Invoice aPInvoice = CreateInvoice(newFactory, TestOrgHeader, typeof(APInvoice), 2000m, "00001001", aUD);
			Invoice aRInvoice = CreateInvoice(newFactory, TestOrgHeader4, typeof(ARInvoice), 1000m, "00001002", aUD);

			aPInvoice = Factory.Load<APInvoice>(aPInvoice.PK);
			aRInvoice = Factory.Load<APInvoice>(aRInvoice.PK);

			AssertEquals("APInvoice1.AH_OutstandingAmount", -2000m, aPInvoice.AH_OutstandingAmount);
			AssertEquals("ARInvoice1.AH_OutstandingAmount", 1000m, aRInvoice.AH_OutstandingAmount);

			TestPaymentApproval.AV_OH = ZGuid.Empty;
			TestPaymentApproval.AV_OH = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;

			OrgLedgerFilter org4Filter = TestPaymentApproval.MatchingBaseObject.MatchingFilterBizO.SettlementOrgInfos.AddNew();
			org4Filter.Organization = TestOrgHeader4.PK;
			org4Filter.APLedger = true;
			org4Filter.ARLedger = true;

			TestPaymentApproval.MatchingBaseObject.ReloadSettlementOrgTransactions();

			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 1000m;

			BusinessObject[] invoicesToMatch = new BusinessObject[] { aPInvoice, aRInvoice };
			TestPaymentApproval.MatchingBaseObject.MoveFromUnmatchToMatch(invoicesToMatch);
			((IMatching)aPInvoice).OSPartialPaymentAmount = ((IMatching)aPInvoice).OSOutstandingAmount;
			((IMatching)aRInvoice).OSPartialPaymentAmount = ((IMatching)aRInvoice).OSOutstandingAmount;

			TestPaymentApproval.MatchingBaseObject.MatchAndClearTransactions();
			Factory.Save();

			if (!TestPaymentApproval.PostsOnSave)
			{
				aPInvoice.AH_OutstandingAmount = -1900;
				TestPaymentApproval.AV_Status = PaymentApprovalStatus.FullyApproved;

				Assert("Precondition", !TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());
				Assert("Precondition", AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.Value);

				TestPaymentApproval.CreateNewPayment();

				Assert(TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());
				AssertContains("OS Partial Payment Amount: Pay Amount must be between -1900.00 and 0", TestPaymentApproval.PaymentCreationErrorMessages.ToMessageListString());

				TestPaymentApproval.PaymentCreationErrorMessages.Clear();
				AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Assert("Precondition", !TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());
				Assert("Precondition", !AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.Value);

				TestPaymentApproval.CreateNewPayment();

				Assert(!TestPaymentApproval.PaymentCreationErrorMessages.HasErrors());
			}
		}

		public void TestMatchPaymentWithDiscount()
		{
			TestPaymentApproval.AV_OH = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.ExchangeRate.Currency = "AUD";
			TestPaymentApproval.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			TestPaymentApproval.AV_Amount = 200m;
			Factory.Save();

			Assert("Precondition", AccountingConfigurationRegistry.Instance.EnablePaymentApprovalFullMatchingValidation.Value);
			var apDiscount = TestObjectCreator.CreateAPDiscount(-200m, ZDateTime.Today, TestPaymentApproval.AV_OH);
			TestPaymentApproval.MatchingBaseObject.MoveAllFromMatchToUnmatch();
			TestPaymentApproval.MatchingBaseObject.AddMiscellaneousTransaction(apDiscount);
			AssertEquals("Matched Transactions Count", 2, TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Count);
			Assert("Matched Transactions should contain apDiscount", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(apDiscount));
			Assert("Matched Transactions should contain PaymentApprovalBase", TestPaymentApproval.PaymentMatchingBaseObject.MatchedTransactions.Contains(TestPaymentApproval));
			TestPaymentApproval.MatchingBaseObject.MatchAndClearTransactions();
			Factory.Save();
			if (!TestPaymentApproval.PostsOnSave)
			{
				TestPaymentApproval.CreateNewPayment();
			}
			AssertNotNull(TestPaymentApproval.NewPaymentMatchingObject);
			AssertNotEquals("If the matched transactions contain a discount, balance will not be 0 after matched.", 0, TestPaymentApproval.NewPaymentMatchingObject.Balance);
			Assert("We no longer validate balance after matched.", !TestPaymentApproval.NewPaymentMatchingObject.HasErrors());
		}

		#region AddressesOnPayments

		public void TestAddressesOnPayments()
		{
			var aPAddress = TestObjectCreator.CreateAddress(TestOrgHeader2, OrgAddressType.Payables, true);
			var aRAddress = TestObjectCreator.CreateAddress(TestOrgHeader2, OrgAddressType.Receivables, true);
			var contact1 = TestObjectCreator.CreateContact(TestOrgHeader2, "contact 1");
			var contact2 = TestObjectCreator.CreateContact(TestOrgHeader2, "contact 2");

			var payment = TestPaymentApproval;
			payment.AV_OH = ZGuid.Empty;

			AssertEquals(ZGuid.Empty, payment.AV_OH);
			AssertEquals(ZGuid.Empty, payment.AV_OA_AddressOverride);
			AssertEquals(ZGuid.Empty, payment.AV_OC_ContactOverride);

			payment.AV_OH = TestOrgHeader2.PK;

			var expectedAddress = aPAddress.PK; //for both AR and AP, AP address should be defaulted
			AssertEquals(expectedAddress, payment.AV_OA_AddressOverride);
			AssertEquals(ZGuid.Empty, payment.AV_OC_ContactOverride);
			payment.AV_OC_ContactOverride = contact2.PK;

			SetUpForTestAddressesOnPayments();

			TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader2, TestPaymentApproval, 1000m);
			Factory.Save();
			var newPayment = Factory.Load<TransactionHeader>(payment.TransactionHeader.PK);
			AssertEquals(newPayment.AH_OA_InvoiceAddressOverride, payment.AV_OA_AddressOverride);
			AssertEquals(newPayment.AH_OC_InvoiceContactOverride, payment.AV_OC_ContactOverride);

			AssertEquals(newPayment.AH_OA_InvoiceAddressOverride, expectedAddress);
			AssertEquals(newPayment.AH_OC_InvoiceContactOverride, contact2.PK);
		}

		protected virtual void SetUpForTestAddressesOnPayments()
		{
		}

		#endregion

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmpty()
		{
			AssertEquals(nameof(TestPaymentApproval.ReversalStatusCode), ZString.Empty, TestPaymentApproval.ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (GetNewBusinessObject() as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (GetNewBusinessObject() as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			USDSellRate = CreateExchangeRate(TestObjectCreator.USD, 0.75M, Core.Constants.ExchangeRateTypes.Code.SellRate);
			USDBuyRate = CreateExchangeRate(TestObjectCreator.USD, 0.75M, Core.Constants.ExchangeRateTypes.Code.BuyRate);
			GBPSellRate = CreateExchangeRate(TestObjectCreator.GBP, 0.45M, Core.Constants.ExchangeRateTypes.Code.SellRate);
			GBPBuyRate = CreateExchangeRate(TestObjectCreator.GBP, 0.45M, Core.Constants.ExchangeRateTypes.Code.BuyRate);

			TestOrgHeader = TestObjectCreator.CreateOrgHeader("Org1", true, false);
			TestOrgHeader2 = TestObjectCreator.CreateOrgHeader("Org2", true, true);
			TestOrgHeader3 = TestObjectCreator.CreateOrgHeader("Org3", true, true);
			TestOrgHeader4 = TestObjectCreator.CreateOrgHeader("Org4", false, true);

			ExpectedEventMessageForTest = ZString.Empty;
			NumberOfTimesEventWasCalledDuringTest = 0;
			Expected = 0;

			Factory.Save();

			ExchangeRateReader.GetReaderInstance().ClearCache();

			fTestPaymentApproval = (PaymentApprovalBase)Factory.New(GetExpectedBusinessObjectType());
			TestPaymentApproval.AV_OH = TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? TestOrgHeader4.PK : TestOrgHeader.PK;
			TestPaymentApproval.AV_PaymentComment = "AP PAYMENT DESCRIPTION";
			TestPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			TestPaymentApproval.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			TestPaymentApproval.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			TestPaymentApproval.AV_Amount = 1000M;

			Assert("Precondition: Test Payment Approval is not in Database", !TestPaymentApproval.IsInDatabase);
			CurrentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		}

		Invoice CreateInvoice(BusinessObjectFactory factory, OrgHeader org, Type invoiceType, ZDecimal oSAmount, ZString transactionNum, RefCurrency currency)
		{
			Invoice invoice = (Invoice)factory.New(invoiceType);
			invoice.AH_OH = org.PK;
			invoice.AH_TransactionNum = transactionNum;
			invoice.AH_RX_NKTransactionCurrency = currency.RX_Code;
			new TestObjectCreator(factory).CreateInvoiceLine(invoice, currency, invoice.AH_ExchangeRate, oSAmount, 0m, 0m);
			factory.Save();
			return invoice;
		}

		AccChequeBook GetAutoPrintChequeBook()
		{
			return GetAutoPrintChequeBook(null, 0, 0, 0);//, null);
		}

		AccChequeBook GetAutoPrintChequeBook(/*AccBankAccount Bank, */BusinessObjectFactory newFactory, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			BusinessObjectFactory testFactory = newFactory ?? Factory;
			AccBankAccount bankAccount = /*(Bank == null) ? */testFactory.NewWithValidTestData<AccBankAccount>(); //: Bank;
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = testFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = testFactory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			testFactory.Save();
			return chequeBook;
		}

		GlbCompany CurrentCompany;

		protected void SetUpRegistryForTest()
		{
			OriginalRegistryValueBeforeTest = AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value;

			PaymentAuthorisationSettingsCollection valuesForTest = new PaymentAuthorisationSettingsCollection();
			UpTo1000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			UpTo2000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			UpTo3000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			UpTo4000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 4000, AuthorisationCodes.ThirdApprovalRequiredOnly);
			UpTo5000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 5000, AuthorisationCodes.FirstAndSecondApprovalRequired);
			UpTo6000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 6000, AuthorisationCodes.FirstAndThirdApprovalRequired);
			UpTo7000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 7000, AuthorisationCodes.SecondAndThirdApprovalRequired);
			Over7000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 7000, AuthorisationCodes.AllThreeApprovalRequired);

			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		protected void ResetRegistryForTest()
		{
			if (OriginalRegistryValueBeforeTest != null)
			{
				AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, OriginalRegistryValueBeforeTest);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return TestPaymentApproval;
		}

		protected abstract ZString ExpectedDefaultLedger
		{
			get;
		}

		protected PaymentAuthorisationSettings GetNewAuthorisationSetting(PaymentAuthorisationSettingsCollection collection,
			ZString range, ZInt amount, ZString requirement)
		{
			PaymentAuthorisationSettings newSetting = collection.AddNew();
			newSetting.Amount = (ZDecimal)amount;
			newSetting.AuthorisationRequirement = requirement;
			newSetting.Range = range;

			return newSetting;
		}

		protected RefExchangeRate CreateExchangeRate(RefCurrency currency, ZDecimal rate, ZString type)
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			return exchangeRate;
		}

		protected void SetUpAsFullyAuthorisedForTests(PaymentApprovalBase approval, GlbStaff user)
		{
			approval.AV_Amount = 7001M;
			approval.AV_GS_NKApproval1st = user.GS_Code;
			approval.AV_GS_NKApproval2nd = user.GS_Code;
			approval.AV_GS_NKApproval3rd = user.GS_Code;
			approval.AV_Status = PaymentApprovalStatus.FullyApproved;
		}

		protected ZString TypeOfExchangeRateToBeUsed
		{
			get { return TestPaymentApproval.IsPayables ? Core.Constants.ExchangeRateTypes.Code.BuyRate : Core.Constants.ExchangeRateTypes.Code.SellRate; }
		}

		protected void AssertPaymentApprovalSecurityCase(SecurityCheckpoint securityCheckpoint, bool isAllowed, ZString paymentType, bool expectedHasSecurityToPostValue)
		{
			var testPaymentApproval = Factory.New(GetExpectedBusinessObjectType()) as PaymentApprovalBase;
			securityCheckpoint.IsAllowed = isAllowed;
			testPaymentApproval.AV_PaymentType = paymentType;
			var assertionMessage =
				string.Format(
					"When setting security {0} to '{1}' we would expect that the user {2} have access to post a payment approval of type {3}",
					securityCheckpoint.DisplayTextPathToSecurityRight,
					isAllowed ? "allowed" : "denied",
					expectedHasSecurityToPostValue ? "does" : "does not",
					paymentType);

			AssertEquals(assertionMessage, expectedHasSecurityToPostValue, testPaymentApproval.UserHasPaymentTypeSecurityToPost());
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		protected PaymentAuthorisationSettingsCollection OriginalRegistryValueBeforeTest;

		PaymentApprovalBase TestPaymentApproval
		{
			get { return fTestPaymentApproval; }
		}

		protected PaymentApprovalBase fTestPaymentApproval;

		protected OrgHeader TestOrgHeader;
		protected OrgHeader TestOrgHeader2;
		protected OrgHeader TestOrgHeader3;
		protected OrgHeader TestOrgHeader4;

		protected ZString ExpectedEventMessageForTest;
		protected int NumberOfTimesEventWasCalledDuringTest;
		protected int Expected;

		protected PaymentAuthorisationSettings UpTo1000;
		protected PaymentAuthorisationSettings UpTo2000;
		protected PaymentAuthorisationSettings UpTo3000;
		protected PaymentAuthorisationSettings UpTo4000;
		protected PaymentAuthorisationSettings UpTo5000;
		protected PaymentAuthorisationSettings UpTo6000;
		protected PaymentAuthorisationSettings UpTo7000;
		protected PaymentAuthorisationSettings Over7000;

		protected RefExchangeRate USDSellRate;
		protected RefExchangeRate USDBuyRate;

		protected RefExchangeRate GBPSellRate;
		protected RefExchangeRate GBPBuyRate;

		#endregion

		#region Test Workflow

		public void TestApplyWorkflowTemplateWhenSaving()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExceptedWorkflowType;
			var task = template.WorkflowItems.Tasks.AddNew();
			var milestone = template.WorkflowItems.Milestones.AddNew();
			var tigger = template.WorkflowItems.Triggers.AddNew();
			newFactory.Save();

			var workflowProvider = (IWorkflowProvider)TestPaymentApproval;
			AssertEquals("Per Condition", false, TestPaymentApproval.IsInDatabase);
			AssertEquals("Per Condition", 0, workflowProvider.WorkflowItems.Count);
			Factory.Save();
			AssertEquals("Per Condition", true, TestPaymentApproval.IsInDatabase);
			AssertEquals("Invoice should create workflow", 3, workflowProvider.WorkflowItems.Count);
			AssertEquals("Invoice should create Task", 1, workflowProvider.WorkflowItems.Tasks.Count);
			AssertEquals("Invoice should create Milestone", 1, workflowProvider.WorkflowItems.Milestones.Count);
			AssertEquals("Invoice should create Tigger", 1, workflowProvider.WorkflowItems.Triggers.Count);
		}

		public void TestDeleteWorkflowItems()
		{
			var workflowProvider = (IWorkflowProvider)TestPaymentApproval;
			var task = workflowProvider.WorkflowItems.Tasks.AddNew();
			AssertEquals("Per Condition", 1, workflowProvider.WorkflowItems.Count);
			TestPaymentApproval.Delete();
			AssertEquals("No WorkflowItems After Delete", 0, workflowProvider.WorkflowItems.Count);
		}

		public void TestGetWorkflowInformationProvider()
		{
			var workflowInformationProvider = (TestPaymentApproval as IWorkflowProvider).GetWorkflowInformationProvider();
			AssertNull(workflowInformationProvider);
		}

		public void TestWorkflowItems()
		{
			var workflowProvider = (IWorkflowProvider)TestPaymentApproval;
			AssertNotNull(workflowProvider.WorkflowItems);
			AssertEquals(true, TestPaymentApproval.IsRegisteredEditableChildObject(workflowProvider.WorkflowItems));
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var workflowProvider = (IWorkflowProvider)TestPaymentApproval;
			var columnValueRanker = workflowProvider.GetTemplateSelectionCriteria() as ColumnValueRanker;
			AssertNotNull(columnValueRanker);
			AssertArrayEqualsByElements(new object[] { TestPaymentApproval.AV_OH, ZGuid.Empty }, columnValueRanker.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
		}

		public string ExceptedWorkflowType => TestPaymentApproval.Ledger == LedgerTypes.AccountsReceivable ? WorkflowDescriptors.ARPaymentApprovalWorkflowDescriptorCode : WorkflowDescriptors.APPaymentApprovalWorkflowDescriptorCode;

		public void TestWorkflowType()
		{
			var workflowProvider = (IWorkflowProvider)TestPaymentApproval;
			AssertEquals(ExceptedWorkflowType, workflowProvider.WorkflowType);
		}

		#endregion

		public void TestCodeAndDescriptionProperty_ForGenericFindBox()
		{
			var codeProperty = "";
			var descriptionProperty = "";
			AssertNoExceptionThrown("Code and Description must be available to show generic find box", () =>
			{
				codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());
				descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(GetExpectedBusinessObjectType());
			});
			AssertEquals(nameof(PaymentApprovalBase.HeaderTransactionNumber), codeProperty);
			AssertEquals(nameof(PaymentApprovalBase.AV_PaymentComment), descriptionProperty);

			var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
			transaction.AH_TransactionNum = "XX0001234";
			transaction.AH_Desc = "Payment description";
			TestPaymentApproval.AV_AH = transaction.PK;
			TestPaymentApproval.AV_PaymentComment = "Some Comment about the Payment";
			AssertEquals("XX0001234", CodePropertyAttribute.CodeFromBusinessObject(TestPaymentApproval));
			AssertEquals("Some Comment about the Payment", DescriptionPropertyAttribute.DescriptionFromBusinessObject(TestPaymentApproval));
		}

		public void TestPaymentApprovalChangedAfterQuoteCreatedByAnotherUser_WithoutPaymentBatch()
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			approval.AV_Amount = 200m;
			approval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();

			AssertNull(approval.PaymentBatch);
			AssertEquals(false, approval.IsLoadedFromPaymentBatch);
			AssertNotNull("Simulate Quote Cache outdated", approval.PaymentQuotes);

			CreateQuoteInAnotherFactory(approval);
			approval.AV_Amount -= 10m;
			AssertExceptionThrown<ZCannotSaveException>(() => Factory.Save());

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactoryForReload" };
			approval.AV_Amount -= 10m;
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		public void TestPaymentApprovalChangedAfterQuoteCreatedByAnotherUser_WithPaymentBatch()
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB = TestObjectCreator.USDBankAccount.PK;
			Factory.Save();

			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			approval.AV_Amount = 200m;
			approval.AV_Status = PaymentApprovalStatus.Draft;
			approval.AV_APB_PaymentBatch = paymentBatch.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactoryForPaymentProcessingModule" };
			approval = newFactory.Load<APPaymentApprovalWithAuthorisation>(approval.PK);
			AssertNotNull(approval.PaymentBatch);
			AssertEquals(false, approval.IsLoadedFromPaymentBatch);
			AssertNotNull("Simulate Quote Cache outdated", approval.PaymentQuotes);

			CreateQuoteInAnotherFactory(approval);
			approval.AV_Amount -= 10m;
			AssertExceptionThrown<ZCannotSaveException>(() => newFactory.Save());

			newFactory = new BusinessObjectFactory() { NameForDebugging = "NewFactoryForReload" };
			approval.AV_Amount -= 10m;
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		AccEPaymentQuote CreateQuoteInAnotherFactory(PaymentApprovalBase paymentApproval)
		{
			var tempFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var testObjectCreatorInTempFactory = new TestObjectCreator(tempFactory);
			var quote = testObjectCreatorInTempFactory.CreateValidEPaymentQuoteForStatus(EPaymentStatusCodes.Quote.Queued, paymentApproval);
			tempFactory.Save();

			return quote;
		}

		public void TestAfterWeSelectBankAccountOfTypeCashAccount()
		{
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			var paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_AB = testBank.PK;
			AssertEquals("Payment type's value should be 'Cash' after a Bank Account with Account type Cash is selected", ReceiptTypes.Cash, paymentApproval.AV_PaymentType);
			AssertEquals("The currency should be the same as selected cash account", testBank.AB_RX_NKAccountCurrency, paymentApproval.AV_RX_NKPaymentCurrency);
			AssertEquals("The Exchange rate currency should be the same as selected cash account", testBank.AB_RX_NKAccountCurrency, paymentApproval.ExchangeRate.Currency);
			Assert("Exchange rate currency should be readonly", paymentApproval.ExchangeRate.CurrencyInfo.ReadOnly);
			Assert("Payment amount currency should be readonly", paymentApproval.AV_RX_NKPaymentCurrencyInfo.ReadOnly);
		}
	}
}
