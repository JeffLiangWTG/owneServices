using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RatingTokenAuthenticationRegistryItemEditor))]
	sealed class RatingTokenAuthenticationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override RegistryItemEditor GetEditor()
		{
			return new RatingTokenAuthenticationRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((RatingTokenAuthenticationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RatingTokenAuthenticationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RatingTokenAuthenticationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new RatingTokenAuthenticationCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var overrideValue = new RatingTokenAuthenticationCollection();
			overrideValue.Add(new RatingTokenAuthentication() { ClientId = "9D4F560C-BA71-464F-88D3-646E41192396", StaffCode = "E" });
			return new RatingTokenAuthenticationCollection[] { new RatingTokenAuthenticationCollection(), overrideValue };
		}
	}
}
