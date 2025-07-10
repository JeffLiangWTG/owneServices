using CargoWise.Customs.FR.MessageDefinitions.TP5.CC035C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC035CProcessor : TP5BaseProcessor<Cc035CType>
	{
		public CC035CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RecoveryNotification;

		protected override ZString GetMRNFromResponseMessage(Cc035CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc035CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.UnderRecoveryProcedure;

		protected override ZString GetNewMessageStatus(Cc035CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc035CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
