using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(ARAPContraVoucherProvider))]
	public class ARAPContraVoucherProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public override void TestDebitInFirstRow()
		{
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra, "TESTDESC");
			Factory.Save();
			ARAPContraVoucherProvider testVoucherProvider = new ARAPContraVoucherProvider(testContra.APRow);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[0].CreditAmount);
			testVoucherProvider = new ARAPContraVoucherProvider(testContra.ARRow);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[0].CreditAmount);
		}

		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestControlAccountNumber()
		{
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLAccountDescriptor aRControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aRControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			aRControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			aRControlLocal.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			aRControlLocal.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			aRControlLocal.AJ_LocalAccountNumber = ARControlAccountNumber;
			aRControlLocal.ParentGLHeaderPK = aRControl.PK;
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ControlAccount.PK.ToGuid());
			Factory.Save();
			AssertEquals(ControlAccountNumber, TestProvider.VoucherLines[0].AccountNumber);
			AssertEquals(ARControlAccountNumber, TestProvider.VoucherLines[1].AccountNumber);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherTypeForAR()
		{
			//string ExpectedVoucherType = "应收帐款-JNL";
			string expectedVoucherType = "CTR";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherTypeForAP()
		{
			//string ExpectedVoucherType = "应收帐款-JNL";
			string expectedVoucherType = "CTR";
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherAmount()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		public void TestContraDetailFromAR()
		{
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra, "TESTDESC");
			Factory.Save();
			ARAPContraVoucherProvider testVoucherProvider = new ARAPContraVoucherProvider(testContra.ARRow);
			AssertEquals(2, testVoucherProvider.VoucherLines.Length);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[0].VoucherNumber);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[1].VoucherNumber);
			AssertEquals(APControlAccountNumber, testVoucherProvider.VoucherLines[0].AccountNumber);
			AssertEquals(ARControlAccountNumber, testVoucherProvider.VoucherLines[1].AccountNumber);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[1].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, testVoucherProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, testVoucherProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(1m, testVoucherProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(120m, testVoucherProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals("APControlDescription", testVoucherProvider.VoucherLines[0].AdditionalAccountDescription);
			AssertEquals("ARControlDescription", testVoucherProvider.VoucherLines[1].AdditionalAccountDescription);
			AssertEquals(new VoucherDescriptionLookUp(testContra.APRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[0].Description);
			AssertEquals(new VoucherDescriptionLookUp(testContra.ARRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[1].Description);
		}

		public void TestContraDetailFromAP()
		{
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra, "TESTDESC");
			Factory.Save();
			ARAPContraVoucherProvider testVoucherProvider = new ARAPContraVoucherProvider(testContra.APRow);
			AssertEquals(2, testVoucherProvider.VoucherLines.Length);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[0].VoucherNumber);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[1].VoucherNumber);
			AssertEquals(APControlAccountNumber, testVoucherProvider.VoucherLines[0].AccountNumber);
			AssertEquals(ARControlAccountNumber, testVoucherProvider.VoucherLines[1].AccountNumber);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[1].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, testVoucherProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, testVoucherProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(1m, testVoucherProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].ForeignCurrencyAmount);
			AssertEquals(120m, testVoucherProvider.VoucherLines[1].ForeignCurrencyAmount);
			AssertEquals("APControlDescription", testVoucherProvider.VoucherLines[0].AdditionalAccountDescription);
			AssertEquals("ARControlDescription", testVoucherProvider.VoucherLines[1].AdditionalAccountDescription);
			AssertEquals(new VoucherDescriptionLookUp(testContra.APRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[0].Description);
			AssertEquals(new VoucherDescriptionLookUp(testContra.ARRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[1].Description);
		}

		public void TestContraDetailLoadOnlyRelatedRows()
		{
			Contra testContra = Contra.New(Factory);
			SetUpForContraDetailTest(testContra, "TESTDESC");
			Contra testContra2 = Contra.New(Factory);
			SetUpForContraDetailTest(testContra2, "TESTDESC2");
			Factory.Save();
			ARAPContraVoucherProvider testVoucherProvider = new ARAPContraVoucherProvider(testContra.APRow);
			AssertEquals(2, testVoucherProvider.VoucherLines.Length);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[0].VoucherNumber);
			AssertEquals(testContra.AH_TransactionNum, testVoucherProvider.VoucherLines[1].VoucherNumber);
			AssertEquals(APControlAccountNumber, testVoucherProvider.VoucherLines[0].AccountNumber);
			AssertEquals(ARControlAccountNumber, testVoucherProvider.VoucherLines[1].AccountNumber);
			AssertEquals(120m, testVoucherProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[1].DebitAmount);
			AssertEquals(0m, testVoucherProvider.VoucherLines[0].CreditAmount);
			AssertEquals(120m, testVoucherProvider.VoucherLines[1].CreditAmount);
			AssertEquals("APControlDescription", testVoucherProvider.VoucherLines[0].AdditionalAccountDescription);
			AssertEquals("ARControlDescription", testVoucherProvider.VoucherLines[1].AdditionalAccountDescription);
			AssertEquals(new VoucherDescriptionLookUp(testContra.APRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[0].Description);
			AssertEquals(new VoucherDescriptionLookUp(testContra.ARRow).GetDescription() + " - " + testContra.AH_Desc, testVoucherProvider.VoucherLines[1].Description);
		}

		void SetUpForContraDetailTest(Contra testContra, string desc)
		{
			testContra.AH_ARAccount = FromAccount.PK;
			testContra.AH_APAccount = ToAccount.PK;
			testContra.AH_OSTotal = 120m;
			testContra.AH_Desc = desc;
			AccGLHeader aPControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader aRControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
			AccGLAccountDescriptor aRControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aRControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			aRControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			aRControlLocal.AJ_LocalAccountNumber = ARControlAccountNumber;
			aRControlLocal.AJ_AccountDescription = "ARControlDescription";
			aRControlLocal.ParentGLHeaderPK = aRControl.PK;
			AccGLAccountDescriptor aPControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
			aPControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
			aPControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			aPControlLocal.AJ_LocalAccountNumber = APControlAccountNumber;
			aPControlLocal.AJ_AccountDescription = "APControlDescription";
			aPControlLocal.ParentGLHeaderPK = aPControl.PK;
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPControl.PK.ToGuid());
		}

		readonly ZString CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CountryCode;
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ARAPContraVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			Contra testContra = Contra.New(Factory);
			testContra.AH_ARAccount = FromAccount.PK;
			testContra.AH_APAccount = ToAccount.PK;
			return testContra.APRow;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARAPContraVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override ZString GetExpectedOrganisationCodeForFirstLine()
		{
			return ToAccount.OH_Code;
		}

		protected override ZString GetExpectedOrganisationCodeForControlLine()
		{
			return FromAccount.OH_Code;
		}

		protected override bool OrganisationCodeForFirstLinesIsApplicableToThisVoucher
		{
			get
			{
				return true;
			}
		}

		const string ARControlAccountNumber = "101";
		const string APControlAccountNumber = "102";
	}
}
