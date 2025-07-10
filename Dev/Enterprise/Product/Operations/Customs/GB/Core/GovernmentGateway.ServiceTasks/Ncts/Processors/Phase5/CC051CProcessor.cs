using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc051c;
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
	class CC051CProcessor : NctsBaseProcessor<Cc051CType>
	{
		public CC051CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "No Release For Transit";

		protected override ZString LRN => null;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc051CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc051CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc051CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc051CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			var invalidCustomsStatusesForPreProcess = new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };
			if (invalidCustomsStatusesForPreProcess.Contains(moveHeader.BM_CustomsStatus))
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void AfterUpdateNCTSHeader(Cc051CType messageObject)
		{
			NctsHeaderItem.Logs.CreateRecreateOrUpdateEventLog(new EventValue(
				eventType: Events.CustomsEntryStatus,
				eventTime: new ZDateTime(messageObject.PreparationDateAndTime).ToOffset(),
				reference: NctsHeaderItem.MovementHeader.BM_Phase));
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc051CType messageObject, EDIMessage incomingMessage)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeaderItem.GetOutgoingMessage(incomingMessage),
				NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		protected override ZString GetMessageInterpretation(Cc051CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("Declaration is NOT RELEASED FOR TRANSIT AT DEPARTURE");
			var noReleaseMotivationCode = messageObject.TransitOperation?.NoReleaseMotivationCode ?? string.Empty;
			note.Append("Motivation code: " + GetNoReleaseMotivationCodeInterpretation(noReleaseMotivationCode));
			note.Append(messageObject.TransitOperation?.NoReleaseMotivationText ?? string.Empty);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		string GetNoReleaseMotivationCodeInterpretation(string noReleaseMotivationCode)
		{
			return string.IsNullOrEmpty(noReleaseMotivationCode)
				? string.Empty
				: $"{noReleaseMotivationCode} ({new NCTS5NoReleaseMotivation().GetDescriptionFromCode(noReleaseMotivationCode)})";
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
