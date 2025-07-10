using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public class PasswordGrantType : AuthorizationGrantType
	{
		string ClientId { get; }
		string ClientSecret { get; }
		string Username { get; }
		string Password { get; }

		public PasswordGrantType(IPasswordGrant config) : base(config)
		{
			ClientId = config.ClientID;
			ClientSecret = config.ClientSecret;
			Username = config.Username;
			Password = config.Password;
		}

		public override Dictionary<string, string> Content()
		{
			return new Dictionary<string, string> {
				{ OAuth2Parameters.ClientID, ClientId },
				{ OAuth2Parameters.ClientSecret, ClientSecret },
				{ OAuth2Parameters.Username, Username },
				{ OAuth2Parameters.Password, Password },
			};
		}
	}
}
