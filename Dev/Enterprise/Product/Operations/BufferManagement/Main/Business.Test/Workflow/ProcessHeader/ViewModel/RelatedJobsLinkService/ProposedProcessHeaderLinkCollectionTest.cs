using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProposedProcessHeaderLinkCollection))]
	class ProposedProcessHeaderLinkCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ProposedProcessHeaderLinkCollection>
	{
		public void TestShouldContainProposedLinksBetweenTemplateWorkflows()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Template 1");
			var template1Workflow = BMSTestHelper.CreateWorkflow(template1, "Workflow 1");
			BMSTestHelper.CreateTask(template1, template1Workflow);

			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Template 2");
			var template2Workflow = BMSTestHelper.CreateWorkflow(template2, "Workflow 2");
			BMSTestHelper.CreateTask(template2, template2Workflow);

			BMSTestHelper.CreateDependencyLink(template1, template1Workflow, template2Workflow);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);
			AssertEquals("Should propose the link between INQ and ORG", 1, collection.Count);
		}

		public void TestAttachingSameTypeJobShouldNotGenerateErrorReports_WhenTemplateLinkIsBetweenDifferentTypesOfJobs_OneWayLink()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Target Job Template");
			var targetTemplateWorkflow = BMSTestHelper.CreateWorkflow(targetTemplate, "Workflow 1");
			BMSTestHelper.CreateWorkflow(targetTemplate, "Workflow 2"); // workflow with the same name as in another template
			BMSTestHelper.CreateTask(targetTemplate, targetTemplateWorkflow);

			var anotherTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Another Template");
			var anotherTemplateWorkflow = BMSTestHelper.CreateWorkflow(anotherTemplate, "Workflow 2");
			BMSTestHelper.CreateTask(anotherTemplate, anotherTemplateWorkflow);

			BMSTestHelper.CreateDependencyLink(targetTemplate, targetTemplateWorkflow, anotherTemplateWorkflow); // one way link

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);
			AssertEquals("Should not propose non-relevant links between INQ and ORG if we are after INQ-INQ relationships", 0, collection.Count);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestAttachingSameTypeJobShouldNotGenerateErrorReports_WhenTemplateLinkIsBetweenDifferentTypesOfJobs_ReverseLink()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Target Job Template");
			var targetTemplateWorkflow = BMSTestHelper.CreateWorkflow(targetTemplate, "Workflow 1");
			BMSTestHelper.CreateWorkflow(targetTemplate, "Workflow 2"); // workflow with the same name as in another template
			BMSTestHelper.CreateTask(targetTemplate, targetTemplateWorkflow);

			var anotherTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Another Template");
			var anotherTemplateWorkflow = BMSTestHelper.CreateWorkflow(anotherTemplate, "Workflow 2");
			BMSTestHelper.CreateTask(anotherTemplate, anotherTemplateWorkflow);

			BMSTestHelper.CreateDependencyLink(targetTemplate, anotherTemplateWorkflow, targetTemplateWorkflow); // reverse link

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);
			AssertEquals("Should not propose non-relevant links between INQ and ORG if we are after INQ-INQ relationships", 0, collection.Count);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestAttachingSameTypeJobShouldNotGenerateErrorReports_WhenTemplateLinkIsBetweenSameTypesOfJobs_OneWaylink()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode });

			var genericTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Generic Template");
			genericTemplate.ProcessHeaders[0].FH_CompletionStatement = "Generic Job Header";
			var genericTemplateWorkflow = BMSTestHelper.CreateWorkflow(genericTemplate, "Generic Workflow");
			BMSTestHelper.CreateTask(genericTemplate, genericTemplateWorkflow);

			var specificTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Specific Template", subType1: "AAA");
			specificTemplate.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			specificTemplate.ProcessHeaders[0].FH_CompletionStatement = "Specific Job Header";
			var specificTemplateWorkflow = BMSTestHelper.CreateWorkflow(specificTemplate, "Specific Workflow");
			BMSTestHelper.CreateTask(specificTemplate, specificTemplateWorkflow);

			BMSTestHelper.CreateDependencyLink(genericTemplate, genericTemplateWorkflow, specificTemplateWorkflow); // one way link

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "AAA";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			AssertContainsExactElementsInAnyOrder("Precondition: both templates should be applied to job1",
				new[] { "Generic Workflow", "Specific Workflow" }, jobHeader1.ProcessHeaders.Select(h => h.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("Precondition: both templates should be applied to job2",
				new[] { "Generic Workflow", "Specific Workflow" }, jobHeader2.ProcessHeaders.Select(h => h.FH_CompletionStatement));

			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);

			AssertEquals("Should propose the link", 1, collection.Count);
			var link = collection[0];
			AssertEquals(jobHeader1, link.FromWorkflow.JobHeader);
			AssertEquals(jobHeader2, link.ToWorkflow.JobHeader);
			AssertEquals("Generic Workflow", link.FromWorkflow.FH_CompletionStatement);
			AssertEquals("Specific Workflow", link.ToWorkflow.FH_CompletionStatement);

			AssertEquals("Should not error report", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestAttachingSameTypeJobShouldNotGenerateErrorReports_WhenTemplateLinkIsBetweenSameTypesOfJobs_Reverselink()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode });

			var genericTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Generic Template");
			genericTemplate.ProcessHeaders[0].FH_CompletionStatement = "Generic Job Header";
			var genericTemplateWorkflow = BMSTestHelper.CreateWorkflow(genericTemplate, "Generic Workflow");
			BMSTestHelper.CreateTask(genericTemplate, genericTemplateWorkflow);

			var specificTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Specific Template", subType1: "AAA");
			specificTemplate.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			specificTemplate.ProcessHeaders[0].FH_CompletionStatement = "Specific Job Header";
			var specificTemplateWorkflow = BMSTestHelper.CreateWorkflow(specificTemplate, "Specific Workflow");
			BMSTestHelper.CreateTask(specificTemplate, specificTemplateWorkflow);

			BMSTestHelper.CreateDependencyLink(genericTemplate, specificTemplateWorkflow, genericTemplateWorkflow); // reverse link

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			job1.O1_EnquiryType = "AAA";
			job2.O1_EnquiryType = "AAA";

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			AssertContainsExactElementsInAnyOrder("Precondition: both templates should be applied to job1",
				new[] { "Generic Workflow", "Specific Workflow" }, jobHeader1.ProcessHeaders.Select(h => h.FH_CompletionStatement));
			AssertContainsExactElementsInAnyOrder("Precondition: both templates should be applied to job2",
				new[] { "Generic Workflow", "Specific Workflow" }, jobHeader2.ProcessHeaders.Select(h => h.FH_CompletionStatement));

			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);

			AssertEquals("Should propose the link", 1, collection.Count);
			var link = collection[0];
			AssertEquals(jobHeader1, link.FromWorkflow.JobHeader);
			AssertEquals(jobHeader2, link.ToWorkflow.JobHeader);
			AssertEquals("Specific Workflow", link.FromWorkflow.FH_CompletionStatement);
			AssertEquals("Generic Workflow", link.ToWorkflow.FH_CompletionStatement);

			AssertEquals("Should not error report", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestFetchHints_ForAttachingSameTypeJob()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var targetTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Target Job Template");
			var targetTemplateWorkflow = BMSTestHelper.CreateWorkflow(targetTemplate, "Target Workflow");
			BMSTestHelper.CreateTask(targetTemplate, targetTemplateWorkflow);

			var anotherTemplate = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Another Template");
			var anotherTemplateWorkflow = BMSTestHelper.CreateWorkflow(anotherTemplate, "Another Template Workflow");
			BMSTestHelper.CreateTask(anotherTemplate, anotherTemplateWorkflow);

			BMSTestHelper.CreateDependencyLink(targetTemplate, targetTemplateWorkflow, anotherTemplateWorkflow); // one way link

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
			};
			using (AssertDbHitsForAllFactories("", expectedDbHits))
			{
				var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2);
			}
		}

		public void TestShouldOnlyContainProposedLinksBetweenTemplateWorkflowsIfIncludedInHeaderLinkSourceTemplates()
		{
			BMSTestHelper.EnableBMSInRegistry();

			var templateFactory = Factory.CreateNewFactory();

			TestConfigsHelper.CreateSchematicTestConfig(templateFactory, new[] { SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, OrgHeaderWorkflowDescriptor.WorkflowTypeCode });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, SalesEnquiryWorkflowDescriptor.WorkflowTypeCode, name: "Template 1");
			var template1Workflow = BMSTestHelper.CreateWorkflow(template1, "Workflow 1");
			BMSTestHelper.CreateTask(template1, template1Workflow);

			var template2 = BMSTestHelper.CreateWorkflowTemplate(templateFactory, OrgHeaderWorkflowDescriptor.WorkflowTypeCode, name: "Template 2");
			var template2Workflow = BMSTestHelper.CreateWorkflow(template2, "Workflow 2");
			BMSTestHelper.CreateTask(template2, template2Workflow);

			BMSTestHelper.CreateDependencyLink(template1, template1Workflow, template2Workflow);

			templateFactory.Save();

			var job1 = Factory.NewWithValidTestData<SalesEnquiry>();
			var job2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);

			var templateList = new List<IProcessTaskTemplate>();
			var collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2, templateList);
			AssertEquals("Should not propose the link between INQ and ORG since source template list does not include the template", 0, collection.Count);

			templateList.Add(template1);
			collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2, templateList);
			AssertEquals("Should propose the link between INQ and ORG since source template list includes the template", 1, collection.Count);

			templateList.Remove(template1);
			templateList.Add(template2);
			collection = new ProposedProcessHeaderLinkCollection(jobHeader1, jobHeader2, templateList);
			AssertEquals("Should propose the link between INQ and ORG since source template list includes the template", 1, collection.Count);
		}

		#region Implementation

		protected override ProposedProcessHeaderLinkCollection GetCollectionToTest()
		{
			return new ProposedProcessHeaderLinkCollection(FromJobHeader, ToJobHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ProposedProcessHeaderLink(FromJobHeader, ToJobHeader);
		}

		ProcessJobHeader FromJobHeader
		{
			get { return fromJobHeader ?? (fromJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory)); }
		}

		ProcessJobHeader fromJobHeader;

		ProcessJobHeader ToJobHeader
		{
			get { return toJobHeader ?? (toJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory)); }
		}

		ProcessJobHeader toJobHeader;

		#endregion
	}
}
