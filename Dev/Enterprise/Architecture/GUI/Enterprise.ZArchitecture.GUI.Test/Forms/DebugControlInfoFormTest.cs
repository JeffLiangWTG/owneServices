using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.Core.GUI.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class DebugControlInfoFormTest : TestCaseWithDummy
	{
		public void TestShowFormInfo()
		{
			using (var debugControlForm = new DebugControlInfoForm())
			using (var testForm = new TestZForm(Dummy))
			{
				testForm.ActiveControl = testForm.TextBox1;
				debugControlForm.ShowFormInfo(testForm);

				AssertEquals("Control Information for TestColumnBoundTextBox", debugControlForm.Text);

				var expectedText = @"ZForm > Test Caption 

Control: TestColumnBoundTextBox (Enterprise.Core.GUI.Testing.TestTextBox)
   in Form: ZForm (Enterprise.Core.GUI.Testing.TestZForm)

(none)
";
				AssertMultilineASCIIEquals(expectedText, debugControlForm.messageTextBox.Text);
				AssertEquals(CharacterCasing.Normal, debugControlForm.messageTextBox.CharacterCasing);
				AssertEquals(Color.White, debugControlForm.messageTextBox.BackColor);

				Assert("dataFieldMapButton should be visible because testForm is ZForm", debugControlForm.dataFieldMapButton.Visible);
				Assert("mcrDataFieldMapButton should be visible because testForm is ZForm", debugControlForm.mcrDataFieldMapButton.Visible);
				Assert("copyContentButton should be invisible because list content is empty", !debugControlForm.copyContentButton.Visible);
			}
		}

		public void TestShowControlInfo()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			using (var debugControlForm = new DebugControlInfoForm())
			using (var testForm = new TestZForm(shipment) { Name = "TestZForm" })
			{
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				testForm.TextBox1.SetDataBinding(shipment, "JS_HouseBill");
				testForm.Show();
				testForm.ActiveControl = testForm.TextBox1;
				debugControlForm.ShowControlInfo(testForm.TextBox1);

				AssertEquals("Control Information for TestColumnBoundTextBox", debugControlForm.Text);

				var expectedText = @"TestZForm > TestColumnBoundTextBox

Control: TestColumnBoundTextBox (Enterprise.Core.GUI.Testing.TestTextBox)
   in Form: TestZForm (Enterprise.Core.GUI.Testing.TestZForm)

DataSource Type: Enterprise.Freight.Forwarding.Business.ForwardingShipment
Binding Member: JS_HouseBill (ZString)

Table/Field Name: JobShipment.JS_HouseBill

DataContext: .ForwardingShipment
Macro: <JS_HouseBill>
";
				AssertMultilineASCIIEquals(expectedText, debugControlForm.messageTextBox.Text);
				Assert("dataFieldMapButton should be visible", debugControlForm.dataFieldMapButton.Visible);
				Assert("mcrDataFieldMapButton should be visible", debugControlForm.mcrDataFieldMapButton.Visible);
				Assert("copyContentButton should be invisible because list content is empty", !debugControlForm.copyContentButton.Visible);
			}
		}

		public void TestShowShowControlInfoWithKForm()
		{
			using (var debugControlForm = new DebugControlInfoForm())
			using (var testForm = new TestKForm() { Name = "TestKForm" })
			{
				testForm.ActiveControl = testForm.TextBox1;
				debugControlForm.ShowControlInfo(testForm.TextBox1);

				AssertEquals("Control Information for TestColumnBoundTextBox", debugControlForm.Text);

				var expectedText = @"TestKForm > TestColumnBoundTextBox

Control: TestColumnBoundTextBox (Enterprise.Core.GUI.Testing.TestTextBox)
   in Form: TestKForm (Enterprise.ZArchitecture.GUI.Testing.TestKForm)

(none)
";

				AssertMultilineASCIIEquals(expectedText, debugControlForm.messageTextBox.Text);
				Assert("dataFieldMapButton should be invisible because testForm is not ZForm", !debugControlForm.dataFieldMapButton.Visible);
				Assert("mcrDataFieldMapButton should be invisible because testForm is not ZForm", !debugControlForm.mcrDataFieldMapButton.Visible);
				Assert("copyContentButton should be invisible because list content is empty", !debugControlForm.copyContentButton.Visible);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyListContent()
		{
			var dummyWithList = Factory.New<DummyWithChangingCodeDescriptionPairList>();
			using (var debugControlForm = new DebugControlInfoForm())
			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.DropEdit.List = dummyWithList.DummyList;
				testForm.DropEdit.SetDataBinding(dummyWithList, AutoDummyBizo.Schema.Z0_FK_Code);
				testForm.Show();
				Application.DoEvents();
				debugControlForm.ShowControlInfo(testForm.DropEdit.CodeBox);
				Assert("dataFieldMapButton should be visible", debugControlForm.dataFieldMapButton.Visible);
				Assert("mcrDataFieldMapButton should be visible", debugControlForm.mcrDataFieldMapButton.Visible);
				Assert("copyContentButton should be visible because list content is not empty", debugControlForm.copyContentButton.Visible);

				var expectedListContentBuilder = new StringBuilder();
				foreach (ICodeDescription item in dummyWithList.DummyList)
				{
					expectedListContentBuilder.AppendLine($"{item.Code}	{item.Description}");
				}

				debugControlForm.copyContentButton.PerformClick();
				var actualListContent = SafeClipboard.GetText();
				AssertEquals("Copy list content should get the correct value", expectedListContentBuilder.ToString(), actualListContent);
			}
		}
	}

	internal class TestKForm : KForm
	{
		public TestKForm()
		{
			TextBox1 = new TestTextBox { Name = "TestColumnBoundTextBox", CaptionResourceString = Res.GetData("1A904F16-BB54-4100-B005-6D361C8F1F99", "Test Caption") };
			Controls.Add(TextBox1);
		}

		internal TestTextBox TextBox1;
	}

	[TestedType(typeof(DebugControlInfoForm))]
	public class DebugControlInfoFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new DebugControlInfoForm();
	}
}
