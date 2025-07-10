using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormAllFactoriesHitTest : BMSGUITestCase
	{
		[TestDate(2017, 8, 17)]
		public void TestLoadHitsAcrossAllFactories_WithConstraintsAndReleaseGroups()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			system.ReleaseGroups.AddNew().FSG_GG_Group = releaseGroup.PK;

			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1", sequence: 0);

			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2", sequence: 0);
			var buffer3 = BMSTestHelper.CreateBuffer(system, "Buffer3", sequence: 0);
			var buffer4 = BMSTestHelper.CreateBuffer(system, "Buffer4", sequence: 0);

			var buffer5 = BMSTestHelper.CreateBuffer(system, "Buffer5", sequence: 0);
			var buffer6 = BMSTestHelper.CreateBuffer(system, "Buffer6", sequence: 0);

			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(buffer1, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(buffer5, board, row: 1);
			var section3 = BMSTestHelper.CreateBoardSection(buffer6, board, row: 2);

			BMSTestHelper.CreateAdditionalComponent(section1, buffer2);
			BMSTestHelper.CreateAdditionalComponent(section1, buffer3);
			BMSTestHelper.CreateAdditionalComponent(section1, buffer4);

			BMSTestHelper.CreateSubBuffer(section1.Component, "A");
			BMSTestHelper.CreateConstraint(section1.Component, "B");
			BMSTestHelper.CreateSubBuffer(section1.Component, "C");

			BMSTestHelper.CreateSubBuffer(section2.Component, "X");
			BMSTestHelper.CreateConstraint(section2.Component, "Y");
			BMSTestHelper.CreateSubBuffer(section2.Component, "Z");

			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.ShowZones = true;
			section2.SectionConfiguration.OverrideChannels = true;
			section2.SectionConfiguration.ShowZones = true;

			section1.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section2.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "A", "A");
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff1.PK);
			var componentLink = section1.Component.GetOrCreateResourceLink(staff1.GS_Code);
			componentLink.FD_IsCapacityConstrained = true;
			componentLink.FD_GS_NKDesignatedAsCapacityConstrainedBy = GlbStaff.CurrentUser.GS_Code;

			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "B", "B");
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, staff2.PK);

			releaseGroup.Staff.AddRange(new[] { staff1, staff2 });

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);

			var viewModel = VisualBoardFormTest.GetViewModel(loadedBoard);
			using (var form = new VisualBoardForm(viewModel))
			{
				var expectedHits = new Dictionary<string, int>
				{
					{ BMBoardSchema.Constants.TableName, 5 },
					{ BMBoardSectionSchema.Constants.TableName, 9 },
					{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 6 },
					{ BMBoardSectionChannelSchema.Constants.TableName, 9 },
					{ BMComponentSchema.Constants.TableName, 0 },
					{ BMComponentReleaseGroupLinkSchema.Constants.TableName, 3 }, // BoardDataSource, ChannelViewModelsPipe factories
					{ BMComponentResourceLinkSchema.Constants.TableName, 2 },
					{ BMControlCustomisationLinkSchema.Constants.TableName, 3 },
					{ BMSystemSchema.Constants.TableName, 1 },
					{ BMSystemReleaseGroupSchema.Constants.TableName, 3 }, // InConstrainedMode using BoardDataSource factory + one staff designated as capacity constrained in ChannelViewModelsPipe factory
					{ GlbHolidaySchema.Constants.TableName, 1 },
					{ GlbResourceCapabilityPivotSchema.Constants.TableName, 2 },
					{ GlbStaffSchema.Constants.TableName, 2 },
					{ GlbStaffHolidaySchema.Constants.TableName, 2 },
					{ GlbWorkTimeSchema.Constants.TableName, 3 },
					{ ProcessTasksSchema.Constants.TableName, 13 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ StmModuleFilterSchema.Constants.TableName, 0 },
				};

				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflowInMainBuffer ", buffer1);
				BMSTestHelper.CreateTask(workflow, staff1.GS_Code, 10);
				BMSTestHelper.CreateTask(workflow, staff2.GS_Code, 10);

				using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f), ignoreHitsFromTablesCachedInUberFactory: true))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		[TestDate(2019, 1, 1)]
		public void TestBoardLoad_InConstrainedMode_ShouldNotHitBMSystemExcessively()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var otherBuffer = BMSTestHelper.CreateBuffer(config.System, "Otherre Bufferre");
			BMSTestHelper.LinkComponents(config.Bucket, otherBuffer);

			var board = BMSTestHelper.CreateBoard(config.System);
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board, board);
			var section1 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var section3 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			BMSTestHelper.CreateAdditionalComponent(section2, otherBuffer);
			section1.Row = 0;
			section2.Row = 1;
			section3.Row = 2;

			section1.RowHeightPercent = 48;
			section2.RowHeightPercent = 48;
			section3.RowHeightPercent = 4;

			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, config.CCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, config.NonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, config.NonCCR2.PK);

			const int numWorkflowSets = 10;

			for (var i = 0; i < numWorkflowSets; i++)
			{
				var workflowInMainBuffer = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflowInMainBuffer " + i, config.Buffer);
				BMSTestHelper.CreateTask(workflowInMainBuffer, config.NonCCR1.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInMainBuffer, config.CCR.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInMainBuffer, config.NonCCR2.GS_Code, 10);

				var workflowInOtherBuffer = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflowInOtherBuffer " + i, otherBuffer);
				BMSTestHelper.CreateTask(workflowInOtherBuffer, config.NonCCR1.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInOtherBuffer, config.CCR.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInOtherBuffer, config.NonCCR2.GS_Code, 10);

				var workflowInBucket = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflowInBucket " + i, config.Bucket);
				BMSTestHelper.CreateTask(workflowInBucket, config.NonCCR1.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInBucket, config.CCR.GS_Code, 10);
				BMSTestHelper.CreateTask(workflowInBucket, config.NonCCR2.GS_Code, 10);
			}

			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ BMSystemSchema.Constants.TableName, 1 },
			};

			RowFactory.ResetCacheAfterDbUpgrade();

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 50))
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(1280, 1024);
				Application.DoEvents();

				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				Assert("There should be one ticket per workflow in the release scheduler section (but not all will fit on screen)", FindTaskCardControls(section1Control).Length > 0);
				Assert("There should be one ticket per task per channel in the buffer section (but not all will fit on screen)", FindTaskCardControls(section2Control).Length > 0);
			}
		}

		[TestDate(2017, 10, 5)]
		[StressTest]
		public void TestRefreshHits_WhenCrazyTreeHierarchyExists()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			const int numberOfTrees = 3;
			const int maxTreeDepth = 4;
			const int nodeChildBreadth = 4;

			for (var treeNum = 0; treeNum < numberOfTrees; treeNum++)
			{
				var root = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

				PopulateTree(1, maxTreeDepth, nodeChildBreadth, config.Buffer, config.Bucket, root, parentWorkflow: null);
				AssertEquals(340, root.ProcessHeaders.Count);
			}

			Factory.Save();

			var hits = new Dictionary<string, int>
			{
				{ BMBoardSchema.Constants.TableName, 3 },
				{ BMBoardSectionSchema.Constants.TableName, 4 },
				{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 2 },
				{ BMBoardSectionChannelSchema.Constants.TableName, 3 },
				{ BMControlCustomisationLinkSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 0 }, // Shapes should never be loaded on boards. ViewApprovedWorkflowSchedule has everything we need.
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 3 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 4 },
				{ ProcessTasksSchema.Constants.TableName, 18 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(hits, ignoreHitsFromTablesCachedInUberFactory: true, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f)))
			using (var form = GetAndShowVisualBoardForm(section))
			{
			}
		}

		static void PopulateTree(int currentDepth, int maxDepth, int nodeChildBreadth, BMComponent evenDepthComponent, BMComponent oddDepthComponent, ProcessJobHeader jobHeader, ProcessHeader parentWorkflow)
		{
			var component = currentDepth % 2 == 0 ? evenDepthComponent : oddDepthComponent;

			for (var breadth = 0; breadth < nodeChildBreadth; breadth++)
			{
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, $"Workflow {currentDepth}.{breadth}", component);
				BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

				if (parentWorkflow != null)
				{
					workflow.GetOrCreateLinkToParent(parentWorkflow).FP_SynchroniseBufferPenetration = true;
				}

				if (currentDepth < maxDepth)
				{
					PopulateTree(currentDepth + 1, maxDepth, nodeChildBreadth, evenDepthComponent, oddDepthComponent, jobHeader, workflow);
				}
			}
		}

		[TestDate(2019, 4, 15)]
		public void TestLoadHitsAcrossAllFactories_ManyRelationshipsWithBuffersAndSubBuffers()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);
			var sections = new List<BMBoardSection>();

			for (int i = 0; i < 3; ++i)
			{
				var buffer = BMSTestHelper.CreateBuffer(system, "Buffer" + i, sequence: 0);
				sections.Add(BMSTestHelper.CreateBoardSection(buffer, board, row: i));

				for (int j = 0; j < 3; ++j)
				{
					var relationship = CreateComponentRelationshipWithBuffersAndSubBuffers(2, 2, system, "Relationship" + i + " " + j);
					BMSTestHelper.CreateAdditionalComponent(sections[i], relationship);
				}
			}

			sections[0].SectionConfiguration.OverrideChannels = true;

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aanensen");
			BMSTestHelper.CreatePrimaryChannelForSection(sections[0], ChannelTypeList.Codes.Resource, staff1.PK);

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			var newFactory = new BusinessObjectFactory();
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);

			var viewModel = VisualBoardFormTest.GetViewModel(loadedBoard);
			using (var form = new VisualBoardForm(viewModel))
			{
				var expectedHits = new Dictionary<string, int>
				{
					{ BMBoardSchema.Constants.TableName, 5 },
					{ BMBoardSectionSchema.Constants.TableName, 8 },
					{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 5 },
					{ BMBoardSectionChannelSchema.Constants.TableName, 9 },
					{ BMComponentSchema.Constants.TableName, 0 }, // hits eliminated from BoardDataSource and ChannelViewModelsPipe factories
					{ BMComponentLinkSchema.Constants.TableName, 13 }, // hits on the above factory, needed for allcomponents.
					{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
					{ BMControlCustomisationLinkSchema.Constants.TableName, 3 },
					{ BMSystemSchema.Constants.TableName, 1 },
					{ BMSystemReleaseGroupSchema.Constants.TableName, 2 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbDepartmentSchema.Constants.TableName, 1 },
					{ GlbHolidaySchema.Constants.TableName, 1 },
					{ GlbResourceCapabilityPivotSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ GlbStaffHolidaySchema.Constants.TableName, 1 },
					{ GlbWorkTimeSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 8 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ StmModuleFilterSchema.Constants.TableName, 14 },
				};

				using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, includeFactoryPredicate: f => BMSTestHelper.IsPAVEFactory(f), ignoreHitsFromTablesCachedInUberFactory: false))
				{
					form.Show();
					Application.DoEvents();
				}
			}
		}

		public void TestDBHitsForVisualBoardStartup_WhenLoadingHeadingAcceptabilityBands()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "SYS");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system, "BucketFirst", sequence: 0);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var bandNum = 20;

			for (int i = 1; i < bandNum; i++)
			{
				var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 1, 2, 3, 4, 5, 6, "HeadingButte" + i);
				var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);
				sectionBand.ShowOn = "Heading";
			}

			Factory.Save();

			AssertDBHitsForBoard(
				expectedHitsForOpeningBoard: new Dictionary<string, int>
				{
					{ BMComponentAcceptabilityBand.Schema.TableName, 2 },
					{ BMBoard.Schema.TableName, 3 },
					{ BMBoardSection.Schema.TableName, 4 },
					{ BMBoardSectionAdditionalComponent.Schema.TableName, 3 },
					{ BMBoardSectionChannel.Schema.TableName, 3 },
					{ BMComponent.Schema.TableName, 2 },
					{ BMControlCustomisationLink.Schema.TableName, 1 },
					{ BMSystem.Schema.TableName, 1 },
					{ BMSystemReleaseGroup.Schema.TableName, 1 },
					{ GlbBranch.Schema.TableName, 1 },
					{ GlbDepartment.Schema.TableName, 1 },
					{ GlbGroup.Schema.TableName, 1 },
					{ ProcessHeader.Schema.TableName, 0 },
					{ ProcessTasks.Schema.TableName, 1 },
					{ StmModuleFilter.Schema.TableName, 5 },
				},
				expectedHitsForRefreshingBoardWithNoCustomFilters: new Dictionary<string, int>
				{
					{ BMComponentAcceptabilityBand.Schema.TableName, 1 },
					{ BMBoard.Schema.TableName, 2 },
					{ BMBoardSection.Schema.TableName, 2 },
					{ BMBoardSectionAdditionalComponent.Schema.TableName, 1 },
					{ BMBoardSectionChannel.Schema.TableName, 2 },
					{ BMComponent.Schema.TableName, 0 },
					{ BMControlCustomisationLink.Schema.TableName, 1 },
					{ BMSystem.Schema.TableName, 0 },
					{ GlbBranch.Schema.TableName, 0 },
					{ GlbDepartment.Schema.TableName, 0 },
					{ ProcessHeader.Schema.TableName, 0 },
					{ ProcessTasks.Schema.TableName, 1 },
					{ StmModuleFilter.Schema.TableName, 2 },
				},
				expectedHitsForRefreshingBoardWithCustomFilter: new Dictionary<string, int>
				{
					{ BMComponentAcceptabilityBand.Schema.TableName, 1 },
					{ BMBoard.Schema.TableName, 2 },
					{ BMBoardSection.Schema.TableName, 3 },
					{ BMBoardSectionAdditionalComponent.Schema.TableName, 2 },
					{ BMBoardSectionChannel.Schema.TableName, 4 },
					{ BMComponent.Schema.TableName, 0 },
					{ BMControlCustomisationLink.Schema.TableName, 0 },
					{ BMSystem.Schema.TableName, 0 },
					{ GlbBranch.Schema.TableName, 0 },
					{ GlbDepartment.Schema.TableName, 0 },
					{ ProcessHeader.Schema.TableName, 1 },
					{ ProcessTasks.Schema.TableName, 1 },
					{ StmModuleFilter.Schema.TableName, 4 },
				}, board: board, useOnlyNewFactories: true, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f));
		}

		public void TestFactoriesGetGarbageCollection()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = CreateSystem("ORG");
			var buffer1 = CreateBuffer(system, "Buffer1", sequence: 0);
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(buffer1, board);
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.CellsPerSubsection = 1;

			var channel1 = VisualBoardsTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			CreateTask(CreateWorkflow(jobHeader, "Gerp1"), staff.GS_Code, 20);
			CreateTask(CreateWorkflow(jobHeader, "Gerp2"), staff.GS_Code, 20);

			Factory.Save();

			AssertFactoriesGetGarbageCollected(board);
		}

		public void TestFactoriesGetGarbageCollection_WithRoadRunnerThingsMaybe()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = CreateSystem("ORG");
			var buffer1 = CreateBuffer(system, "Buffer1", sequence: 0);
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(buffer1, board);
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.ShowZones = true;
			section1.SectionConfiguration.CellsPerSubsection = 4;

			var channel1 = VisualBoardsTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			CreateTask(CreateWorkflow(jobHeader, "Gerp1"), staff.GS_Code, 20);
			CreateTask(CreateWorkflow(jobHeader, "Gerp2"), staff.GS_Code, 20);

			Factory.Save();

			AssertFactoriesGetGarbageCollected(board);
		}

		public void TestFactoriesGetGarbageCollection_Bucket()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = CreateSystem("ORG");
			var buffer1 = CreateBucket(system, "Bucket1", sequence: 0);
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(buffer1, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK, true);

			var jobHeader = CreateJobHeader<OrgHeader>();
			CreateTask(CreateWorkflow(jobHeader, "Gerp1"), staff.GS_Code, 20);
			CreateTask(CreateWorkflow(jobHeader, "Gerp2"), staff.GS_Code, 20);

			Factory.Save();

			AssertFactoriesGetGarbageCollected(board);
		}

		public void TestFactoriesGetGarbageCollection_BufferChanneledByCapability()
		{
			var capability = CreateCapability("NIN", "Nino");

			var system = CreateSystem("ORG");
			var buffer1 = CreateBuffer(system, "Buffer1", sequence: 0);
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(buffer1, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Capability, capability.PK, true);

			var jobHeader = CreateJobHeader<OrgHeader>();
			CreateTask(CreateWorkflow(jobHeader, "Gerp1"), string.Empty, 20, capability: capability);
			CreateTask(CreateWorkflow(jobHeader, "Gerp2"), string.Empty, 20, capability: capability);

			Factory.Save();

			AssertFactoriesGetGarbageCollected(board);
		}

		public void TestFactoriesGetGarbageCollection_WorkflowCards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = CreateSystem("ORG");
			var buffer1 = CreateBuffer(system, "Buffer1", sequence: 0);
			var board = CreateBoard(system);
			var section1 = CreateBoardSection(buffer1, board);
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var jobHeader = CreateJobHeader<OrgHeader>();
			CreateTask(CreateWorkflow(jobHeader, "Gerp1"), staff.GS_Code, 20);
			CreateTask(CreateWorkflow(jobHeader, "Gerp2"), staff.GS_Code, 20);

			var channel1 = VisualBoardsTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertFactoryWasGarbageCollected("SetupTasks");

				form.RefreshNow_ForTest(false);
				Application.DoEvents();

				AssertFactoryWasGarbageCollected("SetupTasks");
			}
		}

		public void TestConstructViewModel_ShouldNotHoldFactoryWhileAlertStatusExists()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var helper = new AlertStatusTestHelper(Factory, system);

			Factory.Save();

			using (var form = new VisualBoardForm(helper.ViewModel))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				form.Show();
				Application.DoEvents();

				GC.Collect();
				GC.WaitForPendingFinalizers();

				AssertNull(BMSTestHelper.GetActiveFactory("ChannelViewModelsPipe"));
			}
		}

		[TestDate(2017, 10, 5)]
		public void TestLoadBoard_ShouldNotHitProcessTaskIterationLinkTableOnceForEachCurrentTask()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, 10, 2, staff: GlbStaff.CurrentUser);

			foreach (var workflow in workflows)
			{
				workflow.Tasks.First().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			Factory.Save();

			var expectedHits = new Dictionary<string, int> { { ProcessTaskIterationLinkSchema.Constants.TableName, 1 } };

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreUnspecified: true, thresholdForUnspecified: 100))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		[TestDate(2017, 07, 04)]
		public void TestLoadBoard_ShouldNotHitBMControlCustomisationTableExcessivelyOnBoardLoad()
		{
			var staff = BMSTestHelper.GetOrCreateStaff(Factory, "AAA", "Aaron A. Bearonson");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1", sequence: 0);
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2", sequence: 1);
			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(buffer1, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(buffer2, board, row: 1);
			section1.SectionConfiguration.OverrideChannels = true;
			section2.SectionConfiguration.OverrideChannels = true;

			var sectionDetailedCard1 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, backgroundColor: "Chartreuse", name: "Layout");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section1, sectionDetailedCard1, jobType: "WKI");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section2, sectionDetailedCard1, jobType: "WKI");
			BMSTestHelper.CreateLine(sectionDetailedCard1, PropertySourceList.Codes.Workflow, "CurrentTaskResourceName", PropertyTypeList.Codes.Text, "LayoutLine", 0, 0, 100, 20, Color.Black.Name, Color.Black.Name, 10, isBold: false, readOnly: false, autoSize: false);

			var sectionDetailedCard2 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Layin");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section1, sectionDetailedCard2, jobType: "ACI");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section2, sectionDetailedCard2, jobType: "ACI");
			BMSTestHelper.CreateLine(sectionDetailedCard2, PropertySourceList.Codes.Workflow, "CurrentTaskResourceName", PropertyTypeList.Codes.Text, "LayinLine", 0, 0, 100, 20, Color.Black.Name, Color.Black.Name, 10, isBold: false, readOnly: true, autoSize: false);

			var sectionDetailedCard3 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Layup");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section1, sectionDetailedCard3, jobType: "AMW");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section2, sectionDetailedCard3, jobType: "AMW");
			BMSTestHelper.CreateLine(sectionDetailedCard3, PropertySourceList.Codes.Workflow, "CurrentTaskResourceName", PropertyTypeList.Codes.Text, "LayupLine", 0, 0, 100, 20, Color.Black.Name, Color.Black.Name, 10, isBold: false, readOnly: false, autoSize: false);

			var sectionDetailedCard4 = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, name: "Laydown");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section1, sectionDetailedCard4, jobType: "PNV");
			BMSTestHelper.CreateControlCustomisationLink(Factory, section2, sectionDetailedCard4, jobType: "PNV");
			BMSTestHelper.CreateLine(sectionDetailedCard4, PropertySourceList.Codes.Workflow, "CurrentTaskResourceName", PropertyTypeList.Codes.Text, "LaydownLine", 0, 0, 100, 20, Color.Black.Name, Color.Black.Name, 10, isBold: false, readOnly: false, autoSize: false);

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			RowFactory.ResetCacheAfterDbUpgrade();

			AssertEquals("CurrentTaskResourceName", sectionDetailedCard1.CustomisationLines[0].PropertyName);
			AssertEquals("CurrentTaskResourceName", sectionDetailedCard2.CustomisationLines[0].PropertyName);
			AssertEquals("CurrentTaskResourceName", sectionDetailedCard3.CustomisationLines[0].PropertyName);
			AssertEquals("CurrentTaskResourceName", sectionDetailedCard4.CustomisationLines[0].PropertyName);

			var expectedHits = new Dictionary<string, int>
			{
				{ BMBoardSchema.Constants.TableName, 4 },
				{ BMBoardSectionSchema.Constants.TableName, 9 },
				{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 5 },
				{ BMBoardSectionChannelSchema.Constants.TableName, 6 },
				{ BMComponentSchema.Constants.TableName, 4 },
				{ BMComponentResourceLinkSchema.Constants.TableName, 1 },
				{ BMControlCustomisationSchema.Constants.TableName, 1 },
				{ BMControlCustomisationLinkSchema.Constants.TableName, 2 },
				{ BMSystemSchema.Constants.TableName, 0 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 0 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbDepartmentSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 1 },
				{ GlbResourceCapabilityPivotSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbStaffHolidaySchema.Constants.TableName, 1 },
				{ GlbWorkTimeSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 8 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmModuleFilterSchema.Constants.TableName, 8 },
				{ TagDefinitionSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 1 },
			};

			using (DisableAsyncBehaviour())
			using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f)))
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void MakeWorkflowsWithAParticularMagnitude(BMComponent buffer, TagMagnitude magnitude, GlbStaff staff, int workflowNum, int taskNum)
		{
			var workflows = BMSTestHelper.CreateWorkflows(buffer, workflowNum, taskNum);
			workflows.ForEach(w =>
			{
				w.FH_FC_CurrentComponent = buffer.PK;
				w.GetTasksWithoutAccessingWorkflowParent().ForEach(t => t.P9_GS_NKAssignedStaffMember = staff.GS_Code);
				w.AddTag(magnitude);
			});
		}

		public void TestRefresh_ShouldNotQueryBMControlCustomisationForEachLink()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var workflowDescriptors = (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>();
			var workflypes = workflowDescriptors.GetAllCodes();

			for (var i = 0; i < 20; i++)
			{
				var customisation = BMSTestHelper.CreateControlCustomisation(Factory, name: "Customisation " + i);
				BMSTestHelper.CreateControlCustomisationLink(Factory, config.BufferBoard, customisation, workflypes[i]);
			}

			Factory.Save();

			var expectedHits = new Dictionary<string, int> { { BMControlCustomisationSchema.Constants.TableName, 1 } };

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 10, useOnlyNewFactories: true))
			{
				form.RefreshBoard();
				Application.DoEvents();
			}
		}

		[TestDate(2022, 10, 26)]
		public void TestLoadBoard_WithManyWorkflows_AndReleaseSequencesEnabled_ShouldNotHitBMReleaseSequenceItemOncePerWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			var board = config.BufferBoard;

			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(config.Buffer, 100, 1, config.ReleaseGroup);
			Factory.Save();

			var expectedHits = new Dictionary<string, int>
			{
				{ BMReleaseSequenceItemSchema.Constants.TableName, 4 },
			};

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 50))
			using (var form = GetAndShowVisualBoardForm(board))
			{
				var tickets = form.FindAll<TaskCardControl>();
				AssertGreaterThan("Just ensuring that the board actually did load, it will render as many tickets in zone 3 as it can.", tickets.Count(), 40);
			}
		}

		public static void AssertDBHitsForBoard(Dictionary<string, int> expectedHitsForOpeningBoard, Dictionary<string, int> expectedHitsForRefreshingBoardWithNoCustomFilters, Dictionary<string, int> expectedHitsForRefreshingBoardWithCustomFilter,
			BMBoard board, bool useOnlyNewFactories = false, Predicate<BusinessObjectFactory> includeFactoryPredicate = null)
		{
			AssertDBHitsForOpeningBoard();
			AssertDBHitsForRefreshingBoardWithNoCustomFilters();
			AssertDBHitsForRefreshingBoardWithCustomFilter(expectedHitsForRefreshingBoardWithCustomFilter, board, useOnlyNewFactories: useOnlyNewFactories, includeFactoryPredicate: includeFactoryPredicate);

			void AssertDBHitsForOpeningBoard()
			{
				var viewModel = VisualBoardFormTest.GetViewModel(board);
				using (var form = new VisualBoardForm(viewModel))
				using (AssertMaxDbHitsForAllFactories("Opening a board with no custom filters", expectedHitsForOpeningBoard, useOnlyNewFactories: useOnlyNewFactories, includeFactoryPredicate: includeFactoryPredicate))
				{
					form.Show();
					Application.DoEvents();
				}
			}

			void AssertDBHitsForRefreshingBoardWithNoCustomFilters()
			{
				var viewModel = VisualBoardFormTest.GetViewModel(board);
				using (var form = new VisualBoardForm(viewModel))
				{
					form.Show();
					Application.DoEvents();

					using (AssertDbHitsForAllFactories("Refreshing a board with no custom filters", expectedHitsForRefreshingBoardWithNoCustomFilters, useOnlyNewFactories: useOnlyNewFactories, includeFactoryPredicate: includeFactoryPredicate))
					{
						form.RefreshNow_ForTest();
						Application.DoEvents();
					}
				}
			}
		}

		/// <summary>
		/// This method should be used to assert the number of hits in the pessimistic scenario when a custom filter is set.
		/// When custom filters are not set, workflow PKs loaded on a board are taken to create a section workflow filter for acceptability band calculations in order to reduce the number of db hits.
		/// Otherwise when a custom filter is set, an extra hit to WorkflowLoader (and db) occurs.
		/// (Сustom filters are temporary filters set on the section view model but not on the section configuraton; they are not stored in the database.)
		/// </summary>
		public static void AssertDBHitsForRefreshingBoardWithCustomFilter(Dictionary<string, int> expectedHits, BMBoard board, bool useOnlyNewFactories = false, Predicate<BusinessObjectFactory> includeFactoryPredicate = null)
		{
			var factory = new BusinessObjectFactory();
			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(factory, "CustomFilter");
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Life is like riding a bicycle. To keep your balance, you must keep moving. (Albert Einstein)");

			var viewModel = VisualBoardFormTest.GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				BMSTestHelper.ClearAcceptabilityBandCache();
				var control = form.FindAll<BMComponentControl>().Single();
				using (AssertDbHitsForAllFactories("Refreshing a board with a custom filter", expectedHits, useOnlyNewFactories: useOnlyNewFactories, includeFactoryPredicate: includeFactoryPredicate))
				{
					control.SetCustomWorkflowFilterForTestingAndRefresh(customFilter);
				}
			}
		}

		#region Implementation

		class DummyWithWorkflowAndFetchStrategy : DummyWithWorkflow
		{
			public DummyWithWorkflowAndFetchStrategy(BusinessObjectFactory factory, System.Data.DataRow row)
				: base(factory, row)
			{
			}

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new DummyFetchForViewStrategy(this);
			}

			public DummyBusinessObject ChildField
			{
				get
				{
					return Factory.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, Z0_Guid));
				}
			}

			class DummyFetchForViewStrategy : BusinessObjectFetchStrategy
			{
				public DummyFetchForViewStrategy(DummyWithWorkflowAndFetchStrategy dummy)
					: base(dummy)
				{
					this.dummy = dummy;
				}
				readonly DummyWithWorkflowAndFetchStrategy dummy;

				protected override void FetchForViewCore(TableColumn[] columns)
				{
					var column = columns.FirstOrDefault(c => c.ColumnName == "<ChildField>");
					if (column != null)
					{
						Factory.AddFetchHint(DummyBizoSchema.Instance, new ZQuery(DummyBizoSchema.PK, dummy.Z0_Guid));
					}
				}
			}
		}

		ComponentRelationship CreateComponentRelationshipWithBuffersAndSubBuffers(int numBuffers, int subBuffersPerBuffer, BMSystem system, string name)
		{
			var relationship = BMSTestHelper.CreateComponentRelationship(Factory, name);

			for (int i = 0; i < numBuffers; ++i)
			{
				var relatedBuffer = BMSTestHelper.CreateBuffer(system, name: name + " Buffer" + i, sequence: 0);
				BMSTestHelper.CreateComponentRelationshipLink(Factory, relationship, relatedBuffer);

				for (int j = 0; j < subBuffersPerBuffer; ++j)
				{
					var subBuffer = BMSTestHelper.CreateSubBuffer(relatedBuffer, name: name + " SubBuffer" + i + " " + j, sequence: 0);
				}
			}

			return relationship;
		}

		static void AssertFactoriesGetGarbageCollected(BMBoard board)
		{
			var viewModel = VisualBoardFormTest.GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertFactoryWasGarbageCollected("SetupTasks", "Pipe", "Channel");

				form.RefreshNow_ForTest(false);
				Application.DoEvents();

				AssertFactoryWasGarbageCollected("SetupTasks", "Pipe", "Channel");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WorkflowDataRegistry.Instance.EnableWorkflowEstimateMeasurement.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			disposables = new DisposableList(new[] { DisableAsyncBehaviour() });
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}
}
