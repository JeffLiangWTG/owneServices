using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormDataRefreshBusProgressIndicatorTest : NonTransactionedTestCase
	{
		#region Saving Detailed Ticket

		public void TestSaveDetailedTicket_WhenSettingCardNote_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var detailedTicket = ShowDetailedTicket(task, form);

				AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(detailedTicket, form,
					updateAction: t => t.P9_CardNote = "In the morning",
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("buffer", "Updating tickets..."),
					});

				var ticket = BMSGUITestCase.FindTaskCardControl(form, task);

				AssertEquals("In the morning", ticket.CardContent.NoteText);
			}
		}

		public void TestSaveDetailedTicket_WhenClosingTask_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var detailedTicket = ShowDetailedTicket(task, form);

				AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(detailedTicket, form,
					updateAction: t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed,
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("buffer", "Updating tickets..."),
					});
			}
		}

		public void TestSaveDetailedTicket_WhenClaimingCapabilityTask_ShouldIndicateProgress()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "DEA", "Being Dave");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DVB", "Davey Boy", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BGD", "Big Dave", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DJD", "DJ Davey Dave", capability);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			var task = BMSTestHelper.CreateTask(workflow, capability: capability);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var detailedTicket = ShowDetailedTicket(task, form);

				AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(detailedTicket, form,
					updateAction: t => t.P9_GS_NKAssignedStaffMember = resource1.GS_Code,
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("buffer", "Updating tickets..."),
					});
			}
		}

		public void TestSaveDetailedTicket_WhenClaimingCapabilityTask_AndTaskAssignmentRestrictionsCascadeToOtherTasks_ShouldIndicateProgress()
		{
			BMSTestHelper.AddTaskTypesToRegistry("ORG", "CDU", "CDF");
			BMSTestHelper.AddTaskAssignmentRestriction("ORG", "CDU", new[] { "CDF" }, RestrictionTypeList.Codes.SameResource);

			var capability = BMSTestHelper.CreateCapability(Factory, "DEA", "Being Dave");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DVB", "Davey Boy", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BGD", "Big Dave", capability);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DJD", "DJ Davey Dave", capability);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "CDU", description: "I thought there'd be some coding", capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CDF", description: "Just less", capability: capability);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var detailedTicket = ShowDetailedTicket(task1, form);

				AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(detailedTicket, form,
					updateAction: t => t.P9_GS_NKAssignedStaffMember = resource1.GS_Code,
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("buffer", "Updating tickets..."),
					});

				task1.Reload();
				task2.Reload();

				AssertEquals(resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
				AssertEquals(resource1.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestSaveDetailedTicket_ForBoardWithMultipleSections_WhenSettingCardNote_ShouldIndicateProgressOnAllSections()
		{
			var otherSection = BMSTestHelper.CreateBoardSection(config.Bucket, board, row: 1);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var detailedTicket = ShowDetailedTicket(task, form);

				AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(detailedTicket, form,
					updateAction: t => t.P9_CardNote = "In the morning",
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("bucket", "Refreshing, please wait..."), // No need to show 'updating tickets' for the bucket section because the task isn't present on this section.
					});
			}
		}

		#endregion

		#region Saving Outside the Board

		public void TestSaveSingleTaskOutsideTheBoard_WhenSettingCardNote_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () => task.P9_CardNote = "In the morning",
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
					});

				var ticket = form.FindAll<TaskCardControl>().Single();

				AssertEquals("In the morning", ticket.CardContent.NoteText);
			}
		}

		public void TestSaveSingleTaskOutsideTheBoard_WhenClosingTask_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () => task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed,
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
					});

				var ticket = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNull(ticket);
			}
		}

		public void TestSaveSingleTaskOutsideTheBoard_WhenDeletingTask_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () => task.Delete(),
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
					});

				var ticket = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNull(ticket);
			}
		}

		public void TestSaveSingleTaskOutsideTheBoard_WhenCreatingNewTask_ShouldIndicateProgress()
		{
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () => BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code),
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
					});

				var tickets = form.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
			}
		}

		public void TestSaveManyTasksOutsideTheBoard_WhenMakingVariousEdits_ShouldIndicateProgress()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			var task3 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () =>
					{
						task1.Delete();
						task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
						task3.P9_CardNote = "OOOH EEE!";
						BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "I'm Mr Meeseeks look at me!");
					},
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
					});

				var tickets = form.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
			}
		}

		public void TestSaveSingleTaskOutsideTheBoard_ForBoardWithMultipleSections_WhenSettingCardNote_ShouldIndicateProgressOnAllSections()
		{
			var otherSection = BMSTestHelper.CreateBoardSection(config.Bucket, board, row: 1);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(form, Factory,
					updateAction: () => task.P9_CardNote = "In the morning",
					expectedIndicatorMessagesPerSection: new[]
					{
						new LoadingMessage("buffer", "Updating tickets..."),
						new LoadingMessage("buffer", "Refreshing, please wait..."),
						new LoadingMessage("bucket", "Refreshing, please wait..."), // No need to show 'updating tickets' for the bucket section because the task isn't present on this section.
					});

				var ticket = form.FindAll<TaskCardControl>().Single();

				AssertEquals("In the morning", ticket.CardContent.NoteText);
			}
		}

		#endregion

		#region Implementation

		static TaskCardDetailControl ShowDetailedTicket(ProcessTask task, VisualBoardForm form)
		{
			TaskCardDetailControl detailedTicket = null;

			var ticket = BMSGUITestCase.FindTaskCardControl(form, task);

			ticket.ShowDetailedCard();

			detailedTicket = form.FindSingle<TaskCardDetailControl>();

			return detailedTicket;
		}

		static void AssertProgressIndicatorShownAfterUpdatingTaskAndSaving(TaskCardDetailControl detailedTicket, VisualBoardForm form, Action<ProcessTask> updateAction, LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			BMSGUITestCase.AssertLoadingIndicatorMessagesShownWhilstPerformingAction(form, () =>
			{
				updateAction(detailedTicket.ProcessTask);
				detailedTicket.Save_ForTest();
				form.AwaitAll();
			},
			expectedIndicatorMessagesPerSection: expectedIndicatorMessagesPerSection);
		}

		static void AssertProgressIndicatorShownAfterUpdatingTaskAndSavingOutsideTheBoard(VisualBoardForm form, BusinessObjectFactory factory, Action updateAction, LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			BMSGUITestCase.AssertLoadingIndicatorMessagesShownWhilstPerformingAction(form, () =>
			{
				updateAction();
				factory.Save();
				form.AwaitAll();
			},
			expectedIndicatorMessagesPerSection: expectedIndicatorMessagesPerSection);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			board = config.BufferBoard;
			section = config.BufferSection;
			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4; // So I can see the tickets better

			workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I wish the milkman would deliver my milk", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
		}

		VisualBoardTestConfig config;
		BMBoard board;
		BMBoardSection section;
		ProcessHeader workflow;

		#endregion
	}
}
