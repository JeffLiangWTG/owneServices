using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(TestRigRegistryEditor))]
	public class TestRigRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TestRigRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TestRigRegistryEditor(new TestRigRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TestRigRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { new TestRigRegistryHeader() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TestRigRegistryControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
