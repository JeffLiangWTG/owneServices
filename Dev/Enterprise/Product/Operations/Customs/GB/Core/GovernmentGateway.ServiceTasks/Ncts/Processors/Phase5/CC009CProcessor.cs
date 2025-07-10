using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc009c;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.tcl;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC009CProcessor : NctsBaseProcessor<Cc009CType>
	{
		public CC009CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Invalidation Decision";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc009CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc009CType messageObject) => messageObject.MessageType.ToString();

		protected override void AfterUpdateNCTSHeader(Cc009CType messageObject)
		{
			var movementHeader = NctsHeaderItem.MovementHeader;
			var phaseOriginal = movementHeader.BM_Phase;

			if (messageObject.Invalidation?.DecisionValue == Flag.Item1)
			{
				NctsHeaderItem.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
				movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Cancellation;
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
			}
			else if (messageObject.Invalidation?.DecisionValue == Flag.Item0 &&
				movementHeader.BM_Phase == NCTS5DeparturePhaseList.Codes.Cancellation)
			{
				NctsHeaderItem.EffectiveMessageStatus = LogicalStatusList.Codes.Invalid;
				movementHeader.BM_Phase = NCTS5DeparturePhaseList.Codes.Declaration;
			}

			var eventValue = new EventValue(Events.CustomsEntryStatus,
				eventTime: (messageObject.Invalidation?.DecisionDateAndTimeValue ?? ZDateTime.Empty).ToOffset(),
				reference: phaseOriginal);
			NctsHeaderItem.Logs.CreateRecreateOrUpdateEventLog(eventValue);
		}

		protected override ZString GetMessageInterpretation(Cc009CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("New declaration status: " +
				(messageObject.Invalidation?.DecisionValue == Flag.Item1 ? "Cancellation accepted" : "Cancellation refused"));
			note.Append("Status granted on: " + GetReadableDateAndTime(messageObject.Invalidation?.DecisionDateAndTimeValue));
			note.Append("Request date and time to invalidate/cancel: " + GetReadableDateAndTime(messageObject.Invalidation?.RequestDateAndTimeValue));
			note.Append("Initiated by customs: " + (messageObject.Invalidation?.InitiatedByCustoms == Flag.Item1 ? "yes" : "no"));
			note.Append("Justification: " + messageObject.Invalidation?.Justification);
			note.Append("Correlation id: " + messageObject.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc009CType messageObject, EDIMessage inboundMessage)
		{
			if (messageObject.Invalidation?.Decision.GetValueOrDefault() == Flag.Item1)
			{
				var outgoingMessage = NctsHeaderItem.GetOutgoingMessage(inboundMessage);
				Customs.Business.PermitHelper.UpdatePendingTransactions(inboundMessage, outgoingMessage, NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
				Customs.Business.PermitHelper.RollbackPermitTransactions(inboundMessage, outgoingMessage, null, NctsPermitHelper.GetPermitAppIdForMessage, ZString.Empty, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
