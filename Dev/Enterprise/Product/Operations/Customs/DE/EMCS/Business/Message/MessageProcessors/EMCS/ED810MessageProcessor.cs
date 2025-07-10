using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED810MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED810>, IED810>
	{
		public ED810MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F1A460B9-F2D5-4D6A-B16C-C386636D475D", "EMCS ED810 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED810> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var exciseMovementEad = provider.ExciseMovementEad;
				result = GetDeclarationFromEADNumber(message, exciseMovementEad.AdministrativeReferenceCode, provider.MessageGroup, exciseMovementEad.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED810> message)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;
			if (IsConsignorDeclaration(messageGroup))
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.CAN;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
			, emcsDeclaration
			, Res.GetString("D2D7A7F5-1C0C-4D6E-A718-6D1AF49D9384", "EMCS Cancellation of e-AD")
			, GetEmailBody(emcsDeclaration, provider)
			, false
			, message.Branch
			, emcsDeclaration
			, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovementEad.AdministrativeReferenceCode);
		}

		protected ZString GetEmailBody(EMCSJobDeclaration declaration, IED810 dataProvider)
		{
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("8D6A1E60-50AF-4D60-8FB1-C757E9274D56", "Your EMCS Declaration for Job {0} has been canceled. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("CE3602FA-0608-42FD-94A6-3C58250A9796", "ARC: {0}", dataProvider.ExciseMovementEad.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("EB2C6F1C-987B-4F15-993E-F4C2270C9A5E", "Cancellation Reason: {0}", declaration.Factory.GetCachedValue<EmcsCancellationReason>().GetDescriptionFromCode(dataProvider.CancellationReasonCode)));
			return emailBody.ToString();
		}
	}
}
