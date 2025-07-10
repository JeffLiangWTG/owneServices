using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc182c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.NCTS;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC182CProcessor : NctsBaseProcessor<Cc182CType>
	{
		public CC182CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Forwarded Incident Notification To ED";

		protected override ZString LRN => null;
		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc182CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc182CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc182CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc182CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;
		protected override ZString GetNewPhase(Cc182CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

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

		protected override void AfterUpdateNCTSHeader(Cc182CType messageObject)
		{
			NctsHeaderItem.BH_ExportFlag = YesNoList.Codes.Yes;
		}

		protected override ZString GetMessageInterpretation(Cc182CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			var interpretation = new NCTS182EdiMessagePrettier(inboundMessage).MakeInboundPrettyForInterpretation(NctsHeaderItem);
			note.Append(interpretation);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;
	}
}
