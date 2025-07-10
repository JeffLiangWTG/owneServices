using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.Common
{
	[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "URL")]
	public static class SecureURLHashProvider
	{
		public static string GetSecurityHash(NameValueCollection queryItems, params string[] keysToUseForHash)
		{
			Argument.NotNull(keysToUseForHash, nameof(keysToUseForHash)); // Suggested By ReviewBot
			if (!(0 >= keysToUseForHash.Length || queryItems != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(keysToUseForHash));
			}

			var cryptoProvider = TripleDES.Create();
			cryptoProvider.InitializeForCargoWise();

			string securedDataAsString = CreateDataSecuredBySecurityHash(queryItems, keysToUseForHash);
			byte[] securityHash = cryptoProvider.CreateSecurityHash(Encoding.ASCII.GetBytes(securedDataAsString));

			// IE decodes the URL before it gets to the application. Put a character that will be url encoded so we can detect this condition.
			return "+" + Convert.ToBase64String(securityHash);
		}

		public static string CreateDataSecuredBySecurityHash(NameValueCollection queryItems, params string[] keysToUseForHash)
		{
			Argument.NotNull(keysToUseForHash, nameof(keysToUseForHash)); // Suggested By ReviewBot
			if (!(0 >= keysToUseForHash.Length || queryItems != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(keysToUseForHash));
			}

			string result = "";
			foreach (string name in keysToUseForHash)
			{
				result += name + "=" + queryItems[name] + "&";
			}
			return result;
		}
	}
}
