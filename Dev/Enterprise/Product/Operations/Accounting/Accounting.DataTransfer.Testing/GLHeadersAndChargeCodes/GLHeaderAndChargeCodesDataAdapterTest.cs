using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	[TestedType(typeof(GLHeadersAndChargeCodesDataAdapter))]
	sealed class GLHeadersAndChargeCodesDataAdapterTest : BaseAccountingDataAdapterTest<BusinessObjectThatDoesntSave, Xsd.GLHeadersAndChargeCodes>
	{
		public void TestImportValueObjectCore()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6666.66.66");

			AccGLHeader importedGLHeader = Factory.LoadTop1<AccGLHeader>(query);
			AssertNotNull("GLHeader should be exist.", importedGLHeader);
			AssertEquals(importedGLHeader.AG_AccountType, Core.Constants.AccountType.Total);
			AssertEquals(importedGLHeader.AG_Description, "Test Account Name");

			query = new ZQuery(AccChargeCodeSchema.AC_Code, "TESTCODE");

			AccChargeCode importedChargeCode = Factory.LoadTop1<AccChargeCode>(query);
			AssertNotNull("ChargeCode should be exist.", importedChargeCode);
			AssertEquals(importedChargeCode.AC_Desc, "Test Description!");
			AssertEquals(importedChargeCode.AC_ChargeType, Core.Constants.ChargeType.Margin);

			query = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, "1001000");
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, "COA");
			AccGLAccountDescriptor importedGLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(query);
			AssertNotNull("AccGLAccountDescriptor should be exist.", importedGLAccountDescriptor);
			AssertEquals(importedGLAccountDescriptor.AJ_AccountDescription, "Test Mapping Account Name");
			AssertEquals(importedGLAccountDescriptor.AJ_ReportCategory, AccountTypeComboBoxConstants.BalanceSheetAccount);

			query = new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, "1001000");
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, "TT0");
			importedGLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(query);
			AssertNotNull("Report Mapping should be exist.", importedGLAccountDescriptor);
			AssertEquals(importedGLAccountDescriptor.AJ_ReportCategory, "A01");
		}

		public void TestImportGLHeaderWithReferences()
		{
			SetupXmlGLheaderWhichIsReferencedByOtherHeader();
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8888.88.87");

			AccGLHeader importedGLHeader = Factory.LoadTop1<AccGLHeader>(query);
			ZQuery tempQuery = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8888.88.97");
			AccGLHeader referencedGlHeader = Factory.LoadTop1<AccGLHeader>(tempQuery);
			AssertNotNull("GLHeader should be exist.", importedGLHeader);
			AssertEquals(importedGLHeader.AG_AccountType, Core.Constants.AccountType.Total);
			AssertEquals(importedGLHeader.AG_Description, "Test Account Name");
			AssertEquals(importedGLHeader.AG_AG_ConsolidationNum, referencedGlHeader.PK);

			query = new ZQuery(AccChargeCodeSchema.AC_Code, "TESTCODE");

			AccChargeCode importedChargeCode = Factory.LoadTop1<AccChargeCode>(query);
			AssertNotNull("ChargeCode should be exist.", importedChargeCode);
			AssertEquals(importedChargeCode.AC_Desc, "Test Description!");
			AssertEquals(importedChargeCode.AC_ChargeType, Core.Constants.ChargeType.Margin);
		}

		public void TestReportSetupWithIncorrectLocalAccountNumber()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			SetupXmlReportSetupWithIncorrectLocalAccountNumber();
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Account Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The local account number '1000xx' for the language 'ZH-CN' cannot be found."));
		}

		public void TestReportSetupWithIncorrectLanguage()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			SetupXmlReportSetupWithIncorrectLanguage();
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Account Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The local account number '1001000' for the language 'FR-FR' cannot be found."));
		}

		public void TestReportSetupWithIncorrectReportCategory()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			SetupXmlReportSetupWithIncorrectReportCategory();
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Report Category error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The category 'X0X' is missing in the report type 'TT0' for the current language 'ZH-CN'."));
		}

		public void TestDuplicateLocalAccountNumber()
		{
			var descriptor = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings.AddNew();
			descriptor.Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.LocalAccountNumber = "1001000";
			descriptor.DebitCredit = Core.Constants.DebitCredit.Debit;
			descriptor.Description = "Test Mapping Account Name";
			descriptor.ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ReportType = "COA";
			descriptor.CountryOfCompliance = "CN";
			descriptor.ParentAccount = "1111.22.33";

			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Account Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The local account number '1001000' for the language 'ZH-CN' is already in mapping by another GL Account in this file."));
		}

		public void TestParentAccountRefersToSameGLHeader()
		{
			ParentAccount = GetSavedGLAccountForTest("9999.88.77");
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;

			var descriptor1 = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings.AddNew();
			descriptor1.Language = Core.Constants.Languages.ChineseSimplified;
			descriptor1.LocalAccountNumber = "99999998";
			descriptor1.ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor1.ReportType = "COA";
			descriptor1.CountryOfCompliance = "CN";
			descriptor1.ParentAccount = "9999.88.77";
			descriptor1.DebitCredit = Core.Constants.DebitCredit.Debit;
			descriptor1.Description = "Test Mapping Account Name 1";

			var descriptor2 = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings.AddNew();
			descriptor2.Language = Core.Constants.Languages.ChineseSimplified;
			descriptor2.LocalAccountNumber = "99999999";
			descriptor2.ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor2.ReportType = "COA";
			descriptor2.CountryOfCompliance = "CN";
			descriptor2.ParentAccount = "9999.88.77";
			descriptor2.DebitCredit = Core.Constants.DebitCredit.Credit;
			descriptor2.Description = "Test Mapping Account Name 2";

			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			Assert("Should have errors", Context.NotificationsHasErrors);
			AssertContains("Should have Parent Account error", @"The Parent Account '9999.88.77' for the language 'ZH-CN' is already in mapping by another Local Account '99999998'.", ((NotificationBuffer)(Context.Notifications)).AsString);
		}

		public void TestDuplicateReportSetupMapping()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "CN";
			var reportSetup = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.AddNew();
			reportSetup.Language = Core.Constants.Languages.ChineseSimplified;
			reportSetup.Country = "CN";
			reportSetup.LocalAccountNumber = "1001000";
			reportSetup.ReportType = "TT0";
			reportSetup.ReportCategory = "A01";
			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The local account number '1001000' for the language 'ZH-CN' is already in mapping to Report Category 'A01' in Report Type 'TT0'."));
		}

		public void TestDuplicateGLAccountNumber()
		{
			var glHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();
			glHeader.AccNumber = "6666.66.66";
			glHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader.Description = "Test Account Name";
			glHeader.AccountType = Core.Constants.AccountType.Total;

			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Account Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The Account Number '6666.66.66' is already in use by another GL Account."));
		}

		public void TestDuplicateChargeCode()
		{
			var chargeCode = Value.SingleGLHeadersAndChargeCodesElement.ChargeCodes.AddNew();
			chargeCode.Code = "TESTCODE";
			chargeCode.Description = "Test Description!";
			chargeCode.DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES";
			chargeCode.ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode.MarginPercentage = "50";

			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Charge Code error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The Code 'TESTCODE' is already in use by another Charge Code in this file."));
		}

		public void TestNotifyBizObjCreatedOrUpdated()
		{
			Adapter.NotifyBizObjCreatedOrUpdated_ForTestOnly(Buffer, new BusinessObjectThatDoesntSave(Factory));
			AssertEquals("The adapter must not add message about creation BusinessObjectThatDoesntSave.", 0, Buffer.Events.Length);
		}

		public void TestDuplicateNotifications()
		{
			var glHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();
			glHeader.AccNumber = "6666.66.66";
			glHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader.Description = "Test Account Name";
			glHeader.AccountType = Core.Constants.AccountType.Total;

			Adapter.ImportFromValueObjectCore_ForTestOnly(new BusinessObjectThatDoesntSave(Factory), Value, Context);

			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Account Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains("Error: The Account Number '6666.66.66' is already in use by another GL Account."));

			var events = ((Context.Notifications as NotificationBuffer).Events);

			Assert("Should have errors", events.Length > 0);

			for (int i = 0; i < events.Length; i++)
			{
				for (int j = 0; j < events.Length; j++)
				{
					if (j != i && events[j].GetHashCode() == events[i].GetHashCode())
					{
						Assert("Should not have duplicated errors", false);
					}
				}
			}
		}

		#region Base Tests

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSave, Xsd.GLHeadersAndChargeCodes> GetNewBizObjXmlDataAdapter()
		{
			return new GLHeadersAndChargeCodesDataAdapter();
		}

		#endregion

		#region Implementation

		void SetupXmlReportSetupWithIncorrectLocalAccountNumber()
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup reportSetup = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.AddNew();
			reportSetup.Language = Core.Constants.Languages.ChineseSimplified;
			reportSetup.Country = "CN";
			reportSetup.LocalAccountNumber = "1000xx";
			reportSetup.ReportType = "TT0";
			reportSetup.ReportCategory = "A01";
		}

		void SetupXmlReportSetupWithIncorrectReportCategory()
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup reportSetup = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.AddNew();
			reportSetup.Language = Core.Constants.Languages.ChineseSimplified;
			reportSetup.Country = "CN";
			reportSetup.LocalAccountNumber = "1001000";
			reportSetup.ReportType = "TT0";
			reportSetup.ReportCategory = "X0X";
		}

		void SetupXmlReportSetupWithIncorrectLanguage()
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup reportSetup1 = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.AddNew();
			reportSetup1.Language = Core.Constants.Languages.French;
			reportSetup1.Country = "CN";
			reportSetup1.LocalAccountNumber = "1001000";
			reportSetup1.ReportType = "TT0";
			reportSetup1.ReportCategory = "A01";
		}

		void SetupXmlReportSetupWithCorrectData()
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup reportSetup = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups.AddNew();
			reportSetup.Language = Core.Constants.Languages.ChineseSimplified;
			reportSetup.Country = "CN";
			reportSetup.LocalAccountNumber = "1001000";
			reportSetup.ReportType = "TT0";
			reportSetup.ReportCategory = "A01";
		}

		void SetupXmlDescriptorWithCorrectData()
		{
			Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping descriptor = Value.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings.AddNew();
			descriptor.Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.LocalAccountNumber = "1001000";
			descriptor.DebitCredit = Core.Constants.DebitCredit.Debit;
			descriptor.Description = "Test Mapping Account Name";
			descriptor.ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ReportType = "COA";
			descriptor.CountryOfCompliance = "CN";

			ParentAccount = GetSavedGLAccountForTest("1111.22.33");
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentAccount = "1111.22.33";

			descriptor.TotalLevel = 15;
			descriptor.PrintSequence = 5;
		}

		void SetupXmlGLheaderWithCorrectData()
		{
			Xsd.GLHeadersGLHeader gLHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();

			gLHeader.AccNumber = "6666.66.66";
			gLHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			gLHeader.Description = "Test Account Name";
			gLHeader.AccountType = Core.Constants.AccountType.Total;

			GetSavedGLAccountForTest("8888.88.88");
			gLHeader.ConsolidationNum = "8888.88.88";
			gLHeader.PercentNum = "8888.88.88";

			GetSavedGLAccountForTest("3333.33.33");
			gLHeader.AlternateNum = "3333.33.33";

			GetSavedGLAccountForTest("9999.99.99");
			gLHeader.HeaderDependsOnTotal = "9999.99.99";

			gLHeader.TotalLevel = 15;

			gLHeader.ControlAccount = Core.Constants.BooleanTrueString;
			gLHeader.DisallowDirectPosting = Core.Constants.BooleanFalseString;

			gLHeader.PrintSequence = 5;
			gLHeader.IsSubAccountMandatory = Core.Constants.BooleanFalseString;
		}

		void SetupXmlGLheaderWhichIsReferencedByOtherHeader()
		{
			Xsd.GLHeadersGLHeader gLHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();

			gLHeader.AccNumber = "8888.88.87";
			gLHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			gLHeader.Description = "Test Account Name";
			gLHeader.AccountType = Core.Constants.AccountType.Total;

			gLHeader.ConsolidationNum = "8888.88.97";
			gLHeader.PercentNum = "8888.88.97";

			gLHeader.AlternateNum = "1111.11.11";

			gLHeader.HeaderDependsOnTotal = "9999.99.99";

			gLHeader.TotalLevel = 15;

			gLHeader.ControlAccount = Core.Constants.BooleanTrueString;
			gLHeader.DisallowDirectPosting = Core.Constants.BooleanFalseString;

			gLHeader.PrintSequence = 5;
			gLHeader.IsSubAccountMandatory = Core.Constants.BooleanFalseString;

			gLHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();

			gLHeader.AccNumber = "8888.88.97";
			gLHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			gLHeader.Description = "Test Account Name";
			gLHeader.AccountType = Core.Constants.AccountType.Total;

			GetSavedGLAccountForTest("8888.88.99");
			gLHeader.ConsolidationNum = "8888.88.99";
			gLHeader.PercentNum = "8888.88.99";

			gLHeader.AlternateNum = "1111.11.11";

			gLHeader.HeaderDependsOnTotal = "9999.99.99";

			gLHeader.TotalLevel = 15;

			gLHeader.ControlAccount = Core.Constants.BooleanTrueString;
			gLHeader.DisallowDirectPosting = Core.Constants.BooleanFalseString;

			gLHeader.PrintSequence = 5;
			gLHeader.IsSubAccountMandatory = Core.Constants.BooleanFalseString;
		}

		void SetupXmlChargeCodeWithCorrectData()
		{
			Xsd.ChargeCodesChargeCode chargeCode = Value.SingleGLHeadersAndChargeCodesElement.ChargeCodes.AddNew();
			chargeCode.Code = "TESTCODE";
			chargeCode.Description = "Test Description!";
			chargeCode.DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES";
			chargeCode.ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode.MarginPercentage = "50";

			GetSavedTaxRateForTest("TESTCODE");
			chargeCode.GSTRate = "TESTCODE";

			GetSavedAccWitholdingForTest("TESTWHCODE");
			chargeCode.WithholdingTaxRate = "TESTWHCODE";

			GetSavedAccGroupsForTest("SALESTEST");
			chargeCode.SalesGroup = "SALESTEST";

			GetSavedAccGroupsForTest("EXPTEST");
			chargeCode.ExpenseGroup = "EXPTEST";

			chargeCode.RevenueAccount = "9999.99.99";

			GetSavedGLAccountForTest("1111.11.11");
			chargeCode.WIPAccount = "1111.11.11";

			GetSavedGLAccountForTest("2222.22.22");
			chargeCode.CostAccount = "2222.22.22";

			chargeCode.AccrualAccount = "3333.33.33";

			chargeCode.ChargeGroup = "CSH";
			chargeCode.IsGroupageCharge = Core.Constants.BooleanTrueString;
			chargeCode.SubGroup = "STG";
			chargeCode.RateCalculator = "AGY";
			chargeCode.ShowOnQuotation = Core.Constants.BooleanTrueString;
			chargeCode.SuppressOnQuoteIfZero = Core.Constants.BooleanFalseString;
			chargeCode.IATA_ChargeCodeMap = "AT";
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXXX.XX.XX");
			Value = new Xsd.GLHeadersAndChargeCodes();
			SetupXmlGLheaderWithCorrectData();
			SetupXmlChargeCodeWithCorrectData();
			SetupXmlDescriptorWithCorrectData();
			SetupXmlReportSetupWithCorrectData();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
		}

		protected override BusinessObjectThatDoesntSave NewBusinessObject()
		{
			return new BusinessObjectThatDoesntSave(Factory);
		}

		Xsd.GLHeadersAndChargeCodes Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;
		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		AccGLHeader ParentAccount;

		AccGLHeader GetSavedGLAccountForTest(ZString accNumber)
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = accNumber;
			return gLHeader;
		}

		void GetSavedTaxRateForTest(ZString codeValue)
		{
			AccTaxRate accTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			accTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			accTaxRate.AT_Code = codeValue;
			return;
		}

		void GetSavedAccWitholdingForTest(ZString codeValue)
		{
			AccWithholding wHTRate = Factory.NewWithValidTestData<AccWithholding>();
			wHTRate.AW_GC = GlbCompany.CurrentCompany.PK;
			wHTRate.AW_Code = codeValue;
			return;
		}

		void GetSavedAccGroupsForTest(ZString codeValue)
		{
			AccGroups accGroup = Factory.NewWithValidTestData<AccGroups>();
			accGroup.AR_Code = codeValue;
			return;
		}

		GLHeadersAndChargeCodesDataAdapter fAdapter;
		GLHeadersAndChargeCodesDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new GLHeadersAndChargeCodesDataAdapter()); }
		}

		protected override string[] StringFieldsToIgnore
		{
			get { return new string[] { "SubAccountType", "CashFlowType", "CompanyFilterList" }; }
		}

		#endregion
	}
}
