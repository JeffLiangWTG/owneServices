using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC928CProcessor : TP5BaseProcessor<Cc928CType>
	{
		public CC928CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.PositiveAcknowledge928;

		protected override ZString GetNewMessageStatus(Cc928CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewDepartureStatus(Cc928CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;

		protected override ZString GetNewPhase(Cc928CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
