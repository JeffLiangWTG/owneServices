using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.ICS
{
	public class ICSIncomingMessageProcessor : BranchMessageProcessor
	{
		public ICSIncomingMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new IcsNorthernIrelandResponseMessageProcessor(Logger));
			result.Add(new CC304AResponseMessageProcessor(Logger));
			result.Add(new CC305AResponseMessageProcessor(Logger));
			result.Add(new CC316AResponseMessageProcessor(Logger));
			result.Add(new CC324AResponseMessageProcessor(Logger));
			result.Add(new CC325AResponseMessageProcessor(Logger));
			result.Add(new CC328AResponseMessageProcessor(Logger));
			result.Add(new CC351AResponseMessageProcessor(Logger));
			return result;
		}
	}
}
