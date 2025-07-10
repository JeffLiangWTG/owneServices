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
	public class ED807MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED807>, IED807>
	{
		public ED807MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("EE405507-99BD-49A9-A31B-D6EF0868FFE9", "EMCS ED807 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED807> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var movementAttribute = provider.ExciseMovementEad;
				result = GetDeclarationFromEADNumber(message, movementAttribute.AdministrativeReferenceCode, provider.MessageGroup, movementAttribute.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED807> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.INT;

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("e4eff63b-dcce-469b-b6ee-b6c297b658bf", "EMCS Interruption of Movement")
				, GetEmailBody(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastSentOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovementEad.AdministrativeReferenceCode);
		}

		static ZString GetEmailBody(EMCSJobDeclaration declaration, IED807 provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("cff03e48-c30c-4680-8462-353027db2e89", "Your EMCS Declaration for Job {0} received a notification that the movement has been interrupted. For details please follow the link to the Job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var headerTableCreator = new HtmlTableCreator();
			var exciseMovementEad = provider.ExciseMovementEad;
			headerTableCreator.WriteRow(Res.GetString("7030c81c-660e-42e5-addf-99716146de41", "ARC"), exciseMovementEad.AdministrativeReferenceCode);
			headerTableCreator.WriteRow(Res.GetString("b233129c-cb98-46e2-ba2f-843fedd5c8d1", "Reason"), exciseMovementEad.Reason + " - " + new EmcsInterruptionReasonList().GetDescriptionFromCode(exciseMovementEad.Reason));
			if (!exciseMovementEad.ComplementaryInformation.IsEmpty)
			{
				headerTableCreator.WriteRow(Res.GetString("40bf169b-1708-46f2-ac63-5f6f93dcbbc2", "Complementary Information"), exciseMovementEad.ComplementaryInformation);
			}
			htmlBody.Append(headerTableCreator.ToHtml());

			if (provider.ControlReportNumbers.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("37158862-54fb-4ae5-b38e-9a268dbf614b", "Control Report"));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				var controlReportTableCreator = new HtmlTableCreator(new[] { Res.GetString("101a0906-6e71-4dc7-8ac6-27e36ec5503b", "Control Report Number") });
				foreach (var controlReportNumber in provider.ControlReportNumbers)
				{
					controlReportTableCreator.WriteRow(controlReportNumber);
				}
				htmlBody.Append(controlReportTableCreator.ToHtml());
			}
			if (provider.EventReportNumbers.Any())
			{
				htmlBody.Append("<br />");
				htmlBody.Append(Res.GetString("d035be4d-718c-47bb-9414-d6c7374f7ad8", "Event Report"));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				var eventReportTableCreator = new HtmlTableCreator(new[] { Res.GetString("2a354a75-cd95-49ce-ad79-b33cffef5967", "Event Report Number") });
				foreach (var eventReportNumber in provider.EventReportNumbers)
				{
					eventReportTableCreator.WriteRow(eventReportNumber);
				}
				htmlBody.Append(eventReportTableCreator.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}

