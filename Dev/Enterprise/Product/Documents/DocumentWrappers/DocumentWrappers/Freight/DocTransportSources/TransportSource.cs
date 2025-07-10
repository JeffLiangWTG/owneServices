using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.Freight
{
	public class TransportSource : ITransportDetails
	{
		public TransportSource(Transport transport)
		{
			this.transport = transport;
		}

		#region ITransportDetails Members

		public ZString ParentDescription
		{
			get { return transport.JW_ParentDescription; }
		}

		public ZString TransportMode
		{
			get { return transport.JW_TransportMode; }
		}

		public ZString TransportType
		{
			get { return transport.JW_TransportType; }
		}

		public ZString TransportTypeDescription
		{
			get { return transport.JW_TransportType_List.GetDescriptionFromCode(TransportType); }
		}

		public ZString Vessel
		{
			get { return transport.JW_Vessel; }
		}

		public ZString VoyageFlight
		{
			get { return transport.JW_VoyageFlight; }
		}

		public ZString Load
		{
			get { return transport.JW_RL_NKLoadPort; }
		}

		public ZString Discharge
		{
			get { return transport.JW_RL_NKDiscPort; }
		}

		public ZByte LegOrder
		{
			get { return transport.JW_LegOrder; }
		}

		public ZDateTime ETD
		{
			get { return transport.JW_ETD; }
		}

		public ZDateTime ETA
		{
			get { return transport.JW_ETA; }
		}

		public ZDateTime ATD
		{
			get { return transport.JW_ATD; }
		}

		public ZDateTime ATA
		{
			get { return transport.JW_ATA; }
		}

		public ZDateTime LCLReceivalCommences
		{
			get { return transport.JW_DepotReceivalCommences; }
		}

		public ZDateTime LCLCutOff
		{
			get { return transport.JW_DepotCutOff; }
		}

		public ZDateTime LCLAvailabilityDate
		{
			get { return transport.JW_DepotAvailabilityDate; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return transport.JW_DepotStorageDate; }
		}

		public ZDateTime FCLReceivalCommences
		{
			get { return transport.JW_TerminalReceivalCommences; }
		}

		public ZDateTime FCLCutOff
		{
			get { return transport.JW_TerminalCutOff; }
		}

		public ZDateTime FCLAvailabilityDate
		{
			get { return transport.JW_TerminalAvailabilityDate; }
		}

		public ZDateTime FCLStorageDate
		{
			get { return transport.JW_TerminalStorageDate; }
		}

		public ZGuid Carrier
		{
			get { return transport.CarrierPK; }
		}

		IFlightDetailsSuppression ITransportDetails.SuppressingBizO
		{
			get { return transport; }
		}

		#endregion

		readonly Transport transport;
	}
}
