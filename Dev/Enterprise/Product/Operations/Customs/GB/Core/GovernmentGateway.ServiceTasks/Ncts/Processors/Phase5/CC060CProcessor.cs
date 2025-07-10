using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc060c;
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
	public class CC060CProcessor : NctsBaseProcessor<Cc060CType>
	{
		public CC060CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Control Decision Notification";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc060CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc060CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc060CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewPhase(Cc060CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override ZString GetNewDeclarationStatus(Cc060CType messageObject)
		{
			switch (messageObject.TransitOperation?.NotificationType)
			{
				case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
					return NCTS5DepartureCustomsStatusList.Codes.DecisionToControl;
				case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
					return NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest;
				case NCTS5NotificationTypes.Codes.IntentionToControl:
					return NCTS5DepartureCustomsStatusList.Codes.IntentionToControl;
			}
			return ZString.Empty;
		}

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			IReadOnlyList<ZString> allowedStatuses = new List<ZString> { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl };
			if (!allowedStatuses.Contains(moveHeader.BM_CustomsStatus))
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void AfterUpdateNCTSHeader(Cc060CType messageObject)
		{
			var moveHeader = NctsHeaderItem.MovementHeader;
			if (moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.DecisionToControl ||
				moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest ||
				moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.IntentionToControl)
			{
				var type = string.Empty;
				var service = NctsHeaderItem.Services.AddNew();
				service.ES_ServiceCode = Constants.ServiceTypes.CTL;
				service.ES_Booked = messageObject.TransitOperation?.ControlNotificationDateAndTime ?? ZDateTime.Empty;
				service.ES_References = MRN;
				switch (messageObject.TransitOperation?.NotificationType)
				{
					case NCTS5NotificationTypes.Codes.DecisionToControlAndRequestedDocumentsIfNeeded:
						service.ES_ServiceNote = "Intention to control. Type of Controls: ";
						break;
					case NCTS5NotificationTypes.Codes.AdditionalDocumentsRequest:
						service.ES_ServiceNote = "Decision to control. Type of Controls: ";
						break;
					case NCTS5NotificationTypes.Codes.IntentionToControl:
						service.ES_ServiceNote = "Additional document request. Type of Controls: ";
						break;
				}
				foreach (var typeOfControl in messageObject.TypeOfControls)
				{
					type = typeOfControl.Type;
					service.ES_ServiceNote = $"{service.ES_ServiceNote} {new NCTS5TypeOfControlTypes().GetDescriptionFromCode(type)} {typeOfControl.Text}";
				}
			}
		}

		protected override ZString GetMessageInterpretation(Cc060CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			ZString notificationType = messageObject.TransitOperation?.NotificationType;
			var typeOfControlTypes = new NCTS5TypeOfControlTypes();
			note.Append("New Customs Status: Decision to Control Notification");
			note.Append($"Status granted on {GetReadableDateAndTime(messageObject.TransitOperation?.ControlNotificationDateAndTime)}");
			note.Append($"Type of Notification: {notificationType} {new NCTS5NotificationTypes().GetDescriptionFromCode(notificationType)}");
			note.Append("");
			foreach (var typeOfControl in messageObject.TypeOfControls)
			{
				note.Append($"Type of Control {typeOfControl.SequenceNumber}: {typeOfControl.Type} {typeOfControlTypes.GetDescriptionFromCode(typeOfControl.Type)} {typeOfControl.Text}");
			}
			foreach (var requestedDocument in messageObject.RequestedDocument)
			{
				note.Append($"Document {requestedDocument.SequenceNumber}: {requestedDocument.DocumentType} {requestedDocument.Description}");
			}

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
