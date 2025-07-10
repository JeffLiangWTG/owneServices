using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestZFormTranslatableBasherTest : TestCaseWithDummy, INotifications
	{
		[GuiTest]
		public void TestBashTranslatable()
		{
			var report = ZFormTranslatableBasherTest.BashTranslatable(
				null,
				() =>
				{
					Form testForm = new TestTranslatableForm(Dummy);
					testForm.Show();
					Application.DoEvents();
					ZGridControlBasher.ExposeAllColumnsInAllGrids(testForm, this);
					return testForm;
				});

			AssertMultilineASCIIEquals("Report",
				EtaloneReport.Replace('\t', ' ').Replace("  ", " ").Trim(),
				report.Replace('\t', ' ').Replace("  ", " ").Replace("<br/>", "\r\n").Trim());
		}

		const string EtaloneReport =
@"Form Enterprise.ZArchitecture.GUI.Testing.TestTranslatableForm has nontranslatable controls:

ZForm: TestForm
Text: Form Caption
Error: FormCaption set in code
Path: TestForm (TestTranslatableForm)

Control: groupBox1
Text: Blaha
Error: Caption is set in code. 
Path: TestForm : groupBox1 (ZGroupBox)

Control: button1
Text: Blaha
Error: Caption is set in code. 
Path: TestForm : groupBox1 : button1 (ZButton)

Menu: menuItem1
Text: Menu One
Error: Res.GetString is not used. 
Path: TestForm : groupBox1 : button1 (ZButton)

Control: groupBox3
Text: Blaha
Error: Control type is not supported for resource strings - use an appropriate control from the Enterprise.ZArchitecture.GUI assembly. 
Path: TestForm : groupBox3 (GroupBox)

Column: Z0_Int
Text: Blaha
Error: Column is not translatable, set the ColumnID or CaptionResourceString.
Path: TestForm : grid (ZGrid)

ToolStripItem: ToolStripButton
Text: Tool Strip Button
Error: Item is not translatable, use ResourceStringCaption to set the text
Path: TestForm :  (ToolStrip)

Drop Edit Item: ZDropEdit
Text: Zee Not Translatable
Error: Drop Down item display is not translatable, use ResString.GetMultilingualString() when creating the related CodePairDescriptionList
Path: TestForm : ZDropEdit (ZDropEdit)

Menu: menuItem2
Text: onClick=Enterprise.ZArchitecture.GUI.Testing.TestTranslatableForm.menuItem2_Click
Error: Use ZMenuItem and set the Caption = ResString.GetMultilingualString() instead of .Text = Res.GetString()
Path: TestForm (TestTranslatableForm)

For detail see Wiki: <A href='https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx'>https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx</A>";

		#region Implementation of INotifications

		public void Add(INotification notification) { }

		#endregion
	}
}
