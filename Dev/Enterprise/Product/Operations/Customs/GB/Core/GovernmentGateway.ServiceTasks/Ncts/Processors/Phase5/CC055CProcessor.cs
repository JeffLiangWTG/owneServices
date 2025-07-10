using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc055c;
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
	class CC055CProcessor : NctsBaseProcessor<Cc055CType>
	{
		public CC055CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Guarantee Not Valid";

		protected override ZString LRN => null;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc055CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc055CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc055CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc055CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
		protected override ZString GetNewPhase(Cc055CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			IReadOnlyList<ZString> allowedStatuses = new List<ZString> { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };
			if (!allowedStatuses.Contains(moveHeader.BM_CustomsStatus))
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc055CType messageObject, EDIMessage incomingMessage)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeaderItem.GetOutgoingMessage(incomingMessage),
				NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		protected override ZString GetMessageInterpretation(Cc055CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("Guarantee invalid");

			var list = new NCTS5InvalidGuaranteeReason();
			foreach (var guaranteeReference in messageObject.GuaranteeReference)
			{
				note.Append("Guarantee sequence: " + guaranteeReference.SequenceNumber);
				note.Append("GRN " + guaranteeReference.Grn);
				foreach (var reason in guaranteeReference.InvalidGuaranteeReason)
				{
					note.Append("Reason : code: " + reason.Code + " " + list.GetDescriptionFromCode(reason.Code ?? string.Empty));
					note.Append("Reason : text: " + reason.Text);
					note.Append(string.Empty);
				}
			}

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
