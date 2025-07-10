using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NotificationFrequencyRegistryItemEditor))]
	sealed class NotificationFrequencyRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new NotificationFrequencyRegistryItemEditor(null, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((NotificationFrequencyControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(NotificationFrequencyControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new NotificationFrequencyRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new NotificationFrequency());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new NotificationFrequency() };
		}
	}
}
