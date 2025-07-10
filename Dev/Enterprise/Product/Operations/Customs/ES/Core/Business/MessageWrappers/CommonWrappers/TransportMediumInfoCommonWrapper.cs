using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class TransportMediumInfoCommonWrapper : ITransportMediumInfoCommon
{
	public TransportMediumInfoCommonWrapper(ZString transportMode, ZString transportId, ZString transportNationality, JobDeclaration declaration = null)
	{
		TransportMode = transportMode;
		TransportId = transportId;
		TransportNationality = declaration?.GetDefaultTerritory(transportNationality) ?? transportNationality;
	}

	public ZString TransportMode { get; }

	public ZString TransportId { get; }

	public ZString TransportNationality { get; }
}
