using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.Tests
{
	public sealed class OAuth20ConfigForTest : IOAuth2Parameters
	{
		public OAuth20ConfigForTest() : this("default")
		{
		}

		public OAuth20ConfigForTest(string cachingKey)
		{
			CachingKey = cachingKey;
			Scopes = new List<string>();
		}

		public string CachingKey { get; set; }

		public string AuthorizationURL { get; set; }

		public string ClientID { get; set; }

		public string ClientSecret { get; set; }

		public string Username { get; set; }

		public string Password { get; set; }

		public string RefreshToken { get; set; }

		public IEnumerable<string> Scopes { get; set; }

		public string FlowCode { get; set; }

		public string PrivateKey { get; set; }

		public string Certificate { get; set; }
	}
}
