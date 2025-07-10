using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class ImportDeltaCResponseMessageProcessor : BaseDeltaGResponseMessageProcessor
	{
		public ImportDeltaCResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => MessageTypeList.Codes.IMC;
	}

	public class ImportDeltaDResponseMessageProcessor : BaseDeltaGResponseMessageProcessor
	{
		public ImportDeltaDResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString MessageType => MessageTypeList.Codes.IMD;
	}
}
