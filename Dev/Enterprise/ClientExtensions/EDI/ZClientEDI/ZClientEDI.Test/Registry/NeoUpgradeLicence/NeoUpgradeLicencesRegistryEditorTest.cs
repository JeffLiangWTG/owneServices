using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(NeoUpgradeLicencesRegistryEditor))]
	public class NeoUpgradeLicencesRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new NeoUpgradeLicencesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		protected override RegistryItemEditor GetEditor() => new NeoUpgradeLicencesRegistryEditor(new NeoUpgradeLicencesRegistryDataType(), null, Factory);
		protected override Type GetExpectedEditorPaneType() => typeof(NeoUpgradeLicencesControl);
		protected override object[] GetValidRegistryValues() => new object[] { new NeoUpgradeLicenceCollection() };
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((NeoUpgradeLicencesControl)editorPane).ReadOnly;
		#endregion Implementation
	}
}
