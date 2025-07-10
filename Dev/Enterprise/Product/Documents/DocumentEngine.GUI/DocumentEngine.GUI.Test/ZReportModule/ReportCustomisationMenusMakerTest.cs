using System.Linq;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Build.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class ReportCustomisationMenusMakerTest : TestCaseWithFactory
	{
		[GuiTest]
		public void TestCustomise()
		{
			ReportCustomisationMenusMaker maker = new ReportCustomisationMenusMaker(null, Env.Security.None, nameof(BusinessContext.AgencyBooking));
			using (MainMenu menu = new MainMenu())
			{
				maker.Make(menu.MenuItems);
				AssertEquals("Customize Menu Text", "Customize Reports", menu.MenuItems[1].Text);
				menu.MenuItems[1].PerformClick();
				using (Form lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("Last Shown Form Type", typeof(ReportCustomisationForm), lastShownForm.GetType());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestCheckoutStateShouldBeUpdatedAfterCustomiseFormClosed()
		{
			using (var form = new ZForm())
			using (MainMenu menu = new MainMenu())
			{
				ReportCustomisationMenusMakerForTest maker = new ReportCustomisationMenusMakerForTest(form, Env.Security.None, nameof(BusinessContext.AgencyBooking));
				maker.Make(menu.MenuItems);

				var customizeReportsMenuItem = menu.MenuItems[1];

				var markAllReportsEditableMenuItem = menu.MenuItems[3];
				var markReportsEditableForClientMenuItem = menu.MenuItems[4];
				var saveReportsConfigChangesMenuItem = menu.MenuItems[5];
				var undoAllChangesMenuItem = menu.MenuItems[6];

				Assert("Pre-Condition: IsFileCheckedOutByMe shoule be false.", !SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));

				Assert("Pre-Condition: Menu item should be enabled.", markAllReportsEditableMenuItem.Enabled);
				Assert("Pre-Condition: Menu item should be enabled.", markReportsEditableForClientMenuItem.Enabled);
				Assert("Pre-Condition: Menu item should not be enabled.", !saveReportsConfigChangesMenuItem.Enabled);
				Assert("Pre-Condition: Menu item should not be enabled.", !undoAllChangesMenuItem.Enabled);

				customizeReportsMenuItem.PerformClick();

				var controller = new DocumentsSetupControllerTest.DocumentsTestSetupController();

				using (Form lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					controller.CheckOut();
					lastShownForm.Close();
				}

				Assert("Menu item should not be enabled.", !markAllReportsEditableMenuItem.Enabled);
				Assert("Menu item should not be enabled.", !markReportsEditableForClientMenuItem.Enabled);
				Assert("Menu item should be enabled.", saveReportsConfigChangesMenuItem.Enabled);
				Assert("Menu item should be enabled.", undoAllChangesMenuItem.Enabled);

				customizeReportsMenuItem.PerformClick();
				using (Form lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					controller.UndoCheckOut();
					lastShownForm.Close();
				}

				Assert("Menu item should be enabled.", markAllReportsEditableMenuItem.Enabled);
				Assert("Menu item should be enabled.", markReportsEditableForClientMenuItem.Enabled);
				Assert("Menu item should not be enabled.", !saveReportsConfigChangesMenuItem.Enabled);
				Assert("Menu item should not be enabled.", !undoAllChangesMenuItem.Enabled);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = true;
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();

			CustomisationMenusMaker<MenuItem>.ShouldAddDebugOnlyMenuItemsForTesting = false;
			MockSourceControl.TearDown();
		}

		public void TestDebugOnlyMenuItems()
		{
			var maker = new ReportCustomisationMenusMaker(null, Env.Security.None, nameof(BusinessContext.AgencyBooking));
			using (var menu = new MainMenu())
			{
				maker.Make(menu.MenuItems);

				var expectedMenuItems = new string[]
				{
					"Customize Reports",
					"Mark All Reports Editable",
					"Mark Reports Editable For Client",
					"Regenerate all clients Documents.xml",
					"Save Reports Config Changes",
					"Undo All Changes"
				};

				var currentMenuItems = menu.MenuItems.OfType<MenuItem>().Select(m => m.Text).ToArray();

				Assert("All of the expected menu items should be in the menu", !expectedMenuItems.Except(currentMenuItems).Any());
			}
		}

		class ReportCustomisationMenusMakerForTest : ReportCustomisationMenusMaker
		{
			public ReportCustomisationMenusMakerForTest(Form parentForm, ISecurityCheckpoint customisationSecurityCheckpoint, string businessContext)
				: base(parentForm, customisationSecurityCheckpoint, businessContext)
			{
			}

			protected override DocumentsSetupController GetNewDocumentsSetupController() => new DocumentsSetupControllerTest.DocumentsTestSetupController();
		}
	}
}
