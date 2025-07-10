using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC045C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC045CMessageProcessor : NCTSMessageProcessor<ICC045CDataProvider>
	{
		public CC045CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC045C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC045C };

		protected override Type MessageInterpreterType => typeof(CC045CMessageInterpreter);

		protected override ICC045CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc045CType, CC045CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC045CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider, headerType: NctsMovementType.Codes.Departure)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC045CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
			{
				DiscardMessage(message, Res.GetString("52E2F54C-8827-45D4-B09C-92246FA19C86", "The message is discarded because its ‘Status at Customs’ has already the status WRO. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC045CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			nctsHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsCleared, eventTime: ((ZDateTime)messageDataProvider.WriteOffDate).ToOffset(), reference: moveHeader.BM_CustomsStatus));
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC045CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			moveHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
		}

		void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
		{
			if (sender is NctsDepartureMovementHeader moveHeader)
			{
				moveHeader.Header.LockFileIfEnabledByConfiguration(Res.GetString("29A19B38-4AC5-4791-BEFC-943F1409FA34", "The tabs are locked for editing because an Write-Off Notification was received."), EUJobMessageTypeList.Codes.NctsDeparture);
				moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
			}
		}
	}
}
