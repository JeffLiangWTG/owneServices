using System;
using System.Collections.Generic;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC140C;
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
	public class CC140CMessageProcessor : NCTSMessageProcessor<ICC140CDataProvider>
	{
		public CC140CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC140C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC140C };

		protected override Type MessageInterpreterType => typeof(CC140CMessageInterpreter);

		protected override ICC140CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc140CType, CC140CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC140CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC140CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (moveHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
			{
				Logger.LogError(Res.GetString("35FE73CE-F6D8-449B-ACCC-DBD27FAEF14D", "The message is discarded because its 'Status at Customs' is not REL. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("6E78B9B6-3B39-471E-9630-110F3C741791", "The message with interchange was discarded, because the Status at Customs of the declaration is not REL."));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC140CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			moveHeader.EnquiryCustomsOffice.CY_Data = messageDataProvider.CustomsOfficeOfEnquiry;

			message.EM_Status = EDIMessage.Status.ProcessedOK;

			nctsHeader.Logs.AddNew(AutoEvents.CustomsImpedimentReceived, moveHeader.BM_CustomsStatus + ";Limit Date to respond: " + ((ZDateTime)messageDataProvider.LimitForResponseDate).ToOffset() + ";MRN: " + messageDataProvider.MRN, ((ZDateTime)messageDataProvider.RequestOnNonArrivedMovementDate).ToOffset());
		}
	}
}
