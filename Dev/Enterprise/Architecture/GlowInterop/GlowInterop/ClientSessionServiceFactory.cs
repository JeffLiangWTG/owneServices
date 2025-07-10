using CargoWise.Authentication.Glow.Client;
using WTG.Foundation.Http;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface IClientSessionServiceFactory
	{
		IClientSessionService Create(IHttpClientFactory clientFactory, IUserSession userSession, ISessionInfo sessionInfo);
	}

	class ClientSessionServiceFactory : IClientSessionServiceFactory
	{
		public IClientSessionService Create(IHttpClientFactory clientFactory, IUserSession userSession, ISessionInfo sessionInfo)
		{
			return new ClientSessionService(clientFactory, userSession, sessionInfo);
		}
	}
}
