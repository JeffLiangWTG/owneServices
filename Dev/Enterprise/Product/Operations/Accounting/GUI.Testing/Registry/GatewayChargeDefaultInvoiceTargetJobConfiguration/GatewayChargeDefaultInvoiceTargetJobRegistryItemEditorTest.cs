using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(GatewayChargeDefaultInvoiceTargetJobRegistryItemEditor))]
	public class GatewayChargeDefaultInvoiceTargetJobRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new GatewayChargeDefaultInvoiceTargetJobRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GatewayChargeDefaultInvoiceTargetJobConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GatewayChargeDefaultInvoiceTargetJobConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GatewayChargeDefaultInvoiceTargetJobConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			var configuration = collection.AddNew();
			configuration.ConsolDirection = "ALL";
			configuration.ConsolTransportMode = "ALL";
			configuration.PreviousSendingAgentType = "ALL";
			configuration.InvoiceTargetJobType = "SCL";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
