using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	///	<summary>
	///	Processes queued received files
	///	</summary>
	public class AUCCRSMessageProcessor : BranchMessageProcessor, ICustomsServiceTaskProcess
	{
		public const string LockKeyPrefix = "LMQ_AU:";

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = new List<ApplicationTypeMessageProcessor>();
			result.Add(new CMRCRSMessageProcessor(Logger));
			return result;
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;

		protected override DisposableBatch DequeueMessages(ZQuery query)
		{
			AddApplicationCodeFilters(query);

			var lockMechanism = new SqlAppLockMechanism(LockKeyPrefix);
			var dequeuer = new LockingMessageDequeuer(lockMechanism, Logger, new CMRMessageKeySetExtractor());
			return dequeuer.DequeueMessages(GetNewFactory(), query);
		}
	}
}
