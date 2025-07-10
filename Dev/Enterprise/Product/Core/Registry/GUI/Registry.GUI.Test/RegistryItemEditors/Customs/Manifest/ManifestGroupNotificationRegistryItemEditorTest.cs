using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ManifestGroupNotificationRegistryItemEditor))]
	sealed class ManifestGroupNotificationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ManifestGroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, ManifestGroupNotification.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ManifestGroupNotificationRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(ManifestGroupNotificationControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { ManifestGroupNotification.Default };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ManifestGroupNotificationControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
