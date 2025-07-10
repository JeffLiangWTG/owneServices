using CargoWise.Customs.FR.MessageDefinitions.TP5.CC028C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC028CProcessor : TP5BaseProcessor<Cc028CType>
	{
		public CC028CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.MrnAllocated;

		protected override ZString GetMRNFromResponseMessage(Cc028CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc028CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc028CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZDateTime GetEntryDate(Cc028CType messageObject) => messageObject.TransitOperation?.DeclarationAcceptanceDate ?? ZDateTime.Empty;

		protected override ZString GetNewDepartureStatus(Cc028CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
	}
}
