using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing.Registry
{
	[TestedType(typeof(GroupNotificationRegistryItemEditor))]
	internal class GroupNotificationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new GroupNotificationRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((GroupNotificationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(GroupNotificationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new Business.GroupNotificationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, GroupNotification.Default);

		protected override object[] GetValidRegistryValues() => new[] { GroupNotification.Default };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
