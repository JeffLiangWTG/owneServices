using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC035C;
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
	public class CC035CMessageProcessor : NCTSMessageProcessor<ICC035CDataProvider>
	{
		public CC035CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC035C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC035C };

		protected override Type MessageInterpreterType => typeof(CC035CMessageInterpreter);

		protected override ICC035CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc035CType, CC035CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC035CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC035CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
			{
				DiscardMessage(message, Res.GetString("D12A2CA0-BA1C-45AE-93F6-107186CCA4F6", "The message is discarded because its ‘Status at Customs’ has already the status WRO. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC035CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
