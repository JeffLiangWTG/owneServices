using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public abstract class AuthorizationGrantType
	{
		public static AuthorizationGrantType Create(IOAuth2Parameters config)
		{
			switch (config.FlowCode)
			{
				case EDICommunicationAuthOutboundGrantTypesList.Codes.Password:
					return new PasswordGrantType(config);
				case EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials:
					return new ClientCredentialsGrantType(config);
				case EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate:
					return new ClientCertificateGrantType(config);
				default:
					throw new ArgumentException("An unknown FlowCode was provided.");
			}
		}

		public string AuthorizationURL { get; }
		string GrantType { get; }
		string Scopes { get;  }

		internal AuthorizationGrantType(ICommonOAuth2Parameters parameters)
		{
			GrantType = OAuth2Parameters.FlowCodeToGrantType(parameters.FlowCode);
			AuthorizationURL = parameters.AuthorizationURL;
			Scopes = OAuth2Parameters.ScopesToString(parameters.Scopes);
		}

		public abstract Dictionary<string, string> Content();

		public Dictionary<string, string> GenerateContent()
		{
			var content = Content();
			content.Add(OAuth2Parameters.GrantType, GrantType);
			content.Add(OAuth2Parameters.Scope, Scopes);
			return content;
		}
	}
}
