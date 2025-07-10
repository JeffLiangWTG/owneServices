using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[GuiTest]
	sealed class ZTextBoxBaseContextMenuManagerTest : TestCaseWithDummy
	{
		public void TestTemplatesMenuLayoutWithManyTemplates()
		{
			Dummy.Z0_Description = @"\\abc";

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax", true))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				for (int i = 0; i < 10; i++)
				{
					CreateTextTemplate(form, $"Test{i}", "random text");
				}

				var layoutCount = 0;
				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Layout += (object sender, LayoutEventArgs e) => { layoutCount++; };
				templateMenu.DropDown.Show();
				Application.DoEvents();
				AssertEquals("layout event should be triggered 3 times.", 3, layoutCount);
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestGetTemplateForm()
		{
			using (var form = new ZForm(Dummy))
			{
				var textBox = new ZTextBox { SupportsMacroTemplates = true };
				var macroFindBox = new ZMacrosFindBox();
				macroFindBox.CodeBox.SupportsMacroTemplates = true;

				form.Controls.Add(textBox);
				form.Controls.Add(macroFindBox);
				form.Show();

				var template = textBox.contextMenuManager.TextTemplatesFactory.New();
				using (var templateForm = textBox.contextMenuManager.GetTemplateForm(template))
				{
					AssertEquals(templateForm.GetType(), typeof(TextTemplateForm));
				}

				template = macroFindBox.CodeBox.contextMenuManager.TextTemplatesFactory.New();
				using (var templateForm = macroFindBox.CodeBox.contextMenuManager.GetTemplateForm(template))
				{
					AssertEquals(templateForm.GetType(), typeof(ExpressionTemplateForm));
				}
			}
		}

		public void TestInitializeContextMenuTwice()
		{
			WeakReference weakRef = null;
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				textBox.contextMenuManager.InitializeContextMenu();
				weakRef = new WeakReference(textBox.ContextMenu);
				textBox.contextMenuManager.InitializeContextMenu();
			}
			Assert(!weakRef.IsAlive);
		}

		[DeveloperOnlyTest]
		public void TestEditActionsOnZTextBox()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				textBox.CharacterCasing = CharacterCasing.Normal;
				form.Controls.Add(textBox);
				form.Show();
				textBox.contextMenuManager.InitializeContextMenu();
				DoTestEditActions(textBox);
			}
		}

		[DeveloperOnlyTest]
		public void TestEditActionsOnZRichTextBox()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZRichTextBox();
				form.Controls.Add(textBox);
				form.Show();
				textBox.contextMenuManager.InitializeContextMenu();
				DoTestEditActions(textBox.RichEdit);
			}
		}

		void DoTestEditActions(TextBoxBase textBox)
		{
			var find = 5;
			var undo = 6;
			var cut = 8;
			var copy = 9;
			var paste = 10;
			var pasteTextOnly = 11;
			var delete = 11;
			var selectAll = 13;
			if (textBox is ZRichTextBox.MyRichTextBox)
			{
				delete = 12;
				selectAll = 14;
			}

			CombineAssertions(() =>
			{
				textBox.ContextMenuStrip.Show();
				textBox.Text = "flower";
				AssertEquals("Find", textBox.ContextMenuStrip.Items[find].Text);
				AssertEquals("Undo", textBox.ContextMenuStrip.Items[undo].Text);
				AssertEquals("Cut", textBox.ContextMenuStrip.Items[cut].Text);
				AssertEquals("Copy", textBox.ContextMenuStrip.Items[copy].Text);
				AssertEquals("Paste", textBox.ContextMenuStrip.Items[paste].Text);
				if (textBox is ZRichTextBox.MyRichTextBox)
				{
					AssertEquals("Paste Text Only", textBox.ContextMenuStrip.Items[pasteTextOnly].Text);

					textBox.ContextMenuStrip.Show();
					AssertEquals("Find - .Enabled", false, textBox.ContextMenuStrip.Items[find].Enabled);
					AssertEquals("Find - .Visible", false, textBox.ContextMenuStrip.Items[find].Visible);
				}
				AssertEquals("Delete", textBox.ContextMenuStrip.Items[delete].Text);
				AssertEquals("Select All", textBox.ContextMenuStrip.Items[selectAll].Text);

				if (textBox is ZTextBox zTextBox)
				{
					zTextBox.EnableFindDialog = false;
					textBox.ContextMenuStrip.Show();
					AssertEquals("EnableFindDialog disabled - .Enabled", false, textBox.ContextMenuStrip.Items[find].Enabled);
					AssertEquals("EnableFindDialog disabled - .Visible", false, textBox.ContextMenuStrip.Items[find].Visible);

					zTextBox.EnableFindDialog = true;
					textBox.ContextMenuStrip.Show();
					AssertEquals("EnableFindDialog enabled - .Enabled", true, textBox.ContextMenuStrip.Items[find].Enabled);
					AssertEquals("EnableFindDialog enabled - .Visible", true, textBox.ContextMenuStrip.Items[find].Visible);
				}
				AssertEquals("Undo", false, textBox.ContextMenuStrip.Items[undo].Enabled);
				AssertEquals("Cut", false, textBox.ContextMenuStrip.Items[cut].Enabled);
				AssertEquals("Copy", false, textBox.ContextMenuStrip.Items[copy].Enabled);
				AssertEquals("Delete", false, textBox.ContextMenuStrip.Items[delete].Enabled);
				AssertEquals("Select All", true, textBox.ContextMenuStrip.Items[selectAll].Enabled);

				textBox.ContextMenuStrip.Items[selectAll].PerformClick();
				AssertEquals("flower", textBox.Text);
				textBox.ContextMenuStrip.Show();
				AssertEquals("Undo", false, textBox.ContextMenuStrip.Items[undo].Enabled);
				AssertEquals("Cut", true, textBox.ContextMenuStrip.Items[cut].Enabled);
				AssertEquals("Copy", true, textBox.ContextMenuStrip.Items[copy].Enabled);
				AssertEquals("Delete", true, textBox.ContextMenuStrip.Items[delete].Enabled);
				textBox.ContextMenuStrip.Items[delete].PerformClick();
				AssertEquals("", textBox.Text);
				textBox.Text = "flower";
				textBox.ContextMenuStrip.Show();
				textBox.ContextMenuStrip.Items[selectAll].PerformClick();
				textBox.ContextMenuStrip.Show();

				var cutItem = textBox.ContextMenuStrip.Items[cut];
				var action = new Action(() =>
				{
					cutItem.PerformClick();
					Application.DoEvents();
				});
				action.Invoke();
				var clipboardText = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				Thread.Sleep(200);
				AssertEquals("", textBox.Text);
				AssertEquals("flower", clipboardText);
				textBox.ContextMenuStrip.Show();
				AssertEquals("Paste", true, textBox.ContextMenuStrip.Items[paste].Enabled);
				if (textBox is ZRichTextBox.MyRichTextBox)
				{
					AssertEquals("Paste Text Only", true, textBox.ContextMenuStrip.Items[pasteTextOnly].Enabled);
				}
				textBox.ContextMenuStrip.Items[paste].PerformClick();
				AssertEquals("flower", textBox.Text);
				textBox.ContextMenuStrip.Show();
				textBox.ContextMenuStrip.Items[paste].PerformClick();
				AssertEquals("flowerflower", textBox.Text);

				textBox.ContextMenuStrip.Show();
				textBox.ContextMenuStrip.Items[selectAll].PerformClick();
				textBox.ContextMenuStrip.Show();

				var copyItem = textBox.ContextMenuStrip.Items[copy];
				action = () =>
				{
					copyItem.PerformClick();
					Application.DoEvents();
				};
				action.Invoke();
				clipboardText = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action, retry: 30);
				Thread.Sleep(200);
				AssertEquals("flowerflower", textBox.Text);
				AssertEquals("flowerflower", clipboardText);
				textBox.ContextMenuStrip.Show();
				textBox.ContextMenuStrip.Items[delete].PerformClick();
				AssertEquals("", textBox.Text);
				textBox.ContextMenuStrip.Show();
				textBox.ContextMenuStrip.Items[paste].PerformClick();
				AssertEquals("flowerflower", textBox.Text);

				textBox.Text = "smile";
				textBox.ReadOnly = true;
				textBox.ContextMenuStrip.Show();
				AssertEquals("Undo", false, textBox.ContextMenuStrip.Items[undo].Enabled);
				AssertEquals("Cut", false, textBox.ContextMenuStrip.Items[cut].Enabled);
				AssertEquals("Copy", false, textBox.ContextMenuStrip.Items[copy].Enabled);
				AssertEquals("Paste", false, textBox.ContextMenuStrip.Items[paste].Enabled);
				if (textBox is ZRichTextBox.MyRichTextBox)
				{
					AssertEquals("Paste Text Only", false, textBox.ContextMenuStrip.Items[pasteTextOnly].Enabled);
				}
				AssertEquals("Delete", false, textBox.ContextMenuStrip.Items[delete].Enabled);
				AssertEquals("Select All", true, textBox.ContextMenuStrip.Items[selectAll].Enabled);
				textBox.ContextMenuStrip.Items[selectAll].PerformClick();
				textBox.ContextMenuStrip.Show();
				Application.DoEvents();
				AssertEquals("Copy", true, textBox.ContextMenuStrip.Items[copy].Enabled);
				AssertEquals("Delete", false, textBox.ContextMenuStrip.Items[delete].Enabled);

				copyItem = textBox.ContextMenuStrip.Items[copy];
				action = () =>
				{
					copyItem.PerformClick();
					Application.DoEvents();
				};
				action.Invoke();
				Thread.Sleep(200);
				clipboardText = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals("smile", clipboardText);
			});
		}

		public void TestTemplatesMenuDoesNotShowOnCodeField()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Code"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Multiline = false;
				form.TextBox.ContextMenuStrip.Show();
				AssertEquals("Insert Template", form.TextBox.ContextMenuStrip.Items[0].Text);
				AssertEquals(false, form.TextBox.ContextMenuStrip.Items[0].Visible);
				AssertEquals(false, form.TextBox.ContextMenuStrip.Items[1].Visible);
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuDoesNotShowOnDecimalField()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Decimal"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Multiline = false;
				form.TextBox.ContextMenuStrip.Show();
				AssertEquals("Insert Template", form.TextBox.ContextMenuStrip.Items[0].Text);
				AssertEquals(false, form.TextBox.ContextMenuStrip.Items[0].Visible);
				AssertEquals(false, form.TextBox.ContextMenuStrip.Items[1].Visible);
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		[ExpectNoExceptions()]
		public void TestShortCutDoesNothingOnDecimalField()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Decimal"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Multiline = false;
				form.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Opened += new EventHandler(delegate
				{
					Fail("templateMenu.DropDown.Opened");
				});
				form.TextBox.contextMenuManager.textBox_KeyDown(null, new KeyEventArgs(Keys.F4));
			}
		}

		[ExpectNoExceptions()]
		public void TestKeyDownWhenDisposed()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				textBox.Dispose();
				textBox.contextMenuManager.textBox_KeyDown(null, new KeyEventArgs(Keys.F4));
			}
		}

		public void TestTemplatesMenuShowsOnDescription()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Description"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Multiline = false;
				form.TextBox.ContextMenuStrip.Show();
				AssertEquals("Insert Template", form.TextBox.ContextMenuStrip.Items[0].Text);
				AssertEquals(true, form.TextBox.ContextMenuStrip.Items[0].Visible);
				AssertEquals(true, form.TextBox.ContextMenuStrip.Items[1].Visible);
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuDoesNotShowOnControlBoundToDataTable()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("Dummy");
			dataTable.Rows.Add("Hello World");

			using (var form = new ZForm(dataTable))
			{
				var textBox = new ZTextBox();
				textBox.SetDataBinding(dataTable, "Dummy");
				textBox.Multiline = true;
				form.Controls.Add(textBox);
				form.Show();
				textBox.contextMenuManager.InitializeContextMenu();
				textBox.ContextMenuStrip.Show();
				AssertEquals("Insert Template", textBox.ContextMenuStrip.Items[0].Text);
				AssertEquals(false, textBox.ContextMenuStrip.Items[0].Visible);
				AssertEquals(false, textBox.ContextMenuStrip.Items[1].Visible);
				textBox.ContextMenuStrip.Hide();
			}
		}

		public void TestEmptyTemplatesMenu()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				AssertEquals("Insert Template", templateMenu.Text);
				AssertEquals(true, templateMenu.Visible);
				AssertEquals(true, form.TextBox.ContextMenuStrip.Items[1].Visible);
				templateMenu.DropDown.Show();
				AssertEquals("Create Template", templateMenu.DropDown.Items[0].Text);
				AssertEquals("Manage Template", templateMenu.DropDown.Items[1].Text);
				AssertEquals(true, templateMenu.DropDown.Items[0].Enabled);
				AssertEquals(false, templateMenu.DropDown.Items[1].Enabled);
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuTemplate()
		{
			Dummy.Z0_Number = 3;

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, "Alpha", "The Lucky Number is <Z0_Number>");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();
				AssertEquals("Alpha", templateMenu.DropDown.Items[0].Text);
				AssertEquals("The Lucky Number is 3", templateMenu.DropDown.Items[0].ToolTipText);
				AssertEquals("Create Template", templateMenu.DropDown.Items[2].Text);
				AssertEquals("Manage Template", templateMenu.DropDown.Items[3].Text);
				AssertEquals(true, templateMenu.DropDown.Items[3].Enabled);
				((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Show();
				AssertEquals("Alpha", ((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals(form.TextBox.Text, "The Lucky Number is 3");
			}
		}

		public void TestTemplatesMenuWithSubmenuCreatedFromSpecialCharacter()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Alpha", templateMenu.DropDown.Items[0].Text);
				AssertEquals("Beta", ((ToolStripMenuItem)templateMenu.DropDown.Items[0]).DropDown.Items[0].Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuWithMultiLevelSubmenuCreatedFromSpecialCharacter()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\Gamma", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				var alpha = (ToolStripMenuItem)templateMenu.DropDown.Items[0];
				var beta = (ToolStripMenuItem)alpha.DropDown.Items[0];
				var gamma = beta.DropDown.Items[0];

				AssertEquals("Alpha", alpha.Text);
				AssertEquals("Beta", beta.Text);
				AssertEquals("Gamma",gamma.Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuWithMultiLevelSubmenuWithManyChildren()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\Gamma", "Greek Alphabet");
				CreateTextTemplate(form, @"Alpha\Beta\Delta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				var alpha = (ToolStripMenuItem)templateMenu.DropDown.Items[0];
				var beta = (ToolStripMenuItem)alpha.DropDown.Items[0];
				var delta = beta.DropDown.Items[0];
				var gamma = beta.DropDown.Items[1];

				AssertEquals("Alpha", alpha.Text);
				AssertEquals("Beta", beta.Text);
				AssertEquals("Delta", delta.Text);
				AssertEquals("Gamma", gamma.Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuWithMultiLevelSubmenuWithManyChildrenAndIntermediateDoubleBackslash()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\\Epsilon\Gamma", "Greek Alphabet");
				CreateTextTemplate(form, @"Alpha\Beta\\Epsilon\Delta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				var alpha = (ToolStripMenuItem)templateMenu.DropDown.Items[0];
				var betaEpsilon = (ToolStripMenuItem)alpha.DropDown.Items[0];
				var delta = betaEpsilon.DropDown.Items[0];
				var gamma = betaEpsilon.DropDown.Items[1];

				AssertEquals("Alpha", alpha.Text);
				AssertEquals(@"Beta\Epsilon", betaEpsilon.Text);
				AssertEquals("Delta", delta.Text);
				AssertEquals("Gamma", gamma.Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuWithSubmenuIgnoresOnlyWhitespace()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\\Epsilon\ \Gamma", "Greek Alphabet");
				CreateTextTemplate(form, @"Alpha\Beta\\Epsilon\ \Delta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				var alpha = (ToolStripMenuItem)templateMenu.DropDown.Items[0];
				var betaEpsilon = (ToolStripMenuItem)alpha.DropDown.Items[0];
				var delta = betaEpsilon.DropDown.Items[0];
				var gamma = betaEpsilon.DropDown.Items[1];

				AssertEquals("Alpha", alpha.Text);
				AssertEquals(@"Beta\Epsilon", betaEpsilon.Text);
				AssertEquals("Delta", delta.Text);
				AssertEquals("Gamma", gamma.Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuWithSubmenuCreatedFromSpecialCharacterWithIntermediateDoubleBackslash()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\\Gamma\Delta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Alpha", templateMenu.DropDown.Items[0].Text);
				AssertEquals(@"Beta\Gamma", ((ToolStripMenuItem)templateMenu.DropDown.Items[0]).DropDown.Items[0].Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuItemWithDoubleBackslashDoesNotCreateSubmenu()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\\Beta", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals(@"Alpha\Beta", templateMenu.DropDown.Items[0].Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuShouldEscapeAllSpecialCharacters()
		{
			Dummy.Z0_Description = @"\\abc";

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax", true))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, "Test", @"""<Z0_Description>"" == ""\\\\abc""");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();
				var expectedText = @"""\\abc"" == ""\\abc""";

				AssertEquals("Test", templateMenu.DropDown.Items[0].Text);
				AssertEquals(expectedText, templateMenu.DropDown.Items[0].ToolTipText);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals(expectedText, form.TextBox.Text);
			}
		}

		public void TestManageTemplatesMenuWithSubmenuCreatedFromSpecialCharacter()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, @"Alpha\Beta\\Gamma", "Greek Alphabet");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Manage Template", templateMenu.DropDown.Items[3].Text);

				var manageTemplateMenu = (ToolStripMenuItem)templateMenu.DropDown.Items[3];
				manageTemplateMenu.DropDown.Show();
				AssertEquals("Alpha", manageTemplateMenu.DropDown.Items[0].Text);
				AssertEquals(@"Beta\Gamma", ((ToolStripMenuItem)manageTemplateMenu.DropDown.Items[0]).DropDown.Items[0].Text);

				templateMenu.DropDown.Hide();
				form.TextBox.ContextMenuStrip.Hide();
			}
		}

		public void TestTemplatesMenuTemplateContainsAdditionalBizo()
		{
			var dummyParent = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var parent = new ZForm(dummyParent) { Text = "Sango" })
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
			{
				form.Visible = false;
				form.Show(parent);

				form.TextBox.contextMenuManager.InitializeContextMenu();
				AssertNotNull(form.TextBox.contextMenuManager.TextTemplatesFactory.New());

				var contextBusinessObjects = form.TextBox.contextMenuManager.TextTemplatesFactory.ContextBusinessObjects;
				AssertEquals("Context menu should only contain two bizo", 2, contextBusinessObjects.Length);
				Assert("Context menu should contain dummyParentBizo", contextBusinessObjects.Contains(dummyParent));
				Assert("Context menu should contain Dummy", contextBusinessObjects.Contains(Dummy));
			}
		}

		public void TestICustomTextTemplateContextShouldContainOwnBizo()
		{
			var customDummyObj = Factory.NewWithValidTestData<CustomDummyBusinessObject>();

			using (var form = new BoundTextBoxFormForTest(customDummyObj, "Z0_VarCharMax"))
			{
				form.Visible = false;

				form.TextBox.contextMenuManager.InitializeContextMenu();
				AssertNotNull(form.TextBox.contextMenuManager.TextTemplatesFactory.New());

				var contextBusinessObjects = form.TextBox.contextMenuManager.TextTemplatesFactory.ContextBusinessObjects;
				Assert("contextBusinessObjects should contain OwnBizo", contextBusinessObjects.Contains(customDummyObj));
			}
		}

		public void TestTemplatesMenuSecurityAccess()
		{
			IGlbStaff staffA;
			IGlbStaff staffB;
			IGlbStaff staffC;
			SetupSecurity(Factory, out staffA, out staffB, out staffC);
			var company2 = Factory.New<IGlbCompany>();
			var branch2 = Factory.New<IGlbBranch>();
			branch2.GB_GC = company2.PK;
			Factory.Save();

			var differentDummy = Factory.New<DummyDependantBusinessObject>();

			using (EnvProxy.Instance.SetTemporaryUserContext(staffB.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					CreateTextTemplate(form, "Alpha", "", true, false);

					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Alpha", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[3], "Alpha");
				}

				using (var form = new BoundTextBoxFormForTest(differentDummy, "ZD1_Code"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					CreateTextTemplate(form, "Beta", "", true, false);

					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Beta", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[3], "Beta");
				}
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					CreateTextTemplate(form, "Gamma", "", false, false);

					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Alpha", "Gamma", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[4], "Gamma");
				}

				using (var form = new BoundTextBoxFormForTest(differentDummy, "ZD1_Code"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					CreateTextTemplate(form, "Delta", "", false, false);

					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Beta", "Delta", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[4], "Delta");
				}
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffC.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					CreateTextTemplate(form, "Epsilon", "", true, true);

					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Alpha", "Epsilon", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[4], "Alpha", "Epsilon");
				}
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffA.GS_LoginName, branch2.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
			{
				using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_VarCharMax"))
				{
					form.TextBox.contextMenuManager.InitializeContextMenu();
					AssertMenuDropDown((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0], "Epsilon", "Gamma", "", "Create Template", "Manage Template");
					AssertMenuDropDown((ToolStripMenuItem)((ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0]).DropDown.Items[4], "Gamma");
				}
			}
		}

		static void SetupSecurity(BusinessObjectFactory factory, out IGlbStaff staffA, out IGlbStaff staffB, out IGlbStaff staffD)
		{
			staffA = factory.New<IGlbStaff>();
			staffA.GS_Code = "AAA";
			staffA.GS_LoginName = "alpha";
			staffA.GS_FullName = "Alpha Albert Anaheim";

			staffB = factory.New<IGlbStaff>();
			staffB.GS_Code = "BBB";
			staffB.GS_LoginName = "beta";
			staffB.GS_FullName = "Beta Brett Boston";

			IGlbSecurity security = factory.New<IGlbSecurity>();
			security.GU_GS = staffB.PK;
			security.GU_GC = EnvProxy.Instance.CurrentCompany.PK;
			security.GU_SecurityRight = StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.Code;
			security.GU_SecurityItemIsAllowed = ZBool.True;

			staffD = factory.New<IGlbStaff>();
			staffD.GS_Code = "DDD";
			staffD.GS_LoginName = "delta";
			staffD.GS_FullName = "Delta Dan Detriot";

			security = factory.New<IGlbSecurity>();
			security.GU_GS = staffD.PK;
			security.GU_SecurityRight = StmNoteTemplateSecurityProvider.SecurityCheckpointForPublish.Code;
			security.GU_SecurityItemIsAllowed = ZBool.True;

			factory.Save();
		}

		public void TestTemplateOnNoteContext()
		{
			var dummyBizOWithAutoLogs = Factory.New<DummyBizOWithAutoLogs>();
			var note = dummyBizOWithAutoLogs.Notes.AddNew();
			StmNoteTemplate templateOnNote1;
			StmNoteTemplate templateOnNote2;
			StmNoteTemplate templateOnBzo;
			using (var form = new BoundTextBoxFormForTest(dummyBizOWithAutoLogs, "Z0_VarCharMax"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				templateOnBzo = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
			}
			using (var form = new BoundTextBoxFormForTest(note, "ST_NoteData"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				templateOnNote1 = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
			}
			using (var form = new BoundTextBoxFormForTest(dummyBizOWithAutoLogs, "Notes+VisibleNotes.ST_NoteData"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				templateOnNote2 = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
			}
			AssertEquals(templateOnBzo.S8_ContextID.Split('.')[0], templateOnNote1.S8_ContextID.Split('.')[0]);
			AssertEquals(templateOnBzo.S8_ContextID.Split('.')[0], templateOnNote2.S8_ContextID.Split('.')[0]);
			AssertEquals(templateOnNote1.S8_ContextID, templateOnNote2.S8_ContextID);
		}

		public void TestTemplateOnCustomContext()
		{
			var legacyNote1 = Factory.NewWithValidTestData<StmNoteTemplate>();
			var legacyNote2 = Factory.NewWithValidTestData<StmNoteTemplate>();

			legacyNote1.S8_ContextID = "." + CustomBusinessObject.GetLegacyIdentifier1("My Field", typeof(ZString));
			legacyNote2.S8_ContextID = "." + CustomBusinessObject.GetLegacyIdentifier2("My Field", typeof(ZString));

			Factory.Save();

			var customFieldName = "My Field";
			var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>();
			var dummy = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
			addCustomField((customFieldName, "STR"));
			setCustomField((customFieldName, "STR"), new ZString("A Value"));

			StmNoteTemplate templateOnBzo;
			using (var form = new BoundTextBoxFormForTest(((ICustomFieldProvider)dummy).GetCustomBusinessObject(), getCustomFieldIdentifier((customFieldName, "STR"))))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				templateOnBzo = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
				templateOnBzo.S8_Description = "Some Description";
				templateOnBzo.S8_TemplateText = "Some Text";
				templateOnBzo.Factory.Save();

				var stmNoteTemplates = form.TextBox.contextMenuManager.TextTemplatesFactory.GetAllTemplatesForControl();
				AssertEquals(3, stmNoteTemplates.Length);
				foreach (var stmNoteTemplate in stmNoteTemplates)
				{
					AssertEquals($"DummyBizo.{getCustomFieldIdentifier((customFieldName, "STR"))}", stmNoteTemplate.S8_ContextID);
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestShortCutBeforeMouseDownDoesNotThrowException()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				textBox.CharacterCasing = CharacterCasing.Normal;
				form.Controls.Add(textBox);
				form.Show();
				textBox.contextMenuManager.textBox_KeyDown(null, new KeyEventArgs(Keys.F4));
				textBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)textBox.ContextMenuStrip.Items[0];
				AssertEquals("Insert Template", templateMenu.Text);
				textBox.ContextMenuStrip.Hide();
			}
		}

		void AssertMenuDropDown(ToolStripMenuItem menu, params string[] expected)
		{
			menu.DropDown.Show();
			AssertEquals(expected.Length, menu.DropDown.Items.Count);
			for (var i = 0; i < expected.Length; i++)
			{
				AssertEquals(expected[i], menu.DropDown.Items[i].Text);
			}
			menu.DropDown.Hide();
		}

		public void TestInsertTextTemplate_UserChoosesToTruncate_ShouldRespectTextBoxMaxLength()
		{
			var notificationProvider = new Mock<IUserNotification>();
			notificationProvider.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), ZMessageBoxButtons.YesNo, ZDialogResult.No))
				.Returns(ZDialogResult.Yes);

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Description"))
			{
				form.TextBox.contextMenuManager.UserNotificationProvider = notificationProvider.Object;
				form.TextBox.contextMenuManager.InitializeContextMenu();

				var template = CreateTextTemplate(form,
					"Wibble",
					"Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly");

				Assert("Template text should be longer than schema max length", template.S8_TemplateText.Length > DummyBizoSchema.Z0_Description.MaxLength);

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Wibble", templateMenu.DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals("Should have truncated text to fit field max length",
					"Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wi", form.TextBox.Text);
			}
		}

		public void TestInsertTextTemplate_WhenExistingTextExists_UserChoosesToTruncate_ShouldRespectTextBoxMaxLength()
		{
			var notificationProvider = new Mock<IUserNotification>();
			notificationProvider.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), ZMessageBoxButtons.YesNo, ZDialogResult.No))
				.Returns(ZDialogResult.Yes);

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Description"))
			{
				form.TextBox.contextMenuManager.UserNotificationProvider = notificationProvider.Object;
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Text = "Timey wimey stuff";
				form.TextBox.Select(6, 11);

				var template = CreateTextTemplate(
					form, "Wibble",
					"Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly");

				Assert("Template text should be longer than schema max length", template.S8_TemplateText.Length > DummyBizoSchema.Z0_Description.MaxLength);

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Wibble", templateMenu.DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals("Should have truncated text to fit field max length and preserved un-selected text",
					"Timey Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wob", form.TextBox.Text);
			}
		}

		public void TestInsertTextTemplate_WhenExistingTextExists_UserChoosesNOTToTruncate_ShouldLeaveText()
		{
			var notificationProvider = new Mock<IUserNotification>();
			notificationProvider.Setup(m => m.Show(It.IsAny<string>(), It.IsAny<string>(), ZMessageBoxButtons.YesNo, ZDialogResult.No))
				.Returns(ZDialogResult.No);

			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Description"))
			{
				form.TextBox.contextMenuManager.UserNotificationProvider = notificationProvider.Object;
				form.TextBox.contextMenuManager.InitializeContextMenu();
				form.TextBox.Text = "Timey wimey stuff";

				CreateTextTemplate(form, "Wibble",
					"Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly Wibbly wobbly");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Wibble", templateMenu.DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals("Should have left text as it was", "Timey wimey stuff", form.TextBox.Text);
			}
		}

		public void TestInsertTextTemplate_ObjectTagIsNotAssigned()
		{
			using (var form = new BoundTextBoxFormForTest(Dummy, "Z0_Description"))
			{
				form.TextBox.contextMenuManager.InitializeContextMenu();
				CreateTextTemplate(form, "Wibble");

				form.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)form.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();

				AssertEquals("Wibble", templateMenu.DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].Tag = null;
				AssertNoExceptionThrown(templateMenu.DropDown.Items[0].PerformClick);
				Assert("ErrorReporter should have reported the error", ErrorReporter.LastMessageReported == "The .Tag of object sender is null. \n It should not be, as if it was before, it would have blown up in TemplatesItem_Opening  or ManageTemplatesItem_Opening).");
				ErrorReporter.Clear();
			}
		}

		public void TestZAutoCompleteTextBoxUndoText()
		{
			using (var form = new ZForm())
			{
				var autocomplete = new ZAutoCompleteTextBox();
				autocomplete.Dock = DockStyle.Fill;
				form.Controls.Add(autocomplete);

				form.Show();

				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.D1);
				KeySender.SendKeyPress(autocomplete, Keys.D1);
				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.D2);
				KeySender.SendKeyPress(autocomplete, Keys.D2);
				KeySender.SendKeyDown(autocomplete, autocomplete.Handle, Keys.D3);
				KeySender.SendKeyPress(autocomplete, Keys.D3);

				AssertEquals("123", autocomplete.Text);

				autocomplete.Undo();
				AssertEquals("12", autocomplete.Text);
				autocomplete.Undo();
				AssertEquals("1", autocomplete.Text);
				autocomplete.Undo();
				AssertEquals("", autocomplete.Text);
			}
		}

		StmNoteTemplate CreateTextTemplate(BoundTextBoxFormForTest form, string description, string templateText = "", bool isPublished = true, bool isAllCompanies = true)
		{
			var template = form.TextBox.contextMenuManager.TextTemplatesFactory.New();
			template.S8_Description = description;
			template.S8_TemplateText = templateText;
			template.IsPublished = isPublished;
			template.IsAllCompanies = isAllCompanies;
			template.Factory.Save();
			return template;
		}

		class BoundTextBoxFormForTest : ZForm
		{
			public BoundTextBoxFormForTest(BusinessObject bzo, string bindingMember, bool shouldEscapeAllSpecialCharacters = false)
				: base(bzo)
			{
				TextBox = new ZTextBox();
				TextBox.Multiline = true;
				TextBox.CharacterCasing = CharacterCasing.Normal;
				TextBox.ShouldEscapeAllSpecialCharacters = shouldEscapeAllSpecialCharacters;
				BindingSource.SetBindingMember(TextBox, bindingMember);
				Controls.Add(TextBox);
				Show();
			}

			public readonly ZTextBox TextBox;
		}

		class CustomDummyBusinessObject : DummyBusinessObject, ICustomTextTemplateContext
		{
			public CustomDummyBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, CargoWise.ComponentModel.KBindingMemberInfo bindingMemberInfo)
			{
				return Array.Empty<BusinessObject>();
			}

			public string GetTextTemplateContextID(object dataSource, CargoWise.ComponentModel.KBindingMemberInfo bindingMemberInfo)
			{
				return string.Empty;
			}
		}
	}
}
