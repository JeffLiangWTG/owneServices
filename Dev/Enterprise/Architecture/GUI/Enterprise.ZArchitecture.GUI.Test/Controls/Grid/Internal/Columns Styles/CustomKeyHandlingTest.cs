using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class CustomKeyHandlingTest : TestCaseWithDummy
	{
		public void TestSetupMultilineTextBoxInConstructor()
		{
			using (var columnStyle = new ZMultiLineTextBoxColumnStyle(new ZMultiLineTextBoxColumnInfo()))
			{
				Assert("Should be multiline", columnStyle.TextBox.Multiline);
				AssertEquals("ScrollBars", ScrollBars.Both, columnStyle.TextBox.ScrollBars);
				Assert("Should accept return", columnStyle.TextBox.AcceptsReturn);
				//This is true almost all the time, but sometimes font family is "Comic Sans MS" and sometimes font size is 12, 20.25, 7.8, etc.
				//Since we don't do anything special to change these, and the default is almost always what we want, just going to comment these lines out.
				//AssertEquals("Microsoft Sans Serif", columnStyle.TextBox.Font.FontFamily.Name);
				//AssertEquals(8.25f, columnStyle.TextBox.Font.Size);
			}
		}

		public void TestMinimumEditControlWidth()
		{
			var dummyChild = Dummy.Collection.AddNew();
			var columnInfo = new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 0);
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZMultiLineTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				testForm.Show();

				columnStyle.sourceData = testForm.Grid.ListManager;

				var scrollBarWidth = testForm.Grid.IsVerticalScrollBarVisible ? SystemInformation.VerticalScrollBarArrowHeight : 0;
				var expectedWidth = (testForm.Grid.ClientRectangle.Width - scrollBarWidth >= 300) ? 300 : (testForm.Grid.ClientRectangle.Width - scrollBarWidth - 1);
				AssertEquals("EditControlWidth", expectedWidth, columnStyle.GetEditControlBounds(new Rectangle(0, 0, 0, 0)).Width);

				columnInfo.MinimumEditControlWidth = 400;
				expectedWidth = (testForm.Grid.ClientRectangle.Width - scrollBarWidth >= 400) ? 400 : (testForm.Grid.ClientRectangle.Width - scrollBarWidth - 1);
				AssertEquals("EditControlWidth", expectedWidth, columnStyle.GetEditControlBounds(new Rectangle(0, 0, 0, 0)).Width);
			}
		}

#if !WINZOR // Removed from Winzor as newlines should be handled client-side, rather than in server-side key event code.
		public void TestShouldProcessCmdKey()
		{
			var dummyChild = Dummy.Collection.AddNew();
			var columnInfo = new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZMultiLineTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				ICustomKeyHandlingGridColumn customKeyHandlingGridColumn = columnStyle;
				columnStyle.sourceData = testForm.Grid.ListManager;
				testForm.Show();

				var message = new Message();
				message.Msg = WindowsMessage.WM_KEYDOWN;
				var keyData = Keys.Enter;
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
		}

		public void TestProcessCmdKey()
		{
			using (var columnStyle = new ZMultiLineTextBoxColumnStyle(new ZMultiLineTextBoxColumnInfo()))
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

		public void TestTemplatesMenuTemplate()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_Number = 3;
			var columnInfo = new ZMultiLineTextBoxColumnInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 40);
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZMultiLineTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				columnStyle.CharacterCasing = CharacterCasing.Normal;
				ICustomKeyHandlingGridColumn customKeyHandlingGridColumn = columnStyle;
				columnStyle.sourceData = testForm.Grid.ListManager;
				testForm.Show();

				testForm.Grid.BeginEdit(columnStyle, 0);

				var template = columnStyle.contextMenuManager.TextTemplatesFactory.New();
				template.S8_Description = "Alpha";
				template.S8_TemplateText = "The Lucky Number is <Z0_Number>";
				template.Factory.Save();

				columnStyle.contextMenuManager.InitializeContextMenu();
				columnStyle.TextBox.ContextMenuStrip.Show();
				var templateMenu = (ToolStripMenuItem)columnStyle.TextBox.ContextMenuStrip.Items[0];
				templateMenu.DropDown.Show();
				AssertEquals("Alpha", templateMenu.DropDown.Items[0].Text);
				AssertEquals("The Lucky Number is 3", templateMenu.DropDown.Items[0].ToolTipText);
				AssertEquals("Create Template", templateMenu.DropDown.Items[2].Text);
				AssertEquals("Manage Template", templateMenu.DropDown.Items[3].Text);
				AssertEquals(true, templateMenu.DropDown.Items[3].Enabled);
				((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Show();
				AssertEquals("Alpha", ((ToolStripMenuItem)templateMenu.DropDown.Items[3]).DropDown.Items[0].Text);

				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals("The Lucky Number is 3", columnStyle.TextBox.Text);
			}
		}
	}
}
