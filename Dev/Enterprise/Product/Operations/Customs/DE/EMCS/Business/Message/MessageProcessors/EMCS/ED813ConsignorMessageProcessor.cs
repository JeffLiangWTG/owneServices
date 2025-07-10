using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED813ConsignorMessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED813>, IED813>
	{
		public ED813ConsignorMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("64D37B90-5567-4FC9-9A34-6F83DF74DF18", "EMCS ED813 Consignor Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED813> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var updateEadEsad = provider.UpdateEadEsad;
				var previousSequenceNumber = (ZShort.ParseSafe(updateEadEsad.SequenceNumber, 1) - 1).ToString();
				result = GetDeclarationFromEADNumber(message, updateEadEsad.AdministrativeReferenceCode, provider.MessageGroup, previousSequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED813> message)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.CHG;
			emcsDeclaration.SequenceNumber = provider.UpdateEadEsad.SequenceNumber;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("3C0C61A5-C323-497C-ABFD-E63CF103CEC1", "EMCS Change of Destination")
				, GetEmailBody(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.UpdateEadEsad.AdministrativeReferenceCode);
		}

		ZString GetEmailBody(EMCSJobDeclaration declaration, IED813 dataProvider)
		{
			var newDestinationCode = dataProvider.NewDestinationCode;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("D9651EA9-947F-48E3-B04D-5AA107BDBD0F", "Your EMCS Declaration for Job {0} received a notification of a changed destination. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("CE3602FA-0608-42FD-94A6-3C58250A9796", "ARC: {0}", dataProvider.UpdateEadEsad.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("D5B95158-97BD-4968-B11E-3AAEC103DB76", "New Destination Code: {0}", newDestinationCode + " " + declaration.Factory.GetCachedValue<EU.EMCS.Business.EMCSDestinationTypeList>().GetDescriptionFromCode(newDestinationCode)));
			return emailBody.ToString();
		}
	}
}
