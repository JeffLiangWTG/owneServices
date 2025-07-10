using System;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using BaseEDIInterchange = Enterprise.Messaging.Business.EDIInterchange;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

[assembly: UniversalCustomsMessageCFGProcessor(ApplicationCodeList.Codes.JPCustoms, typeof(Enterprise.Customs.JP.Common.JPCredentialChangeCustomsMessageProcessor))]

namespace Enterprise.Customs.JP.Common;

public sealed class JPCredentialChangeCustomsMessageProcessor : BaseConfigurationMessageProcessor<CredentialChangesRequest, UniversalEventWrapper>
{
	protected override bool IsValidMessageCore(BaseEDIInterchange outgoingInterchange, CredentialChangesRequest requestMessage, BaseEDIMessage message)
	{
		var originalCredentialMessage = requestMessage.Request;
		return !JPRegistry.Instance.IsMailboxAndRemoteWebPrintClientCredentialsEmpty && originalCredentialMessage?.Password == MailboxAndRemoteWebPrintClientCredentialsRegistryItem.GetApplicationNodePassword(JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value);
	}

	protected override void ProcessMessageCore(BaseEDIMessage message, UniversalEventWrapper responseMessage, ILoggingInformation logger)
	{
		var currentValue = JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value;
		var group = currentValue.NotificationGroup;

		if (responseMessage.IsAcknowledgement)
		{
			currentValue.Status = XtCredentialStatusList.Codes.Registered;
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentValue);
		}
		else
		{
			currentValue.Status = XtCredentialStatusList.Codes.Error;
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentValue);
		}

		if (group != null)
		{
			SendEmail(message, logger, group, responseMessage.IsAcknowledgement);
		}
	}

	void SendEmail(BaseEDIMessage message, ILoggingInformation logger, GlbGroup group, bool isSuccessful)
	{
		var subject = isSuccessful
			? Res.GetString("EFD05EB0-663C-4714-86B7-5607BA73AC3B", "WebPrint endpoint for NACCS integration has been set up successfully")
			: Res.GetString("F0BC7CDD-7B32-4134-8BD1-FA2633BCEE94", "Failed to set up WebPrint endpoint for NACCS integration");

		var bodyText = isSuccessful ? GetSuccessfullBodyText() : GetFailedBodyText(message);

		var emailBuilder = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.FreeFormResponse);

		emailBuilder.AddArgReplacementRange(subject);
		emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, bodyText);

		var emailDef = emailBuilder.ToEmail();

		try
		{
			Env.OutgoingMailManager.Create(message.Factory, emailDef, group.PK.ToGuid(), GroupSourceLocator.GetFromGroup(group));
		}
		catch (EmailSendFailedException e)
		{
			logger.LogError(FormattableString.Invariant($@"Couldn't send email from Message - {message.EM_MessageNum}: {e.Message}. Here are the contents of the email that couldn't be sent:
SUBJECT: {emailDef.Subject}
BODY: {emailDef.Body}"));
		}
	}

	string GetSuccessfullBodyText()
	{
		return Res.GetString("AA5C392B-2A4E-4D62-85F9-FFCBC33381C9", @"As the result of setting up {0}, the WebPrint endpoint has been provisioned successfully.  

Messages from CargoWise directed to NACCS will now be routed through this endpoint and ready for relay by the WebPrint client. 

Please install WebPrint client and set up to connect to CargoWise and NACCS to finish the configuration.", JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.GetLocation());
	}

	string GetFailedBodyText(BaseEDIMessage message)
	{
		return Res.GetString("135A0584-FBC3-47DD-94DD-74ED5398F267", @"Please investigate by going to the Maintain > EDI Messaging > EDI Message module and query by Message Number - {0}. 

The endpoint is necessary as part of {1}.", message.EM_MessageNum, JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.GetLocation());
	}

	protected override LinkedBusinessObjectMetaData GetLinkedBusinessObjectMetaDataCore(BaseEDIMessage message, object linkedObject, ILoggingInformation logger)
	{
		return null;
	}
}
