using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC055C;
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
	public class CC055CMessageProcessor : NCTSMessageProcessor<ICC055CDataProvider>
	{
		public CC055CMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => Res.GetString("FAF1E77B-054C-4B19-BE64-FD4443FA50D7", "CC055C Customs Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageTypes.Codes.CC055C };

		protected override Type MessageInterpreterType => typeof(CC055CMessageInterpreter);

		protected override ICC055CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc055CType, CC055CDataProvider>();

		protected override BusinessObject FindParentOfMessage(BEMessage message, ICC055CDataProvider messageDataProvider) => NctsMessageHelper.LocateHeaderByMRN(message.Factory, messageDataProvider)?.MovementHeader;

		protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC055CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;
			IReadOnlyList<ZString> allowedStatuses = new List<ZString> { NCTS5DepartureCustomsStatusList.Codes.Acknowledged, NCTS5DepartureCustomsStatusList.Codes.PreLodged, NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested, NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid };

			if (!allowedStatuses.Contains(moveHeader.BM_CustomsStatus) || moveHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid && (moveHeader.BM_Phase != NctsMovementHeaderTransactionStatusList.Codes.Amendment || !new ZString[] { LogicalStatusList.Codes.Sent, LogicalStatusList.Codes.Acknowledged }.Contains(nctsHeader.EffectiveMessageStatus)))
			{
				Logger.LogError(Res.GetString("92D8A4E5-45BE-46C8-AA56-1E78D457F0D4", "The message is discarded because its ‘Status at Customs’ does not have the status ACK, PRE, MRN, AMR or GIV. In Case of GIV, the phase status should be 013 and message status SNT or ACK. (Interchange Number:{0}, Number:{1}, Type:{2}); message status set to ERROR.", message.EM_InterchangeNumber, message.EM_MessageNum, message.EM_MessageType));
				message.EM_Status = EDIMessage.Status.Discarded;
				message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, Res.GetString("58D9D820-FD02-4B26-963F-D4334977BEED", "The message with interchange {0} is discarded because its ‘Status at Customs’ is not ACK, PRE, MRN, AMR or GIV. In Case of GIV, the phase status should be 013 and message status SNT or ACK.", message.EM_InterchangeNumber));
			}
		}

		protected override void ProcessMessageCore(BEMessage message, ICC055CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			moveHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;
			moveHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			message.EM_Status = EDIMessage.Status.ProcessedOK;

			nctsHeader.Logs.AddNew(AutoEvents.CustomsCleared, moveHeader.BM_CustomsStatus, ZDateTimeOffset.Now);
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(BEMessage message, ICC055CDataProvider messageDataProvider)
		{
			var moveHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var nctsHeader = moveHeader.Header;

			foreach (var guaranteeReference in messageDataProvider.GuaranteeReferences)
			{
				if (guaranteeReference.InvalidGuaranteeReason.Any(x => x.Code.In(new string[] { NCTS5InvalidGuaranteeReason.Codes.G01, NCTS5InvalidGuaranteeReason.Codes.G02, NCTS5InvalidGuaranteeReason.Codes.G05, NCTS5InvalidGuaranteeReason.Codes.G08, NCTS5InvalidGuaranteeReason.Codes.G09, NCTS5InvalidGuaranteeReason.Codes.G10, NCTS5InvalidGuaranteeReason.Codes.G12 })))
				{
					foreach (var nctsGuarantee in nctsHeader.MovementHeader.Guarantees.Where(x => x.PW_BondNumber == guaranteeReference.GRN))
					{
						Func<SharedCusPermitLineTransaction, bool> additionalCriteria = (SharedCusPermitLineTransaction x) => x.CPL_CPH_PermitHeader == nctsGuarantee.CusGuarantee.PK;
						PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(message.Factory, Core.Constants.CountryCodes.Belgium, ((NctsDepartureMovementHeader)message.EM_LinkedObject).BM_PaperlessInbondNum, additionalCriteria, PermitTransactionStatusList.Codes.Deleted);
					}
				}
			}
		}
	}
}
