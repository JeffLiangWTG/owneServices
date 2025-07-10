using System;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.Registry.Testing
{
	[TestedType(typeof(TriggerPointsConfigurationRegistryItemEditor))]
	class TriggerPointsConfigurationRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new TriggerPointsConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((TriggerPointsConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TriggerPointsConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TriggerPointsConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var triggerPointsConfiguration1 = new TriggerPointsConfiguration();
			triggerPointsConfiguration1.ImportTriggerPoint = ZString.Empty;
			triggerPointsConfiguration1.ExportTriggerPoint = ZString.Empty;

			var triggerPointsConfiguration2 = new TriggerPointsConfiguration();
			triggerPointsConfiguration2.ImportTriggerPoint = TriggerPointsCodeList.Codes.PAB;
			triggerPointsConfiguration2.ExportTriggerPoint = TriggerPointsCodeList.Codes.REC;

			return new object[] { triggerPointsConfiguration1, triggerPointsConfiguration2 };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
