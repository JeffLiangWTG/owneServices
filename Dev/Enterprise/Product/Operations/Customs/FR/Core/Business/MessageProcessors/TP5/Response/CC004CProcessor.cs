using CargoWise.Customs.FR.MessageDefinitions.TP5.CC004C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC004CProcessor : TP5BaseProcessor<Cc004CType>
	{
		public CC004CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.DeclarationAcceptance;

		protected override ZString GetMRNFromResponseMessage(Cc004CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc004CType messageObject) => LogicalStatusList.Codes.Accepted;

		protected override ZString GetNewPhase(Cc004CType messageObject) => NctsMovementHeaderTransactionStatusList.Codes.Declaration;
	}
}
