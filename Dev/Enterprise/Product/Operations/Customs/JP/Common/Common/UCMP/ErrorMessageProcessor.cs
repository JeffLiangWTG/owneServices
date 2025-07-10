using System;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	sealed class ErrorMessageProcessor : NACCSMessageProcessor
	{
		public ErrorMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			var interchange = message.Interchange;

			if (interchange != null && !interchange.IsTransmitInterchange)
			{
				if (interchange.EI_From == Constants.WebPrintParty)
				{
					ProcessMessageFromWebPrint(message, interchange.EI_HeaderText);
				}
				else
				{
					ProcessMessageFromHub(message);
				}
			}
		}

		void ProcessMessageFromHub(EDIMessage message)
		{
			if (message.EM_LinkedObject is IErrorMessageProcessingStrategyParent processingStrategyParent)
			{
				processingStrategyParent.ProcessingStrategy?.ProcessMessage(message);
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}

		void ProcessMessageFromWebPrint(EDIMessage message, string headerText)
		{
			var status = EDIMessage.Status.Failed;

			var bodyContent = GetBodyNodeSafe(message);

			if (!string.IsNullOrWhiteSpace(bodyContent))
			{
				var responseMessage = new UniversalEventWrapper(bodyContent);
				var attributes = xTMessaging.Shared.Utils.GetHeaderTextDictionary(headerText);

				if (responseMessage.HasUnauthorizedFailure)
				{
					if (attributes.TryGetValue(Constants.DirectxT.CompanyCodeAttribute, out var companyCode) && !string.IsNullOrWhiteSpace(companyCode)
						&& attributes.TryGetValue(Constants.DirectxT.ClientMailboxAttribute, out var mailbox) && !string.IsNullOrWhiteSpace(mailbox))
					{
						var company = LoadCompany(message.Factory, companyCode);
						var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);
						var credential = wrapper?.MailboxCredential;

						if (credential?.FullMailBoxAddress.EqualsIgnoringCase(mailbox) ?? false)
						{
							credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
						}

						status = EDIMessage.Status.ProcessedOK;
					}
					else
					{
						Logger.LogWarning(FormattableString.Invariant($"Unable to process unauthorized failure message: {message.EM_MessageNum}"));
					}
				}
				else if (responseMessage.HasMessageTransmissionFailure)
				{
					if (message.EM_LinkedObject is IErrorMessageProcessingStrategyParent processingStrategyParent)
					{
						processingStrategyParent.ProcessingStrategy?.ProcessMessage(message);
						status = EDIMessage.Status.Received;
					}
					else
					{
						status = EDIMessage.Status.Failed;
					}
				}
				else
				{
					Logger.LogWarning(FormattableString.Invariant($"Unkown response message: {message.EM_MessageNum} Response Type: {responseMessage.ResponseType}"));
				}

				var group = JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value.NotificationGroup;
				if (group != null)
				{
					SendEmail(message, group, GetExceptionMessageSafe(bodyContent, message.EM_MessageNum));
				}
			}

			message.EM_Status = status;
		}

		void SendEmail(EDIMessage message, GlbGroup group, string exceptionMessage)
		{
			var webPrintCredentials = JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.Value;
			var subject = Res.GetString("CDF39360-AE9B-429B-8A13-D81359381778", "Failure message ({0}) received for host {1}", message.EM_MessageNum, webPrintCredentials.LocalComputerAlias);

			var bodyText = Res.GetString("4561617F-E1AB-4E73-9E0F-E46F55FFB210", @"An error message was received when trying to communicate with NACCS service.
The error detail is:
{0}
Please investigate by going to the module EDI Message and query by the message number {1}.", exceptionMessage, message.EM_MessageNum);

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
				Logger.LogError(FormattableString.Invariant($@"Couldn't send email from Message - {message.EM_MessageNum}: {e.Message}. Here are the contents of the email that couldn't be sent:
SUBJECT: {emailDef.Subject}
BODY: {emailDef.Body}"));
			}
		}

		GlbCompany LoadCompany(BusinessObjectFactory factory, string companyCode)
		{
			var query = new ZQuery(GlbCompanySchema.GC_Code, companyCode);
			query.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Japan);
			query.AddToFilter(GlbCompanySchema.GC_IsActive, true);

			return factory.LoadTop1<GlbCompany>(query);
		}

		public string GetBodyNodeSafe(EDIMessage message)
		{
			var result = string.Empty;

			try
			{
				using var stream = message.GetEM_MessageDataReader();
				var document = XDocument.Load(stream);
				var bodyNode = document.Root?.XPathSelectElement((NoResString)"//*[local-name()='UniversalInterchange']//*[local-name()='Body']")?.FirstNode;

				result = bodyNode?.ToString();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.LogWarning($"Can't parse a XML Document for message: {message.EM_MessageNum}");
			}

			return result;
		}

		string GetExceptionMessageSafe(string bodyNodeAsString, string messageNumber)
		{
			var result = string.Empty;

			try
			{
				var document = XDocument.Parse(bodyNodeAsString);

				result = document?.Root.XPathSelectElement("//*[local-name()='Reason']")?.Value ?? ZString.Empty;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Logger.LogWarning($"Fail to get the exception message for {messageNumber}");
			}

			return result;
		}
	}
}
