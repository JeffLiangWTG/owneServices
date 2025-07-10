using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class TagLinkValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "First Controller Staff";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			Factory.Save();
		}

		public void TestScope_ValidationFailsWhenScopeInvalid()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ONO", "Onomatopoeias");
			definition.TGD_IsExclusive = true;
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "POP");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Lelel");
			var task = BMSTestHelper.CreateTask(workflow);
			var tagLink1 = (TagLink)workflow.AddTag(mag1).Link;

			Factory.Save();

			AssertNoErrors(tagLink1.TGL_TGM_MagnitudeInfo);

			definition.TGD_Scope = TagScopeList.Codes.Task;
			tagLink1.Validation.ValidateAll();
			AssertHasError(tagLink1.TGL_TGM_MagnitudeInfo, string.Format("Cannot apply tags from group [{0}] here. {1}", definition.DisplayText, TagScopeList.Descriptions.Task));

			definition.TGD_Scope = TagScopeList.Codes.Workflow;
			tagLink1.Validation.ValidateAll();
			AssertNoErrors(tagLink1.TGL_TGM_MagnitudeInfo);

			tagLink1.TGL_ParentId = task.PK;
			tagLink1.TGL_ParentTableCode = task.TablePrefix;
			tagLink1.Validation.ValidateAll();
			AssertHasError(tagLink1.TGL_TGM_MagnitudeInfo, string.Format("Cannot apply tags from group [{0}] here. {1}", definition.DisplayText, TagScopeList.Descriptions.Workflow));
		}

		public void TestMutuallyExclusiveTagValidation()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ONO", "Onomatopoeias");
			definition.TGD_IsExclusive = true;
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "POP");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "WOW");
			var mag3 = BMSTestHelper.CreateTagMagnitude(definition, "ZAP");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Lelel");
			var tagLink1 = (TagLink)workflow.AddTag(mag1).Link;

			AssertNoErrors(tagLink1.TGL_TGM_MagnitudeInfo);

			var taglink2 = (TagLink)workflow.AddTag(mag2).Link;
			tagLink1.Validation.ValidateAll();
			AssertHasError(tagLink1.TGL_TGM_MagnitudeInfo, string.Format("Cannot apply multiple tags from an exclusive Tag Group [{0}].", definition.DisplayText));

			taglink2.Delete();
			tagLink1.Validation.ValidateAll();
			AssertNoErrors(tagLink1.TGL_TGM_MagnitudeInfo);

			var taglink3 = (TagLink)workflow.AddTag(mag3).Link;
			tagLink1.Validation.ValidateAll();
			AssertHasError(tagLink1.TGL_TGM_MagnitudeInfo, string.Format("Cannot apply multiple tags from an exclusive Tag Group [{0}].", definition.DisplayText));

			definition.TGD_IsExclusive = false;
			tagLink1.Validation.ValidateAll();
			AssertNoErrors(tagLink1.TGL_TGM_MagnitudeInfo);
		}

		public void TestUsageScopeValidation()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library", usageScope: TagUsageScopeList.Codes.Rule);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Get Library Card");
			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			AssertHasError(tag.TGL_TGM_MagnitudeInfo, string.Format("Cannot modify [{0}] from here. Tag Group has scope of [{1}].", tag.DisplayText, TagUsageScopeList.Descriptions.Rule));

			definition.TGD_UsageScope = TagUsageScopeList.Codes.User;

			tag.Validation.ValidateAll();
			AssertNoErrors(tag.TGL_TGM_MagnitudeInfo);

			Factory.Save();

			var loadedTag = Factory.Load<RuleRunnerTagLink>(tag.PK);

			AssertNoErrors(loadedTag.TGL_TGM_MagnitudeInfo);

			loadedTag.HasChanges = true;

			loadedTag.Validation.ValidateAll();
			AssertHasError(loadedTag.TGL_TGM_MagnitudeInfo, string.Format("Cannot modify [{0}] from here. Tag Group has scope of [{1}].", loadedTag.DisplayText, TagUsageScopeList.Descriptions.User));
		}

		public void TestTagAddSecurityValidation()
		{
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");
				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				var tagLink = Factory.New<TagLink>();
				tagLink.TGL_TGM_Magnitude = magnitude.PK;
				AssertHasError(tagLink.TGL_TGM_MagnitudeInfo, string.Format("You do not have permission to add the tag [{0}].", tagLink.DisplayText));
			}
		}

		public void TestTagAddSecurity_DoesNotFailWhenItWasAlreadyThere()
		{
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				var task = Factory.NewWithValidTestData<ProcessTask>();
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");
				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.AllowPermissionsForCheckpoint(Env.Security.TagAdd);
				var tagLink = (TagLink)task.AddTag(magnitude).Link;

				AssertNoErrors(tagLink.TGL_TGM_MagnitudeInfo);
				Factory.Save();

				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);
				tagLink.Validation.ValidateAll();
				AssertNoErrors(tagLink.TGL_TGM_MagnitudeInfo);
			}
		}

		public void TestTagAddSecurity_DoesNotFailWhenAddedFromWorkflowTemplate()
		{
			BMSTestHelper.EnableBMSInRegistry();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

				var system = BMSTestHelper.CreateSystem(Factory, "ORG");
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");

				var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
				var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
				templateWorkflow1.FH_CompletionStatement = "workflow1";
				var tag1 = BMSTestHelper.CreateTagLink(templateWorkflow1, magnitude);
				var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
				templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;

				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
				var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");

				AssertEquals(1, workflow1.TagLinks.Count);

				var tagLink = workflow1.TagLinks_ForBinding.Single();

				tagLink.Validation.ValidateAll();
				AssertNoErrors(tagLink.TGL_TGM_MagnitudeInfo);
			}
		}

		public void TestTagAddSecurity_DoesNotFailWhenAddedFromWorkflowTemplate_EvenWhenWithRuleScope()
		{
			BMSTestHelper.EnableBMSInRegistry();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

				var system = BMSTestHelper.CreateSystem(Factory, "ORG");
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");

				var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
				var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
				templateWorkflow1.FH_CompletionStatement = "workflow1";
				var tag1 = BMSTestHelper.CreateTagLink(templateWorkflow1, magnitude);
				var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
				templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;

				Factory.Save();

				definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
				var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");

				AssertEquals(1, workflow1.TagLinks.Count);
				AssertEquals(magnitude, workflow1.TagLinks.First().TagMagnitude);
			}
		}

		public void TestTagAddSecurity_FailWhenNotFromWorkflowTemplate()
		{
			BMSTestHelper.EnableBMSInRegistry();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

				var system = BMSTestHelper.CreateSystem(Factory, "ORG");
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");

				var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
				var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
				templateWorkflow1.FH_CompletionStatement = "workflow1";
				var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
				templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;

				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
				var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");

				AssertEquals(0, workflow1.TagLinks.Count);

				var tagLink = BMSTestHelper.CreateTagLink(workflow1, magnitude);

				tagLink.Validation.ValidateAll();
				AssertHasError(tagLink.TGL_TGM_MagnitudeInfo, string.Format("You do not have permission to add the tag [{0}].", tagLink.DisplayText));
			}
		}

		public void TestTagAddSecurity_FailWhenWhenUserWithNoSecurityAddTagLinkAfterLoadedFromWorkflowTemplate()
		{
			BMSTestHelper.EnableBMSInRegistry();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

				var system = BMSTestHelper.CreateSystem(Factory, "ORG");
				var definition = BMSTestHelper.CreateTagDefinition(Factory, "LIB", "Lets have a good time at the library");
				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "FUN", "It will be fun");

				var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
				var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
				templateWorkflow1.FH_CompletionStatement = "workflow1";
				var tag1 = BMSTestHelper.CreateTagLink(templateWorkflow1, magnitude);
				var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
				templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;

				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				var org = Factory.NewWithValidTestData<OrgHeader>();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
				var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");

				AssertEquals(1, workflow1.TagLinks.Count);
				workflow1.TagLinks_ForBinding[0].Delete();

				Factory.Save();

				var newTagLink = BMSTestHelper.CreateTagLink(workflow1, magnitude);

				newTagLink.Validation.ValidateAll();
				AssertHasError(newTagLink.TGL_TGM_MagnitudeInfo, string.Format("You do not have permission to add the tag [{0}].", newTagLink.DisplayText));
			}
		}

		public void TestValidateTGLTGMMagnitude_NullMagnitude()
		{
			var tagDef = BMSTestHelper.CreateTagDefinition(Factory, "BOO", "Weird BOO goes Boom!");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "The End!");
			var link = BMSTestHelper.CreateTagLink(workflow);

			link.TagDefinitionPk = tagDef.PK;
			tagDef.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			AssertNoExceptionThrown(() => link.Validation.ValidateAll());
			AssertHasError(link.TGL_TGM_MagnitudeInfo, string.Format("Cannot modify [{0}] from here. Tag Group has scope of [{1}].", tagDef.DisplayText, tagDef.Lookups.UsageScopeList.GetDescriptionFromCode(tagDef.TGD_UsageScope)));
		}

		[ExpectNoExceptions]
		public void TestNullTag()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TMA");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var link = (TagLink)workflow.AddTag(magnitude).Link;

			Factory.Save();
			link.TGL_TGM_Magnitude = ZGuid.Invalid;

			link.Validation.ValidateAll();
		}

		[ExpectNoExceptions]
		public void TestNullQueue()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "QUA", "origional name");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var link = queue.AddMember(workflow).Link;

			Factory.Save();
			link.TGL_TGM_Magnitude = ZGuid.Invalid;

			link.Validation.ValidateAll();
		}

		public void TestWorkQueueAddSecurityValidation()
		{
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				var magnitude = BMSTestHelper.CreateWorkQueue(Factory, "FUN", "It will be fun");
				Factory.Save();

				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesAddToQueue);

				var tagLink = Factory.New<TagLink>();
				tagLink.TGL_TGM_Magnitude = magnitude.PK;
				AssertHasError(tagLink.TGL_TGM_MagnitudeInfo, string.Format("You do not have permission to add items to the queue [{0}].", tagLink.DisplayText));
			}
		}

		public void TestMagnitude_ShouldAddErrorForInactiveTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "SAM", "Samsung");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "APL", "More like, Same-Sung!");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "WIN", "Apple. More like, Crapple!", isActive: false);

			Factory.Save();

			var link = Factory.New<TagLink>();

			link.TGL_TGM_Magnitude = magnitude2.PK;
			AssertHasError(link.TGL_TGM_MagnitudeInfo, "This Tag is inactive - it may not be used.");

			link.TGL_TGM_Magnitude = magnitude1.PK;
			AssertNoErrors(link);
		}

		public void TestValidateSequence_WhenLessThanZero_ShouldHaveError()
		{
			var link = Factory.New<TagLink>();
			link.TGL_Sequence = -1;

			AssertHasError(link.TGL_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 0.");

			link.TGL_Sequence = 0;
			AssertNoErrors(link.TGL_SequenceInfo);
		}
	}
}
