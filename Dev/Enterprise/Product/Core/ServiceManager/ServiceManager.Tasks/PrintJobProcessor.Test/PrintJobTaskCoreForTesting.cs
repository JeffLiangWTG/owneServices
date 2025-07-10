using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	sealed class PrintJobTaskCoreForTesting : PrintJobTaskCore
	{
		public override void RunTask(CancellationToken token) { }

		public void RunTaskForJobTypeExposed(params PrintJobType[] types)
		{
			RunTaskForJobTypes(requiresPrintServer: false, CancellationToken.None, types);
		}

		public bool TryGetJobsOfGivenTypesQueryExposed(PrintJobType[] types, out ZNonPersistentDataQuery query)
		{
			return TryGetJobsOfGivenTypesQuery(types, null, out query);
		}

		public string LockConditionOnDeliveryGroupGuid => GetLockConditionOnDeliveryGroupGuid();

		public string LockConditionOnParentGuid => GetLockConditionOnParentGuid();
	}
}
