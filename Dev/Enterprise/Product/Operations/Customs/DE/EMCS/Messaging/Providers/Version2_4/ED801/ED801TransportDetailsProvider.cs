using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED801TransportDetailsProvider : IEMCSTransportDetails
	{
		public ED801TransportDetailsProvider(ED801DBodyEadContainerTransportDetails transportDetails)
		{
			this.transportDetails = Argument.NotNull(transportDetails, nameof(transportDetails));
		}
		readonly ED801DBodyEadContainerTransportDetails transportDetails;

		public ZString UnitCode => transportDetails.TransportUnitCode;
		public ZString IdentityOfUnit => transportDetails.IdentityOfTransportUnits;
		public ZString CommercialSealIdentification => transportDetails.CommercialSealIdentification;
		public ZString ComplementaryInformation => transportDetails.ComplementaryInformation;
		public ZString SealInformation => transportDetails.SealInformation;
	}
}
