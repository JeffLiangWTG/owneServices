using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[TestedType(typeof(AvailableHotkeysForm))]
	sealed class AvailableHotkeysFormTest : ZFormBasherTest
	{
		public void TestMenuShortcutsAreDisplayed()
		{
			using (var form = new ZChildForm())
			{
				var menuItem1 = new MenuItem("My &Cat && Dog");
				menuItem1.Shortcut = Shortcut.CtrlShift0;

				form.Menu = new ZMainMenu();
				form.Menu.MenuItems.Add(menuItem1);

				using (var child = new HotkeyControl { typeNameForDisplay = "Grid" }) //Pretend its a grid
				{
					var menuItemChild = new MenuItem("Child");
					menuItemChild.Shortcut = Shortcut.ShiftF10;

					var menuItemParent = new MenuItem("Parent");
					menuItemParent.MenuItems.Add(menuItemChild);

					child.ContextMenu = new ContextMenu();
					child.ContextMenu.MenuItems.Add(menuItemParent);

					form.Controls.Add(child);

					using (var hotkeyForm = new AvailableHotkeysForm(child))
					{
						var formHotkeyDescriptions = GetCodesList(hotkeyForm.TabControl, 1, "This Form");

						var formattedShortcutName = "Ctrl + Shift + 0";
						var catDogShortcut = formHotkeyDescriptions.Cast<ICodeDescription>().FirstOrDefault(pair => pair.Code == formattedShortcutName);

						AssertNotNull("Should format shortcut name", catDogShortcut);
						AssertEquals("Should remove ampersands from caption", "My Cat & Dog", catDogShortcut.Description);

						var contextHotkeyDescriptions = GetCodesList(hotkeyForm.TabControl, 2, "This Grid");
						var childShortcut = contextHotkeyDescriptions.Cast<ICodeDescription>().FirstOrDefault(pair => pair.Description == "Child");

						AssertNotNull("Should have added the child element", childShortcut);
						AssertEquals("Should format the name for F-keys", "Shift + F10", childShortcut.Code);
					}
				}
			}
		}

		public void TestHiddenMenuShortcutsAreNotDisplayed()
		{
			using (var form = new ZChildForm())
			{
				using (var child = new HotkeyControl { typeNameForDisplay = "Grid" })
				{
					child.ContextMenu = new ContextMenu();

					var menuItem1Child1 = new MenuItem("1Child1");
					menuItem1Child1.Shortcut = Shortcut.ShiftF7;
					menuItem1Child1.Visible = false;

					var menuItem1Child2 = new MenuItem("1Child2");
					menuItem1Child2.Shortcut = Shortcut.ShiftF8;
					menuItem1Child2.Visible = true;

					var menuItemParent1 = new MenuItem("Parent1");
					menuItemParent1.MenuItems.Add(menuItem1Child1);
					menuItemParent1.MenuItems.Add(menuItem1Child2);
					menuItemParent1.Visible = true;
					child.ContextMenu.MenuItems.Add(menuItemParent1);

					var menuItem2Child1 = new MenuItem("2Child1");
					menuItem2Child1.Shortcut = Shortcut.ShiftF9;
					menuItem2Child1.Visible = false;

					var menuItem2Child2 = new MenuItem("2Child2");
					menuItem2Child2.Shortcut = Shortcut.ShiftF10;
					menuItem2Child2.Visible = true;

					var menuItemParent2 = new MenuItem("Parent2");
					menuItemParent2.MenuItems.Add(menuItem2Child1);
					menuItemParent2.MenuItems.Add(menuItem2Child2);
					menuItemParent2.Visible = false;
					child.ContextMenu.MenuItems.Add(menuItemParent2);

					var menuItem3Child = new MenuItem("3Child");
					menuItem3Child.Shortcut = Shortcut.ShiftF11;
					menuItem3Child.Visible = false;
					child.ContextMenu.MenuItems.Add(menuItem3Child);

					var menuItem4Child = new MenuItem("4Child");
					menuItem4Child.Shortcut = Shortcut.ShiftF12;
					menuItem4Child.Visible = true;
					child.ContextMenu.MenuItems.Add(menuItem4Child);

					form.Controls.Add(child);

					using (var hotkeyForm = new AvailableHotkeysForm(child))
					{
						var codeDescriptionPair = GetCodesList(hotkeyForm.TabControl, 2, "This Grid");
						AssertEquals(2, codeDescriptionPair.Count);
						AssertEquals("Shift + F8", codeDescriptionPair[0].Code);
						AssertEquals("Shift + F12", codeDescriptionPair[1].Code);
					}
				}
			}
		}

		public void TestSkipsBadControls()
		{
			using (var outerMost = new ZChildForm())
			using (var noHotkeys = new HotkeyControl { typeNameForDisplay = "No Hotkeys" })
			using (var noProvider = new UserControl())
			using (var goodProvider = new HotkeyControl { typeNameForDisplay = "Textbox" })
			{
				((IHotkeyProvider)outerMost).Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Form hotkey");
				goodProvider.Hotkeys.RegisterHotKey(Keys.B, DummyHandler, "Something");

				noProvider.Controls.Add(goodProvider);
				noHotkeys.Controls.Add(noProvider);
				outerMost.Controls.Add(noHotkeys);

				using (var hotkeyForm = new AvailableHotkeysForm(goodProvider))
				{
					var tabPages = hotkeyForm.TabControl.TabPages;

					AssertEquals("Should always start with global pages", "Global", tabPages[0].Text);
					AssertEquals("Should be the parent form next", "This Form", tabPages[1].Text);
					AssertEquals("Should skip to the next handler", "This Textbox", tabPages[2].Text);

					AssertEquals("Should only be those 4 pages", 3, tabPages.Count);
				}
			}
		}

		public void TestControlParentsBecomeTabPages()
		{
			using (var outerMost = new ZChildForm())
			using (var child1 = new HotkeyControl { typeNameForDisplay = "Container" })
			using (var child2 = new HotkeyControl { typeNameForDisplay = "Textbox" }) // We can pretend
			{
				((IHotkeyProvider)outerMost).Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");
				child1.Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");
				child2.Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");

				child1.Controls.Add(child2);
				outerMost.Controls.Add(child1);

				using (var hotkeyForm = new AvailableHotkeysForm(child2))
				{
					var tabPages = hotkeyForm.TabControl.TabPages;

					AssertEquals("Should always start with global pages", "Global", tabPages[0].Text);
					AssertEquals("Should be the parent form next", "This Form", tabPages[1].Text);
					AssertEquals("Should then be the container", "This Container", tabPages[2].Text);
					AssertEquals("Should be the most focused control", "This Textbox", tabPages[3].Text);

					AssertEquals("Should only be those 4 pages", 4, tabPages.Count);
				}
			}
		}

		public void TestControlSiblingsAreNotShown()
		{
			using (var parent = new ZChildForm())
			using (var child = new HotkeyControl { typeNameForDisplay = "Child" })
			using (var sibling = new HotkeyControl { typeNameForDisplay = "Sibling" })
			{
				((IHotkeyProvider)parent).Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");
				child.Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");
				sibling.Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");

				parent.Controls.Add(child);
				parent.Controls.Add(sibling);

				using (var hotkeyForm = new AvailableHotkeysForm(child))
				{
					var tabPages = hotkeyForm.TabControl.TabPages;

					AssertEquals("Should always start with global pages", "Global", tabPages[0].Text);
					AssertEquals("Should be the parent form next", "This Form", tabPages[1].Text);
					AssertEquals("Should then be the container", "This Child", tabPages[2].Text);

					AssertEquals("Should only be those 3 pages - no sibling", 3, tabPages.Count);
				}
			}
		}

		public void TestHotkeyCodesAreSetUp()
		{
			using (var parent = new ZChildForm())
			using (var child = new HotkeyControl { typeNameForDisplay = "Child" })
			{
				child.Hotkeys.RegisterHotKey(Keys.Control | Keys.A, DummyHandler, "Select all");
				child.Hotkeys.RegisterHotKey(Keys.Control | Keys.B, DummyHandler, "Select nothing");

				parent.Controls.Add(child);

				using (var hotkeyForm = new AvailableHotkeysForm(child))
				{
					var codeDescriptionPair = GetCodesList(hotkeyForm.TabControl, 2, "This Child");

					AssertEquals("Should display both hotkeys", 2, codeDescriptionPair.Count);
					AssertEquals("Should display correctly and in order", "Ctrl + A", codeDescriptionPair[0].Code);
					AssertEquals("Should display correctly and in order", "Select all", codeDescriptionPair[0].Description);
					AssertEquals("Should display correctly and in order", "Ctrl + B", codeDescriptionPair[1].Code);
					AssertEquals("Should display correctly and in order", "Select nothing", codeDescriptionPair[1].Description);
				}
			}
		}

		CodeDescriptionPairList GetCodesList(ZTabControl tabControl, int tabIndex, string expectedTabName)
		{
			var tabPage = tabControl.TabPages[tabIndex];
			AssertEquals("PRE: We have the right tab", expectedTabName, tabPage.Text);

			var grid = tabPage.Controls.OfType<CodeDescriptionListEditControl>().Single();
			return new CodeDescriptionPairList(grid.FieldValue);
		}

		protected override Form GetFormToBashCore()
		{
			var outerMost = new ZChildForm();
			((IHotkeyProvider)outerMost).Hotkeys.RegisterHotKey(Keys.Control | Keys.S, DummyHandler, "Save");

			var child1 = new HotkeyControl { typeNameForDisplay = "Container" };
			child1.Hotkeys.RegisterHotKey(Keys.B, DummyHandler, "Description");

			var child2 = new HotkeyControl { typeNameForDisplay = "Textbox" }; // We can pretend
			child2.Hotkeys.RegisterHotKey(Keys.A, DummyHandler, "Description");

			child1.Controls.Add(child2);
			outerMost.Controls.Add(child1);

			var hotkeyForm = new AvailableHotkeysForm(child2);
			hotkeyForm.Disposed += (o, e) => outerMost.Dispose();
			return hotkeyForm;
		}

		protected override IList<string> AllowedMultilineTabControls => new List<string>(new[] { "tabControl" }); // TabControl.Alignment is Left, which forces Multiline to be true

		static bool DummyHandler(object sender, Keys args) => true;
	}
}
