using System.Collections.Generic;

namespace Enterprise.LogWalker
{
	class PurgeOperationsManager : OperationsManager
	{
		protected override IEnumerable<ILogWalkerOperation> GetOrderedOperations()
		{
			yield return new CleanupLogs();
		}
	}
}
