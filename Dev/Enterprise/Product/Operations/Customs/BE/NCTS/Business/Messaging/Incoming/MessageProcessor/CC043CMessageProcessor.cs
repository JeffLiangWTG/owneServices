using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC043C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC043CMessageProcessor : UnloadingPermissionMessageProcessor<ICC043CDataProvider>
	{
		public CC043CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC043C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC043C };

		protected override Type MessageInterpreterType => typeof(CC043CMessageInterpreter);

		protected override ICC043CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc043CType, CC043CDataProvider>();

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC043CDataProvider messageDataProvider)
		{
			var moveHeader = getMovementHeader(message);

			var isValidStatus = validCustomsStatusses.Contains(moveHeader.BM_CustomsStatus);
			var isUnloadPermissionOrRemarks =  moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted || moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
			var shouldContinueUnloading = messageDataProvider.ContinueUnloading == false;
			var isMessageWithoutConsignment = IsMessageWithoutConsignmentInfo(moveHeader);

			if ((!isValidStatus || (isUnloadPermissionOrRemarks && shouldContinueUnloading)) && !isMessageWithoutConsignment)
			{ 
				DiscardMessage(message, Res.GetString("133F3068-8156-457C-AA49-4F550628C04E", "The message is discarded because it found an NCTS declaration with MRN {3} with a wrong Customs Status {4}. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, messageDataProvider.MRN, moveHeader.BM_CustomsStatus));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC043CDataProvider messageDataProvider)
		{
			base.ProcessMessageCore(message, messageDataProvider);

			var moveHeader = getMovementHeader(message);
			var isMessageWithoutConsignmentInfo = IsMessageWithoutConsignmentInfo(moveHeader);

			moveHeader.Header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			switch (moveHeader.BM_CustomsStatus)
			{
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted:
					moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Arrival;
					break;
				case NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks:
					moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks;
					break;
			}

			if (!isMessageWithoutConsignmentInfo)
			{
				moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;
				moveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			}
		}

		bool IsMessageWithoutConsignmentInfo(NctsArrivalMovementHeader moveHeader) => new ZString[] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent }.Contains(moveHeader.Header.EffectiveMessageStatus) && moveHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks && moveHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;

		void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
		{
			if (sender is NctsArrivalMovementHeader moveHeader)
			{
				moveHeader.Header.LockFileIfEnabledByConfiguration(Res.GetString("A5F5B8D7-F0A1-407A-A21E-4DB039CA284F", "The tab 'Arrival Notification' was locked when message 'Unloading Permission' was received."), EUJobMessageTypeList.Codes.NctsArrivalNotification);
				moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
			}
		}

		readonly HashSet<ZString> validCustomsStatusses = new HashSet<ZString>
		{
			 NctsTransitStatusList.Codes.Unknown,
			 NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted,
			 NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks
		};

		NctsArrivalMovementHeader getMovementHeader(BEMessage message)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			return nctsHeader.ArrivalMovementHeader;
		}
	}
}
