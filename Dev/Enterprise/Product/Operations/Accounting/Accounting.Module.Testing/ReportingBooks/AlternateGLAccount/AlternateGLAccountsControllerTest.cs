using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateGLAccountsController))]
	internal class AlternateGLAccountsControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(AlternateGLAccountCombineParentAccount);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AlternateGLAccounts;
		}

		public void TestCheckPoint()
		{
			AssertEquals(Env.Security.AlternateGLAccountsView, TestController.CheckPointForView_ForTest);
			AssertEquals(Env.Security.AlternateGLAccountsNew, TestController.CheckPointForNew_ForTest);
			AssertEquals(Env.Security.AlternateGLAccountsEdit, TestController.CheckPointForEdit_ForTest);
			AssertEquals(Env.Security.AlternateGLAccountsDelete, TestController.CheckPointForDelete_ForTest);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(AlternateGLAccountCombineParentAccount), TestController.TypeOfTopLevelBusinessObject);
		}

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.AlternateGLAccounts, TestController.ModuleID);
		}

		public void TestGetLoadedBusinessEntityInLocalFactory()
		{
			var alternateGLAccountConbineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			var alternateGLAccount = Factory.NewWithValidTestData<AccAlternateGLAccount>();
			Factory.Save();

			alternateGLAccountConbineParentAccount = TestController.GetLoadedBusinessEntityInLocalFactoryForTest(alternateGLAccount) as AlternateGLAccountCombineParentAccount;
			AssertEquals(alternateGLAccountConbineParentAccount.PK, TestController.GetLoadedBusinessEntityInLocalFactoryForTest(alternateGLAccountConbineParentAccount).Identifier);
			AssertEquals(alternateGLAccountConbineParentAccount.Factory, TestController.GetLoadedBusinessEntityInLocalFactoryForTest(alternateGLAccountConbineParentAccount).Factory);
			AssertNotNull(alternateGLAccountConbineParentAccount);
			AssertEquals(alternateGLAccountConbineParentAccount.AlternateGLAccount.PK, alternateGLAccount.PK);
			AssertEquals(alternateGLAccountConbineParentAccount.GLHeaderPK, ZGuid.Empty);

			var gLHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var attribute = alternateGLAccount.AlternateGLAccountAttributes.AddNew();
			attribute.AAA_AAC_AlternateChart = alternateGLAccount.AGA_AAC_AlternateChart;
			attribute.AAA_AG_GLHeader = gLHeader1.PK;
			attribute.AAA_Sequence = 1;
			Factory.Save();

			alternateGLAccountConbineParentAccount = TestController.GetLoadedBusinessEntityInLocalFactoryForTest(alternateGLAccount) as AlternateGLAccountCombineParentAccount;
			AssertNotNull(alternateGLAccountConbineParentAccount);
			AssertEquals(alternateGLAccountConbineParentAccount.AlternateGLAccount.PK, alternateGLAccount.PK);
			AssertEquals(alternateGLAccountConbineParentAccount.GLHeaderPK, gLHeader1.PK);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = alternateGLAccount.AGA_AAC_AlternateChart;
			alternateGLAccounts.ParentGLAccountPK = gLHeader1.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = alternateGLAccount;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			alternateGLAccountConbineParentAccount = TestController.GetLoadedBusinessEntityInLocalFactoryForTest(alternateGLAccounts) as AlternateGLAccountCombineParentAccount;
			AssertNotNull(alternateGLAccountConbineParentAccount);
			AssertEquals(alternateGLAccountConbineParentAccount.AlternateGLAccount.PK, alternateGLAccount.PK);
			AssertEquals(alternateGLAccountConbineParentAccount.GLHeaderPK, gLHeader1.PK);

			alternateGLAccountConbineParentAccount = TestController.GetLoadedBusinessEntityInLocalFactoryForTest(attribute) as AlternateGLAccountCombineParentAccount;
			AssertNotNull(alternateGLAccountConbineParentAccount);
			AssertEquals(alternateGLAccountConbineParentAccount.AlternateGLAccount.PK, alternateGLAccount.PK);
			AssertEquals(alternateGLAccountConbineParentAccount.GLHeaderPK, gLHeader1.PK);
		}

		public void TestGetNewBusinessEntityInLocalFactoryForTest()
		{
			Assert(TestController.GetNewBusinessEntityInLocalFactoryForTest() is AlternateGLAccounts);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting");
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			var glHeader = TestObjectCreator.CreateGLHeader("Test.aa");

			Factory.Save();

			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);

			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.LOC);

			Factory.Save();

			var alternateGLAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGLAccountCombineParentAccount.GLHeaderPK = glHeader.PK;
			alternateGLAccountCombineParentAccount.AlternateGLAccount = alternateGLAccount;

			return alternateGLAccountCombineParentAccount;
		}

		public void TestGetForm()
		{
			using (var testForm = TestController.GetForm_ForTestOnly(new AlternateGLAccounts(Factory)))
			{
				AssertEquals(typeof(AlternateGLAccountsForm), testForm.GetType());
				var alternateGLAccounts = testForm.BusinessEntityForPersistingForm as AlternateGLAccounts;
				AssertNotNull(alternateGLAccounts);
				AssertEquals(false, alternateGLAccounts.IsInDb);
				AssertNull(alternateGLAccounts.OriginalParentGLAccount);
				AssertEquals(ZGuid.Empty, alternateGLAccounts.OriginalParentGLAccountPK);
			}

			var alternateGLAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);
			alternateGLAccountCombineParentAccount.GLHeaderPK = testObjectCreator.GLHeader1.PK;
			using (var testForm = TestController.GetForm_ForTestOnly(alternateGLAccountCombineParentAccount))
			{
				AssertEquals(typeof(AlternateGLAccountsForm), testForm.GetType());
				var alternateGLAccounts = testForm.BusinessEntityForPersistingForm as AlternateGLAccounts;
				AssertNotNull(alternateGLAccounts);
				AssertEquals(true, alternateGLAccounts.IsInDb);
				AssertEquals(testObjectCreator.GLHeader1.PK, alternateGLAccounts.OriginalParentGLAccountPK);
			}
		}

		public void TestGetForm_RelatedAlternateGLAccountOnlyContainExistedAlternateGLAccount()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting");
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var account1 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var account2 = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", "BSH", "DR", 1, "OV", 2);

			var glHeader = TestObjectCreator.CreateGLHeader("Test.aa");
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			TestObjectCreator.CreateAccAlternateGLAccountDissection(glHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);

			TestObjectCreator.CreateAccAlternateGlAccountAttribute(account1, glHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.LOC);
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(account2, glHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.LOC);
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(account2, glHeader.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.WEU);
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(account2, glHeader.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.FOR);
			Factory.Save();

			var alternateGLAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGLAccountCombineParentAccount.GLHeaderPK = glHeader.PK;
			alternateGLAccountCombineParentAccount.AlternateGLAccount = account1;
			using (var testForm = TestController.GetForm_ForTestOnly(alternateGLAccountCombineParentAccount))
			{
				AssertEquals(typeof(AlternateGLAccountsForm), testForm.GetType());
				var alternateGLAccounts = testForm.BusinessEntityForPersistingForm as AlternateGLAccounts;
				AssertNotNull(alternateGLAccounts);
				AssertEquals(true, alternateGLAccounts.IsInDb);
				AssertEquals(2, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count);

				var alternateGLAccountsWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>();
				Assert(alternateGLAccountsWithAttributeSet.Any(x => x.AlternateGLAccount.IsInDatabase && x.AlternateGLAccount.PK == account1.PK));
				Assert(alternateGLAccountsWithAttributeSet.Any(x => x.AlternateGLAccount.IsInDatabase && x.AlternateGLAccount.PK == account2.PK));
			}
		}

		public void TestSavedForAlternateGLAccountsForm()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting");
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			var glHeader = TestObjectCreator.CreateGLHeader("Test.aa");

			Factory.Save();

			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var attribute = TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK, 1, string.Empty, string.Empty);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			alternateGLAccounts.ParentGLAccountPK = glHeader.PK;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccount.AGA_AccountNum;

			var executeCount = 0;

			using (var newForm = TestController.GetForm_ForTestOnly(alternateGLAccounts))
			{
				TestController.RefreshGrid += AlternateGLAccountController_RefreshGrid;
				AssertEquals("Pre-condition: Should return AlternateGLAccountsForm for AlternateGLAccounts.", true, newForm is AlternateGLAccountsForm);
				AssertEquals("Pre-condition: executeCount should be 0 before factory saved.", 0, executeCount);
				Factory.Save();
				AssertEquals("Pre-condition: executeCount should be 1 after factory saved.", 1, executeCount);
			}

			void AlternateGLAccountController_RefreshGrid(object sender, EventArgs e)
			{
				executeCount++;
			}
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		AlternateGLAccountsControllerForTest fTestController;
		AlternateGLAccountsControllerForTest TestController
		{
			get
			{
				if (fTestController == null)
				{
					fTestController = new AlternateGLAccountsControllerForTest();
				}
				return fTestController;
			}
		}

		class AlternateGLAccountsControllerForTest : AlternateGLAccountsController
		{
			public SecurityCheckpoint CheckPointForView_ForTest => CheckPointForView;

			public SecurityCheckpoint CheckPointForNew_ForTest => CheckPointForNew;

			public SecurityCheckpoint CheckPointForEdit_ForTest => CheckPointForEdit;

			public SecurityCheckpoint CheckPointForDelete_ForTest => CheckPointForDelete;

			public IBusiness GetLoadedBusinessEntityInLocalFactoryForTest(IBusiness sourceEntity)
			{
				return GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
			{
				return GetNewBusinessEntityInLocalFactory();
			}

			public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
			{
				return GetForm(businessEntity);
			}
		}
	}
}
