using WTG.OpenIDConnect.Login;

namespace Enterprise.ZArchitecture.Core
{
	public static class OIDCLoginServerResponseCreator
	{
		public static OIDCLoginResponseMessage CreateFailedResponse(OIDCLoginRequestMessage request, OIDCLoginResponseMessage errorResponse)
		{
			switch (errorResponse.Error)
			{
				case OIDCLoginResponseMessage.ErrorType.Timeout:
					return OIDCLoginResponseMessage.CreateFailedResponse(
						OIDCLoginResponseMessage.ErrorType.Timeout,
						SourceGenerated.ResString.GetMultilingualString("2E6BDB36-DB0A-4E12-9FC8-1E37E68EAC88", "Login timed out"),
						SourceGenerated.ResString.GetMultilingualString("DEC2D73E-D4A4-4B65-A843-AA626363F36E", "Login timed out. Response took longer than {0} seconds.", request.TimeoutSeconds));
				case OIDCLoginResponseMessage.ErrorType.LoginFailed:
					return OIDCLoginResponseMessage.CreateFailedResponse(
						OIDCLoginResponseMessage.ErrorType.LoginFailed,
						SourceGenerated.ResString.GetMultilingualString("98517C94-8D24-4C05-979F-9EDC514867FE", "Failed Login"),
						errorResponse.ErrorDescription);
				case OIDCLoginResponseMessage.ErrorType.OperationCanceled:
					return CreateFailedResponse(OIDCLoginResponseMessage.ErrorType.OperationCanceled);
				case OIDCLoginResponseMessage.ErrorType.Exception:
					return OIDCLoginResponseMessage.CreateFailedResponse(
						OIDCLoginResponseMessage.ErrorType.Exception,
						SourceGenerated.ResString.GetMultilingualString("5ECB7D26-CA82-48FB-8B58-586786B112F7", "Exception during login processing"),
						errorResponse.ErrorString + "\r\n" + errorResponse.ErrorDescription);
				default:
					return errorResponse;
			}
		}

		public static OIDCLoginResponseMessage CreateFailedResponse(OIDCLoginResponseMessage.ErrorType errorType)
		{
			switch (errorType)
			{
				case OIDCLoginResponseMessage.ErrorType.OperationCanceled:
					return OIDCLoginResponseMessage.CreateFailedResponse(
					OIDCLoginResponseMessage.ErrorType.OperationCanceled,
					SourceGenerated.ResString.GetMultilingualString("56B46C0B-EAA0-4047-853E-48E5681846E0", "Login canceled"),
					SourceGenerated.ResString.GetMultilingualString("BD69B9C4-F928-43FC-B84B-A06AB5D50E77", "Login request was canceled"));
				default:
					return OIDCLoginResponseMessage.CreateFailedResponse(
				OIDCLoginResponseMessage.ErrorType.LoginFailed,
				SourceGenerated.ResString.GetMultilingualString("98517C94-8D24-4C05-979F-9EDC514867FE", "Failed Login"),
				SourceGenerated.ResString.GetMultilingualString("3C4B27BD-811C-45DA-8C22-4213F4B78C1F", "Login request did not complete"));
			}
		}
	}
}
