using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC928C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using ICC928CDataProvider = CargoWise.Customs.BE.MessageContracts.Interfaces.NCTS.ICC928CDataProvider;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC928CMessageProcessor : NCTSMessageProcessor<ICC928CDataProvider>
	{
		public CC928CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC928C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC928C };

		protected override Type MessageInterpreterType => typeof(CC928CMessageInterpreter);

		protected override ICC928CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc928CType, CargoWise.Customs.BE.MessageContracts.MessageProviders.NCTS.CC928CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC928CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC928CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			var customsStatus = moveHeader.BM_CustomsStatus;
			var messageStatus = nctsHeader.EffectiveMessageStatus;

			if (!(customsStatus.IsEmpty && (messageStatus == LogicalStatusList.Codes.Sent || messageStatus == LogicalStatusList.Codes.Acknowledged)))
			{
				Logger.LogError(Res.GetString("DF9967A1-C22F-4096-87D7-5714F30A4D79", "The message was discarded, because the Customs Status of the declaration is not blank or the Message Status of the declaration is not {0}. (Interchange Number:{1}, Number:{2}, Type:{3})", string.Join("/", new ZString[] { LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged }), message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("8E4AB8B8-CCAA-4D85-87A6-DEA000BDC0BC", "The message with interchange was discarded, because Customs Status of the declaration is not blank or the Message Status of the declaration is not {0}.", string.Join("/", new ZString[] { LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged })));
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC928CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			switch (moveHeader.BM_AdditionalDeclarationType)
			{
				case NctsTypeOfAdditionalDeclarationList.Codes.D:
					moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.PreLodged;
					break;
				case NctsTypeOfAdditionalDeclarationList.Codes.A:
					moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
					break;
			}
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
