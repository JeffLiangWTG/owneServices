using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{ 
	public class CC019CProcessor : TP5BaseProcessor<Cc019CType>
	{
		public CC019CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.DiscrepanciesAtDestination;

		protected override ZString GetMRNFromResponseMessage(Cc019CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc019CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc019CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString GetNewDepartureStatus(Cc019CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.DiscrepanciesAtDestination;
	}
}
