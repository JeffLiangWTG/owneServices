using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InboundMessageNotificationsRegistryItemEditor))]
	sealed class InboundMessageNotificationsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new InboundMessageNotificationsRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InboundMessageNotificationsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InboundMessageNotificationsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InboundMessageNotificationsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new InboundMessageNotificationsRule());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new InboundMessageNotificationsRule() };
		}
	}
}
