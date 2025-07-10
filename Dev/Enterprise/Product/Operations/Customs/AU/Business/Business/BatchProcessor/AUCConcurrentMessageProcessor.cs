using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	///	<summary>
	///	Processes queued received files
	///	</summary>
	public class AUCConcurrentMessageProcessor : BranchMessageProcessor, ICustomsServiceTaskProcess
	{
		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			List<ApplicationTypeMessageProcessor> result = new List<ApplicationTypeMessageProcessor>();
			result.Add(new CMRConcurrentMessageProcessor(Logger));
			return result;
		}

		protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;

		protected override DisposableBatch DequeueMessages(ZQuery query)
		{
			AddApplicationCodeFilters(query);

			var lockMechanism = GetLockMechanism();
			var dequeuer = new LockingMessageDequeuer(lockMechanism, Logger, new CMRMessageKeySetExtractor());
			return dequeuer.DequeueMessages(GetNewFactory(), query);
		}

		private protected virtual ILockMechanism GetLockMechanism()
		{
			return new SqlAppLockMechanism(AUCCRSMessageProcessor.LockKeyPrefix);
		}
	}
}
