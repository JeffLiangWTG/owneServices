using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystemReleaseGroup))]
	public class BMSystemReleaseGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSerialisingZDateTimes()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;
			releaseGroup.FSG_ResourceCountdownTime = new ZInt(45).GetDateTimeFromMinutes();

			Factory.Save();

			var loadedSystem = new BusinessObjectFactory().Load<BMSystem>(system.PK);
			var loadedReleaseGroup = loadedSystem.ReleaseGroups[0];
			AssertEquals(new ZInt(45).GetDateTimeFromMinutes(), releaseGroup.FSG_ResourceCountdownTime);
		}

		public void TestGroups()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "Group 1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "Group 2";

			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group2.PK;

			AssertEquals(group1, releaseGroup1.Group);
			AssertEquals(group2, releaseGroup2.Group);
		}

		#region Customised Layouts

		public void TestSettingReleaseGroupCardOverrides_ShouldSetHasChanges()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var controlCustomisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			controlCustomisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSystem = newFactory.Load<BMSystem>(system.PK);
			var loadedReleaseGroup = loadedSystem.ReleaseGroups[0];

			AssertEquals(false, loadedReleaseGroup.HasChanges);
			AssertEquals(false, loadedSystem.HasChanges);

			var link = loadedReleaseGroup.CustomisedLayoutLinks.AddNew();
			link.FML_FM_ControlCustomisation = controlCustomisation.PK;

			AssertEquals(true, loadedReleaseGroup.HasChanges);
			AssertEquals(true, loadedSystem.HasChanges);
		}

		public void TestCustomisedCards_TaskCardSection()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			var taskDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var taskSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, taskDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, taskSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNull(loadedTaskDetailedCard);
			AssertNull(loadedTaskSummaryCard);

			loadedSection.Board.MB_GG_ReleaseGroup = group.PK;
			loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedTaskDetailedCard);
			AssertNotNull(loadedTaskSummaryCard);

			AssertEquals(taskDetailedCard.PK, loadedTaskDetailedCard.PK);
			AssertEquals(taskSummaryCard.PK, loadedTaskSummaryCard.PK);
		}

		public void TestCustomisedCards_WorkflowCardSection()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = system.Boards.AddNew().Sections.AddNew();
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);

			var loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNull(loadedWorkflowDetailedCard);
			AssertNull(loadedWorkflowSummaryCard);

			loadedSection.Board.MB_GG_ReleaseGroup = group.PK;
			loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedWorkflowDetailedCard);
			AssertNotNull(loadedWorkflowSummaryCard);

			AssertEquals(workflowDetailedCard.PK, loadedWorkflowDetailedCard.PK);
			AssertEquals(workflowSummaryCard.PK, loadedWorkflowSummaryCard.PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			return system.ReleaseGroups.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
