using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc035c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC035CProcessor : NctsBaseProcessor<Cc035CType>
	{
		public CC035CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Recovery Notification";

		protected override ZString LRN => null;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc035CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc035CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc035CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc035CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure;
		protected override ZString GetNewPhase(Cc035CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			if (moveHeader != null && moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override ZString GetMessageInterpretation(Cc035CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("Recovery notification for NCTS departure received on " + GetReadableDate(messageObject.RecoveryNotification?.RecoveryNotificationDate.Value));
			note.Append(messageObject.RecoveryNotification?.RecoveryNotificationText ?? string.Empty);
			note.Append("Amount recovered: " + (messageObject.RecoveryNotification?.AmountClaimed ?? ZDecimal.Zero) + " " + (messageObject.RecoveryNotification?.Currency ?? string.Empty));
			AppendGuarantorIfPresent(note, messageObject.Guarantor);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
