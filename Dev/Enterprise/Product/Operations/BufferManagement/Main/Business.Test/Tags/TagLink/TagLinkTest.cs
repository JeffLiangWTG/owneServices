using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagLink))]
	class TagLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region Sequence

		public void TestSequence_ForWorkQueueLink_ShouldSetSequenceNumber()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "KEW", "Seven Silver Sausages");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "ZIP");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "BST", "Of all the tags, by far the best.");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var link1 = BMSTestHelper.CreateTagLink(jobHeader1);
			var link2 = BMSTestHelper.CreateTagLink(jobHeader1);
			var link3 = (TagLink)jobHeader2.AddTag(queue).Link;

			AssertEquals((short)0, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);
			AssertEquals((short)1, link3.TGL_Sequence);

			link3.TGL_Sequence = 10;

			link1.TGL_TGM_Magnitude = tag.PK;
			link2.TGL_TGM_Magnitude = tag.PK;

			AssertEquals((short)0, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);

			link1.TGL_TGM_Magnitude = queue.PK;

			AssertEquals("Should set the sequence to make it last in the queue", (short)11, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);

			link1.TGL_TGM_Magnitude = tag.PK;

			AssertEquals((short)0, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);
		}

		#endregion

		#region Work Queue Validation

		public void TestWorkQueueValidation_ForWorkflowOfJobAlreadyInQueue_ShouldAddValidationError()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "QIW", "Listen closely & hear the happiness when you open this can");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "ZIP");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "SCD", "The Second Best Tag");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Store in a cool place");

			var link1 = BMSTestHelper.CreateTagLink(jobHeader);
			var link2 = BMSTestHelper.CreateTagLink(workflow);

			AssertEquals((short)0, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);

			link1.TGL_TGM_Magnitude = queue.PK;
			link2.TGL_TGM_Magnitude = queue.PK;

			AssertEquals((short)1, link1.TGL_Sequence);
			AssertEquals((short)2, link2.TGL_Sequence);

			AssertNoErrors(link1);
			AssertHasError(link2.TGL_TGM_MagnitudeInfo, "Cannot add a workflow of a job already in the queue.");

			link2.TGL_TGM_Magnitude = tag.PK;
			AssertNoErrors(link2);

			AssertEquals((short)1, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);
		}

		public void TestWorkQueueValidation_ForAlreadyInvalidState_ShouldAddValidationError()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "CEW", "Lovingly crafted by Coca-Cola Amatil");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Store in a cool place");

			var link1 = BMSTestHelper.CreateTagLink(jobHeader);
			var link2 = BMSTestHelper.CreateTagLink(workflow);

			AssertEquals((short)0, link1.TGL_Sequence);
			AssertEquals((short)0, link2.TGL_Sequence);

			link1.TGL_TGM_Magnitude = queue.PK;
			link2.TGL_TGM_Magnitude = queue.PK;

			AssertNoErrors(link1);
			AssertHasError(link2.TGL_TGM_MagnitudeInfo, "Cannot add a workflow of a job already in the queue.");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedLink1 = newFactory.Load<TagLink>(link1.PK);
			var loadedLink2 = newFactory.Load<TagLink>(link2.PK);

			loadedLink1.Validation.ValidateAll();
			loadedLink2.Validation.ValidateAll();

			AssertHasError(loadedLink1.TGL_TGM_MagnitudeInfo, "Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:\r\n\tStore in a cool place");
			AssertHasError(loadedLink2.TGL_TGM_MagnitudeInfo, "Cannot add a workflow of a job already in the queue.");
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var link = Factory.New<TagLink>();
			AssertEquals(1.0m, link.TGL_Magnitude);
		}

		#endregion

		#region Security

		public void TestSecurity_MagnitudeReadonly()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DEA");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TMA");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");
			var link = (TagLink)workflow.AddTag(tag1).Link;
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagRemove);
				link.Validation.ValidateTGL_TGM_Magnitude();
				Assert("Tag Link Magnitude should be readonly when the user does not have permission to remove the tag", link.TGL_TGM_MagnitudeInfo.ReadOnly);
			}
		}

		public void TestSecurity_DefinitionReadonly()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DEA");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TMA");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");
			var link = (TagLink)workflow.AddTag(tag1).Link;
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagRemove);
				link.Validation.ValidateTGL_TGM_Magnitude();
				Assert("Tag Link Definition should be readonly when the user does not have permission to remove the tag", link.TagDefinitionPkInfo.ReadOnly);
			}
		}

		public void TestSecurity_ReadOnlyShouldNotBeSetOnValueChange()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DEA");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "TMA");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");
			var link = (TagLink)workflow.AddTag(tag1).Link;
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagRemove);

				Assert("TagRemoveSecurity is TRUE, since there is no TagRemove permission", link.TGL_TGM_MagnitudeInfo.ReadOnly);
				var tmp = link.TGL_TGM_Magnitude;
				link.TGL_TGM_Magnitude = new Guid("{117EF36E-402D-4399-9085-D2782341BE87}");
				Assert("TagRemoveSecurity is FALSE, since we changed the value", !link.TGL_TGM_MagnitudeInfo.ReadOnly);
				link.TGL_TGM_Magnitude = tmp;
				Assert("TagRemoveSecurity is still FALSE, since changing to a non-permitted tag alone is not a problem (unless you try to commit)", !link.TGL_TGM_MagnitudeInfo.ReadOnly);
			}
		}

		#endregion

		#region EffectiveNudge

		public void TestEffectiveNudge()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAV");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 30);
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Alex", nudge: 40);
			var tag4 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Cody", nudge: 50);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Oh HAI! Of all the nudges, by far the biggest.");

			var link1 = (TagLink)workflow.AddTag(tag1).Link;
			var link2 = (TagLink)workflow.AddTag(tag2).Link;
			var link3 = (TagLink)jobHeader.AddTag(tag3).Link;
			var link4 = (TagLink)jobHeader.AddTag(tag4).Link;

			AssertEquals(20m, link1.EffectiveNudge);
			AssertEquals(30m, link2.EffectiveNudge);
			AssertEquals(40m, link3.EffectiveNudge);

			link1.TGL_Magnitude = 0.0m;
			link2.TGL_Magnitude = 0.1m;
			link3.TGL_Magnitude = -1.0m;
			link4.TGL_Magnitude = -0.1m;

			AssertEquals(0m, link1.EffectiveNudge);
			AssertEquals(3m, link2.EffectiveNudge);
			AssertEquals(-40m, link3.EffectiveNudge);
			AssertEquals(-5m, link4.EffectiveNudge);
		}

		public void TestEffectiveNudge_HandlesStupidlyBigAndStupidlySmallNumbers()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAV");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Danielle", nudge: 20);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var link = (TagLink)jobHeader.AddTag(tag).Link;

			link.TGL_Magnitude = decimal.MaxValue;
			AssertEquals(decimal.MaxValue, link.EffectiveNudge);

			link.TGL_Magnitude = decimal.MinValue;
			AssertEquals(decimal.MinValue, link.EffectiveNudge);
		}

		#endregion

		#region Delete

		public void TestDeleteLink_ForItemInWorkQueue_ShouldLeaveGapInSequence()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var link1 = queue.AddMember(workflow5).Link;
			var link2 = queue.AddMember(workflow4).Link;
			var link3 = queue.AddMember(workflow3).Link;
			var link4 = queue.AddMember(workflow2).Link;
			var link5 = queue.AddMember(workflow1).Link;

			Factory.Save();

			var loadedQueue = Factory.CreateNewFactory().Load<WorkQueue>(queue.PK);
			BMSTestCaseWithFactory.AssertSamePK(workflow5, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(workflow4, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(workflow3, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(workflow2, loadedQueue.MembersInSequence.ElementAt(3));
			BMSTestCaseWithFactory.AssertSamePK(workflow1, loadedQueue.MembersInSequence.ElementAt(4));

			AssertEquals((short)1, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow5.PK).TGL_Sequence);
			AssertEquals((short)2, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow4.PK).TGL_Sequence);
			AssertEquals((short)3, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow3.PK).TGL_Sequence);
			AssertEquals((short)4, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow2.PK).TGL_Sequence);
			AssertEquals((short)5, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow1.PK).TGL_Sequence);

			var loadedLink = loadedQueue.Factory.Load<TagLink>(link3.PK);
			loadedLink.Delete();
			loadedQueue.Factory.Save();

			loadedQueue = Factory.CreateNewFactory().Load<WorkQueue>(queue.PK);
			BMSTestCaseWithFactory.AssertSamePK(workflow5, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(workflow4, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(workflow2, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(workflow1, loadedQueue.MembersInSequence.ElementAt(3));

			AssertEquals((short)1, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow5.PK).TGL_Sequence);
			AssertEquals((short)2, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow4.PK).TGL_Sequence);
			AssertEquals((short)4, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow2.PK).TGL_Sequence);
			AssertEquals((short)5, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow1.PK).TGL_Sequence);
		}

		public void TestCannotDeleteRuleUsageScope()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "XML", usageScope: TagUsageScopeList.Codes.Rule);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "It's like bees in my eyes.");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			AssertExceptionThrown<InvalidOperationException>(() => Factory.Save());
			AssertExceptionThrown<CannotDeleteException>(() => tag.Delete());

			definition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			AssertNoExceptionThrown(() => tag.Delete());
		}

		public void TestDeleteWorksMoreThanOnce()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BEE");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "Bees are pretty cool.");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Test completion statement.");
			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			AssertNoExceptionThrown(() => tag.Delete());
			AssertNoExceptionThrown(() => tag.Delete());
		}

		#endregion

		#region RUL UsageScope

		public void TestRULUsageScope_CannotBeAddedManually_ToNormalWorkflow()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BEE");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "Father I cræv höné");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			Factory.Save();

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Make Höné");
			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			AssertExceptionThrown<InvalidOperationException>("We shouldn't be able to manually add RUL-scope taglinks", () => Factory.Save());
		}

		public void TestRULUsageScope_CanBeAddedManually_ToWorkflowTemplate()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BEE");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "Father I cræv höné");
			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			Factory.Save();

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();

			var tag = (TagLink)templateWorkflow1.AddTag(magnitude).Link;

			AssertEquals("We should be able to add taglinks to RUL-scope tag definitions through ProcessTaskTemplates", 1, templateWorkflow1.TagLinks.Count);
		}

		public void TestImmutableUsageScope_ReadOnly_EditableWhenHasChanges()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "XML");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "It's like bees in my eyes.");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			Factory.Save();

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			AssertEquals(true, tag.ReadOnly);

			tag.HasChanges = true;

			AssertEquals(false, tag.ReadOnly);
		}

		public void TestReadOnly_NotReadOnlyWhenScopeInvalid()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "ELM", "Elmo");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "LEM", "Lemonade");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");

			var tag = (TagLink)workflow.AddTag(magnitude).Link;

			AssertEquals(false, tag.ReadOnly);

			Factory.Save();

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			AssertEquals(true, tag.ReadOnly);

			Factory.Save();

			definition.TGD_Scope = TagScopeList.Codes.Task;
			AssertEquals(false, tag.ReadOnly);
		}

		public void TestRULScope_CannotBeAddedToProcessTaskTemplate()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BEE");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BEE", "Father I cræv höné");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "workflow1";
			var tag1 = BMSTestHelper.CreateTagLink(templateWorkflow1, magnitude);
			var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;

			Factory.Save();

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
			var workflowWithTag = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");

			AssertEquals("We should be able to add taglinks to RUL-scope tag definitions through ProcessTaskTemplates", 1, workflowWithTag.TagLinks.Count);

			var tagLink = workflowWithTag.TagLinks_ForBinding.Single();

			AssertEquals("We added the right tag", magnitude, tagLink.TagMagnitude);
		}

		public void TestUsageScopeInteractive()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "MOO";
			definition.TGD_Description = "Mooooooooooooooooooooooo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BUL", "Cow boy");

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var link = Factory.New<TagLink>();
			link.TGL_TGM_Magnitude = magnitude.PK;
			link.TGL_ParentId = workflow.PK;
			link.TGL_ParentTableCode = workflow.TablePrefix;

			AssertEquals(true, link.InOperationalScope);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			AssertEquals(false, link.InOperationalScope);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.User;
			AssertEquals(true, link.InOperationalScope);

			ErrorReporter.Clear();
		}

		public void TestUsageScopeNonInteractive()
		{
			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var definition = Factory.NewWithValidTestData<TagDefinition>();
				definition.TGD_Code = "MOO";
				definition.TGD_Description = "Mooooooooooooooooooooooo";

				var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "BUL", "Cow boy");

				var workflow = Factory.NewWithValidTestData<ProcessHeader>();
				var link = Factory.New<TagLink>();
				link.TGL_TGM_Magnitude = magnitude.PK;
				link.TGL_ParentId = workflow.PK;
				link.TGL_ParentTableCode = workflow.TablePrefix;

				AssertEquals(true, link.InOperationalScope);

				definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
				AssertEquals(true, link.InOperationalScope);

				definition.TGD_UsageScope = TagUsageScopeList.Codes.User;
				AssertEquals(false, link.InOperationalScope);

				ErrorReporter.Clear();
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		#endregion

		#region Logging

		[TestDate(2014, 4, 23)]
		public void TestTagLinkCreateAndDeleteAddsLogs()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Workflow 1";

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "DAN";
			definition.TGD_Description = "Yolo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TEL", "The best magnitude");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Frodo Baggins";
			staff.GS_Code = "FRO";
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var link = BMSTestHelper.CreateTagLink(workflow);
			link.TGL_TGM_Magnitude = magnitude.PK;
			Factory.Save();

			workflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
			var log1 = BMSTestCaseWithFactory.AssertTagEventRaised(workflow, TagActionType.AddTag, magnitude);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			link.TGL_Description = "Dat link";
			Factory.Save();

			workflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);

			var log2 = BMSTestCaseWithFactory.AssertTagEventRaised(workflow, TagActionType.ModifyTag, magnitude);
			AssertNotEquals(log1, log2);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			workflow.TagLinks_ForBinding.DeleteAll();
			workflow.Factory.Save();

			workflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);

			var log3 = BMSTestCaseWithFactory.AssertTagEventRaised(workflow, TagActionType.RemoveTag, magnitude);
			AssertNotEquals(log2, log3);
		}

		[TestDate(2014, 4, 23)]
		public void TestAddTag_ForWorkQueue_ShouldLogOnce()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "This is my hair");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I don't wear wigs");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "ALA", "SKA");

			Factory.Save();

			var link = workflow.AddTag(queue);
			Factory.Save();

			var logs = workflow.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode));

			BMSTestCaseWithFactory.AssertTagEventRaised(workflow, TagActionType.AddTag, queue);
			AssertEquals(1, logs.Length);
		}

		[TestDate(2014, 4, 23)]
		public void TestAddTagAsThoughInGrid_ForWorkQueue_ShouldLogOnce()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "This is my hair");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I don't wear wigs");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "ALA", "SKA");

			Factory.Save();

			var link = workflow.TagLinks_ForBinding.AddNew();
			link.TagDefinitionPk = queue.TGM_TGD_Tag;
			link.TGL_TGM_Magnitude = queue.PK;

			Factory.Save();

			var logs = workflow.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TagWasAddedOrRemovedCode));

			BMSTestCaseWithFactory.AssertTagEventRaised(workflow, TagActionType.AddTag, queue);
			AssertEquals(1, logs.Length);
		}

		[TestDate(2014, 4, 23)]
		public void TestLogOnEmptyMagnitude()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			header.FH_CompletionStatement = "Workflow 1";

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "FAN";
			definition.TGD_Description = "Nolo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TEL", "A magnitude to restrict magnets");

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TAN", "One tanned to a crisp individual");
			Factory.Save();

			var startingLogsCount = header.Logs.GetAllLogs().Count;

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var link = BMSTestHelper.CreateTagLink(header);
			link.TGL_TGM_Magnitude = magnitude.PK;
			Factory.Save();

			header = Factory.CreateNewFactory().Load<ProcessHeader>(header.PK);

			AssertEquals("Log was added on add", startingLogsCount + 1, header.Logs.GetAllLogs().Count);

			link.TGL_TGM_Magnitude = ZGuid.Empty;

			AssertNoExceptionThrown(() => link.Delete());

			header = Factory.CreateNewFactory().Load<ProcessHeader>(header.PK);

			AssertEquals("No log was added when there was no magnitude", startingLogsCount + 1, header.Logs.GetAllLogs().Count);
		}

		public void TestTagMultipleItemsAtOnce_ShouldAddLogForAllItems()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);

			var def = BMSTestHelper.CreateTagDefinition(Factory, "ABC");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "XYZ");

			Factory.Save();

			jobHeader1.AddTag(mag);
			jobHeader2.AddTag(mag);

			Factory.Save();

			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have the Add Tag event. SAD!", jobHeader1.Parent, Events.TagWasAddedOrRemoved, jobHeader1);
			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have the Add Tag event. SAD!", jobHeader2.Parent, Events.TagWasAddedOrRemoved, jobHeader2);
		}

		#endregion

		#region DisplayText

		public void TestDisplayText()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "DAN";
			definition.TGD_Description = "Yolo";

			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TEL", "The best magnitude");

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var link = BMSTestHelper.CreateTagLink(workflow, magnitude);

			AssertEquals("TEL - The best magnitude", link.DisplayText);
			AssertEquals(magnitude.DisplayText, link.DisplayText);
		}

		#endregion

		#region HeaderDelete

		public void TestTagLinkIsDeletedUponParentDeletion()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "RUL", "The best magnitude");
			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			var tagLink = BMSTestHelper.CreateTagLink(header, magnitude);
			Factory.Save();
			tagLink.Definition.TGD_UsageScope = "RUL";

			header.Delete();
			AssertEquals("Precondition", true, header.IsDeleted);
			AssertEquals("Expected no restriction on tagLink being deleted as the parent header is deleted", true, tagLink.IsDeleted);
		}

		#endregion

		#region Ignore Concurrency Check

		public void TestTGL_Magnitude_IgnoreConcurrencyCheck()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "DEF");
			var tagMagnitude = BMSTestHelper.CreateTagMagnitude(tagDefinition, "MAG");
			var link = BMSTestHelper.CreateTagLink(workflow, tagMagnitude);

			AssertEquals(1, workflow.TagLinks.Count);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var reloadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			reloadedWorkflow.TagLinks.Single().TGL_Magnitude = 4m;
			link.TGL_Magnitude = 5m;

			newFactory.Save();
			Factory.Save();

			AssertEquals(4m, reloadedWorkflow.TagLinks.Single().TGL_Magnitude);
			AssertEquals(5m, link.TGL_Magnitude);
		}

		#endregion

		#region Workflow Edit time

		public void TestTagLinkedToProcessTask_WhenUpdatingParentTableCode_UpdatesEditTime()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "R08");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "R08", "A very simple tag");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			var tagLink = BMSTestHelper.CreateTagLink(task, tag);
			var header = task.GetProcessHeader();
			var lastEditTime = header.FH_SystemLastEditTimeUtc;
			Thread.Sleep(TimeSpan.FromMilliseconds(100));

			tagLink.TGL_ParentTableCode = "P9";

			AssertGreaterThan(header.FH_SystemLastEditTimeUtc, lastEditTime);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var task = factory.NewWithValidTestData<ProcessTask>();

			var definition = factory.NewWithValidTestData<TagDefinition>();
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = "WOW";

			return (TagLink)task.AddTag(magnitude).Link;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion
	}
}
