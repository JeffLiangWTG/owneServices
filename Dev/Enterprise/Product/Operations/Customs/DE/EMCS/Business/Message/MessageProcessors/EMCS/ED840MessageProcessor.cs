using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED840MessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED840>, IED840>
	{
		public ED840MessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("30b8dec2-ac33-47b1-8004-fb20e240b472", "EMCS ED840 Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED840> message)
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

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED840> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;
			var administrativeReferenceCode = provider.ExciseMovement.AdministrativeReferenceCode;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.EVT;

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("9594BF1A-9492-4EDB-BC04-B59166EF8273", "EMCS Event Report")
				, GetEmailBodyHeader(emcsDeclaration.JE_DeclarationReference, administrativeReferenceCode)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(administrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(ZString declarationReference, ZString administrativeReferenceCode)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("0FDCB61B-7960-458E-AA2C-0AFDCB12F9BD", "Your EMCS Declaration for Job {0} has received an Event Report. For details please follow the Link to the Job.", declarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("F829DA9B-08CF-4B5D-9E67-E3826A9AA933", "ARC: {0}", administrativeReferenceCode));

			return htmlBody.ToString();
		}
	}
}
