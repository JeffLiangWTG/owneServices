using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class OperationalactionsCustomisationMenuMakerTest : TestCase
	{
		public void TestCustomise()
		{
			Menu.MenuItems.AddRange(Maker.Make());
			MenuItem customiseMenuItem = Menu.MenuItems[1];
			AssertEquals("precondition", "Customize", customiseMenuItem.Text);

			customiseMenuItem.PerformClick();
			AssertEquals("Should not have shown the error dialog.", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertType("Should have shown the correct form.", typeof(OperationalActionCustomizationForm), ZFormModaliser.LastFormShownDialogForTest);

			OperationalActionCustomizationForm form = (OperationalActionCustomizationForm)ZFormModaliser.LastFormShownDialogForTest;
			AssertType(typeof(OperationalActionManager), form.LastDataSourceForTest);

			OperationalActionManager manager = (OperationalActionManager)form.LastDataSourceForTest;
			AssertEquals("Module Name", manager.ModuleName);
		}

		public void TestCustomise_Deny()
		{
			ActionSupportable.customizationCheckpoint = Env.Security.Maintain;
			ActionSupportable.customizationCheckpoint.IsAllowed = false;

			Menu.MenuItems.AddRange(Maker.Make());
			MenuItem customiseMenuItem = Menu.MenuItems[1];
			AssertEquals("precondition", "Customize", customiseMenuItem.Text);

			customiseMenuItem.PerformClick();
			AssertEquals("Should have shown the error dialog.", "Error " + Env.Security.Maintain.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull("Should have not shown the customisation form.", ZFormModaliser.LastFormShownDialogForTest);
		}

		#region Implementation

		ContextMenu Menu
		{
			get { return menu ?? (menu = new ContextMenu()); }
		}
		ContextMenu menu;

		OperationalActionsCustomisationMenuMaker Maker
		{
			get { return maker ?? (maker = new OperationalActionsCustomisationMenuMaker(Context)); }
		}
		OperationalActionsCustomisationMenuMaker maker;

		OperationalActionContext Context
		{
			get { return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name")); }
		}
		OperationalActionContext context;

		MockOperationalActionSupportable ActionSupportable
		{
			get { return actionSupportable ?? (actionSupportable = new MockOperationalActionSupportable()); }
		}
		MockOperationalActionSupportable actionSupportable;

		OperationalActionSupporter ActionSupporter
		{
			get { return actionSupporter ?? (actionSupporter = ActionSupportable.OperationalActionSupporter); }
		}
		OperationalActionSupporter actionSupporter;

		protected override void SetUp()
		{
			MockSourceControl.Setup();
			base.SetUp();
		}

		protected override void TearDown()
		{
			using (menu)
			{
				base.TearDown();
				MockSourceControl.TearDown();
			}
		}

		#endregion
	}
}
