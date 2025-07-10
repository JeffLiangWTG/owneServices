using CargoWise.Customs.FR.MessageDefinitions.TP5.CCF03C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CCF03CProcessor : TP5BaseProcessor<Ccf03CType>
	{
		public CCF03CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledgeF03;

		protected override ZString GetMRNFromResponseMessage(Ccf03CType messageObject) => messageObject.TransitOperation.Mrn;

		protected override ZString GetNewDepartureStatus(Ccf03CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

		protected override ZString GetNewMessageStatus(Ccf03CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Ccf03CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Arrival;

		protected override ZDateTime GetEntryDate(Ccf03CType messageObject) => messageObject.TransitOperation?.DeclarationAcceptanceDate ?? ZDateTime.Empty;
	}
}
