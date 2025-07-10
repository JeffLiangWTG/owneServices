using System;
using System.Collections.Generic;
using Enterprise.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.MailProcessor;

public class OMSSpecificValidation : IServiceTaskSpecificValidation
{
	public OMSSpecificValidation(int secondaryProcessesMaxCount)
	{
		this.secondaryProcessesMaxCount = secondaryProcessesMaxCount;
	}

	bool UsingGraphApiOrExchangeOnlineServer()
	{
		return Env.Registry.UseGraphApiForOutgoing
			|| Env.Registry.SMTPServer.Equals(Office360SMTP, StringComparison.OrdinalIgnoreCase)
			|| Env.Registry.SMTPServer.Equals(OutlookSMTP, StringComparison.OrdinalIgnoreCase);
	}

	public ValidationResult Validate()
	{
		var propertySpecificWarnings = new Dictionary<string, string>();
		if (secondaryProcessesMaxCount > MaxSecondaryProcessesMaxCountForOMSIfOAuth2 && UsingGraphApiOrExchangeOnlineServer())
		{
			var warningMessage = ResString.GetMultilingualString(
				"D159C34A-A9EB-48BE-BE5F-3E0549E339F6",
				"Increasing the number of secondary processes will result in concurrent connections to the mail server which can lead to the {0} server imposing a throttle limit for excessive concurrent connections. ",
				$"Exchange Online/{Office360SMTP}/{OutlookSMTP}");

			propertySpecificWarnings.Add("SecondaryProcessesMaxCountInfo", warningMessage + ResString.GetMultilingualString("9CD02AB7-DD91-4940-A7A7-9E46D6E0A972", "Please refer to {0} for details.", WarningLink));
			propertySpecificWarnings.Add("ExtendedConfigProcessesMaxCountWarningMessage", warningMessage + ResString.GetMultilingualString("CA8EEBC2-4B9F-4D37-AC0E-0639915F6892", "Please refer to the following for details:"));
			propertySpecificWarnings.Add("ExtendedConfigProcessesMaxCountWarningLink", WarningLink);
		}

		return new ValidationResult() { PropertySpecificWarnings = propertySpecificWarnings };
	}

	readonly int secondaryProcessesMaxCount;

	const string Office360SMTP = "smtp.office365.com";
	const string OutlookSMTP = "smtp-mail.outlook.com";
	const int MaxSecondaryProcessesMaxCountForOMSIfOAuth2 = 3;
	const string WarningLink = "https://learn.microsoft.com/en-us/exchange/troubleshoot/send-emails/smtp-submission-improvements#new-throttling-limit-for-concurrent-connections-that-submitmessages";
}
