using System;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public class EDIMessageGraphPreEnqueuer<TMessage, TQueueState> : GrEnginePreEnqueuer<TMessage, TQueueState>
		where TMessage : EDIMessage
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		public EDIMessageGraphPreEnqueuer(GrEngineServiceSetup<TQueueState> setup, Func<TimeSpan> getCriticalDurationInMilliseconds, Func<TimeSpan> getResetDurationInMilliseconds)
			: base(setup)
		{
			Monitor = new EDIMessagePerformanceMonitor(getCriticalDurationInMilliseconds(), getResetDurationInMilliseconds());
		}

		protected override PerformanceMonitor Monitor { get; }

		protected override ZString GetMessageNumber(TMessage m) => m.EM_MessageNum;
	}
}
