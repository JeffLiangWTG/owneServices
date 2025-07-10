using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NotificationEmailTemplateRegistryItemEditor))]
	sealed class NotificationEmailTemplateRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new NotificationEmailTemplateRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((NotificationEmailTemplateRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(NotificationEmailTemplateRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new NotificationEmailTemplateRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, GetType(), "", "test notification email template");
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new NotificationEmailTemplate(GetType(), "test template") };
		}

		#endregion
	}
}
