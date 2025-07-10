using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(ComponentViewFilter))]
	class ComponentViewFilterTest : BoardFilterTestCase<ComponentViewFilter>
	{
		#region Menu Item

		static void AssertMenuItemAndCards(BMComponentControl control, ZToolStripMenuItem menuItem, string text, BusinessObject contextMenuEntity, params BusinessObject[] thingsShownOnBoardSection)
		{
			ClickSubMenuItem(menuItem, text);

			var cardControls = BMSGUITestCase.FindTaskCardControls(control);
			var task1Card = cardControls.FirstOrDefault(c => c.CardContent.Identifier == thingsShownOnBoardSection[0].PK);
			var task2Card = cardControls.FirstOrDefault(c => c.CardContent.Identifier == thingsShownOnBoardSection[1].PK);
			var task3Card = cardControls.FirstOrDefault(c => c.CardContent.Identifier == thingsShownOnBoardSection[2].PK);

			BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "12 Days of Buffmas", text == "12 Days of Buffmas");
			BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Pre-Constraint", text == "   Pre-Constraint");
			BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   bufferConstraint", text == "   bufferConstraint");
			BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Post-Constraint", text == "   Post-Constraint");

			if (contextMenuEntity == null)
			{
				AssertNotNull(task1Card);
				AssertNotNull(task2Card);
				AssertNotNull(task3Card);
			}
			else
			{
				if (contextMenuEntity.PK == thingsShownOnBoardSection[0].PK)
				{
					AssertNotNull(task1Card);
				}
				else
				{
					AssertNull(task1Card);
				}

				if (contextMenuEntity.PK == thingsShownOnBoardSection[1].PK)
				{
					AssertNotNull(task2Card);
				}
				else
				{
					AssertNull(task2Card);
				}

				if (contextMenuEntity.PK == thingsShownOnBoardSection[2].PK)
				{
					AssertNotNull(task3Card);
				}
				else
				{
					AssertNull(task3Card);
				}
			}
		}

		public void TestApplySubMenuItemToTasks_HideAndShowTasks()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			config.Buffer.FC_Name = "12 Days of Buffmas";

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 60, sequence: 10);
			var task2 = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, 60, sequence: 20);
			var task3 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 60, sequence: 30);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();

				AssertMenuItemAndCards(control, menuItem, "12 Days of Buffmas", null, task1, task2, task3);
				AssertMenuItemAndCards(control, menuItem, "   Pre-Constraint", task1, task1, task2, task3);
				AssertMenuItemAndCards(control, menuItem, "   bufferConstraint", task2, task1, task2, task3);
				AssertMenuItemAndCards(control, menuItem, "   Post-Constraint", task3, task1, task2, task3);
			}
		}

		public void TestApplySubMenuItemToWorkflows_HideAndShowWorkflows()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			config.Buffer.FC_Name = "12 Days of Buffmas";

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 10, description: "task11");
			BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60, sequence: 20, description: "task12");
			BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 30, description: "task13");

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "21");
			BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60, sequence: 20, description: "task22");
			BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60, sequence: 30, description: "task23");

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestHelper.CreateTask(workflow3, config.NonCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "task31");
			BMSTestHelper.CreateTask(workflow3, config.CCR.GS_Code, 60, sequence: 20, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, description: "task32");
			BMSTestHelper.CreateTask(workflow3, config.NonCCR1.GS_Code, 60, sequence: 30, description: "task33");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();

				var cardControls = BMSGUITestCase.FindTaskCardControls(control);
				AssertNotNull(cardControls.FirstOrDefault(c => c.CardContent.Identifier == workflow1.PK));
				AssertNotNull(cardControls.FirstOrDefault(c => c.CardContent.Identifier == workflow2.PK));
				AssertNotNull(cardControls.FirstOrDefault(c => c.CardContent.Identifier == workflow3.PK));

				AssertMenuItemAndCards(control, menuItem, "12 Days of Buffmas", null, workflow1, workflow2, workflow3);
				AssertMenuItemAndCards(control, menuItem, "   Pre-Constraint", workflow1, workflow1, workflow2, workflow3);
				AssertMenuItemAndCards(control, menuItem, "   bufferConstraint", workflow2, workflow1, workflow2, workflow3);
				AssertMenuItemAndCards(control, menuItem, "   Post-Constraint", workflow3, workflow1, workflow2, workflow3);
			}
		}

		public void TestApplySubMenuItemToJobLevelWorkflows_HideAndShowJobLevelWorkflows()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			config.Buffer.FC_Name = "12 Days of Buffmas";

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task11 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 10);
			var task21 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60, sequence: 20);
			var task31 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 30);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow2 = jobHeader2.ProcessHeaders[0];
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			var task12 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task22 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60, sequence: 20);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow3 = jobHeader3.ProcessHeaders[0];
			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			var task13 = BMSTestHelper.CreateTask(workflow3, config.NonCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task23 = BMSTestHelper.CreateTask(workflow3, config.CCR.GS_Code, 60, sequence: 20, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task33 = BMSTestHelper.CreateTask(workflow3, config.NonCCR1.GS_Code, 60, sequence: 30);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();

				AssertMenuItemAndCards(control, menuItem, "12 Days of Buffmas", null, jobHeader1, jobHeader2, jobHeader3);
				AssertMenuItemAndCards(control, menuItem, "   Pre-Constraint", jobHeader1, jobHeader1, jobHeader2, jobHeader3);
				AssertMenuItemAndCards(control, menuItem, "   bufferConstraint", jobHeader2, jobHeader1, jobHeader2, jobHeader3);
				AssertMenuItemAndCards(control, menuItem, "   Post-Constraint", jobHeader3, jobHeader1, jobHeader2, jobHeader3);
			}
		}

		#region Constraint Lines

		#region Two Buffers Same Timespans

		public void TestChangingComponent_ShouldUpdateConstraintLine_Up()
		{
			ChangingComponent_ShouldUpdateConstraintLine(FlowDirectionList.Codes.Up);
		}

		public void TestChangingComponent_ShouldUpdateConstraintLine_Down()
		{
			ChangingComponent_ShouldUpdateConstraintLine(FlowDirectionList.Codes.Down);
		}

		public void TestChangingComponent_ShouldUpdateConstraintLine_Left()
		{
			ChangingComponent_ShouldUpdateConstraintLine(FlowDirectionList.Codes.Left);
		}

		public void TestChangingComponent_ShouldUpdateConstraintLine_Right()
		{
			ChangingComponent_ShouldUpdateConstraintLine(FlowDirectionList.Codes.Right);
		}

		void ChangingComponent_ShouldUpdateConstraintLine(ZString flowDirection)
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var addBuffer = BMSTestHelper.CreateBuffer(config.System, "addBuffer");
			var addPreConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Pre", timespanMinutes: 48 * 60, offsetMinutes: 0);
			var addConstraint = BMSTestHelper.CreateConstraint(addBuffer, "Con", offsetMinutes: 48 * 60);
			var addPostConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Post", timespanMinutes: 48 * 60, offsetMinutes: 48 * 60);
			var addComponent = BMSTestHelper.CreateAdditionalComponent(section, addBuffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			section.SectionConfiguration.FlowDirection = flowDirection;
			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var primaryTimeIndex = 7;
				var additionalTimeIndex = 5;

				CombineAssertions("no filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});

				var menuItem = GetMenuItem(form, section);

				ClickSubMenuItem(menuItem, "   add Pre");
				AssertEquals(8, menuItem.DropDownItems.Count);
				CombineAssertions("Additional pre constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "   bufferConstraint");
				CombineAssertions("Primary constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "   add Post");
				CombineAssertions("Additional post constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "   Pre-Constraint");
				CombineAssertions("Primary pre-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "   addBufferCon");
				CombineAssertions("Additional constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "   Post-Constraint");
				CombineAssertions("Primary post-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "addBuffer");
				CombineAssertions("Additional buffer filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("Primary buffer filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("No filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);
				});
			}
		}

		#endregion

		#region Two Buffers Different Timespans Proportionally Same Offsets

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeading_Up()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeadings(FlowDirectionList.Codes.Up);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeading_Down()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeadings(FlowDirectionList.Codes.Down);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeading_Left()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeadings(FlowDirectionList.Codes.Left);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeading_Right()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeadings(FlowDirectionList.Codes.Right);
		}

		void ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallySameOffsets_ShouldMaintainConstraintLineAndZoneHeadings(ZString flowDirection)
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var addBuffer = BMSTestHelper.CreateBuffer(config.System, "addBuffer", timespanMinutes: 24 * 60);
			var addPreConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Pre", timespanMinutes: 16 * 60, offsetMinutes: 0);
			var addConstraint = BMSTestHelper.CreateConstraint(addBuffer, "Con", offsetMinutes: 16 * 60);
			var addPostConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Post", timespanMinutes: 8 * 60, offsetMinutes: 16 * 60);
			var addComponent = BMSTestHelper.CreateAdditionalComponent(section, addBuffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			section.SectionConfiguration.FlowDirection = flowDirection;
			Factory.Save();

			AssertEquals(flowDirection, section.SectionConfiguration.FlowDirection);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var control = form.FindAll<BMComponentControl>().Single();
				var table = control.Table;

				var primaryTimeIndex = 7;
				var additionalTimeIndex = 7;

				var zoneHeadingRow = 3;
				var preConZoneHeadingRow = 4;
				var postConZoneHeadingRow = 5;
				var addPreConZoneHeadingRow = 6;
				var addPostConZoneHeadingRow = 7;
				var nonCCRChannelRow = 8;

				CombineAssertions("no filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "no filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				var menuItem = GetMenuItem(form, section);

				ClickSubMenuItem(menuItem, "   add Pre");
				AssertEquals(8, menuItem.DropDownItems.Count);
				CombineAssertions("Additional pre constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, addPreConZoneHeadingRow, nonCCRChannelRow, "Additional pre constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Zone 0 cell (5)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (3)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor,  // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (3)
					);
				});

				ClickSubMenuItem(menuItem, "   bufferConstraint");
				CombineAssertions("Primary constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Primary constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "   add Post");
				CombineAssertions("Additional post constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, addPostConZoneHeadingRow, nonCCRChannelRow, "Additional post constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, // Zone 1 cells (1)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (9)
					);
				});

				ClickSubMenuItem(menuItem, "   Pre-Constraint");
				CombineAssertions("Primary pre-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, preConZoneHeadingRow, nonCCRChannelRow, "Primary pre-constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Zone 0 cell (5)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (3)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor,  // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (3)
					);
				});

				ClickSubMenuItem(menuItem, "   addBufferCon");
				CombineAssertions("Additional constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Additional constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "   Post-Constraint");
				CombineAssertions("Primary post-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, postConZoneHeadingRow, nonCCRChannelRow, "Primary post-constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, // Zone 1 cells (1)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (9)
					);
				});

				ClickSubMenuItem(menuItem, "addBuffer");
				CombineAssertions("Additional buffer filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Additional buffer filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("Primary buffer filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Primary buffer filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("No filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "no filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});
			}
		}

		#endregion

		#region Two Buffers Different Timespans Proportionally Different Offsets

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets_Up()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets(FlowDirectionList.Codes.Up);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets_Down()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets(FlowDirectionList.Codes.Down);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets_Left()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets(FlowDirectionList.Codes.Left);
		}

		public void TestChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets_Right()
		{
			ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets(FlowDirectionList.Codes.Right);
		}

		void ChangingComponent_TwoBuffersWithDifferentTimespans_ProportionallyDifferentOffsets(ZString flowDirection)
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var addBuffer = BMSTestHelper.CreateBuffer(config.System, "addBuffer", timespanMinutes: 24 * 60);
			var addPreConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Pre", timespanMinutes: 12 * 60, offsetMinutes: 0);
			var addConstraint = BMSTestHelper.CreateConstraint(addBuffer, "Con", offsetMinutes: 12 * 60);
			var addPostConstraintBuffer = BMSTestHelper.CreateSubBuffer(addBuffer, "add Post", timespanMinutes: 12 * 60, offsetMinutes: 12 * 60);
			var addComponent = BMSTestHelper.CreateAdditionalComponent(section, addBuffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			section.SectionConfiguration.FlowDirection = flowDirection;
			Factory.Save();

			AssertEquals(flowDirection, section.SectionConfiguration.FlowDirection);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var control = form.FindAll<BMComponentControl>().Single();
				var table = control.Table;

				var primaryTimeIndex = 7;
				var additionalTimeIndex = 5;

				var zoneHeadingRow = 3;
				var preConZoneHeadingRow = 4;
				var postConZoneHeadingRow = 5;
				var addPreConZoneHeadingRow = 6;
				var addPostConZoneHeadingRow = 7;
				var nonCCRChannelRow = 8;

				CombineAssertions("no filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "no filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				var menuItem = GetMenuItem(form, section);

				ClickSubMenuItem(menuItem, "   add Pre");
				AssertEquals(8, menuItem.DropDownItems.Count);

				CombineAssertions("Additional pre constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, addPreConZoneHeadingRow, nonCCRChannelRow, "Additional pre constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor,// Zone 0 cell (7)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (2)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (2)
					);
				});

				ClickSubMenuItem(menuItem, "   bufferConstraint");
				CombineAssertions("Primary constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Primary constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "   add Post");
				CombineAssertions("Additional post constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, addPostConZoneHeadingRow, nonCCRChannelRow, "Additional post constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (2)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (8)
					);
				});

				ClickSubMenuItem(menuItem, "   Pre-Constraint");
				CombineAssertions("Primary pre-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, preConZoneHeadingRow, nonCCRChannelRow, "Primary pre-constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Zone 0 cell (5)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (3)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor,  // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (3)
					);
				});

				ClickSubMenuItem(menuItem, "   addBufferCon");
				CombineAssertions("Additional constraint filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Additional constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "   Post-Constraint");
				CombineAssertions("Primary post-constraint filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, postConZoneHeadingRow, nonCCRChannelRow, "Primary post-constraint filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, // Zone 1 cells (1)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (9)
					);
				});

				ClickSubMenuItem(menuItem, "addBuffer");
				CombineAssertions("Additional buffer filter applied", () =>
				{
					AssertConstraintLinePresent(false, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(true, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Additional buffer filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("Primary buffer filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "Primary buffer filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});

				ClickSubMenuItem(menuItem, "buffer");
				CombineAssertions("No filter applied", () =>
				{
					AssertConstraintLinePresent(true, section, form, primaryTimeIndex);
					AssertConstraintLinePresent(false, section, form, additionalTimeIndex);

					AssertZoneHeadingColours(table, zoneHeadingRow, nonCCRChannelRow, "no filter applied", flowDirection,
						BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
						BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
						BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
						BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
				});
			}
		}

		#endregion

		#region Helper Functions

		static void AssertConstraintLinePresent(bool hasConstraintLine, BMBoardSection section, VisualBoardForm form, int timeIndex)
		{
			var viewModel = BMSTestHelper.CreateViewModel(section);
			var grid = viewModel.ComponentGrid;
			var cellsWithBorderValue = grid.Cells.Where(c => c.TimeIndex == timeIndex).ToArray();
			var control = form.FindAll<BMComponentControl>().Single();
			foreach (var cell in cellsWithBorderValue)
			{
				var panel = control.Table.GetControlFromPosition(cell.Column, cell.Row) as TaskPanel;
				if (panel != null)
				{
					if (hasConstraintLine)
					{
						AssertEquals($"There should be a constraint line at time index {timeIndex}", true, panel.Border != null && GetPanelBorderThickness(panel.Border, section.SectionConfiguration.FlowDirection) == ComponentGridHelper.BorderThickness);
					}
					else
					{
						AssertNull($"There should not be a constraint line at time index {timeIndex}", panel.Border);
					}
				}
			}
		}

		static int GetPanelBorderThickness(BorderSpec borderSpec, ZString flowDirection)
		{
			switch (flowDirection)
			{
				case FlowDirectionList.Codes.Left:
					return borderSpec.LeftThickness;

				case FlowDirectionList.Codes.Right:
					return borderSpec.RightThickness;

				case FlowDirectionList.Codes.Up:
					return borderSpec.TopThickness;

				case FlowDirectionList.Codes.Down:
					return borderSpec.BottomThickness;

				default:
					return 0;
			}
		}

		#endregion

		#endregion

		public void TestDropDownMenu_ConstrainedBuffer_CCROnlySection_NoExceptionsThrown()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var cCR2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CC2", "CCR Two");
			cCR2.DesignateAsCCR(config.Buffer);
			config.ReleaseGroup.Staff.Add(cCR2);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, cCR2.PK, overrideChannels: true);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);
				menuItem.ShowDropDown();
				AssertEquals(4, menuItem.DropDownItems.Count);

				AssertEquals("buffer", menuItem.DropDownItems[0].Text);
				AssertEquals("   Pre-Constraint", menuItem.DropDownItems[1].Text);
				AssertEquals("   bufferConstraint", menuItem.DropDownItems[2].Text);
				AssertEquals("   Post-Constraint", menuItem.DropDownItems[3].Text);

				CombineAssertions("no exceptions plz", () =>
				{
					AssertNoExceptionThrown("When clicking on the primary buffer menu item", () => ClickSubMenuItem(menuItem, "buffer"));
					AssertNoExceptionThrown("When clicking on the pre constraint menu item", () => ClickSubMenuItem(menuItem, "   Pre-Constraint"));
					AssertNoExceptionThrown("When clicking on the constraint menu item", () => ClickSubMenuItem(menuItem, "   bufferConstraint"));
					AssertNoExceptionThrown("When clicking on the post constraint menu item", () => ClickSubMenuItem(menuItem, "   Post-Constraint"));
				});
			}
		}

		public void TestDropDownMenu_ConstrainedBuffer_OrderedBySequenceNumber()
		{
			var config1 = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config1.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config1.ReleaseGroup.PK;
			config1.Buffer.FC_Name = "Buffer";
			config1.Buffer.FC_DisplaySequence = 5;
			config1.PreConstraintBuffer.FC_DisplaySequence = 6;
			config1.Constraint.FC_DisplaySequence = 7;
			config1.PostConstraintBuffer.FC_DisplaySequence = 8;

			var config2 = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "ROG");
			config2.Buffer.FC_Name = "Chamois";
			config2.Buffer.FC_DisplaySequence = 1;
			config2.PreConstraintBuffer.FC_DisplaySequence = 4;
			config2.Constraint.FC_DisplaySequence = 3;
			config2.PostConstraintBuffer.FC_DisplaySequence = 2;
			config2.CCR.GS_Code = "RCC";
			config2.NonCCR1.GS_Code = "1CN";
			config2.NonCCR2.GS_Code = "2CN";

			var additionalComponent = Factory.NewWithValidTestData<BMBoardSectionAdditionalComponent>();
			additionalComponent.BSA_FC_Component = config2.Buffer.PK;
			additionalComponent.BSA_MS_Section = section.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);

				menuItem.ShowDropDown();

				AssertEquals(8, menuItem.DropDownItems.Count);

				AssertEquals("Chamois", menuItem.DropDownItems[0].Text);
				AssertEquals("   Post-Constraint", menuItem.DropDownItems[1].Text);
				AssertEquals("   bufferConstraint", menuItem.DropDownItems[2].Text);
				AssertEquals("   Pre-Constraint", menuItem.DropDownItems[3].Text);
				AssertEquals("Buffer", menuItem.DropDownItems[4].Text);
				AssertEquals("   Pre-Constraint", menuItem.DropDownItems[5].Text);
				AssertEquals("   bufferConstraint", menuItem.DropDownItems[6].Text);
				AssertEquals("   Post-Constraint", menuItem.DropDownItems[7].Text);
			}
		}

		public void TestDropDownMenu_ReleaseSchedulerBoard()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);
			config.Buffer.FC_Name = "Buffer";
			config.Buffer.FC_DisplaySequence = 1;
			config.PreConstraintBuffer.FC_DisplaySequence = 2;
			config.Constraint.FC_DisplaySequence = 3;
			config.PostConstraintBuffer.FC_DisplaySequence = 4;

			config.Bucket.FC_Name = "Feeding Bucket";
			config.ComponentLink.FL_IsReleaseGateRuleApplied = true;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);

				menuItem.ShowDropDown();

				AssertEquals(5, menuItem.DropDownItems.Count);

				AssertEquals("Feeding Bucket", menuItem.DropDownItems[0].Text);
				AssertEquals("Buffer", menuItem.DropDownItems[1].Text);
				AssertEquals("   Pre-Constraint", menuItem.DropDownItems[2].Text);
				AssertEquals("   bufferConstraint", menuItem.DropDownItems[3].Text);
				AssertEquals("   Post-Constraint", menuItem.DropDownItems[4].Text);
			}
		}

		public void TestDropDownMenu_BucketBoard()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var menuItem = GetMenuItem(form, section);

				menuItem.ShowDropDown();

				AssertEquals(1, menuItem.DropDownItems.Count);
				AssertEquals("bucket", menuItem.DropDownItems[0].Text);
			}
		}

		#endregion

		#region BoardFilterTestCase Overrides

		public override void TestFilterName()
		{
			AssertEquals("Component View Filter", GetFilter().FilterName);
		}

		public override void TestAllowMultiple()
		{
			AssertEquals(false, GetFilter().AllowMultiple);
		}

		public override void TestEquals()
		{
			AssertEquals("Any instance should equal another", GetFilter(), GetFilter());
		}

		public override void TestWhenRemovedThroughFilterManager_ShouldRestoreVisualState()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (DisableAsyncBehaviour())
			using (var form = new ZForm { Width = 800, Height = 800 })
			using (var control = new BMComponentControl(section.SectionConfiguration, viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var menuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Component View Filter");
				menuItem.ShowDropDown();

				AssertEquals(4, menuItem.DropDownItems.Count);

				AssertEquals("buffer", menuItem.DropDownItems[0].Text);
				AssertEquals("   Pre-Constraint", menuItem.DropDownItems[1].Text);
				AssertEquals("   bufferConstraint", menuItem.DropDownItems[2].Text);
				AssertEquals("   Post-Constraint", menuItem.DropDownItems[3].Text);

				AssertEquals("Show only items whose Current Component is buffer", menuItem.DropDownItems[0].ToolTipText);
				AssertEquals("Show only items whose penetrated sub-components include Pre-Constraint", menuItem.DropDownItems[1].ToolTipText);
				AssertEquals("Show only items whose penetrated sub-components include bufferConstraint", menuItem.DropDownItems[2].ToolTipText);
				AssertEquals("Show only items whose penetrated sub-components include Post-Constraint", menuItem.DropDownItems[3].ToolTipText);

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				menuItem.DropDownItems[0].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, true);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));

				var filter = viewModel.FilterManager.AppliedAndChildFilters.OfType<ComponentViewFilter>().Single();
				AssertEquals("Showing items in component: buffer", filter.FilterName);

				menuItem.DropDownItems[1].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, true);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));
				AssertEquals("Showing items in component: Pre-Constraint", filter.FilterName);

				menuItem.DropDownItems[2].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, true);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));
				AssertEquals("Showing items in component: bufferConstraint", filter.FilterName);

				menuItem.DropDownItems[3].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, true);

				AssertEquals(true, viewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));
				AssertEquals("Showing items in component: Post-Constraint", filter.FilterName);

				menuItem.DropDownItems[3].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				AssertEquals(false, viewModel.FilterManager.IsApplied(typeof(ComponentViewFilter)));

				menuItem.DropDownItems[2].PerformClick();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, true);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);

				viewModel.FilterManager.Clear();
				menuItem.HideDropDown();
				menuItem.ShowDropDown();

				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 0, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 1, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 2, false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, 3, false);
			}
		}

		protected override ComponentViewFilter GetFilter()
		{
			return new ComponentViewFilter();
		}

		#endregion

		#region Filter

		public void TestFilter_ShouldBePresentOnBucketAndBufferSections()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var bucketSection = BMSTestHelper.CreateBoardSection(config.Bucket, board, row: 1);
			var releaseSchedulerSection = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			var constrainedModeBufferSection = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 2);
			var nonConstrainedModeBufferSection = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 3);

			constrainedModeBufferSection.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();

				AssertMenuItemPresent(form, bucketSection, true);
				AssertMenuItemPresent(form, releaseSchedulerSection, true);
				AssertMenuItemPresent(form, constrainedModeBufferSection, true);
				AssertMenuItemPresent(form, nonConstrainedModeBufferSection, true);
			}
		}

		#endregion

		#region Pre-constraint/post-constraint headings

		void Apply_ShouldSwitchPreAndPostConstraintHeadings(ZString flowDirection)
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = flowDirection;
			section.SectionConfiguration.CellsPerSubsection = 13;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();
				var table = control.Table;

				AssertZoneHeadingColours(table, 3, 6, "default view, default zone headings", flowDirection,
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);

				AssertZoneHeadingVisible(true, table, 3, section.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 4, section.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				ClickSubMenuItem(menuItem, "   Pre-Constraint");

				AssertZoneHeadingColours(table, 4, 6, "CCR channel background colours - in Pre-Constraint view, pre-constraint zone headings", flowDirection,
					BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Zone 0 cells (5)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (3)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (3)
					);

				AssertZoneHeadingVisible(false, table, 3, section.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 4, section.SectionConfiguration.FlowDirection, "Pre constraint header should be visible");
				AssertZoneHeadingVisible(false, table, 5, section.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				ClickSubMenuItem(menuItem, "   bufferConstraint");

				AssertZoneHeadingColours(table, 3, 6, "CCR channel background colours - in Constraint view, default zone headings", flowDirection,
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);

				AssertZoneHeadingVisible(true, table, 3, section.SectionConfiguration.FlowDirection, "Zone header should be visible");
				AssertZoneHeadingVisible(false, table, 4, section.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				ClickSubMenuItem(menuItem, "   Post-Constraint");

				AssertZoneHeadingColours(table, 5, 6, "CCR channel background colours - in Post-Constraint view, post-constraint zone headings", flowDirection,
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, // Zone 1 cell (1)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (2)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, // Zone 3 cells (9)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (9)
					);

				AssertZoneHeadingVisible(false, table, 3, section.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 4, section.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 5, section.SectionConfiguration.FlowDirection, "Post constraint header should be visible");

				ClickSubMenuItem(menuItem, "   Post-Constraint");

				AssertZoneHeadingColours(table, 3, 6, "CCR channel background colours - in Non-Constrained view, default zone headings", flowDirection,
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);

				AssertZoneHeadingVisible(true, table, 3, section.SectionConfiguration.FlowDirection, "Zone header should be visible");
				AssertZoneHeadingVisible(false, table, 4, section.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");
			}
		}

		static void AssertZoneHeadingColours(TableLayoutPanel table, int zoneHeadingRow, int nonCCRRow, string message, string flowDirection, params Color[] zoneColours)
		{
			bool isVertical = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Down);
			int start = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left) ? 1 : 13;
			int finish = flowDirection.In(FlowDirectionList.Codes.Down, FlowDirectionList.Codes.Right) ? 1 : 13;

			var range = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left) ? Enumerable.Range(1, 13) : Enumerable.Range(1, 13).Reverse();
			foreach (var i in range)
			{
				var zoneHeadingCell = isVertical ? table.GetControlFromPosition(zoneHeadingRow, i) : table.GetControlFromPosition(i, zoneHeadingRow);
				var nonCCRCardCell = isVertical ? table.GetControlFromPosition(nonCCRRow, i) : table.GetControlFromPosition(i, nonCCRRow);
				var index = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left) ? i - start : start - i;

				AssertEquals("Zone heading cell", zoneColours[index], zoneHeadingCell.BackColor);
				AssertColorEquals(string.Format("Background colour of zone heading cell at row " + i), zoneColours[index], zoneHeadingCell.BackColor);

				AssertEquals("Non-CCR cell", zoneColours[index], nonCCRCardCell.BackColor);
				AssertColorEquals(string.Format("Background colour of non-CCR cell at row " + i), zoneColours[index], nonCCRCardCell.BackColor);
			}
		}

		public void TestApply_ShouldSwitchPreAndPostConstraintHeadings_Up()
		{
			Apply_ShouldSwitchPreAndPostConstraintHeadings(FlowDirectionList.Codes.Up);
		}

		public void TestApply_ShouldSwitchPreAndPostConstraintHeadings_Left()
		{
			Apply_ShouldSwitchPreAndPostConstraintHeadings(FlowDirectionList.Codes.Left);
		}

		public void TestApply_ShouldSwitchPreAndPostConstraintHeadings_Down()
		{
			Apply_ShouldSwitchPreAndPostConstraintHeadings(FlowDirectionList.Codes.Down);
		}

		public void TestApply_ShouldSwitchPreAndPostConstraintHeadings_Right()
		{
			Apply_ShouldSwitchPreAndPostConstraintHeadings(FlowDirectionList.Codes.Right);
		}

		#endregion

		public void TestSlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged_Up()
		{
			SlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged(FlowDirectionList.Codes.Up);
		}

		public void TestSlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged_Down()
		{
			SlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged(FlowDirectionList.Codes.Down);
		}

		public void TestSlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged_Left()
		{
			SlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged(FlowDirectionList.Codes.Left);
		}

		public void TestSlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged_Right()
		{
			SlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged(FlowDirectionList.Codes.Right);
		}

		void SlideShowProgression_WhenChangingConstraintModes_ZoneHeadingsShouldRemainUnchanged(ZString flowDirection)
		{
			var config1 = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var config2 = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "RPK");
			config2.CCR.GS_Code = "FSN";
			config2.CCR.GS_FullName = "Frank Sinatra";
			config2.NonCCR1.GS_Code = "DEM";
			config2.NonCCR1.GS_FullName = "Dean Martin";
			config2.NonCCR2.GS_Code = "SAD";
			config2.NonCCR2.GS_FullName = "Sammy Davis Jr.";

			var section1 = BMSTestHelper.CreateBoardSection(config1.Buffer);
			section1.SectionConfiguration.ReleaseGroupPK = config1.ReleaseGroup.PK;
			section1.SectionConfiguration.FlowDirection = flowDirection;
			section1.SectionConfiguration.CellsPerSubsection = 13;
			section1.SectionConfiguration.OverrideChannels = true;

			var section2 = BMSTestHelper.CreateBoardSection(config2.Buffer);
			section2.SectionConfiguration.ReleaseGroupPK = config2.ReleaseGroup.PK;
			section2.SectionConfiguration.FlowDirection = flowDirection;
			section2.SectionConfiguration.CellsPerSubsection = 13;
			section2.SectionConfiguration.OverrideChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, config1.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, config1.NonCCR1.PK, overrideChannels: true);

			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, config2.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, config2.NonCCR1.PK, overrideChannels: true);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, section1.Board, section2.Board);

			BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var numberOfTasks = BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.Value + 10;
			BMSTestHelper.CreateWorkflows(config1.Buffer, numberOfWorkflows: 1, numberOfTasksPerWorkflow: numberOfTasks, staff: config1.CCR);
			BMSTestHelper.CreateWorkflows(config2.Buffer, numberOfWorkflows: 1, numberOfTasksPerWorkflow: 10, staff: config2.CCR);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(slideshow)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().Single();
				var table = control.Table;

				AssertZoneHeadingVisible(true, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				var menuItem = GetMenuItem(form, section1);
				ClickSubMenuItem(menuItem, "   Pre-Constraint");

				AssertZoneHeadingVisible(false, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should now be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				VisualBoardFormTest.MoveNext_ForTest(form, triggeredByButtonClick: true);
				VisualBoardFormTest.MovePrevious_ForTest(form);
				control = form.FindAll<BMComponentControl>().Single();
				table = control.Table;

				AssertZoneHeadingVisible(false, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should STILL be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				menuItem = GetMenuItem(form, section1);
				ClickSubMenuItem(menuItem, "   Post-Constraint");

				AssertZoneHeadingVisible(false, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should now be visible");

				VisualBoardFormTest.MoveNext_ForTest(form, triggeredByButtonClick: true);
				VisualBoardFormTest.MovePrevious_ForTest(form);
				control = form.FindAll<BMComponentControl>().Single();
				table = control.Table;

				AssertZoneHeadingVisible(false, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(true, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should STILL be visible");

				menuItem = GetMenuItem(form, section1);
				ClickSubMenuItem(menuItem, "   bufferConstraint");

				AssertZoneHeadingVisible(true, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should STILL be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				VisualBoardFormTest.MoveNext_ForTest(form, triggeredByButtonClick: true);
				VisualBoardFormTest.MovePrevious_ForTest(form);
				control = form.FindAll<BMComponentControl>().Single();
				table = control.Table;

				AssertZoneHeadingVisible(true, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should STILL be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				menuItem = GetMenuItem(form, section1);
				ClickSubMenuItem(menuItem, "   bufferConstraint");

				AssertZoneHeadingVisible(true, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should STILL be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");

				VisualBoardFormTest.MoveNext_ForTest(form, triggeredByButtonClick: true);
				VisualBoardFormTest.MovePrevious_ForTest(form);
				control = form.FindAll<BMComponentControl>().Single();
				table = control.Table;

				AssertZoneHeadingVisible(true, table, 3, section1.SectionConfiguration.FlowDirection, "Zone header should STILL be visible");
				AssertZoneHeadingVisible(false, table, 4, section1.SectionConfiguration.FlowDirection, "Pre constraint header should NOT be visible");
				AssertZoneHeadingVisible(false, table, 5, section1.SectionConfiguration.FlowDirection, "Post constraint header should NOT be visible");
			}
		}

		static void AssertZoneHeadingVisible(bool isVisible, TableLayoutPanel table, int zoneHeadingRow, string flowDirection, string message)
		{
			int zoneHeaderSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
			bool isVertical = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Down);

			var range = flowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left) ? Enumerable.Range(1, 13) : Enumerable.Range(1, 13).Reverse();
			foreach (var i in range)
			{
				var zoneHeadingCell = isVertical ? table.GetControlFromPosition(zoneHeadingRow, i) : table.GetControlFromPosition(i, zoneHeadingRow);
				var dimension = isVertical ? zoneHeadingCell.Width : zoneHeadingCell.Height;
				if (isVisible)
				{
					AssertEquals(message, zoneHeaderSize, dimension);
				}
				else
				{
					AssertNotEquals(message, zoneHeaderSize, dimension);
				}
			}
		}

		#region Background Colours

		public void TestApply_ShouldSetBackgroundColours()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();
				var grid = control.ViewModel.ComponentGrid;

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, // Post-constraint cells
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);

				ClickSubMenuItem(menuItem, "   Pre-Constraint");

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - in Pre-Constraint view, CCR channels show the same background colours as the default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, // Post-constraint cells
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - in Pre-Constraint view, post-constraint cells look like zone 0 for pre-constraint",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Post-constraint cells
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				ClickSubMenuItem(menuItem, "   bufferConstraint");

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - in Constraint view, CCR channels show the same background colours as the default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, // Post-constraint cells
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - in Constraint view, non-CCR channels show the same background colours as the default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor); // Zone 3 cells (4)

				ClickSubMenuItem(menuItem, "   Post-Constraint");
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Post-Constraint", true);

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - in Post-Constraint view, CCR channels show the same background colours as the default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, // Post-constraint cells
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - in Post-Constraint view, pre-constraint cells look like zone 3 for post-constraint",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, // Post-constraint cells
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				ClickSubMenuItem(menuItem, "   Post-Constraint");
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Post-Constraint", false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   Pre-Constraint", false);
				BMSGUITestCase.AssertSubMenuItemChecked(menuItem, "   bufferConstraint", false);

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - in non-constrained view, CCR channels show the same background colours as the default view",
					BMConstants.Zone0DefaultColor, // Zone 0 cell
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, // Post-constraint cells
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - in non-constrained view, cells have the same background colours as the parent component zone headings",
					BMConstants.Zone0DefaultColor, // Zone 0 cell (1)
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, // Zone 1 cells (4)
					BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, // Zone 2 cells (4)
					BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Zone 3 cells (4)
					);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestApply_ShouldSetBackgroundColours_ConsideringCellFade()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK, overrideChannels: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseDateTime: ZDateTime.UtcNow.AddDays(-6));
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer, releaseDateTime: ZDateTime.UtcNow.AddDays(-10));

			var task1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60, description: "tarrasque1");
			var task2 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, 60, description: "tarrasque2");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var menuItem = GetMenuItem(form, section);
				var control = form.FindAll<BMComponentControl>().Single();
				var grid = control.ViewModel.ComponentGrid;

				var task1Card = BMSGUITestCase.FindTaskCardControl(control, task1);
				var task2Card = BMSGUITestCase.FindTaskCardControl(control, task2);

				AssertEquals(7, task1Card.Cell.Row);
				AssertEquals(2, task1Card.Cell.Column);

				AssertEquals(3, task2Card.Cell.Row);
				AssertEquals(6, task2Card.Cell.Column);

				AssertNotNull(task1Card.Cell.BackgroundFadeColor);
				AssertNotNull(task2Card.Cell.BackgroundFadeColor);

				ClickSubMenuItem(menuItem, "   Pre-Constraint");

				var fadedZone0Colour = BMConstants.Zone0DefaultColor.FadeTowardsWhite();
				var fadedZone1Colour = BMConstants.Zone1DefaultColor.FadeTowardsWhite();
				var fadedZone2Colour = BMConstants.Zone2DefaultColor.FadeTowardsWhite();
				var fadedZone3Colour = BMConstants.Zone3DefaultColor.FadeTowardsWhite();

				AssertCellColumnColours(grid, config.CCR, "CCR channel background colours - in Pre-Constraint view, CCR channels show the same background colours as the default view, with a fade towards white above row 7",
					fadedZone0Colour, // Zone 0 cell
					fadedZone1Colour, fadedZone1Colour, fadedZone1Colour, fadedZone2Colour, // Post-constraint cells
					fadedZone2Colour, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);

				AssertCellColumnColours(grid, config.NonCCR1, "Non-CCR channel background colours - in Pre-Constraint view, post-constraint cells look like zone 0 for pre-constraint, with a fade towards white above row 4",
					fadedZone0Colour, // Zone 0 cell
					fadedZone0Colour, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, BMConstants.Zone0DefaultColor, // Post-constraint cells
					BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone1DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone2DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor, BMConstants.Zone3DefaultColor // Pre-constraint cells
					);
			}
		}

		#endregion

		#region Implementation

		static void AssertMenuItemPresent(VisualBoardForm form, BMBoardSection section, bool menuItemPresent)
		{
			var menuItem = GetMenuItem(form, section);

			if (menuItemPresent)
			{
				AssertNotNull(menuItem);
			}
			else
			{
				AssertNull(menuItem);
			}
		}

		static void AssertCellColumnColours(ComponentGrid grid, GlbStaff resourceChannel, string message, params Color[] columnColours)
		{
			var cells = grid.CardCells.Where(c => c.Channel.EntityPK == resourceChannel.PK).ToArray();

			AssertEquals("Please specify the same number of colours as there are cells in column for channel " + resourceChannel.GS_Code, columnColours.Length, cells.Length);

			CombineAssertions(message, () =>
			{
				for (var i = 0; i < cells.Length; i++)
				{
					var cell = cells[i];

					AssertColorEquals(string.Format("Background colour of cell at row " + cell.Row), columnColours[i], cell.BackColor.Value);
				}
			});
		}

		static ZToolStripMenuItem GetMenuItem(VisualBoardForm form, BMBoardSection section)
		{
			return BMSGUITestCase.GetMenuItem(form, section, "Component View Filter");
		}

		static void ClickSubMenuItem(ZToolStripMenuItem menuItem, string text)
		{
			menuItem.ShowDropDown();
			menuItem.DropDownItems.Cast<ZToolStripMenuItem>().Single(i => i.Text == text).PerformClick();
			Application.DoEvents();
		}

		#endregion
	}

	[TestedType(typeof(ComponentViewFilter))]
	class ComponentViewFilterApplicatorTest_NonConstrained : TaskVisibilityFilterApplicatorTestCase<ComponentViewFilter>
	{
		protected override ComponentViewFilter GetFilter()
		{
			var filter = new ComponentViewFilter();
			var component = Factory.New<BMComponent>();
			filter.CurrentlySelectedOption = component;
			return filter;
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield return new KeyValuePair<string, int>(ProcessTasksSchema.Constants.TableName, 1);
			yield return new KeyValuePair<string, int>(BMComponentResourceLinkSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(BMComponentSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(GlbStaffSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 1);
		}

		#region Redundant tests

		public override void TestIsApplicable()
		{
			Assert(true);
		}

		public override void TestApply_DbHits()
		{
			Assert(true);
		}

		#endregion
	}

	[TestedType(typeof(ComponentViewFilter))]
	class ComponentViewFilterApplicatorTest_PostConstraint : TaskVisibilityFilterApplicatorTestCase<ComponentViewFilter>
	{
		protected override ComponentViewFilter GetFilter()
		{
			var filter = new ComponentViewFilter();
			var component = Factory.New<BMComponent>();
			filter.CurrentlySelectedOption = component;
			return filter;
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield return new KeyValuePair<string, int>(ProcessTasksSchema.Constants.TableName, 1);
			yield return new KeyValuePair<string, int>(BMComponentResourceLinkSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(BMComponentSchema.Constants.TableName, 0);
			yield return new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 1);
		}

		#region Redundant tests

		public override void TestIsApplicable()
		{
			Assert(true);
		}

		public override void TestApply_DbHits()
		{
			Assert(true);
		}

		#endregion
	}

	[TestedType(typeof(ComponentViewFilter))]
	[TestDate(2015, 6, 3)]
	class ComponentViewFilterApplicatorTest : TaskVisibilityFilterApplicatorTestCase<ComponentViewFilter>
	{
		#region FilterApplicatorTestCase Overrides

		public override void TestIsApplicable()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "constrained workflow", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "non-constrained workflow", config.Buffer);

			var preCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code);
			var ccrTask = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code);
			var postCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR2.GS_Code);
			var nonCCRTask = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code);

			Factory.Save();

			var filter = GetFilter();
			var cell = new CellContent(0, 0, CellContentType.Cards);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			AssertIsApplicableIsSane(preCCRTask, ccrTask, postCCRTask, nonCCRTask, config);
		}

		public override void TestApply_DbHits()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "constrained workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "non-constrained workflow");

			var preCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code);
			var ccrTask = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code);
			var postCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR2.GS_Code);
			var nonCCRTask = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			preCCRTask = newFactory.Load<ProcessTask>(preCCRTask.PK);
			ccrTask = newFactory.Load<ProcessTask>(ccrTask.PK);
			postCCRTask = newFactory.Load<ProcessTask>(postCCRTask.PK);
			nonCCRTask = newFactory.Load<ProcessTask>(nonCCRTask.PK);

			section = newFactory.Load<BMBoardSection>(section.PK);

			var filter = GetFilter();
			var cell = new CellContent(0, 0, CellContentType.Cards);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			filter.CurrentlySelectedOption = null;
			var hits = new Dictionary<string, int>
			{
				{ ProcessHeaderSchema.Constants.TableName, 3 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
			};
			CheckIsApplicableAndAssertDbHits(newFactory, filter, viewModel, hits, preCCRTask, ccrTask, postCCRTask, nonCCRTask);

			filter.CurrentlySelectedOption = config.PreConstraintBuffer;
			hits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
			};
			CheckIsApplicableAndAssertDbHits(newFactory, filter, viewModel, hits, preCCRTask, ccrTask, postCCRTask, nonCCRTask);

			filter.CurrentlySelectedOption = config.PostConstraintBuffer;
			hits = new Dictionary<string, int>();
			CheckIsApplicableAndAssertDbHits(newFactory, filter, viewModel, hits, preCCRTask, ccrTask, postCCRTask, nonCCRTask);

			filter.CurrentlySelectedOption = config.Constraint;
			CheckIsApplicableAndAssertDbHits(newFactory, filter, viewModel, hits, preCCRTask, ccrTask, postCCRTask, nonCCRTask);
		}

		static void CheckIsApplicableAndAssertDbHits(BusinessObjectFactory factory, ComponentViewFilter filter, BMBoardSectionViewModel viewModel, Dictionary<string, int> allowedHits, params ProcessTask[] tasks)
		{
			factory.ResetDatabaseLoadCount();

			var cell = new CellContent(0, 0, CellContentType.Cards);

			foreach (var task in tasks)
			{
				filter.IsApplicable(task, cell, viewModel);
			}

			AssertDbHits(allowedHits, factory);
		}

		protected override ComponentViewFilter GetFilter()
		{
			return new ComponentViewFilter();
		}

		protected override IEnumerable<KeyValuePair<string, int>> GetExpectedDbHits()
		{
			yield return new KeyValuePair<string, int>(ProcessTasksSchema.Constants.TableName, 1);
			yield return new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 1);
		}

		#endregion

		#region IsApplicable

		public void TestIsApplicable_WhenCCRTaskIsClosed()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "constrained workflow", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "non-constrained workflow", config.Buffer);

			var preCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code);
			var ccrTask = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var postCCRTask = BMSTestHelper.CreateTask(workflow1, config.NonCCR2.GS_Code);
			var nonCCRTask = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code);

			AssertIsApplicableIsSane(preCCRTask, ccrTask, postCCRTask, nonCCRTask, config);
		}

		void AssertIsApplicableIsSane(ProcessTask preCCRTask, ProcessTask ccrTask, ProcessTask postCCRTask, ProcessTask nonCCRTask, ConstrainedSchematicTestConfig config)
		{
			var filter = GetFilter();

			CombineAssertions("No option selected - all tasks should be applicable", () =>
			{
				AssertNull(filter.CurrentlySelectedOption);

				AssertEquals("preCCRTask", true, filter.IsApplicable(preCCRTask, null, null));
				AssertEquals("ccrTask", true, filter.IsApplicable(ccrTask, null, null));
				AssertEquals("postCCRTask", true, filter.IsApplicable(postCCRTask, null, null));
				AssertEquals("nonCCRTask", true, filter.IsApplicable(nonCCRTask, null, null));
			});

			filter.CurrentlySelectedOption = config.PreConstraintBuffer;

			CombineAssertions("Pre-CCR option selected", () =>
			{
				AssertEquals("preCCRTask", true, filter.IsApplicable(preCCRTask, null, null));
				AssertEquals("ccrTask", false, filter.IsApplicable(ccrTask, null, null));
				AssertEquals("postCCRTask", false, filter.IsApplicable(postCCRTask, null, null));
				AssertEquals("nonCCRTask", false, filter.IsApplicable(nonCCRTask, null, null));
			});

			filter.CurrentlySelectedOption = config.Constraint;

			CombineAssertions("CCR option selected", () =>
			{
				AssertEquals("preCCRTask", false, filter.IsApplicable(preCCRTask, null, null));
				AssertEquals("ccrTask", true, filter.IsApplicable(ccrTask, null, null));
				AssertEquals("postCCRTask", false, filter.IsApplicable(postCCRTask, null, null));
				AssertEquals("nonCCRTask", false, filter.IsApplicable(nonCCRTask, null, null));
			});

			filter.CurrentlySelectedOption = config.PostConstraintBuffer;

			CombineAssertions("Post-CCR option selected", () =>
			{
				AssertEquals("preCCRTask", false, filter.IsApplicable(preCCRTask, null, null));
				AssertEquals("ccrTask", false, filter.IsApplicable(ccrTask, null, null));
				AssertEquals("postCCRTask", true, filter.IsApplicable(postCCRTask, null, null));
				AssertEquals("nonCCRTask", false, filter.IsApplicable(nonCCRTask, null, null));
			});
		}

		#endregion

	}
}
