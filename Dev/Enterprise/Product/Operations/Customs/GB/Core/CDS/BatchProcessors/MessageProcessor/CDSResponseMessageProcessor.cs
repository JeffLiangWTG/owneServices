using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.GB.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSResponseMessageProcessor : CDSMessageProcessor<CDSResponseEDIMessage>
	{
		public CDSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString ProcessMessageCore(CDSResponseEDIMessage cdsEDIMessage,
			BusinessObjectFactory factory)
		{
			var messageDataObject = cdsEDIMessage.MessageDataObject;
			if (messageDataObject == null)
			{
				Logger.Log($"Could not process message #{cdsEDIMessage.EM_MessageNum} as its content was blank. Most likely this is due to a user marking a system-generated eHub acknowledgement message as status queued for reprocessing. Message is skipped.");

				return EDIMessageStatusList.Codes.Discarded;
			}

			var cusEntryNumber = (messageDataObject.GetCusEntryNumberFromLRN(factory) ?? messageDataObject.GetCusEntryNumberFromMRN(factory));
			var parent = cusEntryNumber != null ? cusEntryNumber.Parent : GetMessageAttacheeFromConversationID(cdsEDIMessage.EM_ApplicationReference, factory);

			var entryHeader = parent as CusEntryHeader;
			var asycudaBill = parent as AsycudaBill;

			if (entryHeader == null && asycudaBill == null)
			{
				Logger.Log($"Could not process message #{cdsEDIMessage.EM_MessageNum} as there is no CusEntryHeader/AsycudaBill found. Message is skipped.");

				return EDIMessageStatusList.Codes.Discarded;
			}

			if (entryHeader != null)
			{
				CusEntryHeaderProcessor.ProcessCusEntryHeader(factory, entryHeader, cdsEDIMessage, Logger);
				if (MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration(entryHeader.Declaration))
				{
					MessageResponseSemaphoreHelper.RemoveSemaphoreForDeclaration(entryHeader.Declaration);
				}
			}
			else
			{
				AsycudaBillProcessor.ProcessAsycudaBill(factory, parent as AsycudaBill, cdsEDIMessage, Logger);
			}

			return EDIMessageStatusList.Codes.ProcessedOK;
		}

		public static CDSEDIMessage GetOutgoingMessageFromConversationID(ZString conversationId, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, conversationId);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			var message = factory.LoadTop1<CDSEDIMessage>(query);

			return message;
		}

		public static BusinessObject GetMessageAttacheeFromConversationID(ZString conversationId, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationReference, conversationId);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, CDSEDIMessageTypeList.Codes.ConversationID);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			var message = factory.LoadTop1<CDSResponseEDIMessage>(query);

			return message?.EM_LinkedObject;
		}

		protected override string MessageFriendlyNameCore => "CDS Response Message";

		protected override ZString MessageType => CDSEDIMessageTypeList.Codes.Response;
	}
}
