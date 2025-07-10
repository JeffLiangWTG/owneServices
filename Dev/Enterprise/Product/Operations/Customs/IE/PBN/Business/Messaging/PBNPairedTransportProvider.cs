using CargoWise.Customs.IE.MessageDefinitions.PBN;
using CargoWise.Types;

namespace Enterprise.Customs.IE.PBN.Messaging
{
	public class PBNPairedTransportProvider
	{
		public PBNPairedTransportProvider(PBNPairedTransport pairedTransport)
		{
			this.pairedTransport = pairedTransport;
		}

		public ZString CustomsOffice => pairedTransport.CustomsOffice ?? ZString.Empty;

		public ZString ShipId => pairedTransport.ShipId ?? ZString.Empty;

		public ZString ScheduledTimeofArrival => pairedTransport.ScheduledTimeofArrival ?? ZString.Empty;

		public ZString RegistrationNumber => pairedTransport.RegistrationNumber ?? ZString.Empty;

		readonly PBNPairedTransport pairedTransport;
	}
}
