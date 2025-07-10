using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class DataInterfaceUtilsTest : TestCaseWithFactory
	{
		readonly ZString CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		public void TestTrailingZero()
		{
			ZString account1 = "2340.000";
			ZString account2 = "2340.020";
			ZString account3 = "2340.XXX";
			AssertEquals("2340", DataInterfaceUtils.RemoveTrailingZero(account1));
			AssertEquals("2340.020", DataInterfaceUtils.RemoveTrailingZero(account2));
			AssertEquals("2340.XXX", DataInterfaceUtils.RemoveTrailingZero(account3));
		}

		public void TestGetGLAccountNo()
		{
			AccGLHeader account = Factory.NewWithValidTestData<AccGLHeader>();
			account.AG_AccountNum = "bobs.101";
			Factory.Save();
			AssertEquals("bobs.101", DataInterfaceUtils.GetGLAccountNoWithNoTrailingZero(Factory, account.PK));
		}

		public void TestGetLocalAccountNo()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Taiwan);
			AccGLHeader tWGLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			tWGLAccount.AG_AccountNum = "1111.22.33";
			AccGLAccountDescriptor tWAccountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			tWAccountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			tWAccountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.Taiwan;
			tWAccountDescriptor.AJ_LocalAccountNumber = "9090.909";
			tWAccountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			tWAccountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			tWAccountDescriptor.ParentGLHeaderPK = tWGLAccount.PK;
			Factory.Save();
			AssertEquals("9090.909", DataInterfaceUtils.GetLocalAccountNo(Factory, tWGLAccount.PK));
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1010.22.33";
			AccGLAccountDescriptor accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			accountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			accountDescriptor.AJ_LocalAccountNumber = "1010.101";
			accountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			Factory.Save();
			AssertEquals("1010.101", DataInterfaceUtils.GetLocalAccountNo(Factory, gLAccount.PK));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		public void TestGetLocalAccountNoWithNoTrailingZero()
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1010.33.44";
			AccGLAccountDescriptor accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			accountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			accountDescriptor.AJ_LocalAccountNumber = "1010.000";
			accountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			Factory.Save();
			AssertEquals("1010", DataInterfaceUtils.GetLocalAccountNoWithNoTrailingZero(Factory, gLAccount.PK));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		public void TestGetLocalAccountDescription()
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1010.23.45";
			AccGLAccountDescriptor accountDescriptor = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			accountDescriptor.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			accountDescriptor.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			accountDescriptor.AJ_LocalAccountNumber = "1010.101";
			accountDescriptor.AJ_AccountDescription = "Test";
			accountDescriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			accountDescriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			accountDescriptor.ParentGLHeaderPK = gLAccount.PK;
			Factory.Save();
			AssertEquals("Test", DataInterfaceUtils.GetLocalAccountDescription(Factory, gLAccount.PK));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		public void TestGetVoucherNumberFowWIPAccrual()
		{
			AssertEquals("0406001000", DataInterfaceUtils.GetVoucherNumberFowWIPAccrual(TransactionLineTypes.WIP, 200406));
			AssertEquals("0406001000", DataInterfaceUtils.GetVoucherNumberFowWIPAccrual(TransactionLineTypes.Accrual, 200406));
		}

		public void TestGetYearFromPeriod()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(200407, new ZDateTime(2004, 07, 01), new ZDateTime(2004, 07, 31));
			Factory.Save();
			AssertEquals(2004, DataInterfaceUtils.GetYearFromPeriod(200407, Factory));
		}

		public void TestGetPeriodRange()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);
			periodTestHelper.SetupSinglePeriod(200407, new ZDateTime(2004, 07, 01), new ZDateTime(2004, 07, 31));
			periodTestHelper.SetupSinglePeriod(200408, new ZDateTime(2004, 08, 01), new ZDateTime(2004, 08, 31));
			periodTestHelper.SetupSinglePeriod(200409, new ZDateTime(2004, 09, 01), new ZDateTime(2004, 08, 30));
			Factory.Save();
			AccPeriodManagement[] periods = DataInterfaceUtils.GetPeriodRange(200407, 200408, Factory);
			AssertEquals(2, periods.Length);
		}

		public void TestGetVoucherDescription()
		{
			try
			{
				var headerDescription = "testheader";
				var transactionLineDescription = "testline";
				var jobNumber = "123";

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
				AssertEquals("testheader - testline Job: 123", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, true));
				AssertEquals("testheader - testline", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, false));

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Taiwan);
				AssertEquals("testline Job: 123", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, true));
				AssertEquals("testline", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, false));

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
				AssertEquals("testheader Job: 123", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, true));
				AssertEquals("testheader", DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, false));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
			}
		}
	}
}