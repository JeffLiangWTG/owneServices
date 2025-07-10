using CargoWise.Customs.FR.MessageDefinitions.TP5.CC045C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC045CProcessor : TP5BaseProcessor<Cc045CType>
	{
		public CC045CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.WriteOffNotification;

		protected override ZString GetMRNFromResponseMessage(Cc045CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc045CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

		protected override ZString GetNewMessageStatus(Cc045CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc045CType messageObject) => EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NctsHeader header, Cc045CType messageObject, EDIMessage incomingMessage)
		{
			var guarantees = header.GetEffectiveGuarantees();
			if (guarantees.Count > 0)
			{
				header.MovementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(FRPermitHelper.GetPermitAppIdForMessage(incomingMessage));
				foreach (EU.NCTS.Business.NctsGuarantee nctsGuarantee in guarantees)
				{
					if (!nctsGuarantee.PW_BondNumber.IsEmpty && nctsGuarantee.CusGuarantee == null)
					{
						var errorMessage = $"Guarantee was not written off for {header.BH_JobReference} because {nctsGuarantee.PW_BondNumber} does not refer to a guarantee managed by CW1.";
						header.Logs.AddNew(Events.DeclarationHasErrors, errorMessage);
						Logger.LogWarning(errorMessage);
					}
				}
			}
		}
	}
}
