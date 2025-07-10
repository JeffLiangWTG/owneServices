using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED881MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED881>, IED881>
	{
		public ED881MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("609808AD-37F1-470A-B85B-144DC6A1D1D4", "EMCS ED881 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED881> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var responseAttributes = provider.ResponseAttributes;
				result = GetDeclarationFromEADNumber(message, responseAttributes.AdministrativeReferenceCode, provider.MessageGroup, responseAttributes.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED881> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			if (provider.RequestAccepted)
			{
				emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.MAN;
			}
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			var messageSubject = Res.GetString("068682E2-AB9E-4B82-B55F-336B0AAFE24C", "EMCS Manual Closure {0}", provider.RequestAccepted ? (NoResString)"Accepted" : (NoResString)"Rejected");
			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, messageSubject
				, GetEmailBodyHeader(emcsDeclaration.JE_DeclarationReference, message)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ResponseAttributes.AdministrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(ZString declarationReference, EmcsInboundEDIMessage<IED881> message)
		{
			var provider = message.DataProvider;

			var htmlBody = new StringBuilder();

			htmlBody.Append(provider.RequestAccepted
				? Res.GetString("BF1E5CD6-4FF4-4A22-81A0-9A5DD5C3C01C", "Your EMCS Declaration for Job {0} has been closed manually. For details please follow the link to the Job.", declarationReference)
				: Res.GetString("0ea9ffa8-1db1-47e1-9b83-8de2771e9302", "Your request for a Manual Closure of Job {0} has been rejected. For details please follow the link to the Job.", declarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("0C3E3EF2-09A5-4870-861F-643CE6211168", "Sending Customs Office: {0}", provider.MessageSender));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("9DF396E3-F975-4835-B749-150BC5618DD6", "MRN: {0}", provider.ResponseAttributes.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("926FFF44-A9EB-48B6-8B98-8D1A6FAF851C", "Sequence Number: {0}", provider.ResponseAttributes.SequenceNumber));

			if (!provider.RequestAccepted)
			{
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");

				var rejectionReasonList = message.Factory.GetCachedValue<EmcsManualClosureRejectionReasonList>();
				var reason = provider.RejectionReason;
				htmlBody.Append(Res.GetString("C1121242-682F-4EAA-ACBB-5503E07CA4E9", "Manual Closure Rejection Reason: {0} - {1}", reason, rejectionReasonList.GetDescriptionFromCode(reason)));

				if (reason == EmcsManualClosureRejectionReasonList.Codes.Andere)
				{
					htmlBody.Append("<br />");
					htmlBody.Append("<br />");
					htmlBody.Append(Res.GetString("2EBBEE99-E828-490B-A8AD-8C5987EAABAD", "Rejection Complement: {0}", provider.RejectionComplement));
				}
			}

			return htmlBody.ToString();
		}
	}
}
