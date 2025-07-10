using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	public abstract class JournalTest : TransactionHeaderTest
	{
		[TestDate(2020, 01, 15)]
		public void TestGetPostDate()
		{
			var pastDate = ZDateTime.Today.AddDays(-1);
			AssertEquals(pastDate, Journal.GetPostDate(pastDate));

			AssertEquals(ZDateTime.Today, Journal.GetPostDate(ZDateTime.Today));

			var futureDate = ZDateTime.Today.AddDays(5);
			AssertEquals(ZDateTime.Today, Journal.GetPostDate(futureDate));
		}

		public void TestIsPaymentBasisWithholdingJournal()
		{
			Journal.AH_TransactionCategory = string.Empty;
			AssertEquals(false, Journal.IsPaymentBasisWithholdingJournal);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			AssertEquals(true, Journal.IsPaymentBasisWithholdingJournal);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Standard;
			AssertEquals(false, Journal.IsPaymentBasisWithholdingJournal);
		}

		public void TestOnSaving_AH_PostToGLIsSetTrueWhenTransactionCategoryIsPaymentBasisWithholdingJournal()
		{
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.PaymentBasisWithholding;
			Assert("Precondition:", Journal.IsPaymentBasisWithholdingJournal);
			Factory.Save();
			AssertEquals(Constants.BooleanTrueString, Journal.AH_PostToGL);
		}

		public void TestOnSaving_AH_PostToGLIsNOTSetWhenTransactionCategoryIsNOTPaymentBasisWithholdingJournal()
		{
			var journals = new List<Journal>();

			foreach (var transactionCategory in typeof(Constants.TransactionCategory.Codes).GetConstantValues())
			{
				if (transactionCategory != Constants.TransactionCategory.Codes.PaymentBasisWithholding)
				{
					var journal = (Journal)PrepareTransactionHeaderForTest();
					journal.AH_TransactionCategory = transactionCategory;
					Assert("Precondition:", !journal.IsPaymentBasisWithholdingJournal);
					journals.Add(journal);
				}
			}

			var journal_EmptyCategory = (Journal)PrepareTransactionHeaderForTest();
			journal_EmptyCategory.AH_TransactionCategory = "";
			journals.Add(journal_EmptyCategory);

			Factory.Save();

			foreach (var journal in journals)
			{
				AssertEquals(Constants.BooleanFalseString, journal.AH_PostToGL);
			}
		}

		public void TestIsMultipleInstallmentsJournal()
		{
			Journal.AH_TransactionCategory = string.Empty;
			AssertEquals(false, Journal.IsMultipleInstallmentsJournal);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClearingJournal;
			AssertEquals(true, Journal.IsMultipleInstallmentsJournal);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.InstalmentJournal;
			AssertEquals(true, Journal.IsMultipleInstallmentsJournal);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Standard;
			AssertEquals(false, Journal.IsMultipleInstallmentsJournal);
		}

		public new void TestOSPartialPaymentAmount_ReadOnly()
		{
			foreach (var category in typeof(Constants.TransactionCategory.Codes).GetConstantValues().Prepend(string.Empty))
			{
				Journal.IsMiscellaneousTransaction = false;
				Journal.AH_TransactionCategory = category;
				if (category == Constants.TransactionCategory.Codes.PaymentBasisWithholding)
				{
					AssertEquals("OSPartialPaymentAmount is readonly for Payment Basis Withholding journals", true, Journal.OSPartialPaymentAmount_ReadOnly);
				}
				else
				{
					AssertEquals(false, Journal.OSPartialPaymentAmount_ReadOnly);
				}

				Journal.IsMiscellaneousTransaction = true;
				AssertEquals("OSPartialPaymentAmount is readonly for miscellaneous transaction, such as Bank Fee journals", true, Journal.OSPartialPaymentAmount_ReadOnly);
			}
		}

		public void TestDefaultValues()
		{
			Assert("Transaction Number field should be read only",
				Journal.AH_TransactionNumInfo.ReadOnly);
			AssertZDateTimeDefaultBehaviour(Journal, Journal.AH_DueDateInfo);
			AssertEquals(Constants.TransactionCategory.Codes.Standard, Journal.AH_TransactionCategory);
		}

		public void TestSetAH_OSExTaxAmountFromOverpayedInvoice()
		{
			APInvoice apInvoice = Factory.NewWithValidTestData<APInvoice>();
			((IMatching)apInvoice).OSPartialPaymentAmount = -500m;
			apInvoice.AH_OutstandingAmount = -200m;
			Journal.SetAH_OSExTaxAmountFromOverpayedInvoice(apInvoice);
			AssertEquals(DebitCreditDataEntry.DR, Journal.DebitCreditSign);
			AssertEquals(300M, Journal.AH_OSExTaxAmount);

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			((IMatching)arInvoice).OSPartialPaymentAmount = 300m;
			arInvoice.AH_OutstandingAmount = 100m;
			Journal.SetAH_OSExTaxAmountFromOverpayedInvoice(arInvoice);
			AssertEquals(DebitCreditDataEntry.CR, Journal.DebitCreditSign);
			AssertEquals(200M, Journal.AH_OSExTaxAmount);
		}

		public void TestGetOppositeDebitCreditSign()
		{
			AssertEquals(DebitCreditDataEntry.DR, Journal.GetOppositeDebitCreditSign(DebitCreditDataEntry.CR));
			AssertEquals(DebitCreditDataEntry.CR, Journal.GetOppositeDebitCreditSign(DebitCreditDataEntry.DR));
		}

		public void TestAH_TransactionCategory()
		{
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			AssertEquals(ZString.Empty, Journal.AH_Desc);
			AssertEquals(Journal.MatchingAccount_ForTestOnly, Journal.AH_AG);
			AssertEquals(Journal.DebitCreditSign, Journal.GetOppositeDebitCreditSign(Journal.DebitCreditSignDefault_ForTestOnly));

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionAlreadyPaid;
			AssertEquals(ZString.Empty, Journal.AH_Desc);
			AssertEquals(Journal.MatchingAccount_ForTestOnly, Journal.AH_AG);
			AssertEquals(Journal.DebitCreditSign, Journal.GetOppositeDebitCreditSign(Journal.DebitCreditSignDefault_ForTestOnly));

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.WithholdingTax;
			AssertEquals(Constants.TransactionCategory.Descriptions.WithholdingTax.ToString().ToUpper(), Journal.AH_Desc);
			AssertEquals(Journal.WHTAccount_ForTestOnly, Journal.AH_AG);
			AssertEquals(Journal.DebitCreditSign, Journal.DebitCreditSignDefault_ForTestOnly);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Standard;
			AssertEquals(Journal.JournalDefaultDescription, Journal.AH_Desc);
			AssertEquals(Journal.JournalAccountDefault_ForTestOnly, Journal.AH_AG);
			AssertEquals(Journal.DebitCreditSign, Journal.DebitCreditSignDefault_ForTestOnly);

			Journal parentJournal = GetNewBusinessObject() as Journal;
			AssertEquals(Constants.TransactionCategory.Codes.Standard, parentJournal.AH_TransactionCategory);
			Journal.RelatedJournal = parentJournal;
			parentJournal.RelatedJournal = Journal;
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.WithholdingTax;
			AssertEquals(Constants.TransactionCategory.Codes.WithholdingTax, parentJournal.AH_TransactionCategory);

			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionNotFound;
			AssertEquals(Constants.TransactionCategory.Codes.TransactionNotFound, Journal.AH_TransactionCategory);
			AssertEquals(Constants.TransactionCategory.Codes.TransactionNotFound, parentJournal.AH_TransactionCategory);
			AssertEquals(Journal.DebitCreditSign, Journal.GetOppositeDebitCreditSign(Journal.DebitCreditSignDefault_ForTestOnly));
			AssertEquals(parentJournal.DebitCreditSign, Journal.GetOppositeDebitCreditSign(Journal.DebitCreditSign));
		}

		public void TestDebitCreditSign()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.DebitCreditSign = DebitCreditDataEntry.DR;
			apJournal.AH_OSExTaxAmount = 500M;
			apJournal.RelatedJournal = Journal;
			Journal.RelatedJournal = apJournal;

			apJournal.DebitCreditSign = DebitCreditDataEntry.CR;

			AssertEquals(DebitCreditDataEntry.CR, apJournal.DebitCreditSign);
			AssertEquals(DebitCreditDataEntry.DR, Journal.DebitCreditSign);

			apJournal.DebitCreditSign = DebitCreditDataEntry.DR;

			AssertEquals(DebitCreditDataEntry.DR, apJournal.DebitCreditSign);
			AssertEquals(DebitCreditDataEntry.CR, Journal.DebitCreditSign);
		}

		public void TestUseJournalValidation()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.RelatedJournal = Journal;
			Journal.RelatedJournal = apJournal;

			Assert(!apJournal.UseJournalValidation);
			Assert(!Journal.UseJournalValidation);

			apJournal.UseJournalValidation = true;

			Assert(apJournal.UseJournalValidation);
			Assert(Journal.UseJournalValidation);

			apJournal.UseJournalValidation = false;

			Assert(!apJournal.UseJournalValidation);
			Assert(!Journal.UseJournalValidation);
		}

		public void TestAH_InvoiceDate()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_InvoiceDate = ZDateTime.BrettsBirthday;
			Journal.RelatedJournal = apJournal;
			Journal.AH_InvoiceDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(Journal.AH_InvoiceDate, apJournal.AH_InvoiceDate);
		}

		public void TestAH_PostDate()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_PostDate = ZDateTime.BrettsBirthday;
			Journal.RelatedJournal = apJournal;
			Journal.AH_PostDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(Journal.AH_PostDate, apJournal.AH_PostDate);
		}

		public void TestAH_DueDate()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_DueDate = ZDateTime.BrettsBirthday;
			Journal.RelatedJournal = apJournal;
			Journal.AH_DueDate = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(Journal.AH_DueDate, apJournal.AH_DueDate);
		}

		public void TestAH_Desc()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_Desc = "TEST desc";
			Journal.RelatedJournal = apJournal;
			Journal.AH_Desc = "Invoice Desc";
			AssertEquals(Journal.AH_Desc, apJournal.AH_Desc);
		}

		public void TestAH_ChequeOrReference()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_ChequeOrReference = "TEST reference";
			Journal.RelatedJournal = apJournal;
			Journal.AH_ChequeOrReference = "Invoice Number";
			AssertEquals(Journal.AH_ChequeOrReference, apJournal.AH_ChequeOrReference);
		}

		public void TestAH_ExchangeRate()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_ExchangeRate = 0.988;
			Journal.RelatedJournal = apJournal;
			Journal.AH_ExchangeRate = 1;
			AssertEquals(Journal.AH_ExchangeRate, apJournal.AH_ExchangeRate);
		}

		public void TestAH_OH()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_OH = ZGuid.NewZGuid();
			Journal.RelatedJournal = apJournal;
			Journal.AH_OH = ZGuid.NewZGuid();
			AssertEquals(Journal.AH_OH, apJournal.AH_OH);
		}

		public void TestAH_AG()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_AG = ZGuid.NewZGuid();
			Journal.RelatedJournal = apJournal;
			Journal.AH_AG = ZGuid.NewZGuid();
			AssertEquals(Journal.AH_AG, apJournal.AH_AG);
		}

		public void TestAH_AGForSubAccounts()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

			var journal = (Journal)PrepareTransactionHeaderForTest();
			if (journal.IsMultiSubAccountsSupported)
			{
				journal.AH_AG = glHeader.PK;

				var subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);
				var subAccount2 = GetSubAccount(GlbStaffSchema.Constants.Prefix);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount1.AHS_SubClassParentId);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount2.AHS_SubClassParentId);

				subAccount1.AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;
				Factory.Save();

				Assert("subAccount1 can be saved in the database", subAccount1.IsInDatabase);
				Assert("subAccount2 should be deleted", subAccount2.IsDeleted);

				journal.AH_AG = glHeader.PK;
				subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);

				AssertEquals("sub account count should be 1", 1, journal.SubAccounts.Count);

				JournalSubAccount GetSubAccount(ZString subClassParentTableCode)
				{
					return journal.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x.AHS_SubClassParentTableCode == subClassParentTableCode);
				}
			}
			else
			{
				AssertEquals("SubAccounts Count", 0, journal.SubAccounts.Count);
			}
		}

		public void TestAH_GB()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_GB = ZGuid.NewZGuid();
			Journal.RelatedJournal = apJournal;
			Journal.AH_GB = ZGuid.NewZGuid();
			AssertEquals(Journal.AH_GB, apJournal.AH_GB);
		}

		public void TestAH_GE()
		{
			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_GE = ZGuid.NewZGuid();
			Journal.RelatedJournal = apJournal;
			Journal.AH_GE = ZGuid.NewZGuid();
			AssertEquals(Journal.AH_GE, apJournal.AH_GE);
		}

		public override void TestAH_RX_NKTransactionCurrency()
		{
			base.TestAH_RX_NKTransactionCurrency();

			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			apJournal.AH_RX_NKTransactionCurrency = "USD";
			Journal.RelatedJournal = apJournal;
			Journal.AH_RX_NKTransactionCurrency = "AUD";
			AssertEquals(Journal.AH_RX_NKTransactionCurrency, apJournal.AH_RX_NKTransactionCurrency);
		}

		public void TestValidateAH_PostDate()
		{
			Journal.AH_PostDate = ZDateTime.Today.AddDays(1);
			Assert("Post date must be after or equal to today", Journal.AH_PostDateInfo.HasErrors());
		}

		public void TestValidateAH_DueDate()
		{
			Journal.Validation.ValidateAH_DueDate();
			Journal.AH_DueDate = ZDateTime.Empty;
			Assert("Due date cannot be empty", Journal.AH_DueDateInfo.HasErrors());
		}

		public void TestValidateExchangeRateAmount()
		{
			Journal.ExchangeRate.Rate = 0;
			Assert("Exchange Rate cannot be 0 - should give errors",
				Journal.ExchangeRate.RateInfo.HasErrors());
		}

		public void TestGetNewMatchingValidation()
		{
			AssertNotNull("Should return something", Journal.GetNewMatchingValidation_ForTestOnly());
			AssertEquals("IsInDatabase", false, Journal.IsInDatabase);
			AssertEquals("AH_TransactionCreatedByMatching", false, Journal.AH_TransactionCreatedByMatching);
			Assert("Should be MatchingValidation", Journal.GetNewMatchingValidation_ForTestOnly() is MatchingValidation);

			Journal.AH_TransactionCreatedByMatching = true;
			Assert("Should be JournalValidation", Journal.GetNewMatchingValidation_ForTestOnly() is JournalValidation);

			Factory.Save();
			AssertEquals("IsInDatabase", true, Journal.IsInDatabase);
			Assert("Should be MatchingValidation", Journal.GetNewMatchingValidation_ForTestOnly() is MatchingValidation);

			Journal.AH_TransactionCreatedByMatching = false;
			Assert("Should be MatchingValidation", Journal.GetNewMatchingValidation_ForTestOnly() is MatchingValidation);

			Journal.UseJournalValidation = true;
			Assert("Should be JournalValidation", Journal.GetNewMatchingValidation_ForTestOnly() is JournalValidation);
		}

		public override void TestAH_OSExTaxAmount()
		{
			base.TestAH_OSExTaxAmount();

			Journal.AH_OSExTaxAmount = 100m;
			AssertEquals(100m * Journal.Multiplier_ForTestOnly, ((IMatching)Journal).OSPartialPaymentAmount);

			((IMatching)Journal).OSPartialPaymentAmount = 50m;
			Journal.AH_OSExTaxAmount = 75m;
			AssertEquals(50m, ((IMatching)Journal).OSPartialPaymentAmount);

			APJournal apJournal = Factory.NewWithValidTestData<APJournal>();
			Journal.RelatedJournal = apJournal;

			Journal.AH_OSExTaxAmount = 100m;
			AssertEquals(-100m, ((IMatching)apJournal).OSPartialPaymentAmount);
			AssertEquals(100m, apJournal.AH_OSExTaxAmount);

			((IMatching)apJournal).OSPartialPaymentAmount = -50m;
			Journal.AH_OSExTaxAmount = 75m;
			AssertEquals(-50m, ((IMatching)apJournal).OSPartialPaymentAmount);
			AssertEquals(75m, apJournal.AH_OSExTaxAmount);
		}

		public void TestValidateAH_OSExTaxAmount()
		{
			Journal.AH_OSExTaxAmount = 0;
			Assert("Journal Amount cannot be 0", Journal.AH_OSExTaxAmountInfo.HasErrors());
			Journal.AH_OSExTaxAmount = 2;
			Assert("Journal amount can be greater than 0", !Journal.AH_OSExTaxAmountInfo.HasErrors());
			Journal.AH_OSExTaxAmount = -1;
			Assert("Journal Amount cannot be less than 0", Journal.AH_OSExTaxAmountInfo.HasErrors());
		}

		public void TestGLAccountsFilter()
		{
			var testBSH_GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testBSH_GLHeader.AG_AccountType = "BSH";
			testBSH_GLHeader.AG_AccountNum = "!!!";
			testBSH_GLHeader.AG_DisallowDirectPosting = false;
			testBSH_GLHeader.AG_ControlAccount = false;

			var testTTL_GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testTTL_GLHeader.AG_AccountType = "TTL";
			testTTL_GLHeader.AG_AccountNum = "@@@";
			testTTL_GLHeader.AG_ControlAccount = false;

			var testPL_GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testPL_GLHeader.AG_AccountType = "P&L";
			testPL_GLHeader.AG_AccountNum = "###";
			testPL_GLHeader.AG_ControlAccount = true;
			Factory.Save();

			APJournal testAPJournal = Factory.New<APJournal>();
			var glHeaders = testAPJournal.GLHeaders;
			glHeaders.Load();
			Assert("GLHeaders should contain the Test BSH Header",
				glHeaders.Contains(testBSH_GLHeader.PK));
			Assert("GLHeaders should not contain the Test TTLHeader",
				!glHeaders.Contains(testTTL_GLHeader.PK));
			Assert("GLHeaders should not contain the Test P&L HEader",
				!glHeaders.Contains(testPL_GLHeader.PK));
		}

		public void TestGLAccountsFilter_IsGlobal()
		{
			var globalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			globalGLHeader.AG_AccountType = "BSH";
			globalGLHeader.AG_IsGlobal = true;
			var nonGlobalGLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader1.AG_AccountType = "BSH";
			nonGlobalGLHeader1.AG_IsGlobal = false;
			var nonGlobalGLHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			nonGlobalGLHeader2.AG_AccountType = "BSH";
			nonGlobalGLHeader2.AG_IsGlobal = false;
			nonGlobalGLHeader2.CompanyFilters.AddNew().ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			APJournal testAPJournal = Factory.New<APJournal>();
			var glHeaders = testAPJournal.GLHeaders;
			glHeaders.Load();
			AssertEquals(true, glHeaders.Contains(globalGLHeader.PK));
			AssertEquals(false, glHeaders.Contains(nonGlobalGLHeader1.PK));
			AssertEquals(true, glHeaders.Contains(nonGlobalGLHeader2.PK));
		}

		public void TestPosting()
		{
			// AP JNL DR
			APJournal testAPJournalDR = Factory.New<APJournal>();
			testAPJournalDR.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			testAPJournalDR.AH_ExchangeRate = 2;
			testAPJournalDR.AH_OSExTaxAmount = 100;
			testAPJournalDR.AH_LocalExTaxAmount = 50;
			testAPJournalDR.AH_LocalOutstandingAmount = 50;
			testAPJournalDR.DebitCreditSign = DebitCreditDataEntry.DR;
			Factory.Save();

			// AP JNL CR
			APJournal testAPJournalCR = Factory.New<APJournal>();
			testAPJournalCR.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			testAPJournalCR.AH_ExchangeRate = 2;
			testAPJournalCR.AH_OSExTaxAmount = 100;
			testAPJournalCR.AH_LocalExTaxAmount = 50;
			testAPJournalCR.AH_LocalOutstandingAmount = 50;
			testAPJournalCR.DebitCreditSign = DebitCreditDataEntry.CR;
			Factory.Save();

			ARJournal testARJournalDR = Factory.New<ARJournal>();
			testARJournalDR.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			testARJournalDR.AH_ExchangeRate = 2;
			testARJournalDR.AH_OSExTaxAmount = 100;
			testARJournalDR.AH_LocalExTaxAmount = 50;
			testARJournalDR.AH_LocalOutstandingAmount = 50;
			testARJournalDR.DebitCreditSign = DebitCreditDataEntry.DR;

			Factory.Save();

			ARJournal testARJournalCR = Factory.New<ARJournal>();
			testARJournalCR.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			testARJournalCR.AH_ExchangeRate = 2;
			testARJournalCR.AH_OSExTaxAmount = 100;
			testARJournalCR.AH_LocalExTaxAmount = 50;
			testARJournalCR.AH_LocalOutstandingAmount = 50;
			testARJournalCR.DebitCreditSign = DebitCreditDataEntry.CR;

			Factory.Save();

			// AP JNL DR
			APJournal loadAPJournalDR = Factory.Load<APJournal>(testAPJournalDR.PK);
			DataRow aPJournalDRRow = ((INeedRow)loadAPJournalDR).Row;

			AssertEquals("Row in Database for AP Journal DR should be negated", -50M, aPJournalDRRow["AH_InvoiceAmount"]);
			AssertEquals("Row in Database for AP Journal DR should be negated", -100M, aPJournalDRRow["AH_OSTotal"]);
			AssertEquals("Row in Database for AP Journal DR should be negated", -50M, aPJournalDRRow["AH_OutstandingAmount"]);

			AssertEquals("Loaded Journal should be negated (-100)", -100M, loadAPJournalDR.AH_OSTotal);
			AssertEquals("LocalAmount should be negated", -50M, loadAPJournalDR.AH_InvoiceAmount);
			AssertEquals("DebitCredit should be set to Debit", DebitCreditDataEntry.DR, loadAPJournalDR.DebitCreditSign);

			// AP JNL CR
			APJournal loadAPJournalCR = Factory.Load<APJournal>(testAPJournalCR.PK);
			DataRow aPJournalCRRow = ((INeedRow)loadAPJournalCR).Row;

			AssertEquals("Row in Database for AP Journal CR should not be negated", 50M, aPJournalCRRow["AH_InvoiceAmount"]);
			AssertEquals("Row in Database for AP Journal CR should not be negated", 100M, aPJournalCRRow["AH_OSTotal"]);
			AssertEquals("Row in Database for AP Journal CR should not be negated", 50M, aPJournalCRRow["AH_OutstandingAmount"]);

			AssertEquals("Loaded Journal should not be negated", 100M, loadAPJournalCR.AH_OSTotal);
			AssertEquals("LocalAmount should not be negated", 50M, loadAPJournalCR.AH_InvoiceAmount);
			AssertEquals("DebitCredit should be set to Credit", DebitCreditDataEntry.CR, loadAPJournalCR.DebitCreditSign);

			// AR JNL DR
			ARJournal loadARJournalDR = Factory.Load<ARJournal>(testARJournalDR.PK);
			DataRow aRJournalDRRow = ((INeedRow)loadARJournalDR).Row;

			AssertEquals("Row in Database for AR Journal DR should be negated", -50M, aRJournalDRRow["AH_InvoiceAmount"]);
			AssertEquals("Row in Database for AR Journal DR should be negated", -100M, aRJournalDRRow["AH_OSTotal"]);
			AssertEquals("Row in Database for AR Journal DR should be negated", -50M, aRJournalDRRow["AH_OutstandingAmount"]);

			AssertEquals("Loaded Journal should be negated", -100M, loadARJournalDR.AH_OSTotal);
			AssertEquals("Loaded Journal should be negated", -50M, loadARJournalDR.AH_InvoiceAmount);
			AssertEquals("DebitCredit should be set to Debit", DebitCreditDataEntry.DR, loadARJournalDR.DebitCreditSign);

			// AR JNL CR
			ARJournal loadARJournalCR = Factory.Load<ARJournal>(testARJournalCR.PK);
			DataRow aRJournalCRRow = ((INeedRow)loadARJournalCR).Row;

			AssertEquals("Row in Database for AR Journal CR should not be negated", 50M, aRJournalCRRow["AH_InvoiceAmount"]);
			AssertEquals("Row in Database for AR Journal CR should not be negated", 50M, aRJournalCRRow["AH_OutstandingAmount"]);
			AssertEquals("Row in Database for AR Journal CR should not be negated", 100M, aRJournalCRRow["AH_OSTotal"]);

			AssertEquals("Loaded Journal should not be negated", 100M, loadARJournalCR.AH_OSTotal);
			AssertEquals("Loaded Journal should not be negated", 50M, loadARJournalCR.AH_InvoiceAmount);
			AssertEquals("DebitCredit should be set to Credit", DebitCreditDataEntry.CR, loadARJournalCR.DebitCreditSign);
		}

		public void TestIsMatched()
		{
			Journal.AH_OSExTaxAmount = 100m;
			Journal.AH_OSTaxAmount = 10.00m;
			Journal.AH_LocalOutstandingAmount = 110.00m;
			Assert("Should not be matched", !((IMatching)Journal).IsMatched);
			Journal.AH_LocalOutstandingAmount = 50.00m;
			Assert("Should now be matched", ((IMatching)Journal).IsMatched);
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

			Journal.AH_OSExTaxAmount = 100m;
			Journal.AH_OSTaxAmount = 10.00m;
			Journal.AH_OutstandingAmount = 110.00m;
			Journal.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Journal.MakeOSOutstandingAmountApplicable(110m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 110m, Journal.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)Journal).FullyPay(expectedFullyPaidDate);

			AssertEquals("Fully Paid date on Journal", expectedFullyPaidDate, Journal.AH_FullyPaidDate);
			AssertEquals("Outstanding Amount on Journal", 0m, Journal.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", 0m, Journal.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Journal.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Private field should be set by calling method", 110.00m, Journal.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
		}

		public void TestPartiallyPay()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestPartiallyPay_EnableNewOSOutstandingAmountFeature()
		{
			AssertPartiallyPay(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertPartiallyPay(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Journal.AH_LocalExTaxAmount = 100M;
			Journal.AH_OSTotalAmount = 100M;
			Journal.AH_LocalOutstandingAmount = 70M;
			Journal.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 70m * Journal.Multiplier_ForTestOnly;
				Journal.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 70m, Journal.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 60M;

			IMatching thisIMatching = Journal;
			thisIMatching.OSPartialPaymentAmount = AmountWithMultiplier;
			thisIMatching.PartiallyPay();

			AssertEquals("Local Outstanding Amount should be 10", 10M, Journal.AH_LocalOutstandingAmount);
			AssertEquals("Should not be fully paid", ZDateTime.Empty, Journal.AH_FullyPaidDate);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? 10m : 0m, Journal.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			AssertEquals("Should not be fully paid", ZDateTime.Empty, Journal.AH_FullyPaidDate);

			thisIMatching.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = thisIMatching.CurrentMatchGroup;

			AssertEquals("There should be 1 matchlink", 1, matchLinks.Count);
			TransactionMatchLink matchLink = matchLinks[0];
			AssertEquals("Amount should be +60/-60 depending on Journal type", AmountWithMultiplier, matchLink.AP_Amount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), matchLink.AP_OSAmount);
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

			Journal.AH_OSExTaxAmount = 100m;
			Journal.AH_OSTaxAmount = 10.00m;
			Journal.AH_OutstandingAmount = 110.00m;
			Journal.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Journal.MakeOSOutstandingAmountApplicable(110m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 110m, Journal.AH_OSOutstandingAmount);
			}

			ZDateTime expectedFullyPaidDate = ZDateTime.Now.AddDays(-2);

			((IMatching)Journal).FullyPay(expectedFullyPaidDate);
			Journal.GenerateMatchLinks();
			TransactionMatchLinkCollection matchLinks = ((IMatching)Journal).CurrentMatchGroup;

			AssertEquals("Should have one matchlink record generated", 1, matchLinks.Count);

			TransactionMatchLink singleLink = matchLinks[0];

			AssertEquals("Match Amount should be same as outstanding amount after fully paying",
				110.00m, singleLink.AP_Amount);
			AssertEquals("AP_OSAmount",
				isEnableNewOSOutstandingAmountFeature ? 110.00m : 0m, singleLink.AP_OSAmount);
			AssertEquals("Match FK should be this header", Header.PK, singleLink.AP_AH);
		}

		public void TestUnmatchFullyPaid()
		{
			AssertUnmatchFullyPaid(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchFullyPaid_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchFullyPaid(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchFullyPaid(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Journal.AH_LocalExTaxAmount = 45M;
			Journal.AH_LocalOutstandingAmount = 0M;
			Journal.AH_FullyPaidDate = ZDateTime.Today;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				Journal.MakeOSOutstandingAmountApplicable(0m);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 0m, Journal.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 9M;

			AssertEquals("This amount can be unmatched", UnmatchingResult.Success, ((IMatching)Journal).CanUnmatch(AmountWithMultiplier));
			((IMatching)Journal).Unmatch(AmountWithMultiplier, AmountWithMultiplier);
			AssertEquals("Outstanding amount should be +9/-9 depending on ledger", AmountWithMultiplier, Journal.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), Journal.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Journal.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Fully paid date should be null", ZDateTime.Empty, Journal.AH_FullyPaidDate);
		}

		public void TestUnmatchPartiallyPaid()
		{
			AssertUnmatchPartiallyPaid(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchPartiallyPaid_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchPartiallyPaid(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchPartiallyPaid(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Journal.AH_LocalExTaxAmount = 11M;
			Journal.AH_LocalOutstandingAmount = 10M;
			Journal.AH_FullyPaidDate = ZDateTime.Empty;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 10m * Journal.Multiplier_ForTestOnly;
				Journal.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 10m, Journal.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 2M;

			AssertEquals("This amount cannot be unmatched", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)Journal).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 1M;

			AssertEquals("This amount can be unmatched", UnmatchingResult.Success, ((IMatching)Journal).CanUnmatch(AmountWithMultiplier));
			((IMatching)Journal).Unmatch(AmountWithMultiplier, AmountWithMultiplier);
			AmountWithMultiplier = 11M;
			AssertEquals("Outstanding amount should be +11/-11 depending on ledger", AmountWithMultiplier, Journal.AH_OutstandingAmount);
			AssertEquals("AP_OSAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), Journal.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Journal.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("Fullypaid date should be null", ZDateTime.Empty, Journal.AH_FullyPaidDate);
		}

		public void TestWithInvoiceAmountSignDifferentfromTotalSign()
		{
			AmountWithMultiplier = 1;
			var multiplier = AmountWithMultiplier;
			Journal.AH_InvoiceAmount = 402.9M * multiplier;
			Journal.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(Journal.AH_InvoiceAmount) == Math.Sign(Journal.AH_InvoiceAmount + Journal.AH_GSTAmount));
			var matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)Journal).CanUnmatch(matchLinkAmount));

			AmountWithMultiplier = -1;
			multiplier = AmountWithMultiplier;
			Journal.AH_InvoiceAmount = 402.9M * multiplier;
			Journal.AH_GSTAmount = -491.25M * multiplier;
			AssertEquals("Invoice amount sign should be different than invoice total sign", false, Math.Sign(Journal.AH_InvoiceAmount) == Math.Sign(Journal.AH_InvoiceAmount + Journal.AH_GSTAmount));
			matchLinkAmount = -88.35M * multiplier;
			AssertEquals(UnmatchingResult.Success, ((IMatching)Journal).CanUnmatch(matchLinkAmount));
		}

		public void TestUnmatchWithTax()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: false);
		}

		public void TestUnmatchWithTax_EnableNewOSOutstandingAmountFeature()
		{
			AssertUnmatchWithTax(isEnableNewOSOutstandingAmountFeature: true);
		}

		void AssertUnmatchWithTax(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Journal.AH_LocalExTaxAmount = 90M;
			Journal.AH_LocalOutstandingAmount = 40M;
			Journal.AH_LocalTaxAmount = 10M;

			if (isEnableNewOSOutstandingAmountFeature)
			{
				var osOutstandingAmount = 40m * Journal.Multiplier_ForTestOnly;
				Journal.MakeOSOutstandingAmountApplicable(osOutstandingAmount);

				AssertEquals("PreCondition - AH_IsOSOutstandingAmountApplicable", true, Journal.AH_IsOSOutstandingAmountApplicable);
				AssertEquals("PreCondition - AH_OSOutstandingAmount", 40m, Journal.AH_OSOutstandingAmount_WithMultiplier_ForTestOnly);
			}

			AmountWithMultiplier = 61M;

			AssertEquals("Cannot unmatch 61 since greater than tax amt + invoice amt - outstanding amt", UnmatchingResult.DataErrorAddingMatchAmountExceedOriginalInvoiceAmount, ((IMatching)Journal).CanUnmatch(AmountWithMultiplier));

			AmountWithMultiplier = 60M;

			AssertEquals("Can unmatch 60 since equals tax amt + invoice amt - outstanding amt", UnmatchingResult.Success,
				((IMatching)Journal).CanUnmatch(AmountWithMultiplier));

			((IMatching)Journal).Unmatch(AmountWithMultiplier, AmountWithMultiplier);

			AmountWithMultiplier = 100M;
			AssertEquals("Outstanding amt should be 100", AmountWithMultiplier, Journal.AH_OutstandingAmount);
			AssertEquals("AH_OSOutstandingAmount", isEnableNewOSOutstandingAmountFeature ? AmountWithMultiplier : new ZDecimal(0m), Journal.AH_OSOutstandingAmount);
			AssertEquals("AH_IsOSOutstandingAmountApplicable", isEnableNewOSOutstandingAmountFeature, Journal.AH_IsOSOutstandingAmountApplicable);
		}

		public void TestMatchDateFilter()
		{
			TransactionMatchLink matchlink1 = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			matchlink1.AP_AH = Journal.PK;
			matchlink1.AP_MatchDate = new ZDateTime(2004, 3, 3);

			TransactionMatchLink matchlink2 = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			matchlink2.AP_AH = Journal.PK;
			matchlink2.AP_MatchDate = new ZDateTime(2004, 4, 4);

			Factory.Save();

			AssertNotNull("Matchlink should not be null", Journal.LatestMatchLink);
			AssertEquals("Latest matchlink should be matchlink2", matchlink2.PK, Journal.LatestMatchLink.PK);
		}

		public void TestUnhookRecalculation()
		{
			Journal.UnhookLocalForeignAmountRecalculation();
			Journal.AH_OSExTaxAmount = 120.0m;
			Journal.AH_LocalExTaxAmount = 80.0m;
			Journal.AH_ExchangeRate = 0.4m;

			AssertEquals(120.0m, Journal.AH_OSExTaxAmount);
			AssertEquals(80.0m, Journal.AH_LocalExTaxAmount);
			AssertEquals(0.4m, Journal.AH_ExchangeRate);
		}

		public override void TestAH_OSOutstandingAmount()
		{
			SetupHeaderExRatesAndAmounts(0.57m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as Journal does not have VAT taxes.
			Header.AH_LocalOutstandingAmount = 378.02m;
			AssertEquals("OS Outstanding Amount", 215.473m, Header.AH_Calc_OSOutstandingAmount);

			Header.AH_LocalOutstandingAmount = 110.540m;
			AssertEquals("OS Outstanding Amount with decimal not matching local total (i.e. invoiceamount+gstamount)",
				63.008m, Header.AH_Calc_OSOutstandingAmount);

			SetupHeaderExRatesAndAmounts(0.1234m, 1000, 200.453m, 0); //there is no point of setting non zero tax amount here as Journal does not have VAT taxes.
			AssertEquals("OS Outstanding Amount with decimal and very small Exchange Rate", 200.453m, Header.AH_Calc_OSOutstandingAmount);
		}

		public void TestLocalPartialPaymentAmount()
		{
			Journal.AH_InvoiceAmount = 548.72M;
			Journal.AH_ExchangeRate = 0.6873M;
			Journal.AH_InvoiceAmount = 548.72M; // set here because ExchangeRate recalculates OSExTaxAmount
			Journal.AH_OutstandingAmount = 548.72M;
			Journal.AH_OSTotal = 377.13M;

			((IMatching)Journal).OSPartialPaymentAmount = 377.13M;
			AssertEquals("LocalPartialPayment amount should be 548.72 i.e. same as AH_OutstandingAmount", 548.72M, ((IMatching)Journal).LocalPartialPaymentAmount);
			((IMatching)Journal).OSPartialPaymentAmount = 150M;
			AssertEquals("LocalPartialPayment amount should be 218.25", 218.25M, ((IMatching)Journal).LocalPartialPaymentAmount);
			Journal.IsMiscellaneousTransaction = true;
			Assert("LocalPartialPayment amount should return AH_OutstandingAmount when IsMiscellaneousTransaction = true", ((IMatching)Journal).LocalPartialPaymentAmount == 548.72M && Journal.AH_OutstandingAmount == 548.72M);
		}

		public void TestGeneratePaymentApprovalItems()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: false);

		public void TestGeneratePaymentApprovalItems_EnableNewOSOutstandingAmountFeature()
			=> AssertGeneratePaymentApprovalItems(isEnableNewOSOutstandingAmountFeature: true);

		void AssertGeneratePaymentApprovalItems(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			Journal.AH_InvoiceAmount = 548.72M;
			Journal.AH_ExchangeRate = 0.6873M;
			Journal.AH_InvoiceAmount = 548.72M; // set here because ExchangeRate recalculates OSExTaxAmount
			Journal.AH_OutstandingAmount = 548.72M;
			Journal.AH_OSTotal = 377.13M;

			IMatching journalAsIMatching = Journal;

			journalAsIMatching.OSPartialPaymentAmount = 150M;

			Journal.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
			PaymentApprovalBase newPaymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();

			Journal.AH_TransactionCreatedByMatching = false;
			journalAsIMatching.GeneratePaymentApprovalItems(newPaymentApproval);
			AssertEquals("Payment Approval Items", 1, journalAsIMatching.PaymentApprovalItems.Count);
			AssertEquals("Payment Approval Item Amount", journalAsIMatching.LocalPartialPaymentAmount, journalAsIMatching.PaymentApprovalItems[0].A2_PaymentThisRun);
			AssertEquals("Payment Approval Item Payment OS Amount",
				isEnableNewOSOutstandingAmountFeature ? journalAsIMatching.OSPartialPaymentAmount : new ZDecimal(0m),
				journalAsIMatching.PaymentApprovalItems[0].A2_OSPaymentThisRun
			);
		}

		public void TestUnmatchSystemCreatedTransaction()
		{
			TestCaseHelper.ClearTable(TransactionMatchLink.Schema.TableName);

			Journal.AH_OSExTaxAmount = 19M;
			Journal.AH_LocalExTaxAmount = 19M; // actually 19 in DB
			Journal.AH_LocalOutstandingAmount = 0M;
			Journal.AH_FullyPaidDate = ZDateTime.Today;
			Journal.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink journalMatchLink = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			journalMatchLink.AP_Amount = 19M * Journal.Multiplier_ForTestOnly;
			journalMatchLink.AP_MatchGroupNum = "M00001022";
			journalMatchLink.AP_AH = Journal.PK;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -journalMatchLink.AP_Amount;

			TransactionMatchLink linkToMatch = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			linkToMatch.AP_AH = headerToMatch.PK;
			linkToMatch.AP_Amount = -journalMatchLink.AP_Amount;
			linkToMatch.AP_MatchGroupNum = journalMatchLink.AP_MatchGroupNum;
			TestObjectCreator.SetupMatchLinkMatchDate(Journal);

			Factory.Save();

			journalMatchLink.Unmatch();
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			((IMatching)Journal).ChangeUnmatchDate(expectedPostDate);

			journalMatchLink.Delete();
			linkToMatch.Delete();

			Factory.Save();

			TransactionHeader loadedJournal = Factory.Load<TransactionHeader>(Journal.PK);
			Assert("loadedJournal should be an Journal", loadedJournal is Journal);
			Assert("Current journal should be cancelled", loadedJournal.AH_IsCancelled);
			AssertEquals("Current journal's OutstandingAmount", 0M, loadedJournal.AH_OutstandingAmount);
			Assert("Current journal should be FullyPaid", !loadedJournal.AH_FullyPaidDate.IsEmpty);
			Assert("TransactionBelongsToGroup should be empty on loadedJournal", loadedJournal.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("PostDate should be changed", ZDateTime.Today, loadedJournal.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, loadedJournal.AH_FullyPaidDate.Date);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load(new ZQuery());
			AssertEquals("Should be 2 matchlinks", 2, matchLinks.Count);

			IReversing iRevJournal = ((IReversing)Journal).ReverseTransaction;
			Assert("Should be Journal", iRevJournal is Journal);
			Journal revJournal = (Journal)iRevJournal;
			AssertEquals("Reversing journal's InvoiceAmount", -19M * Journal.Multiplier_ForTestOnly, revJournal.AH_InvoiceAmount);
			AssertEquals("Reversing journal's OutstandingAmount", 0M, revJournal.AH_OutstandingAmount);
			Assert("Reversing journal should be cancelled", revJournal.AH_IsCancelled);
			Assert("Reversing journal should be fullypaid", !revJournal.AH_FullyPaidDate.IsEmpty);
			AssertEquals("TransactionBelongsToGroup on revJournal should = PK of loadedJournal", loadedJournal.PK, revJournal.AH_TransactionBelongsToGroup);
			AssertEquals("PostDate should be changed", expectedPostDate, revJournal.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revJournal.AH_FullyPaidDate.Date);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revJournal.PK);
			TransactionMatchLink revJournalLink = Factory.LoadTop1<TransactionMatchLink>(filter);
			AssertNotNull("There should be a matchlink for revJournal", revJournalLink);
			AssertEquals("Match Group should be M00001000", "M00001000", revJournalLink.AP_MatchGroupNum);
			AssertEquals("Amount", -19M * Journal.Multiplier_ForTestOnly, revJournalLink.AP_Amount);
			Assert("MatchDate should not be null", !revJournalLink.AP_MatchDate.IsEmpty);
			AssertEquals("MatchDate should be as changed post date", expectedPostDate, revJournalLink.AP_MatchDate);
		}

		public void TestLookups()
		{
			Assert(Journal.Lookups is JournalLookups);
		}

		public override void TestGetTopLevelTransaction()
		{
			AssertEquals(Header, Header.GetTopLevelTransaction);
		}

		public void TestControlsReadOnlynessDependsOnIsMiscellaneousTransaction()
		{
			AssertReadOnlynessDependsOnIsMiscellaneousTransaction(Journal.AH_AGInfo);
			AssertReadOnlynessDependsOnIsMiscellaneousTransaction(Journal.DebitCreditSignInfo);
			AssertEquals("IsInDatabase", false, Journal.IsInDatabase);

			Journal.AH_TransactionCreatedByMatching = true;

			Journal.IsMiscellaneousTransaction = false;
			Assert(Journal.DebitCreditSignInfo.Name + " shouldn't be readonly when IsMiscellaneousTransaction is not set", !Journal.DebitCreditSignInfo.ReadOnly);
			Journal.IsMiscellaneousTransaction = true;
			AssertEquals(Journal.DebitCreditSignInfo.Name + " should not be readonly even when IsMiscellaneousTransaction is set, but not in database and AH_TransactionCreatedByMatching", false, Journal.DebitCreditSignInfo.ReadOnly);

			Factory.Save();

			Assert(Journal.DebitCreditSignInfo.Name + " should be readonly now", Journal.DebitCreditSignInfo.ReadOnly);

			Journal.AH_TransactionCreatedByMatching = false;
			Assert(Journal.DebitCreditSignInfo.Name + " should be readonly now", Journal.DebitCreditSignInfo.ReadOnly);
		}

		public void TestUnmatchClearingJournal()
		{
			Journal.AH_OSExTaxAmount = 19M;
			Journal.AH_LocalExTaxAmount = 19M;
			Journal.AH_LocalOutstandingAmount = 0M;
			Journal.AH_FullyPaidDate = ZDateTime.Today;
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
			Journal.AH_TransactionCreatedByMatching = true;

			TransactionMatchLink journalMatchLink = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			journalMatchLink.AP_Amount = 19M * Journal.Multiplier_ForTestOnly;
			journalMatchLink.AP_MatchGroupNum = "M00001022";
			journalMatchLink.AP_AH = Journal.PK;
			journalMatchLink.AP_MatchDate = ZDateTime.Today;

			AccTransactionHeader headerToMatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			headerToMatch.AH_InvoiceAmount = -journalMatchLink.AP_Amount;

			TransactionMatchLink transactionMatchLink = ((IMatching)Journal).CurrentMatchGroup.AddNew();
			transactionMatchLink.AP_AH = headerToMatch.PK;
			transactionMatchLink.AP_Amount = -journalMatchLink.AP_Amount;
			transactionMatchLink.AP_MatchGroupNum = journalMatchLink.AP_MatchGroupNum;
			transactionMatchLink.AP_MatchDate = ZDateTime.Today;

			Factory.Save();

			journalMatchLink.Unmatch();
			ZDateTime expectedPostDate = ZDateTime.BrettsBirthday;
			((IMatching)Journal).ChangeUnmatchDate(expectedPostDate);

			journalMatchLink.Delete();
			transactionMatchLink.Delete();

			Factory.Save();

			TransactionHeader loadedJournal = Factory.Load<TransactionHeader>(Journal.PK);
			Assert("loadedJournal should be an Journal", loadedJournal is Journal);
			Assert("Current journal should be cancelled", loadedJournal.AH_IsCancelled);
			AssertEquals("Current journal's OutstandingAmount", 0M, loadedJournal.AH_OutstandingAmount);
			Assert("Current journal should be FullyPaid", !loadedJournal.AH_FullyPaidDate.IsEmpty);
			Assert("TransactionBelongsToGroup should be empty on loadedJournal", loadedJournal.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals("PostDate should be changed", ZDateTime.Today, loadedJournal.AH_PostDate.Date);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, loadedJournal.AH_FullyPaidDate.Date);

			TransactionMatchLinkCollection matchLinks = new TransactionMatchLinkCollection(Factory);
			matchLinks.Load();
			AssertEquals("Should be 2 matchlinks", 2, matchLinks.Count);

			IReversing iRevJournal = ((IReversing)Journal).ReverseTransaction;
			Assert("Should be Journal", iRevJournal is Journal);
			Journal revJournal = (Journal)iRevJournal;
			AssertEquals("Reversing journal's InvoiceAmount", -19M * Journal.Multiplier_ForTestOnly, revJournal.AH_InvoiceAmount);
			AssertEquals("Reversing journal's OutstandingAmount", 0M, revJournal.AH_OutstandingAmount);
			Assert("Reversing journal should be cancelled", revJournal.AH_IsCancelled);
			Assert("Reversing journal should be fullypaid", !revJournal.AH_FullyPaidDate.IsEmpty);
			AssertEquals("Reversed journal description", "REVERSAL RELATED TO " + Journal.AH_TransactionNum, revJournal.AH_Desc);
			AssertEquals("TransactionBelongsToGroup on revJournal should = PK of loadedJournal", loadedJournal.PK, revJournal.AH_TransactionBelongsToGroup);
			AssertEquals("PostDate should be changed", expectedPostDate, revJournal.AH_PostDate);
			AssertEquals("FullyPaidDate should be changed", expectedPostDate, revJournal.AH_FullyPaidDate.Date);

			ZQuery filter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, revJournal.PK);
			TransactionMatchLink revJournalLink = Factory.LoadTop1<TransactionMatchLink>(filter);
			AssertNotNull("There should be a matchlink for revJournal", revJournalLink);
			AssertEquals("Match Group should be M00001000", "M00001000", revJournalLink.AP_MatchGroupNum);
			AssertEquals("Amount", -19M * Journal.Multiplier_ForTestOnly, revJournalLink.AP_Amount);
			Assert("MatchDate should not be null", !revJournalLink.AP_MatchDate.IsEmpty);
			AssertEquals("MatchDate should be as changed post date", expectedPostDate, revJournalLink.AP_MatchDate);
		}

		public void TestHookAH_TransactionNum_ValueChangedToCompleteDescription()
		{
			Journal.AH_Desc = "Test Journal";
			Journal.AH_TransactionNum = "";

			Journal.HookAH_TransactionNum_ValueChangedToCompleteDescription(null);
			Journal.UnhookAH_TransactionNum_ValueChangedToCompleteDescription(null);
			Assert("Previous statement don't generate any exceptions.", true);

			TransactionHeader transaction = Factory.NewWithValidTestData<APCreditNote>();
			Journal.HookAH_TransactionNum_ValueChangedToCompleteDescription(transaction);
			transaction.AH_TransactionNum = "Test Number";
			AssertEquals("Test Journal", Journal.AH_Desc);

			string descriptionWithPlaceholder = string.Format("Test {0} Journal", DynamicTransactionCreatorClearingJournal.TransactionNumberPlaceholder);
			Journal.AH_Desc = descriptionWithPlaceholder;
			transaction.AH_TransactionNum = "Another Test Number";
			AssertEquals(descriptionWithPlaceholder, Journal.AH_Desc);

			Journal.HookAH_TransactionNum_ValueChangedToCompleteDescription(transaction);
			transaction.AH_TransactionNum = "";
			AssertEquals(descriptionWithPlaceholder, Journal.AH_Desc);

			transaction.AH_TransactionNum = "INVOICE";
			AssertEquals("Test INVOICE Journal", Journal.AH_Desc);

			transaction.AH_TransactionNum = "Another INVOICE";
			AssertEquals("Test INVOICE Journal", Journal.AH_Desc);

			Journal.UnhookAH_TransactionNum_ValueChangedToCompleteDescription(transaction);
			Assert("Previous statement don't generate any exceptions.", true);
		}

		void AssertCanReverse(ZGuid txnPK, bool expected, params string[] acceptableMessages)
		{
			var newFactory = new BusinessObjectFactory();
			var txnInNewFactory = newFactory.Load<TransactionHeader>(txnPK);
			ReversingFactory reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(txnInNewFactory);
			AssertEquals("Test whether or not you can reverse transaction", expected, reversing.CanReverseTransaction);

			if (!reversing.CanReverseTransaction)
			{
				Assert("Message is acceptable", acceptableMessages.ToList().Contains(reversing.CantReverseErrorMessage));
			}
		}

		public void TestTransactionWithIReversing_Standard()
		{
			Journal.AH_OH = TestObjectCreator.AALSHI.PK;
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.Standard;
			Journal.AH_InvoiceAmount = 154.99;
			Journal.AH_OSTotal = 154.99;
			Journal.AH_ChequeOrReference = "81003 6/28/13";
			Journal.AH_FullyPaidDate = ZDateTime.Empty;
			Journal.AH_OutstandingAmount = 154.99;
			Journal.AH_PostToGL = "Y";
			Journal.AH_TransactionCreatedByMatching = false;
			Factory.Save();

			AssertCanReverse(Journal.PK, true);
		}

		public void TestTransactionWithIReversing_TransactionAlreadyPaid()
		{
			AssertTransactionWithIReversing_TransactionAlreadyPaid(false);
		}

		public void TestTransactionWithIReversing_TransactionAlreadyPaid_TwoTheSame()
		{
			AssertTransactionWithIReversing_TransactionAlreadyPaid(true);
		}

		public void TestCashAdvanceOverpaymentJournalCannotBeReversed()
		{
			InvoicingBase invoice;
			if (Journal.Ledger_ForTestOnly == LedgerTypes.AccountsReceivable)
			{
				invoice = Factory.NewWithValidTestData<ARInvoice>();
			}
			else
			{
				invoice = Factory.NewWithValidTestData<APInvoice>();
			}
			Journal.AH_OH = TestObjectCreator.AALSHI.PK;
			Journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.CashAdvanceInvoice;
			Factory.Save();
			AssertCanReverse(Journal.PK, true);

			Journal.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();
			AssertCanReverse(Journal.PK, false, $"This journal is associated with Invoice {invoice.AH_TransactionNum}. Reversing Invoice {invoice.AH_TransactionNum} will reverse this journal.");
		}

		public override void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			Assert("GL header needs to be loaded to set subaccount details", true);
		}

		public void AssertTransactionWithIReversing_TransactionAlreadyPaid(bool twoTheSame)
		{
			int numberOfInitialJournals = twoTheSame ? 2 : 1;
			bool isAR = Journal.Ledger_ForTestOnly == LedgerTypes.AccountsPayable;
			var invoice = TestObjectCreator.CreateInvoiceWithLine(isAR ? typeof(ARInvoice) : typeof(APInvoice),
				"1", TestObjectCreator.AUD, 1, 100 * numberOfInitialJournals, 0, 100 * numberOfInitialJournals, 0, TestObjectCreator.AALSHI, ZGuid.Empty);
			Factory.Save();

			MatchingBase matchingBase = isAR ? new APMatchingBase(Factory) : new ARMatchingBase(Factory);
			matchingBase.PrimaryOrganization = TestObjectCreator.AALSHI.PK;
			AssertEquals("Precondition: MatchingBase found the txn we created", 1, matchingBase.UnmatchedTransactions.Count);
			matchingBase.MoveAllFromUnmatchToMatch();

			var journals = new List<Journal>();
			for (int i = 0; i < numberOfInitialJournals; i++)
			{
				var journal = isAR ? matchingBase.BalancingARJournals.AddNew() : (Journal)matchingBase.BalancingAPJournals.AddNew();
				journal.AH_TransactionCategory = Constants.TransactionCategory.Codes.TransactionAlreadyPaid;
				journal.AH_OH = TestObjectCreator.AALSHI.PK;
				journal.AH_OSExTaxAmount = -100;
				journal.AH_AG = TestObjectCreator.GetGLAccountFromDB(isAR ? "6810.00.00" : "8810.00.00").PK;
				journals.Add(journal);
			}

			foreach (var journal in journals)
			{
				matchingBase.MoveFromUnmatchToMatch(new BusinessObject[] { matchingBase.CopyJournalWithOppositeAmount(journal) });
			}

			matchingBase.MatchAndClearTransactions();
			Factory.Save();

			ZQuery check2JournalsCreatedQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			check2JournalsCreatedQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.AALSHI.PK);
			check2JournalsCreatedQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			var journalsForAALSHI = Factory.Load<TransactionHeader>(check2JournalsCreatedQuery);
			AssertEquals("Precondition: Correct number of journals saved", 2 * numberOfInitialJournals, journalsForAALSHI.Length);

			var unMatchedJournals = journalsForAALSHI.Where(j => j.LatestMatchLink == null).ToList();
			var anUnmatchedJournal = unMatchedJournals[0];
			var matchedJournals = journalsForAALSHI.Where(j => j.LatestMatchLink != null).ToList();
			var aMatchedJournal = matchedJournals[0];

			var matchGroup = aMatchedJournal.LatestMatchLink.AP_MatchGroupNum;

			// Since the lookup for the journal is based on an alternate key, and in our test (with 2 sets of journals) we are 
			// making them the same on purpose, there are two possible journals the message could choose, and it must pick at random since
			// every field is the same except PK and TransactionNum (which are both non-deterministic)
			var acceptableMessages = matchedJournals.Select(j =>
				@"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.

The corresponding journal " + j.AH_TransactionNum + @" is currently matched in match group number " + j.LatestMatchLink.AP_MatchGroupNum + @".
Please review the match group before deciding on the next course of action.

If the Journals are accidentally created, un-match match group number " + j.LatestMatchLink.AP_MatchGroupNum + @" and match off the pair of TAP/TNF journals in a new match session.
If the Journals are overpayment of which a refund is claimed, you can considered matching it off against Receipt/Payment transaction.
If the Journals are overpayment of which no refund is claimed, you can considered matching it off against Overpayment transaction.").ToArray();

			AssertCanReverse(anUnmatchedJournal.PK, false, acceptableMessages);

			AssertCanReverse(aMatchedJournal.PK, false, @"This transaction cannot be reversed because it has been matched with other transactions.");

			UnmatchingRow unmatchRow = new UnmatchingRow(Factory);
			unmatchRow.Initialize(aMatchedJournal.LatestMatchLink);
			unmatchRow.Delete();
			Factory.Save();

			acceptableMessages = matchedJournals.Select(j => @"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.

The corresponding journal " + j.AH_TransactionNum + @" is currently not matched.
If the journals are accidentally created, the pair can be matched off.").ToArray();
			AssertCanReverse(anUnmatchedJournal.PK, false, acceptableMessages);

			acceptableMessages = unMatchedJournals.Select(j => @"This TAP/TNF Journal cannot be reversed.
TAP/TNF journals are created in a pair. Reversing one part will result in an imbalance in the Clearing GL Account.

The corresponding journal " + j.AH_TransactionNum + @" is currently not matched.
If the journals are accidentally created, the pair can be matched off.").ToArray();
			AssertCanReverse(aMatchedJournal.PK, false, acceptableMessages);
		}

		void AssertReadOnlynessDependsOnIsMiscellaneousTransaction(ZPropertyInfo propertyInfo)
		{
			Journal.IsMiscellaneousTransaction = false;
			Assert(propertyInfo.Name + " shouldn't be readonly when IsMiscellaneousTransaction is not set", !propertyInfo.ReadOnly);
			Journal.IsMiscellaneousTransaction = true;
			Assert(propertyInfo.Name + " should be readonly when IsMiscellaneousTransaction is set", propertyInfo.ReadOnly);
		}

		public void TestCheckpointToUnmatch()
		{
			AssertNull("No checkpoint is required to unmatch journal", ((IMiscellaneousTransaction)GetNewBusinessObject()).CheckpointForUnmatch);
		}

		protected Journal Journal
		{
			get { return (Journal)Header; }
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(JournalValidation); }
		}

		public void TestSubAccounts()
		{
			AssertType<JournalSubAccountCollection>(Journal.SubAccounts);
		}

		public void TestSubAccountsOnReload()
		{
			var glHeader = TestObjectCreator.CreateGLHeader();
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(glHeader, GlbStaffSchema.Constants.Prefix, false);

			var journal = (Journal)PrepareTransactionHeaderForTest();
			if (journal.IsMultiSubAccountsSupported)
			{
				journal.AH_AG = glHeader.PK;

				var subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);
				var subAccount2 = GetSubAccount(GlbStaffSchema.Constants.Prefix);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount1.AHS_SubClassParentId);
				AssertEquals("Pre-condition", ZGuid.Empty, subAccount2.AHS_SubClassParentId);

				subAccount1.AHS_SubClassParentId = TestObjectCreator.Creditor1.PK;
				Factory.Save();

				Assert("subAccount1 can be saved in the database", subAccount1.IsInDatabase);
				Assert("subAccount2 should be deleted", subAccount2.IsDeleted);

				var newFactory = Factory.CreateNewFactory();
				journal = (Journal)newFactory.Load(GetExpectedBusinessObjectType(), journal.PK);

				subAccount1 = GetSubAccount(OrgHeaderSchema.Constants.Prefix);

				AssertEquals("sub account count should be 1", 1, journal.SubAccounts.Count);
				AssertEquals("subAccount1 AHS_SubClassParentId", TestObjectCreator.Creditor1.PK, subAccount1.AHS_SubClassParentId);

				JournalSubAccount GetSubAccount(ZString subClassParentTableCode)
				{
					return journal.SubAccounts.Cast<JournalSubAccount>().FirstOrDefault(x => x.AHS_SubClassParentTableCode == subClassParentTableCode);
				}
			}
			else
			{
				AssertEquals("SubAccounts Count", 0, journal.SubAccounts.Count);
			}
		}

		public void TestIsMultiSubAccountsSupported()
		{
			var journal = (Journal)Factory.New(GetExpectedBusinessObjectType());
			AssertEquals(IsExpectMultiSubAccountsSupported, journal.IsMultiSubAccountsSupported);
		}

		protected virtual bool IsExpectMultiSubAccountsSupported => true;

		public void TestSetReadOnlyIncludingChildren()
		{
			var journalType = GetExpectedBusinessObjectType();
			var journal = (Journal)Factory.New(journalType);
			AssertEquals("Pre-condition", false, journal.SubAccounts.ReadOnly);

			Factory.Save();
			AssertEquals("on save", true, journal.SubAccounts.ReadOnly);

			var newFactory = Factory.CreateNewFactory();
			var journal2 = (Journal)newFactory.Load(journalType, journal.PK);

			AssertEquals("on load", true, journal2.SubAccounts.ReadOnly);
		}

		public void TestAlternateGLAccountNumberAndDescription()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader1 = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader1, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, true);

			var alternateGLAccount1 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAttributesForAlteranteGLAccount(alternateGLAccount1, glHeader1, 1, TestObjectCreator.AALSHI.PK.ToGuid(), "TPY", AccountingMasterFilesConstants.LFECodes.LOC, AccountingMasterFilesConstants.LFOCodes.LOC, AccountingMasterFilesConstants.NAV.Code, AccountingMasterFilesConstants.NAV.Code);
			var alternateGLAccount2 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "20.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount2");
			TestObjectCreator.CreateAttributesForAlteranteGLAccount(alternateGLAccount2, glHeader1, 2, TestObjectCreator.ABIGAS.PK.ToGuid(), "INT", AccountingMasterFilesConstants.LFECodes.OEU, AccountingMasterFilesConstants.LFOCodes.FOR, AccountingMasterFilesConstants.NAV.Code, AccountingMasterFilesConstants.NAV.Code);

			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());
			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(TestObjectCreator.NonCurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			Journal.AH_AG = glHeader1.PK;
			Journal.AH_OH = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.AALSHI.OH_IsDebtor = true;
			TestObjectCreator.AALSHI.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";

			AssertEquals("10.00.1000", Journal.AlternateGLAccountNumber);
			AssertEquals("AlternateGLAccount1", Journal.AlternateGLAccountDescription);

			Journal.AH_OH = TestObjectCreator.ABIGAS.PK;
			Journal.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
			TestObjectCreator.ABIGAS.OH_IsDebtor = true;
			TestObjectCreator.ABIGAS.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";

			TestObjectCreator.NonCurrentCompany.GC_RN_NKCountryCode = "CN";
			AssertEquals("20.00.1000", Journal.AlternateGLAccountNumber);
			AssertEquals("AlternateGLAccount2", Journal.AlternateGLAccountDescription);
		}
	}
}
