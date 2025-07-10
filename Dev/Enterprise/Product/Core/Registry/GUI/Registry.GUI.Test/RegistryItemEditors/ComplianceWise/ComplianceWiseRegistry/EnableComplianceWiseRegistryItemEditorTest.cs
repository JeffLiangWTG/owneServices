using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EnableComplianceWiseRegistryItemEditor))]
	sealed class EnableComplianceWiseRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new EnableComplianceWiseRegistryItemEditor(new EnableComplianceWiseRegistryDataType(), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EnableComplianceWiseRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EnableComplianceWiseRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EnableComplianceWiseRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new EnableComplianceWiseRegistryBusinessObject());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EnableComplianceWiseRegistryBusinessObject() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		public new void TestEnableEditorPane()
		{
			using (var control = (EnableComplianceWiseRegistryControl)Editor.NewWinFormsEditorPane())
			{
				Editor.EnableEditorPane(control, true);
				AssertEquals(false, control.ReadOnly);

				Editor.EnableEditorPane(control, false);
				AssertEquals(true, control.ReadOnly);
			}
		}
	}
}
