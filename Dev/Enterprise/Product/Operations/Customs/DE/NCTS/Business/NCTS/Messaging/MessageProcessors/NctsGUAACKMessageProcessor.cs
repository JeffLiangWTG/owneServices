using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsGUAACKMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IGUAACK>, IGUAACK>
	{
		public NctsGUAACKMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("da69e2b5-bbcf-413d-8a8c-3607fc08709f", "NCTS GUAACK Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IGUAACK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IGUAACK> message)
		{
			var provider = message.DataProvider;
			var guaranteeReferenceNumber = provider.GuaranteeReferenceNumber;
			var guaranteeHeader = (CusGuaranteeHeader)message.EM_LinkedObject;

			var originalMessage = GetOriginalMessage(factory, provider.ReferencedMessageIdentifier);

			var newMainAccessCode = originalMessage.Notes.FindByDescription(LogbookHelper.LogbookGUAMainAccessCode).SingleOrDefault().ST_NoteText;
			if (!newMainAccessCode.IsEmpty)
			{
				guaranteeHeader.MainAccessCode = newMainAccessCode.SubstringSafe(0, guaranteeHeader.MainAccessCodeInfo.MaxLength);
			}
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookLocalReferenceNumber(guaranteeReferenceNumber);

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, null
					, Res.GetString("3a587076-05fb-4895-af2f-00989280f07e", "Confirmation of access/administration code change in Guarantee: {0}", guaranteeReferenceNumber)
					, GetEmailBody()
					, isFailure: false
					, message.Branch
					, guaranteeHeader
					, provider.ReferencedMessageIdentifier);

			string GetEmailBody()
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("b5e0f8a6-cb44-4b7f-a575-4af83c3995af", "Your main access code for GRN {0} has been updated.", guaranteeReferenceNumber));
				htmlBody.Append("<br /><br />");

				return htmlBody.ToString();
			}
		}
	}
}
