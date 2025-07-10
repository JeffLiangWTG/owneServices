using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.GUI.Registry.CDSUCRAutomation;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing.Registry
{
	[TestedType(typeof(CDSUCRAutomationControl))]
	public class CDSDUCRAutomationRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CDSDUCRAutomationSettings(new FallbackLevel(Guid.Empty,
				Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control,
			IBusiness businessEntity)
		{
			return (control as CDSUCRAutomationControl).comboBoxCDSAutomationSettings.ReadOnly;
		}
	}

	[TestedType(typeof(CDSUCRAutomationRegistryItemEditor))]
	class DSDUCRAutomationRegistryRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() =>
			new CDSUCRAutomationRegistryItemEditor(new CDSDUCRAutomationRegistryDataType(),
				new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane) =>
			!((CDSUCRAutomationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() =>
			typeof(CDSUCRAutomationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new CDSDUCRAutomationRegistryItem("", null, null, null,
				RegistryStorageFlags.System, RegistryOptions.Default);

		protected override object[] GetValidRegistryValues() => new object[] {
			new CDSDUCRAutomationSettings() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor =>
			RegistryItemEditor.EditorPaneAnchor.All;
	}
}
