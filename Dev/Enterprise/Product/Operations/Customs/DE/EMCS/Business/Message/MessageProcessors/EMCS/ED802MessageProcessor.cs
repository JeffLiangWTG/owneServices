using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED802MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED802>, IED802>
	{
		public ED802MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("FB09243B-3C76-41BD-B5D3-BF3BF400DF64", "EMCS ED802 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED802> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				result = GetDeclarationFromEADNumber(message, provider.ExciseMovement.AdministrativeReferenceCode, provider.MessageGroup, provider.ExciseMovement.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED802> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REM;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("F923D127-7C80-4B7E-B487-C5ED0569A35B", "EMCS Reminder Message")
				, GetEmailBodyHeader(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(EMCSJobDeclaration emcsDeclaration, IED802 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("76CF4B26-34A1-41D7-BC8E-258D8EBC71EE", "Your EMCS Declaration for Job {0} received a reminder message. For details please follow the Link to the Job.", emcsDeclaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("946BA292-ECC9-4D4D-81F3-D3F8C0FBF4CB", "ARC: {0}", dataProvider.ExciseMovement.AdministrativeReferenceCode));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("23E26F13-53B4-435E-AD31-B7C824E7C1DE", "Line No.: {0}", dataProvider.ExciseMovement.SequenceNumber));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("758BE0E8-74F9-4BDC-88A3-7C9ACA602822", "Limit Date Time: {0}", dataProvider.LimitDateTime.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture)));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			if (!dataProvider.ReminderInformation.IsEmpty)
			{
				htmlBody.Append(Res.GetString("D34698FC-FED3-4C35-8A5D-445A1A5A4E65", "Reminder Information: {0}", dataProvider.ReminderInformation));
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
			}
			htmlBody.Append(Res.GetString("664C7D13-6BF5-483F-AC3B-25D0CE7D0D73", "Reminder Message Type: {0} - {1}", dataProvider.ReminderMessageType, emcsDeclaration.Factory.GetCachedValue<EmcsReminderMessageType>().GetDescriptionFromCode(dataProvider.ReminderMessageType)));

			return htmlBody.ToString();
		}
	}
}
