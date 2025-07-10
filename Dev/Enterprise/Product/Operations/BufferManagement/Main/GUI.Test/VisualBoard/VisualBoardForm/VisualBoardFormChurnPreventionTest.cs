using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestDate(2015, 7, 14)]
	class VisualBoardFormChurnPreventionTest : BMSGUITestCase
	{
		#region ActiveBusinessObjectCollection Churn

		public void TestActiveBusinessObjectCollections_ShouldOnlyBeConstructedForConfig_NeverForOperationalData()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section2.Row = 1;

			FilterStripsTestHelper.AddStartsWithFilter(section1.TaskFilter, "Description", "Adam");
			FilterStripsTestHelper.AddStartsWithFilter(section1.WorkflowFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Adam");

			FilterStripsTestHelper.AddStartsWithFilter(section2.TaskFilter, "Description", "Durkee");
			FilterStripsTestHelper.AddStartsWithFilter(section2.WorkflowFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Durkee");

			FilterStripsTestHelper.AddStartsWithFilter(config.ComponentLink.FilterRule, ProcessHeader.ModuleFilterConstants.WorkflowStatus, "OPN");

			var tag1 = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "AAA"), "AAA");
			var tag2 = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "BBB"), "BBB");

			const int numJobs = 10;
			const int numWorkflowSets = 10;

			for (var i = 0; i < numJobs; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var diagram = NetworkTestCase.CreateDiagram(jobHeader);

				for (var j = 0; j < numWorkflowSets; j++)
				{
					var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Adam parent workflow " + j, config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
					NetworkTestCase.CreateShape(parentWorkflow, diagram);

					var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Durkee child workflow " + j, config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
					var dependentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Adam dependent workflow " + j, config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

					BMSTestHelper.CreateTask(parentWorkflow, config.CCR.GS_Code, description: "Adam");
					BMSTestHelper.CreateTask(childWorkflow, config.NonCCR1.GS_Code, description: "Durkee");
					BMSTestHelper.CreateTask(dependentWorkflow, config.NonCCR2.GS_Code, description: "Adam");

					parentWorkflow.AddTag(tag1);
					dependentWorkflow.AddTag(tag2);
				}

				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
				var network = networkViewModel.GetJobNetwork();
				network.SwitchToScaled();
				networkViewModel.ToggleApproval();
			}

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);

			var newFactory = Factory.CreateNewFactory();
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(loadedBoard)))
			{
				form.Show();
				Application.DoEvents();

				var tickets = form.FindAll<TaskCardControl>().ToArray();

				Assert(tickets.Length <= 300);

				AssertContainsExactElementsInAnyOrder("Creating ActiveBusinessObjectCollections adds overhead every time a bizo of the collection's entity type is loaded in the factory. It has to evaluate the collection's query which can be quite expensive when there are many collections and many entities.", new[]
				{
					new KeyValuePair<Type, int>(typeof(BMBoardSectionAdditionalComponentCollection), 5),
					new KeyValuePair<Type, int>(typeof(BMBoardSectionChannelCollection), 8),
					new KeyValuePair<Type, int>(typeof(BMBoardSectionCollection), 4),
					new KeyValuePair<Type, int>(typeof(BMComponentCollection), 4),
					new KeyValuePair<Type, int>(typeof(GlbGroupActiveBusinessObjectCollection), 1),
					new KeyValuePair<Type, int>(typeof(BMComponentLinkDependentCollection), 2),
					new KeyValuePair<Type, int>(typeof(BMComponentReleaseGroupLinkCollection), 1),
					new KeyValuePair<Type, int>(typeof(BMControlCustomisationLinkCollection), 5), // To determine which layout to use for each section, board, release group and system.
					new KeyValuePair<Type, int>(typeof(DaylightSavingTimeZoneCollection), 3),
					new KeyValuePair<Type, int>(typeof(GlbResourceCapabilityPivotCollection), 3),
					new KeyValuePair<Type, int>(typeof(BMComponentResourceLinkCollection), 1),
					new KeyValuePair<Type, int>(typeof(GlbWorkTimeCollection), 2), // WorkTimes are required for majority of board capacity calculations
				}, ActiveBusinessObjectCollection.IndexedCollections_ForTest
					.OrderBy(kvp => kvp.Key.Name)
					.Where(kvp => kvp.Key != typeof(StmLinkCollection)) // Because StmLink is non-deterministic when running multiple tests.
					);
			}
		}

		#endregion

		#region Database Hits Churn

		public void TestShowBoard_WhenRegistryPreventingDbHitsOnGUIThreadEnabled_ShouldReportErrors()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateSectionAndViewModel(config.System);
			var viewModel = BMSTestHelper.CreateViewModel(section.Item1);

			Factory.Save();

			BMSRegistry.Instance.DisallowDBHitsOnBoardGUIThread.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			string[] allowedTables = { "ABCSoftwareAction", "ABCSoftwareConsequenceQueue", "ProcessTasks" };
			BMSRegistry.Instance.AllowedTablesDBHitsOnBoardGUIThread.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedTables);
			var newFactory = Factory.CreateNewFactory();
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);
			using (DisableAsyncBehaviour())
			using (var control = new BMComponentControl(section.Item1.SectionConfiguration, viewModel))
			using (var form = new VisualBoardThreadedDBHitForTest(BMSTestHelper.CreateSlideshowViewModel(loadedBoard)))
			{
				var dummyWorkflowProvider = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
				var jobHeader = ProcessJobHeader.GetForParent(dummyWorkflowProvider, Factory);
				var processHeader = jobHeader.ProcessHeaders[0];
				var task = BMSTestHelper.CreateTask(processHeader);
				form.Show();
				Application.DoEvents();
				control.SetupCards(new BoardRefreshEventArgs());
				ErrorReporter.Clear();

				form.ThreadedDummyDBHit(newFactory, processHeader, false);
				Assert(ErrorReporter.HasBeenReported("DBHit:ProcessHeader"));
				ErrorReporter.Clear();

				form.ThreadedDummyDBHit(newFactory, task, false);
				Assert(!ErrorReporter.HasBeenReported("DBHit:ProcessTasks"));

				form.ThreadedDummyDBHit(newFactory, processHeader, true);
				Assert(!ErrorReporter.HasBeenReported("DBHit:ProcessHeader"));
			}
		}

		#endregion

#region Bitmap Madness
#if !WINZOR  // Bitmap rendering is not used for Winzor, so these tests do not apply.
		public void TestFullRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreRemoved()
		{
			AssertRemovingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(removeAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var loadedTasksToRemove = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, tasksToUpdate.Select(t => t.PK)));

				foreach (var task in loadedTasksToRemove)
				{
					task.Delete();
				}
				newFactory.Save();

				RefreshUntilFullRefreshIsDone(form);
			});
		}

		public void TestFullRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreUpdated()
		{
			AssertUpdatingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(updateAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var loadedTasksToRemove = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, tasksToUpdate.Select(t => t.PK)));

				TestDateAttribute.Date += TimeSpan.FromMinutes(1);
				foreach (var task in loadedTasksToRemove)
				{
					task.P9_Status = "WRK";
				}
				newFactory.Save();

				RefreshUntilFullRefreshIsDone(form);
			});
		}

		public void TestDifferentialRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreRemoved()
		{
			AssertRemovingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(removeAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var loadedTasksToRemove = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, tasksToUpdate.Select(t => t.PK)));

				foreach (var task in loadedTasksToRemove)
				{
					task.Delete();
				}
				newFactory.Save();

				form.RefreshBoard();
			});
		}

		public void TestDifferentialRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreUpdated()
		{
			AssertUpdatingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(updateAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				var newFactory = new BusinessObjectFactory()
				{
					RefreshEnabled = false
				};
				var loadedTasksToRemove = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, tasksToUpdate.Select(t => t.PK)));

				TestDateAttribute.Date += TimeSpan.FromMinutes(1);
				foreach (var task in loadedTasksToRemove)
				{
					task.P9_Status = "WRK";
				}
				newFactory.Save();

				form.RefreshBoard();
			});
		}

		public void TestPartialRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreRemoved()
		{
			AssertRemovingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(removeAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				foreach (var task in tasksToUpdate)
				{
					task.Delete();
				}
				factory.Save();
				Application.DoEvents();
			});
		}

		public void TestPartialRefresh_ShouldDisposeUnusedBitmapsAndRemoveFromCacheAndNotTouchUsedOnes_WhenSomeTasksAreUpdated()
		{
			AssertUpdatingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(updateAndRefreshAction: (factory, form, tasksToUpdate) =>
			{
				TestDateAttribute.Date += TimeSpan.FromMinutes(1);
				foreach (var task in tasksToUpdate)
				{
					task.P9_Status = "WRK";
				}
				factory.Save();
				Application.DoEvents();
			});
		}

		void AssertRemovingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(Action<BusinessObjectFactory, VisualBoardForm, IEnumerable<ProcessTask>> removeAndRefreshAction)
		{
			const int numWorkflows = 50;
			const int numTasksToRemove = 3;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: numWorkflows, numberOfTasksPerWorkflow: 1);
			var tasks = workflows.SelectMany(w => w.Tasks).ToArray();
			var tasksToStay = tasks.Skip(numTasksToRemove).ToArray();
			var tasksToRemove = tasks.Take(numTasksToRemove).ToArray();
			var board = config.BufferBoard;

			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board, width: 1200, height: 1200))
			{
				Application.DoEvents();

				AssertRefreshDisposesUnusedBitmapsAndRemovesThemFromCacheAndDoesNotTouchUsedOnes(tasksToStay, tasksToRemove,
					expectedTasksCountAfterRefresh: tasksToStay.Length,
					totalTasksCount: tasksToStay.Length + tasksToRemove.Length,
					form: form,
					updateAndRefreshAction: () => removeAndRefreshAction.Invoke(Factory, form, tasksToRemove));
			}
		}

		void AssertUpdatingTasksAndRefreshingDisposesUnusedBitmapsAndRemovesFromCacheAndDoesNotTouchUsedOnes(Action<BusinessObjectFactory, VisualBoardForm, IEnumerable<ProcessTask>> updateAndRefreshAction)
		{
			const int numWorkflows = 50;
			const int numTasksToRemove = 3;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: numWorkflows, numberOfTasksPerWorkflow: 1);
			var tasks = workflows.SelectMany(w => w.Tasks).ToArray();
			var tasksToStay = tasks.Skip(numTasksToRemove).ToArray();
			var tasksToUpdate = tasks.Take(numTasksToRemove).ToArray();
			var board = config.BufferBoard;

			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board, width: 1200, height: 1200))
			{
				Application.DoEvents();

				AssertRefreshDisposesUnusedBitmapsAndRemovesThemFromCacheAndDoesNotTouchUsedOnes(tasksToStay, tasksToUpdate,
					expectedTasksCountAfterRefresh: tasksToStay.Length + tasksToUpdate.Length,
					totalTasksCount: tasksToStay.Length + tasksToUpdate.Length,
					form: form,
					updateAndRefreshAction: () => updateAndRefreshAction.Invoke(Factory, form, tasksToUpdate));
			}
		}

		void AssertRefreshDisposesUnusedBitmapsAndRemovesThemFromCacheAndDoesNotTouchUsedOnes(IEnumerable<ProcessTask> tasksToStayUnchanged, IEnumerable<ProcessTask> tasksToBeUpdatedOrRemoved,
			int expectedTasksCountAfterRefresh, int totalTasksCount,
			VisualBoardForm form, Action updateAndRefreshAction)
		{
			CardBitmaps[] GetCurrentlyCachedBitmaps(IEnumerable<ProcessTask> tasks)
			{
				var sectionViewModel = (BMBoardSectionViewModel)form.BoardViewModel.GetSections().Single();
				return tasks.Where(t => !t.IsDeleted).Select(t => sectionViewModel.GetCardBitmaps(new TaskCardContent(t, sectionViewModel))).ToArray();
			}

			var bitmapsToStay = GetCurrentlyCachedBitmaps(tasksToStayUnchanged);
			var bitmapsToBeUpdatedOrRemoved = GetCurrentlyCachedBitmaps(tasksToBeUpdatedOrRemoved);

			CombineAssertions("Bitmaps to be removed should be used and cached", () =>
			{
				foreach (var bitmap in bitmapsToBeUpdatedOrRemoved)
				{
					Assert("Precondition: bitmaps to be updated or removed should not be disposed yet", !bitmap.IsDisposed);
				}
				AssertNumberOfCachedBitmaps(form, totalTasksCount);
			});

			updateAndRefreshAction.Invoke();

			CombineAssertions("Kept bitmaps should stay in use and not be disposed", () =>
			{
				foreach (var bitmap in bitmapsToStay)
				{
					Assert("Used bitmaps should not be disposed", !bitmap.IsDisposed);
				}

				AssertContainsExactElementsInAnyOrder("Tasks that haven't been updated should have the same bitmaps in the cache", bitmapsToStay, GetCurrentlyCachedBitmaps(tasksToStayUnchanged));
			});

			CombineAssertions("Unused bitmaps should be disposed", () =>
			{
				foreach (var bitmap in bitmapsToBeUpdatedOrRemoved)
				{
					Assert("Old bitmaps for updated or removed cards should be disposed", bitmap.IsDisposed);
				}

				var bitmapsForUpdatedTasks = GetCurrentlyCachedBitmaps(tasksToBeUpdatedOrRemoved);
				var updatedIncompleteTasks = tasksToBeUpdatedOrRemoved.Where(t => !t.IsDeleted && t.IsOpen).ToArray();

				AssertCollectionNotContains("Bitmaps previously cached for updated tasks should be removed", bitmapsToBeUpdatedOrRemoved, bitmapsForUpdatedTasks);
				AssertEquals("Updated tasks should have new bitmaps added to the cache", updatedIncompleteTasks.Length, bitmapsForUpdatedTasks.Length);
			});

			CombineAssertions("Old bitmaps for updated or removed cards should be removed from cache", () =>
			{
				AssertNumberOfCachedBitmaps(form, expectedTasksCountAfterRefresh);
			});
		}

		public void TestRefreshManyTimes_ShouldNotCacheBitmapsExcessively()
		{
			const int numWorkflows = 50;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: numWorkflows, numberOfTasksPerWorkflow: 1);
			var board = config.BufferBoard;

			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4;

			Factory.Save();

			var expectedNumberOfTicketsCreated = numWorkflows + 1; // One for the template ticket

			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = GetAndShowVisualBoardForm(board, width: 1200, height: 1200))
			{
				form.WindowState = FormWindowState.Maximized;
				Application.DoEvents();

				AssertNumberOfCachedBitmaps(form, numWorkflows);
				AssertEquals(expectedNumberOfTicketsCreated, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);

				var numRefreshes = BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.Value + 1; // :-|

				for (var i = 1; i <= numRefreshes; i++)
				{
					form.RefreshBoard();
					Application.DoEvents();

					if (i == numRefreshes) // Last refresh will re-create tickets (previous refreshes only update tickets whose underlying data has changed).
					{
						expectedNumberOfTicketsCreated += numWorkflows;
					}

					AssertEquals(expectedNumberOfTicketsCreated, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
				}

				AssertNumberOfCachedBitmaps(form, numWorkflows);
			}
		}

		public void TestUpdateTicketViaPartialRefresh_ShouldNotCacheBitmapsExcessively()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "State of the Younion", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);
			var board = config.BufferBoard;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var ticket = form.FindSingle<TaskCardControl>();

				AssertNumberOfCachedBitmaps(form, 1);

				task.P9_CardNote = "Younited Statesss";
				Factory.Save();
				Application.DoEvents();

				var newTicket = form.FindSingle<TaskCardControl>();

				AssertNumberOfCachedBitmaps(form, 1);
				AssertNotEquals(ticket, newTicket);

				AssertEquals("Younited Statesss", newTicket.CardContent.NoteText);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
				Application.DoEvents();

				var newerTicket = form.FindSingleOrDefault<TaskCardControl>();

				AssertNull(newerTicket);
				AssertNumberOfCachedBitmaps(form, 0);
			}
		}

		public void TestUpdateTicketViaNormalRefresh_ShouldNotCacheBitmapsExcessively()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "State of the Younion", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);
			var board = config.BufferBoard;

			Factory.Save();
			Factory.RefreshEnabled = false; // Make tickets update through board refresh rather than individually with data refresh bus.

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var ticket = form.FindSingle<TaskCardControl>();

				AssertNumberOfCachedBitmaps(form, 1);

				TestDateAttribute.AddMinutes(1);

				task.P9_CardNote = "Younited Statesss";
				Factory.Save();
				Application.DoEvents();

				var sameTicket = form.FindSingle<TaskCardControl>();

				AssertNumberOfCachedBitmaps(form, 1);
				AssertEquals(ticket, sameTicket);

				RefreshUntilFullRefreshIsDone(form);
				Application.DoEvents();

				var newTicket = form.FindSingle<TaskCardControl>();

				AssertNumberOfCachedBitmaps(form, 1);
				AssertNotEquals(ticket, newTicket);
				AssertEquals("Younited Statesss", newTicket.CardContent.NoteText);

				TestDateAttribute.AddMinutes(1);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
				RefreshUntilFullRefreshIsDone(form);
				Application.DoEvents();

				var newerTicket = form.FindSingleOrDefault<TaskCardControl>();

				AssertNull(newerTicket);
				AssertNumberOfCachedBitmaps(form, 0);
			}
		}

		void RefreshUntilFullRefreshIsDone(VisualBoardForm form)
		{
			for (int i = 0; i < BMSRegistry.Instance.NumberOfRefreshesBeforeFullRefresh.Value; i++)
			{
				form.RefreshBoard();
			}
		}

		public void TestReloadBoard_ShouldNotCacheBitmapsExcessively()
		{
			const int numWorkflows = 50;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: numWorkflows, numberOfTasksPerWorkflow: 1);
			var board = config.BufferBoard;

			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4;

			Factory.Save();

			var expectedNumberOfTicketsCreated = numWorkflows + 1; // One for the template ticket

			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = GetAndShowVisualBoardForm(board, width: 1200, height: 1200))
			{
				form.WindowState = FormWindowState.Maximized;
				Application.DoEvents();

				AssertNumberOfCachedBitmaps(form, numWorkflows);
				AssertEquals(expectedNumberOfTicketsCreated, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);

				form.ReloadBoard();
				Application.DoEvents();

				expectedNumberOfTicketsCreated *= 2;

				AssertEquals(expectedNumberOfTicketsCreated, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
				AssertNumberOfCachedBitmaps(form, numWorkflows);
			}
		}

		public void TestPartialRefresh_ShouldNotDisposeBitmapsWhichMayBeStillInUse()
		{
			const int numWorkflows = 2;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var workflows = BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: numWorkflows, numberOfTasksPerWorkflow: 1);
			var board = config.BufferBoard;
			config.BufferSection.SectionConfiguration.CellsPerSubsection = 4;
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				Application.DoEvents();

				var controls = form.FindAll<BMComponentControl>();
				var viewModels = controls.Select(c => c.ViewModel).ToArray();
				var bitmapCaches = GetBitmapCaches(viewModels);

				CombineAssertions("Precondition", () =>
				{
					AssertEquals(numWorkflows, bitmapCaches.Length);
					AssertAllBitmapsAreInUse(bitmapCaches);
				});

				var bmBoardSectionViewModel = viewModels[0];
				var allocationMap = CardAllocationMap.NewAllocationMap(config.BufferSection, bmBoardSectionViewModel, TaskChannelMap.ForTest(config.BufferSection, bmBoardSectionViewModel, workflows));
				bmBoardSectionViewModel.ComponentGrid.RefreshComponent(Factory, bmBoardSectionViewModel, allocationMap, requiresFullRedraw: false);

				CombineAssertions("After differential refresh", () =>
				{
					AssertEquals(numWorkflows, bitmapCaches.Length);
					AssertAllBitmapsAreInUse(bitmapCaches);
				});
			}
		}

		static void AssertNumberOfCachedBitmaps(VisualBoardForm form, int expectedNumberOfCachedBitmaps)
		{
			var viewModels = form.FindAll<BMComponentControl>().Select(c => c.ViewModel);
			var bitmapCaches = GetBitmapCaches(viewModels);
			AssertEquals(expectedNumberOfCachedBitmaps, bitmapCaches.Length);
		}

		static CardBitmaps[] GetBitmapCaches(IEnumerable<BMBoardSectionViewModel> viewModels)
		{
			return viewModels.SelectMany(vm => GetBitmapCaches(vm)).ToArray();
		}

		static IEnumerable<CardBitmaps> GetBitmapCaches(BMBoardSectionViewModel viewModel)
		{
			return ((IDictionary<Tuple<ZGuid, ZDateTime>, CardBitmaps>)typeof(BMBoardSectionViewModel).GetField("cardBitmaps", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(viewModel)).Values;
		}

		static void AssertAllBitmapsAreInUse(IEnumerable<CardBitmaps> bitmapCaches)
		{
			Assert("Bitmaps should not be disposed", bitmapCaches.All(b => !b.NormalBitmap.IsDisposed()));
		}
#endif
#endregion

		#region Implementation

		protected override bool ShouldDisableAsyncBehaviour => true;

		protected override void SetUp()
		{
			base.SetUp();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);
		}

		class VisualBoardThreadedDBHitForTest : VisualBoardForm
		{
			public VisualBoardThreadedDBHitForTest(BoardSlideshowViewModel viewModel) : base(viewModel)
			{
			}

			public void ThreadedDummyDBHit(BusinessObjectFactory factory, BusinessObject bizo, bool newThread)
			{
				if (!newThread)
				{
					DummyDBHit(factory, bizo);
				}
				else
				{
					factory.ThreadSentry.RelinquishThreadOwnership();
					Task.Factory.StartNew(() =>
					{
						factory.ThreadSentry.TakeThreadOwnership();
						DummyDBHit(factory, bizo);
					}).Wait();
				}
			}

			void DummyDBHit(BusinessObjectFactory factory, BusinessObject bizo)
			{
				var query = new ZQuery(bizo.PKSchemaColumn, bizo.PK);
				factory.LoadTop1(bizo.GetType(), query);
			}
		}

		#endregion
	}
}
