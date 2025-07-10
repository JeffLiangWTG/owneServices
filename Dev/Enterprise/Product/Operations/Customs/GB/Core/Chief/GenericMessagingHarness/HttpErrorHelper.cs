using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.Chief
{
	public static class HttpErrorHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public static string HandleWebExceptionAndPrepareLogMessage(IUrlProvider cspConnection, Exception ex, CredentialsSetting credential, ZString messageText)
		{
			string httpStatusUnauthorised = "401";
			string httpStatusObjectNotFound = "404";
			string httpStatusInternalServerError = "500";

			var credentialString = string.Format("Badge: {0}/{1}. Device: {2}. Username: {3}. Password: {4}. URL: {5}", credential.BadgeCode, credential.Company, credential.Printer, credential.Username, credential.Password, cspConnection.Url);

			string helpExplanation = string.Empty;
			string exceptionMessage = "(no exception raised)";
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
					helpExplanation = string.Format("The CSP server suffered an internal crash (http 500). This is not an {0} error but it may be transient.  Please report the problem to the CSP helpdesk.", BrandingFactory.Instance.ProductName);
				}
				exceptionMessage = ex.Message;
			}
			return string.Format("Could not communicate with CSP. Full text: {0}. {1} Credentials are [{2}]. Message starts: {3}", exceptionMessage, helpExplanation, credentialString, messageText.SubstringSafe(0, 150));
		}

		public static string HandleWebExceptionAndPrepareLogMessage(IUrlProvider cspConnection, Exception ex, CredentialsSetting credential)
		{
			return HandleWebExceptionAndPrepareLogMessage(cspConnection, ex, credential, ZString.Empty);
		}
	}
}
