using CargoWise.Customs.FR.MessageDefinitions.TP5.CC917C;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class CC917CProcessor : TP5BaseProcessor<Cc917CType>
	{
		public CC917CProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString GetMessageSubType() => TP5ResponseMessageSubTypeList.Codes.XmlNack;

		protected override ZString GetNewMessageStatus(Cc917CType messageObject) => LogicalStatusList.Codes.Error;
	}
}
