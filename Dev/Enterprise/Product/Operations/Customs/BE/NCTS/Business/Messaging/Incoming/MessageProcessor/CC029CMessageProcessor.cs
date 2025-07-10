using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC029C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC029CMessageProcessor : NCTSMessageProcessor<ICC029CDataProvider>
	{
		public CC029CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC029C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC029C };

		protected override ICC029CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc029CType, CC029CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC029CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRNOrMRN(message.Factory, messageDataProvider, NctsMoveHeaderType.Codes.Departure)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC029CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var invalidCustomsStatusesForPreProcess = new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Cancelled, NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested };

			if (invalidCustomsStatusesForPreProcess.Contains(moveHeader.BM_CustomsStatus))
			{
				DiscardMessage(message, Res.GetString("62D0E08B-AE4C-48AE-B714-ACD161F9F9D5", "The message was discarded, Status at Customs cannot be one of these statuses: {0}. (Interchange Number:{1}, Number:{2}, Type:{3}); message status set to ERROR.", string.Join("/", invalidCustomsStatusesForPreProcess), message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			}
		}

		protected override Type MessageInterpreterType => typeof(CC029CMessageInterpreter);

		protected override void ProcessMessageCore(BEMessage message, ICC029CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			moveHeader.CustomsEntryStatusLogAdded += MovementHeader_CustomsEntryStatusLogAdded;

			var movementReferenceNumber = nctsHeader.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = messageDataProvider.MRN;
			movementReferenceNumber.CE_IssueDate = messageDataProvider.ReleaseDate;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
			{
				moveHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			}
			moveHeader.BM_EntryDate = messageDataProvider.DeclarationAcceptanceDate;
			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

			NctsMessageHelper.RequestTADFromCustoms(nctsHeader);
		}

		void MovementHeader_CustomsEntryStatusLogAdded(object sender, EventArgs e)
		{
			if (sender is NctsDepartureMovementHeader moveHeader)
			{
				moveHeader.Header.LockFileIfEnabledByConfiguration(Res.GetString("B57FA72B-FA9E-4DDD-B705-B0AB99A9A4C6", "The tabs are locked for editing because a Release for Transit was received."), EUJobMessageTypeList.Codes.NctsDeparture);
				moveHeader.CustomsEntryStatusLogAdded -= MovementHeader_CustomsEntryStatusLogAdded;
			}
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC029CDataProvider messageDataProvider)
		{
			Customs.Business.PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Belgium, ((NctsDepartureMovementHeader)message.EM_LinkedObject).BM_PaperlessInbondNum, null, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
		}

		protected override bool CheckMessageSequenceIsValidCore(BEMessage message) => !(message.EM_LinkedObject is NctsDepartureMovementHeader movementHeader)
			|| movementHeader.BM_Phase != NctsMovementHeaderTransactionStatusList.Codes.Declaration || (movementHeader.BM_CustomsStatus != ZString.Empty
			&& movementHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.Acknowledged && movementHeader.BM_CustomsStatus != NCTS5DepartureCustomsStatusList.Codes.PreLodged);
	}
}
