using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Messaging.Business
{
	public class ImmutableCommunicationAuth : IEDIClientAuth
	{
		public ImmutableCommunicationAuth(
			ZGuid pk,
			string authorizationURL,
			string authorizationMode,
			string ecaClientId,
			string ecaClientSecret,
			string flowCode,
			string ecaPassword,
			IEnumerable<string> ecaScopes,
			string ecaUsername,
			string certificate,
			string privateKey)
		{
			PK = pk;
			AuthorizationURL = authorizationURL;
			AuthorizationMode = authorizationMode;
			ClientID = ecaClientId;
			ClientSecret = ecaClientSecret;
			FlowCode = flowCode;
			Password = ecaPassword;
			Scopes = ecaScopes;
			Username = ecaUsername;
			Certificate = certificate;
			PrivateKey = privateKey;
		}

		public static ImmutableCommunicationAuth FromFactoryObject(IEDIClientAuth communicationAuth)
		{
			if (communicationAuth == null)
			{
				return null;
			}
			return new ImmutableCommunicationAuth(
				communicationAuth.PK,
				communicationAuth.AuthorizationURL,
				communicationAuth.AuthorizationMode,
				communicationAuth.ClientID,
				communicationAuth.ClientSecret,
				communicationAuth.FlowCode,
				communicationAuth.Password,
				communicationAuth.Scopes,
				communicationAuth.Username,
				communicationAuth.Certificate,
				communicationAuth.PrivateKey
			);
		}

		public string CachingKey => PK.ToString();

		public ZGuid PK { get; }

		public string FlowCode { get; }

		public string AuthorizationURL { get; }

		public string ClientID { get; }

		public string ClientSecret { get; }

		public string Username { get; }

		public string Password { get; }

		public IEnumerable<string> Scopes { get; }

		public string PrivateKey { get; }

		public string Certificate { get; }

		public string AuthorizationMode { get; }
	}
}
