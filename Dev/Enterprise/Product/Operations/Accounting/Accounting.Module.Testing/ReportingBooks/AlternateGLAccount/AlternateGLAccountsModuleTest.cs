using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateGLAccountsModule))]
	class AlternateGLAccountsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AlternateGLAccounts;
		}

		public void TestModuleID()
		{
			using (var module = new AlternateGLAccountsModule())
			{
				AssertEquals(ModuleIDs.AlternateGLAccounts, module.ID);
			}
		}

		public void TestLicenseCheckpoints()
		{
			using (var module = new AlternateGLAccountsModule())
			{
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new AlternateGLAccountsModule())
			{
				Assert(module.GridCollection is AlternateGLAccountCombineParentAccountCollection);
			}
		}

		public void TestPerformSearch()
		{
			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "10.00.1000");
			var account1 = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Credit, 1, AccGLHeader.Constants.SectionTypes.Codes.Assets, 2, "10.00.1010");
			Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader1.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account1, Creator.GLHeader1.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader2.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");

			Factory.Save();

			using (var alternateGLAccountsModule = new AlternateGLAccountsModule_ForTest())
			{
				alternateGLAccountsModule.PerformSearch_ForTest();
				AssertEquals("Grid Collection should contain 3 element", 3,
					((IFilterGridModuleInternalsForTesting)alternateGLAccountsModule).GridCollection.Count);

				account1.Delete();
				Factory.Save();

				alternateGLAccountsModule.AlternateGLAccountsModule_RefreshGrid_ForTestOnly(null, EventArgs.Empty);
				AssertEquals("Grid Collection should contain 2 element", 2,
					((IFilterGridModuleInternalsForTesting)alternateGLAccountsModule).GridCollection.Count);
			}
		}

		public void TestPerformSearch_DisposeModule()
		{
			using (var alternateGLAccountsModule = new AlternateGLAccountsModule_ForTest())
			{
				alternateGLAccountsModule.DisplayGrid.Dispose();
				Assert(alternateGLAccountsModule.DisplayGrid.IsDisposed);
				AssertNoExceptionThrown(() => alternateGLAccountsModule.AlternateGLAccountsModule_RefreshGrid_ForTestOnly(null, EventArgs.Empty));
			}
		}

		public void TestMenuItems()
		{
			using (var module = new AlternateGLAccountsModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("View"));
				AssertNotNull(module.ToolBarButtons.FindByText("New"));
				AssertNotNull(module.ToolBarButtons.FindByText("Edit"));
				AssertNotNull(module.ToolBarButtons.FindByText("Delete"));
				var actionsButton = module.ToolBarButtons.FindByText("&Actions");
				AssertNotNull(actionsButton);
				AssertNotNull(actionsButton.DropDownMenu.MenuItems.FindByText("Bulk Create Alternate GL Accounts"));
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
			Assert(Module.ModuleDecisionProvider.AllowExcelExport);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AlternateGLAccounts, Module.SecurityCheckpoint);
		}

		public void TestDeleteMultiple_DeleteAllAlternateAccountsWithCommonParent()
		{
			var chart = Creator.CreateAlternateChart("ABC");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "8AA8AA", "BSH", "DR", 1, "OV", 1);
			var glHeader = Creator.CreateGLHeader("1234.00.00");
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(account, glHeader.PK);
			var glHeader1 = Creator.CreateGLHeader("1234.10.00");
			var attribute1 = Creator.CreateAccAlternateGlAccountAttribute(account, glHeader1.PK);
			var account2 = Creator.CreateAccAlternateGlAccount(chart.PK, "6AA6AA", "P&L", "CR", 1, "OV", 2);
			var glHeader3 = Creator.CreateGLHeader("1234.10.10");
			var attribute2 = Creator.CreateAccAlternateGlAccountAttribute(account2, glHeader3.PK);
			Factory.Save();

			var alternateGlAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount.AlternateGLAccount = account;
			alternateGlAccountCombineParentAccount.GLHeaderPK = Creator.GLHeader1.PK;

			var alternateGlAccountCombineParentAccount1 = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount1.AlternateGLAccount = account2;
			alternateGlAccountCombineParentAccount1.GLHeaderPK = glHeader3.PK;

			Module.PerformSearch_ForTest();
			AssertEquals("Grid Collection should contain 3 element", 3,
				((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			Module.DeleteMultiple_ForTestOnly(new BusinessObject[] { alternateGlAccountCombineParentAccount, alternateGlAccountCombineParentAccount1 });
			AssertMultilineASCIIEquals(@"The selected Alternate Account(s) and it's related will be deleted:

Alternate Chart: ABC - Parent Account: 1234.00.00 - Alternate Account: 8AA8AA
Alternate Chart: ABC - Parent Account: 1234.10.00 - Alternate Account: 8AA8AA
Alternate Chart: ABC - Parent Account: 1234.10.10 - Alternate Account: 6AA6AA

Click 'Yes' to proceed, click 'No' to cancel the action.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

			Assert(account.IsDeleted);
			Assert(attribute.IsDeleted);
			Assert(attribute1.IsDeleted);
			Assert(account2.IsDeleted);
			Factory.Save();

			AssertEquals("Grid Collection should contain 0 element", 0, ((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);
		}

		public void TestDeleteMultiple_CanDeleteAndCannotDeleteMixed()
		{
			var chart = Creator.CreateAlternateChart("ABC");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "8AA8AA", "BSH", "DR", 1, "OV", 1);
			var glHeader = Creator.CreateGLHeader("1234.00.00");
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(account, glHeader.PK);
			var account2 = Creator.CreateAccAlternateGlAccount(chart.PK, "6AA6AA", "TTL", "CR", 1, "", 2);
			var account3 = Creator.CreateAccAlternateGlAccount(chart.PK, "9XX9XX", "P&L", "CR", 1, "OV", 2);
			account3.AGA_AGA_PercentNum = account2.PK;
			var glHeader3 = Creator.CreateGLHeader("1234.10.10");
			var attribute3 = Creator.CreateAccAlternateGlAccountAttribute(account3, glHeader3.PK);
			var deletedAccount = Creator.CreateAccAlternateGlAccount(chart.PK, "7AA7AA", "TTL", "CR", 1, "", 2);

			Factory.Save();

			var alternateGlAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount.AlternateGLAccount = account;
			alternateGlAccountCombineParentAccount.GLHeaderPK = Creator.GLHeader1.PK;

			var alternateGlAccountCombineParentAccount1 = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount1.AlternateGLAccount = account2;

			var deletedAlternateGlAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			deletedAlternateGlAccountCombineParentAccount.AlternateGLAccount = deletedAccount;

			Module.PerformSearch_ForTest();
			AssertEquals("Grid Collection should contain 4 element", 4,
				((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);

			deletedAccount.Delete();
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			Module.DeleteMultiple_ForTestOnly(new BusinessObject[] { alternateGlAccountCombineParentAccount, alternateGlAccountCombineParentAccount1, deletedAlternateGlAccountCombineParentAccount });
			AssertMultilineASCIIEquals(@"Some Alternate Accounts are already deleted, no action will be performed on them.
The selected Alternate Account(s) and it's related will be deleted:

Alternate Chart: ABC - Parent Account: 1234.00.00 - Alternate Account: 8AA8AA

The selected Alternate Account(s) cannot be deleted because it is used as either Alternate Number, Consolidate, Percent Number or Total Reference in Alternate Account:

Alternate Chart: ABC - Alternate Account: 6AA6AA

Click 'Yes' to proceed, click 'No' to cancel the action.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

			Assert(account.IsDeleted);
			Assert(!account2.IsDeleted);

			AssertEquals("Grid Collection should contain 2 element", 2, ((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);
		}

		public void TestDeleteMultiple_AllCannotDelete()
		{
			var chart = Creator.CreateAlternateChart("ABC");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "8AA8AA", "CLN", "DR", 1, "", 1);
			var account2 = Creator.CreateAccAlternateGlAccount(chart.PK, "9XX9XX", "P&L", "CR", 1, "OV", 2);
			account2.AGA_AGA_PercentNum = account.PK;
			var glHeader2 = Creator.CreateGLHeader("1234.10.00");
			var attribute2 = Creator.CreateAccAlternateGlAccountAttribute(account2, glHeader2.PK);

			var account3 = Creator.CreateAccAlternateGlAccount(chart.PK, "6AA6AA", "TTL", "CR", 1, "", 2);

			var account4 = Creator.CreateAccAlternateGlAccount(chart.PK, "1XX1XX", "P&L", "CR", 1, "OV", 2);
			account4.AGA_AGA_PercentNum = account3.PK;
			var glHeader3 = Creator.CreateGLHeader("1234.10.10");
			var attribute3 = Creator.CreateAccAlternateGlAccountAttribute(account4, glHeader3.PK);

			Factory.Save();

			var alternateGlAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount.AlternateGLAccount = account;

			var alternateGlAccountCombineParentAccount1 = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount1.AlternateGLAccount = account3;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Module.DeleteMultiple_ForTestOnly(new BusinessObject[] { alternateGlAccountCombineParentAccount, alternateGlAccountCombineParentAccount1 });
			AssertMultilineASCIIEquals(@"The selected Alternate Account(s) cannot be deleted because it is used as either Alternate Number, Consolidate, Percent Number or Total Reference in Alternate Account:

Alternate Chart: ABC - Alternate Account: 8AA8AA
Alternate Chart: ABC - Alternate Account: 6AA6AA", UnitTestUserNotification.Instance.LastMessage.Text);

			Assert(!account.IsDeleted);
			Assert(!account3.IsDeleted);
		}

		public void TestDeleteMultiple_CanDeleteDirectly()
		{
			var chart = Creator.CreateAlternateChart("ABC");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "8AA8AA", "CLN", "DR", 1, "", 1);
			var account2 = Creator.CreateAccAlternateGlAccount(chart.PK, "6AA6AA", "P&L", "CR", 1, "OV", 2);
			var glHeader2 = Creator.CreateGLHeader("1234.10.10");
			var attribute2 = Creator.CreateAccAlternateGlAccountAttribute(account2, glHeader2.PK);

			Factory.Save();

			var alternateGlAccountCombineParentAccount = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount.AlternateGLAccount = account;

			var alternateGlAccountCombineParentAccount1 = new AlternateGLAccountCombineParentAccount(Factory);
			alternateGlAccountCombineParentAccount1.AlternateGLAccount = account2;
			alternateGlAccountCombineParentAccount1.GLHeaderPK = glHeader2.PK;

			Module.PerformSearch_ForTest();
			AssertEquals("Grid Collection should contain 2 element", 2, ((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddUserResponse("yes");

			Module.DeleteMultiple_ForTestOnly(new BusinessObject[] { alternateGlAccountCombineParentAccount, alternateGlAccountCombineParentAccount1 });
			AssertMultilineASCIIEquals(@"The selected records will be deleted:

8AA8AA - desc
6AA6AA - desc", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Grid Collection should contain 0 element", 0, ((IFilterGridModuleInternalsForTesting)Module).GridCollection.Count);
		}

		#region Implementation

		AlternateGLAccountsModule_ForTest Module;
		TestObjectCreator Creator;

		protected override void SetUp()
		{
			base.SetUp();
			Module = new AlternateGLAccountsModule_ForTest();
			Creator = new TestObjectCreator(Factory);
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

	class AlternateGLAccountsModule_ForTest : AlternateGLAccountsModule
	{
		public bool GetAllowAdvancedDataAutomationWizard()
		{
			return AllowAdvancedDataAutomationWizard;
		}

		public void DeleteMultiple_ForTestOnly(BusinessObject[] selectedBusinessObjects)
		{
			DeleteMultiple(selectedBusinessObjects);
		}

		public void AlternateGLAccountsModule_RefreshGrid_ForTestOnly(object sender, EventArgs e)
		{
			AlternateGLAccountsModule_RefreshGrid(sender, e);
		}
	}
}
