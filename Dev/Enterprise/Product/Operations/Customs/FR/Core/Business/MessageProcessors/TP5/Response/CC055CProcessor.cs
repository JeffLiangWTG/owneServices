using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.TP5.CC055C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC055CProcessor : TP5BaseProcessor<Cc055CType>
	{
		public CC055CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.GuaranteeNotValid;

		protected override ZString GetMRNFromResponseMessage(Cc055CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc055CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

		protected override ZString GetNewMessageStatus(Cc055CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc055CType messageObject) => EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NctsHeader header, Cc055CType messageObject, EDIMessage incomingMessage)
		{
			var invalidGuaranteesFromMessage = messageObject?.GuaranteeReference;
			var appId = FRPermitHelper.GetPermitAppIdForMessage(incomingMessage);
			foreach (var messageGuarantee in invalidGuaranteesFromMessage)
			{
				var nctsGuarantee = header.GetEffectiveGuarantees().Cast<FRNctsGuarantee>().FirstOrDefault(x => x.GuaranteeReferenceNumber == messageGuarantee.Grn);
				if (nctsGuarantee != null)
				{
					header.MovementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(appId, nctsGuarantee.CusGuarantee);
				}
			}
			header.MovementHeader.GuaranteeTransactionCoordinator.CounterBalanceTransactionsOfGuaranteesNoLongerInDeclaration(appId);
		}
	}
}
