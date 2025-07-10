using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RequireReasonForCLRRegistryItemEditor))]
	sealed class RequireReasonForCLRRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor() => new RequireReasonForCLRRegistryItemEditor(new RequireReasonForCLRDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((RequireReasonForCLRControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(RequireReasonForCLRControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new RequireReasonForCLRRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new RequireReasonForCLRWrapper());

		protected override object[] GetValidRegistryValues() => new object[] { new RequireReasonForCLRWrapper() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		#endregion
	}
}
