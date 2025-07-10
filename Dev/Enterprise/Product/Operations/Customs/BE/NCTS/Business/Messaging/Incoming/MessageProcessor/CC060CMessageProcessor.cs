using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC060C;
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
	public class CC060CMessageProcessor : NCTSMessageProcessor<ICC060CDataProvider>
	{
		public CC060CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC060C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC060C };

		protected override Type MessageInterpreterType => typeof(CC060CMessageInterpreter);

		protected override ICC060CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc060CType, CC060CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC060CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRNOrMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC060CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			var validCustomsStatusesForPreProcess = new ZString[] { NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated };
			var validPhaseStatussesForPreProcess = new ZString[] { NctsMovementHeaderTransactionStatusList.Codes.Amendment, NctsMovementHeaderTransactionStatusList.Codes.Declaration };
			var validMessageStatussesForPreProcess = new ZString[] { LogicalStatusList.Codes.Accepted, LogicalStatusList.Codes.Invalid };

			if (!validCustomsStatusesForPreProcess.Contains(moveHeader.BM_CustomsStatus)
				|| !validPhaseStatussesForPreProcess.Contains(moveHeader.BM_Phase)
				|| !validMessageStatussesForPreProcess.Contains(moveHeader.BM_MessageStatus))
			{
				Logger.LogError(Res.GetString("B2E49AF4-B8F3-4423-8190-79DFC04BC8AA", "The message was discarded, because the Status at Customs of the declaration is different from {0}, or the phase is different from {1} or the message status is different from {2}. (Interchange Number:{3}, Number:{4}, Type:{5}); message status set to DISCARDED.", string.Join("/", validCustomsStatusesForPreProcess), string.Join("/", validPhaseStatussesForPreProcess), string.Join("/", validMessageStatussesForPreProcess), message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("A33C0B9A-3443-4A74-A653-78E47B825129", "The message with interchange was discarded, because the Status at Customs of the declaration is different from {0}, or the phase is different from {1} or the message status is different from {2}.", string.Join("/", validCustomsStatusesForPreProcess), string.Join("/", validPhaseStatussesForPreProcess), string.Join("/", validMessageStatussesForPreProcess)));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC060CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			switch (messageDataProvider.NotificationType)
			{
				case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
					moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
					break;
				case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
					moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
					break;
				case NCTS5NotificationTypes.Codes.IntentionToControl:
					moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
					break;
			}

			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl ||
				moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest ||
				moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.IntentionToControl)
			{
				var service = nctsHeader.Services.AddNew();
				service.ES_ServiceCode = Constants.ServiceTypes.CTL;
				service.ES_Booked = messageDataProvider.ControlNotificationDateAndTimeUtc;
				service.ES_References = messageDataProvider.MRN;
				switch (messageDataProvider.NotificationType)
				{
					case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
						service.ES_ServiceNote = Res.GetString("9DBA0AC2-FAE4-42DB-BF64-5AFB346D046B", "Intention to control. Type of Controls: ");
						break;
					case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
						service.ES_ServiceNote = Res.GetString("B22DEB16-FB57-4309-B404-503D353B9233", "Decision to control. Type of Controls: ");
						break;
					case NCTS5NotificationTypes.Codes.IntentionToControl:
						service.ES_ServiceNote = Res.GetString("2CB18CF2-9C41-4E50-B2E6-1EDBF4770C5A", "Additional document request. Type of Controls: ");
						break;
				}
				foreach (var typeOfControl in messageDataProvider.TypeOfControls)
				{
					var type = typeOfControl.Type;
					service.ES_ServiceNote = $"{service.ES_ServiceNote} {new NCTS5TypeOfControlTypes().GetDescriptionFromCode(type)} {typeOfControl.Text}";
				}
			}

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			nctsHeader.Logs.AddNew(AutoEvents.CustomsImpedimentReceived, moveHeader.BM_CustomsStatus, ((ZDateTime)messageDataProvider.ControlNotificationDateAndTimeUtc).ToOffset());
		}

		protected override bool CheckMessageSequenceIsValidCore(BEMessage message) => !(message.EM_LinkedObject is NctsDepartureMovementHeader nctsMovementHeader && nctsMovementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Acknowledged);
	}
}
