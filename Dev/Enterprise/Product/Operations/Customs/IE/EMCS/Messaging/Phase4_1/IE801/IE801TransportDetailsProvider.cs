using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE801TransportDetailsProvider : IEMCSTransportDetails
	{
		public IE801TransportDetailsProvider(TransportDetailsType transportDetails)
		{
			this.transportDetails = Argument.NotNull(transportDetails, nameof(transportDetails));
		}
		readonly TransportDetailsType transportDetails;

		public ZString UnitCode => transportDetails.TransportUnitCode;

		public ZString IdentityOfUnit => transportDetails.IdentityOfTransportUnits;

		public ZString CommercialSealIdentification => transportDetails.CommercialSealIdentification;

		public ZString ComplementaryInformation => transportDetails.ComplementaryInformation?.Value;

		public ZString SealInformation => transportDetails.SealInformation?.Value;
	}
}
