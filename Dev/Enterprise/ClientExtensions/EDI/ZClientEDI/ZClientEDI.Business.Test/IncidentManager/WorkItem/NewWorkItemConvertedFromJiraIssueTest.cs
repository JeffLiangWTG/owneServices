using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	abstract class NewWorkItemConvertedFromJiraIssueRelatedItemTestCase : WorkItemRelatedItemTestCase
	{
	}

	[TestedType(typeof(NewWorkItemConvertedFromJiraIssue))]
	public class NewWorkItemConvertedFromJiraIssueTest : WorkItemCommonTest<NewWorkItemConvertedFromJiraIssue>
	{
		public void TestSaveUseCorrectNumberFountainNumbers()
		{
			var item1 = Factory.NewWithValidTestData<WorkItem>();
			AssertType<NewWorkItem>(item1);
			Factory.Save();

			var item2 = Factory.NewWithValidTestData<WorkItemConvertedFromJiraIssue>();
			AssertType<NewWorkItemConvertedFromJiraIssue>(item2);
			Factory.Save();

			var number = Convert.ToInt32(item1.JobNumber.Substring(2));
			var number2 = Convert.ToInt32(item2.JobNumber.Substring(2));

			AssertEquals(number + 1, number2);
		}
	}

	[TestedType(typeof(NewWorkItemConvertedFromJiraIssue))]
	sealed class NewWorkItemConvertedFromJiraIssueRelatedItemTest : NewWorkItemConvertedFromJiraIssueRelatedItemTestCase
	{
	}

	abstract class NewWorkItemConvertedFromJiraIssueRelatedItemSourceTestCase : WorkItemRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(NewWorkItemConvertedFromJiraIssue))]
	sealed class NewWorkItemConvertedFromJiraIssueRelatedItemSourceTest : NewWorkItemConvertedFromJiraIssueRelatedItemSourceTestCase
	{
	}
}
