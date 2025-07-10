using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface ITransportDetails
{
	ZInt TransportUnitCode { get; }

	ZString IdentityOfTransportUnits { get; }

	ZString CommercialSealIdentification { get; }

	ZString ComplementaryInformation { get; }

	ZString ComplementaryInformationLanguage { get; }

	ZString SealInformation { get; }

	ZString SealInformationLanguage { get; }
}
