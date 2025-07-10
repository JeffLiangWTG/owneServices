using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class ED801TransportDetailsProvider : IEMCSTransportDetails
	{
		public ED801TransportDetailsProvider(ED801EBodyEadContainerTransportDetails transportDetails)
		{
			this.transportDetails = Argument.NotNull(transportDetails, nameof(transportDetails));
		}
		readonly ED801EBodyEadContainerTransportDetails transportDetails;

		public ZString UnitCode => transportDetails.TransportUnitCode;
		public ZString IdentityOfUnit => transportDetails.IdentityOfTransportUnits;
		public ZString CommercialSealIdentification => transportDetails.CommercialSealIdentification;
		public ZString ComplementaryInformation => transportDetails.ComplementaryInformation;
		public ZString SealInformation => transportDetails.SealInformation;
	}
}
