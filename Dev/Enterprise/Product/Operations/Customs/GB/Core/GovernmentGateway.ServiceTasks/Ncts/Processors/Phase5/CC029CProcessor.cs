using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase5.cc029c;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5
{
	class CC029CProcessor : NctsBaseProcessor<Cc029CType>
	{
		public CC029CProcessor(ILogger serviceLogger, LoggingInformation loggingInformation) : base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => "Release For Transit";

		protected override ZString LRN => MessageObject?.TransitOperation?.Lrn;

		protected override ZString MRN => MessageObject?.TransitOperation?.Mrn;

		protected override string GetMessageId(Cc029CType messageObject) => messageObject.MessageIdentification;
		protected override string GetMessageTypeCode(Cc029CType messageObject) => messageObject.MessageType.ToString();
		protected override ZString GetNewMessageStatus(Cc029CType messageObject) => LogicalStatusList.Codes.Accepted;
		protected override ZString GetNewDeclarationStatus(Cc029CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
		protected override ZString GetNewPhase(Cc029CType messageObject) => NCTS5DeparturePhaseList.Codes.Declaration;

		protected override bool MessageShouldBeDiscardedCore(NctsHeader nctsHeader, out string discardReasonText)
		{
			var shouldDiscard = base.MessageShouldBeDiscardedCore(nctsHeader, out discardReasonText);
			var moveHeader = nctsHeader.MovementHeader;

			var invalidCustomsStatusesForPreProcess = new ZString[] { NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };
			if (invalidCustomsStatusesForPreProcess.Contains(moveHeader.BM_CustomsStatus))
			{
				discardReasonText = BecauseWrongCustomsStatusReasonText(moveHeader.BM_CustomsStatus);
				shouldDiscard = true;
			}

			return shouldDiscard;
		}

		protected override void AfterUpdateNCTSHeader(Cc029CType messageObject)
		{
			var moveHeader = NctsHeaderItem.MovementHeader;

			var movementReferenceNumber = NctsHeaderItem.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = MRN;
			var releaseDate = messageObject.TransitOperation?.ReleaseDate ?? ZDateTime.Empty;
			movementReferenceNumber.CE_IssueDate = releaseDate;

			if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
			{
				moveHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			}
			moveHeader.BM_EntryDate = messageObject.TransitOperation?.DeclarationAcceptanceDate ?? ZDateTime.Empty;

			var preparationDate = messageObject.PreparationDateAndTime;
			var eventDateTime = new ZDateTime(releaseDate.Year, releaseDate.Month, releaseDate.Day, preparationDate.Hour, preparationDate.Minute, preparationDate.Second);
			NctsHeaderItem.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: eventDateTime.ToOffset(), reference: moveHeader.BM_CustomsStatus));
			NctsHeaderItem.LockFileIfEnabledByConfiguration(ZString.Format("The tabs are locked for editing because a Release for Transit was received."), EUJobMessageTypeList.Codes.NctsDeparture);
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(Cc029CType messageObject, EDIMessage incomingMessage)
		{
			if (NctsHeaderItem != null)
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(incomingMessage, NctsHeaderItem.GetOutgoingMessage(incomingMessage), NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.UnitedKingdom, messageOnly: false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				GBPermitHelper.UpdateGuaranteeAndPwBondAmount(NctsHeaderItem, incomingMessage, GetGuaranteeReferenceAmountToBeCovered(messageObject));
			}
		}

		protected override void GenerateDocuments(Cc029CType messageObject, EDIMessage inboundMessage)
		{
			var linkedObject = inboundMessage.EM_LinkedObject;
			var header = linkedObject as NctsHeader ?? (linkedObject as Business.NctsDepartureMovementHeader)?.Header;
			if (header != null)
			{
				var nctsMessage = inboundMessage.Factory.Load<NctsEdiMessage>(inboundMessage.PK);
				new NctsTadEdocSaver(header).RenderTadAndStoreInEdocs(nctsMessage);
			}
		}

		protected override ZString GetMessageInterpretation(Cc029CType messageObject, EDIMessage inboundMessage)
		{
			var note = new ZStringBuilder();

			note.Append("New detailed status: Goods Released for Transit at Departure");
			note.Append("Status granted on " + GetReadableDate(messageObject.TransitOperation?.ReleaseDate));
			note.Append("Acceptance Date " + GetReadableDate(messageObject.TransitOperation?.DeclarationAcceptanceDate));

			return note.ToStringWithDelimiterBetweenAppends("</br>");
		}

		internal override ZString AcceptMovementType => NctsMovementType.Codes.Departure;

		protected override ZString CorrelationIdentifier => MessageObject?.CorrelationIdentifier;

		Money GetGuaranteeReferenceAmountToBeCovered(Cc029CType messageObject)
		{
			var guarantee = messageObject?.Guarantee?.FirstOrDefault();
			if (guarantee != null)
			{
				var guaranteeReference = guarantee?.GuaranteeReference?.FirstOrDefault();
				if (guaranteeReference != null)
				{
					return new Money(guaranteeReference.AmountToBeCovered, new ZArchitecture.Environment.Currency(guaranteeReference.Currency));
				}
			}

			return null;
		}
	}
}
