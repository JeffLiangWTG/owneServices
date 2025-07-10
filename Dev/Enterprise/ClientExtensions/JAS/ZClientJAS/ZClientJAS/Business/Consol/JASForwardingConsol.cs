using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business
{
	[Enterprise.Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.NotApplied, "Enterprise.Client.JAS.Metadata.JASForwardingConsol, ZClientJAS, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350")]
	public class JASForwardingConsol : ForwardingConsol, IJXCExportHeader, IJXCImportHeader, ITransportParent
	{
		public JASForwardingConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void UpdateAWBPrintedCore()
		{
			base.UpdateAWBPrintedCore();
			ExportJXCAirOrOceanMessage();
		}

		public bool AreShipmentJobsClosed()
		{
			bool result = false;

			if (Shipments.Count > 0)
			{
				result = true;

				foreach (JASForwardingShipment shipment in Shipments)
				{
					if (shipment.Job == null || (shipment.Job != null && !shipment.Job.IsClosed))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#region IsSuitableForJXCAirOrOceanMessage

		public enum SuitableForJXCAirOrOceanMessage
		{
			NoShipmentAttached,
			ConsolAndShipmentsNotCompatible,
			InvalidConsolTransportMode,
			Suitable
		}

		public SuitableForJXCAirOrOceanMessage IsSuitableForJXCAirOrOceanMessage()
		{
			SuitableForJXCAirOrOceanMessage result;

			if (Shipments.Count < 1)
			{
				result = SuitableForJXCAirOrOceanMessage.NoShipmentAttached;
			}
			else
			{
				switch (JK_TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						result = AreConsolShipmentsAirShipments();
						break;

					case Core.Constants.TransportModes.Sea:
						result = AreConsolShipmentsSeaShipments();
						break;

					default:
						result = SuitableForJXCAirOrOceanMessage.InvalidConsolTransportMode;
						break;
				}
			}

			return result;
		}

		SuitableForJXCAirOrOceanMessage AreConsolShipmentsAirShipments()
		{
			foreach (JASForwardingShipment shipment in Shipments)
			{
				if (!shipment.IsAir)
				{
					return SuitableForJXCAirOrOceanMessage.ConsolAndShipmentsNotCompatible;
				}
			}

			return SuitableForJXCAirOrOceanMessage.Suitable;
		}

		SuitableForJXCAirOrOceanMessage AreConsolShipmentsSeaShipments()
		{
			foreach (JASForwardingShipment shipment in Shipments)
			{
				if (!shipment.IsSea)
				{
					return SuitableForJXCAirOrOceanMessage.ConsolAndShipmentsNotCompatible;
				}
			}

			return SuitableForJXCAirOrOceanMessage.Suitable;
		}

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = base.HumanReadableNameCore;

				if (JK_MasterBillNum.IsEmpty && !JK_AgentsReference.IsEmpty)
				{
					result += " (Agent Ref='" + JK_AgentsReference + "')";
				}

				return result;
			}
		}

		public override DocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					fDocumentSupporter = new JASForwardingConsolDocumentSupporter(this);
				}
				return fDocumentSupporter;
			}
		}

		DocumentSupporter fDocumentSupporter;

		#endregion

		#region Related Business Objects

		public new JASOrgHeader SendingForwarder
		{
			get { return (JASOrgHeader)base.SendingForwarder; }
		}

		public new JASOrgHeader ReceivingForwarder
		{
			get { return (JASOrgHeader)base.ReceivingForwarder; }
		}

		public new JASOrgHeader ShippingLine
		{
			get { return (JASOrgHeader)base.ShippingLine; }
		}

		protected override ConsolShipmentCollection GetNewConsolShipmentCollection()
		{
			return new JASForwardingConsolShipmentCollection(this);
		}

		#endregion

		#region ITransportParent Members

		TransportSupporter ITransportParent.TransportSupporter
		{
			get { return new JASForwardingConsolTransportSupporter(this); }
		}

		#endregion

		#region IJXCExportHeader Members

		public ZString SendingOfficeCode
		{
			get { return (SendingForwarder != null) ? SendingForwarder.CustomsCodes.GetUOC() : ZString.Empty; }
		}

		public ZString DestOfficeCode
		{
			get { return (ReceivingForwarder != null) ? ReceivingForwarder.CustomsCodes.GetUOC() : ZString.Empty; }
		}

		public ZString SendingNettingCode
		{
			get { return (SendingForwarder != null) ? SendingForwarder.CustomsCodes.GetUNC() : ZString.Empty; }
		}

		public ZString DestNettingCode
		{
			get { return (ReceivingForwarder != null) ? ReceivingForwarder.CustomsCodes.GetUNC() : ZString.Empty; }
		}

		public ZString FreightDest
		{
			get { return Transports.ArrivalTransport.JW_RL_NKDiscPort; }
		}

		virtual
 public void ExportJXCAirOrOceanMessage()
		{
			AirOceanMessageExporter exporter = AirOceanMessageExporter.New(this);
			exporter.WriteToFile();
		}

		#endregion

		#region IJXCImportHeader

		public void SetDestinationForwarder(ZString destOfficeCode, ZString destNettingCode)
		{
			JASOrgHeader jASOrgHeader = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, destOfficeCode, destNettingCode);
			if (jASOrgHeader != null)
			{
				SetDefaultReceivingForwarderAddress(jASOrgHeader);
			}
		}

		public void SetSendingForwarder(ZString sendingOfficeCode, ZString sendingNettingCode)
		{
			JASOrgHeader jASOrgHeader = JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(Factory, sendingOfficeCode, sendingNettingCode);
			if (jASOrgHeader != null)
			{
				SetDefaultSendingForwarderAddress(jASOrgHeader);
			}
		}

		public void SetFreightDestination(ZString freightDest)
		{
			JK_RL_NKDischargePort = freightDest.Left(JK_RL_NKDischargePortInfo.MaxLength);
		}

		#endregion
	}
}
