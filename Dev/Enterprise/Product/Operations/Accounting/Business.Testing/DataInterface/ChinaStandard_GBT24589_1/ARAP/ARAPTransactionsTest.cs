using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(ARAPTransactions))]
	public class ARAPTransactionsTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestClassProperties()
		{
			AssertEquals("T303", ARAPTransactions.LocID);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31));
			AccGLHeader aPControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLAccountDescriptor aRControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aRControlLocal.ParentGLHeaderPK = aRControl.PK;
			aRControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			aRControlLocal.AJ_AccountDescription = "ARControlDescription";
			aRControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccGLAccountDescriptor aPControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aPControlLocal.ParentGLHeaderPK = aPControl.PK;
			aPControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			aPControlLocal.AJ_AccountDescription = "APControlDescription";
			aPControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControl.PK.ToGuid());
			ARInvoice testARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testARInvoice.AH_TransactionNum = "100111";
			testARInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
			//
			testARInvoice.AH_PostDate = new ZDateTime(2006, 3, 13);
			testARInvoice.AH_DueDate = new ZDateTime(2006, 3, 18);
			testARInvoice.AH_TransactionReference = "Invoice No";
			testARInvoice.AH_ExchangeRate = 1m;
			testARInvoice.AH_InvoiceAmount = 111m;
			testARInvoice.AH_OutstandingAmount = 111m;
			testARInvoice.AH_Desc = "Invoice Desc";
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.Australia));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "澳元");
			Factory.Save();
			ARAPTransactionsCollection collection = new ARAPTransactionsCollection(Factory);
			collection.BuildTransactions(LedgerTypes.AccountsReceivable, new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AssertEquals(1, collection.Count);
			ARAPTransactions arTransactions = collection[0];
			AssertEquals(arTransactions.ClientCode, "AALSHI");
			AssertEquals(arTransactions.GLAccountNumber, "");
			AssertEquals(arTransactions.VoucherDate, "20060313");
			AssertEquals(arTransactions.EnteredDate, "20060329");
			AssertEquals(arTransactions.FinancialYear, 2006);
			AssertEquals(arTransactions.Period, 200603);
			AssertEquals(arTransactions.VoucherTypeNumber, "1");
			AssertEquals(arTransactions.VoucherNumber, "00001000");
			AssertEquals(arTransactions.BaseCurrency, "澳元");
			AssertEquals(arTransactions.TransactionCurrency, "澳元");
			AssertEquals(arTransactions.ExRate, 1m);
			AssertEquals(arTransactions.CRDR, "平");
			AssertEquals(arTransactions.LocalBalance, 0m);
			AssertEquals(arTransactions.LocalTransactionAmount, 0m);
			AssertEquals(arTransactions.OSTransactionAmount, 0m);
			AssertEquals(arTransactions.OSBalance, 0m);
			AssertEquals(arTransactions.OSTransactionAmount, 0m);
			AssertEquals(arTransactions.Description, "Invoice Desc");
			AssertEquals(arTransactions.DueDate, "20060318");
			AssertEquals(arTransactions.VerifyMatchVoucherNumber, "00001000");
			AssertEquals(arTransactions.VerifyMatchDate, "20060313");
			AssertEquals(arTransactions.BillsTypeCode, "D01");
			AssertEquals(arTransactions.TransactionTypeCode, "ARINV");
			AssertEquals(arTransactions.BillsNumber, "00001000");
			AssertEquals(arTransactions.InvoiceNumber, "Invoice No");
			AssertEquals(arTransactions.ContractNumber, ZString.Empty);
			AssertEquals(arTransactions.ItemCode, ZString.Empty);
			AssertEquals(arTransactions.PaymentTypeCode, "");
			AssertEquals(arTransactions.VerifyMatchFlag, "1");
			AssertEquals(arTransactions.RemittanceDraftNumber, "");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARAPTransactions();
		}
	}

	[TestedType(typeof(ARAPTransactionsCollection))]
	public class ARAPTransactionsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ARAPTransactionsCollection>
	{
		public void TestBuildTransactions()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31));
			AccGLHeader aPControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLAccountDescriptor aRControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aRControlLocal.ParentGLHeaderPK = aRControl.PK;
			aRControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			aRControlLocal.AJ_AccountDescription = "ARControlDescription";
			aRControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccGLAccountDescriptor aPControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aPControlLocal.ParentGLHeaderPK = aPControl.PK;
			aPControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			aPControlLocal.AJ_AccountDescription = "APControlDescription";
			aPControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControl.PK.ToGuid());
			ARInvoice testARInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARCreditNote testARCreditNote = Factory.NewWithValidTestData(typeof(ARCreditNote)) as ARCreditNote;
			APCreditNote testAPCreditNote = Factory.NewWithValidTestData(typeof(APCreditNote)) as APCreditNote;
			ARAdjustmentNote testARAdjustmentNote = Factory.NewWithValidTestData(typeof(ARAdjustmentNote)) as ARAdjustmentNote;
			APAdjustmentNote testAPAdjustmentNote = Factory.NewWithValidTestData(typeof(APAdjustmentNote)) as APAdjustmentNote;
			ARJournal testARJournal = Factory.NewWithValidTestData(typeof(ARJournal)) as ARJournal;
			ARReceipt testARReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			ARPayment testARPayment = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			APJournal testAPJournal = Factory.NewWithValidTestData(typeof(APJournal)) as APJournal;
			APReceipt testAPReceipt = Factory.NewWithValidTestData(typeof(APReceipt)) as APReceipt;
			APPayment testAPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			Contra testContra = Contra.New(Factory);
			testContra.AH_ARAccount = FromAccount.PK;
			testContra.AH_APAccount = ToAccount.PK;
			testContra.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARInvoice.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPInvocie.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARCreditNote.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPCreditNote.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARAdjustmentNote.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPAdjustmentNote.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARJournal.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARReceipt.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testARPayment.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPJournal.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPReceipt.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			testAPPayment.AH_PostDate = new ZDateTime(2006, 3, 30, 23, 59, 00);
			Factory.Save();
			ARAPTransactionsCollection collection = new ARAPTransactionsCollection(Factory);
			AssertEquals(0, collection.Count);
			collection.BuildTransactions(LedgerTypes.AccountsReceivable, new ZDateTime(2007, 3, 1, 23, 59, 00), new ZDateTime(2007, 3, 31, 23, 59, 00));
			AssertEquals(0, collection.Count); // test date Filter
			collection.BuildTransactions(LedgerTypes.General, new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AssertEquals(0, collection.Count); // test Ledger Type Filter
			collection.BuildTransactions(LedgerTypes.AccountsReceivable, new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AssertEquals(7, collection.Count);
			collection.RemoveAndDeleteAll();
			AssertEquals(0, collection.Count);
			collection.BuildTransactions(LedgerTypes.AccountsPayable, new ZDateTime(2006, 3, 1, 23, 59, 00), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AssertEquals(7, collection.Count);
		}

		OrgHeader fFromAccount;
		protected OrgHeader FromAccount
		{
			get
			{
				if (fFromAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(fromAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.PK, ToAccount.PK);
					fFromAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fFromAccount;
			}
		}

		OrgHeader fToAccount;
		protected OrgHeader ToAccount
		{
			get
			{
				if (fToAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery toAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(toAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					fToAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fToAccount;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ARAPTransactions();
		}

		protected override ARAPTransactionsCollection GetCollectionToTest()
		{
			return new ARAPTransactionsCollection(Factory);
		}
	}
}
