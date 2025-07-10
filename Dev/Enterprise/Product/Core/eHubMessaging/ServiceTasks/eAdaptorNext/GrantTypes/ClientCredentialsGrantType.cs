using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public class ClientCredentialsGrantType : AuthorizationGrantType
	{
		public string ClientId { get; }
		public string ClientSecret { get; }

		public ClientCredentialsGrantType(IClientCredentialsGrant config) : base(config)
		{
			ClientId = config.ClientID;
			ClientSecret = config.ClientSecret;
		}

		public override Dictionary<string, string> Content()
		{
			return new Dictionary<string, string> {
				{ OAuth2Parameters.ClientID, ClientId },
				{ OAuth2Parameters.ClientSecret, ClientSecret }
			};
		}
	}
}
