using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public static class HttpErrorHelper
	{
		public static string HandleWebExceptionAndPrepareLogMessage(IUrlProvider cspConnection, Exception ex, McpIslCredentialsSetting credential, ZString messageText)
		{
			var httpStatusUnauthorised = "401";
			var httpStatusObjectNotFound = "404";
			var httpStatusInternalServerError = "500";

			var credentialString = string.Format("Code: {0}. Device: {1}. Username: {2}. Password: {3}. URL: {4}", credential.McpIslCompanyCode, credential.McpIslDevice, credential.McpIslUsername, credential.McpIslPassword, cspConnection.Url);

			var helpExplanation = string.Empty;
			var exceptionMessage = "(no exception raised)";
			if (ex != null)
			{
				if (ex.Message.Contains(httpStatusUnauthorised))
				{
					helpExplanation = "Check that the credentials in your registry are correct for the badge and server in question. ";
				}
				if (ex.Message.Contains(httpStatusObjectNotFound))
				{
					helpExplanation = "Check the URL is correct. Server reported a 'file not found' error.";
				}
				if (ex.Message.Contains(httpStatusInternalServerError))
				{
					helpExplanation = $"The CSP server suffered an internal crash (http 500). This is not an {BrandingFactory.Instance.ProductName} error but it may be transient.  Please report the problem to the CSP helpdesk.";
				}
				exceptionMessage = ex.Message;
			}
			return string.Format("Could not communicate with CSP. Full text: {0}. {1} Credentials are [{2}]. Message starts: {3}", exceptionMessage, helpExplanation, credentialString, messageText.SubstringSafe(0, 150));
		}

		public static string HandleWebExceptionAndPrepareLogMessage(IUrlProvider cspConnection, Exception ex, McpIslCredentialsSetting credential)
		{
			return HandleWebExceptionAndPrepareLogMessage(cspConnection, ex, credential, ZString.Empty);
		}
	}
}
