using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE813TransportDetailsProvider : IEMCSTransportDetails
	{
		public IE813TransportDetailsProvider(TransportDetailsType transportDetails)
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
