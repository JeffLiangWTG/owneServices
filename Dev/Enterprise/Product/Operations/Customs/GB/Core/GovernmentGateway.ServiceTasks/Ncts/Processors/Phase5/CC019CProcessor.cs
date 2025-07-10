using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc019c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC019CProcessor : NctsBaseProcessor<Cc019CType>
	{
		public CC019CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Discrepancies";

		protected override ZString LRN => null;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override ZString NoteForUnableToFindALinkedBusinessObject => ZString.Format("The processing of the message with interchange failed because the message could not be linked to a NCTS declaration with MRN {0}.", MRN);

		protected override string GetMessageId(Cc019CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc019CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc019CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc019CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;
		protected override ZString GetNewPhase(Cc019CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override ZString GetMessageInterpretation(Cc019CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();
			var interpretation = new NCTS019EdiMessagePrettier(inboundMessage).MakeInboundPrettyForInterpretation(NctsHeaderItem);
			note.Append(interpretation);
			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
