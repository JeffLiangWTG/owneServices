using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	public class TagProviderTest : BMSTestCaseWithFactory
	{
		#region Applicable Tags

		public void TestApplicableTags_ForNullMagnitudes()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var link = BMSTestHelper.CreateTagLink(jobHeader);

			AssertEquals(0, jobHeader.GetApplicableTags().Count());

			link.TGL_TGM_Magnitude = config.RedTag.PK;
			AssertEquals(config.RedTag, jobHeader.GetApplicableTags().Single());
		}

		public void TestApplicableTags()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag1_1 = BMSTestHelper.CreateTagMagnitude(definition1, "AAB");
			var mag1_2 = BMSTestHelper.CreateTagMagnitude(definition1, "AAC");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BBA");
			var mag2_1 = BMSTestHelper.CreateTagMagnitude(definition2, "BBB");
			var mag2_2 = BMSTestHelper.CreateTagMagnitude(definition2, "BBC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow);

			jobHeader.AddTag(mag1_1);
			jobHeader.AddTag(mag2_1);
			workflow.AddTag(mag1_2);
			workflow.AddTag(mag2_1);
			task.AddTag(mag2_2);

			Factory.Save();

			AssertTagsApplied("Should not inherit tags from anything.", jobHeader, mag1_1, mag2_1);
			AssertTagsApplied("Inherits magnitude1_1 from JobHeader. Mag2_1 should not be duplicated.", workflow, mag1_1, mag1_2, mag2_1);
			AssertTagsApplied("Inherit everything.", task, mag1_1, mag1_2, mag2_1, mag2_2);
		}

		public void TestApplicableTags_IsExclusive()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA", isExclusive: true);
			var mag1_1 = BMSTestHelper.CreateTagMagnitude(definition1, "AAB");
			var mag1_2 = BMSTestHelper.CreateTagMagnitude(definition1, "AAC");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BBA", isExclusive: true);
			var mag2_1 = BMSTestHelper.CreateTagMagnitude(definition2, "BBB");
			var mag2_2 = BMSTestHelper.CreateTagMagnitude(definition2, "BBC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow);

			jobHeader.AddTag(mag1_1);
			jobHeader.AddTag(mag2_1);
			workflow.AddTag(mag1_2);
			workflow.AddTag(mag2_1);
			task.AddTag(mag2_2);

			Factory.Save();

			AssertTagsApplied("Should not inherit tags from anything.", jobHeader, mag1_1, mag2_1);
			AssertTagsApplied("Tag mag1_2 should override mag1_1.", workflow, mag1_2, mag2_1);
			AssertTagsApplied("Tag mag1_2 should override mag1_1 and mag2_2 should override 2_1.", task, mag1_2, mag2_2);
		}

		public void TestApplicableTagLinks()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tag1 = BMSTestHelper.CreateTagMagnitude(definition1, "AAB");
			var tag2 = BMSTestHelper.CreateTagMagnitude(definition1, "AAC");

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var link1 = jobHeader.AddTag(tag1).Link;
			var link2 = workflow.AddTag(tag2).Link;

			AssertContainsExactElementsInAnyOrder(new[] { link1 }, jobHeader.GetApplicableTagLinks());
			AssertContainsExactElementsInAnyOrder(new[] { link1, link2 }, workflow.GetApplicableTagLinks());
		}

		public void TestApplicableTagLinks_ForExclusiveTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var link1 = jobHeader.AddTag(config.PlatinumTag).Link;
			var link2 = workflow.AddTag(config.RedTag).Link;

			AssertContainsExactElementsInAnyOrder(new[] { link1 }, jobHeader.GetApplicableTagLinks());
			AssertContainsExactElementsInAnyOrder("Workflow should override rather than inherit jobHeader's exclusive tag", new[] { link2 }, workflow.GetApplicableTagLinks());
		}

		#endregion

		#region Definition Cache

		public void TestLoadDefinitionsWithoutMagnitudes()
		{
			var definition1 = Factory.New<TagDefinition>();
			definition1.TGD_Code = "GR1";

			Factory.Save();

			AssertCollectionContains(TagProvider.GetAllTagDefinitions(Factory).AllDefinitionsRelevantToCurrentWorkflowManagementMode, d => d.PK == definition1.PK);
		}

		public void TestLoads()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "GR1");
			BMSTestHelper.CreateTagMagnitude(definition1, "M1");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "GR2");
			BMSTestHelper.CreateTagMagnitude(definition2, "M2");

			var definition3 = BMSTestHelper.CreateTagDefinition(Factory, "GR3");
			BMSTestHelper.CreateTagMagnitude(definition3, "M3");

			Factory.Save();

			AssertCollectionContains(TagProvider.GetAllTagDefinitions(Factory).AllDefinitionsRelevantToCurrentWorkflowManagementMode, d => d.PK == definition1.PK);
			AssertCollectionContains(TagProvider.GetAllTagDefinitions(Factory).AllDefinitionsRelevantToCurrentWorkflowManagementMode, d => d.PK == definition2.PK);
			AssertCollectionContains(TagProvider.GetAllTagDefinitions(Factory).AllDefinitionsRelevantToCurrentWorkflowManagementMode, d => d.PK == definition3.PK);
		}

		#endregion

		#region Visual Styles

		public void TestTagOrder()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag1_1 = BMSTestHelper.CreateTagMagnitude(definition1, "AAB", color: Color.Blue, visualPriority: 0);
			var mag1_2 = BMSTestHelper.CreateTagMagnitude(definition1, "AAC", color: Color.Red, visualPriority: 0);

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BBA");
			var mag2_1 = BMSTestHelper.CreateTagMagnitude(definition2, "BBB", color: Color.Orange, visualPriority: 1);
			var mag2_2 = BMSTestHelper.CreateTagMagnitude(definition2, "BBC", color: Color.Green, visualPriority: 200);

			var magnitudePks = new HashSet<ZGuid>(new[] { mag1_1, mag1_2, mag2_1, mag2_2 }.Select(m => m.PK)).ToImmutableHashSet();

			AssertEquals(true, TagProvider.GetOrderedColors(TagProvider.GetAllTagDefinitions(Factory), magnitudePks).SequenceEqual(new[] { Color.Green, Color.Orange, Color.Blue, Color.Red }));
		}

		public void TestTagOrder_ApplyToBorder_EnsureOrderedColorsAreConsistent()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "CAA");
			var mag1_1 = BMSTestHelper.CreateTagMagnitude(definition1, "AAB", description: "Black", color: Color.Black, visualPriority: 1, borderStyle: VisualBoardButtonBorderStyle.Solid);
			mag1_1.ApplyColorToBorder = true;

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BBB");
			var mag1_2 = BMSTestHelper.CreateTagMagnitude(definition2, "BBC", description: "Red", color: Color.Red, visualPriority: 1, borderStyle: VisualBoardButtonBorderStyle.Solid);
			mag1_2.ApplyColorToBorder = true;

			Factory.Save();

			var definitions = TagProvider.GetAllTagDefinitions(Factory);
			var applicableMagnitudes = new HashSet<ZGuid>(definitions.AllMagnitudes.Select(m => m.PK)).ToImmutableHashSet();
			var magnitudePks = new HashSet<ZGuid>(new[] { mag1_1, mag1_2 }.Select(m => m.PK)).ToImmutableHashSet();

			CombineAssertions("The first tag colour in sequence should match that of the border colour, irrespective of the tag definition codes", () =>
			{
				AssertEquals("Ordered colours", true, TagProvider.GetOrderedColors(definitions, magnitudePks).SequenceEqual(new[] { Color.Black, Color.Red }));
				AssertEquals("Border colour", Color.Black, TagProvider.GetBorderColor(definitions, applicableMagnitudes));
			});
		}

		public void TestBorderStyle_Sorting()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TUE", "Tuesday", borderStyle: VisualBoardButtonBorderStyle.Outset);
			magnitude2.VisualStylePriority = 1;
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", borderStyle: VisualBoardButtonBorderStyle.Inset);
			magnitude1.VisualStylePriority = 1;
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "WED", "Wednesday", borderStyle: VisualBoardButtonBorderStyle.Solid);
			magnitude3.VisualStylePriority = 1;

			Factory.Save();

			var definitions = TagProvider.GetAllTagDefinitions(Factory);
			var applicableMagnitudes = new HashSet<ZGuid>(definitions.AllMagnitudes.Select(m => m.PK)).ToImmutableHashSet();

			AssertEquals("Alphabetical", new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Inset, false), TagProvider.GetBorderStyle(definitions, applicableMagnitudes));

			magnitude1.BorderStyle = string.Empty;

			AssertEquals("Alphabetical", new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, false), TagProvider.GetBorderStyle(definitions, applicableMagnitudes));

			magnitude3.VisualStylePriority = 2;

			AssertEquals("Priority trumps alphabetical", new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, false), TagProvider.GetBorderStyle(definitions, applicableMagnitudes));

			magnitude2.BorderStyle = string.Empty;
			magnitude3.BorderStyle = string.Empty;

			AssertNull(TagProvider.GetBorderStyle(definitions, applicableMagnitudes));
		}

		public void TestBackgroundColor_Sorting()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue);
			magnitude1.ApplyColorToBackground = true;
			magnitude1.VisualStylePriority = 1;
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TUE", "Tuesday", color: Color.Wheat);
			magnitude2.ApplyColorToBackground = true;
			magnitude2.VisualStylePriority = 1;
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "WED", "Wednesday", color: Color.Red);
			magnitude3.ApplyColorToBackground = true;
			magnitude3.VisualStylePriority = 1;

			Factory.Save();

			var definitions = TagProvider.GetAllTagDefinitions(Factory);
			var applicableMagnitudes = new HashSet<ZGuid>(definitions.AllMagnitudes.Select(m => m.PK)).ToImmutableHashSet();

			AssertEquals("Alphabetical", Color.Blue, TagProvider.GetBackgroundColor(definitions, applicableMagnitudes));

			magnitude1.ApplyColorToBackground = false;

			AssertEquals("Alphabetical", Color.Wheat, TagProvider.GetBackgroundColor(definitions, applicableMagnitudes));

			magnitude3.VisualStylePriority = 2;

			AssertEquals("Priority trumps alphabetical", Color.Red, TagProvider.GetBackgroundColor(definitions, applicableMagnitudes));

			magnitude2.ApplyColorToBackground = false;
			magnitude3.ApplyColorToBackground = false;

			AssertNull(TagProvider.GetBackgroundColor(definitions, applicableMagnitudes));
		}

		public void TestBorderColor_Sorting()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "MON", "Monday", color: Color.Blue);
			magnitude1.ApplyColorToBorder = true;
			magnitude1.VisualStylePriority = 1;
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TUE", "Tuesday", color: Color.Wheat);
			magnitude2.ApplyColorToBorder = true;
			magnitude2.VisualStylePriority = 1;
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "WED", "Wednesday", color: Color.Red);
			magnitude3.ApplyColorToBorder = true;
			magnitude3.VisualStylePriority = 1;

			Factory.Save();

			var definitions = TagProvider.GetAllTagDefinitions(Factory);
			var applicableMagnitudes = new HashSet<ZGuid>(definitions.AllMagnitudes.Select(m => m.PK)).ToImmutableHashSet();

			AssertEquals("Alphabetical", Color.Blue, TagProvider.GetBorderColor(definitions, applicableMagnitudes));

			magnitude1.ApplyColorToBorder = false;

			AssertEquals("Alphabetical", Color.Wheat, TagProvider.GetBorderColor(definitions, applicableMagnitudes));

			magnitude3.VisualStylePriority = 2;

			AssertEquals("Priority trumps alphabetical", Color.Red, TagProvider.GetBorderColor(definitions, applicableMagnitudes));

			magnitude2.ApplyColorToBorder = false;
			magnitude3.ApplyColorToBorder = false;

			AssertNull(TagProvider.GetBorderColor(definitions, applicableMagnitudes));
		}

		#endregion

		#region System-defined Tags

		public void TestCCPMTag_ShouldAlwaysBeCCPMTag()
		{
			var tag = Factory.Load<TagMagnitude>(TagProvider.GetCCPMReadyToReleaseTagMagnitudePK());

			AssertEquals(BMConstants.ReadyToReleaseTagCode, tag.TGM_Code);
			AssertEquals("This should be the CCPM version of the RTR tag.", BMConstants.CCPMReleaseRulesTagGroupCode, tag.Definition.TGD_Code);
		}

		public void TestSystemDefinedTags()
		{
			AssertSystemDefinedTagLoaded(TagProvider.GetCCPMReleaseTagGroup(Factory));
			AssertSystemDefinedTagLoaded(TagProvider.GetWorkQueuesTagGroup(Factory));
		}

		static void AssertSystemDefinedTagLoaded(TagDefinition tagGroup)
		{
			AssertNotNull(tagGroup);
			AssertEquals(true, tagGroup.TGD_IsSystem);
		}

		#endregion

		#region Performance

		public void TestGetApplicableTagLinks_ShouldNotCreateActiveBusinessObjectCollections()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");

			jobHeader1.AddTag(config.DerpyHoovesTag);
			jobHeader2.AddTag(config.PrincessCelestiaTag);

			workflow1.AddTag(config.PrincessLunaTag);
			workflow2.AddTag(config.RainbowDashTag);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

			AssertEquals(0, ActiveBusinessObjectCollectionIndexFinderForTest.GetNumberOfRetainedIndexes(TagLinkSchema.Constants.TableName, newFactory));

			loadedWorkflow1.GetApplicableTagLinks();
			loadedWorkflow2.GetApplicableTagLinks();

			AssertEquals(0, ActiveBusinessObjectCollectionIndexFinderForTest.GetNumberOfRetainedIndexes(TagLinkSchema.Constants.TableName, newFactory));
		}

		#endregion
	}

	class TagProvider_ChildWorkflowTest : BMSTestCaseWithFactory
	{
		#region Child Workflows

		public void TestChildWorkflowTags_NonExclusive_ShouldInheritFromParentWorkflows()
		{
			jobHeader.AddTag(config.DerpyHoovesTag);
			grandparentWorkflow.AddTag(config.PrincessCelestiaTag);
			parentWorkflow.AddTag(config.RainbowDashTag);
			childWorkflow.AddTag(config.PrincessLunaTag);

			AssertTagApplied(jobHeader, config.DerpyHoovesTag);
			AssertTagsApplied(grandparentWorkflow, config.DerpyHoovesTag, config.PrincessCelestiaTag);
			AssertTagsApplied(parentWorkflow, config.DerpyHoovesTag, config.RainbowDashTag);
			AssertTagsApplied(childWorkflow, config.DerpyHoovesTag, config.PrincessLunaTag);
		}

		public void TestChildWorkflowTags_NonExclusive_ShouldOnlyInheritFromJobHeader()
		{
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");

			BMSTestHelper.MakeChildOf(grandparentWorkflow, workflow2);

			workflow2.AddTag(config.DerpyHoovesTag);

			grandparentWorkflow.AddTag(config.PrincessCelestiaTag);
			parentWorkflow.AddTag(config.RainbowDashTag);
			childWorkflow.AddTag(config.PrincessLunaTag);

			AssertTagsApplied(workflow2, config.DerpyHoovesTag);
			AssertTagsApplied(grandparentWorkflow, config.PrincessCelestiaTag);
			AssertTagsApplied(parentWorkflow, config.RainbowDashTag);
			AssertTagsApplied(childWorkflow, config.PrincessLunaTag);
		}

		public void TestChildWorkflowTags_Exclusive_ShouldNotInheritFromParentWorkflowsWhenAppliedAtChild()
		{
			jobHeader.AddTag(config.GreenTag);
			grandparentWorkflow.AddTag(config.PlatinumTag);
			parentWorkflow.AddTag(config.RedTag);
			childWorkflow.AddTag(config.GoldTag);

			AssertTagApplied(jobHeader, config.GreenTag);
			AssertTagsApplied(grandparentWorkflow, config.PlatinumTag);
			AssertTagsApplied(parentWorkflow, config.RedTag);
			AssertTagsApplied(childWorkflow, config.GoldTag);
		}

		public void TestChildWorkflowTags_Exclusive_ShouldInheritFromParentWorkflowsWhenNotAppliedAtChild()
		{
			jobHeader.AddTag(config.GreenTag);
			parentWorkflow.AddTag(config.RedTag);

			AssertTagApplied(jobHeader, config.GreenTag);
			AssertTagsApplied(grandparentWorkflow, config.GreenTag);
			AssertTagsApplied(parentWorkflow, config.RedTag);
			AssertTagsApplied(childWorkflow, config.GreenTag);
		}

		#endregion

		#region Db Hits

		public void TestDbHits_GetApplicableTags_ForChildWorkflows()
		{
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedChildWorkflow = newFactory.Load<ProcessHeader>(childWorkflow.PK);

			loadedChildWorkflow.GetApplicableTags();

			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparent");
			parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parent");
			childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "child");

			BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);
			BMSTestHelper.MakeChildOf(parentWorkflow, grandparentWorkflow);
		}

		TagsTestConfig config;
		ProcessJobHeader jobHeader;
		ProcessHeader grandparentWorkflow, parentWorkflow, childWorkflow;

		#endregion
	}
}
