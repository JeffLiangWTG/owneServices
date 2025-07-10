using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GroupNotificationRegistryItemEditor))]
	sealed class GroupNotificationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GroupNotificationRegistryItem<GroupNotification>("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, GroupNotification.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GroupNotificationRegistryItemEditor(RegistryItem.DataType, null, new BusinessObjectFactory());
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(GroupNotificationControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new[] { GroupNotification.Default };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((GroupNotificationControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
