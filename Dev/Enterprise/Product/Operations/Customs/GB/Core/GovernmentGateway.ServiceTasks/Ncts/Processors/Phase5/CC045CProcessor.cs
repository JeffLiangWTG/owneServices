using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc045c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC045CProcessor : NctsBaseProcessor<Cc045CType>
	{
		public CC045CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Write-Off Notification";

		protected override ZString LRN => null;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc045CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc045CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc045CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc045CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;
		protected override ZString GetNewPhase(Cc045CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

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

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc045CType messageObject, EDIMessage incomingMessage)
		{
			var localReferenceNumber = NctsHeaderItem.MovementHeader.BM_PaperlessInbondNum;
			if (NctsHeaderItem.GetEffectiveGuarantees().Count > 0)
			{
				PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeaderItem.Messages.LastOutgoingMessage, NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, PermitTransactionStatusList.Codes.Confirmed);
				foreach (var nctsGuarantee in NctsHeaderItem.GetEffectiveGuarantees())
				{
					nctsGuarantee.CusGuarantee?.AddTransaction(localReferenceNumber, "NCTS write-off " + localReferenceNumber + " [" + NctsHeaderItem.MovementReferenceNumber + "]", incomingMessage.EM_MessageNum, ZString.Empty, nctsGuarantee.PW_BondAmount, 0, status: PermitTransactionStatusList.Codes.Confirmed);
				}
			}
		}

		protected override ZString GetMessageInterpretation(Cc045CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("Write-Off notification for NCTS departure received on " + GetReadableDate(messageObject.TransitOperation?.WriteOffDate));
			AppendGuarantorIfPresent(note, messageObject.Guarantor);

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;
	}
}
