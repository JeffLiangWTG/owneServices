using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED803MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED803>, IED803>
	{
		public ED803MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("CCB262C0-F6E0-4B3F-AAA2-C5641293A552", "EMCS ED803 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED803> message)
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

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED803> message)
		{
			var provider = message.DataProvider;
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.DIV;
			SubscribeDocumentLinking(emcsDeclaration);

			messageGroup = provider.MessageGroup;
			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("D318AE30-0217-4C4D-B697-2C8BE0F30731", "EMCS Notification of diverted e-AD")
				, GetEmailBodyHeader(emcsDeclaration.JE_DeclarationReference, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovementEad.AdministrativeReferenceCode);
		}

		protected ZString GetEmailBodyHeader(ZString declarationReference, IED803 dataProvider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("21C2A761-0924-4E5C-BB7A-135C04FF10D2", @"Your EMCS Declaration for Job {0} received a notification of diverted e-AD. For details please follow the link to the Job.", declarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("925E9ECC-59AC-4CBA-A159-F8B6B3CF2D5E", @"Notification Date/Time: {0}", dataProvider.NotificationDateTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("929B6B28-3E88-4898-B72A-D9E836B9A88A", @"Notification Type: {0} - {1}", dataProvider.NotificationType, new EmcsNotificationType().GetDescriptionFromCode(dataProvider.NotificationType)));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("9891BC4A-3FBA-48AA-9215-FCD5855A8F53", @"ARC: {0}", dataProvider.ExciseMovementEad.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("FB676C13-FB77-4ADA-A403-F7746149C792", @"Sequence Number: {0}", dataProvider.ExciseMovementEad.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (dataProvider.DownstreamARCs.Any())
			{
				var htmlTableCreator = new HtmlTableCreator(new[] { Res.GetString("4EF0C620-6C4A-448A-8312-58014F6A0C4A", "Downstream ARC") });
				foreach (var downstreamArc in dataProvider.DownstreamARCs)
				{
					htmlTableCreator.WriteRow(downstreamArc);
				}
				htmlBody.Append(htmlTableCreator.ToHtml());
			}
			return htmlBody.ToString();
		}
	}
}
