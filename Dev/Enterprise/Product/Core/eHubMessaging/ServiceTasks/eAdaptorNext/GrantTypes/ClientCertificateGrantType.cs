using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public class ClientCertificateGrantType : AuthorizationGrantType
	{
		string ClientId { get; }
		string ClientAssertion { get; }

		//Client Assertion Type Standard [https://www.rfc-editor.org/rfc/rfc7523#section-2.2]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

		public ClientCertificateGrantType(IClientCertificateGrant config) : base(config)
		{
			ClientId = config.ClientID;
			ClientAssertion = JwtHelper.GenerateJWTToken(config.ClientID, config.AuthorizationURL, config.Certificate, config.PrivateKey);
		}

		public override Dictionary<string, string> Content()
		{
			return new Dictionary<string, string> {
				{ OAuth2Parameters.ClientID, ClientId },
				{ OAuth2Parameters.ClientAssertion, ClientAssertion },
				{ OAuth2Parameters.ClientAssertionType, ClientAssertionType }
			};
		}
	}
}
