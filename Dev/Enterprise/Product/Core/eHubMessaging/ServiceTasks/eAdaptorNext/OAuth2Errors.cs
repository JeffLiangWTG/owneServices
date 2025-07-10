using System;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class OAuth2Errors
	{
		public static class Errors
		{
			public const string InvalidRequest = "invalid_request";
			public const string UnathorizedClient = "unauthorized_client";
			public const string UnsupportedResponseType = "unsupported_response_type";
			public const string InvalidScope = "invalid_scope";
			public const string ServerError = "server_error";
			public const string TemporarilyUnavailable = "temporarily_unavailable";
		}

		public static OAuth2ErrorTypes StringToEnum(string code)
		{
			switch (code)
			{
				case Errors.InvalidRequest:
					return OAuth2ErrorTypes.InvalidRequest;
				case Errors.UnathorizedClient:
					return OAuth2ErrorTypes.UnathorizedClient;
				case Errors.UnsupportedResponseType:
					return OAuth2ErrorTypes.UnsupportedResponseType;
				case Errors.InvalidScope:
					return OAuth2ErrorTypes.InvalidScope;
				case Errors.ServerError:
					return OAuth2ErrorTypes.ServerError;
				case Errors.TemporarilyUnavailable:
					return OAuth2ErrorTypes.TemporarilyUnavailable;
				default:
					return OAuth2ErrorTypes.InvalidError;
			}
		}

		public static string EnumToString(OAuth2ErrorTypes error)
		{
			switch (error)
			{
				case OAuth2ErrorTypes.InvalidRequest:
					return Errors.InvalidRequest;
				case OAuth2ErrorTypes.UnathorizedClient:
					return Errors.UnathorizedClient;
				case OAuth2ErrorTypes.UnsupportedResponseType:
					return Errors.UnsupportedResponseType;
				case OAuth2ErrorTypes.InvalidScope:
					return Errors.InvalidScope;
				case OAuth2ErrorTypes.ServerError:
					return Errors.ServerError;
				case OAuth2ErrorTypes.TemporarilyUnavailable:
					return Errors.TemporarilyUnavailable;
				default:
					throw new ArgumentException("Invalid Enum provided.");
			}
		}
	}
}
