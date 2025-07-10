using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class StatusIndicatorControlTest : BMSTestCaseWithFactory
	{
		#region Helper Methods

		void AssertPrePostConstraintImages(Tuple<ProcessTask, Image>[] imagesToTest, ConstrainedSchematicTestConfig config, int timeIndex)
		{
			foreach (var test in imagesToTest)
			{
				var section = config.BufferSection;
				section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
				section.SectionConfiguration.CellsPerSubsection = 13;
				section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;

				var sectionViewModel = BMSTestHelper.CreateViewModel(section);

				var preConstraintPKs = section.AllComponents.SelectMany(b => b.GetBuffers(ConstraintStatus.PreConstraint)).Select(b => b.PK).ToList();
				var postConstraintPKs = section.AllComponents.SelectMany(b => b.GetBuffers(ConstraintStatus.PostConstraint)).Select(b => b.PK).ToList();

				sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, new[] { test.Item1 });

				var cell = sectionViewModel.ComponentGrid.CardCells.FirstOrDefault(c => c.TimeIndex == timeIndex);
				var cardContent = sectionViewModel.ComponentGrid.CardAllocationMap.CardsByCell_ForTest[cell].Single();
				var parent = new ControlCustomisationViewModel(Factory.NewWithValidTestData<BMControlCustomisation>(), sectionViewModel, cardContent, cell, test.Item1);

				Factory.Save();

				using (var control = new StatusIndicatorControl(parent, BMBoardSectionOrientation.Horizontal))
				{
					parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
					AssertEquals(true, control.Visible);
					AssertEquals($"Should show the correct image for {test.Item1.P9_Description}", true, control.ShownImages.Contains(test.Item2));

					foreach (var c in control.Controls)
					{
						AssertEquals("Image should be stretched on any DPI scale", PictureBoxSizeMode.StretchImage, ((OptimisticPictureBox)c).SizeMode);
					}
				}
			}
		}

		void TestStatus_CCRResourceOnNextTask(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(0); // Primary Zone 3, Pre Zone 3, Post Zone 3

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow, config.CCR.GS_Code, 60);
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 0);
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "task1");
			var task2 = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "task2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "task3");

			AssertEquals(workflow.FH_FC_CurrentComponent, config.Buffer.PK);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(0); // Primary Zone 3, Pre Zone 3, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 0);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3); // Primary Zone 3, Pre Zone 2, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 3);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5); // Primary Zone 2, Pre Zone 1, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 5);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-8); // Primary Zone 1, Pre Zone 0, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 8);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-9); // Primary Zone 1, Pre Zone 0, Post Zone 2
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 9);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-11); // Primary Zone 1, Pre Zone 0, Post Zone 1
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 11);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12); // Primary Zone 0, Pre Zone 0, Post Zone 0
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 12);
		}

		void TestStatus_Preconstraint(bool showChildComponentZones)
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferAndReleaseSchedulerBoard.MB_Name = "Are Board";
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;

			var ccrResource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			ccrResource1.DesignateAsCCR(config.Buffer);
			var nonCCRResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var ccrResource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			ccrResource2.DesignateAsCCR(config.Buffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task1 = CreateTask(workflow, ccrResource1.GS_Code, 60);
			var task2 = CreateTask(workflow, nonCCRResource.GS_Code, 60);
			var task3 = CreateTask(workflow, ccrResource2.GS_Code, 60);

			var section = CreateBoardSection(config.Buffer);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var cardContent = new TaskCardContent(task2, sectionViewModel);

			var preConstraintPKs = section.Component.GetBuffers(ConstraintStatus.PreConstraint).Select(b => b.PK).ToList();
			var postConstraintPKs = section.Component.GetBuffers(ConstraintStatus.PostConstraint).Select(b => b.PK).ToList();
			var cell = new CellContent(0, 0, CellContentType.Cards, section.SectionConfiguration, preConstraintPKs, postConstraintPKs);
			cell.Zone = 3;
			cell.SubComponentZones.Add(config.PreConstraintBuffer.PK, 3);
			cell.SubComponentZones.Add(config.PostConstraintBuffer.PK, 3);
			var parent = new ControlCustomisationViewModel(Factory.New<BMControlCustomisation>(), sectionViewModel, cardContent, cell, task2);

			Factory.Save();

			using (var control = new StatusIndicatorControl(parent, BMBoardSectionOrientation.Horizontal))
			{
				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
				AssertEquals(true, control.Visible);
				AssertEquals(2, control.ShownImages.Count);
				AssertEquals(StatusIndicatorControl.WorkingStatusImage, control.ShownImages.ElementAt(0));
				AssertEquals(StatusIndicatorControl.CCRPreconstraintTaskImage, control.ShownImages.ElementAt(1));
			}
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk_NoPostConstraint(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;

			config.Buffer.ChildComponents.Delete(config.PostConstraintBuffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "task1");
			var task2 = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "task2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "task3");

			AssertEquals(workflow.FH_FC_CurrentComponent, config.Buffer.PK);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(0); // Primary Zone 3, Pre Zone 3, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 0);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3); // Primary Zone 3, Pre Zone 2, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 3);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5); // Primary Zone 2, Pre Zone 1, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 5);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-8); // Primary Zone 1, Pre Zone 0, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 8);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-9); // Primary Zone 1, Pre Zone 0, Post Zone 2
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 9);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-11); // Primary Zone 1, Pre Zone 0, Post Zone 1
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 11);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12); // Primary Zone 0, Pre Zone 0, Post Zone 0
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 12);
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPreConstraints(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;
			var preConstraint2 = BMSTestHelper.CreateSubBuffer(config.Buffer, "Pre-Constraint 2", timespanMinutes: 48 * 60);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "task1");
			var task2 = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "task2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "task3");

			Factory.Save();

			AssertEquals(workflow.FH_FC_CurrentComponent, config.Buffer.PK);

			var preConstraints = new Dictionary<ZGuid, int>();
			preConstraints.Add(config.PreConstraintBuffer.PK, 3);
			preConstraints.Add(preConstraint2.PK, 3);
			var postConstraints = new Dictionary<ZGuid, int>();
			postConstraints.Add(config.PostConstraintBuffer.PK, 3);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(0); // Primary Zone 3, Pre 1 Zone 3, Pre 2 Zone 3, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 0);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3); // Primary Zone 3, Pre 1 Zone 2, Pre 2 Zone 2, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 3);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5); // Primary Zone 2, Pre 1 Zone 1, Pre 2 Zone 1, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 5);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-8); // Primary Zone 1, Pre 1 Zone 0, Pre 2 Zone 0, Post Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 8);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-10); // Primary Zone 1, Pre 1 Zone 0, Pre 2 Zone 0, Post Zone 1
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 10);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12); // Primary Zone 0, Pre 1 Zone 0, Pre 2 Zone 0, Post Zone 0
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 12);
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPostConstraints(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;
			var postConstraint2 = BMSTestHelper.CreateSubBuffer(config.Buffer, "Post-Constraint 2", timespanMinutes: 24 * 60, offsetMinutes: 64 * 60);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "task1");
			var task2 = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "task2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "task3");

			Factory.Save();

			AssertEquals(workflow.FH_FC_CurrentComponent, config.Buffer.PK);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(0); // Primary Zone 3, Pre Zone 3, Post 1 Zone 3, Post 2 Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 0);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3); // Primary Zone 3, Pre Zone 2, Post 1 Zone 3, Post 2 Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 3);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5); // Primary Zone 2, Pre Zone 1, Post 1 Zone 3, Post 2 Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 5);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-8); // Primary Zone 1, Pre Zone 0, Post 1 Zone 3, Post 2 Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 8);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-9); // Primary Zone 1, Pre Zone 0, Post 1 Zone 2, Post 2 Zone 2
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintTaskImage)
			}, config, timeIndex: 9);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-10); // Primary Zone 1, Pre Zone 0, Post 1 Zone 2, Post 2 Zone 1
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 10);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-11); // Primary Zone 1, Pre Zone 0, Post 1 Zone 1, Post 2 Zone 0
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 11);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12); // Primary Zone 0, Pre Zone 0, Post 1 Zone 0, Post 2 Zone 0
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(task1, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage),
				new Tuple<ProcessTask, Image>(task3, StatusIndicatorControl.CCRPostconstraintAtRiskTaskImage)
			}, config, timeIndex: 12);
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPreConstraints_EnsureCorrectConstraintConsidered(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;
			var additionalBuffer = BMSTestHelper.CreateBuffer(config.System, "additional");
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Pre-Constraint Additional", timespanMinutes: 48 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(additionalBuffer, "Constraint Additional", offsetMinutes: 48 * 60);
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Post-Constraint Additional", timespanMinutes: 48 * 60, offsetMinutes: 48 * 60);
			BMSTestHelper.CreateAdditionalComponent(config.BufferSection, additionalBuffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var taskPre = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "taskPre");
			var taskCon = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "taskCon");
			var taskPost = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "taskPost");

			Factory.Save();

			AssertEquals("The workflow should be in the primary buffer", workflow.FH_FC_CurrentComponent, config.Buffer.PK);
			AssertNotEquals("The workflow should not be in the additional buffer", workflow.FH_FC_CurrentComponent, additionalBuffer.PK);

			// Place the workflow in an area where it SHOULD NOT be at pre constraint risk

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-4); // Primary Zone 2, Pre Zone 2, Pre Additional Zone 1, Post Zone 3, Post Additional Zone 3
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(taskPre, StatusIndicatorControl.CCRPreconstraintTaskImage), // pre con task's hourglass should NOT be Risky Red
				new Tuple<ProcessTask, Image>(taskPost, StatusIndicatorControl.CCRPostconstraintTaskImage) // post con task's hourglass should definitely be Basic Black
			}, config, timeIndex: 4);
		}

		void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPostConstraints_EnsureCorrectConstraintConsidered(bool showChildComponentZones)
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ShowChildComponentZones = showChildComponentZones;
			var additionalBuffer = BMSTestHelper.CreateBuffer(config.System, "additional");
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Pre-Constraint Additional", timespanMinutes: 48 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(additionalBuffer, "Constraint Additional", offsetMinutes: 48 * 60);
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Post-Constraint Additional", timespanMinutes: 48 * 60, offsetMinutes: 48 * 60);
			BMSTestHelper.CreateAdditionalComponent(config.BufferSection, additionalBuffer);

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "workflow", config.Buffer);
			var taskPre = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, description: "taskPre");
			var taskCon = CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 2, description: "taskCon");
			var taskPost = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 3, description: "taskPost");

			Factory.Save();

			AssertEquals("The workflow should be in the primary buffer", workflow.FH_FC_CurrentComponent, config.Buffer.PK);
			AssertNotEquals("The workflow should not be in the additional buffer", workflow.FH_FC_CurrentComponent, additionalBuffer.PK);

			// Place the workflow in an area where it SHOULD NOT be at post constraint risk

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-10); // Primary Zone 1, Pre Zone 0, Pre Additional Zone 0, Post Zone 2 Post Additional Zone 1
			Factory.Save();

			AssertPrePostConstraintImages(new Tuple<ProcessTask, Image>[]
			{
				new Tuple<ProcessTask, Image>(taskPre, StatusIndicatorControl.CCRPreconstraintAtRiskTaskImage), // pre con task's hourglass should definitely be Risky Red
				new Tuple<ProcessTask, Image>(taskPost, StatusIndicatorControl.CCRPostconstraintTaskImage) // post con task's hourglass should NOT be Risky Red
			}, config, timeIndex: 10);
		}

		#endregion

		#region Hide Child Component Zones

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask(showChildComponentZones: false);
		}

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk(showChildComponentZones: false);
		}

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_Preconstraint_ChildComponentsHidden()
		{
			TestStatus_Preconstraint(showChildComponentZones: false);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_NoPostConstraint_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_NoPostConstraint(showChildComponentZones: false);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPreConstraints_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoPreConstraints(showChildComponentZones: false);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPostConstraints_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoPostConstraints(showChildComponentZones: false);
		}

		[TestDate(2017, 12, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPreConstraints_EnsureCorrectConstraintConsidered_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPreConstraints_EnsureCorrectConstraintConsidered(showChildComponentZones: false);
		}

		[TestDate(2017, 12, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPostConstraints_EnsureCorrectConstraintConsidered_ChildComponentsHidden()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPostConstraints_EnsureCorrectConstraintConsidered(showChildComponentZones: false);
		}

		#endregion

		#region Show Child Component Zones

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask(showChildComponentZones: true);
		}

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk(showChildComponentZones: true);
		}

		[TestDate(2017, 6, 19, 10, 0, 0)]
		public void TestStatus_Preconstraint_ChildComponentsShown()
		{
			TestStatus_Preconstraint(showChildComponentZones: true);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_NoPostConstraint_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_NoPostConstraint(showChildComponentZones: true);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPreConstraints_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoPreConstraints(showChildComponentZones: true);
		}

		[TestDate(2017, 8, 18, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoPostConstraints_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoPostConstraints(showChildComponentZones: true);
		}

		[TestDate(2017, 12, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPreConstraints_EnsureCorrectConstraintConsidered_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPreConstraints_EnsureCorrectConstraintConsidered(showChildComponentZones: true);
		}

		[TestDate(2017, 12, 19, 10, 0, 0)]
		public void TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPostConstraints_EnsureCorrectConstraintConsidered_ChildComponentsShown()
		{
			TestStatus_CCRResourceOnNextTask_AtRisk_TwoBuffers_TwoPostConstraints_EnsureCorrectConstraintConsidered(showChildComponentZones: true);
		}

		#endregion
	}

	class StatusIndicatorControlTransactionedTest : TransactionedTestCase
	{
		public void TestStatusUpdated()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var system = BMSTestHelper.CreateSystem(factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(factory).ProcessHeaders.AddNew();

			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 60);

			var capability = factory.NewWithValidTestData<GlbCapability>();

			factory.Save();

			var taskInAnotherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(task.PK);

			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(factory);
			var cardContent = new TaskCardContent(taskInAnotherFactory, sectionViewModel);
			var parent = new ControlCustomisationViewModel(factory.New<BMControlCustomisation>(), sectionViewModel, cardContent, new CellContent(0, 0, CellContentType.Cards));

			using (var control = new StatusIndicatorControl(parent, BMBoardSectionOrientation.Horizontal))
			{
				control.SetDataBinding(null, ".");

				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
				AssertEquals(1, control.ShownImages.Count);
				AssertEquals(StatusIndicatorControl.WorkingStatusImage, control.ShownImages.ElementAt(0));

				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Suspended);
				AssertEquals(2, control.ShownImages.Count);
				AssertEquals(StatusIndicatorControl.SuspendedStatusImage, control.ShownImages.ElementAt(0));
				AssertEquals(StatusIndicatorControl.CurrentTaskImage, control.ShownImages.ElementAt(1));

				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
				AssertEquals(0, control.ShownImages.Count);

				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Assigned);
				AssertEquals(1, control.ShownImages.Count);
				AssertEquals(StatusIndicatorControl.CurrentTaskImage, control.ShownImages.ElementAt(0));

				taskInAnotherFactory.P9_G4_RequiredCapability = capability.PK;

				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Cancelled);
				parent.UpdateStatus(ProcessTaskStatusCodeList.Codes.Assigned);
				AssertEquals(2, control.ShownImages.Count);
				AssertEquals(StatusIndicatorControl.NextAvailableResourceImage, control.ShownImages.ElementAt(0));
				AssertEquals(StatusIndicatorControl.CurrentTaskImage, control.ShownImages.ElementAt(1));
			}
		}
	}
}
