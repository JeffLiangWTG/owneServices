using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeaderLink))]
	class ProcessHeaderLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region IEntityRelationship

		public void TestIRelationShip_IsApplicable()
		{
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false, description: "Prereq Job Header");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "I've been tasked");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var qualityIterationWorkflow1 = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(workflow);
			var qualityIterationWorkflow2 = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(qualityIterationWorkflow1);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = BMSTestHelper.CreateTask(qualityIterationWorkflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task3 = BMSTestHelper.CreateTask(qualityIterationWorkflow2, GlbStaff.CurrentUser.GS_Code, 0);

			var link = jobHeader2.GetOrCreateDependencyLink(jobHeader);

			Factory.Save();

			var applicabilityFunc = new Func<ILink, bool>(l => ((ProcessHeaderLink)l).FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);
			AssertEquals(true, link.IsApplicableDependencyToEntity(qualityIterationWorkflow2, new ProcessHeaderDescendantsStrategy(), applicabilityFunc: applicabilityFunc));
			AssertEquals(true, link.IsApplicableDependencyToEntity(qualityIterationWorkflow1, new ProcessHeaderDescendantsStrategy(), applicabilityFunc: applicabilityFunc));
			AssertEquals(WorkflowStatusList.Codes.Blocked, qualityIterationWorkflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, qualityIterationWorkflow2.FH_Status);
		}

		public void TestIRelationship_FromTo()
		{
			var workflowFrom = Factory.NewWithValidTestData<ProcessHeader>();
			var workflowTo = Factory.NewWithValidTestData<ProcessHeader>();
			var link = Factory.New<ProcessHeaderLink>();

			var relationship = (IEntityRelationship)link;
			AssertNull(relationship.From);
			AssertNull(relationship.To);

			relationship.From = workflowFrom;
			AssertEquals(workflowFrom, relationship.From);

			relationship.To = workflowTo;
			AssertEquals(workflowTo, relationship.To);
		}

		public void TestDisplayText()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			AssertEquals("workflow1 -> workflow2", link.DisplayText);
		}

		#endregion

		#region Templates

		public void TestApplyTemplateRaceCondition()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "workflow1");
			BMSTestHelper.CreateTask(template, workflow1, description: "Nnnnngggggh");

			Factory.Save();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				Factory.Save();
			}

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var jobHeader1 = factory1.Load<ProcessJobHeader>(jobHeader.PK);
			var jobHeader2 = factory2.Load<ProcessJobHeader>(jobHeader.PK);

			AssertEquals(0, jobHeader1.ProcessHeaders.Count);
			AssertEquals(0, jobHeader2.ProcessHeaders.Count);

			jobHeader1.ApplyTemplate(template);
			factory1.Save();

			jobHeader2.ApplyTemplate(template);
			factory2.Save();

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var jobHeader3 = factory3.Load<ProcessJobHeader>(jobHeader.PK);

			AssertEquals(1, jobHeader3.ProcessHeaders.Count);
		}

		public void TestFromToExternalTemplate_OperationalLink()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var link = jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			AssertEquals(ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals(ZGuid.Empty, link.ToWorkflowExternalTemplatePK);
		}

		public void TestFromToExternalTemplate_TemplateLink_WorkflowSpecified()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var jobHeader = template.GetJobHeader();

			var workflow1 = BMSTestHelper.CreateWorkflow(template, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "workflow2");

			var link = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();

			link.FP_FH_HeaderFrom = workflow1.PK;
			AssertEquals(link.HeaderFrom, workflow1);
			link.FP_FH_HeaderTo = workflow2.PK;
			AssertEquals(link.HeaderTo, workflow2);

			AssertEquals(ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals(ZGuid.Empty, link.ToWorkflowExternalTemplatePK);

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, jobHeader }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, jobHeader }, link.Lookups.HeaderTos);

			var workflow3 = BMSTestHelper.CreateWorkflow(template, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(template, "workflow4");
			link.FP_FH_HeaderFrom = workflow3.PK;
			AssertEquals(link.HeaderFrom, workflow3);

			link.FP_FH_HeaderTo = workflow4.PK;
			AssertEquals(link.HeaderTo, workflow4);
		}

		public void TestFromToExternalTemplate_TemplateLink_ExternalWorkflowSpecified()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", "AAA");

			var jobHeader1 = template1.GetJobHeader();
			var jobHeader2 = template2.GetJobHeader();

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Template1 workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Template2 workflow");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();

			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2.PK;

			AssertEquals(ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals(template2.PK, link.ToWorkflowExternalTemplatePK);

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, jobHeader1 }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow2, jobHeader2 }, link.Lookups.HeaderTos);
		}

		public void TestFromToExternalTemplate_TemplateLink_ExternalWorkflowTemplateSpecified()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", "AAA");

			var jobHeader1 = template1.GetJobHeader();
			var jobHeader2 = template2.GetJobHeader();

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Template1 workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Template2 workflow");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();

			link.FromWorkflowExternalTemplatePK = template2.PK;

			AssertEquals(template2.PK, link.FromWorkflowExternalTemplatePK);
			AssertEquals(ZGuid.Empty, link.ToWorkflowExternalTemplatePK);

			AssertContainsExactElementsInAnyOrder(new[] { workflow2, jobHeader2 }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, jobHeader1 }, link.Lookups.HeaderTos);

			link.ToWorkflowExternalTemplatePK = template2.PK;

			AssertEquals(template2.PK, link.FromWorkflowExternalTemplatePK);
			AssertEquals(template2.PK, link.ToWorkflowExternalTemplatePK);

			AssertContainsExactElementsInAnyOrder(new[] { workflow2, jobHeader2 }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow2, jobHeader2 }, link.Lookups.HeaderTos);

			link.FromWorkflowExternalTemplatePK = ZGuid.Empty;

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, jobHeader1 }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow2, jobHeader2 }, link.Lookups.HeaderTos);
		}

		public void TestFromToExternalTemplate_TemplateLink_WorkflowNotSpecified()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var jobHeader = template.GetJobHeader();

			var workflow1 = BMSTestHelper.CreateWorkflow(template, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "workflow2");

			var link = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();

			AssertEquals(ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals(ZGuid.Empty, link.ToWorkflowExternalTemplatePK);

			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, jobHeader }, link.Lookups.HeaderFroms);
			AssertContainsExactElementsInAnyOrder(new[] { workflow1, workflow2, jobHeader }, link.Lookups.HeaderTos);
		}

		public void TestToExternalTemplate_TemplateLink_SetAndUnSetExternalToWorkflow_ShouldRemoveExternalWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", "AAA");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Template1 workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Template2 workflow");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			link.ToWorkflowExternalTemplatePK = template2.PK;

			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2.PK;

			AssertEquals(ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals(template2.PK, link.ToWorkflowExternalTemplatePK);

			link.Validation.ValidateAll();
			AssertNoErrors(link);

			link.ToWorkflowExternalTemplatePK = ZGuid.Empty;
			AssertEquals("Removing the ToWorkflowExternalTemplatePK should actually have an effect", ZGuid.Empty, link.ToWorkflowExternalTemplatePK);
			AssertEquals("Removing the ToWorkflowExternalTemplatePK should un-set FP_FH_HeaderTo since it would cause the value to get re-populated", ZGuid.Empty, link.FP_FH_HeaderTo);
		}

		public void TestFromExternalTemplate_TemplateLink_SetAndUnSetExternalFromWorkflow_ShouldRemoveExternalWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", "AAA");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Template1 workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Template2 workflow");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			link.FromWorkflowExternalTemplatePK = template2.PK;

			link.FP_FH_HeaderFrom = workflow2.PK;
			link.FP_FH_HeaderTo = workflow1.PK;

			AssertEquals(template2.PK, link.FromWorkflowExternalTemplatePK);
			AssertEquals(ZGuid.Empty, link.ToWorkflowExternalTemplatePK);

			link.Validation.ValidateAll();
			AssertNoErrors(link);

			link.FromWorkflowExternalTemplatePK = ZGuid.Empty;
			AssertEquals("Removing the FromWorkflowExternalTemplatePK should actually have an effect", ZGuid.Empty, link.FromWorkflowExternalTemplatePK);
			AssertEquals("Removing the FromWorkflowExternalTemplatePK should un-set FP_FH_HeaderFrom since it would cause the value to get re-populated", ZGuid.Empty, link.FP_FH_HeaderFrom);
		}

		public void TestExternalLinks_BetweenJobs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", "AAA");

			var jobHeader1 = template1.GetJobHeader();
			var jobHeader2 = template2.GetJobHeader();

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Template1 workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Template2 workflow");

			var link = BMSTestHelper.CreateDependencyLink(template1, jobHeader1, jobHeader2);

			AssertNoErrors(link);
		}

		public void TestLinkBetweenJobAndItsOwnWorkflow_ShouldNotBeValid()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var jobHeader = template.GetJobHeader();
			var workflow = BMSTestHelper.CreateWorkflow(template);

			var link = BMSTestHelper.CreateDependencyLink(template, jobHeader, workflow);
			link.RunPreSaveValidation();

			AssertHasError(link.FP_FH_HeaderToInfo, "This is a dependency link between workflows that are also involved in a Parent-Child relationship.");
		}

		public void TestApplyModifiedLinkTemplate_ShouldNotThrowException_HeaderFrom()
		{
			AssertApplyModifiedLinkTemplateShouldNotThrowException(workflowToDeleteDescription: "Workflow From");
		}

		public void TestApplyModifiedLinkTemplate_ShouldNotThrowException_HeaderTo()
		{
			AssertApplyModifiedLinkTemplateShouldNotThrowException(workflowToDeleteDescription: "Workflow To");
		}

		void AssertApplyModifiedLinkTemplateShouldNotThrowException(string workflowToDeleteDescription)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template");

			var templateWorkflowFrom = BMSTestHelper.CreateWorkflow(template, "Workflow From");
			var templateTaskFrom = BMSTestHelper.CreateTask(template, templateWorkflowFrom);

			var templateWorkflowTo = BMSTestHelper.CreateWorkflow(template, "Workflow To");
			var templateTaskTo = BMSTestHelper.CreateTask(template, templateWorkflowTo);

			var templateWorkflowExtra = BMSTestHelper.CreateWorkflow(template, "Workflow Extra");
			var templateTaskExtra = BMSTestHelper.CreateTask(template, templateWorkflowExtra);

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflowFrom, templateWorkflowTo);
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Link Count", 1, template.ProcessHeaderLinks.Count);
				AssertNotNull("HeaderTo", templateLink.HeaderTo);
				AssertNotNull("HeaderFrom", templateLink.HeaderFrom);
			});

			Factory.Save();

			var jobFactory = new BusinessObjectFactory { RefreshEnabled = true };
			var job = jobFactory.New<IWorkItem>();
			job.WKI_Summary = "BIN";

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, jobFactory);
			var loadedTemplate = jobFactory.Load<ProcessTaskTemplate>(template.PK);
			jobHeader.ApplyTemplate(loadedTemplate);

			AssertEquals("First application of the template before we delete the workflow from the template", 4, jobHeader.ProcessHeaders.Count);
			var workflowToRemove = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == workflowToDeleteDescription);
			AssertEquals("A link should exist from the template workflow we are about to delete", 1, workflowToRemove.Links.Count());

			var nonDeletedLinkWorkflowDescription = string.Empty;
			if (templateLink.HeaderFrom.FH_CompletionStatement == workflowToDeleteDescription)
			{
				templateLink.FP_FH_HeaderFrom = templateWorkflowExtra.PK;
				templateWorkflowFrom.Delete();
				nonDeletedLinkWorkflowDescription = templateLink.HeaderTo.FH_CompletionStatement;
			}
			else if (templateLink.HeaderTo.FH_CompletionStatement == workflowToDeleteDescription)
			{
				templateLink.FP_FH_HeaderTo = templateWorkflowExtra.PK;
				templateWorkflowTo.Delete();
				nonDeletedLinkWorkflowDescription = templateLink.HeaderFrom.FH_CompletionStatement;
			}
			else
			{
				Assert("workflowDescriptionToDelete should exist in link", false);
			}

			Factory.Save();

			jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow From").Delete();
			jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow To").Delete();
			jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Extra").Delete();

			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			CombineAssertions("Upon applying the updated template", () =>
			{
				AssertNoExceptionThrown("Should not throw exception due to deleted template workflow", () => { jobFactory.Save(); });

				AssertEquals("All but the deleted template workflow should be applied", 3, jobHeader.ProcessHeaders.Count);

				AssertEquals("The deleted template workflow should not be applied", 0, jobHeader.ProcessHeaders.Count(w => w.FH_CompletionStatement == workflowToDeleteDescription));
				AssertEquals("A link should exist from non deleted workflow", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == nonDeletedLinkWorkflowDescription).Links.Count());
				AssertEquals("A link should exist to the 'extra' workflow", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Extra").Links.Count());
			});
		}

		#endregion

		#region IProcessHeaderLinkRow

		public void TestIProcessHeaderLinkRow_ShouldContainAllSchemaColumns()
		{
			var message = string.Format(@"{0} should contain all properties on the ProcessHeaderLink row. If new columns are added, corresponding properties should also be added to {1} and its methods {2} and {3}.",
				nameof(IProcessHeaderLinkRow),
				nameof(ProposedProcessHeaderLink),
				typeof(ProposedProcessHeaderLink).GetMethod("EnsureRealLinkExists", BindingFlags.NonPublic | BindingFlags.Instance).Name,
				typeof(ProposedProcessHeaderLink).GetMethod("TryCreateFromTemplateLink", BindingFlags.NonPublic | BindingFlags.Static).Name
				);

			CombineAssertions(message, () =>
			{
				foreach (SchemaColumn column in ProcessHeaderLinkSchema.All.Except(new[] { ProcessHeaderLinkSchema.FP_IsValid }))
				{
					if (!column.IsPKColumn)
					{
						AssertNotNull(column.Name, typeof(IProcessHeaderLinkRow).GetProperty(column.Name));
					}
				}
			});
		}

		#endregion

		#region Buffer Penetration

		public void TestSynchroniseBufferPenetration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			var link1 = childWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			var link2 = childWorkflow.GetOrCreateLinkToParent(jobHeader);
			var link3 = parentWorkflow.GetOrCreateLinkToParent(jobHeader);

			AssertEquals(false, link1.FP_SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(true, link2.FP_SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(true, link3.FP_SynchroniseBufferPenetrationInfo.ReadOnly);

			var dodgyLink = Factory.New<ProcessHeaderLink>();
			AssertEquals(true, dodgyLink.FP_SynchroniseBufferPenetrationInfo.ReadOnly);
		}

		#endregion

		#region Delete

		[ExpectNoExceptions]
		public void TestDelete_WhenAnAttachmentExists()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			var shape = Factory.New<IBMNCNShape>();
			var fromShape = Factory.New<IBMNCNShape>();
			var attachment = Factory.New<IBMNCNAttachment>();
			attachment.BNA_FP_ProcessHeaderLink = link.PK;
			attachment.BNA_BNS_Owner = attachment.BNA_BNS_ToShape = shape.Identifier;
			attachment.BNA_BNS_FromShape = fromShape.Identifier;

			Factory.Save();

			link.Delete();
			Factory.Save();
		}

		public void TestDelete_UpdateProcessHeaderToEditTime()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header1 = flowHeader.ProcessHeaders.AddNew();

			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderTo = header1.PK;
			link.FP_LinkType = "DEP";
			link.HeaderTo.FH_SystemLastEditTimeUtc = ZDateTime.UtcToday.AddDays(-3);

			link.Delete();
			AssertEquals(ZDateTime.UtcToday.Date, header1.FH_SystemLastEditTimeUtc.Date);
		}

		#endregion

		#region Staggered Starts

		[TestDate(2013, 12, 3)]
		public void TestStaggeredReleaseTimeDelay()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			var link = workflow1.Links.First();

			link.FP_TimeDelayMinutes = 10;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(10), link.StaggeredReleaseTimeDelay);

			link.StaggeredReleaseTimeDelay = new ZDateTime(2013, 1, 2, 1, 0, 0); // 1 days, 1 hour
			AssertEquals(25 * 60, link.FP_TimeDelayMinutes);
		}

		#endregion

		#region Dependency Applicability

		public void TestIsApplicableDependency_WorkflowToWorkflow()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			AssertEquals("Direct workflow to workflow link is always applicable", true, link.IsApplicableDependencyToEntity(workflow2));
		}

		public void TestIsApplicableDependency_JobToWorkflow()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = jobHeader1.GetOrCreateDependencyLink(workflow2);

			AssertEquals("Job to workflow link is applicable", true, link.IsApplicableDependencyToEntity(workflow2));
		}

		public void TestIsApplicableDependency_JobToJob_WhenDirectWorkflowLinkAlsoExists()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link1 = jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			var link2 = workflow1.GetOrCreateDependencyLink(workflow2);

			AssertEquals("Job to job link is NOT applicable since a more specific link exists", false, link1.IsApplicableDependencyToEntity(workflow2));
			AssertEquals("Direct workflow to workflow link is always applicable", true, link2.IsApplicableDependencyToEntity(workflow2));
		}

		public void TestIsApplicableDependency_WorkflowToJob()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(jobHeader2);

			AssertEquals("Workflow to job link is applicable", true, link.IsApplicableDependencyToEntity(workflow2));
		}

		#endregion

		#region Duplicate Relationships

		public void TestDuplicateRelationshipMessage()
		{
			var saveInitiator = new Mock<ISaveInitiator>();
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			header1.FH_CompletionStatement = "Workflow 1";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Workflow 2";

			var link1 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1.FP_FH_HeaderTo = header2.PK;
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			var link2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link2.FP_FH_HeaderTo = header2.PK;
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var exception = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			ZExceptionReporting.HandleSaveException(exception, saveInitiator.Object);

			AssertEquals("There are duplicate Related Workflow links. The duplicate values are: (From Workflow: Workflow 1, To Workflow: Workflow 2, Link Type: DEP).", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Display Properties

		public void TestHumanReadableName()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link = Factory.New<ProcessHeaderLink>();
			AssertEquals(nameof(ProcessHeaderLink), link.HumanReadableName);

			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			AssertEquals("Dependency Link from [] to []", link.HumanReadableName);

			link.FP_FH_HeaderFrom = workflow1.PK;
			AssertEquals("Dependency Link from [workflow1] to []", link.HumanReadableName);

			link.FP_FH_HeaderTo = workflow2.PK;
			AssertEquals("Dependency Link from [workflow1] to [workflow2]", link.HumanReadableName);

			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			AssertEquals("Parent-Child Link from child [workflow1] to parent [workflow2]", link.HumanReadableName);
		}

		public void TestHeadersFromAndToDescriptions()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			((OrgHeader)jobHeader.Parent).OH_Code = "MAIFRISTORG";

			var link = Factory.New<ProcessHeaderLink>();

			AssertEquals(ZString.Empty, link.FromHeaderDescription);
			AssertEquals(ZString.Empty, link.ToHeaderDescription);

			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2.PK;

			AssertEquals("workflow1 (MAIFRISTORG)", link.FromHeaderDescription);
			AssertEquals("workflow2 (MAIFRISTORG)", link.ToHeaderDescription);
		}

		#endregion

		#region Process New Link

		public void TestProcessNewLink_CascadesApprovedFlag()
		{
			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			parentJobHeader.FH_IsApproved = true;
			AssertEquals(false, childJobHeader.FH_IsApproved);

			var link = Factory.New<ProcessHeaderLink>();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link.FP_FH_HeaderFrom = childJobHeader.PK;
			link.FP_FH_HeaderTo = parentJobHeader.PK;
			link.ProcessNewLink();

			AssertEquals(true, childJobHeader.FH_IsApproved);
		}

		#endregion

		#region Resource Strings

		public void TestResourceString_FP_TimeDelayFactor()
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");
			var link = processHeader.GetOrCreateLinkToParent(processHeader);
			var fullDescription = DataBoundResourceStrings.GetDataForProperty(link.FP_TimeDelayFactorInfo).FullDescription;

			AssertEquals("The number of times to multiply (factor) the sum of the estimates of previous workflows that need to be elapsed when determining a workflow's Staggered Release Delay Expiry.", fullDescription);
		}

		public void TestResourceString_FP_TimeDelayMinutes()
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");
			var link = processHeader.GetOrCreateLinkToParent(processHeader);
			var fullDescription = DataBoundResourceStrings.GetDataForProperty(link.FP_TimeDelayMinutesInfo).FullDescription;

			AssertEquals("The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.", fullDescription);
		}

		public void TestResourceString_StaggeredReleaseTimeDelay()
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");
			var link = processHeader.GetOrCreateLinkToParent(processHeader);
			var fullDescription = DataBoundResourceStrings.GetDataForProperty(link.StaggeredReleaseTimeDelayInfo).FullDescription;

			AssertEquals("The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.", fullDescription);
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, link.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			link.FP_FH_HeaderFrom = workflow3.PK;
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			link.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummy, factory);
			var header1 = flowHeader.ProcessHeaders.AddNew();
			var header2 = flowHeader.ProcessHeaders.AddNew();

			var link = factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = header1.PK;
			link.FP_FH_HeaderTo = header2.PK;
			link.FP_LinkType = "DEP";

			return link;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
