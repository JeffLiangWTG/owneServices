using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class MultiControlWithMultilineTextBoxTest : TestCaseWithDummy
	{
		public void TestEditWithOnlyOneCharacter()
		{
			var dummy = Factory.New<DummyWithReferenceableCollection>();
			var dummyChild = dummy.Codes.AddNew();
			dummyChild.Code = nameof(FieldType.TextCodeFindBox);
			var columnInfo = new ZMultiControlColumnStyleInfo("Code", 40);
			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnInfo);
				columnInfo.FieldTypeColumnName = "Code";
				testForm.Show();
				Application.DoEvents();
				testForm.Grid.SetDataBinding(dummy, "Codes");
				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnInfo.ColumnName])
				{
					columnStyle.GetColumnValueAtRowOverride = "";
					var bounds = new Rectangle(10, 10, 100, 100);
					columnStyle.EditExposed(testForm.Grid.ListManager, 0, bounds, false);
					columnStyle.GridControl.Text = "A";
					columnStyle.EditExposed(testForm.Grid.ListManager, 0, bounds, true);
					columnStyle.CommitExposed(testForm.Grid.ListManager, 0);
					AssertEquals("A", dummyChild.Code);
				}
			}
		}

		public void TestEdit()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);
			var columnInfo = new ZMultiControlColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnInfo);
				columnInfo.FieldTypeColumnName = DummyBizoSchema.Constants.Z0_NVarChar;
				testForm.Show();
				Application.DoEvents();
				testForm.Grid.SetDataBinding(Dummy, "Collection");
				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnInfo.ColumnName])
				{
					var bounds = new Rectangle(10, 10, 100, 100);
					columnStyle.EditExposed(testForm.Grid.ListManager, 0, bounds, false);
					Assert("Single line textBox", !columnStyle.TextBox.Multiline);
					AssertEquals("Should not have scrollbars", ScrollBars.None, columnStyle.TextBox.ScrollBars);
					AssertEquals(12, columnStyle.TextBox.Bounds.X);
					AssertEquals(12, columnStyle.TextBox.Bounds.Y);
					AssertEquals(98, columnStyle.TextBox.Bounds.Width);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(13), columnStyle.TextBox.Bounds.Height, 1m);

					dummyChild.Z0_NVarChar = nameof(FieldType.TextMultiLine);
					columnStyle.EditExposed(testForm.Grid.ListManager, 0, bounds, false);
					Assert("Multi line textBox", columnStyle.TextBox.Multiline);
					AssertEquals("Should have scrollbars", ScrollBars.Both, columnStyle.TextBox.ScrollBars);
					AssertEquals(12, columnStyle.TextBox.Bounds.X);
					AssertEquals(12, columnStyle.TextBox.Bounds.Y);
					AssertEquals(98, columnStyle.TextBox.Bounds.Width);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(198), columnStyle.TextBox.Bounds.Height, 1m);
				}
			}
		}

		public void TestEditWithDisposedTextBox()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);

			using (var testForm = new ZTestForm())
			{
				var columnInfo = new ZMultiControlColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);
				testForm.Grid.ColumnStyles.Add(columnInfo);
				columnInfo.FieldTypeColumnName = DummyBizoSchema.Constants.Z0_NVarChar;

				testForm.Show();
				Application.DoEvents();

				testForm.Grid.SetDataBinding(Dummy, "Collection");
				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnInfo.ColumnName])
				{
					columnStyle.TextBox.Dispose();
					AssertNoExceptionThrown(() => columnStyle.EditExposed(testForm.Grid.ListManager, 0, new Rectangle(10, 10, 100, 100), false));
				}
			}
		}

#if !WINZOR
		public void TestShouldProcessCmdKey()
		{
			var dummyChild = Dummy.Collection.AddNew();
			var columnInfo = new ZMultiControlColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);
			ZTestForm testForm = null;
			ZMultiControlColumnStyle columnStyle = null;

			try
			{
				testForm = new ZTestForm();
				testForm.Grid.ColumnStyles.Add(columnInfo);
				testForm.Show();
				testForm.Grid.SetDataBinding(Dummy, "Collection");
				columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnInfo.ColumnName];
				var customKeyHandlingGridColumn = (ICustomKeyHandlingGridColumn)columnStyle;

				var message = new Message();
				message.Msg = WindowsMessage.WM_KEYDOWN;
				var keyData = Keys.Enter;
				Assert("not a MUL columntype", !customKeyHandlingGridColumn.ShouldProcessCmdKey(ref message, keyData));

				columnStyle.EditControl.ControlType = FieldType.TextMultiLine;
				Assert("Should be processed", customKeyHandlingGridColumn.ShouldProcessCmdKey(ref message, keyData));

				keyData = Keys.End;
				Assert("not an Enter key", !customKeyHandlingGridColumn.ShouldProcessCmdKey(ref message, keyData));

				keyData = Keys.Enter;
				message.Msg = WindowsMessage.WM_LBUTTONDOWN;
				Assert("not a key down windows message", !customKeyHandlingGridColumn.ShouldProcessCmdKey(ref message, keyData));

				message.Msg = WindowsMessage.WM_KEYDOWN;
				columnStyle.ReadOnly = true;
				Assert("cell is readonly", !customKeyHandlingGridColumn.ShouldProcessCmdKey(ref message, keyData));
			}
			finally
			{
				testForm.Dispose();
				columnStyle.Dispose();
			}
		}

		public void TestProcessCmdKey()
		{
			using (var columnStyle = new ZMultiControlColumnStyle(new ZMultiControlColumnStyleInfo()))
			{
				var message = new Message();
				message.Msg = WindowsMessage.WM_KEYDOWN;
				var keyData = Keys.Enter;

				columnStyle.TextBox.Text = "First LineSecond Line";
				((DataGridTextBox)columnStyle.TextBox).IsInEditOrNavigateMode = true;
				columnStyle.TextBox.SelectionStart = 10;
				columnStyle.TextBox.SelectionLength = 0;

				var customKeyHandlingGridColumn = (ICustomKeyHandlingGridColumn)columnStyle;
				customKeyHandlingGridColumn.ProcessCmdKey(ref message, keyData);
				AssertEquals("First Line" + System.Environment.NewLine + "Second Line", columnStyle.TextBox.Text);
				AssertEquals(12, columnStyle.TextBox.SelectionStart);
				AssertEquals(false, ((DataGridTextBox)columnStyle.TextBox).IsInEditOrNavigateMode);
			}
		}
#endif

		public class TestTextTemplates : TestCaseWithDummy
		{
			public void TestTemplatesMenuTemplate()
			{
				var dummyChild = Dummy.Collection.AddNew();
				dummyChild.Z0_Number = 3;
				dummyChild = Dummy.Collection.AddNew();
				dummyChild.Z0_Number = 4;
				var columnInfo = new ZMultiControlColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);
				columnInfo.FieldTypeColumnName = "Z0_VarCharMax";
				columnInfo.CharacterCasing = CharacterCasing.Normal;
				using (var testForm = new ZTestForm(Dummy))
				{
					testForm.Grid.Columns.Add(columnInfo);
					testForm.Grid.RefreshTableStyles();
					var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
					columnStyle.sourceData = testForm.Grid.ListManager;
					testForm.Show();

					testForm.Grid.BeginEdit(columnStyle, 0);

					var template = ((ZTextBox)columnStyle.EditControl.CurrentEditor).contextMenuManager.TextTemplatesFactory.New();
					template.S8_Description = "Alpha";
					template.S8_TemplateText = "The Lucky Number is <Z0_Number>";
					template.Factory.Save();

					((ZTextBox)columnStyle.EditControl.CurrentEditor).contextMenuManager.InitializeContextMenu();
					((ZTextBox)columnStyle.EditControl.CurrentEditor).ContextMenuStrip.Show();
					var templateMenu = (ToolStripMenuItem)((ZTextBox)columnStyle.EditControl.CurrentEditor).ContextMenuStrip.Items[0];
					templateMenu.DropDown.Show();
					AssertEquals("Alpha", templateMenu.DropDown.Items[0].Text);
					AssertEquals("The Lucky Number is 3", templateMenu.DropDown.Items[0].ToolTipText);
					AssertEquals("Create Template", templateMenu.DropDown.Items[2].Text);
					AssertEquals("Manage Template", templateMenu.DropDown.Items[3].Text);
					AssertEquals(true, templateMenu.DropDown.Items[3].Enabled);
					((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Show();
					AssertEquals("Alpha", ((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Items[0].Text);

					testForm.Grid.BeginEdit(columnStyle, 1);
					templateMenu.DropDown.Show();
					templateMenu.DropDown.Items[0].PerformClick();
					AssertEquals("The Lucky Number is 4", ((ZTextBox)columnStyle.EditControl.CurrentEditor).Text);
				}
			}
		}
	}
}
