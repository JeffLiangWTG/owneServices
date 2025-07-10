using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc004c;
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
	class CC004CProcessor : NctsBaseProcessor<Cc004CType>
	{
		public CC004CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Amendment Acceptance";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc004CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc004CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc004CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewPhase(Cc004CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override ZString GetNewDeclarationStatus(Cc004CType messageObject)
		{
			var newStatus = ZString.Empty;

			var moveHeader = NctsHeaderItem.MovementHeader;
			var movementReferenceNumber = NctsHeaderItem.MovementReferenceEntryNumber;

			if (movementReferenceNumber.CE_IssueDate.IsValid)
			{
				newStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			}
			else
			{
				if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
				{
					newStatus = string.IsNullOrEmpty(movementReferenceNumber.CE_EntryNum) ? NCTS5DepartureCustomsStatusList.Codes.PreLodged : NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
				}
				else
				{
					if (!string.IsNullOrEmpty(movementReferenceNumber.CE_EntryNum))
					{
						newStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					}
				}
			}
			return newStatus;
		}

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			IReadOnlyList<ZString> allowedStatuses = new List<ZString>
			{
				NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
				NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
				NCTS5DepartureCustomsStatusList.Codes.PreLodged,
				NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested,
				NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
			};
			if (!allowedStatuses.Contains(moveHeader.BM_CustomsStatus))
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}
			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid)
			{
				if (moveHeader.BM_Phase != NctsMovementHeaderTransactionStatusList.Codes.Amendment)
				{
					discardReasonText = BecauseWrongPhaseReasonText(moveHeader.BM_Phase);
					shouldDiscard = true;
				}
				else if (!IsSentAcknowledgedOrOK(nctsHeader.EffectiveMessageStatus))
				{
					discardReasonText = BecauseWrongMessageStatusReasonText(nctsHeader.EffectiveMessageStatus);
					shouldDiscard = true;
				}
			}

			return shouldDiscard;
		}

		protected override void AfterUpdateNCTSHeader(Cc004CType messageObject)
		{
			var movementReferenceNumber = NctsHeaderItem.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = MRN;

			var moveHeader = NctsHeaderItem.MovementHeader;
			if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
			{
				moveHeader.BM_EntryDate = messageObject.TransitOperation?.AmendmentAcceptanceDateAndTime ?? ZDateTime.Empty;
			}
		}

		protected override ZString GetMessageInterpretation(Cc004CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("New detailed status: Amendment acceptance");
			note.Append("Status granted on: " + GetReadableDateAndTime(messageObject.TransitOperation?.AmendmentAcceptanceDateAndTime));
			note.Append("Amendment submission date and time: " + GetReadableDateAndTime(messageObject.TransitOperation?.AmendmentSubmissionDateAndTime));
			note.Append("Correlation id: " + messageObject.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc004CType messageObject, EDIMessage inboundMessage)
		{
			if (NctsHeaderItem.MovementHeader.BM_CustomsStatus.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested))
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(inboundMessage, NctsHeaderItem.GetOutgoingMessage(inboundMessage), NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
