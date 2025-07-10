using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Build.Testing;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	abstract class CustomisationMenusMakerTest<T> : TestCaseWithDummy
			where T : class
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckOutAndCheckIn()
		{
			using (var form = GetForm())
			{
				var maker = GetMaker(form);
				maker.Make(GetMenuItems(form));

				var controller = new DocumentsSetupControllerTest.DocumentsTestSetupController();
				AssertEquals("IsCheckedOut", false, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));

				PerformClick(maker.MarkAllEditableMenu);
				AssertEquals("IsCheckedOut", true, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", true, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));

				PerformClick(maker.SaveConfigChangesMenu);
				AssertEquals("IsCheckedOut", true, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", true, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckOutAndUndoCheckOut()
		{
			using (var form = GetForm())
			{
				var maker = GetMaker(form);
				maker.Make(GetMenuItems(form));

				var controller = new DocumentsSetupControllerTest.DocumentsTestSetupController();
				AssertEquals("IsCheckedOut", false, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));

				PerformClick(maker.MarkAllEditableMenu);
				AssertEquals("IsCheckedOut", true, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", true, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));

				PerformClick(maker.UndoAllChangesMenu);
				AssertEquals("IsCheckedOut", false, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCheckOutCheckInClientSpecific()
		{
			using (var form = GetForm())
			{
				var maker = GetMaker(form);
				maker.Make(GetMenuItems(form));

				var controller = new DocumentsSetupControllerTest.DocumentsTestSetupController();
				AssertEquals("IsCheckedOut", false, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(ClientSpecificDocumentsSetupControllerTest.TestFileName));

				PerformClick(maker.TestClientMenu);
				AssertEquals("IsCheckedOut", true, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", true, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(ClientSpecificDocumentsSetupControllerTest.TestFileName));

				PerformClick(maker.UndoAllChangesMenu);
				AssertEquals("IsCheckedOut", false, controller.IsCheckedOutByMe);
				AssertEquals("IsFileCheckedOutByMe", false, SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(ClientSpecificDocumentsSetupControllerTest.TestFileName));
			}
		}

		public void TestMake()
		{
			using (var form = GetForm())
			{
				var originalValue = DocumentsDataRegistry.Instance.DocumentsSourceControlIsEnabled.Value;

				DocumentsDataRegistry.Instance.DocumentsSourceControlIsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var maker = GetMaker(form);
				GetMenuItems(form).Clear();
				maker.Make(GetMenuItems(form));
				AssertEquals("MenuItems.Count - should be 8 when document source control is enabled (ie the disable file is not present)", 8, GetMenuItems(form).Count);
				AssertEquals("MenuItems[0].Text", " ", GetText((T)GetMenuItems(form)[0]));
				AssertEquals("MenuItems[1].Text", "Customize", GetText((T)GetMenuItems(form)[1]));
				AssertEquals("MenuItems[2].Text", " ", GetText((T)GetMenuItems(form)[2]));
				AssertEquals("MenuItems[3].Text", "Mark All Documents Editable", GetText((T)GetMenuItems(form)[3]));
				AssertEquals("MenuItems[4].Text", "Mark Documents Editable For Client", GetText((T)GetMenuItems(form)[4]));
				AssertEquals("MenuItems[6].Text", "Save Documents Config Changes", GetText((T)GetMenuItems(form)[5]));
				AssertEquals("MenuItems[7].Text", "Undo All Changes", GetText((T)GetMenuItems(form)[6]));
				AssertEquals("MenuItems[8].Text", "Regenerate all clients Documents.xml", GetText((T)GetMenuItems(form)[7]));

				DocumentsDataRegistry.Instance.DocumentsSourceControlIsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				maker = GetMaker(form);
				GetMenuItems(form).Clear();
				maker.Make(GetMenuItems(form));
				AssertEquals("MenuItems.Count - should be just 2 when document source control is disable (ie the disable file is found)", 2, GetMenuItems(form).Count);

				// Clean-up
				DocumentsDataRegistry.Instance.DocumentsSourceControlIsEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		#region Implementation

		protected abstract string GetText(T menuItem);
		protected abstract IList GetMenuItems(Form form);
		protected abstract void PerformClick(T menuItem);
		protected abstract ZDocumentsMenuItemHelper<T> GetNewHelper();

		MockCustomisationMenuMaker GetMaker(Form form) => new MockCustomisationMenuMaker(form, Env.Security.None, GetNewHelper());

		Form GetForm()
		{
			var result = new Form
			{
				Menu = new MainMenu(),
				MainMenuStrip = new MenuStrip()
			};
			result.Show();
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			DocumentTablesCleaner.Clean();
			MockCustomisationMenuMaker.ShouldAddDebugOnlyMenuItemsForTesting = true;
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			try
			{
				MockCustomisationMenuMaker.ShouldAddDebugOnlyMenuItemsForTesting = false;
				if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath))
				{
					SourceControl.EnterpriseDatabase.UndoCheckOut(DocumentsSetupControllerTest.DocumentsTestDataFile.TestDataFilePath, false);
				}
				if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(ClientSpecificDocumentsSetupControllerTest.TestFileName))
				{
					SourceControl.EnterpriseDatabase.UndoCheckOut(ClientSpecificDocumentsSetupControllerTest.TestFileName, false);
				}
				MockSourceControl.TearDown();
			}
			finally
			{
				base.TearDown();
			}
		}

		class MockCustomisationMenuMaker : CustomisationMenusMaker<T>
		{
			public MockCustomisationMenuMaker(Form parentForm, SecurityCheckpoint customisationSecurityCheckpoint, ZDocumentsMenuItemHelper<T> helper)
				: base(parentForm, customisationSecurityCheckpoint, helper)
			{
			}

			public T TestClientMenu => FindClientMenu(Clients.EDI);

			T FindClientMenu(Clients client)
			{
				var code = client.ToString();

				foreach (T menu in MenuItemHelper.GetItems(MarkEditableForClientMenu))
				{
					if (MenuItemHelper.GetName(menu) == code)
					{
						return menu;
					}
					else if (MenuItemHelper.GetName(menu) == code[0].ToString())
					{
						foreach (T subMenu in MenuItemHelper.GetItems(menu))
						{
							if (MenuItemHelper.GetName(subMenu) == code)
							{
								return subMenu;
							}
						}
					}
				}
				return null;
			}

			protected override IMenuCustomisationForm GetCustomisationForm() => throw new NotImplementedException();

			protected override ClientSpecificDocumentsSetupController GetNewClientSpecificDocumentsSetupControllerWithDocCompleteTask(string clientName) => new ClientSpecificDocumentsTestSetupController(clientName);

			protected override DocumentsSetupController GetNewDocumentsSetupController() => new DocumentsSetupControllerTest.DocumentsTestSetupController();

			protected override string CustomisationMenusDescription => "Documents";
		}

		#endregion
	}
}
