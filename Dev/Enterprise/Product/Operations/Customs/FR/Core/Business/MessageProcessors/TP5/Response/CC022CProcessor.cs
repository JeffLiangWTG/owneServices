using CargoWise.Customs.FR.MessageDefinitions.TP5.CC022C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC022CProcessor : TP5BaseProcessor<Cc022CType>
	{
		public CC022CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.NotificationToAmendDeclaration;

		protected override ZString GetMRNFromResponseMessage(Cc022CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc022CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;

		protected override ZString GetNewMessageStatus(Cc022CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc022CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
