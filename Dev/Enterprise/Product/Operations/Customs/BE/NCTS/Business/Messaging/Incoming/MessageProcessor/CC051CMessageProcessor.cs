using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC051C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC051CMessageProcessor : NCTSMessageProcessor<ICC051CDataProvider>
	{
		public CC051CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC051C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC051C };

		protected override Type MessageInterpreterType => typeof(CC051CMessageInterpreter);

		protected override ICC051CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc051CType, CC051CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC051CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC051CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			if (!(new ZString[] { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, NCTS5DepartureCustomsStatusList.Codes.DecisionToControl, NCTS5DepartureCustomsStatusList.Codes.AdditionalDocumentsRequest, NCTS5DepartureCustomsStatusList.Codes.IntentionToControl }).Contains(moveHeader.BM_CustomsStatus))
			{
				DiscardMessage(message, Res.GetString("6A420D48-2795-4358-AF5F-ECF9E4D14883", "The message is discarded because its ‘Status at Customs’ has already the status {3}. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to DISCARDED.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType, moveHeader.BM_CustomsStatus));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC051CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_CustomsStatus = StatusCodes.NotReleasedForExport;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC051CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			foreach (var nctsGuarantee in nctsHeader.MovementHeader.Guarantees)
			{
				if (nctsGuarantee.CusGuarantee != null)
				{
					PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Belgium, moveHeader.BM_PaperlessInbondNum, (SharedCusPermitLineTransaction x) => x.CPL_CPH_PermitHeader == nctsGuarantee.CusGuarantee.PK, PermitTransactionStatusList.Codes.Deleted);
				}
			}
		}
	}
}
