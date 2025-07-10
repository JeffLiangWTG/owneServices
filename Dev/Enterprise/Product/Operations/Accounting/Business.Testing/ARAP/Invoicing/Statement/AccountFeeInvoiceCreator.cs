using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AccountFeeInvoiceCreator))]
	public class AccountFeeInvoiceCreatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAccountFeeSettingsIsTakenFromCorrectLevel()
		{
			var org1 = Creator.CreateOrgHeader("org" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);

			var debtorGroup = Creator.CreateDebtorGroup();
			var org2 = Creator.CreateOrgHeader("org" + AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, true, true);
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var org3 = Creator.CreateOrgHeader("org3", true, true);

			CreateINVTransaction(GlbBranch.CurrentBranch, org1, Creator.USD, "INV 001", 200M, 2.0M, new ZDateTime(2015, 07, 01, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org2, Creator.USD, "INV 002", 150M, 2.0M, new ZDateTime(2015, 07, 02, 0, 0, 0));
			CreateCRDTransaction(GlbBranch.CurrentBranch, org3, Creator.USD, "INV 003", 110M, 2.0M, new ZDateTime(2015, 07, 03, 0, 0, 0));
			Factory.Save();

			DynamicBusinessObjectCollection bizos = new DynamicBusinessObjectCollection(Factory);
			bizos.Load(string.Format("SELECT * FROM fnGetAccountFee('{0}', NULL, '01 JUL 2015', '30 JUL 2015')", Env.CurrentCompany.PK));
			AssertEquals("No Rows", 0, bizos.Count);

			SetAccountFeeSettings(org1.CompanyData.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 1500M, "USD", Creator.GLHeader2.PK, null, true, false);
			SetAccountFeeSettings(debtorGroup.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, 1600M, "EUR", Creator.GLHeader1.PK, null, false, true);
			SetAccountFeeSettings(GlbCompany.CurrentCompany.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists, 1800M, "AUD", Creator.GLHeader2.PK, null, false, false);
			Factory.Save();

			bizos = new DynamicBusinessObjectCollection(Factory);
			bizos.Load(string.Format("SELECT * FROM fnGetAccountFee('{0}', NULL, '01 JUL 2015', '30 JUL 2015') WHERE OrgPK IN ('{1}', '{2}', '{3}') ORDER BY AAF_FeeAmount ASC",
				Env.CurrentCompany.PK, org1.PK, org2.PK, org3.PK));
			AssertEquals("3 Rows", 3, bizos.Count);
			AssertAccountFee(bizos[0], Creator.GLHeader2.PK, "USD", AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 1500M);
			AssertAccountFee(bizos[1], Creator.GLHeader1.PK, "EUR", AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, 1600M);
			AssertAccountFee(bizos[2], Creator.GLHeader2.PK, "AUD", AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists, 1800M);
		}

		public void TestAccountFeeSettingsRuleIsAppliedCorrectly()
		{
			var org1 = Creator.CreateOrgHeader("org" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);

			var debtorGroup = Creator.CreateDebtorGroup();
			var org2 = Creator.CreateOrgHeader("org" + AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, true, true);
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			var org3 = Creator.CreateOrgHeader("org3", true, true);

			SetAccountFeeSettings(org1.CompanyData.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists, 1500M, "USD", Creator.GLHeader2.PK, null, true, false);
			SetAccountFeeSettings(debtorGroup.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, 1600M, "EUR", Creator.GLHeader1.PK, null, false, true);
			SetAccountFeeSettings(GlbCompany.CurrentCompany.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 1800M, "AUD", Creator.GLHeader2.PK, null, false, false);

			CreateINVTransaction(GlbBranch.CurrentBranch, org1, Creator.USD, "INV 001", 200M, 2.0M, new ZDateTime(2015, 07, 01, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org2, Creator.USD, "INV 002", 150M, 2.0M, new ZDateTime(2015, 07, 02, 0, 0, 0));
			var receipt = Creator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100M, Creator.AUDBankAccount.PK);
			receipt.AH_OH = org3.PK;
			receipt.AH_PostDate = new ZDateTime(2015, 07, 12, 0, 0, 0);

			var payment = Creator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsReceivable, -200M, Creator.AUDBankAccount.PK);
			payment.AH_OH = org3.PK;
			payment.AH_PostDate = new ZDateTime(2015, 07, 14, 0, 0, 0);

			Factory.Save();
			GlbCompany.CurrentCompany.Factory.Save();

			InitializeDummyBizo();
			dummyBizO.DoAccountFeeTransaction = true;
			dummyBizO.AccountFeeInvoiceCreator.AccFeeFromDate = new ZDateTime(2015, 07, 01, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.AccFeeToDate = new ZDateTime(2015, 07, 15, 0, 0, 0);
			DynamicBusinessObjectCollection bizos = dummyBizO.AccountFeeInvoiceCreator.GetAccountFeeSettingsFromDatabase();

			AssertEquals("Rows", 2, bizos.Count);
			AssertAccountFee(bizos, org1.PK, true, Creator.GLHeader2.PK, "USD", AccAccountFee.AccountFeeCalculationRuleType.WhenEitherTransactionPostedOrOutstandingBalanceExists, 1500M);
			AssertAccountFee(bizos, org2.PK, true, Creator.GLHeader1.PK, "EUR", AccAccountFee.AccountFeeCalculationRuleType.WhenOutstandingBalacneExists, 1600M);
			AssertEquals("Org3 Shouldn't Exists", false, bizos.Any(x => new ZGuid(x["OrgPK"]) == org3.PK));

			CreateINVTransaction(GlbBranch.CurrentBranch, org3, Creator.USD, "INV 001", 200M, 2.0M, new ZDateTime(2015, 07, 05, 0, 0, 0));
			Factory.Save();

			bizos = dummyBizO.AccountFeeInvoiceCreator.GetAccountFeeSettingsFromDatabase();
			AssertAccountFee(bizos, org3.PK, true, Creator.GLHeader2.PK, "AUD", AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 1800M);

			payment = Creator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsReceivable, 300M, Creator.AUDBankAccount.PK);
			payment.AH_OH = org2.PK;
			payment.AH_PostDate = new ZDateTime(2015, 07, 14, 0, 0, 0);

			receipt = Creator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Payment, LedgerTypes.AccountsReceivable, -500M, Creator.AUDBankAccount.PK);
			receipt.AH_OH = org2.PK;
			receipt.AH_PostDate = new ZDateTime(2015, 07, 20, 0, 0, 0);

			Factory.Save();

			bizos = dummyBizO.AccountFeeInvoiceCreator.GetAccountFeeSettingsFromDatabase();
			AssertEquals("Org2 Shouldn't Exists", false, bizos.Any(x => new ZGuid(x["OrgPK"]) == org2.PK));
		}

		public void TestAccountFeeInvoiceDescriptionLength()
		{
			InitializeDummyBizo();
			AssertEquals("Max Length should be 128", AccTransactionHeaderSchema.AH_Desc.MaxLength, dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDescriptionInfo.MaxLength);
		}

		public void TestDefaultValueofAccFeeTaxID()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.AccountFeeDefaultTaxID.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				AccountingConfigurationRegistry.Instance.AccountFeeDefaultTaxID.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty);
				InitializeDummyBizo();
				AssertEquals("Acc Fee tax ID", Guid.Empty, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID);

				AccountingConfigurationRegistry.Instance.AccountFeeDefaultTaxID.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Creator.SVAT1.PK.ToGuid());
				InitializeDummyBizo();
				AssertEquals("Acc Fee tax ID", Creator.SVAT1.PK, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.AccountFeeDefaultTaxID.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestAccFeeTaxIDReadonly()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var prevValueOfIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				InitializeDummyBizo();

				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

				dummyBizO.DoAccountFeeTransaction = false;
				AssertEquals("Acc Fee tax ID: ReadOnly", true, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);
				dummyBizO.DoAccountFeeTransaction = true;
				AssertEquals("Acc Fee tax ID: ReadOnly", false, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				AssertEquals("Acc Fee tax ID: ReadOnly", true, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				AssertEquals("Acc Fee tax ID: ReadOnly", false, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);

				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				AssertEquals("Acc Fee tax ID: ReadOnly", true, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AssertEquals("Acc Fee tax ID: ReadOnly", false, dummyBizO.AccountFeeInvoiceCreator.AccFeeTaxID_ReadOnly);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevValueOfIsGSTRegistered;
				AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestAccountFeeInvoiceCreation()
		{
			Setup();

			var transactionOrg1 = Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, org1.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var transactionOrg2 = Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, org2.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			var transactionOrg3 = Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, org3.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertEquals(0, transactionOrg1.Length);
			AssertEquals(0, transactionOrg2.Length);
			AssertEquals(0, transactionOrg3.Length);

			//Create few Transactions
			CreateINVTransaction(GlbBranch.CurrentBranch, org3, Creator.USD, "INV 001", 200M, 2.0M, new ZDateTime(2015, 07, 05, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org2, Creator.USD, "INV 002", 250M, 2.0M, new ZDateTime(2015, 07, 06, 0, 0, 0));
			CreateINVTransaction(GlbBranch.CurrentBranch, org1, Creator.USD, "INV 003", 350M, 2.0M, new ZDateTime(2015, 07, 07, 0, 0, 0));
			Factory.Save();

			var qry = new ZQuery(AccTransactionHeaderSchema.AH_PostDate, new ZDateTime(2015, 07, 13, 0, 0, 0));
			qry.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, new ZDateTime(2015, 07, 25, 0, 0, 0));
			qry.AddToFilter(AccTransactionHeaderSchema.AH_OH, new ZGuid[] { org1.PK, org2.PK, org3.PK });
			qry.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, "INV");
			qry.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			dummyBizO.DoAccountFeeTransaction = false;
			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			var accFeeInvoices = Factory.Load<ARInvoice>(qry);
			Assert(accFeeInvoices == null || accFeeInvoices.Length == 0);

			dummyBizO.DoAccountFeeTransaction = true;
			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			accFeeInvoices = Factory.Load<ARInvoice>(qry);
			Assert(accFeeInvoices != null && accFeeInvoices.Length == 2);

			transactionOrg1 = accFeeInvoices.Where(x => x.AH_OH == org1.PK).ToArray();
			transactionOrg2 = accFeeInvoices.Where(x => x.AH_OH == org2.PK).ToArray();

			Assert(transactionOrg1 != null && transactionOrg1.Length == 1);
			AssertInvoiceInfo(transactionOrg1[0], org1.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "AUD", 1.0m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 13, 0, 0, 0), Creator.GLHeader1.PK, 250m);

			Assert(transactionOrg2 != null && transactionOrg2.Length == 1);
			AssertInvoiceInfo(transactionOrg2[0], org2.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "USD", 0.75m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 13, 0, 0, 0), Creator.GLHeader1.PK, 350m);

			AssertEquals(false, accFeeInvoices.Where(x => x.AH_OH == org3.PK).Any());

			var rate = newCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "SEL";
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(30);
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			rate.RE_RX_NKExCurrency = "TST";
			rate.RE_SellRate = 0.85m;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-7);
			Factory.Save();

			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			accFeeInvoices = Factory.Load<ARInvoice>(qry);
			Assert(accFeeInvoices != null && accFeeInvoices.Length == 3);

			transactionOrg1 = accFeeInvoices.Where(x => x.AH_OH == org1.PK).ToArray();
			transactionOrg2 = accFeeInvoices.Where(x => x.AH_OH == org2.PK).ToArray();
			transactionOrg3 = accFeeInvoices.Where(x => x.AH_OH == org3.PK).ToArray();

			Assert(transactionOrg1 != null && transactionOrg1.Length == 1);
			Assert(transactionOrg2 != null && transactionOrg2.Length == 1);
			Assert(transactionOrg3 != null && transactionOrg3.Length == 1);
			AssertInvoiceInfo(transactionOrg3[0], org3.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "TST", 0.85m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 13, 0, 0, 0), Creator.GLHeader2.PK, 450m);
		}

		public void TestDuplicateCheckProcessDiscardsReversedInvoice()
		{
			Setup();

			var transactionOrg1 = Factory.Load<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, org1.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals(0, transactionOrg1.Length);

			//Create few Transactions
			var transaction = CreateINVTransaction(GlbBranch.CurrentBranch, org1, Creator.AUD, "INV 003", 350M, 1.0M, new ZDateTime(2015, 07, 07, 0, 0, 0));
			Factory.Save();

			var qry = new ZQuery(AccTransactionHeaderSchema.AH_PostDate, new ZDateTime(2015, 07, 13, 0, 0, 0));
			qry.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDate);
			qry.AddToFilter(AccTransactionHeaderSchema.AH_OH, new ZGuid[] { org1.PK });
			qry.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, "INV");
			qry.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			//Create First account fee Invoice
			dummyBizO.DoAccountFeeTransaction = true;
			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			var accFeeInvoices = Factory.Load<ARInvoice>(qry);
			Assert("One Account Fee Invoice should be created", accFeeInvoices != null && accFeeInvoices.Length == 1);

			transactionOrg1 = accFeeInvoices.Where(x => x.AH_OH == org1.PK).ToArray();
			Assert("Org1: Debtor of Account Fee Invoice", transactionOrg1 != null && transactionOrg1.Length == 1);
			AssertInvoiceInfo(transactionOrg1[0], org1.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "AUD", 1.0m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 13, 0, 0, 0), Creator.GLHeader1.PK, 250m);

			//Now try to create the account Fee Invoice again with new Info
			dummyBizO.AccountFeeInvoiceCreator.AccFeePostDate = new ZDateTime(2015, 07, 12, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			accFeeInvoices = Factory.Load<ARInvoice>(qry);
			Assert("Only One Account Fee Invoice should Exist. No new Account Fee should be created", accFeeInvoices != null && accFeeInvoices.Length == 1);

			transactionOrg1 = accFeeInvoices.Where(x => x.AH_OH == org1.PK).ToArray();
			Assert("Org1: Debtor of Account Fee Invoice", transactionOrg1 != null && transactionOrg1.Length == 1);
			AssertNotEquals("New Post Date [Post Date should be the post date of old Account Fee Invoice. Not the new One]", new ZDateTime(2015, 07, 12, 0, 0, 0), transactionOrg1[0].AH_PostDate);
			AssertEquals("Old Post Date [Post Date should be the post  date of old Account Fee Invoice. Not the new One]", new ZDateTime(2015, 07, 13, 0, 0, 0), transactionOrg1[0].AH_PostDate);
			AssertInvoiceInfo(transactionOrg1[0], org1.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "AUD", 1.0m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 13, 0, 0, 0), Creator.GLHeader1.PK, 250m);

			//Now Reverse the created account fee
			var reversing = new ARInvoiceReversing(transactionOrg1[0]);
			reversing.Reverse();
			Factory.Save();

			//Now try to create the account Fee Invoice again with new Info
			dummyBizO.AccountFeeInvoiceCreator.AccFeePostDate = new ZDateTime(2015, 07, 11, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.CreateAndSaveAccountFeeInvoices();

			qry = new ZQuery(AccTransactionHeaderSchema.AH_PostDate, new ZDateTime(2015, 07, 11, 0, 0, 0));
			qry.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDate);
			qry.AddToFilter(AccTransactionHeaderSchema.AH_OH, new ZGuid[] { org1.PK });
			qry.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, "INV");
			qry.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			accFeeInvoices = Factory.Load<ARInvoice>(qry);

			Assert("One new Account Fee", accFeeInvoices != null && accFeeInvoices.Length == 1);

			transactionOrg1 = accFeeInvoices.ToArray();
			AssertEquals("New Post Date [Post Date should be the post date of New Account Fee Invoice. Not the Old One]", new ZDateTime(2015, 07, 11, 0, 0, 0), transactionOrg1[0].AH_PostDate);
			AssertNotEquals("Old Post Date [Post Date should be the post date of New Account Fee Invoice. Not the Old One]", new ZDateTime(2015, 07, 13, 0, 0, 0), transactionOrg1[0].AH_PostDate);
			AssertInvoiceInfo(transactionOrg1[0], org1.PK, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, "AUD", 1.0m, new ZDateTime(2015, 07, 25, 0, 0, 0), new ZDateTime(2015, 07, 11, 0, 0, 0), Creator.GLHeader1.PK, 250m);
		}

		public void TestEmptyDateValidattion()
		{
			Setup();
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;

			dummyBizO.AccountFeeInvoiceCreator.ValidateAccFeeInvoiceDate();
			dummyBizO.AccountFeeInvoiceCreator.ValidateAccFeePostDate();
			AssertEquals("Precondition: Invoice date is not empty", false, dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDate.IsEmpty);
			AssertNoErrors("Should not have any error", dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDateInfo);

			AssertEquals("Precondition: Post date is not empty", false, dummyBizO.AccountFeeInvoiceCreator.AccFeePostDate.IsEmpty);
			AssertNoErrors("Should not have any error", dummyBizO.AccountFeeInvoiceCreator.AccFeePostDateInfo);

			dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDate = ZDateTime.Empty;
			dummyBizO.AccountFeeInvoiceCreator.AccFeePostDate = ZDateTime.Empty;
			dummyBizO.AccountFeeInvoiceCreator.ValidateAccFeeInvoiceDate();
			dummyBizO.AccountFeeInvoiceCreator.ValidateAccFeePostDate();

			AssertHasErrors("Should have error as empty invoice date is not allowed", dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDateInfo);
			AssertHasErrors("Should have error as empty post date is not allowed", dummyBizO.AccountFeeInvoiceCreator.AccFeePostDateInfo);
		}

		#region Assert Functions

		void AssertInvoiceInfo(ARInvoice transaction, ZGuid orgPK, ZGuid companyPK, ZGuid branchPK, ZString currency, ZDecimal exchangeRate, ZDateTime invoiceDate, ZDateTime postDate,
								ZGuid gLAccount, ZDecimal amount)
		{
			AssertEquals(AccTransactionHeaderSchema.AH_OH.Name, orgPK, transaction.AH_OH);
			AssertEquals(AccTransactionHeaderSchema.AH_GC.Name, companyPK, transaction.AH_GC);
			AssertEquals(AccTransactionHeaderSchema.AH_GB.Name, branchPK, transaction.AH_GB);
			AssertEquals(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency.Name, currency, transaction.AH_RX_NKTransactionCurrency);
			AssertEquals(AccTransactionHeaderSchema.AH_ExchangeRate.Name, exchangeRate, Utilities.Round(transaction.AH_ExchangeRate, 2));
			AssertEquals(AccTransactionHeaderSchema.AH_InvoiceDate.Name, invoiceDate, transaction.AH_InvoiceDate);
			AssertEquals(AccTransactionHeaderSchema.AH_PostDate.Name, postDate, transaction.AH_PostDate);
			AssertEquals(AccTransactionLinesSchema.AL_AG.Name, gLAccount, transaction.Lines[0].AL_AG);
			AssertEquals(AccTransactionLinesSchema.AL_RX_NKTransactionCurrency.Name, currency, transaction.Lines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(AccTransactionLinesSchema.AL_ExchangeRate.Name, exchangeRate, Utilities.Round(transaction.Lines[0].AL_ExchangeRate, 2));
			AssertEquals(AccTransactionLinesSchema.AL_OSAmount.Name, amount, transaction.Lines[0].AL_OSExTaxAmount);
		}

		void AssertAccountFee(DynamicBusinessObjectCollection bizos, ZGuid orgPK, ZBool expectedGenerateAccFee, ZGuid expectedGLAccount, ZString expectedCurrency, ZString expectedRule, ZDecimal expectedFeeAmount)
		{
			if (bizos != null)
			{
				var bizo = bizos.Where(x => new ZGuid(x["OrgPK"]) == orgPK).ToArray();
				Assert(bizo != null && bizo.Length == 1);
				AssertAccountFee(bizo[0], expectedGLAccount, expectedCurrency, expectedRule, expectedFeeAmount);
				AssertEquals("Generate Account Fee", expectedGenerateAccFee, (new ZInt(bizo[0]["GenerateAccountFee"]) == 1));
			}
		}

		void AssertAccountFee(DynamicBusinessObject bizo, ZGuid expectedGLAccount, ZString expectedCurrency, ZString expectedRule, ZDecimal expectedFeeAmount)
		{
			AssertEquals("GL Account", expectedGLAccount, new ZGuid(bizo["AAF_AG_GLAccount"]));
			AssertEquals("Currency", expectedCurrency, new ZString(bizo["AAF_RX_NKFeeCurrency"]));
			AssertEquals("Rule", expectedRule, new ZString(bizo["AAF_Rule"]));
			AssertEquals("Fee Amount", expectedFeeAmount, new ZDecimal(bizo["AAF_FeeAmount"]));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var supporter = new DummyAccountFeeInvoiceSupportable(GlbBranch.CurrentBranch);
			return new AccountFeeInvoiceCreator(supporter);
		}

		void Setup(bool setAccountFeeAtCompanyLevel = true)
		{
			SetupPeriod(2016);

			org1 = Creator.CreateOrgHeader("ORG_" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);

			debtorGroup = Creator.CreateDebtorGroup();
			org2 = Creator.CreateOrgHeader("ORG2_" + AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, true, true);
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;

			org3 = Creator.CreateOrgHeader("ORG3_", true, true);
			newCurrency = Factory.New<RefCurrency>();
			newCurrency.RX_Code = "TST";
			newCurrency.RX_Desc = "TEST CURRENCY";
			newCurrency.RX_SubUnitRatio = 2;

			SetAccountFeeSettings(org1.CompanyData.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 250m, "AUD", Creator.GLHeader1.PK, GlbCompany.CurrentCompany.PK, true, false);
			SetAccountFeeSettings(debtorGroup.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 350m, "USD", Creator.GLHeader1.PK, GlbCompany.CurrentCompany.PK, false, true);
			if (setAccountFeeAtCompanyLevel)
			{
				SetAccountFeeSettings(org3.PK, AccAccountFee.AccountFeeCalculationRuleType.WhenTransactionPosted, 450m, newCurrency.RX_Code, Creator.GLHeader2.PK, GlbCompany.CurrentCompany.PK, false, false);
			}

			var rate = Creator.USD.ExchangeRates.AddNew();
			rate.RE_ExRateType = "SEL";
			rate.RE_ExpiryDate = ZDateTime.Today.AddDays(30);
			rate.RE_GC = GlbCompany.CurrentCompany.PK;
			rate.RE_RX_NKExCurrency = "USD";
			rate.RE_SellRate = 0.75m;
			rate.RE_StartDate = ZDateTime.Today.AddDays(-7);

			Factory.Save();
			GlbCompany.CurrentCompany.Factory.Save();

			InitializeDummyBizo();
		}

		void SetupPeriod(int year)
		{
			if (PeriodManagementTestHelper == null)
			{
				PeriodManagementTestHelper = new AccountingPeriodTestHelper();
			}
			PeriodManagementTestHelper.PostPeriodsForEntireYear(year);
		}
		AccountingPeriodTestHelper PeriodManagementTestHelper;

		void InitializeDummyBizo()
		{
			dummyBizO = new DummyAccountFeeInvoiceSupportable(GlbBranch.CurrentBranch);
			dummyBizO.DoAccountFeeTransaction = true;

			dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDate = new ZDateTime(2015, 07, 25, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.AccFeePostDate = new ZDateTime(2015, 07, 13, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.AccFeeInvoiceDescription = "Acc Fee Invoice Header";
			dummyBizO.AccountFeeInvoiceCreator.AccFeeFromDate = new ZDateTime(2015, 07, 01, 0, 0, 0);
			dummyBizO.AccountFeeInvoiceCreator.AccFeeToDate = new ZDateTime(2015, 07, 15, 0, 0, 0);
		}

		InvoicingBase CreateINVTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString invoiceNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNumber, currency, exchangeRate, invoiceAmount * exchangeRate, 0M, invoiceAmount, 0M, debtor, Creator.FRT.PK, postDate, postDate.AddDays(-2), postDate.AddDays(-5), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;
			transaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			transaction.AH_PostDate = postDate;
			transaction.AH_TransactionNum = invoiceNumber;

			return transaction;
		}

		ARCreditNote CreateCRDTransaction(GlbBranch branch, OrgHeader debtor, RefCurrency currency, ZString cRDNumber, ZDecimal invoiceAmount, ZDecimal exchangeRate, ZDateTime postDate)
		{
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			var newJob = Creator.CreateJob(debtor, 0.0M, Creator.Agent, 0.0M);
			var transaction = Creator.CreateARCreditNoteWithLine(cRDNumber, debtor, Creator.USD, exchangeRate, "Credit Note Line 1", newJob, Creator.FRT, invoiceAmount, new ZDateTime(2015, 02, 12, 0, 0, 0), false);
			var jobCharge = Creator.CreateJobCharge(transaction.Lines[0], newJob, Creator.FRT, currency);

			transaction.Lines[0].AL_JH = transaction.AH_JH = newJob.PK;
			newJob.JH_GC = transaction.AH_GC = transaction.Lines[0].AL_GC = branch.GB_GC;
			newJob.JH_GB = jobCharge.JR_GB = transaction.AH_GB = transaction.Lines[0].AL_GB = branch.PK;

			transaction.AH_RX_NKTransactionCurrency = currency.RX_Code;
			transaction.AH_FullyPaidDate = ZDateTime.Empty;
			transaction.AH_TransactionNum = cRDNumber;
			transaction.AH_PostDate = postDate;

			return transaction;
		}

		void SetAccountFeeSettings(ZGuid parentObjectPK, ZString ruleType, ZDecimal amount, ZString currencyNK, ZGuid gLPK, ZGuid? companyPK, bool isOrgLevel, bool isDebtorLevel)
		{
			var accountFee = Factory.New<AccAccountFee>();
			accountFee.AAF_AG_GLAccount = gLPK;
			accountFee.AAF_FeeAmount = amount;
			accountFee.AAF_RX_NKFeeCurrency = currencyNK;
			accountFee.AAF_GC_Company = companyPK ?? new ZGuid(Env.CurrentCompany.PK);
			if (isOrgLevel)
			{
				accountFee.AAF_OB_CompanyData = parentObjectPK;
			}
			else if (isDebtorLevel)
			{
				accountFee.AAF_OJ_DebtorGroup = parentObjectPK;
			}
			accountFee.AAF_Rule = ruleType;
		}

		TestObjectCreator Creator
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
		TestObjectCreator fTestObjectCreator;

		DummyAccountFeeInvoiceSupportable dummyBizO;
		OrgHeader org1, org2, org3;
		OrgDebtorGroup debtorGroup;
		RefCurrency newCurrency;

		#endregion

	}

	public class DummyAccountFeeInvoiceSupportable : NonPersistentBusinessObject, IAccountFeeInvoiceSupportable
	{
		public DummyAccountFeeInvoiceSupportable(GlbBranch branch)
			: base()
		{
			this.branch = branch;
		}

		readonly GlbBranch branch;

		public bool DoAccountFeeTransaction
		{
			get;
			set;
		}

		public AccountFeeInvoiceCreator AccountFeeInvoiceCreator
		{
			get
			{
				if (accountFeeInvoiceCreator == null)
				{
					accountFeeInvoiceCreator = new AccountFeeInvoiceCreator(this);
				}
				return accountFeeInvoiceCreator;
			}
		}

		AccountFeeInvoiceCreator accountFeeInvoiceCreator;

		#region IAccountFeeInvoiceSupportable

		public string GetOrgPKQueryForAccountFee(ZSqlParameterCollection sqlParams)
		{
			return string.Empty;
		}

		public GlbCompany Company
		{
			get { return branch.Company; }
		}

		public GlbBranch Branch
		{
			get { return branch; }
		}

		public bool CreateAccountFee
		{
			get { return DoAccountFeeTransaction; }
		}

		#endregion
	}
}
