using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Pipes.Test;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.BufferManagement.Business.ApprovedShapeBufferPenetrationService;
using PropertyCache = Enterprise.VisualBoards.Business.PropertyCache;

namespace Enterprise.BufferManagement.Business.Test
{
	public class BMBoardSectionViewModelTest : BMSTestCaseWithFactory
	{
		#region Construction

		public void TestConstructViewModel_ShouldNotHoldWorkflowsAndTasks()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			var detailedCard = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var summaryCard = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, detailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, summaryCard);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_Code = "AAA";
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "My Workflow", bucket);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = ConstructViewModelInAnotherFactory(section.MS_MB_Board);

			GC.Collect(); // This is a unit test.
			GC.WaitForPendingFinalizers(); // This is a unit test.

			var activeFactories = BMSTestHelper.GetActiveFactoryNames();
			var message = string.Join(System.Environment.NewLine, activeFactories);

			CombineAssertions("Should contain factories we expect to see created (customised controls are loaded and cached in the BMBoardSectionViewModel, and the Shared Board Factory is used for various shared data)" + System.Environment.NewLine + message, () =>
			{
				AssertCollectionContains("Should contain the Shared Board Factory", "Shared Board Factory", activeFactories);
				AssertCollectionContains($"Should contain the {SlideShowFactory} Factory", SlideShowFactory, activeFactories);
				AssertCollectionContains($"Should contain the {BoardFactory} Factory", BoardFactory, activeFactories);
			});

			var boardFactory = BMSTestHelper.GetActiveFactory(BoardFactory);
			var slideShowFactory = BMSTestHelper.GetActiveFactory(SlideShowFactory);

			AssertBusinessObjectsHeldInFactory(boardFactory, Tuple.Create(typeof(ProcessHeader), 0));
			AssertBusinessObjectsHeldInFactory(boardFactory, Tuple.Create(typeof(ProcessTask), 0));
			AssertBusinessObjectsHeldInFactory(slideShowFactory, Tuple.Create(typeof(ProcessHeader), 0));
			AssertBusinessObjectsHeldInFactory(slideShowFactory, Tuple.Create(typeof(ProcessTask), 0));
		}

		static BoardViewModel ConstructViewModelInAnotherFactory(ZGuid boardPK)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = BoardFactory };
			var board = factory.Load<BMBoard>(boardPK);

			var slideShowFactory = new BusinessObjectFactory { NameForDebugging = SlideShowFactory };
			var boardInAnotherFactory = slideShowFactory.Load<BMBoard>(boardPK);
			var slideShow = BMSTestHelper.CreateSlideshowViewModel(boardInAnotherFactory);

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board, slideShow);
			boardViewModel.Build(board);

			return boardViewModel;
		}

		const string BoardFactory = "Should now be retained in memory";
		const string SlideShowFactory = "ConstructViewModelInAnotherFactory";

		#endregion

		#region Acceptability Bands

		public void TestSectionSubHeading_BucketWithoutAcceptabilityBand()
		{
			var section = CreateBoardSection(CreateBucket(CreateSystem()));
			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(string.Empty, viewModel.SubHeadingAppearance.SectionSubHeading);
			AssertEquals(string.Empty, viewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
			AssertColorEquals(Color.Empty, viewModel.SubHeadingAppearance.SectionHeadingBackgroundColor);
		}

		public void TestSectionSubHeading_BucketWithAcceptabilityBand_ButConfiguredNotToShowIt()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 0, 0, 0, 0, 0);
			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand); // 'Tile' by default

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(string.Empty, viewModel.SubHeadingAppearance.SectionSubHeading);
			AssertEquals(string.Empty, viewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
			AssertColorEquals(Color.Empty, viewModel.SubHeadingAppearance.SectionHeadingBackgroundColor);
		}

		public void TestSectionSubHeading_BucketWithAcceptabilityBand()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: High Risk",
@"High Risk: Number of Workflows: 0 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);

			CreateWorkflows(bucket, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Caution",
@"Caution: Number of Workflows: 10 (target is between 10 and 21)", BMConstants.CautionBoardColor);

			CreateWorkflows(bucket, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Good",
@"Good: Number of Workflows: 12 (target is between 10 and 21)", BMConstants.GoodBoardColor);

			CreateWorkflows(bucket, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Excellent",
@"Excellent: Number of Workflows: 14 (target is between 10 and 21)", BMConstants.ExcellentBoardColor);

			CreateWorkflows(bucket, 4);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Good",
@"Good: Number of Workflows: 18 (target is between 10 and 21)", BMConstants.GoodBoardColor);

			CreateWorkflows(bucket, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Caution",
@"Caution: Number of Workflows: 20 (target is between 10 and 21)", BMConstants.CautionBoardColor);

			CreateWorkflows(bucket, 2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: High Risk",
@"High Risk: Number of Workflows: 22 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);
		}

		public void TestSectionSubHeading_BucketWithAcceptabilityBand_ShouldFilterByReleaseGroup()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
			acceptabilityBand.BAB_FiltersByReleaseGroup = true;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);

			Factory.Save();

			var section = CreateBoardSection(bucket);
			section.Board.MB_GG_ReleaseGroup = group.PK;
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(" ", viewModel.SubHeadingAppearance.SectionSubHeading);
			AssertEquals("Loading acceptability bands in the background.", viewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
			AssertColorEquals(Color.AliceBlue, viewModel.SubHeadingAppearance.SectionHeadingBackgroundColor);

			CreateWorkflows(bucket, 10);
			CreateWorkflows(bucket, 1, releaseGroup: group);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Filtering by release group, so workflows outside the board's group should not be matched", "Status: High Risk",
@"High Risk: Number of Workflows: 1 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);

			CreateWorkflows(bucket, 10, releaseGroup: group);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Caution",
@"Caution: Number of Workflows: 11 (target is between 10 and 21)", BMConstants.CautionBoardColor);
		}

		public void TestSectionSubHeading_BucketWithAcceptabilityBand_BoardSectionWithNoReleaseGroup_ShouldIncludeWorkFromAllReleaseGroups()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
			acceptabilityBand.BAB_FiltersByReleaseGroup = true;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = CreateReleaseGroup(system, group1);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = CreateReleaseGroup(system, group2);

			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: High Risk",
@"High Risk: Number of Workflows: 0 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);

			CreateWorkflows(bucket, 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Caution",
@"Caution: Number of Workflows: 10 (target is between 10 and 21)", BMConstants.CautionBoardColor);

			CreateWorkflows(bucket, 2, releaseGroup: group1);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Good",
@"Good: Number of Workflows: 12 (target is between 10 and 21)", BMConstants.GoodBoardColor);

			CreateWorkflows(bucket, 2, releaseGroup: group2);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: Excellent",
@"Excellent: Number of Workflows: 14 (target is between 10 and 21)", BMConstants.ExcellentBoardColor);
		}

		public void TestSectionSubHeading_BucketWithNullAcceptabilityBandResult_ShouldBeUnknownStatus()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var band = CreateAcceptabilityBand(bucket, 0, 2, 4, 6, 8, 10, "Null Result Band", @"
				SELECT NULL ""Value"", FC_PK ""Component"", NULL ""ReleaseGroup""
				FROM dbo.BMComponent
				");

			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Heading);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			BMSTestHelper.CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(viewModel, section);

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Acceptability band should be high-risk since a value cannot be determined",
				"Status: High Risk", "High Risk: Null Result Band: could not determine result (target is between 0 and 10)", BMConstants.HighRiskBoardColor);
		}

		public void TestSectionSubHeading_BucketWithNoAcceptabilityBandResult_ShouldNotShowStatus()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var band = CreateAcceptabilityBand(bucket, 0, 2, 4, 6, 8, 10, "Null Result Band", @"
				SELECT NULL ""Value"", NULL ""Component"", NULL ""ReleaseGroup""
				FROM dbo.BMComponent
				WHERE 1 = 2
				");

			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Heading);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Should not show any acceptability band details", null, null, Color.Empty);
		}

		public void TestSectionSubHeading_BucketWithMultipleAcceptabilityBands_ShouldPickMostOffendingForHeadingAndStatus()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand1 = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var acceptabilityBand2 = CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);
			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand1, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand2, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			CreateWorkflows(bucket, 1, numberOfTasksPerWorkflow: 0);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "", "Status: High Risk",
@"High Risk: Number of Tasks per Workflow: 0 (target is between 2 and 7)
High Risk: Number of Workflows: 1 (target is between 10 and 22)", BMConstants.HighRiskBoardColor);

			CreateWorkflows(bucket, 10, numberOfTasksPerWorkflow: 0);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Number of tasks band is 'high risk' still", "Status: High Risk",
@"High Risk: Number of Tasks per Workflow: 0 (target is between 2 and 7)
Caution: Number of Workflows: 11 (target is between 10 and 22)", BMConstants.HighRiskBoardColor);

			CreateWorkflows(bucket, 2, numberOfTasksPerWorkflow: 13);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Number of tasks band is 'caution' still", "Status: Caution",
@"Caution: Number of Tasks per Workflow: 2 (target is between 2 and 7)
Good: Number of Workflows: 13 (target is between 10 and 22)", BMConstants.CautionBoardColor);

			CreateWorkflows(bucket, 2, numberOfTasksPerWorkflow: 10);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Number of tasks band is 'good' still", "Status: Good",
@"Good: Number of Tasks per Workflow: 3 (target is between 2 and 7)
Excellent: Number of Workflows: 15 (target is between 10 and 22)", BMConstants.GoodBoardColor);

			CreateWorkflows(bucket, 1, numberOfTasksPerWorkflow: 18);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Both bands 'excellent' now", "Status: Excellent",
@"Excellent: Number of Tasks per Workflow: 4 (target is between 2 and 7)
Excellent: Number of Workflows: 16 (target is between 10 and 22)", BMConstants.ExcellentBoardColor);
		}

		public void TestSectionSubHeading_BucketWithMultipleAcceptabilityBands_WithNoResult()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var acceptabilityBand1 = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var acceptabilityBand2 = CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);
			var acceptabilityBand3 = BMSTestHelper.CreateAcceptabilityBand_NullResult(bucket, 2, 3, 4, 5, 6, 7, "no result 1");
			var acceptabilityBand4 = BMSTestHelper.CreateAcceptabilityBand_NullResult(bucket, 2, 3, 4, 5, 6, 7, "no result 2");
			var acceptabilityBand5 = BMSTestHelper.CreateAcceptabilityBand_NullResult(bucket, 2, 3, 4, 5, 6, 7, "no result 3");

			var section = CreateBoardSection(bucket);

			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand1, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand2, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand3, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand4, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand5, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			CreateWorkflows(bucket, 1, numberOfTasksPerWorkflow: 0);
			Factory.Save();

			BMSTestHelper.CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(viewModel, section);
			var results = viewModel.GetAcceptabilityBandResults(Factory);
			viewModel.RefreshAcceptabilityBandSubHeading(results);

			AssertMultilineASCIIEquals("",
@"High Risk: Number of Tasks per Workflow: 0 (target is between 2 and 7)
High Risk: Number of Workflows: 1 (target is between 10 and 22)", viewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
		}

		public void TestSectionSubHeading_BucketWithMultipleAcceptabilityBands_ShouldNotShowInactiveBand()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand1 = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var acceptabilityBand2 = CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);
			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand1, AcceptabilityBandShowOnOption.Heading);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand2, AcceptabilityBandShowOnOption.Heading);

			acceptabilityBand2.BAB_IsActive = false;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			CreateWorkflows(bucket, 11, numberOfTasksPerWorkflow: 0);
			Factory.Save();

			AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Number of tasks band is inactive, so should use the only active band for the status calculation", "Status: Caution",
@"Caution: Number of Workflows: 11 (target is between 10 and 22)", BMConstants.CautionBoardColor);
		}

		public void TestBoardSectionAcceptabilityBands()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			var band1 = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var band2 = CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			section.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band1.PK;
			section.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band2.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(2, viewModel.BoardSectionAcceptabilityBands.Length);
			AssertEquals(band1.PK, viewModel.BoardSectionAcceptabilityBands.ElementAt(0).AcceptabilityBandPK);
			AssertEquals(band2.PK, viewModel.BoardSectionAcceptabilityBands.ElementAt(1).AcceptabilityBandPK);
		}

		public void TestAcceptabilityBandDelete()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var boardA = BMSTestHelper.CreateBoard(system, name: "First board");
			var sectionA = BMSTestHelper.CreateBoardSection(bucket, boardA);

			var boardB = BMSTestHelper.CreateBoard(system, name: "Second board");
			var sectionB = BMSTestHelper.CreateBoardSection(bucket, boardB);

			var bandA = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 22);
			var bandB = BMSTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;
			sectionA.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandB.PK;

			sectionB.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = bandA.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(sectionA);

			AssertEquals(bandA.PK, viewModel.BoardSectionAcceptabilityBands.ElementAt(0).AcceptabilityBandPK);
			AssertEquals(bandB.PK, viewModel.BoardSectionAcceptabilityBands.ElementAt(1).AcceptabilityBandPK);
			AssertEquals(true, sectionA.MS_LayoutData.Contains(bandA.PK.ToString()));
			AssertEquals(1, sectionA.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Count(b => b.AcceptabilityBandPK == bandA.PK));
			AssertEquals(2, sectionA.SectionConfiguration.AcceptabilityBands.Count);
			bandA.Delete();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var reloadedSectionA = newFactory.Load<BMBoardSection>(sectionA.PK);
			var reloadedSectionB = newFactory.Load<BMBoardSection>(sectionB.PK);

			AssertEquals("It should remove the AcceptabilityBand from First board.", 0, reloadedSectionA.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Count(b => b.AcceptabilityBandPK == bandA.PK));
			AssertEquals("It should not remove other AcceptabilityBands in First board.", 1, reloadedSectionA.SectionConfiguration.AcceptabilityBands.Count);
			AssertEquals("It should update the XML field", false, reloadedSectionA.MS_LayoutData.Contains(bandA.PK.ToString()));

			AssertEquals("It should remove the AcceptabilityBand from Second board.", 0, reloadedSectionB.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Count(b => b.AcceptabilityBandPK == bandA.PK));
		}

		public void TestShouldNotCalculateAcceptabilityBands_WhenWorkflowManagementModeIsEWForBWF()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system, name: "board");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 1, 2, 3, 4, 5, 6);

			section.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band.PK;

			Factory.Save();

			void TestCase(string workflowManagementMode, bool shouldCalculate)
			{
				BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowManagementMode);
				var viewModel = BMSTestHelper.CreateViewModel(section);

				if (shouldCalculate)
				{
					AssertEquals(band.PK, viewModel.BoardSectionAcceptabilityBands.ElementAt(0).AcceptabilityBandPK);
					AssertEquals(false, viewModel.SuppressAcceptabilityBandVisualisation);
					AssertEquals(1, viewModel.GetAcceptabilityBandResults(Factory).Length);
				}
				else
				{
					AssertNull(viewModel.BoardSectionAcceptabilityBands);
					Assert(viewModel.SuppressAcceptabilityBandVisualisation);
					AssertNull(viewModel.GetAcceptabilityBandResults(Factory));
				}
			}

			TestCase(WorkflowManagementModes.Codes.BasicWorkflow, shouldCalculate: false);
			TestCase(WorkflowManagementModes.Codes.EnhancedWorkflow, shouldCalculate: false);
			TestCase(WorkflowManagementModes.Codes.IncludesBufferManagement, shouldCalculate: true);
			TestCase(WorkflowManagementModes.Codes.PlanningManagement, shouldCalculate: true);
		}

		public void TestShouldNotCountWorkflowsNotOnBoard_WhenFilterBySectionAndSimpleBoardQueryIsEnabled()
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var ab = VisualBoardsTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "AB", type: AcceptabilityBandTypes.Codes.Count);
			ab.BAB_FiltersBySection = true;
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var provider = new ExperimentalSettingsProvider(section.Board.PK, Factory);
			provider.ExperimentalSettings.Add(new ExperimentalSetting { Key = ExperimentalSettingsProvider.SimpleBoardQueryExperimentalSettingsKey, Value = true.ToString() });
			provider.SaveSettings();
			BMSTestHelper.AddAcceptabilityBandToSection(section, ab);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
					TaskChannelMap.ForTest(section, viewModel, new[] { workflow1, workflow2 }), viewModel);

			var result = viewModel.GetAcceptabilityBandResults(Factory).First();
			AssertEquals(1m, result.Result.Value);
		}

		#endregion

		#region CustomisedCard

		public void TestCustomisedCardLayouts()
		{
			var system = CreateSystem();
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(CustomisedControlTypeList.Codes.TaskCard, viewModel.GetSummaryCard(string.Empty).FM_ControlType);
			AssertEquals(CustomisedControlTypeList.Codes.DetailedCard, viewModel.GetDetailedCard(string.Empty).FM_ControlType);

			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(CustomisedControlTypeList.Codes.WorkflowSummaryCard, viewModel.GetSummaryCard(string.Empty).FM_ControlType);
			AssertEquals(CustomisedControlTypeList.Codes.WorkflowDetailedCard, viewModel.GetDetailedCard(string.Empty).FM_ControlType);

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);

			AssertEquals(CustomisedControlTypeList.Codes.WorkflowSummaryCard, viewModel.GetSummaryCard(string.Empty).FM_ControlType);
			AssertEquals(CustomisedControlTypeList.Codes.WorkflowDetailedCard, viewModel.GetDetailedCard(string.Empty).FM_ControlType);
		}

		#endregion

		#region MoveWorkflow

		public void TestMoveWorkflow_HasSecurity()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "Bucket 1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "Bucket 2";

			var link1_2 = bucket1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = bucket2.PK;

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Coding 1";
			AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);

			workflow.MoveToComponent(bucket2);
			AssertEquals(bucket2.PK, workflow.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move workflow Organization - Coding 1 from component [Bucket 1] to component [Bucket 2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMoveWorkflow_NoSecurity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "Bucket 1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "Bucket 2";

			var link1_2 = bucket1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = bucket2.PK;

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Coding 1";
			AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				workflow.MoveToComponent(bucket2);
			}

			AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);
			AssertMultilineASCIIEquals("Transfer failure reason", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Buffer Management Systems -> Current Buffer Management Component
", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region ChannelSequence

		public void TestPrimaryChannelOrder_ShouldSortBySequence()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var staff1 = Factory.New<GlbStaff>();
			var staff2 = Factory.New<GlbStaff>();
			var staff3 = Factory.New<GlbStaff>();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel1.MSC_Sequence = 3;
			channel1.MSC_ParentID = staff1.PK;
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel2.MSC_Sequence = 2;
			channel2.MSC_ParentID = staff2.PK;
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel3.MSC_Sequence = 1;
			channel3.MSC_ParentID = staff3.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(3, channels.Length);
			AssertEquals(staff3.PK, channels[0].EntityPK);
			AssertEquals(staff2.PK, channels[1].EntityPK);
			AssertEquals(staff1.PK, channels[2].EntityPK);
		}

		public void TestSecondaryChannelOrder_ShouldSortBySequence()
		{
			var system = Factory.New<BMSystem>();
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var staff1 = Factory.New<GlbStaff>();
			var staff2 = Factory.New<GlbStaff>();
			var staff3 = Factory.New<GlbStaff>();
			var channel1 = BMBoardSectionTestHelper.CreateSecondaryChannelForSection(section);
			channel1.MSC_Sequence = 3;
			channel1.MSC_ParentID = staff1.PK;
			var channel2 = BMBoardSectionTestHelper.CreateSecondaryChannelForSection(section);
			channel2.MSC_Sequence = 2;
			channel2.MSC_ParentID = staff2.PK;
			var channel3 = BMBoardSectionTestHelper.CreateSecondaryChannelForSection(section);
			channel3.MSC_Sequence = 1;
			channel3.MSC_ParentID = staff3.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.SecondaryAxisChannels.ToArray();
			AssertEquals(3, channels.Length);
			AssertEquals(staff3.PK, channels[0].EntityPK);
			AssertEquals(staff2.PK, channels[1].EntityPK);
			AssertEquals(staff1.PK, channels[2].EntityPK);
		}

		#endregion

		#region ReleaseSchedulerSection

		public void TestPrimaryChannels_ReleaseSchedulerSection()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			resource1.DesignateAsCCR(buffer);
			resource2.DesignateAsCCR(buffer);

			Factory.Save();

			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource1, buffer));
			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource2, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource3, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource4, buffer));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(3, channels.Length);

			AssertCollectionContains(channels, c => c.EntityPK == resource1.PK);
			AssertCollectionContains(channels, c => c.EntityPK == resource2.PK);

			AssertEquals(ZGuid.Empty, channels[2].EntityPK);

			AssertEquals(ChannelTypeList.Codes.Resource, channels[0].EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, channels[1].EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, channels[2].EntityType);
		}

		public void TestPrimaryChannels_ReleaseSchedulerSection_NoReleaseGroup()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var section = CreateReleaseSchedulerBoardSection(buffer, null);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(1, channels.Length);

			AssertEquals(ZGuid.Empty, channels[0].EntityPK);
			AssertEquals(ZString.Empty, channels[0].ChannelEntityCode);
			AssertEquals(ChannelTypeList.Codes.Resource, channels[0].EntityType);
		}

		public void TestSecondaryChannels_ReleaseSchedulerSection()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			resource1.DesignateAsCCR(buffer);
			resource2.DesignateAsCCR(buffer);

			Factory.Save();

			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource1, buffer));
			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource2, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource3, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource4, buffer));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var secondaryChannels = viewModel.SecondaryAxisChannels.ToArray();
			AssertEquals(2, secondaryChannels.Length);

			var releasedChannel = secondaryChannels[0];
			var unReleasedChannel = secondaryChannels[1];

			AssertEquals("Released", releasedChannel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Un-Released", unReleasedChannel.GetChannelName(DisplayNameType.FullName));

			AssertEquals(ChannelTypeList.Codes.Resource, releasedChannel.EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, unReleasedChannel.EntityType);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1_unreleased = jobHeader.ProcessHeaders[0];
			workflow1_unreleased.FH_FC_CurrentComponent = bucket.PK;
			var task1 = CreateTask(workflow1_unreleased, resource1.GS_Code, 60);

			var workflow2_released = jobHeader.ProcessHeaders.AddNew();
			workflow2_released.FH_FC_CurrentComponent = buffer.PK;
			var task2 = CreateTask(workflow2_released, resource1.GS_Code, 60);

			AssertEquals(false, releasedChannel.IsInChannel(task1, false));
			AssertEquals(true, releasedChannel.IsInChannel(task2, false));

			AssertEquals(true, unReleasedChannel.IsInChannel(task1, false));
			AssertEquals(false, unReleasedChannel.IsInChannel(task2, false));

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			viewModel = BMSTestHelper.CreateViewModel(section);
			secondaryChannels = viewModel.SecondaryAxisChannels.ToArray();
			AssertEquals(2, secondaryChannels.Length);

			releasedChannel = secondaryChannels[0];
			unReleasedChannel = secondaryChannels[1];

			AssertEquals("Released", releasedChannel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Un-Released", unReleasedChannel.GetChannelName(DisplayNameType.FullName));

			AssertEquals(ChannelTypeList.Codes.Resource, releasedChannel.EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, unReleasedChannel.EntityType);

			jobHeader = CreateJobHeader<OrgHeader>();

			workflow1_unreleased = jobHeader.ProcessHeaders[0];
			workflow1_unreleased.FH_FC_CurrentComponent = bucket.PK;
			task1 = CreateTask(workflow1_unreleased, resource1.GS_Code, 60);

			workflow2_released = jobHeader.ProcessHeaders.AddNew();
			workflow2_released.FH_FC_CurrentComponent = buffer.PK;
			task2 = CreateTask(workflow2_released, resource1.GS_Code, 60);

			AssertEquals(true, releasedChannel.IsInChannel(task1, true));
			AssertEquals(true, releasedChannel.IsInChannel(task2, true));

			AssertEquals(false, unReleasedChannel.IsInChannel(task1, true));
			AssertEquals(false, unReleasedChannel.IsInChannel(task2, true));
		}

		public void TestSecondaryChannels_ReleaseSchedulerSection_NoReleaseGroup()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var secondaryChannels = viewModel.SecondaryAxisChannels.ToArray();
			AssertEquals(2, secondaryChannels.Length);

			var releasedChannel = secondaryChannels[0];
			var unReleasedChannel = secondaryChannels[1];

			AssertEquals("Released", releasedChannel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Un-Released", unReleasedChannel.GetChannelName(DisplayNameType.FullName));

			AssertEquals(ChannelTypeList.Codes.Resource, releasedChannel.EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, unReleasedChannel.EntityType);
		}

		public void TestPrimaryChannels_ReleaseSchedulerSection_NonConstrainedReleaseGroup()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			Factory.Save();

			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource1, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource2, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource3, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource4, buffer));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(1, channels.Length);

			AssertEquals(ChannelTypeList.Codes.Resource, channels[0].EntityType);
		}

		public void TestSecondaryChannels_ReleaseSchedulerSection_NonConstrainedReleaseGroup()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			LinkComponents(bucket, buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var section = CreateReleaseSchedulerBoardSection(buffer, group);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			var resource4 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3, resource4);

			Factory.Save();

			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource1, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource2, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource3, buffer));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource4, buffer));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var secondaryChannels = viewModel.SecondaryAxisChannels.ToArray();
			AssertEquals(2, secondaryChannels.Length);

			var releasedChannel = secondaryChannels[0];
			var unReleasedChannel = secondaryChannels[1];

			AssertEquals("Released", releasedChannel.GetChannelName(DisplayNameType.FullName));
			AssertEquals("Un-Released", unReleasedChannel.GetChannelName(DisplayNameType.FullName));

			AssertEquals(ChannelTypeList.Codes.Resource, releasedChannel.EntityType);
			AssertEquals(ChannelTypeList.Codes.Resource, unReleasedChannel.EntityType);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow1_unreleased = jobHeader.ProcessHeaders[0];
			workflow1_unreleased.FH_FC_CurrentComponent = bucket.PK;
			var task1 = CreateTask(workflow1_unreleased, resource1.GS_Code, 60);

			var workflow2_released = jobHeader.ProcessHeaders.AddNew();
			workflow2_released.FH_FC_CurrentComponent = buffer.PK;
			var task2 = CreateTask(workflow2_released, resource1.GS_Code, 60);

			AssertEquals(false, releasedChannel.IsInChannel(task1, false));
			AssertEquals(true, releasedChannel.IsInChannel(task2, false));

			AssertEquals(true, unReleasedChannel.IsInChannel(task1, false));
			AssertEquals(false, unReleasedChannel.IsInChannel(task2, false));
		}

		#endregion

		#region SectionName

		public void TestSectionName()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			component.FC_Name = "Mai Component";
			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;
			var config = (BMComponentSectionConfiguration)section.Configuration;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Mai Component", viewModel.SectionName);

			config.SectionNameIsOverridden = true;
			config.SectionNameOverride = "Overridden Name";
			viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Overridden Name", viewModel.SectionName);

			config.SectionNameIsOverridden = false;
			viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Mai Component", viewModel.SectionName);
		}

		public void TestSectionName_ReleaseSequencer()
		{
			var system = CreateSystem();
			var buffer = CreateBuffer(system, "Mai Buffer");
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.IsReleaseScheduler = true;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Release Gate for Mai Buffer", viewModel.SectionName);
		}

		public void TestSectionName_MultiComponentBoard()
		{
			var system = CreateSystem();
			var buffer1 = CreateBuffer(system, "Mai Buffer");
			var buffer2 = CreateBuffer(system, "Another Buffer1");
			var buffer3 = CreateBuffer(system, "Another Buffer2");

			var section = CreateBoardSection(buffer1);
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer3.PK;
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Mai Buffer, Another Buffer1, Another Buffer2", viewModel.SectionName);
		}

		public void TestSectionName_SubComponent()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Name = "Mai Buffer";
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = subBuffer.PK;

			var viewModel = BMSTestHelper.CreateViewModel(section);
			AssertEquals("Mai Buffer: sub-buffer", viewModel.SectionName);
		}

		#endregion

		#region Bitmap cache

		[TestDate(2015, 7, 14)]
		public void TestGetCardBitmaps_ShouldReturnNewCardBitmapsIfLastEditTimeChanged()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var cardContent = new TaskCardContent(task, viewModel);
			var lastEditTime = cardContent.LastEditTime;

			using (var bitmaps = GetOrCreateCardBitmaps(viewModel, cardContent))
			{
				AssertNotNull(bitmaps);
				AssertEquals("Calling GetOrCreateCardBitmaps again should return the same instance since the cached version is still valid", bitmaps, GetOrCreateCardBitmaps(viewModel, cardContent));

				TestDateAttribute.AddMinutes(1);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				var newCardContent = new TaskCardContent(task, viewModel);
				AssertNotEquals(lastEditTime, newCardContent.LastEditTime);

				using (var newBitmaps = GetOrCreateCardBitmaps(viewModel, newCardContent))
				{
					AssertNotEquals("Should return new bitmaps insteads of returning disposed bitmaps from cache", bitmaps, newBitmaps);
				}
			}
		}

		static CardBitmaps GetOrCreateCardBitmaps(BMBoardSectionViewModel viewModel, ICardContent cardContent)
		{
			return viewModel.GetOrCreateCardBitmaps(cardContent, () => new CardBitmaps(new Bitmap(1, 1), new Bitmap(1, 1)));
		}

		#endregion

		#region Filters

		#region TaskFilter and WorkflowFilter

		#endregion

		#region Filter Caching

		[RequiresSTA]
		public void TestPartialRefreshDoesntClearFilters()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK, true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK, true);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, staff1.GS_Code, 60);
			var task2 = CreateTask(workflow2, staff2.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var refreshes = new List<CellContent>();
			viewModel.ComponentGrid.CardCells.ForEach(c => c.ContentRefreshed += (s, args) => refreshes.Add(c));

			var setup = new LoadCardContentSetup(viewModel, () => section.Factory);
			var mockDispatcher = new MockDispatcher();
			BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), setup, null).ExecuteAll(mockDispatcher, mockDispatcher);
			mockDispatcher.DispatchAll();

			AssertContainsExactElementsInAnyOrder("All of the cells were refreshed during load.", viewModel.ComponentGrid.CardCells, refreshes);
			refreshes.Clear();

			viewModel.FilterManager.ApplyFilter(new SimpleCardVisibilityFilter((card, cell) => card.WorkflowIdentifier == workflow1.PK));

			AssertContainsExactElementsInAnyOrder("All of the cells were refreshed during filter application.", viewModel.ComponentGrid.CardCells, refreshes);
			refreshes.Clear();

			AssertContainsExactElementsInAnyOrder("Only one task is visible.", new[] { task1.PK }, GetVisibleCards(viewModel));

			viewModel.RefreshAll(new WorkflowUpdatedOperation(new[] { task1.PK }, Array.Empty<ZGuid>(), new BusinessObjectFactory()));
			AssertContainsExactElementsInAnyOrder("Only one of the cells were refreshed.", viewModel.ComponentGrid.CardCells.Where(c => c.Channel.EntityPK == staff1.PK), refreshes);
			AssertContainsExactElementsInAnyOrder("Only one task is visible.", new[] { task1.PK }, GetVisibleCards(viewModel));
		}

		static IEnumerable<ZGuid> GetVisibleCards(BMBoardSectionViewModel viewModel)
		{
			var filterMap = viewModel.ComponentGrid.FilterMap;
			return viewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest.SelectMany(pair => pair.Value.Where(card => filterMap.IsVisible(pair.Key, card)).Select(c => c.Identifier));
		}

		#endregion

		#endregion

		#region Misc/Other

		public void TestSection_ChildComponentModeForBufferWithOnlyConstraintComponentAsPrimaryBuffer_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system, "name", "board");

			var buffer = BMSTestHelper.CreateBuffer(system, "Has only constraint as its component(s)");
			BMSTestHelper.CreateConstraint(buffer, "Con", offsetMinutes: 64 * 60);

			var additionalBuffer = BMSTestHelper.CreateBuffer(system, "Has pre, con, post as its component(s)");
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Pre", timespanMinutes: 64 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(additionalBuffer, "Con2", offsetMinutes: 64 * 60);
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);
			var group = BMSTestHelper.CreateGroup(Factory);
			ConstrainedModeHelper.SwitchToConstrainedMode(group, buffer.PK);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.ShowZones = true;
			section.SectionConfiguration.ShowChildComponentZones = true;
			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);

			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			AssertEquals(true, section.IsInConstrainedMode);

			var ccr = BMSTestHelper.CreateStaff(Factory, "AAA");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, ccr.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, BMSTestHelper.CreateStaff(Factory, "BBB").PK);

			ccr.DesignateAsCCR(buffer);

			Factory.Save();

			AssertNoExceptionThrown(() => BMSTestHelper.CreateViewModel(section));
		}

		public void TestPopulateCacheForWorkflows_NullReferenceException()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = ProcessJobHeader.GetForParent(enquiry, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1", buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", buffer);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var task1 = CreateTask(workflow1, staff1.GS_Code, 60);
			var task2 = CreateTask(workflow2, staff2.GS_Code, 60);

			//standalone task
			var task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_Description = "standaloneTask";

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertNoExceptionThrown(delegate
			{ BMSTestHelper.CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(viewModel, section); });
		}

		public void TestGetOpenQualityIterations_ShouldUseTableValuedParameters()
		{
			ConstrainedSchematicTestConfig config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var tuple = BMSTestHelper.CreateSectionAndViewModel(config.Buffer);
			var section = tuple.Item1;
			var sectionViewModel = tuple.Item2;

			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			resource1.Groups.Add(config.ReleaseGroup);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Test Worfklow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, "", 0, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, "", 0, sequence: 2);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");

					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					sectionViewModel = BMSTestHelper.CreateViewModel(section);
					Factory.Save();
					BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, sectionViewModel, workflow), sectionViewModel);

					//This will cause the StartabilityProvider to calculate currentness
					task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					sectionViewModel = BMSTestHelper.CreateViewModel(section);
					Factory.Save();
					BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, sectionViewModel, workflow), sectionViewModel);

					var executedCommand = Db.Connection.ExecutedCommands.Where(c => c.Contains("FROM dbo.ProcessTaskIterationLink\r\n\tWHERE"));
					Assert("At least one matching command should be executed", !executedCommand.IsNullOrEmpty());

					foreach (string query in executedCommand)
					{
						CombineAssertions(() =>
						{
							AssertContains("StartabilityProvider.GetOpenQualityIterations() should use TVPs", "WHERE (P9I_P9_ContainmentBarrierTask in (SELECT Value FROM @", query);
							Assert("StartabilityProvider.GetOpenQualityIterations() should not use a parameter list)", !Regex.IsMatch(query, @"WHERE \(P9I_P9_ContainmentBarrierTask in \((@(.*?),)*?@(.*?)\)"));
							AssertNotContains("StartabilityProvider.GetOpenQualityIterations() should not use a parameter comparison (this can occur when there is only one query parameter)", "WHERE P9I_P9_ContainmentBarrierTask = ", query);
						});
					}
				}
			}
		}

		[GuiTest]
		public void TestGetOpenQualityIterations_ShouldNotThrowException_WhenMoreThanOneQualityIterationInSameWorkflow()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var staffUnderReview = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var reviewer = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			section.SectionConfiguration.OverrideChannels = true;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staffUnderReview.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, reviewer.PK);

			var mainWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow", currentComponent: config.Buffer);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", mainWorkflow.JobHeader.FH_WorkflowType);

			var workingTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			var qcbTask = CreateTask(mainWorkflow, reviewer.GS_Code, lowEstMinutes: 1, sequence: 2, taskType: "QCB");
			var lastTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 100);

			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = workingTask.PK;

				viewModel.CommitResponse();
				viewModel.CommitResponse();

				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				Factory.Save();
			}

			var bmBoardSectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var taskChannelMap = TaskChannelMap.ForTest(section, bmBoardSectionViewModel, mainWorkflow);

			var tasksFromFirstIteration = mainWorkflow.Tasks.Where(t => t.Iteration.Equals("1"));
			var workingTaskFromFirstIteration = tasksFromFirstIteration.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskFromFirstIteration = tasksFromFirstIteration.First(t => t.P9_Type == qcbTask.P9_Type);

			var taskFromSecondIteration = mainWorkflow.Tasks.Where(t => t.Iteration.Equals("2"));
			var workingTaskFromSecondIteration = taskFromSecondIteration.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskFromSecondIteration = taskFromSecondIteration.First(t => t.P9_Type == qcbTask.P9_Type);

			AssertContainsExactElementsInAnyOrder("Board should only show workable tasks", new[]
			{
				workingTaskFromSecondIteration.PK,
				qcbTaskFromSecondIteration.PK,
				workingTaskFromFirstIteration.PK,
				qcbTaskFromFirstIteration.PK,
				lastTask.PK
			}, taskChannelMap.AllTasks.Select(t => t.PK).ToArray());

			AssertNoExceptionThrown("Should not throw exception when populate startable cache", () => BMBoardSectionViewModel.CreateAndPopulatePropertyCache(taskChannelMap, bmBoardSectionViewModel));

			CombineAssertions(() =>
			{
				AssertEquals("workingTaskFromSecondIteration has no prereqs so should be startable", true, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, workingTaskFromSecondIteration.PK));
				AssertEquals("qcbTaskFromSecondIteration task should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, qcbTaskFromSecondIteration.PK));
				AssertEquals("workingTaskFromFirstIteration has no prereqs so should be startable", true, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, workingTaskFromSecondIteration.PK));
				AssertEquals("qcbTaskFromFirstIteration task should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, qcbTaskFromSecondIteration.PK));
				AssertEquals("Blocked lastTask should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, lastTask.PK));
			});
		}

		[GuiTest]
		public void TestGetOpenQualityIterations_ShouldConsiderBlockedWorkflows_WhenStartabilityIsCached()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var staffUnderReview = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var reviewer = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			section.SectionConfiguration.OverrideChannels = true;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staffUnderReview.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, reviewer.PK);

			var mainWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow", currentComponent: config.Buffer);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", mainWorkflow.JobHeader.FH_WorkflowType);

			var workingTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			var qcbTask = CreateTask(mainWorkflow, reviewer.GS_Code, lowEstMinutes: 1, sequence: 2, taskType: "QCB");
			var lastTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 100);

			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = workingTask.PK;

				viewModel.CommitResponse();

				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			Factory.Save();

			var qiWorkflow = mainWorkflow.JobHeader.ProcessHeaders.First(p => p != mainWorkflow);
			var workingTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == qcbTask.P9_Type);

			var bmBoardSectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var taskChannelMap = TaskChannelMap.ForTest(section, bmBoardSectionViewModel, mainWorkflow, qiWorkflow);

			AssertContainsExactElementsInAnyOrder("Board should only show workable tasks", new[] { workingTaskInQIWorkflow.PK, qcbTaskInQIWorkflow.PK, lastTask.PK }, taskChannelMap.AllTasks.Select(t => t.PK));

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(taskChannelMap, bmBoardSectionViewModel);

			CombineAssertions(() =>
			{
				AssertEquals("workingTaskInQIWorkflow has no prereqs so should be startable", true, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, workingTaskInQIWorkflow.PK));
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, qcbTaskInQIWorkflow.PK));
				AssertEquals("Blocked lastTask should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, lastTask.PK));
			});

			var prereqOfQIWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Prereq of Quality Iteration Workflow");
			var taskInPrereqOfQIWorkflow = CreateTask(prereqOfQIWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			prereqOfQIWorkflow.GetOrCreateDependencyLink(qiWorkflow);

			Factory.Save();

			taskChannelMap = TaskChannelMap.ForTest(section, bmBoardSectionViewModel, mainWorkflow, qiWorkflow);
			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(taskChannelMap, bmBoardSectionViewModel);

			AssertContainsExactElementsInAnyOrder("Board should still only show workable tasks", new[] { workingTaskInQIWorkflow.PK, qcbTaskInQIWorkflow.PK, lastTask.PK }, taskChannelMap.AllTasks.Select(t => t.PK));

			CombineAssertions(() =>
			{
				AssertEquals("workingTaskInQIWorkflow has prereqs workflow so should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, workingTaskInQIWorkflow.PK));
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, qcbTaskInQIWorkflow.PK));
				AssertEquals("Blocked lastTask should not be startable", false, GetIsStartableCachedValue(bmBoardSectionViewModel.Cache, lastTask.PK));
			});
		}

		#endregion

		#region Capacity

		[TestDate(2024, 2, 08)]
		public void TestPopulateCache_ShouldCallCapacityQueryOnlyOnceAndCacheResults()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var staff1 = BMSTestHelper.CreateStaff(Factory, "ST1");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "ST2");
			var channel1 = BMBoardSectionTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);
			var channel2 = BMBoardSectionTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			var propertyCache = new PropertyCache();
			var newFactory = new BusinessObjectFactory();

			using (var track = TestConnection.TrackExecutedCommands())
			{
				viewModel.PopulateRoadRunnerDetails(newFactory, propertyCache, viewModel.ComponentGrid.CardAllocationMap, viewModel.PrimaryChannels, viewModel.ReleaseGroupPK);

				var capacityQueryCount = TestConnection.ExecutedCommands.Count(s => s.Contains("CAPACITY CALCULATION SIMPLE QUERY"));
				AssertEquals("Should hit query only once", 1, capacityQueryCount);

				var numberOfStaffs = TestConnection.ExecutedCommands.Count(s => s.Contains("CAPACITY CALCULATION GetNumberOfStaffInCapabilities"));
				AssertEquals("Should be one staff", 1, numberOfStaffs);

				var fetchHintQuery = TestConnection.ExecutedCommands.Count(s => s.Contains("GW_ParentTableCode") && s.Contains(GlbStaffSchema.Constants.Prefix));
				AssertEquals("Should hit the fetch hint query that contains a reference to the GlbStaff parent table code", 1, fetchHintQuery);
			}

			var cacheCapacities = propertyCache.GetDumpOfCacheForTest().Where(kv => kv.Key.Contains("ChannelCapacity")).Select(kv => kv.Key).ToArray();
			AssertEquals("Should add 2 capacities in local cache", 2, cacheCapacities.Length);
			Assert("Staff1 capacity is in local cache", cacheCapacities.Any(c => c.Contains(staff1.PK.ToString())));
			Assert("Staff2 capacity is in local cache", cacheCapacities.Any(c => c.Contains(staff2.PK.ToString())));

			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			propertyCache.Clear();

			using (var track = TestConnection.TrackExecutedCommands())
			{
				viewModel.PopulateRoadRunnerDetails(Factory, propertyCache, viewModel.ComponentGrid.CardAllocationMap, viewModel.PrimaryChannels, viewModel.ReleaseGroupPK);

				var capacityQueryCount = TestConnection.ExecutedCommands.Count(s => s.Contains("CAPACITY CALCULATION"));
				AssertEquals("Should not hit query, capacity is in DB cache", 0, capacityQueryCount);

				var fetchHintQuery = TestConnection.ExecutedCommands.Count(s => s.Contains("GW_ParentTableCode") && s.Contains(GlbStaffSchema.Constants.Prefix));
				AssertEquals("The fetch hint query should not be hit at all since capacity is cached", 0, fetchHintQuery);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		bool GetIsStartableCachedValue(PropertyCache cache, ZGuid taskPK) => cache.GetCachedValue<bool>(taskPK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent);

		#endregion
	}

	public class BMBoardSectionViewModelTestWithAsync : BMSTestCaseWithFactory
	{
		public void TestPopulateCacheForTasks_WhenGetApprovedShapeDetailsDoesNotExplode_NullReferenceException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			channel.MSC_ParentID = staff1.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			var task1 = BMSTestHelper.CreateTask(workflow, staff1.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, staff1.GS_Code, 60);

			Factory.Save();

			var nullAsyncStrategyMock = new Mock<IAsyncStrategy>();
			nullAsyncStrategyMock.Setup(a => a.GetAsync(It.IsAny<Func<Dictionary<ZGuid, ApprovedShapeDetails>>>(), It.IsAny<IThreadSentry>(), nameof(ApprovedShapeBufferPenetrationService), It.IsAny<CancellationTokenSource>()))
								 .Returns((Task.FromResult<Dictionary<ZGuid, ApprovedShapeDetails>>(null)));

			var viewModel = BMSTestHelper.CreateViewModel(section);
			using (VisualBoardsTestCase.ApplyAsyncStrategy(nullAsyncStrategyMock.Object))
			{
				AssertNoExceptionThrown(delegate
				{ BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, viewModel, workflow), viewModel); });
			}
		}
	}
}

