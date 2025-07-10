using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc056c;
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
	public class CC056CProcessor : NctsBaseProcessor<Cc056CType>
	{
		public CC056CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Rejection From Office Of Departure";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc056CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc056CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc056CType messageObject) => LogicalStatusList.Codes.Invalid;

		bool IsDeclarationAcknowledged { get; set; }

		bool IsRegenerateLrnRequired =>  !EU.Registry.EUCustomsDataRegistry.Instance.NctsIsManualDepartureCustomerReferenceEnabled.Value;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			var messageStatus = nctsHeader.EffectiveMessageStatus;
			var customsStatus = moveHeader.BM_CustomsStatus;
			var phase = moveHeader.BM_Phase;
			var rejectionType = MessageObject?.TransitOperation?.BusinessRejectionType;

			var phaseAndCustomsStatusIsCorrect = false;
			switch (rejectionType)
			{
				case "013":
					phaseAndCustomsStatusIsCorrect = phase == NCTS5DeparturePhaseList.Codes.Amendment && customsStatus.In(new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested });
					break;
				case "014":
					phaseAndCustomsStatusIsCorrect = phase == NCTS5DeparturePhaseList.Codes.Cancellation && customsStatus.In(new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit });
					break;
				case "015":
					phaseAndCustomsStatusIsCorrect = phase == NCTS5DeparturePhaseList.Codes.Declaration && string.IsNullOrEmpty(customsStatus);
					break;
				case "054":
					phaseAndCustomsStatusIsCorrect = customsStatus.In(new ZString[] { NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl });
					break;
				case "141":
					phaseAndCustomsStatusIsCorrect = customsStatus == NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
					break;
				case "170":
					phaseAndCustomsStatusIsCorrect = customsStatus.In(new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged });
					break;
			}

			IsDeclarationAcknowledged = nctsHeader.EffectiveMessageStatus == LogicalStatusList.Codes.Accepted
				&& moveHeader.BM_Phase == NCTS5DeparturePhaseList.Codes.Declaration
				&& new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged }.Contains(moveHeader.BM_CustomsStatus);

			if (!IsDeclarationAcknowledged && (!IsSentAcknowledgedOrOK(messageStatus) || !phaseAndCustomsStatusIsCorrect))
			{
				var messageStatusText = $"Message Status{(!string.IsNullOrEmpty(messageStatus) ? $" of {messageStatus}" : string.Empty)}";
				var customsStatusText = $"Customs Status{(!string.IsNullOrEmpty(customsStatus) ? $" of {customsStatus}" : string.Empty)}";
				var phaseStatusText = !string.IsNullOrEmpty(phase) ? $" for Phase Status of {phase}" : string.Empty;
				var rejectionTypeText = $"Rejection Type{(!string.IsNullOrEmpty(rejectionType) ? $" of {rejectionType}" : string.Empty)}";

				discardReasonText = $"The message was discarded, because the {messageStatusText} and the {customsStatusText} for the declaration could not be mapped to correct value of the Business {rejectionTypeText} element{phaseStatusText}.";
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void UpdateNCTSHeader(Cc056CType messageObject)
		{
			base.UpdateNCTSHeader(messageObject);
			if (IsDeclarationAcknowledged)
			{
				var moveHeader = NctsHeaderItem.MovementHeader;
				moveHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.Unknown;
				if (IsRegenerateLrnRequired)
				{
					moveHeader.PrepareForLrnRegeneration();
				}
			}
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc056CType messageObject, EDIMessage inboundMessage)
		{
			var rejectionType = messageObject.TransitOperation?.BusinessRejectionType;
			if (rejectionType == "015")
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(inboundMessage, NctsHeaderItem.GetOutgoingMessage(inboundMessage), EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			}
		}

		protected override ZString GetMessageInterpretation(Cc056CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append($"Declaration received an error for type {new ZString(messageObject.TransitOperation?.BusinessRejectionType)} on {GetReadableDateAndTime(messageObject.TransitOperation?.RejectionDateAndTime)}");
			var rejectionCodeDescription = new RejectionCodes().GetMultilingualDescriptionFromCode(messageObject.TransitOperation?.RejectionCode);
			note.Append($"Reason: {messageObject.TransitOperation?.RejectionCode} {rejectionCodeDescription} {messageObject.TransitOperation?.RejectionReason}");

			foreach (var functionalError in messageObject.FunctionalError)
			{
				var functionalErrorCodeDescription = new FunctionalErrorCodes().GetMultilingualDescriptionFromCode(GetXmlRepresentation(functionalError.ErrorCode));
				note.Append(string.Empty);
				note.Append($"Functional error code: {GetXmlRepresentation(functionalError.ErrorCode)} {functionalErrorCodeDescription}");
				note.Append($"Reason: {GetFunctionalErrorLink(functionalError.ErrorReason)}");
				note.Append($"Attribute: {functionalError.ErrorPointer}");
				note.Append($"Element in declaration now contains the value: {functionalError.OriginalAttributeValue}");
			}

			if (IsDeclarationAcknowledged && IsRegenerateLrnRequired)
			{
				note.Append("LRN has been reset to allow resubmission");
			}

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected string GetFunctionalErrorLink(ZString errorReason)
		{
			var result = string.Empty;
			var rules = new[] { "B", "C", "E", "G", "R", "S" };

			if (!errorReason.IsEmpty)
			{
				if (rules.Any(r => errorReason.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
				{
					result = errorReason.StartsWith("BR") ? errorReason : string.Format(CultureInfo.InvariantCulture, "<a href=\"https://developer.service.hmrc.gov.uk/guides/ctc-traders-phase5-tis/documentation/rules-{0}.html#{1}\" target=\"_new\">{2}</a>", errorReason.SubstringSafe(0, 1).ToLower(), errorReason.ToLower(), errorReason);
				}
			}

			return result;
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
