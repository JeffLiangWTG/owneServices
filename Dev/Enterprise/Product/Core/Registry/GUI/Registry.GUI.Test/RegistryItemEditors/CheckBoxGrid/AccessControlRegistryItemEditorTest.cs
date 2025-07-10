using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.Business.Web;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AccessControlRegistryItemEditor))]
	sealed class AccessControlRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new AccessControlRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebAccessControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebAccessControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new AccessControlRegistryItem("", null, null, null, new DummyAccessRules(), RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			OrgsRoleAccessCollection collection = new DummyAccessRules().GetDefaultCollection();

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
