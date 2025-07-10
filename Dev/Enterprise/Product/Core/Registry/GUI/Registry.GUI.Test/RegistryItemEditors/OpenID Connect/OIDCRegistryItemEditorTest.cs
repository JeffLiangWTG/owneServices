using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OIDCRegistryItemEditor))]
	sealed class OIDCRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new OIDCRegistryItemEditor(new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue));
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((OIDCRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OIDCRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OIDCConfigRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, OIDCConfig.DefaultValue);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { OIDCConfig.DefaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
		#endregion
	}
}
