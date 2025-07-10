using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderDbHitTest : BMSTestCaseWithFactory
	{
		[TestDate(2017, 1, 2)]
		public void TestWorkflowSaveDbHits()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "WorkflowBeingModified");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			loadedWorkflow.GetTasksWithoutAccessingWorkflowParent().Single().P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			newFactory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessHeaderSchema.Constants.TableName, 3 },
				{ ProcessTasksSchema.Constants.TableName, 3 }, // The parent collection is guaranteed to be loaded by Task Task Assignment Restrictions, so it's better not to pretend this doesn't exist in unit tests.
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
			};

			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}

		public void TestDBHitsForJobWorkflow_NestedClosedQualityIterations()
		{
			var dummyWorkflowProvider = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(dummyWorkflowProvider, Factory);
			var processHeader = jobHeader.ProcessHeaders[0];

			var task = BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 30, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var previousHeader = processHeader;

			for (int i = 0; i < 10; i++)
			{
				var childProcessHeader = jobHeader.ProcessHeaders.AddNew();
				BMSTestHelper.MakeChildOf(childProcessHeader, previousHeader);

				for (int j = 0; j < 5; j++)
				{
					var childTask = BMSTestHelper.CreateTask(childProcessHeader, lowEstMinutes: 30, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
				}

				previousHeader = childProcessHeader;
			}

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedProcessHeader = newFactory.Load<ProcessHeader>(processHeader.PK);

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessTask.Schema.TableName, 1 },
				{ ProcessHeader.Schema.TableName, 3 }
			};

			var remainingHours = loadedProcessHeader.RemainingEstimateHoursIncludingChildren;
			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
		}
	}
}
