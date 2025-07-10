using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderCollectionProviderTest : BMSTestCaseWithFactory
	{
		public void TestTemplateProcessHeaderCollection_ShouldCreateJobHeaderIfNotPresent()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var system2 = BMSTestHelper.CreateSystem(Factory, "SIM");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			AssertEquals("Collection should not allow new rows when ProcessType has no BMSystem", false, template.ProcessHeaders.AllowNew);
			AssertEquals("Collection should be empty when ProcessType has no BMSystem", 0, template.ProcessHeaders.Count);

			template.P0_ProcessType = "ORG";

			AssertEquals("Collection should allow new rows when ProcessType has a BMSystem", true, template.ProcessHeaders.AllowNew);
			AssertEquals("Collection should include a job-level workflow when ProcessType has a BMSystem", 1, template.ProcessHeaders.Count);

			var jobHeader = template.ProcessHeaders[0];

			AssertType<ProcessJobHeader>(jobHeader);
			AssertEquals("Job is complete.", jobHeader.FH_CompletionStatement);
			AssertEquals(template.PK, jobHeader.FH_P0_Template);

			template.P0_ProcessType = "SIM";

			AssertEquals("Collection should allow new rows when ProcessType has a BMSystem", true, template.ProcessHeaders.AllowNew);
			AssertEquals("Collection should include a job-level workflow when ProcessType has a BMSystem", 1, template.ProcessHeaders.Count);

			AssertEquals("Job-level workflow can be re-used when changing to a different process type that also has a BMSystem", jobHeader, template.ProcessHeaders[0]);
			AssertEquals("Job is complete.", template.ProcessHeaders[0].FH_CompletionStatement);

			template.P0_ProcessType = "SHP";

			AssertEquals("Collection should not allow new rows when ProcessType has no BMSystem", false, template.ProcessHeaders.AllowNew);
			AssertEquals("Collection should be empty when ProcessType has no BMSystem", 0, template.ProcessHeaders.Count);

			AssertEquals(true, jobHeader.IsDeleted);
		}

		public void TestTemplateProcessHeaderCollection_ShouldCreateJobHeaderIfOldOneGotDeletedSomehow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			var jobHeader = template.GetJobHeader();

			AssertNotNull(jobHeader);

			jobHeader.Delete();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);
			var loadedJobHeader = loadedTemplate.GetJobHeader();

			AssertNotNull(loadedJobHeader);
		}

		public void TestTemplateProcessHeaderCollection_ShouldAssociateWorkflowsWithJobHeader()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			AssertEquals(1, template.ProcessHeaders.Count);

			var jobHeader = (ProcessJobHeader)template.ProcessHeaders[0];
			var workflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			var workflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();

			AssertEquals(jobHeader, workflow1.JobHeader);
			AssertEquals(jobHeader, workflow2.JobHeader);

			AssertEquals("Template Job", jobHeader.ProcessHeaderType);
			AssertEquals("Template Workflow", workflow1.ProcessHeaderType);
			AssertEquals("Template Workflow", workflow2.ProcessHeaderType);
		}

		public void TestTemplateProcessHeaderCollection_ShouldCreateJobHeaderIfNotPresent_AndLeaveWhenSaved()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			AssertEquals(1, template.ProcessHeaders.Count);

			var jobHeader = template.ProcessHeaders[0];

			Factory.Save();

			template.P0_ProcessType = "SHP";

			AssertEquals("Collection should include a job-level workflow even when ProcessType has no BMSystem if that record has been saved. It's possible users have changed something relevant so we'll just leave it where it is.", 1, template.ProcessHeaders.Count);
			AssertEquals("Job-level workflow can be re-used when changing to a different process type, even if it has no BMSystem", jobHeader, template.ProcessHeaders[0]);
		}

		public void TestTemplateProcessHeaderCollection_ShouldCreateJobHeaderIfNotPresent_AndLeaveWhenOtherWorkflowsHaveBeenCreated()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			AssertEquals(1, template.ProcessHeaders.Count);

			var jobHeader = template.ProcessHeaders[0];
			var workflow = template.ProcessHeaders.AddNew();

			AssertEquals(jobHeader, workflow.JobHeader);

			template.P0_ProcessType = "SHP";

			AssertEquals("Should not delete the job-level workflow since the user has added their own workflow. There's no harm in leaving them as they won't get applied to jobs. Maybe the user plans to change the workfow type back to one that has a BMSystem.", 2, template.ProcessHeaders.Count);
		}

		public void TestGetForTask_DeletedTask()
		{
			var task = Factory.New<ProcessTask>();
			task.Delete();
			var provider = ObjectFactory.Get<IProcessHeaderCollectionProvider>();
			var collection = provider.GetForTask(task);
			AssertNotNull(collection);
			AssertEquals(0, collection.Count);
		}

		public void TestGetCollection()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header 2");
			Factory.Save();

			var provider = ObjectFactory.Get<IProcessHeaderCollectionProvider>();
			var collectionFromProvider = provider.GetCollection(Factory);

			var processHeadersLoadedFromDB = Factory.Load<ProcessHeader>(new ZQuery());
			AssertEquals("Collection should return all process headers from database", processHeadersLoadedFromDB.Length, collectionFromProvider.Count);
			Assert("Collection should contains jobHeader1", collectionFromProvider.Contains(jobHeader1));
			Assert("Collection should contains jobHeader2", collectionFromProvider.Contains(jobHeader2));
		}
	}
}
