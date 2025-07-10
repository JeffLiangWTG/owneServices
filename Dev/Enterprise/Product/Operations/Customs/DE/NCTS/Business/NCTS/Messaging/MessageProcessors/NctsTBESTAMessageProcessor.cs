using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsTBESTAMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<ITBESTA>, ITBESTA>
	{
		public NctsTBESTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		protected override string MessageFriendlyNameCore => Res.GetString("6A2801D1-50A8-4383-A5E3-3DD662C5402C", "NCTS TBESTA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ITBESTA> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ITBESTA> message)
		{
			var header = (NctsHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			header.EffectiveMessageStatus = NctsMessageStatusList.Codes.ArrivalNotificationSent;

			var dataProvider = message.DataProvider;
			var (mappedSuccessfully, mappedStatusCode) = NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus(dataProvider.DestinationStatus);
			if (mappedSuccessfully)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = mappedStatusCode;
			}
			var emailBody = GetStatusEmailBody(header, header.MovementReferenceNumber, dataProvider.DestinationStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NctsCustomsStatus);
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, header, Res.GetString("1A225ADE-F6F4-4A5B-9795-90A161A82C42", "NCTS TBE Status Message"), emailBody, false, message.Branch, header, dataProvider.ReferencedMessageIdentifier);
		}
	}
}
