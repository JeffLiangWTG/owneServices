using CargoWise.Customs.FR.MessageDefinitions.TP5.CC140C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC140CProcessor : TP5BaseProcessor<Cc140CType>
	{
		public CC140CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RequestOnNonArrivedMovement;

		protected override ZString GetMRNFromResponseMessage(Cc140CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc140CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc140CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;

		protected override ZString GetNewDepartureStatus(Cc140CType messageObject) => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;
	}
}
