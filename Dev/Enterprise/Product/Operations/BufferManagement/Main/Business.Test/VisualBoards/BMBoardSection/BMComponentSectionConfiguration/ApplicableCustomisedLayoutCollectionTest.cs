using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ApplicableCustomisedLayoutCollection))]
	class ApplicableCustomisedLayoutCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ApplicableCustomisedLayoutCollection>
	{
		public void TestBuild_TaskSection()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);
			var group = Factory.Load<GlbGroup>(config.ReleaseGroup.PK);
			group.GG_Desc = "Slightly Silly Party";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var taskSummaryCard1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Ze roial pentomime raizes 'er 'arpoon");
			var taskSummaryCard2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "And fires at ze unsuspecting brrrekfast");
			var taskSummaryCard3 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Beng! Right in ze toast!");

			var taskDetailedCard1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, name: "Tarquin Fin-tim-lin-bin-whin-bim-lim-bus-stop-F'tang-F'tang-Olé-Biscuitbarrel");
			var taskDetailedCard2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, name: "Malcolm Peter Brian Telescope Adrian Umbrella Stand Jasper Wednesday");

			var workflowSummaryCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard);
			var workflowDetailedCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard1, "WKI");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard2, "WKP");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section.Board, taskSummaryCard3);
			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, taskDetailedCard1);
			BMSTestHelper.CreateControlCustomisationLink(Factory, config.System, taskDetailedCard2);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowSummaryCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowDetailedCard);

			var collection = new ApplicableCustomisedLayoutCollection(section.SectionConfiguration).Cast<ApplicableCustomisedLayout>().ToArray();

			AssertEquals(4, collection.Length);

			var item1 = collection.Single(i => i.LayoutPK == taskSummaryCard1.PK);
			var item2 = collection.Single(i => i.LayoutPK == taskSummaryCard2.PK);
			var item3 = collection.Single(i => i.LayoutPK == taskSummaryCard3.PK);
			var item4 = collection.Single(i => i.LayoutPK == taskDetailedCard1.PK);

			AssertCustomisedLayoutDetails(item1, CustomisedControlTypeList.Descriptions.TaskCard, "WKI (Work Item)", "Ze roial pentomime raizes 'er 'arpoon", "Section 'bucket' on Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item2, CustomisedControlTypeList.Descriptions.TaskCard, "WKP (Project)", "And fires at ze unsuspecting brrrekfast", "Section 'bucket' on Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item3, CustomisedControlTypeList.Descriptions.TaskCard, "", "Beng! Right in ze toast!", "Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item4, CustomisedControlTypeList.Descriptions.DetailedCard, "", "Tarquin Fin-tim-lin-bin-whin-bim-lim-bus-stop-F'tang-F'tang-Olé-Biscuitbarrel", "Release Group 'Slightly Silly Party'");
		}

		public void TestBuild_WorkflowSection()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var group = Factory.Load<GlbGroup>(config.ReleaseGroup.PK);
			group.GG_Desc = "Slightly Silly Party";

			var system = Factory.NewWithValidTestData<BMSystem>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflowSummaryCard1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard, name: "Ze roial pentomime raizes 'er 'arpoon");
			var workflowSummaryCard2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard, name: "And fires at ze unsuspecting brrrekfast");
			var workflowSummaryCard3 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard, name: "Beng! Right in ze toast!");

			var workflowDetailedCard1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard, name: "Tarquin Fin-tim-lin-bin-whin-bim-lim-bus-stop-F'tang-F'tang-Olé-Biscuitbarrel");
			var workflowDetailedCard2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard, name: "Malcolm Peter Brian Telescope Adrian Umbrella Stand Jasper Wednesday");

			var taskSummaryCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var taskDetailedCard = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowSummaryCard1, "WKI");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, workflowSummaryCard2, "WKP");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section.Board, workflowSummaryCard3);
			BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, workflowDetailedCard1);
			BMSTestHelper.CreateControlCustomisationLink(Factory, config.System, workflowDetailedCard2);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskSummaryCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskDetailedCard);

			var collection = new ApplicableCustomisedLayoutCollection(section.SectionConfiguration).Cast<ApplicableCustomisedLayout>().ToArray();

			AssertEquals(4, collection.Length);

			var item1 = collection.Single(i => i.LayoutPK == workflowSummaryCard1.PK);
			var item2 = collection.Single(i => i.LayoutPK == workflowSummaryCard2.PK);
			var item3 = collection.Single(i => i.LayoutPK == workflowSummaryCard3.PK);
			var item4 = collection.Single(i => i.LayoutPK == workflowDetailedCard1.PK);

			AssertCustomisedLayoutDetails(item1, CustomisedControlTypeList.Descriptions.WorkflowSummaryCard, "WKI (Work Item)", "Ze roial pentomime raizes 'er 'arpoon", "Section 'bucket' on Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item2, CustomisedControlTypeList.Descriptions.WorkflowSummaryCard, "WKP (Project)", "And fires at ze unsuspecting brrrekfast", "Section 'bucket' on Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item3, CustomisedControlTypeList.Descriptions.WorkflowSummaryCard, "", "Beng! Right in ze toast!", "Visual Board 'An Board'");
			AssertCustomisedLayoutDetails(item4, CustomisedControlTypeList.Descriptions.WorkflowDetailedCard, "", "Tarquin Fin-tim-lin-bin-whin-bim-lim-bus-stop-F'tang-F'tang-Olé-Biscuitbarrel", "Release Group 'Slightly Silly Party'");
		}

		public void TestSectionConfiguration_ShouldOrderPreferredVisualLayoutsProperly()
		{
			/* properly being: Section layouts, then Board layouts, then Release Group layouts, then System layouts */
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var group = Factory.Load<GlbGroup>(config.ReleaseGroup.PK);
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var section = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var systemLayout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			systemLayout1.FM_JobType = "";
			systemLayout1.FM_Name = "systemLayout1";
			var systemLayout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			systemLayout2.FM_Name = "systemLayout2";

			var groupLayout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			groupLayout1.FM_JobType = "B";
			groupLayout1.FM_Name = "groupLayout1";
			var groupLayout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			groupLayout2.FM_Name = "groupLayout2";

			var boardLayout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			boardLayout1.FM_JobType = "C";
			boardLayout1.FM_Name = "boardLayout1";
			var boardLayout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			boardLayout2.FM_Name = "boardLayout2";

			var sectionLayout1 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			sectionLayout1.FM_JobType = "D";
			sectionLayout1.FM_Name = "sectionLayout1";
			var sectionLayout2 = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			sectionLayout2.FM_Name = "sectionLayout2";

			var systemLink1 = BMSTestHelper.CreateControlCustomisationLink(Factory, system, systemLayout1, "INC");
			var systemLink2 = BMSTestHelper.CreateControlCustomisationLink(Factory, system, systemLayout2);

			var groupLink1 = BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, groupLayout1, "INC");
			var groupLink2 = BMSTestHelper.CreateControlCustomisationLink(Factory, releaseGroup, groupLayout2);

			var boardLink1 = BMSTestHelper.CreateControlCustomisationLink(Factory, board, boardLayout1, "INC");
			var boardLink2 = BMSTestHelper.CreateControlCustomisationLink(Factory, board, boardLayout2);

			var sectionLink1 = BMSTestHelper.CreateControlCustomisationLink(Factory, section, sectionLayout1, "INC");
			var sectionLink2 = BMSTestHelper.CreateControlCustomisationLink(Factory, section, sectionLayout2);

			Factory.Save();

			var layouts = new ApplicableCustomisedLayoutCollection(section.SectionConfiguration);
			string[] expectedLayoutNames = { sectionLayout1.FM_Name, sectionLayout2.FM_Name };

			AssertContainsExactElementsInAnyOrder("Expect section-level layouts in any order, and yet...", expectedLayoutNames, layouts.Select(l => l.LayoutName));

			sectionLink1.Delete();
			sectionLink2.Delete();
			sectionLayout1.Delete();
			sectionLayout2.Delete();
			Factory.Save();

			layouts = new ApplicableCustomisedLayoutCollection(section.SectionConfiguration);
			string[] expectedLayoutNames2 = { boardLayout1.FM_Name, boardLayout2.FM_Name };

			AssertContainsExactElementsInAnyOrder("Expect board-level layouts in any order, and yet...", expectedLayoutNames2, layouts.Select(l => l.LayoutName));

			boardLink1.Delete();
			boardLink2.Delete();
			boardLayout1.Delete();
			boardLayout2.Delete();
			Factory.Save();

			layouts = new ApplicableCustomisedLayoutCollection(section.SectionConfiguration);
			string[] expectedLayoutNames3 = { groupLayout1.FM_Name, groupLayout2.FM_Name };

			AssertContainsExactElementsInAnyOrder("Expect release group-level layouts in any order, and yet...", expectedLayoutNames3, layouts.Select(l => l.LayoutName));
		}

		#region Implementation

		protected override ApplicableCustomisedLayoutCollection GetCollectionToTest()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			return new ApplicableCustomisedLayoutCollection(section.SectionConfiguration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var layout = BMSTestHelper.CreateControlCustomisation(Factory);
			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, Factory.New<BMBoard>(), layout);

			return new ApplicableCustomisedLayout(link);
		}

		static void AssertCustomisedLayoutDetails(ApplicableCustomisedLayout layoutDetails, string controlType, string jobType, string layoutName, string source)
		{
			CombineAssertions(() =>
			{
				AssertEquals(controlType, layoutDetails.ControlType);
				AssertEquals(jobType, layoutDetails.JobType);
				AssertEquals(layoutName, layoutDetails.LayoutName);
				AssertEquals(source, layoutDetails.Source);
			});
		}

		#endregion
	}
}
