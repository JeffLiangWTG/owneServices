using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JASConsolForm : ConsolForm, IJXCExportForm
	{
		public JASConsolForm(JASForwardingConsol consol)
			: base(consol)
		{
			InitializeComponent();
			ZFormMenuStrategy.AddActionsMenuItem(this, JXCAirOceanMenuItemText, OnJXCAirOceanMenuItem_Clicked);
			ZFormMenuStrategy.AddActionsMenuItem(this, JXCAirProfitShareMenuItemText, OnJXCAirProfitShareMenuItem_Clicked);
		}

		public JASForwardingConsol Consol
		{
			get { return (JASForwardingConsol)DataSource; }
		}

		#region Attach / Detach Event Handlers

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Consol != null)
			{
				DetachDocumentSupporterEventHandlers();
			}
			base.SetDataBinding(dataSource, dataMember);
			if (Consol != null)
			{
				AttachDocumentSupporterEventHandlers();
			}
		}

		void AttachDocumentSupporterEventHandlers()
		{
			JASForwardingConsolDocumentSupporter consolDocumentSupporter = (JASForwardingConsolDocumentSupporter)Consol.DocumentSupporter;
			consolDocumentSupporter.GettingDataStateBeforeRun += new CancelEventHandler(ConsolDocumentSupporter_GettingDataStateBeforeRun);
		}

		void DetachDocumentSupporterEventHandlers()
		{
			JASForwardingConsolDocumentSupporter consolDocumentSupporter = (JASForwardingConsolDocumentSupporter)Consol.DocumentSupporter;
			consolDocumentSupporter.GettingDataStateBeforeRun -= new CancelEventHandler(ConsolDocumentSupporter_GettingDataStateBeforeRun);
		}

		#endregion

		#region Event Handlers

		#region Air / Ocean Message

		void OnJXCAirOceanMenuItem_Clicked(object sender, EventArgs e)
		{
			HandleJXCAirOceanMenuItemClick();
		}

		void HandleJXCAirOceanMenuItemClick()
		{
			ExportJXCMessage(AirOceanMessageExporter.New(Consol), EnsureConsolSuitableForJXCAirOrOceanMessage);
		}

		void EnsureConsolSuitableForJXCAirOrOceanMessage(object sender, CancelEventArgs e)
		{
			JASForwardingConsol.SuitableForJXCAirOrOceanMessage suitable = Consol.IsSuitableForJXCAirOrOceanMessage();

			if (suitable != JASForwardingConsol.SuitableForJXCAirOrOceanMessage.Suitable)
			{
				e.Cancel = true;
				string errorMessage;

				if (suitable == JASForwardingConsol.SuitableForJXCAirOrOceanMessage.NoShipmentAttached)
				{
					errorMessage = "Consol does not have any shipments";
				}
				else if (suitable == JASForwardingConsol.SuitableForJXCAirOrOceanMessage.ConsolAndShipmentsNotCompatible)
				{
					errorMessage = "Consol is incompatible for JXC messaging. It contains shipments with different Transport Modes (i.e. Air Shipment and Ocean Shipment)";
				}
				else
				{
					errorMessage = "Could not export JXC message from this Consol";
				}

				Globals.Message.ShowError(errorMessage);
			}
		}

		void ConsolDocumentSupporter_GettingDataStateBeforeRun(object sender, CancelEventArgs args)
		{
			HandleConsolDocumentSupporter_GettingDataStateBeforeRun(args);
		}

		void HandleConsolDocumentSupporter_GettingDataStateBeforeRun(CancelEventArgs args)
		{
			JXCMessageGUIExportDirector exportDirector = GetNewGUIExportDirector(AirOceanMessageExporter.New(Consol), this);
			exportDirector.AdditionalPreExportCheck += EnsureConsolSuitableForJXCAirOrOceanMessage;
			if (!exportDirector.EnsureMessageCanBeExported())
			{
				args.Cancel = true;

				DialogResult queryResult = Globals.Message.Show(QueryControllerUserIfShouldExportJXCAirOceanMessage, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (queryResult == DialogResult.Yes)
				{
					args.Cancel = false;
				}
			}
		}

		#endregion

		#region Air Profit Share Message

		void OnJXCAirProfitShareMenuItem_Clicked(object sender, EventArgs e)
		{
			HandleJXCAirProfitShareMenuItemClick();
		}

		void HandleJXCAirProfitShareMenuItemClick()
		{
			ExportJXCMessage(new ProfitShareMessageExporter(Consol), EnsureConsolSuitableForJXCAirProfitShareMessage);
		}

		void EnsureConsolSuitableForJXCAirProfitShareMessage(object sender, CancelEventArgs e)
		{
			e.Cancel = true;

			if (Consol.Shipments.Count < 1)
			{
				Globals.Message.ShowError("Consol does not have any shipments");
			}
			else if (!Consol.IsAir)
			{
				Globals.Message.ShowError("Consol is not an Air Consol");
			}
			else
			{
				e.Cancel = !EnsureShipmentJobsHaveBeenClosed();
			}
		}

		bool EnsureShipmentJobsHaveBeenClosed()
		{
			bool result = true;

			if (!Consol.AreShipmentJobsClosed())
			{
				DialogResult queryResult = Globals.Message.Show(QueryUserIfShouldExportJXCProfitShareMessage, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = (queryResult == DialogResult.Yes);
			}

			return result;
		}

		#endregion

		void ExportJXCMessage(JXCMessageExporter exporter, CancelEventHandler ensureCanExportJXCMethod)
		{
			JXCMessageGUIExportDirector exportDirector = GetNewGUIExportDirector(exporter, this);
			exportDirector.AdditionalPreExportCheck += ensureCanExportJXCMethod;
			if (!exportDirector.EnsureMessageCanBeExported())
			{
				DialogResult queryResult = Globals.Message.Show(QueryControllerUserIfShouldExportJXCAirOceanMessage, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (queryResult == DialogResult.Yes)
				{
					exportDirector.Export();
				}
			}
			else
			{
				exportDirector.Export();
			}
		}

		protected virtual
 JXCMessageGUIExportDirector GetNewGUIExportDirector(JXCMessageExporter exporter, IJXCExportForm form)
		{
			return new JXCMessageGUIExportDirector(exporter, form);
		}

		#endregion

		#region IJXCExportForm Members

		void IJXCExportForm.ValidateAll()
		{
			if (Consol.TransportMode == Core.Constants.TransportModes.Air)
			{
				Consol.RegisterEditableChildObject(Consol.AWBHeader);

				foreach (Freight.Forwarding.AWB.Business.IExportAWBRateLine line in Consol.AWBHeader.AWBRateLines)
				{
					Consol.RegisterEditableChildObject(line.NatureAndQtyOfGoods);
				}

				foreach (Freight.Forwarding.Business.ForwardingShipment shipment in Consol.Shipments)
				{
					shipment.RegisterEditableChildObject(shipment.AWBHeader);

					foreach (Freight.Forwarding.AWB.Business.IExportAWBRateLine line in shipment.AWBHeader.AWBRateLines)
					{
						shipment.RegisterEditableChildObject(line.NatureAndQtyOfGoods);
					}
				}
			}

			ValidateAll(ValidationType.Full);
		}

		BusinessObject IJXCExportForm.BusinessEntity
		{
			get { return BusinessEntity as BusinessObject; }
		}

		#endregion

		const string JXCAirOceanMenuItemText = "Export JXC Air/Ocean Message(s)";
		const string JXCAirProfitShareMenuItemText = "Export JXC Air Profit Share Message";
		const string QueryControllerUserIfShouldExportJXCAirOceanMessage = "Do you want to continue anyway? (The message will most likely be rejected by JASWW JXC Validator)";
		const string QueryUserIfShouldExportJXCProfitShareMessage = "There are Shipment Jobs which have not been closed and/or there are shipments without invoices.\r\nDo you want to continue anyway? (The message might contain incorrect figures)";
	}
}
