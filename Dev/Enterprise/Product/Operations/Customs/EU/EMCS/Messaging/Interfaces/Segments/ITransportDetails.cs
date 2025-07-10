using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface ITransportDetails
	{
		ZString TransportUnitCode { get; set; }

		ZString IdentityOfTransportUnits { get; set; }

		ZString CommercialSealIdentification { get; set; }

		ZString ComplementaryInformation { get; set; }

		ZString SealInformation { get; set; }
	}
}
