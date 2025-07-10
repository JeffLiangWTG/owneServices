using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(QueueStatusFilter))]
	class QueueStatusFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter_ForJobsInQueues()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "jobHeader4");

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code);

			queue1.AddMember(jobHeader1);
			queue1.AddMember(jobHeader2);

			queue2.AddMember(jobHeader3);
			queue2.AddMember(jobHeader4);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.QueueStatus];

			filter.IsActive = true;
			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { jobHeader1, jobHeader3, workflow1, workflow3 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { jobHeader2, jobHeader4, workflow2, workflow4 }, results);

			// Release item at the front of queue1
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { jobHeader2, jobHeader3, workflow2, workflow3 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { jobHeader4, workflow4 }, results);

			// Close item at the front of queue2
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { jobHeader2, jobHeader4, workflow2, workflow4 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(GetNextQueueItemSummary(), 0, results.Length);
		}

		public void TestFilter_ForWorkflowsInQueues()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code);

			queue1.AddMember(workflow1);
			queue1.AddMember(workflow2);

			queue2.AddMember(workflow3);
			queue2.AddMember(workflow4);

			Factory.Save();

			var bizo = new ProcessHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo[ProcessHeader.ModuleFilterConstants.QueueStatus];

			filter.IsActive = true;
			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			var results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder("Should not find jobHeader since it's not part of a queue, but should find its workflows because they're at the top of their queues, and yet..." + GetNextQueueItemSummary(), new[] { workflow1, workflow3 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { workflow2, workflow4 }, results);

			// Release item at the front of queue1
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { workflow2, workflow3 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { workflow4 }, results);

			// Close item at the front of queue2
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertContainsExactElementsInAnyOrder(GetNextQueueItemSummary(), new[] { workflow2, workflow4 }, results);

			filter.Property = QueueStatusList.Codes.Blocked;
			results = Factory.Load<ProcessHeader>(bizo.Filter);
			AssertEquals(GetNextQueueItemSummary(), 0, results.Length);
		}

		public void TestQueueStatusFilter_WithWorkflowOnlyFilter_ShouldCreateASimplerQuery()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			var strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.QueueStatus);
			var filter = (QueueStatusFilter)strip.CurrentModuleFilter;

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			var query = filterBizo.Filter.LiteralTextADOFormatted;
			AssertContains("UNION ALL should appear in the query because we haven't specified workflows only, and yet..." + query, "UNION ALL", query);

			filter.Property = QueueStatusList.Codes.Blocked;
			query = filterBizo.Filter.LiteralTextADOFormatted;
			AssertContains("UNION ALL should appear in the query because we haven't specified workflows only, and yet..." + query, "UNION ALL", query);

			strip = filterBizo.FilterStrips.AddNew(ProcessHeader.ModuleFilterConstants.JobOrWorkflow);
			var jobOrWorkflowFilter = (ModuleFlagsFilter)strip.CurrentModuleFilter;
			jobOrWorkflowFilter.Property1 = true;

			filter.Property = QueueStatusList.Codes.ReadyToRelease;
			query = filterBizo.Filter.LiteralTextADOFormatted;
			AssertNotContains("UNION should not appear in the query because we specified workflows only, and yet..." + query, "UNION", query, ignoreCase: true);

			filter.Property = QueueStatusList.Codes.Blocked;
			query = filterBizo.Filter.LiteralTextADOFormatted;
			AssertNotContains("UNION should not appear in the query because we specified workflows only, and yet..." + query, "UNION", query, ignoreCase: true);
		}

		string GetNextQueueItemSummary()
		{
			const int columnWidth = 50;
			var columns = new[] { "Completion Statement", "Queue Description", "Queue PK", "Sequence" };
			var result = new ZStringBuilder();
			result.AppendLine("Next Items in Each Queue:");
			result.AppendLine();

			foreach (var column in columns)
			{
				result.Append(column.PadRight(columnWidth));
			}

			result.AppendLine();
			result.AppendLine(string.Empty.PadRight(columnWidth * columns.Length, '-'));

			using (var reader = TestConnection.Command("SELECT * FROM dbo.GetWorkQueueTagLinks();").ExecuteReader())
			{
				while (reader.Read())
				{
					result.AppendLine();

					var processHeader = Factory.Load<ProcessHeader>(reader.GetGuid(0));
					result.Append(processHeader.FH_CompletionStatement.PadRight(columnWidth).Substring(0, columnWidth));

					for (var i = 1; i < reader.FieldCount; i++)
					{
						result.Append(reader[i].ToString().PadRight(columnWidth).Substring(0, columnWidth));
					}
				}
			}

			result.AppendLine();
			return result.ToString();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			return filterBizo[ProcessHeader.ModuleFilterConstants.QueueStatus];
		}
	}
}
