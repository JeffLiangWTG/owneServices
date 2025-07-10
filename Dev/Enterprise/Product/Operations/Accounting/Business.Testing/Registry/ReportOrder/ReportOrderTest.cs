using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ReportOrder))]
	public class ReportOrderTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateLanguage()
		{
			AssertNoErrors("Precondition: Language should not have errors.", BizObj.LanguageInfo);
			Assert("Precondition: LanguageList[0].Code should not be empty.", !string.IsNullOrEmpty(BizObj.LanguageList[0].Code));

			BizObj.Language = "!@#";
			AssertHasError(BizObj.LanguageInfo, "Enter a valid selection.");

			BizObj.Language = BizObj.LanguageList[0].Code;
			AssertNoErrors(BizObj.LanguageInfo);

			BizObj.Language = "";
			AssertHasError(BizObj.LanguageInfo, "Please enter a value.");
		}

		public void TestValidateLanguageIsUnique()
		{
			ReportOrderCollection collection = new ReportOrderCollection(Factory) { BizObj };
			AssertNoErrors("Precondition: Language should not have errors.", BizObj.LanguageInfo);

			BizObj.Language = BizObj.LanguageList[0].Code;
			AssertNoErrors(BizObj.LanguageInfo);

			ReportOrder reportOrder2 = collection.AddNew();
			reportOrder2.Language = BizObj.Language;
			Assert(reportOrder2.RowErrors.Contains("Duplicate Country/Region Code and Language."));
		}

		public void TestValidateAccountsOrderBeginsWith()
		{
			AssertNoErrors("Precondition: AccountsOrderBeginsWith should not have errors.", BizObj.AccountsOrderBeginsWithInfo);
			Assert("Precondition: AccountOrderTypeList[0].Code should not be empty.", !string.IsNullOrEmpty(BizObj.AccountOrderTypeList[0].Code));

			BizObj.AccountsOrderBeginsWith = "!@#";
			AssertHasError(BizObj.AccountsOrderBeginsWithInfo, "Enter a valid selection.");

			BizObj.AccountsOrderBeginsWith = BizObj.AccountOrderTypeList[0].Code;
			AssertNoErrors(BizObj.AccountsOrderBeginsWithInfo);

			BizObj.AccountsOrderBeginsWith = "";
			AssertHasError(BizObj.AccountsOrderBeginsWithInfo, "Please enter a value.");
		}

		public void TestValidateGLAccountSecondReportStartsFrom()
		{
			AssertNoErrors("Precondition: GLAccountSecondReportStartsFrom should not have errors.", BizObj.GLAccountSecondReportStartsFromInfo);

			PopulateAccountDescriptorList();
			BizObj.Language = BizObj.LanguageList[0].Code;

			String expectedBalanceSheetMessage = @"The account you have selected is the first multi language account mapping, when multi language accounts are sorted in ascending order.
This account will be used when running the multi language ‘Balance Sheet’ report.
You should select the account that should be used as the starting point when running the multi language ‘Profit and Loss’ report.
You cannot select the first multi language account mapping in this field.";
			String expectedProfitAndLossMessage = @"The account you have selected is the first multi language account mapping, when multi language accounts are sorted in ascending order.
This account will be used when running the multi language ‘Profit and Loss’ report.
You should select the account that should be used as the starting point when running the multi language ‘Balance Sheet’ report.
You cannot select the first multi language account mapping in this field.";

			BizObj.GLAccountSecondReportStartsFrom = ZGuid.Empty;
			AssertHasError(BizObj.GLAccountSecondReportStartsFromInfo, "Please enter a value.");

			BizObj.GLAccountSecondReportStartsFrom = AccGLAccountDescriptor2.PK;
			AssertHasError(BizObj.GLAccountSecondReportStartsFromInfo, "Enter a valid selection.");

			BizObj.AccountsOrderBeginsWith = "BalanceSheet";
			BizObj.GLAccountSecondReportStartsFrom = AccGLAccountDescriptor1.PK;
			AssertHasError(BizObj.GLAccountSecondReportStartsFromInfo, expectedBalanceSheetMessage);

			BizObj.GLAccountSecondReportStartsFrom = AccGLAccountDescriptor3.PK;
			AssertHasError(BizObj.GLAccountSecondReportStartsFromInfo, "Enter a valid selection.");

			BizObj.AccountsOrderBeginsWith = "ProfitAndLoss";
			BizObj.GLAccountSecondReportStartsFrom = AccGLAccountDescriptor1.PK;
			AssertHasError(BizObj.GLAccountSecondReportStartsFromInfo, expectedProfitAndLossMessage);

			BizObj.GLAccountSecondReportStartsFrom = AccGLAccountDescriptor4.PK;
			AssertNoErrors(BizObj.GLAccountSecondReportStartsFromInfo);
		}

		public void TestRunPreSaveValidation()
		{
			BizObj.Language = "!@#";
			BizObj.AccountsOrderBeginsWith = "!@#";
			BizObj.GLAccountSecondReportStartsFrom = ZGuid.Empty;

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.LanguageInfo);
			AssertHasErrors(BizObj.AccountsOrderBeginsWithInfo);
			AssertHasErrors(BizObj.GLAccountSecondReportStartsFromInfo);
		}

		public void TestLanguageList()
		{
			CodeDescriptionPairList languages = new CodeDescriptionPairList(OLookUpEditType.Language);
			AssertEquals("Count", languages.Count + 1, BizObj.LanguageList.Count);
			AssertEquals("GetDescriptionFromCode(\"ZZZ\")", "External Link to General Ledger", BizObj.LanguageList.GetDescriptionFromCode("ZZZ"));
		}

		public void TestAccountOrderTypeList()
		{
			AssertEquals("AccountOrderTypeList.Count", 2, BizObj.AccountOrderTypeList.Count);
			AssertEquals("AccountOrderTypeList should contain 'ProfitAndLoss'", true, BizObj.AccountOrderTypeList.ContainsCode("ProfitAndLoss"));
		}

		public void TestAccountDescriptorList()
		{
			PopulateAccountDescriptorList();
			BizObj.Language = BizObj.LanguageList[0].Code;

			BizObj.AccountDescriptorList.Load();
			AssertEquals("AccountDescriptorList should contain AccGLAccountDescriptor1", true, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor1));
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor2", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor2));
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor3", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor3));
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor5", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor5));

			BizObj.Language = BizObj.LanguageList[1].Code;
			BizObj.AccountDescriptorList.Load();
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor1", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor1));
			AssertEquals("AccountDescriptorList should contain AccGLAccountDescriptor2", true, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor2));
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor3", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor3));
			AssertEquals("AccountDescriptorList should NOT contain AccGLAccountDescriptor5", false, BizObj.AccountDescriptorList.Contains(AccGLAccountDescriptor5));
		}

		public void TestAccountsOrderEndsWith()
		{
			Assert("Pre-condition: AccountsOrderBeginsWith should be Empty", BizObj.AccountsOrderBeginsWith.IsEmpty);
			Assert("Should be Empty when AccountsOrderBeginsWith is not set", BizObj.AccountsOrderEndsWith.IsEmpty);

			BizObj.AccountsOrderBeginsWith = "ProfitAndLoss";
			AssertEquals("AccountsOrderEndsWith", "BalanceSheet", BizObj.AccountsOrderEndsWith);

			BizObj.AccountsOrderBeginsWith = "BalanceSheet";
			AssertEquals("AccountsOrderEndsWith", "ProfitAndLoss", BizObj.AccountsOrderEndsWith);
		}

		public void TestGLAccountFirstReportStartsFrom()
		{
			BizObj.Language = BizObj.LanguageList[0].Code;
			AssertEquals("Pre-condition: No suitable account found", ZGuid.Empty, BizObj.GLAccountFirstReportStartsFrom);

			PopulateAccountDescriptorList();
			AssertEquals("Should be the first account", AccGLAccountDescriptor1.PK, BizObj.GLAccountFirstReportStartsFrom);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ReportOrder result = new ReportOrder(Factory);
			result.Language = Enterprise.Core.SharedConstants.Languages.English;
			result.AccountsOrderBeginsWith = "ProfitAndLoss";
			result.GLAccountSecondReportStartsFrom = ZGuid.NewZGuid();

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ReportOrder BizObj
		{
			get { return (ReportOrder)base.BizObj; }
		}

		void PopulateAccountDescriptorList()
		{
			Type accGLAccountDescriptorType = typeof(AccGLAccountDescriptor);

			AccGLAccountDescriptor1 = Factory.NewWithValidTestData(accGLAccountDescriptorType);
			AccGLAccountDescriptor2 = Factory.NewWithValidTestData(accGLAccountDescriptorType);
			AccGLAccountDescriptor3 = Factory.NewWithValidTestData(accGLAccountDescriptorType);
			AccGLAccountDescriptor4 = Factory.NewWithValidTestData(accGLAccountDescriptorType);
			AccGLAccountDescriptor5 = Factory.NewWithValidTestData(accGLAccountDescriptorType);

			AccGLAccountDescriptor1[AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber] = new ZString("100");
			AccGLAccountDescriptor2[AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber] = new ZString("200");
			AccGLAccountDescriptor3[AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber] = new ZString("300");
			AccGLAccountDescriptor4[AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber] = new ZString("400");
			AccGLAccountDescriptor4[AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber] = new ZString("500");

			AccGLAccountDescriptor1[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);
			AccGLAccountDescriptor2[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);
			AccGLAccountDescriptor3[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Consolidation);
			AccGLAccountDescriptor4[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);
			AccGLAccountDescriptor5[AccGLAccountDescriptorSchema.Constants.AJ_ReportCategory] = new ZString(AccountTypeComboBoxConstants.Header);

			AccGLAccountDescriptor1[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(BizObj.LanguageList[0].Code);
			AccGLAccountDescriptor2[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(BizObj.LanguageList[1].Code);
			AccGLAccountDescriptor3[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(BizObj.LanguageList[0].Code);
			AccGLAccountDescriptor4[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(BizObj.LanguageList[0].Code);
			AccGLAccountDescriptor5[AccGLAccountDescriptorSchema.Constants.AJ_Language] = new ZString(BizObj.LanguageList[0].Code);
			AccGLAccountDescriptor5[AccGLAccountDescriptorSchema.Constants.AJ_ReportType] = new ZString("abc");

			Factory.Save();
		}

		BusinessObject AccGLAccountDescriptor1;
		BusinessObject AccGLAccountDescriptor2;
		BusinessObject AccGLAccountDescriptor3;
		BusinessObject AccGLAccountDescriptor4;
		BusinessObject AccGLAccountDescriptor5;
		#endregion
	}
}
