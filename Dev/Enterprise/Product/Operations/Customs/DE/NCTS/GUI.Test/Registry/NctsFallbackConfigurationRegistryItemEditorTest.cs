using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(NctsFallbackConfigurationRegistryItemEditor))]
	class NctsFallbackConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new NctsFallbackConfigurationRegistryItemEditor(new NctsFallbackConfigurationRegistryDataType(), new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), new BusinessObjectFactory());

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			var control = (NctsFallbackConfigurationRegistryItemControl)editorPane;
			var groupBox = (ZArchitecture.GUI.ZGroupBox)control.Controls.Find("fallbackSettingsGroupBox", true).Single();
			return !groupBox.GetReadOnly();
		}

		protected override Type GetExpectedEditorPaneType() => typeof(NctsFallbackConfigurationRegistryItemControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new NctsFallbackConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues() =>
		[
			new NctsFallbackConfiguration { Start = ZDateTime.Today.AddHours(6).AddMinutes(30).AddSeconds(10), CustomsIncidentNumber = "1234567890" },
		];

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
	}
}
