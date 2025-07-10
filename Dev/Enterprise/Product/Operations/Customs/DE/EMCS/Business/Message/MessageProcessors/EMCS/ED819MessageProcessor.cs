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
	public class ED819MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED819>, IED819>
	{
		public ED819MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2CA7C393-E751-49A6-A14E-34D488047E7A", "EMCS ED819 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED819> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var exciseMovement = provider.ExciseMovement;
				result = GetDeclarationFromEADNumber(message, exciseMovement.AdministrativeReferenceCode, provider.MessageGroup, exciseMovement.SequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED819> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.ALT;
			if (IsConsigneeDeclaration(messageGroup))
			{
				emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			}

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("1F0A4449-C586-4435-AE5E-C0717160649C", "EMCS Notification of Alert or Rejection")
				, GetEmailBodyHeader(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(EMCSJobDeclaration emcsDeclaration, IED819 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("DC7752BA-DDEB-4C70-B42C-8AB053A8DE84", "Your EMCS Declaration for Job {0} received an Alert or Rejection. For details please follow the link to the Job.", emcsDeclaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("D85F3A13-4E71-4E10-AAB5-91EB9965CF17", "ARC: {0}", dataProvider.ExciseMovement.AdministrativeReferenceCode));

			if (dataProvider.AlertOrRejectionReasons.Any())
			{
				var emailTable = new HtmlTableCreator(new[]
				{
					Res.GetString("143BD01A-3243-40BC-80D7-1EAF1D190530", "Reason"),
					Res.GetString("ABD48E8A-2882-4E78-8B1E-4AEA177D7917", "Additional Information")
				});
				foreach (var alertOrRejectionReason in dataProvider.AlertOrRejectionReasons)
				{
					emailTable.WriteRow(alertOrRejectionReason.ReasonCode + " - " + emcsDeclaration.Factory.GetCachedValue<EU.EMCS.Business.EMCSAlertRejectionCodeList>().GetDescriptionFromCode(alertOrRejectionReason.ReasonCode)
						, alertOrRejectionReason.ComplementaryInformation);
				}
				htmlBody.Append("<br />");
				htmlBody.Append("<br />");
				htmlBody.Append(emailTable.ToHtml());
			}

			return htmlBody.ToString();
		}
	}
}
