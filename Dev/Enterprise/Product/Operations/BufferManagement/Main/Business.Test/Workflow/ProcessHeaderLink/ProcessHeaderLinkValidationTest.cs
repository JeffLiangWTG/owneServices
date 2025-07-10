using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll_ShouldNotValidateCircularDependenciesWhenRelevantChangesHaveNotOccurred()
		{
			var notInitialisedByValidationCycleCache = new WorkflowCycleCache();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Snapple");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Frack");

			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedLink = newFactory.Load<ProcessHeaderLink>(link.PK);

			object dependencyCycleCache;
			object parentChildCycleCache;

			using (loadedLink.EmulatePreSaveValidation_ForTest()) // need to emulate pre save validation here as cannot run it directly: the cache in this case would clear on validation finish
			{
				workflow1.Validation.ValidateAll();

				AssertNull(newFactory.ValidationCache);

				loadedLink.Validation.ValidateAll();

				AssertEquals(false, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.DependencyValidationCacheKey, out dependencyCycleCache));
				AssertEquals(false, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.ParentChildValidationCacheKey, out parentChildCycleCache));

				loadedLink.FP_TimeDelayMinutes = 1;
				loadedLink.Validation.ValidateAll();

				AssertEquals(false, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.DependencyValidationCacheKey, out dependencyCycleCache));
				AssertEquals(false, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.ParentChildValidationCacheKey, out parentChildCycleCache));

				loadedLink.FP_FH_HeaderFrom = workflow2.PK;

				AssertEquals("Should only validate invalid networks when relevant properties change", true, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.DependencyValidationCacheKey, out dependencyCycleCache));
				AssertEquals("Should only validate invalid networks when relevant properties change", false, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.ParentChildValidationCacheKey, out parentChildCycleCache));
			}

			newFactory = newFactory.CreateNewFactory();
			loadedLink = newFactory.Load<ProcessHeaderLink>(link.PK);

			using (loadedLink.EmulatePreSaveValidation_ForTest())
			{
				loadedLink.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

				AssertEquals("Should only validate invalid networks when relevant properties change", true, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.DependencyValidationCacheKey, out dependencyCycleCache));
				AssertEquals("Should only validate invalid networks when relevant properties change", true, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.ParentChildValidationCacheKey, out parentChildCycleCache));
			}

			newFactory.Save();

			newFactory = newFactory.CreateNewFactory();
			loadedLink = newFactory.Load<ProcessHeaderLink>(link.PK);

			using (loadedLink.EmulatePreSaveValidation_ForTest())
			{
				loadedLink.FP_FH_HeaderTo = workflow1.PK;

				AssertEquals("Should only validate invalid networks when relevant properties change", true, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.DependencyValidationCacheKey, out dependencyCycleCache));
				AssertEquals("Should only validate invalid networks when relevant properties change", true, newFactory.ValidationCache.CachedData.TryGetValue(ProcessHeaderLinkCycleValidationHelper.ParentChildValidationCacheKey, out parentChildCycleCache));
			}
		}

		public void TestValidateDependency_ForTemplateLink_WhenTemplateNotSet_ShouldReportException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", "PAV", "BUF", "FIX", "GPR", name: "Apple");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", "PAV", "BUF", name: "Vapple");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Snarry");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Hape");

			var link = Factory.New<ProcessHeaderLink>();

			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
				link.FP_FH_HeaderFrom = workflow1.PK;
				link.FP_FH_HeaderTo = workflow2.PK;
			}

			AssertMultilineASCIIEquals("",
@"This ProcessHeaderLink doesn't have a Template selected, but is a template ProcessHeaderLink. It must have been created independenly of ProcessHeaderLinkCollection, or I didn't write code properly.
From Workflow Template: Apple
To Workflow Template: Vapple
Type = DEP
Snarry -> Hape", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestExternalTemplateWorkflows_BothWorkflowsCannotBeExternal()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT");
			var template3 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "GLW");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(template2);
			var workflow2_2 = BMSTestHelper.CreateWorkflow(template2);
			var workflow3 = BMSTestHelper.CreateWorkflow(template3);

			var link = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			link.FromWorkflowExternalTemplatePK = template1.PK;
			link.FP_FH_HeaderFrom = workflow1.PK;
			link.FP_FH_HeaderTo = workflow2_2.PK;

			AssertNoErrors(link);

			link.ToWorkflowExternalTemplatePK = template3.PK;
			link.FP_FH_HeaderTo = workflow3.PK;

			AssertHasError(link.FP_FH_HeaderFromInfo, "Both the 'from' and the 'to' workflow cannot be on other Workflow Templates. This link would never be applied to a job.");
			AssertHasError(link.FP_FH_HeaderToInfo, "Both the 'from' and the 'to' workflow cannot be on other Workflow Templates. This link would never be applied to a job.");

			link.FromWorkflowExternalTemplatePK = ZGuid.Empty;
			link.FP_FH_HeaderFrom = workflow2_1.PK;

			AssertNoErrors(link);

			link.ToWorkflowExternalTemplatePK = ZGuid.Empty;
			link.FP_FH_HeaderTo = workflow2_2.PK;

			AssertNoErrors(link);
		}

		public void TestValidateTemplateDependencyLinks_WhenLoopDependencyRegistryItemDisabled_ShouldNotDetectPotentialLoopDependencies()
		{
			WorkflowDataRegistry.Instance.EnableTemplatePotentialLoopValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");
			var template3 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", name: "Template 3");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Same workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Same workflow");
			var workflow3 = BMSTestHelper.CreateWorkflow(template3, "Different workflow");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template3.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link1.ToWorkflowExternalTemplatePK = template1.PK;
			link1.FP_FH_HeaderFrom = workflow3.PK;
			link1.FP_FH_HeaderTo = workflow1.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link2.ToWorkflowExternalTemplatePK = template3.PK;
			link2.FP_FH_HeaderFrom = workflow2.PK;
			link2.FP_FH_HeaderTo = workflow3.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoWarnings(link1.FP_FH_HeaderFromInfo);
			AssertNoWarnings(link1.FP_FH_HeaderToInfo);
			AssertNoWarnings(link2.FP_FH_HeaderFromInfo);
			AssertNoWarnings(link2.FP_FH_HeaderToInfo);
		}
		public void TestValidateTemplateDependencyLinks_InvolvingTwoTemplateWorkflowsInDifferentTemplatesButSameDescription_DifferentTemplateCompany_ShouldNotDetectPotentialLoopedDependency()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");
			template2.P0_GC = Factory.New<GlbCompany>().PK;
			var template3 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", name: "Template 3");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Same workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Same workflow");
			var workflow3 = BMSTestHelper.CreateWorkflow(template3, "Different workflow");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template3.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link1.ToWorkflowExternalTemplatePK = template1.PK;
			link1.FP_FH_HeaderFrom = workflow3.PK;
			link1.FP_FH_HeaderTo = workflow1.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link2.ToWorkflowExternalTemplatePK = template3.PK;
			link2.FP_FH_HeaderFrom = workflow2.PK;
			link2.FP_FH_HeaderTo = workflow3.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoErrors(link1.FP_FH_HeaderFromInfo);
			AssertNoErrors(link1.FP_FH_HeaderToInfo);
			AssertNoErrors(link2.FP_FH_HeaderFromInfo);
			AssertNoErrors(link2.FP_FH_HeaderToInfo);
		}

		public void TestValidateTemplateParentChildLinks_InvolvingTwoTemplateWorkflowsInDifferentTemplatesButSameDescription_DifferentTemplateCompany_ShouldNotDetectPotentialLoopedRelationship()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");
			template2.P0_GC = Factory.New<GlbCompany>().PK;
			var template3 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", name: "Template 3");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Same workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Same workflow");
			var workflow3 = BMSTestHelper.CreateWorkflow(template3, "Different workflow");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template3.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link1.ToWorkflowExternalTemplatePK = template1.PK;
			link1.FP_FH_HeaderFrom = workflow3.PK;
			link1.FP_FH_HeaderTo = workflow1.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link2.ToWorkflowExternalTemplatePK = template3.PK;
			link2.FP_FH_HeaderFrom = workflow2.PK;
			link2.FP_FH_HeaderTo = workflow3.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoErrors(link1.FP_FH_HeaderFromInfo);
			AssertNoErrors(link1.FP_FH_HeaderToInfo);
			AssertNoErrors(link2.FP_FH_HeaderFromInfo);
			AssertNoErrors(link2.FP_FH_HeaderToInfo);
		}

		public void TestValidateTemplateDependencyLinks_TwoJobTypes_TemplatesNotLinked_ShouldNotDetectPotentialLoopedDependency()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", name: "Template 2");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Workflow A");
			var workflow2 = BMSTestHelper.CreateWorkflow(template1, "Workflow B");
			var workflow3 = BMSTestHelper.CreateWorkflow(template2, "Workflow A");
			var workflow4 = BMSTestHelper.CreateWorkflow(template2, "Workflow B");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link1.FP_FH_HeaderFrom = workflow1.PK;
			link1.FP_FH_HeaderTo = workflow2.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link2.FP_FH_HeaderFrom = workflow4.PK;
			link2.FP_FH_HeaderTo = workflow3.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoErrors(link1.FP_FH_HeaderFromInfo);
			AssertNoErrors(link1.FP_FH_HeaderToInfo);
			AssertNoErrors(link2.FP_FH_HeaderFromInfo);
			AssertNoErrors(link2.FP_FH_HeaderToInfo);
		}

		public void TestValidateTemplateDependencyLinks_TwoJobTypes_TemplatesNotLinked_ShouldNotDetectPotentialLoopedRelationship()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", name: "Template 2");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Workflow A");
			var workflow2 = BMSTestHelper.CreateWorkflow(template1, "Workflow B");
			var workflow3 = BMSTestHelper.CreateWorkflow(template2, "Workflow A");
			var workflow4 = BMSTestHelper.CreateWorkflow(template2, "Workflow B");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link1.FP_FH_HeaderFrom = workflow1.PK;
			link1.FP_FH_HeaderTo = workflow2.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link2.FP_FH_HeaderFrom = workflow4.PK;
			link2.FP_FH_HeaderTo = workflow3.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			AssertNoErrors(link1.FP_FH_HeaderFromInfo);
			AssertNoErrors(link1.FP_FH_HeaderToInfo);
			AssertNoErrors(link2.FP_FH_HeaderFromInfo);
			AssertNoErrors(link2.FP_FH_HeaderToInfo);
		}

		public void TestValidateTemplateDependencyLinks_InvolvingTwoTemplateWorkflowsInDifferentTemplatesButSameDescription_SameJobType_DifferentCompany_ShouldNotDetectPotentialLoopedRelationship()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI" });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var workflow1a = BMSTestHelper.CreateWorkflow(template1, "Workflow A");
			var workflow1b = BMSTestHelper.CreateWorkflow(template1, "Workflow B");

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");
			template2.P0_GC = Factory.New<GlbCompany>().PK;
			var workflow2a = BMSTestHelper.CreateWorkflow(template2, "Workflow A");
			var workflow2b = BMSTestHelper.CreateWorkflow(template2, "Workflow B");

			Factory.Save();

			var link1 = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link1.FP_FH_HeaderFrom = workflow1a.PK;
			link1.FP_FH_HeaderTo = workflow1b.PK;

			var link2 = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			link2.FP_FH_HeaderFrom = workflow2b.PK;
			link2.FP_FH_HeaderTo = workflow2a.PK;

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertNoErrors(link1.FP_FH_HeaderFromInfo);
				AssertNoErrors(link1.FP_FH_HeaderToInfo);
				AssertNoErrors(link2.FP_FH_HeaderFromInfo);
				AssertNoErrors(link2.FP_FH_HeaderToInfo);
			});
		}

		public void TestValidateTemplateDependencyLinks_WhenTemplateFallsBack_WhenOneWorkflowNotYetSpecified_ShouldNotThrowExceptions()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Workflow A");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Workflow B");

			var link = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link.FP_FH_HeaderFrom = workflow2.PK;

			AssertNoExceptionThrown("Running validation on a link with no header to set yet should not throw exceptions. It's an invalid situation but not an exceptional one. SAD!", () => link.RunPreSaveValidation());
			AssertHasError("The validation should have run.", link.FP_FH_HeaderToInfo, "Please enter a To Workflow.");
		}

		public void TestValidateTemplateDependencyLinks_ShouldValidateLoops_EvenWhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameTemplate_PostreqLinkCreated()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", name: "Template");
			var header1 = BMSTestHelper.CreateWorkflow(template, "Looped workflow A");
			var header2 = BMSTestHelper.CreateWorkflow(template, "Looped workflow B");
			var stubHeader = BMSTestHelper.CreateWorkflow(template, "Stub workflow");

			BMSTestHelper.CreateDependencyLink(template, header1, header2);
			BMSTestHelper.CreateDependencyLink(template, header2, header1);

			// postreq link to the stub workflow
			var linkToStubHeader = BMSTestHelper.CreateDependencyLink(template, header2, stubHeader);

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Template.Looped workflow A
Template.Looped workflow B
Template.Stub workflow");
		}

		public void TestValidateTemplateDependencyLinks_ShouldValidateLoops_EvenWhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameTemplate_PrereqLinkCreated()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", name: "Template");
			var header1 = BMSTestHelper.CreateWorkflow(template, "Looped workflow A");
			var header2 = BMSTestHelper.CreateWorkflow(template, "Looped workflow B");
			var stubHeader = BMSTestHelper.CreateWorkflow(template, "Stub workflow");

			BMSTestHelper.CreateDependencyLink(template, header1, header2);
			BMSTestHelper.CreateDependencyLink(template, header2, header1);

			// prereq link to the stub workflow
			var linkToStubHeader = BMSTestHelper.CreateDependencyLink(template, stubHeader, header2);

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Template.Looped workflow A
Template.Looped workflow B
Template.Stub workflow");
		}

		public void TestValidateTemplateDependencyLinks_ShouldValidateLoops_EvenWhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameTemplate_ParentLinkCreated()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", name: "Template");
			var header1 = BMSTestHelper.CreateWorkflow(template, "Looped workflow A");
			var header2 = BMSTestHelper.CreateWorkflow(template, "Looped workflow B");
			var stubHeader = BMSTestHelper.CreateWorkflow(template, "Stub workflow");

			BMSTestHelper.CreateDependencyLink(template, header1, header2);
			BMSTestHelper.CreateDependencyLink(template, header2, header1);

			// parent link to the stub workflow
			var linkToStubHeader = BMSTestHelper.CreateDependencyLink(template, header2, stubHeader);
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Template.Looped workflow A
Template.Looped workflow B
Template.Stub workflow");
		}

		public void TestValidateTemplateDependencyLinks_ShouldValidateLoops_EvenWhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameTemplate_ChildLinkCreated()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "DUM" });

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", name: "Template");
			var header1 = BMSTestHelper.CreateWorkflow(template, "Looped workflow A");
			var header2 = BMSTestHelper.CreateWorkflow(template, "Looped workflow B");
			var stubHeader = BMSTestHelper.CreateWorkflow(template, "Stub workflow");

			BMSTestHelper.CreateDependencyLink(template, header1, header2);
			BMSTestHelper.CreateDependencyLink(template, header2, header1);

			// child link to the stub workflow
			var linkToStubHeader = BMSTestHelper.CreateDependencyLink(template, stubHeader, header2);
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Template.Looped workflow A
Template.Looped workflow B
Template.Stub workflow");
		}

		public void TestLinkType()
		{
			var link = Factory.New<ProcessHeaderLink>();

			foreach (ICodeDescription item in new ProcessHeaderLinkTypeList())
			{
				link.FP_LinkType = item.Code;
				AssertNoErrors(link.FP_LinkTypeInfo);
			}
		}

		public void TestWorkflowCanOnlyHaveOneParent()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			var link1 = childWorkflow.GetOrCreateLinkToParent(workflow1);
			AssertNoErrors(link1);

			var link2 = childWorkflow.GetOrCreateLinkToParent(workflow2);
			AssertHasError(link2.FP_FH_HeaderFromInfo, "The 'from' workflow already has a parent.");

			link1.Delete();
			link2.Validation.ValidateAll();
			AssertNoErrors(link2);
		}

		public void TestFromToDifferent()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_FH_ParentHeader = ZGuid.NewZGuid();
			var otherHeader = Factory.NewWithValidTestData<ProcessHeader>();
			otherHeader.FH_FH_ParentHeader = ZGuid.NewZGuid();
			var link = header.LinksFromMeToOthers_ForBinding.AddNew();

			link.FP_FH_HeaderFrom = header.PK;
			link.FP_FH_HeaderTo = otherHeader.PK;
			AssertNoErrors(link.FP_FH_HeaderFromInfo);
			AssertNoErrors(link.FP_FH_HeaderToInfo);

			link.FP_FH_HeaderTo = header.PK;
			AssertHasErrors(link.FP_FH_HeaderToInfo);

			link.FP_FH_HeaderFrom = ZGuid.Empty;
			link.FP_FH_HeaderFrom = header.PK;
			AssertHasErrors(link.FP_FH_HeaderFromInfo);
			AssertHasErrors(link.FP_FH_HeaderToInfo);
		}

		public void TestUniqueness()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_FH_ParentHeader = ZGuid.NewZGuid();
			var otherHeader = Factory.NewWithValidTestData<ProcessHeader>();
			otherHeader.FH_FH_ParentHeader = ZGuid.NewZGuid();
			var yetAnotherHeader = Factory.NewWithValidTestData<ProcessHeader>();
			yetAnotherHeader.FH_FH_ParentHeader = ZGuid.NewZGuid();

			var link1 = header.ChildLinks_ForBinding.AddNew();
			link1.FP_FH_HeaderFrom = otherHeader.PK;
			AssertNoErrors(link1.FP_FH_HeaderFromInfo);
			AssertNoErrors(link1.FP_FH_HeaderToInfo);

			var link2 = header.ChildLinks_ForBinding.AddNew();
			link2.FP_FH_HeaderFrom = otherHeader.PK;
			AssertHasError(link2.FP_FH_HeaderFromInfo, "The From Workflow has been duplicated and must be unique.");

			link2.FP_FH_HeaderFrom = yetAnotherHeader.PK;
			AssertNoErrors(link2.FP_FH_HeaderFromInfo);
		}

		public void TestDbHitsDuringValidation()
		{
			AssertEquals(Factory.DatabaseLoadCount, 0);

			var header = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false), "W1");
			var otherHeader = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false), "W1");
			var yetAnotherHeader = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, false), "W1");

			var link1 = header.ChildLinks_ForBinding.AddNew();
			link1.FP_FH_HeaderFrom = otherHeader.PK;

			var link2 = header.ChildLinks_ForBinding.AddNew();
			link2.FP_FH_HeaderFrom = yetAnotherHeader.PK;

			var link3 = header.PrerequisiteLinks_ForBinding.AddNew();
			link3.FP_FH_HeaderFrom = otherHeader.PK;

			Factory.ResetDatabaseLoadCount();

			link1.RunPreSaveValidation();
			link2.RunPreSaveValidation();
			link3.RunPreSaveValidation();

			AssertEquals(0, Factory.DatabaseLoadCount);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedLink2 = newFactory.Load<ProcessHeaderLink>(link2.PK);

			newFactory.ResetDatabaseLoadCount();
			loadedLink2.MarkAsNeedingValidation();
			loadedLink2.RunPreSaveValidation();

			AssertDbHits(new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 4 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 4 },
			}, newFactory);

			loadedLink2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency; // Link now has relevant changes, so should do full network validation.
			loadedLink2.MarkAsNeedingValidation();
			loadedLink2.RunPreSaveValidation();

			AssertDbHits(new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 9 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 13 },
			}, newFactory);
		}

		public void TestDbHitsDuringValidation_ManyLinks()
		{
			var link = CreateJobWithManyLinks();
			var loadedLink = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessHeaderLink>(link.PK);

			using (AssertDbHitsForAllFactories(new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 5 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 131 }, // seems high, but we need one for each level of the heirarchy. SAD!
			}, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				ValidateLink(loadedLink);
			}
		}

		public void TestValidation_ShouldNotCreateTerribleProcessHeaderLinkQueries()
		{
			var link = CreateJobWithManyLinks();
			var loadedLink = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessHeaderLink>(link.PK);
			IEnumerable<string> trackedCommands;

			using (TestConnection.TrackExecutedCommands())
			{
				ValidateLink(loadedLink);
				trackedCommands = TestConnection.ExecutedCommands;
			}

			var processHeaderLinkCommands = trackedCommands.Where(x => x.Contains(nameof(ProcessHeaderLink))).ToArray();
			AssertEquals(131, processHeaderLinkCommands.Length);

			var badCommands = processHeaderLinkCommands.Where(x => StringExtension.Contains(x, " or FP_FH_Header", StringComparison.OrdinalIgnoreCase));
			AssertContainsExactElementsInAnyOrder("Validating link loops should not use queries with billions and billions of ORs (which perform horribly). Better to have more quick DB hits than fewer really terrible ones. SAD!", Array.Empty<string>(), badCommands);
		}

		static void ValidateLink(ProcessHeaderLink link)
		{
			using (link.EmulatePreSaveValidation_ForTest())
			{
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
				link.Validation.ValidateAll();
			}
		}

		ProcessHeaderLink CreateJobWithManyLinks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var topWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "top workflow");
			ProcessHeaderLink firstLink = null;

			Factory.SuspendValidation();

			const int numberOfLevelsInEveryDirection = 3;

			for (var i = 0; i < numberOfLevelsInEveryDirection; i++)
			{
				AddLayerOfPostreqs(i, topWorkflow);
			}

			void AddLayerOfPostreqs(int layer, ProcessHeader firstWorkflow)
			{
				for (var postreqCounter = 0; postreqCounter < numberOfLevelsInEveryDirection; postreqCounter++)
				{
					var postreqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "postreq " + layer);
					var link = firstWorkflow.GetOrCreateDependencyLink(postreqWorkflow);

					if (firstLink == null)
					{
						firstLink = link;
					}

					if (layer < numberOfLevelsInEveryDirection)
					{
						layer++;
						AddLayerOfPostreqs(layer, postreqWorkflow);
					}

					var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "child " + layer);
					BMSTestHelper.MakeChildOf(childWorkflow, postreqWorkflow);
				}
			}

			Factory.Save();
			Factory.ResumeValidation();

			return firstLink;
		}

		public void TestMakeDependency_LoopedDependency()
		{
			var header1 = Factory.NewWithValidTestData<ProcessHeader>();
			header1.FH_CompletionStatement = "Berkflow1";
			var header2 = Factory.NewWithValidTestData<ProcessHeader>();
			header2.FH_CompletionStatement = "Berkflow2";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			link1_2.Validation.ValidateAll();
			link2_1.Validation.ValidateAll();

			var message = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Berkflow1
Berkflow2";
			AssertNoError(link1_2.FP_FH_HeaderToInfo, message);
			AssertNoError(link2_1.FP_FH_HeaderToInfo, message);

			link1_2.RunPreSaveValidation();
			link2_1.RunPreSaveValidation();

			AssertHasError(link1_2.FP_FH_HeaderToInfo, message);
			AssertHasError(link2_1.FP_FH_HeaderToInfo, message);
		}

		public void TestMakeDependency_LoopedDependencyAcrossJob()
		{
			var job1 = Factory.New<DummyWithWorkflow>();
			var job2 = Factory.New<DummyWithWorkflow>();

			var header1 = VisualBoardsTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(job1, Factory), "W1");
			var header2 = VisualBoardsTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(job2, Factory), "W2");
			var header3 = VisualBoardsTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(job2, Factory), "W3");

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_3 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_3.FP_FH_HeaderTo = header3.PK;
			link2_3.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link3_1 = header3.LinksFromMeToOthers_ForBinding.AddNew();
			link3_1.FP_FH_HeaderTo = header1.PK;
			link3_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			link1_2.Validation.ValidateAll();
			link2_3.Validation.ValidateAll();
			link3_1.Validation.ValidateAll();

			var message = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - W1
Dummy Business Object Default - W2
Dummy Business Object Default - W3";

			AssertNoError(link1_2.FP_FH_HeaderToInfo, message);
			AssertNoError(link2_3.FP_FH_HeaderToInfo, message);
			AssertNoError(link3_1.FP_FH_HeaderToInfo, message);

			link1_2.RunPreSaveValidation();
			link2_3.RunPreSaveValidation();
			link3_1.RunPreSaveValidation();

			AssertHasError(link1_2.FP_FH_HeaderToInfo, message);
			AssertHasError(link2_3.FP_FH_HeaderToInfo, message);
			AssertHasError(link3_1.FP_FH_HeaderToInfo, message);

			link3_1.Delete();

			link1_2.Validation.ValidateAll();
			link2_3.Validation.ValidateAll();

			AssertNoError(link1_2.FP_FH_HeaderToInfo, message);
			AssertNoError(link2_3.FP_FH_HeaderToInfo, message);
		}

		public void TestMakeDependency_WorkflowDependantOnOwnJobShouldBeInvalid()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var link = jobHeader1.GetOrCreateDependencyLink(workflow1);

			link.Validation.ValidateAll();

			AssertEquals(workflow1.ParentHeader, jobHeader1);
			AssertEquals(workflow1.JobHeader, jobHeader1);
			AssertEquals(1, workflow1.GetPrerequisitesUpTheTree().Count());
			Assert(workflow1.GetPrerequisitesUpTheTree().Any(jh => jh == jobHeader1));

			const string errorMessage = "This is a dependency link between workflows that are also involved in a Parent-Child relationship.";
			AssertNoError(link.FP_FH_HeaderToInfo, errorMessage);

			link.RunPreSaveValidation();

			AssertHasError(link.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestMakeDependency_JobDependantOnOwnWorkflowShouldBeInvalid()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var link = workflow1.GetOrCreateDependencyLink(jobHeader1);

			link.Validation.ValidateAll();

			const string errorMessage = "This is a dependency link between workflows that are also involved in a Parent-Child relationship.";
			AssertNoError(link.FP_FH_HeaderToInfo, errorMessage);

			link.RunPreSaveValidation();

			AssertHasError(link.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestMakeDependency_WorkflowDependantOnOwnJobExtended()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy, Factory);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "W1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(dummy2, Factory), "W2");
			var link1 = jobHeader1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow2.GetOrCreateDependencyLink(workflow1);

			AssertEquals(workflow1.ParentHeader, jobHeader1);
			AssertEquals(workflow1.JobHeader, jobHeader1);
			AssertEquals(2, workflow1.GetPrerequisitesUpTheTree().Count());
			Assert(workflow1.GetPrerequisitesUpTheTree().Any(jh => jh == jobHeader1));

			const string errorMessage = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Job Dummy Business Object Default is complete.
Dummy Business Object Default - W1
Dummy Business Object Default - W2";

			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();

			AssertNoError(link1.FP_FH_HeaderToInfo, errorMessage);
			AssertNoError(link2.FP_FH_HeaderToInfo, errorMessage);

			link1.RunPreSaveValidation();

			AssertHasError(link1.FP_FH_HeaderToInfo, errorMessage);
			AssertNoError(link2.FP_FH_HeaderToInfo, errorMessage);

			link2.RunPreSaveValidation();

			AssertHasError(link1.FP_FH_HeaderToInfo, errorMessage);
			AssertHasError(link2.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestMakeDependency_JobDependantOnOwnWorkflowExtended()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy, Factory);
			var workflow1 = VisualBoardsTestHelper.CreateWorkflow(jobHeader1, "W1");
			var workflow2 = VisualBoardsTestHelper.CreateWorkflow(ProcessJobHeader.GetForParent(dummy2, Factory), "W2");
			var link1 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2 = workflow2.GetOrCreateDependencyLink(jobHeader1);

			link2.Validation.ValidateAll();

			const string errorMessage = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Job Dummy Business Object Default is complete.
Dummy Business Object Default - W1
Dummy Business Object Default - W2";

			AssertNoError(link2.FP_FH_HeaderToInfo, errorMessage);

			link2.RunPreSaveValidation();

			AssertHasError(link2.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestMakeDependency_ShouldNotValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameJob_PostreqLinkCreated()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";
			var stubHeader = jobHeader.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// postreq link to the stub workflow
			var linkToStubHeader = header2.LinksFromMeToOthers_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderTo = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.RunPreSaveValidation();

			AssertNoErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");

			var linkToIncludeStubHeaderIntoCycle = stubHeader.LinksFromMeToOthers_ForBinding.AddNew();
			linkToIncludeStubHeaderIntoCycle.FP_FH_HeaderTo = header1.PK;
			linkToIncludeStubHeaderIntoCycle.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.MarkAsNeedingValidation();
			linkToStubHeader.RunPreSaveValidation();

			AssertHasErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");
			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_ButHeadersAreWithinDifferentJobs_PostreqLinkCreated()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy1, Factory);
			var header1 = jobHeader1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader1.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";

			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(dummy2, Factory);
			var stubHeader = jobHeader2.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// postreq link to the stub workflow
			var linkToStubHeader = header2.LinksFromMeToOthers_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderTo = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var linkBetweenJobHeaders = jobHeader1.LinksFromOthersToMe_ForBinding.AddNew();
			linkBetweenJobHeaders.FP_FH_HeaderFrom = jobHeader2.PK;
			linkBetweenJobHeaders.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldNotValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameJob_PrereqLinkCreated()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";
			var stubHeader = jobHeader.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// prereq link to the stub workflow
			var linkToStubHeader = header2.LinksFromOthersToMe_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderFrom = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.RunPreSaveValidation();

			AssertNoErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");

			var linkToIncludeStubHeaderIntoCycle = stubHeader.LinksFromOthersToMe_ForBinding.AddNew();
			linkToIncludeStubHeaderIntoCycle.FP_FH_HeaderFrom = header1.PK;
			linkToIncludeStubHeaderIntoCycle.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.MarkAsNeedingValidation();
			linkToStubHeader.RunPreSaveValidation();

			AssertHasErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");
			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_ButHeadersAreWithinDifferentJobs_PrereqLinkCreated()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy1, Factory);
			var header1 = jobHeader1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader1.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";

			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(dummy2, Factory);
			var stubHeader = jobHeader2.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// prereq link to the stub workflow
			var linkToStubHeader = header2.LinksFromOthersToMe_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderFrom = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var linkBetweenJobHeaders = jobHeader1.LinksFromMeToOthers_ForBinding.AddNew();
			linkBetweenJobHeaders.FP_FH_HeaderTo = jobHeader2.PK;
			linkBetweenJobHeaders.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Job Dummy Business Object Default is complete.
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldNotValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameJob_ParentLinkCreated()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";
			var stubHeader = jobHeader.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// parent link to the stub workflow
			var linkToStubHeader = header2.LinksFromMeToOthers_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderTo = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertNoErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");

			var linkToIncludeStubHeaderIntoCycle = stubHeader.LinksFromMeToOthers_ForBinding.AddNew();
			linkToIncludeStubHeaderIntoCycle.FP_FH_HeaderTo = header1.PK;
			linkToIncludeStubHeaderIntoCycle.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.MarkAsNeedingValidation();
			linkToStubHeader.RunPreSaveValidation();

			AssertHasErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");
			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_ButHeadersAreWithinDifferentJobs_ParentLinkCreated()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy1, Factory);
			var header1 = jobHeader1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader1.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";

			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(dummy2, Factory);
			var stubHeader = jobHeader2.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// parent link to the stub workflow
			var linkToStubHeader = header2.LinksFromMeToOthers_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderTo = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			var linkBetweenJobHeaders = jobHeader1.LinksFromOthersToMe_ForBinding.AddNew();
			linkBetweenJobHeaders.FP_FH_HeaderFrom = jobHeader2.PK;
			linkBetweenJobHeaders.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldNotValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_AndBothHeadersAreWithinTheSameJob_ChildLinkCreated()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";
			var stubHeader = jobHeader.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// child link to the stub workflow
			var linkToStubHeader = header2.LinksFromOthersToMe_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderFrom = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertNoErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");

			var linkToIncludeStubHeaderIntoCycle = stubHeader.LinksFromOthersToMe_ForBinding.AddNew();
			linkToIncludeStubHeaderIntoCycle.FP_FH_HeaderFrom = header1.PK;
			linkToIncludeStubHeaderIntoCycle.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			linkToStubHeader.MarkAsNeedingValidation();
			linkToStubHeader.RunPreSaveValidation();

			AssertHasErrorContaining(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:");
			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestMakeDependency_ShouldValidateLoops_WhenCreatedLinkIsTheOnlyLinkOnProcessHeader_ButHeadersAreWithinDifferentJobs_ChildLinkCreated()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(dummy1, Factory);
			var header1 = jobHeader1.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Looped workflow A";
			var header2 = jobHeader1.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Looped workflow B";

			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(dummy2, Factory);
			var stubHeader = jobHeader2.ProcessHeaders.AddNew();
			stubHeader.FH_CompletionStatement = "Stub workflow";

			var link1_2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = header2.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link2_1 = header2.LinksFromMeToOthers_ForBinding.AddNew();
			link2_1.FP_FH_HeaderTo = header1.PK;
			link2_1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			// child link to the stub workflow
			var linkToStubHeader = header2.LinksFromOthersToMe_ForBinding.AddNew();
			linkToStubHeader.FP_FH_HeaderFrom = stubHeader.PK;
			linkToStubHeader.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			var linkBetweenJobHeaders = jobHeader1.LinksFromMeToOthers_ForBinding.AddNew();
			linkBetweenJobHeaders.FP_FH_HeaderTo = jobHeader2.PK;
			linkBetweenJobHeaders.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			linkToStubHeader.RunPreSaveValidation();

			AssertHasError(linkToStubHeader.FP_FH_HeaderToInfo, @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Looped workflow A
Dummy Business Object Default - Looped workflow B
Dummy Business Object Default - Stub workflow");
		}

		public void TestCheckHeaderTo_NullHeaderTo_ShouldNotExplode()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders.AddNew();
			var link = workflow.LinksFromMeToOthers_ForBinding.AddNew();
			AssertNoExceptionThrown(link.Validation.ValidateAll);
		}

		public void TestCheckHeaderTo_NullHeaderFrom_ShouldNotExplode()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders.AddNew();
			var link = workflow.LinksFromOthersToMe_ForBinding.AddNew();
			AssertNoExceptionThrown(link.Validation.ValidateAll);
		}

		public void TestCheckLink_NullFP_FH_HeaderFrom_ShouldNotExplode_WhenSave()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Same workflow Name");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Same workflow Name");

			Factory.Save();

			var link = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link.ToWorkflowExternalTemplatePK = template1.PK;
			link.FP_FH_HeaderFrom = ZGuid.Empty;
			link.FP_FH_HeaderTo = workflow1.PK;

			AssertNoExceptionThrown(link.RunPreSaveValidation);
			AssertHasError(link.FP_FH_HeaderFromInfo, "Please enter a From Workflow.");
		}

		public void TestCheckLink_NullFP_FH_HeaderTo_ShouldNotExplode_WhenSave()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "WKI", "ORG" });
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT", name: "Template 2");

			var workflow1 = BMSTestHelper.CreateWorkflow(template1, "Same workflow Name");
			var workflow2 = BMSTestHelper.CreateWorkflow(template2, "Same workflow Name");

			Factory.Save();

			var link = (ProcessHeaderLink)template2.ProcessHeaderLinks.AddNew();
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link.ToWorkflowExternalTemplatePK = template1.PK;
			link.FP_FH_HeaderFrom = workflow2.PK;
			link.FP_FH_HeaderTo = ZGuid.Empty;

			AssertNoExceptionThrown(link.RunPreSaveValidation);
			AssertHasError(link.FP_FH_HeaderToInfo, "Please enter a To Workflow.");
		}

		public void TestCircularDependencyBetweenToWorkflowInDeferentWorkitem()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job1", addDefaultProcessHeaderIfNone: false);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Coding workflow 1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "Reviewing workflow 1");
			var workflow1_3 = BMSTestHelper.CreateWorkflow(jobHeader1, "Development 1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job2", addDefaultProcessHeaderIfNone: false);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "Coding workflow 2");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Reviewing workflow 2");
			var workflow2_3 = BMSTestHelper.CreateWorkflow(jobHeader2, "Development 2");

			var link11_12 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			var link12_13 = workflow1_2.GetOrCreateDependencyLink(workflow1_3);

			var link21_22 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			var link22_23 = workflow2_2.GetOrCreateDependencyLink(workflow2_3);

			link11_12.RunPreSaveValidation();
			link12_13.RunPreSaveValidation();
			link21_22.RunPreSaveValidation();
			link22_23.RunPreSaveValidation();

			AssertNoErrors(link11_12.FP_FH_HeaderToInfo);
			AssertNoErrors(link12_13.FP_FH_HeaderToInfo);
			AssertNoErrors(link21_22.FP_FH_HeaderToInfo);
			AssertNoErrors(link22_23.FP_FH_HeaderToInfo);

			var link13_2 = workflow1_3.GetOrCreateDependencyLink(jobHeader2);
			var parentChildLink = workflow2_2.GetOrCreateLinkToParent(workflow1_2);

			Factory.ValidationCache.CachedData.Clear();

			parentChildLink.RunPreSaveValidation();

			var expectedError = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Coding workflow 2
Dummy Business Object Default - Development 1
Dummy Business Object Default - Reviewing workflow 1
Dummy Business Object Default - Reviewing workflow 2";
			AssertHasError("Loop dependency", parentChildLink.FP_FH_HeaderFromInfo, expectedError);

			parentChildLink.Delete();
			var link22_12 = workflow2_2.GetOrCreateDependencyLink(workflow1_2);

			link22_12.Validation.ValidateAll();

			expectedError = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - Coding workflow 2
Dummy Business Object Default - Development 1
Dummy Business Object Default - Reviewing workflow 1
Dummy Business Object Default - Reviewing workflow 2";

			AssertNoError("Loop dependency", link22_12.FP_FH_HeaderToInfo, expectedError);

			link22_12.RunPreSaveValidation();

			AssertHasError("Loop dependency", link22_12.FP_FH_HeaderToInfo, expectedError);
		}

		public void TestNoCircularDependencyBetweenDeferentWorkitems()
		{
			var job174011 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "174011 job");

			var job173877 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "173877 job");

			var job173871 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "173871 job");

			job174011.GetOrCreateDependencyLink(job173877);
			job173871.GetOrCreateDependencyLink(job173877);

			var job174505 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "174505 job", addDefaultProcessHeaderIfNone: false);
			var workflow174505 = VisualBoardsTestHelper.CreateWorkflow(job174505, "Implementation Coding");

			job173877.GetOrCreateDependencyLink(job174505);

			var job173879 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "173879 job");
			job174505.GetOrCreateDependencyLink(job173879);

			var job174172 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "174172 job", addDefaultProcessHeaderIfNone: false);
			var workflow174172 = VisualBoardsTestHelper.CreateWorkflow(job174172, "PROJECT EVALUATION");

			var job176066 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "176066 job");
			job176066.GetOrCreateLinkToParent(job174172);
			job176066.GetOrCreateDependencyLink(workflow174505);

			var job175961 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "175961 job");
			job175961.GetOrCreateLinkToParent(job174172);

			var job166650 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "166650 job");

			var job168755 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "168755 job", addDefaultProcessHeaderIfNone: false);
			var workflow168755 = VisualBoardsTestHelper.CreateWorkflow(job168755, "DETAILED NCN/CCPM");
			job168755.GetOrCreateLinkToParent(job166650);
			workflow168755.GetOrCreateDependencyLink(workflow174172);
			workflow168755.GetOrCreateDependencyLink(job174172);
			workflow168755.GetOrCreateDependencyLink(job173879);

			var job171698 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "171698 job", addDefaultProcessHeaderIfNone: false);
			var workflow171698 = VisualBoardsTestHelper.CreateWorkflow(job171698, "DETAILED NCN/CCPM");

			var job170349 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "170349 job");
			job168755.GetOrCreateLinkToParent(job171698);
			workflow171698.GetOrCreateDependencyLink(job170349);

			var job169202 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "169202 job");

			var job167816 = VisualBoardsTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "167816 job", addDefaultProcessHeaderIfNone: false);
			job167816.GetOrCreateLinkToParent(job169202);
			var workflow167816 = VisualBoardsTestHelper.CreateWorkflow(job167816, "HLD");

			job170349.GetOrCreateDependencyLink(workflow167816);
			job167816.GetOrCreateDependencyLink(job173871);

			var link = job176066.GetOrCreateDependencyLink(job175961);
			link.RunPreSaveValidation();
			AssertNoErrors(link.FP_FH_HeaderFromInfo);
			AssertNoErrors(link.FP_FH_HeaderToInfo);
		}

		public void TestPrerequisiteCircularDependency_WhenTheyHaveParentChildDependency()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1");
			var link = workflow1_2.GetOrCreateDependencyLink(jobHeader1);

			link.Validation.ValidateAll();

			const string errorMessage = "This is a dependency link between workflows that are also involved in a Parent-Child relationship.";

			AssertNoError(link.FP_FH_HeaderToInfo, errorMessage);

			link.RunPreSaveValidation();

			AssertHasError(link.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestParentChild_WhenPrerequisiteCircularDependencyExists()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "AAA");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "BBB");

			workflow2.MakePrerequisiteOf(workflow1);
			var link1_2 = workflow1.PostrequisiteLinks_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = workflow2.PK;
			var parentLink = workflow1.GetOrCreateLinkToParent(workflow2);

			AssertHasError(parentLink.FP_FH_HeaderToInfo, "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.");
		}

		public void TestParentChild_ShouldNotErrorOnValidEdge_WhenPrerequisiteCircularDependencyExists()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "AAA");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "BBB");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "CCC");

			workflow2.MakePrerequisiteOf(workflow1);
			var link1_2 = workflow1.PostrequisiteLinks_ForBinding.AddNew();
			link1_2.FP_FH_HeaderTo = workflow2.PK;
			var parentLink = workflow1.GetOrCreateLinkToParent(workflow3);

			var errorMessage = "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.";
			AssertNoError(parentLink.FP_FH_HeaderToInfo, errorMessage);

			parentLink.FP_FH_HeaderTo = workflow2.PK;
			AssertHasError(parentLink.FP_FH_HeaderToInfo, errorMessage);
		}

		public void TestParentChildCircularDependency_WhenTheyHavePrerequisiteDependency()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var current = BMSTestHelper.CreateWorkflow(jobHeader, "current");
			var parent = BMSTestHelper.CreateWorkflow(jobHeader, "parent");
			var child = BMSTestHelper.CreateWorkflow(jobHeader, "child");
			current.MakePrerequisiteOf(parent);
			child.MakePrerequisiteOf(current);
			var link = child.GetOrCreateLinkToParent(current);

			link.Validation.ValidateAll();

			AssertHasError(link.FP_FH_HeaderToInfo, "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.");
		}

		public void TestPrePostrequisite_MustNotBeExistingParentOrChildrenOrPostPrerequisite()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var current = BMSTestHelper.CreateWorkflow(jobHeader, "current");
			var parent = BMSTestHelper.CreateWorkflow(jobHeader, "parent");
			var child = BMSTestHelper.CreateWorkflow(jobHeader, "child");
			var grandchild = BMSTestHelper.CreateWorkflow(jobHeader, "grandchild");
			var grandparent = BMSTestHelper.CreateWorkflow(jobHeader, "grandparent");

			var link1 = child.GetOrCreateLinkToParent(current);
			var link2 = grandchild.GetOrCreateLinkToParent(child);
			var link3 = current.GetOrCreateLinkToParent(parent);
			var link4 = parent.GetOrCreateLinkToParent(grandparent);

			var expectedError = "Invalid relationship. These workflows are already in a parent/child relationship and cannot also be in a pre/post requisite relationship. Please note that this may be an indirect relationship to the current workflow.";

			var linkPre = parent.GetOrCreateDependencyLink(current);
			AssertHasError("Pre-requisite MUST NOT be existing parent", linkPre.FP_FH_HeaderFromInfo, expectedError);

			linkPre.FP_FH_HeaderFrom = child.PK;
			AssertHasError("Pre-requisite MUST NOT be existing child", linkPre.FP_FH_HeaderFromInfo, expectedError);

			linkPre.FP_FH_HeaderFrom = grandparent.PK;
			AssertHasError("Pre-requisite MUST NOT be existing grandparent", linkPre.FP_FH_HeaderFromInfo, expectedError);

			linkPre.FP_FH_HeaderFrom = grandchild.PK;
			AssertHasError("Pre-requisite MUST NOT be existing grandchild", linkPre.FP_FH_HeaderFromInfo, expectedError);

			linkPre.Delete();

			var linkPost = current.GetOrCreateDependencyLink(parent);
			AssertHasError("Post-requisite MUST NOT be existing parent", linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.FP_FH_HeaderTo = child.PK;
			AssertHasError("Post-requisite MUST NOT be existing child", linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.FP_FH_HeaderTo = grandparent.PK;
			AssertHasError("Post-requisite MUST NOT be existing grandparent", linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.FP_FH_HeaderTo = grandchild.PK;
			AssertHasError("Post-requisite MUST NOT be existing grandchild", linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.Delete();

			expectedError = @"This link is part of a looped Parent-Child relationship. The following workflows are involved in a loop:
Dummy Business Object Default - child
Dummy Business Object Default - current";

			var linkParent = current.ParentLinks_ForBinding.AddNew();
			linkParent.FP_FH_HeaderTo = child.PK;

			AssertNoError(linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.RunPreSaveValidation();

			AssertHasError("Parent MUST NOT be existing child", linkParent.FP_FH_HeaderToInfo, expectedError);

			expectedError = @"This link is part of a looped Parent-Child relationship. The following workflows are involved in a loop:
Dummy Business Object Default - child
Dummy Business Object Default - current
Dummy Business Object Default - grandchild";

			linkParent.FP_FH_HeaderTo = grandchild.PK;

			AssertNoError(linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.RunPreSaveValidation();

			AssertHasError("Parent MUST NOT be existing grand child", linkParent.FP_FH_HeaderToInfo, expectedError);
		}

		public void TestParentChild_MustNotBeExistingPrePostRequisiteOrChildParent()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var current = BMSTestHelper.CreateWorkflow(jobHeader, "current");
			var pre1 = BMSTestHelper.CreateWorkflow(jobHeader, "pre1");
			var post1 = BMSTestHelper.CreateWorkflow(jobHeader, "post1");
			var pre2 = BMSTestHelper.CreateWorkflow(jobHeader, "pre2");
			var post2 = BMSTestHelper.CreateWorkflow(jobHeader, "post2");

			pre1.GetOrCreateDependencyLink(current);
			pre2.GetOrCreateDependencyLink(pre1);
			current.GetOrCreateDependencyLink(post1);
			post1.GetOrCreateDependencyLink(post2);

			var expectedError = "Invalid relationship. These workflows are already in a pre/post requisite relationship and cannot also be in parent/child relationship. Please note that this may be an indirect relationship to the current workflow.";

			var linkChild = current.ChildLinks_ForBinding.AddNew();
			linkChild.FP_FH_HeaderFrom = post1.PK;
			AssertHasError("Child MUST NOT be existing post-requisite", linkChild.FP_FH_HeaderFromInfo, expectedError);

			linkChild.FP_FH_HeaderFrom = pre1.PK;
			AssertHasError("Child MUST NOT be existing pre-requisite", linkChild.FP_FH_HeaderFromInfo, expectedError);

			linkChild.FP_FH_HeaderFrom = post2.PK;
			AssertHasError("Child MUST NOT be existing 2nd level post-requisite", linkChild.FP_FH_HeaderFromInfo, expectedError);

			linkChild.FP_FH_HeaderFrom = pre2.PK;
			AssertHasError("Child MUST NOT be existing 2nd level pre-requisite", linkChild.FP_FH_HeaderFromInfo, expectedError);

			linkChild.Delete();

			var linkParent = current.ParentLinks_ForBinding.AddNew();
			linkParent.FP_FH_HeaderTo = post1.PK;
			AssertHasError("Parent MUST NOT be existing post-requisite", linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.FP_FH_HeaderTo = pre1.PK;
			AssertHasError("Parent MUST NOT be existing pre-requisite", linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.FP_FH_HeaderTo = post2.PK;
			AssertHasError("Parent MUST NOT be existing 2nd level post-requisite", linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.FP_FH_HeaderTo = pre2.PK;
			AssertHasError("Parent MUST NOT be existing 2nd level pre-requisite", linkParent.FP_FH_HeaderToInfo, expectedError);

			linkParent.Delete();

			expectedError = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - current
Dummy Business Object Default - pre1";

			var linkPost = current.PostrequisiteLinks_ForBinding.AddNew();
			linkPost.FP_FH_HeaderTo = pre1.PK;

			AssertNoError(linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.RunPreSaveValidation();

			AssertHasError("Post MUST NOT be existing pre-requisite", linkPost.FP_FH_HeaderToInfo, expectedError);

			expectedError = @"This link is part of a looped dependency. The following workflows are involved in a loop:
Dummy Business Object Default - current
Dummy Business Object Default - pre1
Dummy Business Object Default - pre2";

			linkPost.FP_FH_HeaderTo = pre2.PK;

			AssertNoError(linkPost.FP_FH_HeaderToInfo, expectedError);

			linkPost.RunPreSaveValidation();

			AssertHasError("Post MUST NOT be existing 2nd level pre-requisite", linkPost.FP_FH_HeaderToInfo, expectedError);
		}

		public void TestLinkValidation_WithInvalidData()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var current = BMSTestHelper.CreateWorkflow(jobHeader, "current");
			var pre = BMSTestHelper.CreateWorkflow(jobHeader, "pre");

			var linkChild = current.ChildLinks_ForBinding.AddNew();
			linkChild.FP_FH_HeaderFrom = ZGuid.Empty;

			var linkPre = current.PrerequisiteLinks_ForBinding.AddNew();
			AssertNoExceptionThrown(() =>
			{
				linkPre.FP_FH_HeaderFrom = pre.PK;
			});
		}

		#region Light Validation

		public void TestShouldSupportLightValidation()
		{
			var link = Factory.New<ProcessHeaderLink>();
			Assert(link is ILightValidationInternals);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
