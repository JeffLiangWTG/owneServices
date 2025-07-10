using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC019C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC019CMessageProcessor : NCTSMessageProcessor<ICC019CDataProvider>
	{
		public CC019CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override ZString NoteForUnableToFindALinkedBusinessObject(ICC019CDataProvider messageDataProvider) => Res.GetString("81BE6DB5-369B-4A17-813E-B22D6AC8D89C", "The processing of the message with interchange failed because the message could not be linked to a NCTS declaration with MRN {0}.", messageDataProvider.MRN);

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC019C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC019C };

		protected override Type MessageInterpreterType => typeof(CC019CMessageInterpreter);

		protected override ICC019CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc019CType, CC019CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC019CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC019CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
			{
				Logger.LogError(Res.GetString("9F7C37EC-C518-4475-BF0B-65FEC095CF14", "The message is discarded because its ‘Status at Customs’ has already the status WRO. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("2A6A7A97-BE15-420C-9AC7-1669E61165D5", "The message with interchange was discarded, because the Status at Customs of the declaration has already the status WRO."));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC019CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
