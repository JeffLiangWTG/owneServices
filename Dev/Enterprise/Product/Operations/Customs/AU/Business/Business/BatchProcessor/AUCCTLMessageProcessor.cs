using System.Collections.Generic;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	///	<summary>
	///	Processes queued received files
	///	</summary>
	public class AUCCTLMessageProcessor : BaseMessageProcessor, ICustomsServiceTaskProcess
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = base.GetMessageProcessors();
			result.Add(new CMRCTLMessageProcessor(Logger));
			return result;
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;
	}
}
