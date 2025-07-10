using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE813TransportDetailsProvider : IEMCSTransportDetails
	{
		public IE813TransportDetailsProvider(TransportDetailsType transportDetails)
		{
			this.transportDetails = Argument.NotNull(transportDetails, nameof(transportDetails));
		}
		readonly TransportDetailsType transportDetails;

		public ZString UnitCode => transportDetails.TransportUnitCode;
		public ZString IdentityOfUnit => transportDetails.IdentityOfTransportUnits;
		public ZString CommercialSealIdentification => transportDetails.CommercialSealIdentification;
		public ZString ComplementaryInformation => new ZString(transportDetails.ComplementaryInformation?.Value);
		public ZString SealInformation => new ZString(transportDetails.SealInformation?.Value);
	}
}
