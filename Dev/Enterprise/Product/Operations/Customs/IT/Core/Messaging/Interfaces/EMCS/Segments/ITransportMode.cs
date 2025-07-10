using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public interface ITransportMode
{
	ZInt TransportModeCode { get; }
	ZString ComplementaryInformation { get; }
	ZString ComplementaryInformationLanguage { get; }
}
