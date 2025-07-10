using System;
using Enterprise.Client.JAS.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JASShipmentForm : ShipmentForm
	{
		public JASShipmentForm(JASForwardingShipment shipment)
			: base(shipment)
		{
			InitializeComponent();
			ZFormMenuStrategy.AddActionsMenuItem(this, ExportPreShipmentMessage, OnJXCMenuItem_Clicked);
		}

		void OnJXCMenuItem_Clicked(object sender, EventArgs e)
		{
			HandleJXCMenuItemClick();
		}

		void HandleJXCMenuItemClick()
		{
			if (Shipment.HasChanges || !Shipment.IsInDatabase)
			{
				Globals.Message.ShowError(MustSaveErrorMessage);
			}
			else if (IsShipmentSuitableForJXC())
			{
				ZFormModaliser.ShowDialogAndDispose(new PreShipmentExporterForm(Shipment));
			}
		}

		bool IsShipmentSuitableForJXC()
		{
			bool result;
			JASForwardingShipment.SuitableForJXC suitableForJXC = Shipment.IsSuitableForJXC();

			if (suitableForJXC == JASForwardingShipment.SuitableForJXC.Suitable)
			{
				result = true;
			}
			else
			{
				result = false;
				string errorMessage;

				if (suitableForJXC == JASForwardingShipment.SuitableForJXC.NotAPreShipment)
				{
					errorMessage = "Shipment is already attached to a consol (not a pre-shipment). Please create message from the consol instead";
				}
				else
				{
					errorMessage = "Could not export JXC message from this Shipment";
				}

				Globals.Message.ShowError(errorMessage);
			}

			return result;
		}

		new JASForwardingShipment Shipment
		{
			get { return (JASForwardingShipment)base.Shipment; }
		}

		const string MustSaveErrorMessage = "You must save before you can export data to JXC file";
		const string ExportPreShipmentMessage = "Export JXC Pre-Shipment Message";
	}
}
