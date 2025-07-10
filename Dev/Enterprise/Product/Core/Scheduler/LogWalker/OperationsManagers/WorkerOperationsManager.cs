using System.Collections.Generic;

namespace Enterprise.LogWalker
{
	class WorkerOperationsManager : OperationsManager
	{
		protected override IEnumerable<ILogWalkerOperation> GetOrderedOperations()
		{
			yield return GetNewBroadcaster();
		}

		protected virtual NewsBroadcaster GetNewBroadcaster() => new NewsBroadcaster();
	}
}
