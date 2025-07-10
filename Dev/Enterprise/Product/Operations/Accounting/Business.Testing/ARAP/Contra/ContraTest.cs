using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(Contra))]
	public class ContraTest : NonPersistentBusinessObjectTestCase, IReversingTest
	{
		public void TestTransactionNumberGenerator()
		{
			TransactionNumberSequenceCustomisationCollection customisation = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
			element.Include = true;
			element = customisation.AddNew();
			element.Length = 8;
			element.Order = 2;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Include = true;
			element = customisation.AddNew();
			element.Order = 50;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "BNE00000001BRN", TestContra.ARRow.AH_TransactionNum);
			AssertEquals("Transaction number", "BNE00000001BRN", TestContra.APRow.AH_TransactionNum);
		}

		public void TestLocalAmountReadOnly()
		{
			if (TestContra.AH_ExchangeRateCurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				AssertEquals("The AH_InvoiceAmount should be read only as the currency is equal to the CurrentCompany.LocalCurrency", true, TestContra.AH_InvoiceAmountInfo.ReadOnly);
			}
			else if (TestContra.AH_ExchangeRateCurrencyCodeInfo.HasErrors())
			{
				Assert("The AH_InvoiceAmount should be read only as the currency has errors", TestContra.AH_InvoiceAmountInfo.ReadOnly);
			}
			else
			{
				Assert("The AH_InvoiceAmount shouldn't be read only as the currency doesn't has any errors and it's not equal to the CurrentCompany.LocalCurrency", TestContra.AH_InvoiceAmountInfo.ReadOnly);
			}

			Currency newCurrency = new Currency("AAA");

			TestContra.AH_ExchangeRateCurrencyCode = newCurrency.Code;
			Assert("The AH_InvoiceAmount shouldn't be read only as the currency doesn't has any errors and it's not equal to the CurrentCompany.LocalCurrency", !TestContra.AH_InvoiceAmountInfo.ReadOnly);

			TestContra.APRow.AH_RX_NKTransactionCurrency = ZString.Empty;
			Assert("The AH_InvoiceAmount should be read only as the currency has errors", TestContra.AH_InvoiceAmountInfo.ReadOnly);

			TestContra.AH_ExchangeRateCurrencyCodeInfo.ClearAllNotifications();
			TestContra.AH_ExchangeRateCurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			Assert("The AH_InvoiceAmount should be read only as the currency is equal to the CurrentCompany.LocalCurrency", TestContra.AH_InvoiceAmountInfo.ReadOnly);
		}

		public void TestAfterContraPosting()
		{
			TestContra.AH_ExchangeRateAmount = 1;
			TestContra.AH_OSTotal = 34;
			TestContra.AH_ExchangeRateCurrencyCode = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			AssertEquals(new ZDecimal(34), TestContra.AH_InvoiceAmount);
			AssertEquals(new ZDecimal(-34), TestContra.AH_Calc_PayAfterContra);
			AssertEquals(new ZDecimal(-34), TestContra.AH_Calc_RecAfterContra);

			TestContra.AH_ExchangeRateAmount = 2;
			AssertEquals(new ZDecimal(17), TestContra.AH_InvoiceAmount);
			AssertEquals(new ZDecimal(-17), TestContra.AH_Calc_PayAfterContra);
			AssertEquals(new ZDecimal(-17), TestContra.AH_Calc_RecAfterContra);

			TestContra.AH_OSTotal = 100;
			AssertEquals(new ZDecimal(50), TestContra.AH_InvoiceAmount);
			AssertEquals(new ZDecimal(-50), TestContra.AH_Calc_PayAfterContra);
			AssertEquals(new ZDecimal(-50), TestContra.AH_Calc_RecAfterContra);

			TestContra.AH_OSTotal = -100;
			AssertEquals(new ZDecimal(-50), TestContra.AH_InvoiceAmount);
			AssertEquals(new ZDecimal(50), TestContra.AH_Calc_PayAfterContra);
			AssertEquals(new ZDecimal(50), TestContra.AH_Calc_RecAfterContra);
		}

		public void TestReadOnlys()
		{
			Assert("should be read only", TestContra.AH_Calc_PayAfterContraInfo.ReadOnly);
			Assert("should be read only", TestContra.AH_Calc_RecAfterContraInfo.ReadOnly);
			Assert("should be read only", TestContra.AH_Calc_PayBeforeContraInfo.ReadOnly);
			Assert("should be read only", TestContra.AH_Calc_RecBeforeContraInfo.ReadOnly);
			Assert("should be read only", TestContra.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
			Assert("Should be read only", TestContra.AH_TransactionNumInfo.ReadOnly);
			Assert("Should be read only", ((IMatching)TestContra).OSPartialPaymentAmount_ReadOnly);
			Assert("should not be read only", !TestContra.AH_OSTotalInfo.ReadOnly);
		}

		public void TestSetAH_InvoiceAmount()
		{
			TestContra.AH_OSTotal = 100;
			TestContra.ExchangeRate.Rate = 20;
			AssertEquals("Invoice amount should be 5", new ZDecimal(5), TestContra.AH_InvoiceAmount);

			TestContra.AH_OSTotal = -100;
			TestContra.ExchangeRate.Rate = 20;
			AssertEquals("Invoice amount should be -5", new ZDecimal(-5), TestContra.AH_InvoiceAmount);

			TestContra.AH_OSTotal = -120;
			TestContra.ExchangeRate.Rate = 20;
			AssertEquals("Invoice amount should be -6", new ZDecimal(-6), TestContra.AH_InvoiceAmount);

			TestContra.ExchangeRate.Rate = -1;
			Assert("Rate cannot be negative", TestContra.AH_ExchangeRateAmountInfo.HasErrors());

			TestContra.ExchangeRate.Rate = 0;
			Assert("Rate cannot equal 0", TestContra.AH_ExchangeRateAmountInfo.HasErrors());
		}

		public void TestCurrencies()
		{
			AssertNotNull(TestContra.Currencies);
		}

		public void TestOrgDebtors()
		{
			AssertNotNull(TestContra.OrgDebtors);
		}

		public void TestOrgCreditors()
		{
			AssertNotNull(TestContra.OrgCreditors);
		}

		public void TestReverseAndSaveDoesntResetOriginalTransactionNumber()
		{
			TestContra.AH_OSTotal = 500.00m;
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_ExchangeRateAmount = 0.95m;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_Desc = "DESCRIPTION";
			TestContra.AH_OSTotal = 500.00m;

			ZString expectedNumber = Env.NumberFountains.ContraNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ARContraRow aRRow = newFactory.Load<ARContraRow>(TestContra.ARRow.PK);
			APContraRow aPRow = newFactory.Load<APContraRow>(TestContra.APRow.PK);

			Contra loadedContra = Contra.Load(Factory, aRRow, aPRow, LedgerTypes.AccountsReceivable);

			AssertEquals("Should have set transaction number on AP row to next", expectedNumber, loadedContra.APRow.AH_TransactionNum);
			AssertEquals("Should have set transaction number on AR row to next", expectedNumber, loadedContra.ARRow.AH_TransactionNum);

			((IReversing)loadedContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)loadedContra).ReverseTransaction;

			((IReversing)reversingContra).SetCancellationFlag(true);
			((IMatching)reversingContra.ARRow).CurrentMatchGroup.AddNew().AP_AH = reversingContra.ARRow.PK;
			((IMatching)reversingContra.APRow).CurrentMatchGroup.AddNew().AP_AH = reversingContra.APRow.PK;
			((IReversing)loadedContra).SetCancellationFlag(true);
			((IMatching)loadedContra.ARRow).CurrentMatchGroup.AddNew().AP_AH = loadedContra.ARRow.PK;
			((IMatching)loadedContra.APRow).CurrentMatchGroup.AddNew().AP_AH = loadedContra.APRow.PK;

			TestObjectCreator.SetupMatchLinkMatchDate(reversingContra.ARRow);
			TestObjectCreator.SetupMatchLinkMatchDate(reversingContra.APRow);
			TestObjectCreator.SetupMatchLinkMatchDate(loadedContra.ARRow);
			TestObjectCreator.SetupMatchLinkMatchDate(loadedContra.APRow);

			ZString reverseTransactionNumber = Env.NumberFountains.ContraNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);

			newFactory.Save();

			newFactory = new BusinessObjectFactory();

			ARContraRow originalARRow = newFactory.Load<ARContraRow>(loadedContra.ARRow.PK);
			APContraRow originalAPRow = newFactory.Load<APContraRow>(loadedContra.APRow.PK);

			ARContraRow reversingARRow = newFactory.Load<ARContraRow>(reversingContra.ARRow.PK);
			APContraRow reversingAPRow = newFactory.Load<APContraRow>(reversingContra.APRow.PK);

			loadedContra = Contra.Load(Factory, originalARRow, originalAPRow, LedgerTypes.AccountsReceivable);
			reversingContra = Contra.Load(Factory, reversingARRow, reversingAPRow, LedgerTypes.AccountsReceivable);

			AssertEquals("Cancellation Flag on Loaded Contra AR Row", ZBool.True, loadedContra.ARRow.AH_IsCancelled);
			AssertEquals("Cancellation Flag on Loaded Contra AP Row", ZBool.True, loadedContra.APRow.AH_IsCancelled);

			AssertEquals("Should still have same transaction number on original AP Row", expectedNumber, loadedContra.APRow.AH_TransactionNum);
			AssertEquals("Should still have same transaction number on original AR Row", expectedNumber, loadedContra.ARRow.AH_TransactionNum);

			AssertEquals("Reversing AP Row should have next number", reverseTransactionNumber, reversingContra.APRow.AH_TransactionNum);
			AssertEquals("Reversing AR Row should have next number", reverseTransactionNumber, reversingContra.ARRow.AH_TransactionNum);
		}

		public void TestOrganisationBalances()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			// Accounts Payable
			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "CREDITOR01";
			creditor.OH_IsCreditor = true;

			APInvoice testAPInvoice = Factory.New<APInvoice>();
			testAPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			testAPInvoice.AH_OH = creditor.PK;
			testAPInvoice.AH_IsCancelled = false;
			testAPInvoice.AH_TransactionNum = "$1";
			testAPInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			InvoicingLineBase line = testObjectCreator.CreateInvoiceLine(testAPInvoice, 0m, GlbCompany.CurrentCompany.LocalCurrency, 1m);
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 100.0M;
			line.AL_OSExTaxAmount = 100.0M;

			// Accounts Receivable
			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "DEBTOR01";
			debtor.OH_IsDebtor = true;

			ARInvoice testARInvoice = Factory.New<ARInvoice>();
			testARInvoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			//TestARInvoice.AH_LocalExTaxAmount = 100.0M;
			testARInvoice.AH_OH = debtor.PK;
			testARInvoice.AH_IsCancelled = false;
			testARInvoice.AH_TransactionNum = "$1";
			testARInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			line = testObjectCreator.CreateInvoiceLine(testARInvoice, 0m, GlbCompany.CurrentCompany.LocalCurrency, 1m);
			line.AL_AT = ZGuid.Empty;
			line.AL_LocalExTaxAmount = 100.0M;
			line.AL_OSExTaxAmount = 100.0M;

			Factory.Save();

			TestContra.AH_APAccount = creditor.PK;
			TestContra.AH_ARAccount = debtor.PK;

			AssertEquals("Creditors 'Before' Total Amount", 100M, TestContra.AH_Calc_PayBeforeContra);
			AssertEquals("Debtors 'Before' Total Amount", 100M, TestContra.AH_Calc_RecBeforeContra);

			TestContra.AH_OSTotal = 100;

			AssertEquals("Creditors 'After' Total Amount", 0M, TestContra.AH_Calc_PayAfterContra);
			AssertEquals("Debtors 'After' Total Amount", 0M, TestContra.AH_Calc_RecAfterContra);

			TestContra.AH_OSTotal = -100;

			AssertEquals("Creditors 'After' Total Amount", 200M, TestContra.AH_Calc_PayAfterContra);
			AssertEquals("Debtors 'After' Total Amount", 200M, TestContra.AH_Calc_RecAfterContra);
		}

		public void TestGeneratePaymentApprovalItems()
		{
			// This will only be called for manually created contras.
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			IMatching contraAsIMatching = TestContra;
			contraAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);

			AssertEquals("Payment Approval Items", 2, contraAsIMatching.PaymentApprovalItems.Count);
		}

		#region TestUnmatchSystemGeneratedContra

		public void TestUnmatchSystemGeneratedContra()
			=> AssertUnmatchSystemGeneratedContra(isEnableNewOSOutstandingAmountFeature: false);

		public void TestUnmatchSystemGeneratedContra_EnableNewOSOutstandingAmountFeature()
			=> AssertUnmatchSystemGeneratedContra(isEnableNewOSOutstandingAmountFeature: true);

		public void AssertUnmatchSystemGeneratedContra(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContra.APRow.AH_TransactionCreatedByMatching = true;
			TestContra.ARRow.AH_TransactionCreatedByMatching = true;
			TestContra.AH_InvoiceAmount = 80M;
			TestContra.AH_OSTotal = 80M;
			TestContra.APRow.AH_OutstandingAmount = 0M;
			TestContra.ARRow.AH_OutstandingAmount = 0M;

			TransactionMatchLinkGroup matchLinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink aRRowMatch = matchLinks.AddNew();
			aRRowMatch.AP_AH = TestContra.ARRow.PK;
			aRRowMatch.AP_Amount = -80M;

			TransactionMatchLink aPRowMatch = matchLinks.AddNew();
			aPRowMatch.AP_AH = TestContra.APRow.PK;
			aPRowMatch.AP_Amount = 80M;
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinks);

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContra.APRow.MakeOSOutstandingAmountApplicable(0m);
				TestContra.ARRow.MakeOSOutstandingAmountApplicable(0m);

				aRRowMatch.AP_OSAmount = -80M;
				aPRowMatch.AP_OSAmount = 80M;
			}

			Factory.Save();

			(TestContra as IMatching).Unmatch(80m, 80m);

			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			(TestContra as IMatching).ChangeUnmatchDate(expectedPostDate);

			// In practice these will be deleted after all rows have been unmatched
			aRRowMatch.Delete();
			aPRowMatch.Delete();

			Factory.Save();
			// What should happen
			// -2 Reversing ContraRows should be created
			// -There should be 4 matchlinks 
			// Both the original and reverse AR and AP rows AH_IsCancelled flag should be set to false

			//Check AH_IsCancelled "HasChanges" flag
			Assert("Reversing ARRow should be cancelled", TestContra.ARRow.AH_IsCancelled);
			Assert("Reversing APRow should be cancelled", TestContra.APRow.AH_IsCancelled);

			BusinessObjectFactory factoryForReLoading = new BusinessObjectFactory();

			// Test that no system generated contraRows are not cancelled in the unmatching
			var originalContraFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Contra);
			originalContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCreatedByMatching, SQLComparisonOperator.Equal, true);
			originalContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			var rows = factoryForReLoading.Load(typeof(AccTransactionHeader), originalContraFilter) as AccTransactionHeader[];

			Assert("There are Contras", rows.Any());
			foreach (var row in rows)
			{
				AssertEquals("All Rows should have AH_IsCancelled as True", true, row.AH_IsCancelled);
			}

			// Pull out the Reversing ARRow
			ZQuery revARContraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
			revARContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			revARContraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestContra.ARRow.PK);
			originalContraFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			ARContraRow revARContraRow = Factory.LoadTop1(typeof(ARContraRow), revARContraRowFilter) as ARContraRow;
			AssertNotNull("Original ARContraRow should be reversed", revARContraRow);
			AssertEquals("Reversing ARRow should have InvoiceAmount = 80", 80M, revARContraRow.AH_InvoiceAmount);
			AssertEquals("Reversing ARRow should have OSTotal = 80", 80M, revARContraRow.AH_OSTotal);
			AssertEquals("Reversing ARRow should have Outstanding Amount = 0", 0M, revARContraRow.AH_OutstandingAmount);
			AssertEquals("Reversing ARRow should have OS Outstanding Amount = 0", 0M, revARContraRow.AH_OSOutstandingAmount);
			Assert("Reversing ARRow should be cancelled", revARContraRow.AH_IsCancelled);
			AssertEquals("Reversing ARRow should have TransactionBelongsToGroup set to original ARRow", TestContra.ARRow.AH_TransactionBelongsToGroup, revARContraRow.AH_TransactionBelongsToGroup);
			AssertEquals("PostDate should be changed", expectedPostDate, revARContraRow.AH_PostDate);
			AssertEquals("ARRow Outstanding Amount", 0m, TestContra.ARRow.AH_OutstandingAmount);
			AssertEquals("ARRow OS Outstanding Amount", 0m, TestContra.ARRow.AH_OSOutstandingAmount);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, TestContra.ARRow.AH_FullyPaidDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revARContraRow.AH_FullyPaidDate);

			// Pull out the reversing APRow 
			ZQuery revAPContraRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra);
			revAPContraRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			revAPContraRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, TestContra.APRow.PK);

			APContraRow revAPContraRow = Factory.LoadTop1(typeof(APContraRow), revAPContraRowFilter) as APContraRow;
			AssertNotNull("Original APContraRow should be reversed", revAPContraRow);
			AssertEquals("Reversing APRow should have InvoiceAmount = -80", -80M, revAPContraRow.AH_InvoiceAmount);
			AssertEquals("Reversing APRow should have OSTotal = -80", -80M, revAPContraRow.AH_OSTotal);
			AssertEquals("Reversing APRow should have outstanding amt = 0", 0M, revAPContraRow.AH_OutstandingAmount);
			AssertEquals("Reversing APRow should have OS outstanding amt = 0", 0M, revAPContraRow.AH_OSOutstandingAmount);
			Assert("Reversing APRow should be cancelled", revAPContraRow.AH_IsCancelled);
			AssertEquals("PostDate should be changed", expectedPostDate, revAPContraRow.AH_PostDate);
			AssertEquals("APRow Outstanding Amount", 0m, TestContra.APRow.AH_OutstandingAmount);
			AssertEquals("APRow OS Outstanding Amount", 0m, TestContra.APRow.AH_OSOutstandingAmount);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, TestContra.APRow.AH_FullyPaidDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revAPContraRow.AH_FullyPaidDate);

			AssertEquals("Reversing AR and AP ContraRows should have the same transaction number", revARContraRow.AH_TransactionNum, revAPContraRow.AH_TransactionNum);

			// Pull out the Matchlink for Reversing ARRow
			ZQuery revARContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revARContraRow.PK);
			TransactionMatchLink revARContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revARContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing ARRow should be matched", revARContraRowMatch);
			AssertEquals("Reversing ARRow match amount should be 80", 80M, revARContraRowMatch.AP_Amount);
			AssertEquals("Reversing ARRow match OS amount should be 80",
				isEnableNewOSOutstandingAmountFeature ? 80m : 0m,
				revARContraRowMatch.AP_OSAmount
			);
			AssertEquals("MatchDate should as changed post date.", expectedPostDate, revARContraRowMatch.AP_MatchDate);
			ZString commonMatchGroupNum = revARContraRowMatch.AP_MatchGroupNum;
			ZDateTime commonMatchDate = revARContraRowMatch.AP_MatchDate;

			// Pull out the matchlink for Original ARRow
			ZQuery aRContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestContra.ARRow.PK);
			TransactionMatchLink aRContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aRContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original ARRow should be matched", aRContraRowMatch);
			AssertEquals("Original ARRow match amount should be -80", -80M, aRContraRowMatch.AP_Amount);
			AssertEquals("Original ARRow match OS amount should be -80",
				isEnableNewOSOutstandingAmountFeature ? -80m : 0m,
				aRContraRowMatch.AP_OSAmount
			);
			AssertEquals("MatchGroupNum should be the same for all matchlinks", commonMatchGroupNum, aRContraRowMatch.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same for all matchlinks", commonMatchDate, aRContraRowMatch.AP_MatchDate);

			// Pull out the Matchlink for Reversing APRow
			ZQuery revAPContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revAPContraRow.PK);
			TransactionMatchLink revAPContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), revAPContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Reversing APRow should be matched", revAPContraRowMatch);
			AssertEquals("Match amount of Reversing APContraRow matchlink should be -80", -80M, revAPContraRowMatch.AP_Amount);
			AssertEquals("Reversing APContraRow match OS amount should be -80",
				isEnableNewOSOutstandingAmountFeature ? -80m : 0m,
				revAPContraRowMatch.AP_OSAmount
			);
			AssertEquals("MatchGroupNum should be the same for all matchlinks", commonMatchGroupNum, revAPContraRowMatch.AP_MatchGroupNum);
			AssertEquals("MatchDate should be the same for all matchlinks", commonMatchDate, revAPContraRowMatch.AP_MatchDate);

			// Pull out the Matchlink for Original APRow
			ZQuery aPContraRowMatchFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestContra.APRow.PK);
			TransactionMatchLink aPContraRowMatch = Factory.LoadTop1(typeof(TransactionMatchLink), aPContraRowMatchFilter) as TransactionMatchLink;
			AssertNotNull("Original APRow should be matched", aPContraRowMatch);
			AssertEquals("Match amount of Original APContraRow matchlink should be 80", 80M, aPContraRowMatch.AP_Amount);
			AssertEquals("Original APContraRow match OS amount should be 80",
				isEnableNewOSOutstandingAmountFeature ? 80m : 0m,
				aPContraRowMatch.AP_OSAmount
			);
		}

		#endregion

		#region Test Validate Methods

		public void TestValidateAH_Desc()
		{
			TestContra.AH_Desc = "";
			TestContra.ValidateAH_Desc();
			Assert("expect error", TestContra.AH_DescInfo.HasErrors());
		}

		public void TestValidateAH_PostDate()
		{
			PeriodValidationProvider validationProvider;
			AccountingPeriodTestHelper periodHelper;

			periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();
			validationProvider = new PeriodValidationProvider(Factory);

			TestContra.AH_PostDate = ZDateTime.Empty;
			TestContra.ValidateAH_PostDate();
			Assert("expect error", TestContra.AH_PostDateInfo.HasErrors());

			TestContra.AH_PostDate = periodHelper.PreviousGLClosedPeriod.AM_StartDate.AddDays(-30);
			TestContra.ValidateAH_PostDate();
			Assert("Should be error on post date", TestContra.AH_PostDateInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestContra.AH_PostDate = periodHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			TestContra.ValidateAH_PostDate();
			Assert("Should be no error on Post Date", !TestContra.AH_PostDateInfo.HasErrors());

			TestContra.AH_PostDate = periodHelper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
			TestContra.ValidateAH_PostDate();
			Assert("Should be error on Post Date", TestContra.AH_PostDateInfo.HasErrors());

			TestContra.AH_PostDate = periodHelper.PreviousOpenPeriod.AM_StartDate.AddDays(5);
			TestContra.ValidateAH_PostDate();
			Assert("Should be no error on Post Date", !TestContra.AH_PostDateInfo.HasErrors());
		}

		[TestDate(2006, 11, 15)]
		public void TestValidateAH_PostDate_Reversing()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime originalPostDate = ZDateTime.Today.AddDays(-5);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(originalPostDate);
			if (accPeriod == null)
			{
				accPeriod = Factory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(originalPostDate);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(originalPostDate.Year, originalPostDate.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			Factory.Save();

			TestContra.AH_PostDate = originalPostDate;

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(TestContra);
			reversing.Reverse();

			Contra reverseContra = reversing.ReverseTransaction as Contra;
			reverseContra.ValidateAH_PostDate();
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseContra.AH_PostDateInfo.HasErrors());

			reverseContra.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("AH_PostDateInfo.HasErrors()", true, reverseContra.AH_PostDateInfo.HasErrors());
			string expectedError = "Reversing post date cannot be before the original post date of '" + TestContra.AH_PostDate.ToShortDateString() + "'.";
			AssertEquals("Contains(ExpectedError)", true, reverseContra.AH_PostDateInfo.GetErrors().Contains(expectedError));

			reverseContra.AH_PostDate = originalPostDate;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseContra.AH_PostDateInfo.HasErrors());

			reverseContra.AH_PostDate = originalPostDate.AddDays(1);
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseContra.AH_PostDateInfo.HasErrors());

			reverseContra.APRow.ReadOnly = true;
			reverseContra.ARRow.ReadOnly = true;
			Assert("ARRow validation should be a TransactionReversalValidation", reverseContra.ARRow.Validation is TransactionReversalValidation);
			Assert("APRow validation should be a TransactionReversalValidation", reverseContra.APRow.Validation is TransactionReversalValidation);

			reverseContra.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseContra.AH_PostDateInfo.HasErrors());
			reverseContra.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("AH_PostDateInfo should not have any error because the context is not set", false, reverseContra.AH_PostDateInfo.HasErrors());

			Factory.SetContext(BusinessContext.ReverseDateForm);

			expectedError = "The post date cannot be in the future";
			reverseContra.AH_PostDate = originalPostDate.AddDays(6);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseContra.AH_PostDateInfo.GetErrors().Contains(expectedError));

			expectedError = "This date does not fall into a valid accounting period’s date range.\r\nPlease go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.";
			reverseContra.AH_PostDate = originalPostDate.AddYears(2);
			AssertEquals("We should get an error because the context is set : " + expectedError, true, reverseContra.AH_PostDateInfo.GetErrors().Contains(expectedError));
		}

		public void TestValidateAH_InvoiceDate()
		{
			TestContra.AH_InvoiceDate = ZDateTime.Empty;
			TestContra.ValidateAH_InvoiceDate();
			Assert("expect error", TestContra.AH_InvoiceDateInfo.HasErrors());
		}

		public void TestValidateAH_ARAccount()
		{
			TestContra.AH_ARAccount = ZGuid.Empty;
			TestContra.ValidateAH_ARAccount();
			AssertHasErrorContaining(TestContra.AH_ARAccountInfo, "Please enter an Account");

			TestContra.AH_ARAccount = ZGuid.NewZGuid();
			TestContra.ValidateAH_ARAccount();
			AssertHasErrorContaining(TestContra.AH_ARAccountInfo, "Enter a valid Account");
		}

		public void TestValidateAH_APAccount()
		{
			TestContra.AH_APAccount = ZGuid.Empty;
			TestContra.ValidateAH_APAccount();
			AssertHasErrorContaining(TestContra.AH_APAccountInfo, "Please enter an Account");

			TestContra.AH_APAccount = ZGuid.NewZGuid();
			TestContra.ValidateAH_APAccount();
			AssertHasErrorContaining(TestContra.AH_APAccountInfo, "Enter a valid Account");
		}

		public void TestValidateAccountConsolidationCategoryClasses_Receivables()
		{
			AssertValidateAccountConsolidationCategoryClasses(true);
		}

		public void TestValidateAccountConsolidationCategoryClasses_Payables()
		{
			AssertValidateAccountConsolidationCategoryClasses(false);
		}

		void AssertValidateAccountConsolidationCategoryClasses(bool isARController)
		{
			Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
			Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;

			var contra = Contra.New(Factory, isARController ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable);
			AssertEquals(ARAccount.MiscServ.ConsolidatedAccountingCategoryClass, APAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertResults(true);
			AssertEquals(string.Empty, APAccount.MiscServ.ConsolidatedAccountingCategoryClass);

			SetConsolidatedAccountingCategory(APAccount, AccountsCategory.Unrelated);
			AssertEquals("TPY", APAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertNotEquals(ARAccount.MiscServ.ConsolidatedAccountingCategoryClass, APAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertResults(false);

			SetConsolidatedAccountingCategory(ARAccount, AccountsCategory.Unrelated);
			AssertEquals("TPY", ARAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertEquals(ARAccount.MiscServ.ConsolidatedAccountingCategoryClass, APAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertResults(true);

			SetConsolidatedAccountingCategory(ARAccount, AccountsCategory.WhollyOwned);
			AssertEquals("INT", ARAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertNotEquals(ARAccount.MiscServ.ConsolidatedAccountingCategoryClass, APAccount.MiscServ.ConsolidatedAccountingCategoryClass);
			AssertResults(false);

			if (isARController)
			{
				Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
				Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = true;
				AssertResults(false);
				Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = true;
				Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
				AssertResults(true);
			}
			else
			{
				Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = true;
				Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
				AssertResults(false);
				Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = false;
				Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed = true;
				AssertResults(true);
			}

			void SetConsolidatedAccountingCategory(OrgHeader account, string value)
			{
				if (isARController)
				{
					account.MiscServ.OM_ARConsolidatedAccountingCategory = value;
				}
				else
				{
					account.MiscServ.OM_APConsolidatedAccountingCategory = value;
				}
			}

			void AssertResults(bool isValid)
			{
				contra.AH_APAccount = APAccount.PK;
				contra.AH_ARAccount = ARAccount.PK;
				AssertResult(contra.AH_ARAccountInfo, isValid);
				AssertResult(contra.AH_APAccountInfo, isValid);
			}

			void AssertResult(ZPropertyInfo accountInfo, bool isValid)
			{
				if (isValid)
				{
					Assert(!accountInfo.HasErrors());
				}
				else
				{
					var securityPath = isARController ? "Manage -> Receivables -> Receivables Transactions -> New Transactions -> Allow Contra Transactions with different account consolidation category class" :
						"Manage -> Payables -> Payables Transactions -> New Transactions -> Allow Contra Transactions with different account consolidation category class";

					var expectedError = string.Format(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", securityPath);
					AssertHasErrorContaining(accountInfo, expectedError);
				}
			}
		}

		public void TestValidateAH_ExchangeRateAmount()
		{
			TestContra.AH_ExchangeRateAmount = 9;
			Assert("should not have errors", !TestContra.AH_ExchangeRateAmountInfo.HasErrors());
			TestContra.AH_ExchangeRateAmount = 0;
			Assert("should have errors since equals 0", TestContra.AH_ExchangeRateAmountInfo.HasErrors());
		}

		public void TestValidateAH_ExchangeRateCurrency()
		{
			TestContra.ExchangeRate.Currency = "USD";
			AssertNoErrors("Should not be any errors on currency", TestContra.AH_ExchangeRateCurrencyCodeInfo);

			TestContra.ExchangeRate.Currency = "XXX";
			AssertHasErrors("Should error on currency", TestContra.AH_ExchangeRateCurrencyCodeInfo);

			TestContra.ExchangeRate.Currency = "USD";
			AssertNoErrors("Should not be any errors on currency", TestContra.AH_ExchangeRateCurrencyCodeInfo);
		}

		#endregion

		#region Test Properties

		public void TestAH_Desc()
		{
			TestContra.AH_Desc = "###";
			Assert("expect get equals set", TestContra.AH_Desc == "###");
		}

		public void TestAH_PostDate()
		{
			TestContra.AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDate;
			Assert("expect get equals set", TestContra.AH_PostDate.ToDateTime() == Env.Time.CurrentLocalDate);
		}

		public void TestAH_InvoiceAmount()
		{
			TestContra.AH_InvoiceAmount = 45;
			Assert("expect get equals set", TestContra.AH_InvoiceAmount == 45);
		}

		public void TestAH_ARAccount()
		{
			TestContra.AH_ARAccount = Factory.LoadTop1(typeof(OrgDebtorGroup), new ZQuery()).PK;
			Assert("expect get equals set", TestContra.AH_ARAccount == Factory.LoadTop1(typeof(OrgDebtorGroup), new ZQuery()).PK);
		}

		public void TestAH_APAccount()
		{
			TestContra.AH_APAccount = Factory.LoadTop1(typeof(OrgCreditorGroup), new ZQuery()).PK;
			Assert("expect get equals set", TestContra.AH_APAccount == Factory.LoadTop1(typeof(OrgCreditorGroup), new ZQuery()).PK);
		}

		public void TestAH_Calc_LocalRX()
		{
			Assert("should equal the local currency", TestContra.AH_Calc_LocalRX == GlbCompany.CurrentCompany.LocalCurrency.PK);
		}

		public void TestLocalCurrencySubUnitRatio()
		{
			AssertEquals("number of decimal places should equal the current company's currency",
				GlbCompany.CurrentCompany.LocalCurrency.Decimals, TestContra.LocalCurrencySubUnitRatio);
		}

		public void TestAH_ExchangeRateCurrency()
		{
			TestContra = Contra.CreateContra_ForTestOnly(Factory);
			Assert("The Exchange Rate Currency should be empty", TestContra.AH_ExchangeRateCurrencyCode.IsEmpty);
		}

		public void TestInvoiceBatchNumber()
		{
			Assert("Invoice Batch Number should be empty", ((IMatching)TestContra).InvoiceBatchNumber.IsEmpty);
		}

		public void TestInvoiceTransactionReference()
		{
			Assert("Invoice Transaction Reference should be empty", ((IMatching)TestContra).InvoiceTransactionReference.IsEmpty);
		}

		public void TestControllerLedger()
		{
			TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals("ControllerLedger should be set to AR", LedgerTypes.AccountsReceivable, TestContra.ControllerLedger);
			TestContra = Contra.New(Factory);
			Assert("ControllerLedger should be empty", TestContra.ControllerLedger.IsEmpty);
		}

		#endregion

		public void TestUserAllowedToBackPost()
		{
			bool isReceivablesPostAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool isPayablesPostAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				//For AP
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				Assert("AH_PostDateInfo should not be readonly", !TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);

				//For AR
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				Assert("AH_PostDateInfo should not be readonly", !TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isReceivablesPostAllowed;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isPayablesPostAllowed;
			}
		}

		public void TestGetPK()
		{
			TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			AssertEquals(TestContra.APRow.PK, TestContra.PK);

			TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			AssertEquals(TestContra.ARRow.PK, TestContra.PK);
		}

		public void TestSetControllerLedgerWhenLoadContra()
		{
			var loadedContra1 = Contra.Load(Factory, TestContra.ARRow, TestContra.APRow, LedgerTypes.AccountsReceivable);
			AssertEquals(LedgerTypes.AccountsReceivable, loadedContra1.ControllerLedger);

			var loadedContra2 = Contra.Load(Factory, TestContra.ARRow, TestContra.APRow, LedgerTypes.AccountsPayable);
			AssertEquals(LedgerTypes.AccountsPayable, loadedContra2.ControllerLedger);
		}

		public void TestAH_PostDateInfo_ReadOnlyStatusWhenTheLedgerIsNotSpeceified()
		{
			bool isReceivablesPostAllowed = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
			bool isPayablesPostAllowed = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;

			try
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				TestContra = Contra.New(Factory);
				Assert("AH_PostDateInfo should not be readonly", !TestContra.AH_PostDateInfo.ReadOnly);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				TestContra = Contra.New(Factory);
				Assert("AH_PostDateInfo should be readonly", TestContra.AH_PostDateInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isReceivablesPostAllowed;
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = isPayablesPostAllowed;
			}
		}

		public void TestActiveOrgValidation()
		{
			using (new TestObjectCreator.MakeOrgTemporarilyInactive(TestObjectCreator.InActiveOrg))
			{
				ZGuid activeOrgPK = TestObjectCreator.ActiveOrg.PK;
				ZGuid inactiveOrgPK = TestObjectCreator.InActiveOrg.PK;

				AssertAccountActiveValidationWorks(TestContra.AH_APAccountInfo, activeOrgPK, inactiveOrgPK);
				AssertAccountActiveValidationWorks(TestContra.AH_ARAccountInfo, activeOrgPK, inactiveOrgPK);
			}
		}

		public void TestDoNotValidateBranchDepartmentCombinationWhenReversing()
		{
			var contra = TestContra;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(currentBranch, new GlbDepartment[] { department });

			contra.GenerateReverseTransaction(true);
			var reversingContra = (Contra)((IReversing)contra).ReverseTransaction;

			AssertEquals(contra.ARRow.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(contra.ARRow.AH_GE, Env.CurrentDepartment.PK);

			AssertEquals(contra.APRow.AH_GB, Env.CurrentBranch.PK);
			AssertEquals(contra.APRow.AH_GE, Env.CurrentDepartment.PK);

			contra.RunPreSaveValidation();
			contra.ARRow.RunPreSaveValidation();
			contra.APRow.RunPreSaveValidation();

			AssertNoErrors(contra.ARRow.AH_GEInfo);
			AssertNoErrors(contra.APRow.AH_GEInfo);
		}

		void AssertAccountActiveValidationWorks(ZPropertyInfo fromOrToAccountInfo, ZGuid activeOrg, ZGuid inactiveOrg)
		{
			fromOrToAccountInfo.Value = activeOrg;
			AssertNoErrors(fromOrToAccountInfo);

			fromOrToAccountInfo.Value = inactiveOrg;
			AssertHasError(fromOrToAccountInfo, "This Account is inactive - it may not be used.");
		}

		[TestedType(typeof(Contra))]
		public class ContraMatchingTest : Base.Transaction.Testing.IMatchingTestCase
		{
			protected override bool IsShownOnMatchingForm
			{
				get { return false; }
			}

			protected override IMatching GetNewIMatching(ZGuid branchPK, ZGuid organisationPK, ZString currencyCode)
			{
				Contra bizObj = Contra.CreateContra_ForTestOnly(Factory);
				bizObj.InitialiseNew_ForTestOnly();
				bizObj.APRow.AH_GB = branchPK;
				bizObj.APRow.AH_OH = organisationPK;
				bizObj.APRow.AH_RX_NKTransactionCurrency = currencyCode;
				return bizObj;
			}
		}

		public void TestContraReversingValidatedForConcurrentReversal()
		{
			var handleError = TestContra as IHandleDeleteError;

			AssertNotNull("Implements IHandleError", handleError);
			Assert("RollbackAfterDeleteError false on new Contra", !handleError.RollbackAfterDeleteError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			Factory.Save();

			Assert("RollbackAfterDeleteError is true by default", handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError is false by default", !handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);

			TestContra.ARRow.IsCancelled = true;

			Assert("RollbackAfterDeleteError on saved and cancelled Contra is false to prevent rollback to non-cancelled state", !handleError.RollbackAfterDeleteError);
			Assert("DisableFormOnDeleteConcurrencyError on saved and cancelled Contra is true", handleError.DisableFormOnDeleteConcurrencyError);
			Assert("RebindAfterDeleteError is always false", !handleError.RebindAfterDeleteError);
		}

		public void TestTablePrefix()
		{
			AssertEquals("Table Prefix should be AH.", "AH", TestContra.TablePrefix);
		}

		public void TestTableName()
		{
			AssertEquals("Table Name should be AccTransactionHeader.", "AccTransactionHeader", TestContra.TableName);
		}

		public void TestPKSchemaColumn()
		{
			AssertEquals("PKSchemaColumn should be AccTransactionHeaderSchema.PK.", AccTransactionHeaderSchema.PK, TestContra.PKSchemaColumn);
		}

		#region IReversingTest Members

		public void TestIsReversedImplementation()
		{
			Assert("Should not be reversed", !((IReversing)TestContra).IsReversed);
			TestContra.ARRow.AH_IsCancelled = true;
			TestContra.APRow.AH_IsCancelled = true;
			Assert("Should be reversed", ((IReversing)TestContra).IsReversed);
		}

		public void TestIsMatchedImplementation()
		{
			TestContra.AH_OSTotal = 200.00m;

			Assert("Contra should not be matched", !((IMatching)TestContra).IsMatched);

			TestContra.APRow.AH_LocalOutstandingAmount = 150.00m; // "Match" AP row, but AR row remains unmatched
			Assert("Contra should be matched", ((IMatching)TestContra).IsMatched);

			TestContra.APRow.AH_LocalOutstandingAmount = 200.00m; // "Un Match" AP row again
			Assert("Contra should not be matched", !((IMatching)TestContra).IsMatched);

			TestContra.ARRow.AH_LocalOutstandingAmount = 10.00m; // "Match" AR row, but AP row remains unmatched
			Assert("Contra should be matched", ((IMatching)TestContra).IsMatched);

			TestContra.APRow.AH_LocalOutstandingAmount = 200.00m; // "Match" AP row as well - both rows matched
			Assert("Contra should be matched", ((IMatching)TestContra).IsMatched);
		}

		public void TestFullyPay()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestFullyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertFullyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertFullyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContra.ARRow.AH_OutstandingAmount = 500.00m;
			TestContra.ARRow.AH_OSExTaxAmount = 500.00m;

			TestContra.APRow.AH_OutstandingAmount = 500.00m;
			TestContra.APRow.AH_OSExTaxAmount = 500.00m;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContra.ARRow.MakeOSOutstandingAmountApplicable(500m);

				AssertEquals("PreCondition - ARRow AH_IsOSOutstandingAmountApplicable", true, TestContra.ARRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - ARRow AH_OSOutstandingAmount", 500m, TestContra.ARRow.AH_OSOutstandingAmount);

				TestContra.APRow.MakeOSOutstandingAmountApplicable(500m);

				AssertEquals("PreCondition - APRow AH_IsOSOutstandingAmountApplicable", true, TestContra.APRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - APRow AH_OSOutstandingAmount", 500m, TestContra.APRow.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now;
			((IMatching)TestContra).FullyPay(expectedFullyPaidDate);

			AssertEquals("AR Row fully paid date", expectedFullyPaidDate, TestContra.ARRow.AH_FullyPaidDate);
			AssertEquals("AP Row fully paid date", expectedFullyPaidDate, TestContra.APRow.AH_FullyPaidDate);

			AssertEquals("AR Row Outstanding Amount", 0m, TestContra.ARRow.AH_OutstandingAmount);
			AssertEquals("AP Row Outstanding Amount", 0m, TestContra.APRow.AH_OutstandingAmount);

			AssertEquals("APRow AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContra.APRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("ARRow AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, TestContra.ARRow.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("APRow AH_OSOutstandingAmount", 0m, TestContra.APRow.AH_OSOutstandingAmount);
			AssertEquals("ARRow AH_OSOutstandingAmount", 0m, TestContra.ARRow.AH_OSOutstandingAmount);
		}

		public void TestPartiallyPay()
		{
			var exp = AssertExceptionThrown<NotSupportedException>(() => ((IMatching)TestContra).PartiallyPay());

			AssertEquals("You cannot partially pay a Contra.", exp.Message);
		}

		public void TestSetCancellationFlag()
		{
			((IReversing)TestContra).SetCancellationFlag(true);
			AssertEquals("Cancelled flag on AP", ZBool.True, TestContra.APRow.AH_IsCancelled);
			AssertEquals("Cancelled flag on AR", ZBool.True, TestContra.ARRow.AH_IsCancelled);

			((IReversing)TestContra).SetCancellationFlag(false);
			AssertEquals("Cancelled flag on AP", ZBool.False, TestContra.APRow.AH_IsCancelled);
			AssertEquals("Cancelled flag on AR", ZBool.False, TestContra.ARRow.AH_IsCancelled);
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestGenerateMatchLinks_EnableNewOSOutstandingAmountFeature()
		{
			AssertGenerateMatchLinks(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertGenerateMatchLinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			TestContra.AH_OSTotal = 100m;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				TestContra.ARRow.MakeOSOutstandingAmountApplicable(TestContra.ARRow.AH_OutstandingAmount);

				AssertEquals("PreCondition - ARRow AH_IsOSOutstandingAmountApplicable", true, TestContra.ARRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - ARRow AH_OSOutstandingAmount", 100m, TestContra.ARRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);

				TestContra.APRow.MakeOSOutstandingAmountApplicable(TestContra.APRow.AH_OutstandingAmount);

				AssertEquals("PreCondition - APRow AH_IsOSOutstandingAmountApplicable", true, TestContra.APRow.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - APRow AH_OSOutstandingAmount", 100m, TestContra.APRow.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			AssertNotNull("Matchlink collection should not be null - should return empty collection", ((IMatching)TestContra).CurrentMatchGroup);

			((IMatching)TestContra).FullyPay(expectedFullyPaidDate);
			TestContra.GenerateMatchLinks();

			AssertNotNull("Matchlink collection should not be null", ((IMatching)TestContra).CurrentMatchGroup);

			TransactionMatchLinkCollection matchLinks = ((IMatching)TestContra).CurrentMatchGroup;

			AssertEquals("Should have one matchlink record generated", 2, matchLinks.Count);

			ZQuery matchlinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestContra.APRow.PK);
			TransactionMatchLink[] aPLinks = (TransactionMatchLink[])matchLinks.Find(matchlinkFilter);
			AssertEquals("Should only be one AP Matchlink", 1, aPLinks.Length);
			TransactionMatchLink aPLink = aPLinks[0];
			AssertEquals("Match Amount should be same as outstanding amount before fully paying",
				100.00m, aPLink.AP_Amount);
			AssertEquals("APRow AP_OSAmount",
				isEnableNewOSOutstandingAmountFeature ? 100.00m : 0m, aPLink.AP_OSAmount);

			matchlinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestContra.ARRow.PK);
			TransactionMatchLink[] aRLinks = (TransactionMatchLink[])matchLinks.Find(matchlinkFilter);
			AssertEquals("Should only be one AP Matchlink", 1, aRLinks.Length);
			TransactionMatchLink aRLink = aRLinks[0];
			AssertEquals("Match Amount should be same as outstanding amount before fully paying",
				-100.00m, aRLink.AP_Amount);
			AssertEquals("ARRow AP_OSAmount",
				isEnableNewOSOutstandingAmountFeature ? -100.00m : 0m, aRLink.AP_OSAmount);
		}

		public void TestReverseTransaction()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_ExchangeRateAmount = 0.95m;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_Desc = "DESCRIPTION";
			TestContra.AH_OSTotal = 500.00m;

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

			AssertEquals("AP Account on reversing contra", APAccount.PK, reversingContra.AH_APAccount);
			AssertEquals("AR Account on reversing contra", ARAccount.PK, reversingContra.AH_ARAccount);
			AssertEquals("Currency on reversing contra", ForeignCurrency.RX_Code, reversingContra.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate on reversing contra", 0.95m, reversingContra.AH_ExchangeRateAmount);
			AssertEquals("Post date should be today", ZDateTime.Now.Date, reversingContra.AH_PostDate.Date);
			AssertEquals("Invoice date should be today", ZDateTime.Now.Date, reversingContra.AH_InvoiceDate.Date);

			AssertEquals("OS Total should be negative of original contra", -500.00m, reversingContra.AH_OSTotal);
			AssertEquals("Invoice Amount should be negative of original contra", -526.32m, reversingContra.AH_InvoiceAmount);

			AssertEquals("Outstanding amount on APRow should be -526.32", -526.32m, reversingContra.APRow.AH_OutstandingAmount);
			AssertEquals("Outstanding amount on ARRow should be 526.32", 526.32m, reversingContra.ARRow.AH_OutstandingAmount);

			AssertEquals("Original transaction number should be set", TestContra.AH_TransactionNum, reversingContra.OriginalTransactionNumber);
			AssertEquals("Original transaction type should be set", TransactionTypes.Contra, reversingContra.OriginalTransactionType);
		}

		public void TestReverseDoesntRecaclucateLocalAmountForARRow()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_OSTotal = 51.72m;
			TestContra.AH_InvoiceAmount = 102.96m;
			AssertEquals("ExchangeRate should have been set to 0.502331", 0.502331m, TestContra.ExchangeRate.Rate);
			TestContra.AH_Desc = "DESCRIPTION";

			Factory.Save();

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;
			AssertEquals(reversingContra.ARRow.AH_InvoiceAmount, 102.96m);
			AssertEquals(reversingContra.APRow.AH_InvoiceAmount, -102.96m);
			AssertEquals(reversingContra.AH_InvoiceAmount, -102.96m);
			AssertEquals("ExchangeRate should have been set to 0.502331", 0.502331m, reversingContra.ExchangeRate.Rate);
		}

		public void TestReverseAndSaveDoesntCauseAmtRecalculation()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_OSTotal = 10773.70m;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_InvoiceAmount = 12189.36m;
			TestContra.AH_Desc = "DESCRIPTION";
			AssertEquals(0.883861M, TestContra.AH_ExchangeRateAmount);

			Factory.Save();

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

			AssertEquals("Currency on reversing contra", ForeignCurrency.RX_Code, reversingContra.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate on reversing contra", 0.883861m, reversingContra.AH_ExchangeRateAmount);
			AssertEquals("OS Total should be negative of original contra", -10773.70m, reversingContra.AH_OSTotal);
			AssertEquals("Invoice Amount should be negative of original contra", -12189.36m, reversingContra.AH_InvoiceAmount);
		}

		public void TestReverseAndSaveDoesntCauseAmtRecalculationForReciprocalCompany()
		{
			var newFactory = new BusinessObjectFactory();
			GlbCompany newCompany = newFactory.New<GlbCompany>();
			newCompany.GC_Code = "TST";
			newCompany.GC_IsReciprocal = true;
			GlbBranch newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "TST";
			newFactory.Save();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: IsReciprocal has been set properly for Env.CurrentCompany", true, Env.CurrentCompany.IsReciprocal);
				AssertEquals("Precondition: IsReciprocal has been set properly for GlbCompany.CurrentCompany", true, GlbCompany.CurrentCompany.GC_IsReciprocal);

				TestContra.AH_APAccount = testObjectCreator.ABIGAS.PK;
				TestContra.AH_ARAccount = testObjectCreator.AALSHI.PK;
				TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
				TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
				TestContra.AH_OSTotal = 10773.70m;
				TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
				TestContra.AH_InvoiceAmount = 12189.36m;
				TestContra.AH_Desc = "DESCRIPTION";
				AssertEquals("Precondition", 1.1314M, TestContra.AH_ExchangeRateAmount);

				Factory.Save();

				((IReversing)TestContra).GenerateReverseTransaction(true);
				Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

				AssertEquals("Currency on reversing contra", ForeignCurrency.RX_Code, reversingContra.AH_RX_NKTransactionCurrency);
				AssertEquals("Exchange Rate on reversing contra", 1.1314M, reversingContra.AH_ExchangeRateAmount);
				AssertEquals("OS Total should be negative of original contra", -10773.70m, reversingContra.AH_OSTotal);
				AssertEquals("Invoice Amount should be negative of original contra", -12189.36m, reversingContra.AH_InvoiceAmount);
			}
		}

		public void TestSetTransactionBelongsToGroupField()
		{
			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

			ZGuid groupingGuid = TestContra.ARRow.AH_TransactionBelongsToGroup; // This is set initially and set to both contra row objects

			((IReversing)TestContra).SetTransactionBelongsToGroupField(groupingGuid);
			AssertEquals("Both rows should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, TestContra.ARRow.AH_TransactionBelongsToGroup);
			AssertEquals("Both rows should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, TestContra.APRow.AH_TransactionBelongsToGroup);

			AssertEquals("Both rows in reversing contra should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, reversingContra.ARRow.AH_TransactionBelongsToGroup);
			AssertEquals("Both rows in reversing contra should have the transaction belongs to group field set to the grouping guid passed",
				groupingGuid, reversingContra.APRow.AH_TransactionBelongsToGroup);
		}

		public void TestAH_NumberOfSupportingDocuments()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_ExchangeRateAmount = 0.95m;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_Desc = "DESCRIPTION";
			TestContra.AH_OSTotal = 500.00m;
			TestContra.AH_NumberOfSupportingDocuments = 2;

			Factory.Save();

			ZString transactionNumber = TestContra.AH_TransactionNum;

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;
			AssertEquals("Number Of Supporting Documents", 1, reversingContra.AH_NumberOfSupportingDocuments.ToZInt());
		}
		public void TestSetDescription()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_ExchangeRateAmount = 0.95m;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_Desc = "DESCRIPTION";
			TestContra.AH_OSTotal = 500.00m;

			Factory.Save();

			ZString transactionNumber = TestContra.AH_TransactionNum;

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

			ZString descriptionToSet = string.Format("Reversal Related to {0}", transactionNumber);
			((IReversing)reversingContra).SetDescription(descriptionToSet);

			AssertEquals("Description on reversing transaction", descriptionToSet, reversingContra.AH_Desc);
		}

		public void TestReversingReason()
		{
			TestContra.AH_APAccount = APAccount.PK;
			TestContra.AH_ARAccount = ARAccount.PK;
			TestContra.AH_ExchangeRateCurrencyCode = ForeignCurrency.RX_Code;
			TestContra.AH_ExchangeRateAmount = 0.95m;
			TestContra.AH_PostDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_InvoiceDate = ZDateTime.Now.AddDays(-10);
			TestContra.AH_Desc = "DESCRIPTION";
			TestContra.AH_OSTotal = 500.00m;

			Factory.Save();

			ZString transactionNumber = TestContra.AH_TransactionNum;

			((IReversing)TestContra).GenerateReverseTransaction(true);
			Contra reversingContra = (Contra)((IReversing)TestContra).ReverseTransaction;

			ZString descriptionToSet = string.Format("Reversal Related to {0}", transactionNumber);

			((IReversing)reversingContra).SetDescription(descriptionToSet);

			ZString reversingReason = "REASON";
			((IReversing)reversingContra).ReversingReason = reversingReason;

			AssertEquals("Description after setting reversing reason", descriptionToSet + " " + reversingReason, reversingContra.AH_Desc);

			AssertEquals("Reversing Reason should return same as value set", reversingReason, ((IReversing)reversingContra).ReversingReason);
		}

		[ExpectNoExceptions]
		public void TestReversingReasonExceedsMaxLength()
		{
			string reverseReason = new string('d', AccTransactionHeaderSchema.AH_Desc.MaxLength + 1);
			TestContra.ReversingReason = reverseReason;
		}

		#endregion

		#region ITransaction Implementation Testing

		public void TestCurrencyImplementation()
		{
			TestContra.ExchangeRate.Currency = ForeignCurrency.RX_Code;
			AssertEquals("should return foreign", ForeignCurrency.RX_Code, ((ITransaction)TestContra).CurrencyCode);

			TestContra.ExchangeRate.Currency = ZString.Empty;
			AssertEquals("Should return empty code", ZString.Empty, ((ITransaction)TestContra).CurrencyCode);

			TestContra.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("should return local currency code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, ((ITransaction)TestContra).CurrencyCode);
		}

		public void TestLedgerImplementation()
		{
			AssertEquals("Should be emtpy string - Contra is always of two ledgers", ZString.Empty, ((ITransaction)TestContra).Ledger);
		}

		public void TestOverseasTotalAmountImplementation()
		{
			TestContra.AH_OSTotal = 350.00m;
			AssertEquals("Overseas total", 350.00m, ((ITransaction)TestContra).OverseasTotalAmount);
		}

		public void TestPostDateImplementation()
		{
			ZDateTime postDate = ZDateTime.Now.AddDays(-2);
			TestContra.AH_PostDate = postDate;
			AssertEquals("Post Date", postDate, ((ITransaction)TestContra).PostDate);
		}

		public void TestTransactionDateImplementation()
		{
			ZDateTime transactionDate = ZDateTime.Now.AddDays(20);
			TestContra.AH_InvoiceDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)TestContra).TransactionDate);

			transactionDate = ZDateTime.Now.AddDays(10);
			((ITransaction)TestContra).TransactionDate = transactionDate;
			AssertEquals("Transaction Date", transactionDate, ((ITransaction)TestContra).TransactionDate);
		}

		public void TestTransactionTypeImplementation()
		{
			AssertEquals("Transaction Type", TransactionTypes.Contra, ((ITransaction)TestContra).TransactionType);
		}

		public void TestOrganisationImplementation()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			((ITransaction)TestContra).Organization = newOrganisation.PK;
			AssertEquals("ARAccount should be defaulted by NewOrganisation", newOrganisation.PK, TestContra.AH_ARAccount);
			AssertEquals("APAccount should not be defaulted", ZGuid.Empty, TestContra.AH_APAccount);
			TestContra = Contra.New(Factory, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			((ITransaction)TestContra).Organization = newOrganisation.PK;
			AssertEquals("APAccount should be defaulted by NewOrganisation", newOrganisation.PK, TestContra.AH_APAccount);
			AssertEquals("ARAccount should not be defaulted", ZGuid.Empty, TestContra.AH_ARAccount);
		}

		#region ReversalStatusCode

		public void TestReversalStatusCode_ShouldBeEmpty()
		{
			AssertEquals(nameof(TestContra.ReversalStatusCode), ZString.Empty, TestContra.ReversalStatusCode);
		}

		public void TestReversalStatusCode_ReadOnly_ShouldBeTrue()
		{
			AssertEquals(nameof(ITransaction.ReversalStatusCode_ReadOnly), true, (TestContra as ITransaction).ReversalStatusCode_ReadOnly);
		}

		public void TestReversalStatusCodeList_ShouldBeNull()
		{
			AssertNull(nameof(ITransaction.ReversalStatusCodeList), (TestContra as ITransaction).ReversalStatusCodeList);
		}

		#endregion ReversalStatusCode

		#endregion

		#region IDataExportBatchSource Members

		public void TestIsDataExportBatchSupported()
		{
			AssertEquals("IsDataExportBatchSupported", ((IDataExportBatchSource)TestContra.ARRow).IsDataExportBatchSupported, ((IDataExportBatchSource)TestContra).IsDataExportBatchSupported);
		}

		public void TestRelatedBatchCollection()
		{
			var batch = TestObjectCreator.CreateDataExportBatchForHeader(TestContra.ARRow);
			Factory.Save();
			AssertCollectionContains("Public collection contains batch", batch, TestContra.DataExportBatchCollection);
		}

		#endregion

		#region Decimal Places

		public void TestZDecimalsHaveCorrectDecimalPlaces()
		{
			var localList = new List<string>
				{
					nameof(TestContra.AH_Calc_RecBeforeContra),
					nameof(TestContra.AH_Calc_RecAfterContra),
					nameof(TestContra.AH_Calc_PayBeforeContra),
					nameof(TestContra.AH_Calc_PayAfterContra),
				};

			var tester = new DecimalPlacesAttributeTester(TestContra);
			tester.CheckLocalCurrency(localList, nameof(TestContra.LocalDecimals));
		}

		public void TestDecimalPlacesAttributeApplyToAllZDecimalProperties()
		{
			var properties = typeof(Contra).GetProperties().Where(x => x.PropertyType == typeof(ZDecimal)).ToList().SkipWhile(x => x.Name == "AH_ExchangeRateAmount" || x.Name == "AH_InvoiceAmount" || x.Name == "AH_OSTotal");
			Assert(properties.All(x => Attribute.IsDefined(x, typeof(DecimalPlacesAttribute))));
		}

		#endregion

		#region Implementation

		Contra TestContra;

		protected override void SetUp()
		{
			MakeActiveAndInactiveOrgsDebtorsAndCreditors();
			TestCaseHelper.ClearTable(AutoAccPeriodManagement.Schema.TableName);
			TestContra = (Contra)GetNewBusinessObject();
		}

		void MakeActiveAndInactiveOrgsDebtorsAndCreditors()
		{
			TestObjectCreator.InActiveOrg.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.InActiveOrg.CompanyData.OB_IsDebtor = true;
			TestObjectCreator.ActiveOrg.CompanyData.OB_IsCreditor = true;
			TestObjectCreator.ActiveOrg.CompanyData.OB_IsDebtor = true;
			Factory.Save();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Contra.New(Factory);
		}

		OrgHeader fAPAccount;
		protected OrgHeader APAccount
		{
			get
			{
				if (fAPAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

					ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(filter, JoinCondition.And);

					query.AddSubQuery(subQuery, JoinCondition.And);

					fAPAccount = Factory.LoadTop1<OrgHeader>(query);
				}
				return fAPAccount;
			}
		}

		OrgHeader fARAccount;
		protected OrgHeader ARAccount
		{
			get
			{
				if (fARAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

					ZQuery filter = new ZQuery(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					filter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(filter, JoinCondition.And);

					query.AddSubQuery(subQuery, JoinCondition.And);

					fARAccount = Factory.LoadTop1<OrgHeader>(query);
				}
				return fARAccount;
			}
		}

		RefCurrency fForeignCurrency;
		protected RefCurrency ForeignCurrency
		{
			get
			{
				if (fForeignCurrency == null)
				{
					fForeignCurrency = Factory.New<RefCurrency>();
					fForeignCurrency.RX_Code = "FOR";
					fForeignCurrency.RX_SubUnitRatio = 100;
				}
				return fForeignCurrency;
			}
		}

		#endregion
	}
}
