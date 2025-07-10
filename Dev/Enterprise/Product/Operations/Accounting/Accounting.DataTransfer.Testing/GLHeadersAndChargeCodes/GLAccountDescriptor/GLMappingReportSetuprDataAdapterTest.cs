using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	[TestedType(typeof(GLMappingReportSetupDataAdapter))]
	public class GLMappingReportSetuprDataAdapterTest : ValueObjectDataAdapterTest<GLDescriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup>
	{
		public void TestImportFromValueObjectCore()
		{
			AccGLAccountDescriptor accGLAccountDescriptor = TestMappingAccount.Clone() as AccGLAccountDescriptor;
			accGLAccountDescriptor.AJ_ReportType = "XXX";
			accGLAccountDescriptor.AJ_ReportCategory = "XXX";

			GLAccountDesc.YJ_AG = ParentAccount.PK;
			GLAccountDesc.YJ_AJ = accGLAccountDescriptor.PK;

			GLAccountDescAdapter.ImportFromValueObjectCore_ForTestOnly(GLAccountDesc, Value, Context);

			AssertEquals("TT0", GLAccountDesc.ReportType);
			AssertEquals("A01", GLAccountDesc.ReportCategory);
			AssertEquals("8888.88.88", GLAccountDesc.GLAccountDescriptor.AJ_LocalAccountNumber);

			AssertEquals("TT0", accGLAccountDescriptor.AJ_ReportType);
			AssertEquals("A01", accGLAccountDescriptor.AJ_ReportCategory);
		}

		public void TestNotifyBizObjCreatedOrUpdated()
		{
			GLAccountDescAdapter.NotifyBizObjCreatedOrUpdated_ForTestOnly(Buffer, new BusinessObjectThatDoesntSave(Factory));
			AssertEquals("The adapter must not add message about creation BusinessObjectThatDoesntSave.", 0, Buffer.Events.Length);
		}

		#region Base Tests

		public new void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			AssertEquals(true, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
		}

		public new void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked()
		{
			IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
			AssertEquals(false, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);

			AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, adapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);
		}

		protected override bool IsCreateOrUpdateFromValueObjectSupported
		{
			get
			{
				return false;
			}
		}

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

		protected override ValueObjectDataAdapter<GLDescriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup> GetNewBizObjXmlDataAdapter()
		{
			return new GLMappingReportSetupDataAdapter();
		}

		#endregion

		#region Implementation

		void SetupXmlWithCorrectData()
		{
			Value.Language = Core.Constants.Languages.ChineseSimplified;
			Value.LocalAccountNumber = "8888.88.88";
			Value.ReportType = "TT0";
			Value.ReportCategory = "A01";
			ParentAccount = GetSavedGLAccountForTest("1111.22.33");
			ParentAccount.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestMappingAccount = GetSavedGLAccountDescForTest("8888.88.88", "COA", "BSH", ParentAccount.PK);

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXXX.XX.XX");

			Value = new Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup();
			SetupXmlWithCorrectData();
			GLAccountDesc = Factory.New<GLDescriptorPivot>();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
		}

		GLDescriptorPivot GLAccountDesc;
		Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;
		AccGLHeader ParentAccount;
		AccGLAccountDescriptor TestMappingAccount;

		AccGLHeader GetSavedGLAccountForTest(ZString accNumber)
		{
			AccGLHeader gLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			gLAccount.AG_AccountNum = accNumber;
			return gLAccount;
		}

		AccGLAccountDescriptor GetSavedGLAccountDescForTest(ZString accNumber, ZString reportType, ZString reportCategory, ZGuid parentAccountPK)
		{
			AccGLAccountDescriptor gLAccDesc = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			gLAccDesc.AJ_LocalAccountNumber = accNumber;
			gLAccDesc.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			gLAccDesc.AJ_ReportCategory = reportCategory;
			gLAccDesc.AJ_ReportType = reportType;
			gLAccDesc.ParentGLHeaderPK = parentAccountPK;
			return gLAccDesc;
		}

		GLMappingReportSetupDataAdapter fGLAccountDesc;
		GLMappingReportSetupDataAdapter GLAccountDescAdapter
		{
			get { return fGLAccountDesc ?? (fGLAccountDesc = new GLMappingReportSetupDataAdapter()); }
		}

		#endregion
	}
}
