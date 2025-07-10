using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC056C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using CustomsStatusCodes = Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists.NCTS5DepartureCustomsStatusList.Codes;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC056CMessageProcessor : NCTSMessageProcessor<ICC056CDataProvider>
	{
		public CC056CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC056C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC056C };

		protected override Type MessageInterpreterType => typeof(CC056CMessageInterpreter);

		protected override ICC056CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc056CType, CC056CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC056CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRNOrMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC056CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			var rejectionType = messageDataProvider.BusinessRejectionType;

			if (!IsDeclarationStatusCorrect(rejectionType, nctsHeader.EffectiveMessageStatus, moveHeader.BM_CustomsStatus, moveHeader.BM_Phase))
			{
				Logger.LogError(Res.GetString("5980A03B-8123-4306-A1A1-FA5A6CD86242", "The message was discarded, because the Message Status and the Customs status of the declaration could not be mapped to correct value of the Business Rejection Type element. (Interchange Number:{0}, Number:{1}, Type:{2})", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("3731BD6D-797B-4EA4-A3D7-E317806923E6", "The message with interchange was discarded, because the Message Status and the Customs status of the declaration could not be mapped to correct value of the Business Rejection Type element."));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC056CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			var rejectionType = messageDataProvider.BusinessRejectionType;

			if (IsDeclarationStatusCorrect(rejectionType, nctsHeader.EffectiveMessageStatus, moveHeader.BM_CustomsStatus, moveHeader.BM_Phase))
			{
				nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
				if (moveHeader.BM_CustomsStatus == CustomsStatusCodes.GuaranteeInvalid && moveHeader.BM_Phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment)
				{
					moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
				}
			}

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC056CDataProvider messageDataProvider)
		{
			var rejectionType = messageDataProvider.BusinessRejectionType;
			if (rejectionType == BEOutgoingMessageTypes.Codes.CC015C)
			{
				Customs.Business.PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Belgium, ((NctsDepartureMovementHeader)message.EM_LinkedObject).BM_PaperlessInbondNum, null, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			}
		}

		bool IsDeclarationStatusCorrect(ZString rejectionType, ZString messageStatus, ZString customsStatus, ZString phase)
		{
			ZBool messageStatusIsSentOrAcknowledge = messageStatus == LogicalStatusList.Codes.Sent || messageStatus == LogicalStatusList.Codes.Acknowledged;
			return messageStatusIsSentOrAcknowledge && PhaseAndCustomsStatusIsCorrect();

			ZBool PhaseAndCustomsStatusIsCorrect()
			{
				ZBool phaseAndCustomsStatusIsCorrect = false;

				switch (rejectionType)
				{
					case BEOutgoingMessageTypes.Codes.CC013C:
						phaseAndCustomsStatusIsCorrect = phase == NctsMovementHeaderTransactionStatusList.Codes.Amendment && customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged, CustomsStatusCodes.MrnAllocated, CustomsStatusCodes.AmendmentRequested, CustomsStatusCodes.GuaranteeInvalid });
						break;
					case BEOutgoingMessageTypes.Codes.CC014C:
						phaseAndCustomsStatusIsCorrect = phase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation && customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged, CustomsStatusCodes.MrnAllocated, CustomsStatusCodes.ReleasedForTransit });
						break;
					case BEOutgoingMessageTypes.Codes.CC015C:
						phaseAndCustomsStatusIsCorrect = phase == NctsMovementHeaderTransactionStatusList.Codes.Declaration && string.IsNullOrEmpty(customsStatus);
						break;
					case BEOutgoingMessageTypes.Codes.CC054C:
						phaseAndCustomsStatusIsCorrect = customsStatus.In(new ZString[] { CustomsStatusCodes.AmendmentRequested, CustomsStatusCodes.DecisionToControl, CustomsStatusCodes.AdditionalDocumentsRequest, CustomsStatusCodes.IntentionToControl });
						break;
					case BEOutgoingMessageTypes.Codes.CC141C:
						phaseAndCustomsStatusIsCorrect = customsStatus == NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
						break;
					case BEOutgoingMessageTypes.Codes.CC170C:
						phaseAndCustomsStatusIsCorrect = customsStatus.In(new ZString[] { CustomsStatusCodes.Acknowledged, CustomsStatusCodes.PreLodged });
						break;
				}
				return phaseAndCustomsStatusIsCorrect;
			}
		}
	}
}
