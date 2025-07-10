using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ScavengingPurgeSettingsRegistryEditor))]
	public class ScavengingPurgeSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ScavengingPurgeSettingsRegistryItem("Category");
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ScavengingPurgeSettingsRegistryEditor(new ScavengingPurgeSettingsRegistryDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ScavengingPurgeSettingsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ScavengingPurgeSettings.GetDefaults() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ScavengingPurgeSettingsControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
