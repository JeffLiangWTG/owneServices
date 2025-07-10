using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContainerPenaltyFreeDaysOptionsRegistryItemEditor))]
	sealed class ContainerPenaltyFreeDaysOptionsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ContainerPenaltyFreeDaysOptionsRegistryItemEditor(new ContainerPenaltyFreeDaysOptionsRegistryItemDataType(new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ContainerPenaltyFreeDaysOptionsRegistryItemControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ContainerPenaltyFreeDaysOptionsRegistryItemControl);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override Integration.IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ContainerPenaltyFreeDaysOptionsRegistryItem(
						"DefaultContainerDetentionFreeDaysForImport",
						FreightDataRegistry.Categories.Freight_Container,
						ResString.GetMultilingualString("a78b6812-2f79-4c76-87d0-1694560b74b7", "Container Detention Free Days for Import"),
						ResString.GetMultilingualString("e3a24001-ca81-4bb0-84be-89d25122a7a0", "Number of days that a container will be held in detention free of charge."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new ContainerPenaltyFreeDaysOptions { FreeDays = 10, UnlimitedFreeDays = false });
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new ContainerPenaltyFreeDaysOptions { FreeDays = 10 } };
		}
	}
}
