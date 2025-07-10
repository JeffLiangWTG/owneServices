using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	[TestedType(typeof(GLAccountDescriptorDataAdapter))]
	public class GLAccountDescriptorDataAdapterTest : ValueObjectDataAdapterTest<AccGLAccountDescriptor, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping>
	{
		public void TestImportFromValueObjectCore()
		{
			Value.ParentAccount = "";
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals("6666.66.66", GLAccountDesc.AJ_LocalAccountNumber);
			AssertEquals(Core.Constants.DebitCredit.Debit, GLAccountDesc.AJ_DebitCredit);
			AssertEquals("Test Account Name", GLAccountDesc.AJ_AccountDescription);
			AssertEquals(Core.Constants.AccountType.Total, GLAccountDesc.AJ_ReportCategory);
			AssertEquals(15, GLAccountDesc.AJ_TotalLevel.ToZInt());
			AssertEquals(5, GLAccountDesc.AJ_PrintSequence.ToZInt());
			AssertEquals(0, GLAccountDesc.Notifications.GetErrors().Count());
		}

		public void TestImportFromValueObjectCore_ForNTE()
		{
			Value.ReportCategory = Core.Constants.AccountType.Note;
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals("6666.66.66", GLAccountDesc.AJ_LocalAccountNumber);
			AssertEquals(Core.Constants.DebitCredit.Debit, GLAccountDesc.AJ_DebitCredit);
			AssertEquals("Test Account Name", GLAccountDesc.AJ_AccountDescription);
			AssertEquals(Core.Constants.AccountType.Note, GLAccountDesc.AJ_ReportCategory);
			AssertEquals(0, GLAccountDesc.AJ_TotalLevel.ToZInt());
			AssertEquals(5, GLAccountDesc.AJ_PrintSequence.ToZInt());
			AssertEquals("CN", GLAccountDesc.AJ_RN_NKCountryOfCompliance);
			AssertEquals(Core.Constants.Languages.ChineseSimplified, GLAccountDesc.AJ_Language);
			AssertEquals("1111.22.33", GLAccountDesc.ParentGLHeader.AG_AccountNum);
			AssertNoErrors(GLAccountDesc.AJ_ReportCategoryInfo);
		}

		public void TestSetReferencesToGLAccounts()
		{
			Value.ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;

			GLAccountDescAdapter.SetReferencesToGLAccounts(GLAccountDesc, Value, Context);
			AssertEquals(ConsolidationAccount.PK, GLAccountDesc.AJ_AJ_PercentNum);
			AssertEquals(ConsolidationAccount.PK, GLAccountDesc.AJ_AJ_ConsolidationNum);
			AssertEquals(AlternateAccount.PK, GLAccountDesc.AJ_AJ_AlternativeNum);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_HeaderDependsOnTotal);

			GLAccountDesc = Factory.New<AccGLAccountDescriptor>();
			Value.ReportCategory = Core.Constants.AccountType.Header;

			GLAccountDescAdapter.SetReferencesToGLAccounts(GLAccountDesc, Value, Context);
			AssertEquals(ConsolidationAccount.PK, GLAccountDesc.AJ_AJ_PercentNum);
			AssertEquals(ConsolidationAccount.PK, GLAccountDesc.AJ_AJ_ConsolidationNum);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_AlternativeNum);
			AssertEquals(TotalReferenceAccount.PK, GLAccountDesc.AJ_AJ_HeaderDependsOnTotal);

			GLAccountDesc = Factory.New<AccGLAccountDescriptor>();
			Value.ReportCategory = Core.Constants.AccountType.Note;
			Value.CarriedForwardAccount = "XXXX.XX.XX";
			GLAccountDescAdapter.SetReferencesToGLAccounts(GLAccountDesc, Value, Context);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_PercentNum);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_ConsolidationNum);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_AlternativeNum);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_HeaderDependsOnTotal);
			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_CarriedForwardAccount);
		}

		public void TestIncorrectAccountNumber()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_LocalAccountNumberInfo.HasErrors());

			Value.LocalAccountNumber = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			AssertEquals("Should have no error. AJ_LocalAccountNumber can be any format", false, GLAccountDesc.AJ_LocalAccountNumberInfo.HasErrors());
		}

		public void TestIncorrectDebitCredit()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_DebitCreditInfo.HasErrors());

			Value.DebitCredit = "XXX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(GLAccountDesc.AJ_DebitCreditInfo.HasErrors());
		}

		public void TestIncorrectDescription()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_AccountDescriptionInfo.HasErrors());

			for (int i = 0; i < AccGLAccountDescriptorSchema.AJ_AccountDescription.MaxLength; i++)
			{
				Value.Description += "a";
			}
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(Buffer.HasWarnings);
		}

		public void TestIncorrectReportCategory()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_ReportCategoryInfo.HasErrors());

			Value.ReportCategory = "XXX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(GLAccountDesc.AJ_ReportCategoryInfo.HasErrors());

			Value.ReportCategory = Core.Constants.AccountType.Note;
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			AssertNoErrors(GLAccountDesc.AJ_ReportCategoryInfo);
		}

		public void TestIncorrectConsolidationNum()
		{
			GLAccountDesc.AJ_ConsolidatedAccountList.Load();
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			AssertNoErrors(GLAccountDesc.AJ_AJ_ConsolidationNumInfo);

			Value.ConsolidationNum = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_ConsolidationNum);
		}

		public void TestIncorrectPercentNum()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertNoErrors(GLAccountDesc.AJ_AJ_PercentNumInfo);

			Value.PercentNum = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_PercentNum);
		}

		public void TestIncorrectAlternateNum()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			Assert(!GLAccountDesc.AJ_AJ_AlternativeNumInfo.HasErrors());

			Value.ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			Value.AlternativeNum = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_AlternativeNum);
		}

		public void TestIncorrectHeaderDependsOnTotal()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_AJ_HeaderDependsOnTotalInfo.HasErrors());

			Value.ReportCategory = Core.Constants.AccountType.Header;
			Value.HeaderDependsOnTotal = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_HeaderDependsOnTotal);
		}

		public void TestIncorrectCarriedForwardAccount()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_AJ_CarriedForwardAccountInfo.HasErrors());

			Value.ReportCategory = AccountTypeComboBoxConstants.CarriedForwardAccount;
			Value.CarriedForwardAccount = "XXXX.XX.XX";

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals(ZGuid.Empty, GLAccountDesc.AJ_AJ_CarriedForwardAccount);
		}

		public void TestIncorrectTotalLevel()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_TotalLevelInfo.HasErrors());

			Value.ReportCategory = Core.Constants.AccountType.Consolidation;
			Value.TotalLevel = -999;
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			AssertEquals(0, GLAccountDesc.AJ_TotalLevel.ToZInt());

			Value.ReportCategory = Core.Constants.AccountType.Total;
			Value.TotalLevel = -999;

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(GLAccountDesc.AJ_TotalLevelInfo.HasErrors());
		}

		public void TestAddErrorsToNotifications()
		{
			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(!GLAccountDesc.AJ_PrintSequenceInfo.HasErrors());

			Value.ReportCategory = Core.Constants.AccountType.Total;
			Value.TotalLevel = 1000;

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);
			Assert(GLAccountDesc.AJ_TotalLevelInfo.HasErrors());
			Assert(Buffer.AsString.Contains(GLAccountDesc.AJ_TotalLevelInfo.GetErrors().GetFirstMessage()));
		}

		public void TestNotifyBizObjCreatedOrUpdated()
		{
			GLAccountDescAdapter.NotifyBizObjCreatedOrUpdated_ForTestOnly(Buffer, new BusinessObjectThatDoesntSave(Factory));
			AssertEquals("The adapter must not add message about creation BusinessObjectThatDoesntSave.", 0, Buffer.Events.Length);
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
			get { return GLAccountDescAdapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return GLAccountDescAdapter.RootElementName; }
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

		protected override ValueObjectDataAdapter<AccGLAccountDescriptor, Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping> GetNewBizObjXmlDataAdapter()
		{
			return new GLAccountDescriptorDataAdapter();
		}

		#endregion

		#region Implementation

		void SetupXmlWithCorrectData()
		{
			Value.Language = Core.Constants.Languages.ChineseSimplified;
			Value.CountryOfCompliance = Core.Constants.CountryCodes.China;
			Value.LocalAccountNumber = "6666.66.66";
			Value.DebitCredit = Core.Constants.DebitCredit.Debit;
			Value.Description = "Test Account Name";
			Value.ReportCategory = AccountTypeComboBoxConstants.Total;
			Value.ReportType = "COA";
			Value.CountryOfCompliance = "CN";

			ParentAccount = GetSavedGLAccountForTest("1111.22.33");
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.Total;
			Value.ParentAccount = "1111.22.33";

			AccGLHeader gLAccount = GetSavedGLAccountForTest("7777.77.88");
			gLAccount.AG_AccountType = AccountTypeComboBoxConstants.Consolidation;
			ConsolidationAccount = GetSavedGLAccountDescForTest("8888.88.88", "CLN", gLAccount.PK);
			Value.ConsolidationNum = "8888.88.88";
			Value.PercentNum = "8888.88.88";

			AccGLHeader gLAccount1 = GetSavedGLAccountForTest("7777.77.33");
			gLAccount1.AG_AccountType = AccountTypeComboBoxConstants.Alternate;
			AlternateAccount = GetSavedGLAccountDescForTest("3333.33.33", "ALT", gLAccount1.PK);
			Value.AlternativeNum = "3333.33.33";

			AccGLHeader gLAccount2 = GetSavedGLAccountForTest("7777.77.99");
			gLAccount2.AG_AccountType = AccountTypeComboBoxConstants.Total;
			TotalReferenceAccount = GetSavedGLAccountDescForTest("9999.99.99", "TTL", gLAccount2.PK);
			Value.HeaderDependsOnTotal = "9999.99.99";

			AccGLHeader gLAccount3 = GetSavedGLAccountForTest("7777.77.77");
			gLAccount3.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			GetSavedGLAccountDescForTest("7777.88.99", "P&L", gLAccount3.PK);
			Value.CarriedForwardAccount = "7777.88.99";

			Value.TotalLevel = 15;
			Value.PrintSequence = 5;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXXX.XX.XX");

			Value = new Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping();
			SetupXmlWithCorrectData();
			GLAccountDesc = Factory.New<AccGLAccountDescriptor>();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
		}

		AccGLAccountDescriptor GLAccountDesc;
		Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;
		AccGLHeader ParentAccount;
		AccGLAccountDescriptor ConsolidationAccount;
		AccGLAccountDescriptor AlternateAccount;
		AccGLAccountDescriptor TotalReferenceAccount;

		AccGLHeader GetSavedGLAccountForTest(ZString accNumber)
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount.AG_AccountNum = accNumber;
			return gLAccount;
		}

		AccGLAccountDescriptor GetSavedGLAccountDescForTest(ZString accNumber, ZString reportCategory, ZGuid parentAccountPK)
		{
			AccGLAccountDescriptor gLAccDesc = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			gLAccDesc.AJ_LocalAccountNumber = accNumber;
			gLAccDesc.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			gLAccDesc.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			gLAccDesc.AJ_ReportCategory = reportCategory;
			gLAccDesc.AJ_ReportType = "COA";
			gLAccDesc.ParentGLHeaderPK = parentAccountPK;
			return gLAccDesc;
		}

		GLAccountDescriptorDataAdapter fGLAccountDesc;
		GLAccountDescriptorDataAdapter GLAccountDescAdapter
		{
			get { return fGLAccountDesc ?? (fGLAccountDesc = new GLAccountDescriptorDataAdapter()); }
		}

		#endregion
	}
}
