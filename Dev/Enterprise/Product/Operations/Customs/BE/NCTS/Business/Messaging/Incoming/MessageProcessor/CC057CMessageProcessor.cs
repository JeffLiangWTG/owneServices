using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC057C;
using CargoWise.Customs.Shared.MessageContracts;
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

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC057CMessageProcessor : NCTSMessageProcessor<ICC057CDataProvider>
	{
		public CC057CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC057C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC057C };

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC057CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider, NctsMoveHeaderType.Codes.Arrival);

		protected override ICC057CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc057CType, CC057CDataProvider>();

		protected override Type MessageInterpreterType => typeof(CC057CMessageInterpreter);

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC057CDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			var moveHeader = nctsHeader.ArrivalMovementHeader;
			var continueProcessing = false;

			switch (messageDataProvider.TransitOperation?.BusinessRejectionType)
			{
				case BEOutgoingMessageTypes.Codes.CC007C:
					continueProcessing = nctsHeader.EffectiveMessageStatus.ToString().In(LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent) && moveHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.Unknown;
					break;
				case BEOutgoingMessageTypes.Codes.CC044C:
					continueProcessing = nctsHeader.EffectiveMessageStatus.ToString().In(LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent) && moveHeader.BM_CustomsStatus.ToString().In(NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
					break;
			}

			if (!continueProcessing)
			{
				Logger.LogError(Res.GetString("BD99DC4C-49C3-444E-A561-B906755E67CC", "The message was discarded because the arrival declaration with MRN {4} has the wrong Customs Status ({5}) in combination with its 'Message Status' ({6}).(Interchange Number : {0}, Number : {1}, Type: {2}); message status set to {3}.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, EDIMessage.Status.Discarded, messageDataProvider.MRN, moveHeader.BM_CustomsStatus, nctsHeader.EffectiveMessageStatus));
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("0FBFD078-43A4-4FD7-B859-5316234A7606", "The message was discarded because the arrival declaration with MRN {0} has the wrong Customs Status ({1}) in combination with its 'Message Status' ({2}).", messageDataProvider.MRN, moveHeader.BM_CustomsStatus, nctsHeader.EffectiveMessageStatus));
				message.EM_Status = EDIMessageStatusList.Codes.Discarded;
			}
			else
			{
				message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC057CDataProvider messageDataProvider)
		{
			var nctsHeader = NctsMessageHelper.GetNctsHeaderFromLinkedObject(message);
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
