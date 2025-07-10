using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc057c;
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
	public class CC057CProcessor : NctsBaseProcessor<Cc057CType>
	{
		public CC057CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Rejection From Office Of Destination";

		protected override ZString LRN => null;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc057CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc057CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc057CType messageObject) => LogicalStatusList.Codes.Invalid;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.ArrivalMovementHeader;

			if (moveHeader != null)
			{
				var customsStatus = moveHeader.BM_CustomsStatus;

				var customsStatusIsCorrect = false;
				switch (MessageObject.TransitOperation?.BusinessRejectionType)
				{
					case "007":
						customsStatusIsCorrect = customsStatus.IsEmpty;
						break;
					case "044":
						customsStatusIsCorrect = customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted
							|| customsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
						break;
				}

				if (!IsSentAcknowledgedOrOK(nctsHeader.EffectiveMessageStatus) || !customsStatusIsCorrect)
				{
					discardReasonText = ZString.Format("The message was discarded, because the arrival declaration with MRN {0} has the wrong Customs Status ({1}) in combination with its Message Status ({2})", MRN, customsStatus, nctsHeader.EffectiveMessageStatus);
					shouldDiscard = true;
				}
			}

			return shouldDiscard;
		}

		protected override ZString GetMessageInterpretation(Cc057CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append($"Declaration received an error for type {new ZString(messageObject.TransitOperation?.BusinessRejectionType)} on {GetReadableDateAndTime(messageObject.TransitOperation?.RejectionDateAndTime)}");
			note.Append($"Reason: {messageObject.TransitOperation?.RejectionCode} {messageObject.TransitOperation?.RejectionReason}");

			foreach (var functionalError in messageObject.FunctionalError)
			{
				note.Append(string.Empty);
				note.Append($"Functional error code: {GetXmlRepresentation(functionalError.ErrorCode)}");
				note.Append($"Reason: {functionalError.ErrorReason}");
				note.Append($"Attribute: {functionalError.ErrorPointer}");
				note.Append($"Element in declaration now contains the value: {functionalError.OriginalAttributeValue}");
			}

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Arrival;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
