using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC009C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class CC009CMessageProcessor : NCTSMessageProcessor<ICC009CDataProvider>
	{
		public CC009CMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => BEIncomingMessageTypes.Descriptions.CC009C;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC009C };

		protected override Type MessageInterpreterType => typeof(CC009CMessageInterpreter);

		protected override ICC009CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc009CType, CC009CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC009CDataProvider messageDataProvider) =>
			NctsMessageHelper.LocateHeaderByLRNOrMRN(message.Factory, messageDataProvider, NctsTypeOfAdditionalDeclarationList.Codes.D)?.MovementHeader;

		protected override void ProcessMessageCore(BEMessage message, ICC009CDataProvider messageDataProvider)
		{
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = movementHeader.Header;
			var originalPhase = movementHeader.BM_Phase;
			var originalMessageStatus = nctsHeader.EffectiveMessageStatus;
			var originCustomsStatus = movementHeader.BM_CustomsStatus;

			IReadOnlyList<ZString> customsStatusListForCancelled =
			[
				NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
				NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry,
				NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
				NCTS5DepartureCustomsStatusList.Codes.Acknowledged
			];

			if (messageDataProvider.Decision && customsStatusListForCancelled.Contains(originCustomsStatus))
			{
				movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.Cancelled;
				movementHeader.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomsEntryStatus, eventTime: ZDateTimeOffset.Now, reference: NCTS5DepartureCustomsStatusList.Codes.Cancelled), true);
			}

			if (messageDataProvider.InitiatedByCustoms || ((originalMessageStatus == LogicalStatusList.Codes.Sent || originalMessageStatus == LogicalStatusList.Codes.Acknowledged)
				&& originalPhase == NctsMovementHeaderTransactionStatusList.Codes.Cancellation))
			{
				nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
				movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			}

			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC009CDataProvider messageDataProvider)
		{
			if (messageDataProvider.Decision)
			{
				var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
				movementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
			}
		}
	}
}
