using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business
{
	public class CAMessageProcessor : BaseMessageProcessor
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new ACIMessageProcessor(Logger));
			result.Add(new EXPMessageProcessor(Logger));
			result.Add(new IMPMessageProcessor(Logger));
			result.Add(new CACustomsMessageProcessor(Logger));
			return result;
		}

		public void ExecuteBatchForDebug()
		{
			ExecuteBatch(CancellationToken.None);
		}

		protected override ZQuery ValidBranchesForMessageFilter
		{
			get { return BatchProcessorUtilities.AllActiveComapnyBranchesMessageFilter; }
		}

		protected override bool MessageShouldBeProcessedInASeparateFactory
		{
			get { return true; }
		}

		protected override void LogSaving()
		{
		}
	}
}
