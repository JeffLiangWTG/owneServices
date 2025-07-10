using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	class ShelfStatusesTest : TestCase
	{
		public void TestShelfStatuses()
		{
			AssertEquals("CheckedIn", "CIN", ShelfStatuses.CheckedIn);
			AssertEquals("Rejected", "REJ", ShelfStatuses.Rejected);
			AssertEquals("Finished", "BIM", ShelfStatuses.BuildIsImported);
			AssertEquals("CheckedInAndNotified", "CNF", ShelfStatuses.CheckedInAndNotified);
			AssertEquals("RejectedAndNotified", "RNF", ShelfStatuses.RejectedAndNotified);
			AssertEquals("Passed", "PAS", ShelfStatuses.Passed);
			AssertEquals("PassedAndNotified", "PAN", ShelfStatuses.PassedAndNotified);
			AssertEquals("QueuedForBranchDetection", "QBD", ShelfStatuses.QueuedForBranchDetection);
			AssertEquals("DeploymentJobFailed", "DJF", ShelfStatuses.DeploymentJobFailed);
			AssertEquals("DeploymentJobFailedAndNotified", "DNF", ShelfStatuses.DeploymentJobFailedAndNotified);
		}
	}
}