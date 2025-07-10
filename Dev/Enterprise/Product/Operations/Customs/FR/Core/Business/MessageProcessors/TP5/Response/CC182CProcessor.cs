using CargoWise.Customs.FR.MessageDefinitions.TP5.CC182C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC182CProcessor : TP5BaseProcessor<Cc182CType>
	{
		public CC182CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.ForwardedIncidentNotificationToEd;

		protected override ZString GetMRNFromResponseMessage(Cc182CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewDepartureStatus(Cc182CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;

		protected override ZString GetNewMessageStatus(Cc182CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc182CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
