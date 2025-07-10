using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class OAuth2Parameters
	{
		#region OAuth2.0 parameters

		public const string GrantType = "grant_type";
		public const string ClientID = "client_id";
		public const string ClientSecret = "client_secret";
		public const string RefreshToken = "refresh_token";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string Scope = "scope";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string Code = "code";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string RedirectURI = "redirect_uri";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string Username = "username";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string Password = "password";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string ClientAssertionType = "client_assertion_type";
		public const string ClientAssertion = "client_assertion";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		public const string Assertion = "assertion";
		public const string CodeVerifier = "code_verifier";

		#endregion

		#region OAuth20AuthorizationGrantTypes

		public static class GrantTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
			public const string PasswordGrantType = "password";
			public const string ClientCredentialsGrantType = "client_credentials";
			public const string RefreshTokenGrantType = "refresh_token";
		}

		public static string FlowCodeToGrantType(string flowCode)
		{
			switch (flowCode)
			{
				case EDICommunicationAuthOutboundGrantTypesList.Codes.Password:
					return GrantTypes.PasswordGrantType;
				case EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCredentials:
					return GrantTypes.ClientCredentialsGrantType;
				case EDICommunicationAuthOutboundGrantTypesList.Codes.ClientCertificate:
					return GrantTypes.ClientCredentialsGrantType;
				default:
					throw new ArgumentException("An unknown FlowCode was provided.");
			}
		}

		#endregion

		public static string ScopesToString(IEnumerable<string> scopes)
		{
			return string.Join(",", scopes);
		}
	}
}
