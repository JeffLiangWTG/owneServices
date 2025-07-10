using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Registry.Testing
{
	[TestedType(typeof(DigitalSignatureRegistryItemEditor))]
	sealed class DigitalSignatureRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DigitalSignatureRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DigitalSignatureRegistryControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DigitalSignatureRegistryControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DigitalSignatureRegistryItem("", null, null, null, DigitalSignatureTestHelper.GetDigitalSignatureRegistry());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { DigitalSignatureTestHelper.GetDigitalSignatureRegistry(), DigitalSignatureTestHelper.GetDigitalSignatureRegistry_WithPassword() };
		}
	}
}
