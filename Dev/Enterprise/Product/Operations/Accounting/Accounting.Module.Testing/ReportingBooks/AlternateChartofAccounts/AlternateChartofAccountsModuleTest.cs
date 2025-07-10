using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateChartofAccountsModule))]
	class AlternateChartofAccountsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AlternateChartofAccounts;
		}

		public void TestModuleID()
		{
			using (var module = new AlternateChartofAccountsModule())
			{
				AssertEquals(ModuleIDs.AlternateChartofAccounts, module.ID);
			}
		}

		public void TestLicenseCheckpoints()
		{
			using (var module = new AlternateChartofAccountsModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new AlternateChartofAccountsModule())
			{
				Assert(module.GridCollection is AccAlternateChartCollection);
			}
		}

		public void TestHandleDeleteClickCore()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT");
			Factory.Save();
			using (var testModule = new AlternateChartofAccountsModule_ForTest())
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var testCollection = (BusinessObjectCollection)testModule.GridCollection;
				testModule.PerformSearch_ForTest();
				testCollection.Load();
				testModule.DisplayGrid.SelectAllElements();

				testModule.HandleDeleteClickCore_ForTestOnly(null, null);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				var alternateGLAccount = creator.CreateAccAlternateGlAccount(chart.PK, "12", Core.Constants.AccountType.BalanceSheetAccount);
				Factory.Save();
				testModule.DisplayGrid.SelectAllElements();
				testModule.HandleDeleteClickCore_ForTestOnly(null, null);
				AssertEquals("Can't delete Alternate Chart of Account because Reporting Books or Alternate GL Accounts are using this chart. Please remove this chart from all Reporting Books and delete all its Alternate GL Account before deleting.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				alternateGLAccount.Delete();
				var reportingBook = creator.CreateReportingBook("1", "1", chart.PK, "UUU");
				reportingBook.ARB_AAC_AlternateChart = chart.PK;
				Factory.Save();
				testModule.DisplayGrid.SelectAllElements();
				testModule.HandleDeleteClickCore_ForTestOnly(null, null);
				AssertEquals("Can't delete Alternate Chart of Account because Reporting Books or Alternate GL Accounts are using this chart. Please remove this chart from all Reporting Books and delete all its Alternate GL Account before deleting.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((IFilterModuleInternalsForTesting)testModule).LastController.LastShownForm.Dispose();
			}
		}

		public void TestAllowedActions()
		{
			Assert(Module.HasActions);
			Assert(Module.AllowView);
			Assert(Module.AllowNew);
			Assert(Module.AllowEdit);
			Assert(Module.AllowDelete);
			Assert(!Module.AllowUniversalCopy);
			Assert(!Module.GetAllowAdvancedDataAutomationWizard());
			Assert(Module.AllowCopyFilterGridHyperlinkToClipboard);
			using (var module = new AlternateChartofAccountsModule())
			{
				Assert(module.ModuleDecisionProvider.AllowExcelExport);
			}
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AlternateChartofAccounts, Module.SecurityCheckpoint);
		}

		#region Implementation

		AlternateChartofAccountsModule_ForTest Module;

		protected override void SetUp()
		{
			base.SetUp();
			Module = new AlternateChartofAccountsModule_ForTest();
		}

		protected override void TearDown()
		{
			if (Module != null)
			{
				Module.Dispose();
			}
			base.TearDown();
		}

		#endregion
	}

	class AlternateChartofAccountsModule_ForTest : AlternateChartofAccountsModule
	{
		public bool GetAllowAdvancedDataAutomationWizard()
		{
			return AllowAdvancedDataAutomationWizard;
		}

		public void HandleDeleteClickCore_ForTestOnly(object sender, EventArgs e)
		{
			HandleDeleteClickCore(sender, e);
		}
	}
}
