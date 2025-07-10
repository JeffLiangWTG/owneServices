using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(MexicoNotificationRemainingFolioConfigurationRegistryItemEditor))]
	public class MexicoNotificationRemainingFolioConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new MexicoNotificationRemainingFolioConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((MexicoNotificationRemainingFolioConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(MexicoNotificationRemainingFolioConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new MexicoNotificationRemainingFolioConfigurationRegistryItem(
						name: string.Empty,
						category: null,
						caption: null,
						hint: null,
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: new MexicoNotificationRemainingFolioConfiguration()
						);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { GetRegistryItemWithSystemStorageLevel().DefaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
