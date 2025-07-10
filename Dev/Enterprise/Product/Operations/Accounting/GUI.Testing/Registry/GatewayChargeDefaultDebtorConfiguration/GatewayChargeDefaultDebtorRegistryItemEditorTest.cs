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
	[TestedType(typeof(GatewayChargeDefaultDebtorRegistryItemEditor))]
	public class GatewayChargeDefaultDebtorRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new GatewayChargeDefaultDebtorRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((GatewayChargeDefaultDebtorConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(GatewayChargeDefaultDebtorConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GatewayChargeDefaultDebtorConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			var configuration = collection.AddNew();
			configuration.ConsolDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.ConsolTransportMode = Core.Constants.TransportModes.Air;
			configuration.ChargeGroup = "ALL";
			configuration.ConsolPaymentTerm = Core.Constants.PaymentType.Prepaid;
			configuration.RelatedJob = Core.Constants.GatewayRelatedJob.Codes.RelatedToShipment;
			configuration.PreviousSendingAgent = Core.Constants.GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent;
			configuration.Debtor = Core.Constants.GatewayDebtor.Codes.SendingAgent;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
