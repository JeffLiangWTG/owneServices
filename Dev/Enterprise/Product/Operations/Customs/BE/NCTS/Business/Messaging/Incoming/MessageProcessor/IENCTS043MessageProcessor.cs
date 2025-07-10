using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.IENCTS043;
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
	public class IENCTS043MessageProcessor : UnloadingPermissionMessageProcessor<ICC043CAndIENCTS043CDataProvider>
	{
		public IENCTS043MessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.IENCTS043;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.IENCTS043 };

		protected override Type MessageInterpreterType => typeof(IENCTS043MessageInterpreter);

		protected override ICC043CAndIENCTS043CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Iencts043Type, IENCTS043DataProvider>();

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC043CAndIENCTS043CDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			var moveHeader = nctsHeader.ArrivalMovementHeader;
			var isArrivalSent = moveHeader.BM_CustomsStatus == ZString.Empty && moveHeader.BM_MessageStatus == LogicalStatusList.Codes.Sent && moveHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Arrival;
			var isAcceptedDiscrepencyOrRelease = (moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease || moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease || moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease || moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease) && moveHeader.BM_MessageStatus == LogicalStatusList.Codes.Accepted;
			var isCC043CRecieved = nctsHeader.Messages.Cast<BEMessage>().Any(x => x.EM_MessageSubType == BEIncomingMessageSubTypes.Codes.CC043C && x.EM_Status == EDIMessage.Status.ProcessedOK);

			if (!isArrivalSent && !isAcceptedDiscrepencyOrRelease)
			{
				DiscardMessage(message, Res.GetString("372265C3-7149-4E20-AD05-4566CCA2DEE1", "The message is discarded because it found an NCTS declaration with MRN {3} with a wrong Customs Status {4}, Phase status {5}, Message Status {6}. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, messageDataProvider.MRN, moveHeader.BM_CustomsStatus, moveHeader.BM_Phase, moveHeader.BM_MessageStatus));
			} else if (isCC043CRecieved)
			{
				DiscardMessage(message, Res.GetString("B706B450-9852-4DE4-B98D-72D3B96D177D", "The message is discarded because it found an NCTS declaration with MRN {3} that already received a CC043C Message. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, messageDataProvider.MRN));
			}
		}
	}
}
