using System;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class NewWorkItemLookupsTest : WorkItemLookupsTest<NewWorkItem>
	{
		public void TestActiveReviewTasks()
		{
			var reviewTasks = new CodeDescriptionBoolCollection();

			reviewTasks.Add("AAA", (NoResString)"active review task 1", true);
			reviewTasks.Add("BBB", (NoResString)"inactive review task 1", false);
			reviewTasks.Add("CCC", (NoResString)"active review task 2", true);
			reviewTasks.Add("DDD", (NoResString)"inactive review task 2", false);

			EDIDataRegistry.Instance.ReviewTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reviewTasks);

			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			CodeDescriptionPairList activeReviewTasks = workItem.Lookups.ActiveReviewTasks;

			AssertEquals(2, activeReviewTasks.Count);
			AssertEquals(true, activeReviewTasks.ContainsCode("AAA"));
			AssertEquals(true, activeReviewTasks.ContainsCode("CCC"));
			AssertEquals(false, activeReviewTasks.ContainsCode("BBB"));
			AssertEquals(false, activeReviewTasks.ContainsCode("DDD"));
		}
	}
}