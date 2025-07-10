using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeConfigurationRegistryItemEditor))]
	class ElectronicProcessingChargeConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ElectronicProcessingChargeConfigurationRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ElectronicProcessingChargeConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ElectronicProcessingChargeConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ElectronicProcessingChargeConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new ElectronicProcessingChargeConfigurationCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ElectronicProcessingChargeConfigurationCollection();
			collection.Add(new ElectronicProcessingChargeConfiguration { JobType = "SHP", StartDate = new ZDate(2021, 7, 19), EndDate = new ZDate(2021, 7, 20) });

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
