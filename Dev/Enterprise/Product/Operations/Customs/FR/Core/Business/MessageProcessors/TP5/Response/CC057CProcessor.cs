using CargoWise.Customs.FR.MessageDefinitions.TP5.CC057C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC057CProcessor : TP5BaseProcessor<Cc057CType>
	{
		public CC057CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDestination;

		protected override ZString GetMRNFromResponseMessage(Cc057CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc057CType messageObject) => LogicalStatusList.Codes.Invalid;
	}
}
