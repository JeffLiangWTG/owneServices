using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	static class WorkQueueMembershipValidator
	{
		internal static bool CanAdd(WorkQueue queue, ProcessHeader processHeader, out string message, bool ignoreAlreadyInQueueRule = false, int sequenceNumber = 0)
		{
			foreach (var rule in GetRules(ignoreAlreadyInQueueRule))
			{
				if (!rule.CanAdd(queue, processHeader, out message))
				{
					return false;
				}
			}

			if (sequenceNumber > 0 && !IsSequenceWithinShortIntRange(sequenceNumber))
			{
				message = Res.GetString("0D424FF6-C740-49C5-880F-596B66986455", "Maximum sequence number reached!");
				return false;
			}

			message = null;
			return true;
		}

		static IEnumerable<IQueueMembershipValidationRule> GetRules(bool ignoreAlreadyInQueueRule)
		{
			if (!ignoreAlreadyInQueueRule)
			{
				yield return new AddToSameQueueMultipleTimesRule();
			}

			yield return new AddToMultipleQueuesRule();
			yield return new AddWorkflowWhenJobAlreadyInQueueRule();
			yield return new AddJobWhenWorkflowAlreadyInQueueRule();
			yield return new AddJobWhenWorkflowInAnotherQueueRule();
			yield return new AddWorkflowWhenJobAlreadyInAnotherQueueRule();
		}

		internal static bool IsSequenceWithinShortIntRange(int sequenceNumber)
		{
			return short.MinValue <= sequenceNumber && sequenceNumber <= short.MaxValue;
		}
	}
}
