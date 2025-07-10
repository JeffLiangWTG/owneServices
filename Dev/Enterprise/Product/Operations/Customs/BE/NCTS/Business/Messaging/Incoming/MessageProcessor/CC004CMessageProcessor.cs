using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC004C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC004CMessageProcessor : NCTSMessageProcessor<ICC004CDataProvider>
	{
		public CC004CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC004C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC004C };

		protected override Type MessageInterpreterType => typeof(CC004CMessageInterpreter);

		protected override ICC004CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc004CType, CC004CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC004CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByLRNOrMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC004CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			var customsStatus = moveHeader.BM_CustomsStatus;

			if (customsStatus != NCTS5DepartureCustomsStatusList.Codes.MrnAllocated && customsStatus != NCTS5DepartureCustomsStatusList.Codes.Acknowledged &&
				customsStatus != NCTS5DepartureCustomsStatusList.Codes.PreLodged && customsStatus != NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested &&
				customsStatus != NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid ||
				(customsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid && (moveHeader.BM_Phase != NctsMovementHeaderTransactionStatusList.Codes.Amendment || !nctsHeader.EffectiveMessageStatus.In<ZString>(LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged))))
			{
				DiscardMessage(message, Res.GetString("36991978-587A-478B-AD0D-2358CDD387E4", "The message was discarded, because the Status at Customs of the declaration is different from ACK, PRE, MRN, AMR or GIV. In case of GIV, phase status needs to be 013 and message status SNT or ACK (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC004CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			var movementReferenceNumber = nctsHeader.MovementReferenceEntryNumber;
			movementReferenceNumber.CE_EntryNum = messageDataProvider.MRN;

			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			if (movementReferenceNumber.CE_IssueDate.IsValid)
			{
				moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;
			}
			else
			{
				if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.D)
				{
					moveHeader.BM_CustomsStatus = string.IsNullOrEmpty(movementReferenceNumber.CE_EntryNum) ? NCTS5DepartureCustomsStatusList.Codes.PreLodged : NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
				}
				else
				{
					if (!string.IsNullOrEmpty(movementReferenceNumber.CE_EntryNum))
					{
						moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
					}
				}
			}

			if (moveHeader.BM_AdditionalDeclarationType == NctsTypeOfAdditionalDeclarationList.Codes.A)
			{
				moveHeader.BM_EntryDate = messageDataProvider.EntryDate;
			}

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC004CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			if (nctsHeader.HasLogWith(Events.CustomsEntryStatus.Code, NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit))
			{
				PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Belgium, nctsHeader.MovementHeader.BM_PaperlessInbondNum, null, PermitTransactionStatusList.Codes.Confirmed);
			}
		}
	}
}
