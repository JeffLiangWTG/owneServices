using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(VoucherLine))]
	public class VoucherLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOutstandingAmount()
		{
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.OutstandingAmount = 11m;
			AssertEquals(11m, lineToTest.OutstandingAmount);
		}

		public void TestOrganisationCode()
		{
			VoucherLine lineToTest = new VoucherLine(Factory);
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			lineToTest.OrganisationCode = testObjectCreator.AALSHI.OH_Code;
			AssertEquals(testObjectCreator.AALSHI.OH_Code, lineToTest.OrganisationCode);
		}

		public void TestToString()
		{
			AccGLHeader gLAccount = SetGLAccount();
			SetAccountDescriptor(gLAccount);
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = gLAccount.PK;
			lineToTest.VoucherDate = new ZDateTime(2004, 11, 30, 1, 1, 1);
			lineToTest.DebitAmount = 120.0m;
			lineToTest.CreditAmount = 0.0m;
			lineToTest.VoucherType = "AR-INV";
			lineToTest.VoucherNumber = "310029";
			lineToTest.Description = "AR Invoice";
			lineToTest.AttachmentCount = 2;
			string expectedValue = DataInterfaceConstant.Quote + "1010.101" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + "2004-11-30" + DataInterfaceConstant.Tab + "120.00" + DataInterfaceConstant.Tab + "0.00" + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "AR-INV" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "310029" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + "2" + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "AR Invoice" + DataInterfaceConstant.Quote;
			AssertEquals(expectedValue, lineToTest.ToString());
		}

		public void TestToStringWithTrailingZero()
		{
			AccGLHeader gLAccount = SetGLAccount();
			AccGLAccountDescriptor accountDescriptor = SetAccountDescriptor(gLAccount);
			accountDescriptor.AJ_LocalAccountNumber = "1230.000";
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = gLAccount.PK;
			lineToTest.VoucherDate = new ZDateTime(2004, 11, 30, 1, 1, 1);
			lineToTest.DebitAmount = 120.0m;
			lineToTest.CreditAmount = 0.0m;
			lineToTest.VoucherType = "AR-INV";
			lineToTest.VoucherNumber = "31002948737";
			lineToTest.Description = "AR Invoice 12";
			lineToTest.AttachmentCount = 2;
			string expectedValue = DataInterfaceConstant.Quote + "1230" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + "2004-11-30" + DataInterfaceConstant.Tab + "120.00" + DataInterfaceConstant.Tab + "0.00" + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "AR-INV" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "31002948737" + DataInterfaceConstant.Quote + DataInterfaceConstant.Tab + "2" + DataInterfaceConstant.Tab + DataInterfaceConstant.Quote + "AR Invoice" + DataInterfaceConstant.Quote;
			AssertEquals(expectedValue, lineToTest.ToString());
		}

		public void TestValidateLine()
		{
			var lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = ZGuid.Empty;
			lineToTest.VoucherDate = new ZDateTime(2004, 11, 30, 1, 1, 1);
			lineToTest.DebitAmount = 120.0m;
			lineToTest.CreditAmount = 0.0m;
			lineToTest.VoucherType = "AR-INV";
			lineToTest.VoucherNumber = "31002948737";
			lineToTest.Description = "AR Invoice 12";
			lineToTest.GLAccountNumber = "11001001";
			lineToTest.AttachmentCount = 2;
			Assert(!lineToTest.ValidateLine());
			AssertEquals("11001001", lineToTest.ValidationError.Message);
			AssertEquals(VoucherLineErrorType.LocalGLNotMapped, lineToTest.ValidationError.Key);
			lineToTest.Description = null;
			lineToTest.AccountNumber = "11220001";
			Assert(!lineToTest.ValidateLine());
			AssertEquals("Voucher line of transaction '31002948737' won't be created. Please check relative Transaction/Job Description.", lineToTest.ValidationError.Message);
			AssertEquals(VoucherLineErrorType.NoVoucherDescription, lineToTest.ValidationError.Key);
			lineToTest.Description = "AR Invoice 12";
			Assert(lineToTest.ValidateLine());
		}

		public void TestValidateAdditionalAccountDescription()
		{
			AccGLHeader gLAccount = SetGLAccount();
			AccGLAccountDescriptor accountDescriptor = SetAccountDescriptor(gLAccount);
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = gLAccount.PK;
			AssertEquals(accountDescriptor.AJ_LocalAccountNumber, lineToTest.AccountNumber);
			AssertEquals(accountDescriptor.AJ_AccountDescription, lineToTest.AdditionalAccountDescription);
		}

		public void TestAdditionalDescriptionIfLocalDescriptionNotSupplied()
		{
			AccGLHeader gLAccount = SetGLAccount();
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = gLAccount.PK;
			AssertEquals("", lineToTest.AccountNumber);
			AssertEquals("Error-No GL mapping for " + gLAccount.AccountNum, lineToTest.AdditionalAccountDescription);
		}

		public void TestIsDebit()
		{
			var line = new VoucherLine(Factory);
			line.DebitAmount = 1m;
			line.CreditAmount = 0m;
			Assert(line.IsDebit);
		}

		public void TestIsCredit()
		{
			var line = new VoucherLine(Factory);
			line.DebitAmount = 0m;
			line.CreditAmount = 1m;
			Assert(line.IsCredit);
		}

		public void TestAdditionalDescriptionIfNoGLFound()
		{
			SetGLAccount();
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.AccountPK = ZGuid.NewZGuid();
			AssertEquals("", lineToTest.AccountNumber);
			AssertEquals("Error-No GL Account found", lineToTest.AdditionalAccountDescription);
		}

		public void TestEnsureVoucherNumberIs10Characters()
		{
			VoucherLine lineToTest = new VoucherLine(Factory);
			lineToTest.fVoucherNumber = "120504001001";
			AssertEquals("120504001001", lineToTest.VoucherNumber);
			lineToTest.fVoucherNumber = "0504001001";
			AssertEquals("0504001001", lineToTest.VoucherNumber);
			lineToTest.fVoucherNumber = "01001";
			AssertEquals("01001", lineToTest.VoucherNumber);
			lineToTest.fVoucherNumber = "04010011";
			AssertEquals("04010011", lineToTest.VoucherNumber);
			lineToTest.fVoucherNumber = "";
			AssertEquals("", lineToTest.VoucherNumber);
		}

		public void TestOSAmount()
		{
			AccTransactionHeader transactionHeader = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			VoucherLine lineToTest = new VoucherLine(transactionHeader);
			RefCurrency testCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(ZArchitecture.Schema.RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			lineToTest.DebitAmount = 73.7m;
			lineToTest.CreditAmount = 80.4m;
			lineToTest.OSDebitAmount = 11m;
			lineToTest.OSCreditAmount = 12m;
			lineToTest.ExchangeRate = 6.7m;
			lineToTest.CurrencyCode = testCurrency.RX_Code;
			AssertEquals("ExchangeRate", 6.7m, lineToTest.ExchangeRate);
			AssertEquals("OSCreditAmount", 12m, lineToTest.OSCreditAmount);
			AssertEquals("OSDebitAmount", 11m, lineToTest.OSDebitAmount);
			AssertEquals("OSCreditAmount", 80.4m, lineToTest.CreditAmount);
			AssertEquals("OSDebitAmount", 73.7m, lineToTest.DebitAmount);
			lineToTest.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("ExchangeRate", 1m, lineToTest.ExchangeRate);
			AssertEquals("OSCreditAmount", 80.4m, lineToTest.OSCreditAmount);
			AssertEquals("OSDebitAmount", 73.7m, lineToTest.OSDebitAmount);
			AssertEquals("OSCreditAmount", 80.4m, lineToTest.CreditAmount);
			AssertEquals("OSDebitAmount", 73.7m, lineToTest.DebitAmount);
		}

		AccGLAccountDescriptor SetAccountDescriptor(AccGLHeader gLAccount)
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

		AccGLHeader SetGLAccount()
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			gLAccount.AG_AccountNum = "1111.11.11";
			return gLAccount;
		}

		protected override void SetUp()
		{
			OriginalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			base.SetUp();
		}

		ZString OriginalCountryCode;
		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(OriginalCountryCode);
			base.TearDown();
		}
	}
}
