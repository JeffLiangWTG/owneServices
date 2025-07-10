using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	///	<summary>
	///	Processes queued received files
	///	</summary>
	public class AUCMessageProcessor : BranchMessageProcessor, ICustomsServiceTaskProcess
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			return new List<ApplicationTypeMessageProcessor>()
			{
				new CMRAllMessageProcessor(Logger),
				new MessageProcessorFactory(Logger),
				new EXDOCApplicationTypeMessageProcessor(Logger),
				new NEXDOCApplicationTypeMessageProcessor(Logger),
				new COLSApplicationTypeMessageProcessor(Logger)
			};
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;
	}
}
