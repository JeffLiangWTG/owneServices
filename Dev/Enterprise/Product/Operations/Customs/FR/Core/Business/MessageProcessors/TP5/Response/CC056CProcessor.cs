using CargoWise.Customs.FR.MessageDefinitions.TP5.CC056C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC056CProcessor : TP5BaseProcessor<Cc056CType>
	{
		public CC056CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.RejectionFromOfficeOfDeparture;

		protected override ZString GetMRNFromResponseMessage(Cc056CType messageObject) => messageObject.TransitOperation?.Mrn ?? ZString.Empty;

		protected override ZString GetNewMessageStatus(Cc056CType messageObject) => LogicalStatusList.Codes.Invalid;
	}
}
