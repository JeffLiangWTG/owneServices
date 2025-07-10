using CargoWise.Customs.FR.MessageDefinitions.TP5.CD906C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	internal class CD906CProcessor : TP5BaseProcessor<Cd906CType>
	{
		public CD906CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.FunctionalRejection;

		protected override ZString GetMRNFromResponseMessage(Cd906CType messageObject) => messageObject.Header?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cd906CType messageObject) => LogicalStatusList.Codes.Error;
	}
}
