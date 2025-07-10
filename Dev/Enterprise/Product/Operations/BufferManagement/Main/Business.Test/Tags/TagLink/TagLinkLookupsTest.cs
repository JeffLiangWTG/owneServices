using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class TagLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Magnitudes

		public void TestMagnitudes_ShouldFilterByUsageScope()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "OMG", "Oh My Goodness", usageScope: TagUsageScopeList.Codes.Rule);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "HAL", "Halo 2 Beta");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "STI", "Sting");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition2, "MAD", "Mad about you");

			Factory.Save();

			var link = Factory.New<TagLink>();

			AssertCollectionContains(magnitude2, link.Lookups.Magnitudes);
			AssertCollectionNotContains(magnitude1, link.Lookups.Magnitudes);

			link.TGL_TGM_Magnitude = magnitude1.PK;
			AssertCollectionContains(magnitude1, link.Lookups.Magnitudes);
		}

		public void TestMagnitudes_ForTemplates_ShouldFilterByUsageScope()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "OMG", "Oh My Goodness", usageScope: TagUsageScopeList.Codes.User);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "HAL", "Halo 2 Beta");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "STI", "Sting");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition2, "MAD", "Mad about you");

			Factory.Save();

			var link = Factory.New<TagLinkTemplate>();

			AssertCollectionContains(magnitude2, link.Lookups.Magnitudes);
			AssertCollectionNotContains(magnitude1, link.Lookups.Magnitudes);

			link.TGL_TGM_Magnitude = magnitude1.PK;
			AssertCollectionContains(magnitude1, link.Lookups.Magnitudes);
		}

		#endregion

		#region Delete

		public void TestTagLinkBusinessObjecteIsDeleted()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "OMG", "Oh My Goodness", usageScope: TagUsageScopeList.Codes.Rule);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "HAL", "Halo 2 Beta");
			Factory.Save();

			var link = Factory.New<TagLink>();
			link.Delete();
			AssertNoExceptionThrown(() => { var defination1 = link.Lookups.Definitions; });
			AssertNoExceptionThrown(() => { var magnitude = link.Lookups.Magnitudes; });
		}

		#endregion

		#region Definitions

		public void TestDefinitions_ShouldRestrictByScope()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "RJC", "Rejected Cartoons");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "SPO", "My spoon is too big");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "IAM", "I am a banana");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition2, "FUN", "Funny Hats Only");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I am the queen of France");
			var task = BMSTestHelper.CreateTask(workflow, description: "Do you want to see a movie?");

			Factory.Save();

			var workflowTag1 = (TagLink)workflow.AddTag(magnitude1).Link;
			var workflowTag2 = (TagLink)workflow.AddTag(magnitude2).Link;
			var taskTag1 = (TagLink)task.AddTag(magnitude1).Link;
			var taskTag2 = (TagLink)task.AddTag(magnitude2).Link;

			AssertCollectionContains(magnitude1, workflowTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, workflowTag1.Lookups.Definitions);
			AssertCollectionContains(magnitude1, taskTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, taskTag1.Lookups.Definitions);

			AssertCollectionContains(definition1, workflowTag2.Lookups.Definitions);
			AssertCollectionContains(definition1, taskTag2.Lookups.Definitions);

			definition1.TGD_Scope = TagScopeList.Codes.Task;
			Factory.Save();

			AssertCollectionContains(magnitude1, workflowTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, workflowTag1.Lookups.Definitions);
			AssertCollectionContains(magnitude1, taskTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, taskTag1.Lookups.Definitions);

			AssertCollectionNotContains(definition1, workflowTag2.Lookups.Definitions);
			AssertCollectionContains(definition1, taskTag2.Lookups.Definitions);

			definition1.TGD_Scope = TagScopeList.Codes.Workflow;
			Factory.Save();

			AssertCollectionContains(magnitude1, workflowTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, workflowTag1.Lookups.Definitions);
			AssertCollectionContains(magnitude1, taskTag1.Lookups.Magnitudes);
			AssertCollectionContains(definition1, taskTag1.Lookups.Definitions);

			AssertCollectionContains(definition1, workflowTag2.Lookups.Definitions);
			AssertCollectionNotContains(definition1, taskTag2.Lookups.Definitions);
		}

		#endregion
	}
}
