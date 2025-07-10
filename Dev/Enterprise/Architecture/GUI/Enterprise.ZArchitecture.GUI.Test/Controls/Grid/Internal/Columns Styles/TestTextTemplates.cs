using System.Windows.Forms;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class TestTextTemplates : CargoWise.EntityFramework.Testing.TestCaseWithDummy
	{
		public void TestTemplatesMenuTemplate()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_Number = 3;
			dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_Number = 4;
			var columnInfo = new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 40);
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				columnStyle.CharacterCasing = CharacterCasing.Normal;
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

				testForm.Grid.BeginEdit(columnStyle, 1);
				templateMenu.DropDown.Show();
				templateMenu.DropDown.Items[0].PerformClick();
				AssertEquals("The Lucky Number is 4", columnStyle.TextBox.Text);
			}
		}

		public void TestMacroSelected()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].Z0_Description = "Some meaningless text<BR />";
			Dummy.Collection[0].Z0_Number = 12345;

			using (var testForm = new ZTestForm(Dummy))
			{
				var columnInfo = new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 40);
				columnInfo.SupportsMacroTemplates = true;
				columnInfo.MacroOpeningBracket = "(*";
				columnInfo.MacroClosingBracket = "*)";

				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				columnStyle.CharacterCasing = CharacterCasing.Normal;
				columnStyle.sourceData = testForm.Grid.ListManager;
				testForm.Show();

				testForm.Grid.BeginEdit(columnStyle, 0);
				columnStyle.ColumnStartedEditingForTest(columnStyle.TextBox);
				columnStyle.TextBox.SelectionStart = 10;
				columnStyle.TextBox.SelectionLength = 0;
				columnStyle.MacroSelected("<Z0_Number>");

				AssertEquals("Some meani(*Z0_Number*)ngless text<BR />", columnStyle.TextBox.Text);
				AssertEquals("Some meani12345ngless text<BR />", columnStyle.GetPreviewText());
			}
		}

		public void TestRootIndexOutOfRange()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].Z0_Description = "Some meaningless text<BR />";
			Dummy.Collection[0].Z0_Number = 12345;

			using (var testForm = new ZTestForm(Dummy))
			{
				var columnInfo = new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 40);
				columnInfo.SupportsMacroTemplates = true;
				columnInfo.MacroOpeningBracket = "(*";
				columnInfo.MacroClosingBracket = "*)";

				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				columnStyle.CharacterCasing = CharacterCasing.Normal;
				columnStyle.sourceData = testForm.Grid.ListManager;
				testForm.Show();

				testForm.Grid.BeginEdit(columnStyle, 0);

				columnStyle.EditingRowNum = -1;
				columnStyle.sourceData.List.Clear();
				var bizObjCollection = columnStyle.GetRoots();
				AssertNull(bizObjCollection);
			}
		}

		public void TestDisableTextTemplateForPasswordInGridRegistry()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_Number = 3;

			var columnInfo = new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Description, 40);
			using (var testForm = new ZTestForm(Dummy))
			{
				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				testForm.Show();
				testForm.Grid.BeginEdit(columnStyle, 0);

				columnStyle.contextMenuManager.TextTemplatesFactory.New();
				columnStyle.contextMenuManager.InitializeContextMenu();
				columnStyle.TextBox.ContextMenuStrip.Show();

				AssertEquals("PasswordChar Is Not Set, TextTemplate Is Enable", true, columnStyle.contextMenuManager.GetTextTemplateEnabled_ForTest());
			}

			using (var testForm = new ZTestForm(Dummy))
			{
				columnInfo.PasswordChar = '*';

				testForm.Grid.Columns.Add(columnInfo);
				testForm.Grid.RefreshTableStyles();
				var columnStyle = (ZTextBoxColumnStyle)testForm.Grid.Columns[columnInfo.ColumnName].ColumnStyle;
				testForm.Show();
				testForm.Grid.BeginEdit(columnStyle, 0);

				columnStyle.contextMenuManager.TextTemplatesFactory.New();
				columnStyle.contextMenuManager.InitializeContextMenu();
				columnStyle.TextBox.ContextMenuStrip.Show();

				AssertEquals("PasswordChar Is Set, TextTemplate Is Disable", false, columnStyle.contextMenuManager.GetTextTemplateEnabled_ForTest());
			}
		}
	}
}
