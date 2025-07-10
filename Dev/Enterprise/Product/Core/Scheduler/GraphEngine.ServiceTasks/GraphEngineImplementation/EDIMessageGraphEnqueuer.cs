using System;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public class EDIMessageGraphEnqueuer<TMessage, TQueueState> : GrEngineEnqueuer<TMessage, TQueueState>
		where TMessage : EDIMessage
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public EDIMessageGraphEnqueuer(GrEngineServiceSetup<TQueueState> setup, GrEngineDequeuer<TQueueState> dequeuer)
			: base(setup, dequeuer)
		{
		}

		protected override ZString GetMessageNumber(TMessage message)
		{
			return message.EM_MessageNum;
		}

		protected override string[] GetKeys(TMessage b)
		{
			return new string[] { b.EM_MessageOwner }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
		}

		protected override ITableSchema GetSchema()
		{
			return EDIMessageSchema.Instance;
		}
	}
}
