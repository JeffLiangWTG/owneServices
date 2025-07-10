using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.VisualBoards.Business.Test
{
	public class FilterManagerTest : VisualBoardsTestCase
	{
		public void TestRemoveFilter_ByType()
		{
			var filterable = new DummyFilterable();
			var manager = new FilterManager(filterable);
			var filter1 = new TestFilterForToggling("Agrajag");
			var filter2 = new TestFilter();

			manager.ApplyFilter(filter1);
			manager.ApplyFilter(filter2);

			AssertEquals(true, manager.IsApplied(filter1));
			AssertEquals(true, manager.IsApplied(filter2));

			manager.RemoveFilters(typeof(TestFilter));

			AssertEquals(true, manager.IsApplied(filter1));
			AssertEquals(false, manager.IsApplied(filter2));

			manager.RemoveFilters(typeof(TestFilterForToggling));

			AssertEquals(false, manager.IsApplied(filter1));
			AssertEquals(false, manager.IsApplied(filter2));
		}

		public void TestRefreshFilter_WhenFilterRequiresChildrenToBeRefreshed_ShouldPropagateUpdatesToChildFilterManagers()
		{
			var grandparentFilterable = new DummyFilterable();
			var parentFilterable = new DummyFilterable();
			var childFilterable = new DummyFilterable();

			parentFilterable.SetChild(childFilterable);
			grandparentFilterable.SetChild(parentFilterable);

			var descendantRefreshableFilter = new TestFilterRequiringChildUpdates();
			var otherFilter = new TestFilter();

			var updates = new List<Tuple<string, FiltersChangedEventArgs>>();

			grandparentFilterable.FilterManager.FiltersUpdated += (s, e) => updates.Add(Tuple.Create("grandparent", e));
			parentFilterable.FilterManager.FiltersUpdated += (s, e) => updates.Add(Tuple.Create("parent", e));
			childFilterable.FilterManager.FiltersUpdated += (s, e) => updates.Add(Tuple.Create("child", e));

			// Applying filter that propagates refreshes to ancestors as well as descendants.

			grandparentFilterable.FilterManager.ApplyFilter(descendantRefreshableFilter);
			AssertUpdates("Updates to descendants are received when applying filter marked as such", updates,
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("parent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("child", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)));

			parentFilterable.FilterManager.ApplyFilter(descendantRefreshableFilter);
			AssertUpdates("Updates to descendants and ancestors are received when applying filter marked as such", updates,
				Tuple.Create("parent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("child", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)));

			childFilterable.FilterManager.ApplyFilter(descendantRefreshableFilter);
			AssertUpdates("Updates to descendants and ancestors are received when applying filter marked as such", updates,
				Tuple.Create("child", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("parent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { descendantRefreshableFilter }, null)));

			// Applying filter that only refreshes ancestors.

			grandparentFilterable.FilterManager.ApplyFilter(otherFilter);
			AssertUpdates("Updates are received when applying filters, and should propagate to ancestors only", updates,
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { otherFilter }, null)));

			parentFilterable.FilterManager.ApplyFilter(otherFilter);
			AssertUpdates("Updates are received when applying filters, and should propagate to ancestors only", updates,
				Tuple.Create("parent", new FiltersChangedEventArgs(new[] { otherFilter }, null)),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { otherFilter }, null)));

			childFilterable.FilterManager.ApplyFilter(otherFilter);
			AssertUpdates("Updates are received when applying filters, and should propagate to ancestors only", updates,
				Tuple.Create("child", new FiltersChangedEventArgs(new[] { otherFilter }, null)),
				Tuple.Create("parent", new FiltersChangedEventArgs(new[] { otherFilter }, null)),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(new[] { otherFilter }, null)));

			// Removing filter that propagates refreshes to ancestors as well as descendants.

			grandparentFilterable.FilterManager.RemoveFilter(descendantRefreshableFilter, shouldRefresh: true);
			AssertUpdates("Updates to descendants are received when removing filter marked as such", updates,
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("parent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("child", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })));

			parentFilterable.FilterManager.RemoveFilter(descendantRefreshableFilter, shouldRefresh: true);
			AssertUpdates("Updates to descendants and ancestors are received when removing filter marked as such", updates,
				Tuple.Create("parent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("child", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })));

			childFilterable.FilterManager.RemoveFilter(descendantRefreshableFilter, shouldRefresh: true);
			AssertUpdates("Updates to descendants and ancestors are received when removing filter marked as such", updates,
				Tuple.Create("child", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("parent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { descendantRefreshableFilter })));

			// Removing filter that only refreshes ancestors.

			grandparentFilterable.FilterManager.RemoveFilter(otherFilter, shouldRefresh: true);
			AssertUpdates("Updates are received when removing filters, and should propagate to ancestors only", updates,
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { otherFilter })));

			parentFilterable.FilterManager.RemoveFilter(otherFilter, shouldRefresh: true);
			AssertUpdates("Updates are received when removing filters, and should propagate to ancestors only", updates,
				Tuple.Create("parent", new FiltersChangedEventArgs(null, new[] { otherFilter })),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { otherFilter })));

			childFilterable.FilterManager.RemoveFilter(otherFilter, shouldRefresh: true);
			AssertUpdates("Updates are received when removing filters, and should propagate to ancestors only", updates,
				Tuple.Create("child", new FiltersChangedEventArgs(null, new[] { otherFilter })),
				Tuple.Create("parent", new FiltersChangedEventArgs(null, new[] { otherFilter })),
				Tuple.Create("grandparent", new FiltersChangedEventArgs(null, new[] { otherFilter })));
		}

		static void AssertUpdates(string message, List<Tuple<string, FiltersChangedEventArgs>> actualUpdates, params Tuple<string, FiltersChangedEventArgs>[] expectedUpdates)
		{
			string[] Transform(IEnumerable<Tuple<string, FiltersChangedEventArgs>> set)
			{
				return set.Select(t => FormattableString.Invariant($@"{t.Item1}: Filters added: {string.Join(",", t.Item2.AddedFilters)}; Filters removed: {string.Join(",", t.Item2.RemovedFilters)}")).ToArray();
			}

			try
			{
				AssertSequencesEqual(message, Transform(expectedUpdates), Transform(actualUpdates));
			}
			finally
			{
				actualUpdates.Clear();
			}
		}

		public void TestIsApplied()
		{
			var filterable = new DummyFilterable();
			var filters = filterable.FilterManager;
			var filter1 = new TestFilterForToggling("Filter1");
			var filter2 = new TestFilterForToggling("Filter2");

			filters.ApplyFilter(filter1);

			AssertEquals(true, filterable.FilterManager.IsApplied(filter1));
			AssertEquals(false, filterable.FilterManager.IsApplied(filter2));
		}

		public void TestIsApplied_Parent()
		{
			var parentFilterable = new DummyFilterable();
			var childFilterable = new DummyFilterable();

			parentFilterable.SetChild(childFilterable);

			var parentFilters = parentFilterable.FilterManager;
			var childFilters = childFilterable.FilterManager;

			var filter1 = new TestFilterForToggling("Filter1");
			var filter2 = new TestFilterForToggling("Filter2");
			var filter3 = new TestFilterForToggling("Filter3");

			childFilters.ApplyFilter(filter1);
			parentFilters.ApplyFilter(filter2);

			AssertEquals(true, childFilterable.FilterManager.IsApplied(filter1));
			AssertEquals(true, childFilterable.FilterManager.IsApplied(filter2));
			AssertEquals(false, childFilterable.FilterManager.IsApplied(filter3));

			AssertEquals(false, parentFilterable.FilterManager.IsApplied(filter1));
			AssertEquals(true, parentFilterable.FilterManager.IsApplied(filter2));
			AssertEquals(false, parentFilterable.FilterManager.IsApplied(filter3));
		}

		public void TestIsApplied_ByType()
		{
			var parentFilterable = new DummyFilterable();
			var childFilterable = new DummyFilterable();

			parentFilterable.SetChild(childFilterable);

			var parentFilters = parentFilterable.FilterManager;
			var childFilters = childFilterable.FilterManager;

			var filter1 = new TestFilter();

			childFilters.ApplyFilter(filter1);
			parentFilters.ApplyFilter(filter1);

			AssertEquals(true, childFilterable.FilterManager.IsApplied(typeof(TestFilter)));
			AssertEquals(true, parentFilterable.FilterManager.IsApplied(typeof(TestFilter)));

			AssertEquals(true, childFilterable.FilterManager.IsApplied(typeof(IBoardFilter)));
			AssertEquals(true, parentFilterable.FilterManager.IsApplied(typeof(IBoardFilter)));

			AssertEquals(false, childFilterable.FilterManager.IsApplied(typeof(TestFilterForToggling)));
			AssertEquals(false, parentFilterable.FilterManager.IsApplied(typeof(TestFilterForToggling)));

			childFilters.Clear();
			parentFilters.Clear();
			AssertEquals(false, childFilterable.FilterManager.IsApplied(typeof(TestFilter)));
			AssertEquals(false, parentFilterable.FilterManager.IsApplied(typeof(TestFilter)));
		}

		public void TestClearFilters_ShouldNotRefreshIfThereAreNoFiltersApplied()
		{
			var filterable = new DummyFilterable();
			var filters = new FilterManager(filterable);
			filters.ApplyingFilters += (s, e) => Assert("Should not apply filters", false);

			AssertEquals(0, filters.AppliedFilters.Count());
			filters.Clear();
		}

		public void TestClearFiltersOfType_ShouldNotRefreshIfThereAreNoFiltersApplied()
		{
			var filterable = new DummyFilterable();
			var filters = new FilterManager(filterable);
			filters.ApplyingFilters += (s, e) => Assert("Should not apply filters", false);

			AssertEquals(0, filters.AppliedFilters.Count());
			filterable.FilterManager.ClearFiltersOfType<IBoardFilter>();
		}

		public void TestApplyFilter()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var filter1 = new TestFilter();
			filter1.ApplyAction = () => apply1Count++;

			var apply2Count = 0;
			var filter2 = new TestFilter();
			filter2.ApplyAction = () => apply2Count++;

			filters.ApplyFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());
			filters.ApplyFilter(filter1);
			AssertEquals(2, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());

			filters.ApplyFilter(filter2);
			AssertEquals(3, apply1Count);
			AssertEquals(1, apply2Count);
			AssertEquals(2, filters.AppliedFilters.Count());
		}

		public void TestRemoveFilter()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var filter1 = new TestFilter();
			filter1.ApplyAction = () => apply1Count++;

			filters.RemoveFilter(filter1);
			AssertEquals(0, filters.AppliedFilters.Count());

			filters.ApplyFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());
			filters.RemoveFilter(filter1);
			AssertEquals(0, filters.AppliedFilters.Count());

			filters.RemoveFilter(filter1);
			AssertEquals(0, filters.AppliedFilters.Count());

			filters.ApplyFilter(filter1);
			AssertEquals(2, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());
			filters.RemoveFilter(filter1);
			AssertEquals(0, filters.AppliedFilters.Count());
		}

		public void TestClearFilters()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var filter1 = new TestFilter();
			filter1.ApplyAction = () => apply1Count++;

			filters.ApplyFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());

			filters.Clear();
			AssertEquals(0, filters.AppliedFilters.Count());

			filters.Clear();
			AssertEquals(0, filters.AppliedFilters.Count());
		}

		public void TestToggleFilter()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var filter1 = new TestFilter();
			filter1.ApplyAction = () => apply1Count++;

			filters.ToggleFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());

			filters.ToggleFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(0, filters.AppliedFilters.Count());
		}

		public void TestToggleFilter_WithDifferentFilterInstances()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var filter1 = new TestFilterForToggling("Filter");
			var filter2 = new TestFilterForToggling("Filter");
			Assert("The two different filters have the same hashcode, so should be equal", filter1.Equals(filter2));

			filter1.ApplyAction = () => apply1Count++;
			filter2.ApplyAction = () => apply1Count++;

			filters.ToggleFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(1, filters.AppliedFilters.Count());

			filters.ToggleFilter(filter2);
			AssertEquals(1, apply1Count);
			AssertEquals(0, filters.AppliedFilters.Count());
		}

		public void TestApplyFilter_WhenNotAllowingMultiple()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);
			var sectionViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First();
			var filters = sectionViewModel.FilterManager;
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, sectionViewModel, Array.Empty<ProcessTask>());

			var apply1Count = 0;
			var apply2Count = 0;
			var filter1 = new TestFilterWhichDoesNotAllowMultiples();
			var filter2 = new TestFilterWhichDoesNotAllowMultiples();
			Assert("The two filters are different", !filter1.Equals(filter2));

			filter1.ApplyAction = () => apply1Count++;
			filter2.ApplyAction = () => apply2Count++;

			filters.ApplyFilter(filter1);
			AssertEquals(1, apply1Count);
			AssertEquals(0, apply2Count);
			AssertEquals(1, filters.AppliedFilters.Count());

			filters.ApplyFilter(filter2);
			AssertEquals(1, apply1Count);
			AssertEquals(1, apply2Count);
			AssertEquals(1, filters.AppliedFilters.Count());
		}

		public void TestNestedFilters_ShouldNotifyChildrenWhenUpdatingParent()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component = CreateBucket(system);
			var board = system.Boards.AddNew();
			var section1 = CreateBoardSection(component, board);
			var section2 = CreateBoardSection(component, board);
			Factory.Save();

			var boardViewModel = GetViewModel(board);

			boardViewModel.GetSections().OfType<BMBoardSectionViewModel>()
				.ForEach(vm => vm.ComponentGrid.AllocateTasks_ForTest(Factory.Load<BMBoardSection>(vm.SectionPK), vm, Array.Empty<ProcessTask>()));

			var apply1Count = 0;
			var filter1 = new TestFilter();
			filter1.ApplyAction = () => apply1Count++;

			var apply2Count = 0;
			var filter2 = new TestFilter();
			filter2.ApplyAction = () => apply2Count++;

			boardViewModel.FilterManager.ApplyFilter(filter1);
			AssertEquals(2, apply1Count);
			AssertEquals(1, boardViewModel.FilterManager.AppliedFilters.Count());
			boardViewModel.FilterManager.ApplyFilter(filter1);
			AssertEquals(4, apply1Count);
			AssertEquals(1, boardViewModel.FilterManager.AppliedFilters.Count());

			boardViewModel.FilterManager.ApplyFilter(filter2);
			AssertEquals(6, apply1Count);
			AssertEquals(2, apply2Count);
			AssertEquals(2, boardViewModel.FilterManager.AppliedFilters.Count());
		}

		public void TestClearFiltersOfType()
		{
			var parentFilterable = new DummyFilterable();
			var childFilterable = new DummyFilterable();

			parentFilterable.SetChild(childFilterable);

			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();
			var parentFilters = parentFilterable.FilterManager;
			var filters = childFilterable.FilterManager;

			var filter1 = new TestFilter();
			var filter2 = new TestFilterWhichDoesNotAllowMultiples();

			filters.ApplyFilter(filter1);
			parentFilters.ApplyFilter(filter2);
			filters.ApplyFilter(filter2);
			AssertEquals(2, filters.AppliedFilters.Count());
			AssertEquals(1, parentFilters.AppliedFilters.Count());

			childFilterable.FilterManager.ClearFiltersOfType<TestFilterWhichDoesNotAllowMultiples>();
			AssertEquals(1, filters.AppliedFilters.Count());
			AssertEquals(0, parentFilters.AppliedFilters.Count());
			AssertEquals(filter1, filters.AppliedFilters.ElementAt(0));
		}

		#region Implementation

		static BoardViewModel GetViewModel(BMBoard board)
		{
			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(board);
			boardViewModel.Build(board);

			foreach (BMBoardSectionViewModel viewModel in boardViewModel.GetSections())
			{
				SubscribeCells(viewModel);
			}

			return boardViewModel;
		}

		static void SubscribeCells(BMBoardSectionViewModel viewModel)
		{
			foreach (var cell in viewModel.ComponentGrid.Cells)
			{
				cell.ContentRefreshed += MakeContentRefreshed(viewModel);
			}
		}

		static EventHandler<ContentRefreshedEventArgs> MakeContentRefreshed(BMBoardSectionViewModel viewModel)
		{
			return (sender, eventArgs) =>
			{
				foreach (var filter in viewModel.FilterManager.AppliedAndInheritedFilters)
				{
					var cardsByCell = viewModel.ComponentGrid.CardAllocationMap.GetCardsByCells().Where(c => c.Value.Any()).ToArray();
					if (cardsByCell.Length > 0)
					{
						foreach (var pair in cardsByCell)
						{
							foreach (var card in pair.Value)
							{
								ApplyFilter(filter, card, pair.Key, viewModel);
							}
						}
					}
					else
					{
						ApplyFilter(filter, null, null, viewModel);
					}
				}
			};
		}

		static void ApplyFilter(IBoardFilter filter, ICardContent card, CellContent cell, BMBoardSectionViewModel boardSection)
		{
			var applicator = (IFilterApplicator)filter;
			if (applicator != null)
			{
				applicator.Apply(null, applicator.IsApplicable(card, cell, boardSection), null);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			strategy = new TaskTrackingAsyncBaseStrategy();
			disposables = new DisposableList(new[] { VisualBoardsTestCase.ApplyAsyncStrategy(strategy) });
		}

		protected override void TearDown()
		{
			base.TearDown();

			strategy.AwaitAll(taskToIgnore: null);
			disposables.Dispose();
		}

		DisposableList disposables;
		TaskTrackingAsyncBaseStrategy strategy;

		#endregion
	}
}
