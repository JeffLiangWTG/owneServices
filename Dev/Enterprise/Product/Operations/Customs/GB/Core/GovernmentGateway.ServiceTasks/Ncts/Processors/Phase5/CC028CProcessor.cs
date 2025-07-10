using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc028c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC028CProcessor : NctsBaseProcessor<Cc028CType>
	{
		public CC028CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "MRN Allocated";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc028CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc028CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc028CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc028CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
		protected override ZString GetNewPhase(Cc028CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			if (new ZString[] { ZString.Empty, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.Acknowledged }.Contains(moveHeader.BM_CustomsStatus))
			{ }
			else
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void AfterUpdateNCTSHeader(Cc028CType messageObject)
		{
			var moveHeader = NctsHeaderItem.MovementHeader;
			if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
			{
				moveHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			}

			var movementReferenceNumber = NctsHeaderItem.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = MRN;
			var entryDate = messageObject.TransitOperation?.DeclarationAcceptanceDate ?? ZDateTime.Empty;
			moveHeader.BM_EntryDate = entryDate;
			NctsHeaderItem.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: entryDate.ToOffset(), reference: OriginalCustomsStatus));
		}

		protected override ZString GetMessageInterpretation(Cc028CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("New declaration status: Declaration MRN Allocated");
			note.Append("Status granted on: " + GetReadableDate(messageObject.TransitOperation?.DeclarationAcceptanceDate));
			note.Append("Correlation id: " + messageObject.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
