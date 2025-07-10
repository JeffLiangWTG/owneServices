using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocChinaJournalListing))]
	sealed class DocChinaJournalListingTest : DocumentWrapperTestCase
	{
		public void TestDocChinaJournalListingWrappedObject()
		{
			DocChinaJournalListing docChinaJournalListing = DocChinaJournalListing.New(TestChinaJournalListing, Factory);
			AssertEquals("Wrapped Object should be TestChinaJournalListing", TestChinaJournalListing, docChinaJournalListing.WrappedObject);
		}

		public void TestPeriod()
		{
			DocChinaJournalListing docChinaJournalListing = DocChinaJournalListing.New(TestChinaJournalListing, Factory);
			AssertEquals("Period should be 200401", 200401, docChinaJournalListing.Period);
		}

		public void TestProperties()
		{
			DocChinaJournalListing docChinaJournalListing = DocChinaJournalListing.New(TestChinaJournalListing, Factory);
			AssertEquals("Period should be 2004-01-01", new ZDateTime(2004, 1, 1), docChinaJournalListing.FromDate);
			AssertEquals("Period should be 2004-01-31", new ZDateTime(2004, 1, 31), docChinaJournalListing.EndDate);
			AssertEquals("Period should be empty", "", docChinaJournalListing.Branch);
		}

		public void TestVoucherLinesCount()
		{
			var apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			var arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

			var arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			arControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			arControlLocal.ParentGLHeaderPK = arControl.PK;
			arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
			arControlLocal.AJ_AccountDescription = "ARControlDescription";

			var apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			apControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			apControlLocal.ParentGLHeaderPK = apControl.PK;
			apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
			apControlLocal.AJ_AccountDescription = "APControlDescription";
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());

			AssertEquals(2, TestWrapper.VoucherLines.Count);
		}

		#region Implementation

		ChinaJournalListing fTestChinaJournalListing;

		ChinaJournalListing TestChinaJournalListing
		{
			get
			{
				return fTestChinaJournalListing ??
						(fTestChinaJournalListing =
							new ChinaJournalListing(Factory, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31), ""));
			}
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocChinaJournalListing.New(TestChinaJournalListing, Factory) };
		}

		DocChinaJournalListing fTestWrapper;

		DocChinaJournalListing TestWrapper
		{
			get { return fTestWrapper ?? (fTestWrapper = DocChinaJournalListing.New(TestChinaJournalListing, Factory)); }
		}

		protected override void SetUp()
		{
			OriginalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;

			var testPeriod = 200401;
			var testDate = new ZDateTime(2004, 1, 15);
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(testPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1111.11.11";
			SetupAccountDescriptor(gLAccount);

			var testTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testTransaction.AH_AG = gLAccount.PK;
			testTransaction.AH_PostDate = testDate;
			testTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			testTransaction.AH_TransactionType = TransactionTypes.Invoice;

			testTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testTransaction.AH_AG = gLAccount.PK;
			testTransaction.AH_PostDate = new ZDateTime(2004, 1, 10);
			testTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			testTransaction.AH_TransactionType = TransactionTypes.Invoice;

			testTransaction = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testTransaction.AH_AG = gLAccount.PK;
			testTransaction.AH_PostDate = new ZDateTime(2004, 1, 12);
			testTransaction.AH_Ledger = LedgerTypes.General;
			testTransaction.AH_TransactionType = TransactionTypes.Journal;

			base.SetUp();
		}

		ZString OriginalCountryCode;
		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(OriginalCountryCode);
			base.TearDown();
		}

		AccGLAccountDescriptor SetupAccountDescriptor(AccGLHeader gLAccount)
		{
			AccGLAccountDescriptor accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			accountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			accountDescriptor.AJ_LocalAccountNumber = "1010.101";
			accountDescriptor.AJ_AccountDescription = "My Description";
			accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			accountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			Factory.Save();
			return accountDescriptor;
		}

		#endregion
	}
}
