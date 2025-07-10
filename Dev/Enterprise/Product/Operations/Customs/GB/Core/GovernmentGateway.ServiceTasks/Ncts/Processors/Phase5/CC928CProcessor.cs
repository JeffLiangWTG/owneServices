using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc928c;
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
	class CC928CProcessor : NctsBaseProcessor<Cc928CType>
	{
		public CC928CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Positive Acknowledge";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;

		protected override ZString MRN => null;

		protected override string GetMessageId(Cc928CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc928CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc928CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewPhase(Cc928CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override ZString GetNewDeclarationStatus(Cc928CType messageObject)
		{
			switch (NctsHeaderItem.MovementHeader.BM_AdditionalDeclarationType)
			{
				case NctsTypeOfAdditionalDeclarationList.Codes.D:
					return NCTS5DepartureCustomsStatusList.Codes.PreLodged;
				case NctsTypeOfAdditionalDeclarationList.Codes.A:
					return NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
			}

			return ZString.Empty;
		}

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			var customsStatus = moveHeader.BM_CustomsStatus;
			var messageStatus = nctsHeader.EffectiveMessageStatus;

			if (!customsStatus.IsEmpty)
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(customsStatus);
				shouldDiscard = true;
			}
			else if (!IsSentAcknowledgedOrOK(messageStatus))
			{
				discardReasonText = BecauseWrongMessageStatusReasonText(messageStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override ZString GetMessageInterpretation(Cc928CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("New declaration status: Declaration Accepted");
			note.Append("Correlation id: " + messageObject.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
