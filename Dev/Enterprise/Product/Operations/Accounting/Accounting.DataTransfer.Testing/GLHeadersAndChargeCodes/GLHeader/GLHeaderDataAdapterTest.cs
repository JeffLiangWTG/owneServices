using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing.GLHeadersAndChargeCodes.GLHeader
{
	[TestedType(typeof(GLHeaderDataAdapter))]
	sealed class GLHeaderDataAdapterTest : BaseAccountingDataAdapterTest<AccGLHeader, Xsd.GLHeadersGLHeader>
	{
		public void TestProcessHeader_SetStatisticalUnitsOverMaxLength()
		{
			var adapter = new GLHeaderDataAdapter();
			var value = new Xsd.GLHeadersGLHeader();
			value.AccountType = Core.Constants.AccountType.Note;

			GLHeader = Factory.New<AccGLHeader>();
			value.StatisticalUnits = "ThisIsTooLong";
			AssertNoExceptionThrown(() => adapter.ProcessHeader_ForTestOnly(GLHeader, value, Context));
			AssertEquals("Thi", GLHeader.AG_StatisticalUnits);
			AssertHasError(GLHeader.AG_StatisticalUnitsInfo, "Enter a valid Units.");
			Assert(((NotificationBuffer)(Context.Notifications)).AsString.Contains("Warning: Maximum length of this field has been exceeded (value=ThisIsTooLong)"));

			GLHeader = Factory.New<AccGLHeader>();
			value.StatisticalUnits = "KWHIsTooLong";
			AssertNoExceptionThrown(() => adapter.ProcessHeader_ForTestOnly(GLHeader, value, Context));
			AssertEquals("KWH", GLHeader.AG_StatisticalUnits);
			AssertNoError(GLHeader.AG_StatisticalUnitsInfo, "Enter a valid Units.");
			Assert(((NotificationBuffer)(Context.Notifications)).AsString.Contains("Warning: Maximum length of this field has been exceeded (value=ThisIsTooLong)"));
		}

		public void TestImportFromValueObjectCore()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);

			AssertEquals("6666.66.66", GLHeader.AG_AccountNum);
			AssertEquals(Core.Constants.DebitCredit.Debit, GLHeader.AG_DebitCredit);
			AssertEquals("Test Account Name", GLHeader.AG_Description);
			AssertEquals(Core.Constants.AccountType.Total, GLHeader.AG_AccountType);
			AssertEquals(15, GLHeader.AG_TotalLevel);
			AssertEquals(Core.Constants.BooleanTrueString, GLHeader.AG_ControlAccount.ToString());
			AssertEquals(Core.Constants.BooleanFalseString, GLHeader.AG_DisallowDirectPosting.ToString());
			AssertEquals(5, GLHeader.AG_PrintSequence);
			AssertEquals("TS", GLHeader.AG_Column);
			AssertEquals(0, GLHeader.Notifications.GetErrors().Count());
			AssertNullOrEmpty(GLHeader.AG_StatisticalUnits);
		}

		public void TestImportFromValueObjectCore_ForNTE()
		{
			Value.AccountType = Core.Constants.AccountType.Note;
			Value.SubAccountType = Core.Constants.SubAccountType.StaffAndResources;
			Value.IsSubAccountMandatory = "Y";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);

			AssertEquals("6666.66.66", GLHeader.AG_AccountNum);
			AssertEquals(Core.Constants.DebitCredit.Debit, GLHeader.AG_DebitCredit);
			AssertEquals("Test Account Name", GLHeader.AG_Description);
			AssertEquals(Core.Constants.AccountType.Note, GLHeader.AG_AccountType);
			AssertEquals(0, GLHeader.AG_TotalLevel);
			AssertEquals(Core.Constants.BooleanTrueString, GLHeader.AG_ControlAccount.ToString());
			AssertEquals(Core.Constants.BooleanFalseString, GLHeader.AG_DisallowDirectPosting.ToString());
			AssertEquals(5, GLHeader.AG_PrintSequence);
			AssertEquals("TS", GLHeader.AG_Column);

			AssertNullOrEmpty(GLHeader.AG_CashFlowType);
			AssertEquals("KWH", GLHeader.AG_StatisticalUnits);
			AssertEquals(0, GLHeader.Notifications.GetErrors().Count());
		}

		public void TestSetReferencesToGLAccounts()
		{
			Value.AccountType = Core.Constants.AccountType.BalanceSheetAccount;

			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ConsolidationAccount.PK, GLHeader.AG_AG_PercentNum);
			AssertEquals(ConsolidationAccount.PK, GLHeader.AG_AG_ConsolidationNum);
			AssertEquals(AlternateAccount.PK, GLHeader.AG_AG_AlternateNum);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_HeaderDependsOnTotal);

			GLHeader = Factory.New<AccGLHeader>();
			Value.AccountType = Core.Constants.AccountType.Header;

			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ConsolidationAccount.PK, GLHeader.AG_AG_PercentNum);
			AssertEquals(ConsolidationAccount.PK, GLHeader.AG_AG_ConsolidationNum);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_AlternateNum);
			AssertEquals(TotalReferenceAccount.PK, GLHeader.AG_AG_HeaderDependsOnTotal);

			GLHeader = Factory.New<AccGLHeader>();
			Value.AccountType = Core.Constants.AccountType.Note;
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_AlternateNum);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_HeaderDependsOnTotal);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_PercentNum);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_ConsolidationNum);
		}

		public void TestIncorrectStatisticalUnits()
		{
			//Valid StatisticalUnits
			Value.AccountType = Core.Constants.AccountType.Note;
			Value.StatisticalUnits = "KWH";
			GLHeaderAdapter.ProcessHeader_ForTestOnly(GLHeader, Value, Context);
			AssertEquals("KWH", GLHeader.AG_StatisticalUnits);

			//Invalid StatisticalUnits
			GLHeader = Factory.New<AccGLHeader>();
			Value.StatisticalUnits = "111";
			GLHeaderAdapter.ProcessHeader_ForTestOnly(GLHeader, Value, Context);
			AssertHasError(GLHeader.AG_StatisticalUnitsInfo, "Enter a valid Units.");

			//Empty StatisticalUnits
			GLHeader = Factory.New<AccGLHeader>();
			Value.StatisticalUnits = "";
			GLHeaderAdapter.ProcessHeader_ForTestOnly(GLHeader, Value, Context);
			Assert(((NotificationBuffer)(Context.Notifications)).AsString.Contains("Statistical Units can not be empty when Account Type is NTE."));

			//Not NTE Type 
			GLHeader = Factory.New<AccGLHeader>();
			Value.AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Value.StatisticalUnits = "111";
			GLHeaderAdapter.ProcessHeader_ForTestOnly(GLHeader, Value, Context);
			AssertEquals(false, GLHeader.AG_StatisticalUnitsInfo.Notifications.HasErrors());
			AssertNullOrEmpty(GLHeader.AG_StatisticalUnits);
		}

		public void TestIncorrectAccountNumber()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_AccountNumInfo.HasErrors());

			Value.AccNumber = "XXXX.XX.XX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_AccountNumInfo.HasErrors());
		}

		public void TestIncorrectDebitCredit()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_DebitCreditInfo.HasErrors());

			Value.DebitCredit = "XXX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_DebitCreditInfo.HasErrors());
		}

		public void TestIncorrectDescription()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_DescriptionInfo.HasErrors());

			for (int i = 0; i < AccGLHeaderSchema.AG_Description.MaxLength; i++)
			{
				Value.Description += "a";
			}
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(Buffer.HasWarnings);
		}

		public void TestIncorrectAccounttype()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_AccountTypeInfo.HasErrors());

			Value.AccountType = "XXX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_AccountTypeInfo.HasErrors());

			Value.AccountType = Core.Constants.AccountType.Note;
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_AccountTypeInfo.HasErrors());
		}

		public void TestIncorrectCashFlowType()
		{
			Assert("Precondition", Value.AccountType != Core.Constants.AccountType.BalanceSheetAccount
				&& Value.AccountType != Core.Constants.AccountType.ProfitAndLossAccount);

			Value.CashFlowType = "ABC";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertHasError(GLHeader.AG_CashFlowTypeInfo, "cash flow type is only applicable for 'P&L' and 'BSH' type GL Account.");

			Value.AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			Value.CashFlowType = "";
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertNoErrors(GLHeader.AG_CashFlowTypeInfo);
			AssertEquals("XXX", GLHeader.AG_CashFlowType);

			Value.CashFlowType = "CBA";
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertHasError(GLHeader.AG_CashFlowTypeInfo, "Enter a valid Cash Flow Cat..");
		}

		public void TestIncorrectConsolidationNum()
		{
			GLHeader.AG_AG_ConsolidationNumList.Load();
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertNoErrors(GLHeader.AG_AG_ConsolidationNumInfo);

			Value.ConsolidationNum = "XXXX.XX.XX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_ConsolidationNum);
		}

		public void TestIncorrectPercentNum()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertNoErrors(GLHeader.AG_AG_PercentNumInfo);

			Value.PercentNum = "XXXX.XX.XX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_PercentNum);
		}
		public void TestIncorrectAlternateNum()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			Assert(!GLHeader.AG_AG_AlternateNumInfo.HasErrors());

			Value.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			Value.AlternateNum = "XXXX.XX.XX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_AlternateNum);
		}
		public void TestIncorrectHeaderDependsOnTotal()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			Assert(!GLHeader.AG_AG_HeaderDependsOnTotalInfo.HasErrors());

			Value.AccountType = Core.Constants.AccountType.Header;
			Value.HeaderDependsOnTotal = "XXXX.XX.XX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(GLHeader, Value, Context);
			AssertEquals(ZGuid.Empty, GLHeader.AG_AG_HeaderDependsOnTotal);
		}

		public void TestIncorrectTotalLevel()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_TotalLevelInfo.HasErrors());

			Value.AccountType = Core.Constants.AccountType.Consolidation;
			Value.TotalLevel = -99999999;
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertEquals(0, GLHeader.AG_TotalLevel);

			Value.AccountType = Core.Constants.AccountType.Total;
			Value.TotalLevel = -99999999;

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_TotalLevelInfo.HasErrors());
		}

		public void TestIncorrectControlAccount()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_ControlAccountInfo.HasErrors());

			Value.ControlAccount = "XXX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertEquals(Core.Constants.BooleanFalseString, GLHeader.AG_ControlAccount.ToString());
		}

		public void TestIncorrectDisallowDirectPosting()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_DisallowDirectPostingInfo.HasErrors());

			Value.DisallowDirectPosting = "XXX";

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			AssertEquals(Core.Constants.BooleanFalseString, GLHeader.AG_DisallowDirectPosting.ToString());
		}

		public void TestIncorrectPrintSequence()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_PrintSequenceInfo.HasErrors());

			Value.PrintSequence = 1000;

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_PrintSequenceInfo.HasErrors());
		}

		public void TestAddErrorsToNotifications()
		{
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(!GLHeader.AG_PrintSequenceInfo.HasErrors());

			Value.PrintSequence = 1000;

			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(GLHeader, Value, Context);
			Assert(GLHeader.AG_PrintSequenceInfo.HasErrors());
			Assert(Buffer.AsString.Contains(GLHeader.AG_PrintSequenceInfo.GetErrors().GetFirstMessage()));
		}

		public void TestNotifyBizObjCreatedOrUpdated()
		{
			GLHeaderAdapter.NotifyBizObjCreatedOrUpdated_ForTestOnly(Buffer, new BusinessObjectThatDoesntSave(Factory));
			AssertEquals("The adapter must not add message about creation BusinessObjectThatDoesntSave.", 0, Buffer.Events.Length);
		}

		public void TestDuplicateAlternateNum()
		{
			const string baseAcct = "2014.09.10";
			const string Acct1 = "2014.09.11";
			const string Acct2 = "2014.09.12";

			var header = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader = NewGLHeadersGLHeaderForAltNumTest(baseAcct, ZString.Empty);
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should not have errors", !Context.NotificationsHasErrors);

			header = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader = NewGLHeadersGLHeaderForAltNumTest(Acct1, baseAcct);
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should not have errors", !Context.NotificationsHasErrors);

			header = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader = NewGLHeadersGLHeaderForAltNumTest(Acct2, baseAcct);
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Alternate Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains
				("Error - AG_AG_AlternateNum: This alternate number is already used for another GL Account"));

			((NotificationBuffer)(Context.Notifications)).Clear();
			Assert("Should not have errors", !Context.NotificationsHasErrors);
			Factory.Save();
			header = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader = NewGLHeadersGLHeaderForAltNumTest(Acct2, baseAcct);
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Alternate Number error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains
				("Error - AG_AG_AlternateNum: This alternate number is already used for another GL Account"));
		}

		[GuiTest]
		public void TestImportCSVFile()
		{
			string testCSVBody = @"GLACCOUNT,8110.10.10,CR,test ALT,ALT,,,,,0,N,N,0,TS,,Y,,
								GLACCOUNT,6111.00.11,CR,test1,BSH,,,8111.00.11,,0,N,N,0,TS,STR,Y,XXX,";
			DataImporterBusinessObject businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());

			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8110.10.10");
			AccGLHeader existGLHeader = Factory.LoadTop1<AccGLHeader>(query);
			AssertNotNull(existGLHeader);

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				using (TestDataImporterForm form = new TestDataImporterForm(businessEntity, "Import CSV Account Chart", BillingInterfaceName.CSVAccountChartImport)) // Interface name for billing purposes
				{
					GLHeaderAndChargeCodeFlatFileDataImporter importer = new GLHeaderAndChargeCodeFlatFileDataImporter();
					form.Importer = importer;

					form.Show();
					Application.DoEvents();
					File.WriteAllText(tempFile.Filename, testCSVBody);

					form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(true, true);
					form.ImportFromFile(tempFile.Filename);
					query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6111.00.11");
					AccGLHeader newGLHeader = Factory.LoadTop1<AccGLHeader>(query);
					AssertNull(newGLHeader);

					form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(false, true);
					form.ImportFromFile(tempFile.Filename);
					query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6111.00.11");
					newGLHeader = Factory.LoadTop1<AccGLHeader>(query);
					AssertNotNull(newGLHeader);
				}
			}
		}

		public void TestIncorrectCompanyFilterList()
		{
			const string baseAcct = "2014.12.30";
			var header = Factory.New<AccGLHeader>();
			var glHeader = NewGLHeadersGLHeaderForAltNumTest(baseAcct, ZString.Empty);
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should not have errors", !Context.NotificationsHasErrors);

			glHeader.CompanyFilterList = "ABC";
			GLHeaderAdapter.ImportFromValueObjectCore_ForTestOnly(header, glHeader, Context);
			GLHeaderAdapter.SetReferencesToGLAccounts(header, glHeader, Context);
			Assert("Should have errors", Context.NotificationsHasErrors);
			Assert("Should have Company Filter error", ((NotificationBuffer)(Context.Notifications)).AsString.Contains
				("Error: GL Header Code 2014.12.30 - Company Filter includes invalid companies."));
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
			get { return GLHeaderAdapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return GLHeaderAdapter.RootElementName; }
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

		protected override ValueObjectDataAdapter<AccGLHeader, Xsd.GLHeadersGLHeader> GetNewBizObjXmlDataAdapter()
		{
			return new GLHeaderDataAdapter();
		}

		#endregion

		#region Implementation

		void SetupXmlWithCorrectData()
		{
			Value.AccNumber = "6666.66.66";
			Value.DebitCredit = Core.Constants.DebitCredit.Debit;
			Value.Description = "Test Account Name";
			Value.AccountType = Core.Constants.AccountType.Total;
			Value.Section = "TS";

			ConsolidationAccount = GetSavedGLAccountForTest("8888.88.88");
			ConsolidationAccount.AG_AccountType = "CLN";
			ConsolidationAccount.AG_Column = "AS";
			Value.ConsolidationNum = "8888.88.88";
			Value.PercentNum = "8888.88.88";

			AlternateAccount = GetSavedGLAccountForTest("3333.33.33");
			Value.AlternateNum = "3333.33.33";

			TotalReferenceAccount = GetSavedGLAccountForTest("9999.99.99");
			TotalReferenceAccount.AG_AccountType = "TTL";
			TotalReferenceAccount.AG_Column = "AS";
			Value.HeaderDependsOnTotal = "9999.99.99";

			Value.TotalLevel = 15;

			Value.ControlAccount = Core.Constants.BooleanTrueString;
			Value.DisallowDirectPosting = Core.Constants.BooleanFalseString;

			Value.PrintSequence = 5;
			Value.StatisticalUnits = "KWH";
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXXX.XX.XX");
			GLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			Value = new Xsd.GLHeadersGLHeader();
			SetupXmlWithCorrectData();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
		}

		AccGLHeader GLHeader;
		Xsd.GLHeadersGLHeader Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;

		AccGLHeader ConsolidationAccount;
		AccGLHeader AlternateAccount;
		AccGLHeader TotalReferenceAccount;

		AccGLHeader GetSavedGLAccountForTest(ZString accNumber)
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_AccountNum = accNumber;
			return gLHeader;
		}

		GLHeaderDataAdapter fGLHeader;
		GLHeaderDataAdapter GLHeaderAdapter
		{
			get
			{
				if (fGLHeader == null)
				{
					fGLHeader = new GLHeaderDataAdapter();
				}

				return fGLHeader;
			}
		}

		protected override string[] StringFieldsToIgnore
		{
			get { return new string[] { "SubAccountType", "CashFlowType", "CompanyFilterList" }; }
		}

		Xsd.GLHeadersGLHeader NewGLHeadersGLHeaderForAltNumTest(ZString accNumber, ZString alternateNum)
		{
			var xsdHeader = new Xsd.GLHeadersGLHeader();
			xsdHeader.AccNumber = accNumber;
			xsdHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			xsdHeader.Description = "Test Account Name";
			xsdHeader.Section = "TS";
			xsdHeader.AlternateNum = alternateNum;
			xsdHeader.IsSubAccountMandatory = Core.Constants.BooleanFalseString;
			xsdHeader.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			xsdHeader.CashFlowType = "O01";
			xsdHeader.CompanyFilterList = "EDI";

			xsdHeader.PercentNum = "8888.88.88";
			xsdHeader.ConsolidationNum = "8888.88.88";

			return xsdHeader;
		}

		#endregion
	}

	class TestDataImporterForm : DataImporterForm
	{
		public TestDataImporterForm() : base()
		{
		}

		public TestDataImporterForm(string formCaption, BillingInterfaceName interfaceName)
			: base(formCaption, interfaceName)
		{
		}

		public TestDataImporterForm(DataImporterBusinessObject businessEntity, string formCaption,
			BillingInterfaceName interfaceName)
			: base(businessEntity, formCaption, interfaceName)
		{
		}

		internal new void ImportFromFile(ZString fileName)
		{
			base.ImportFromFile(fileName);
		}
	}
}
