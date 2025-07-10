using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TrackAndTraceNotificationsRegistryItemEditor))]
	sealed class TrackAndTraceNotificationsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TrackAndTraceNotificationsRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TrackAndTraceNotificationsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TrackAndTraceNotificationsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TrackAndTraceNotificationsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new TrackAndTraceNotificationsRule(), new TrackAndTraceNotificationsRuleVisibilityProvider());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new TrackAndTraceNotificationsRule() };
		}
	}
}
