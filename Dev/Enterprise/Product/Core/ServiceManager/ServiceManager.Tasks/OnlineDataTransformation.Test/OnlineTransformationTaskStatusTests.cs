using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	sealed class OnlineTransformationTaskStatusTests : TestCase
	{
		public void TestOnlineTransformationTaskStatus_AcceptsOnlyUniqueCompletedTasks()
		{
			var completed = new List<string>() { "a", "b" };
			new OnlineTransformationTaskStatus(completed, new List<string>());

			completed.Add("a");
			var status = new OnlineTransformationTaskStatus(completed, new List<string>());
			AssertContainsExactElementsInAnyOrder(new[] { "a", "b" }, status.Completed);
		}

		public void TestOnlineTransformationTaskStatus_AcceptsOnlyUniquePendingTasks()
		{
			var pending = new List<string>() { "a", "b" };
			new OnlineTransformationTaskStatus(new List<string>(), pending);

			pending.Add("a");
			var status = new OnlineTransformationTaskStatus(new List<string>(), pending);
			AssertContainsExactElementsInAnyOrder(new[] { "a", "b" }, status.Pending);
		}
	}
}
