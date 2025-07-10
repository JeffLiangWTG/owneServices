using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ParameterizedStringRegistryItemEditor))]
	sealed class ParameterizedStringRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ParameterizedStringRegistryItem("test", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, RegistryOptions.Default,
				ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")), ResString.GetMultilingualString("p", "parameter"));
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ParameterizedStringRegistryItemEditor(GetRegistryItemWithSystemStorageLevel());
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ParameterizedStringControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var item = (ParameterizedStringRegistryItem)GetRegistryItemWithSystemStorageLevel();
			return new object[] {
				ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")),
				item.Deserialise("{0} Testing"),
				item.Deserialise("Nothing"),
			};
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		public void TestInformationText()
		{
			var editor = (ParameterizedStringRegistryItemEditor)GetEditor();
			using (var form = new Form())
			using (var control = (ParameterizedStringControl)editor.NewWinFormsEditorPane())
			{
				form.Controls.Add(control);
				form.Show();
				editor.SetValueFromEditorPane(control, ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")));
				AssertEquals("{0} will be replaced with the value of parameter\r\nPreview:\r\nTest Value", control.informationTextBox.Text);
				control.valueTextBox.Text = "{0} Testing";
				AssertEquals("{0} will be replaced with the value of parameter\r\nPreview:\r\nValue Testing", control.informationTextBox.Text);
				control.valueTextBox.Text = "Something else";
				AssertEquals("{0} will be replaced with the value of parameter\r\nPreview:\r\nSomething else", control.informationTextBox.Text);
			}
		}
	}
}
