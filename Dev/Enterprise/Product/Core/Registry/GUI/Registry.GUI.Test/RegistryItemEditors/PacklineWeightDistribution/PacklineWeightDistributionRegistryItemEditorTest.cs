using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.PacklineWeightDistribution
{
	[TestedType(typeof(PacklineWeightDistributionRegistryItemEditor))]
	sealed class PacklineWeightDistributionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectNoExceptions]
		public override void TestRegistryItemAcceptsEditorValue()
		{
			using (FreightDataRegistry.Instance.PacklineWeightDistribution.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new PacklineWeightDistributionConfiguration()))
			{
				RegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new PacklineWeightDistributionConfiguration() { EnablePacklineWeightDistribution = true, EnableActualWeightDistribution = true, EnableVolumetricWeightDistribution = true });
			}
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PacklineWeightDistributionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((PacklineWeightDistributionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PacklineWeightDistributionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PacklineWeightDistributionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new PacklineWeightDistributionConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new PacklineWeightDistributionConfiguration() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}
	}
}
