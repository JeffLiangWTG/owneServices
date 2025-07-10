using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.GUI
{
	public class CachedOutboundOAuthConfig
	{
		public CachedOutboundOAuthConfig(EDICommunicationPartyConfig config)
		{
			var auth = config.Auth;
			FlowCode = auth.ECA_FlowCode;
			AuthorizationURL = auth.AuthorizationURL;
			ClientID = auth.ClientID;
			ClientSecret = auth.ClientSecret;
			Username = auth.Username;
			Password = auth.Password;
			Scopes = auth.Scopes;
		}

		public bool IsCached(EDICommunicationPartyConfig config)
		{
			var auth = config.Auth;
			return auth.ECA_FlowCode == FlowCode
				&& auth.AuthorizationURL.Equals(AuthorizationURL)
				&& auth.ClientID.Equals(ClientID)
				&& auth.ClientSecret.Equals(ClientSecret)
				&& auth.Username.Equals(Username)
				&& auth.Password.Equals(Password)
				&& (auth.Scopes.Count() == Scopes.Count() && (!auth.Scopes.Except(Scopes).Any() || !Scopes.Except(auth.Scopes).Any()));
		}

		readonly string FlowCode;
		readonly string AuthorizationURL;
		readonly string ClientID;
		readonly string ClientSecret;
		readonly string Username;
		readonly string Password;
		readonly IEnumerable<string> Scopes;
	}
}
