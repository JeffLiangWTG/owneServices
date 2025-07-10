using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(HBLAndHAWBBrandingOptionRegistryItemEditor))]
	sealed class HBLAndHAWBBrandingOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestHBLAndHAWBBrandingOptionEditorInfo()
		{
			RegistryItemEditor editor = RegistryItemEditorFactory.NewEditor(null, null, null, new HBLAndHAWBBrandingOptionEditorInfo(), null);
			AssertEquals("Editor.GetType()", typeof(HBLAndHAWBBrandingOptionRegistryItemEditor), editor.GetType());
		}

		public void TestHBLAndHAWBBrandingOptionControl()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				var fieldInfo = typeof(RadioButtonControl).GetField("yesRadioButton", BindingFlags.NonPublic | BindingFlags.Instance);
				var yesRadioButton = (ZRadioButton)fieldInfo.GetValue(editorPane);

				fieldInfo = typeof(RadioButtonControl).GetField("noRadioButton", BindingFlags.NonPublic | BindingFlags.Instance);
				var noRadioButton = (ZRadioButton)fieldInfo.GetValue(editorPane);

				AssertEquals("yesRadioButton.Text", HBLAndHAWBBrandingOptionEditorInfo.AgentBranded, yesRadioButton.Text);
				AssertEquals("noRadioButton.Text", HBLAndHAWBBrandingOptionEditorInfo.ClientBranded, noRadioButton.Text);

				Editor.SetValueFromEditorPane(editorPane, HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);
				AssertEquals("yesRadioButton.Checked", true, yesRadioButton.Checked);
				AssertEquals("noRadioButton.Checked", false, noRadioButton.Checked);

				Editor.SetValueFromEditorPane(editorPane, HBLAndHAWBBrandingOptionEditorInfo.ClientBranded);
				AssertEquals("yesRadioButton.Checked", false, yesRadioButton.Checked);
				AssertEquals("noRadioButton.Checked", true, noRadioButton.Checked);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new HBLAndHAWBBrandingOptionRegistryItemEditor(null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(HBLAndHAWBBrandingOptionRegistryItemEditor.HBLAndHAWBBrandingOptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new string[]
			{
				HBLAndHAWBBrandingOptionEditorInfo.AgentBranded,
				HBLAndHAWBBrandingOptionEditorInfo.ClientBranded
			};
		}

		#endregion
	}
}
