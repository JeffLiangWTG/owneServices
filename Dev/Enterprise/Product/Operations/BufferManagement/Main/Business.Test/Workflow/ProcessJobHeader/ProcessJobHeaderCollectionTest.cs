using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessJobHeaderCollection))]
	class ProcessJobHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ProcessJobHeaderCollection>
	{
		public void TestCollection_ShouldExcludeWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			Factory.Save();

			var collection = new ProcessJobHeaderCollection(Factory.CreateNewFactory());

			AssertEquals(1, collection.Count);
			VisualBoardsTestCase.AssertSamePK(jobHeader, collection[0]);
		}

		public void TestCollection_ShouldExcludeTemplateJobHeaders()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "No... Well OK.");

			AssertNotNull(templateJobHeader);

			Factory.Save();

			var collection = new ProcessJobHeaderCollection(Factory.CreateNewFactory());

			AssertEquals(1, collection.Count);
			VisualBoardsTestCase.AssertSamePK(jobHeader, collection[0]);
		}
	}
}
