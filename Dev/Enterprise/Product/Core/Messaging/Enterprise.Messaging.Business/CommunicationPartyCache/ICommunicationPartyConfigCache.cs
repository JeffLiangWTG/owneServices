using System;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business
{
	public interface ICommunicationPartyConfigCache
	{
		DateTimeOffset GetExpirationDate();
		bool TryGetInboundCommunicationPartyConfigByClientId(ZString communicationAuthClientID,
			ZString communicationAuthEndpoint,
			string applicationCode,
			out IEDICommunicationPartyConfig communicationPartyConfig);

		bool TryGetInboundCommunicationPartyConfigByUsername(ZString username,
			string applicationCode,
			out IEDICommunicationPartyConfig communicationPartyConfig);

		bool TryGetOutboundCommunicationPartyConfig(ZGuid communicationPartyConfigPk, out IEDICommunicationPartyConfig communicationPartyConfig);
	}
}
