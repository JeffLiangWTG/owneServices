using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.ViewModels;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Test only")]
public class SearchViewModelTests : TestCase
{
	public void TestSearchAsyncLastCallUpdatesResults()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock.SetupSequence(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns(() => GetSearchResult(() => Task.Delay(100), ("slow", "Slow Result")))
			.Returns(() => GetSearchResult(() => Task.Delay(1), ("fast", "Fast Result")));

		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask1 = Task.Run(() => searchViewModel.SearchAsync("12"));
		var searchTask2 = Task.Run(async () =>
		{
			await Task.Delay(10);
			await searchViewModel.SearchAsync("14");
		});

		Task.WaitAll(searchTask1, searchTask2);

		// Assert
		var allItems = searchViewModel.SearchResults
			.SelectMany(s => s.Items)
			.ToList();
		AssertEquals(1, allItems.Count);
		AssertEquals("Fast Result", allItems.First().Name);

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestSearchResultsUpdatedOnDifferentSearch()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns<SearchSource.SearchParameters>((p) =>
				GetSearchResult(() => Task.Delay(100),
					("Section X", "X-Z"), // will be placed at the very end according to order
					("Section X", "X-a"),
					("Section 0 - empty", null),
					($"Section 1 of: {p.SearchValue}", $"Result 1 of: {p.SearchValue}"),
					($"Section 1 of: {p.SearchValue}", $"Result 2 of: {p.SearchValue}"),
					("Section 2 of: fix name", $"Result 3 of: {p.SearchValue}"),
					("Section 2 of: fix name", $"Result 4 of: {p.SearchValue}"),
					("Section 3 - empty", null)
				));

		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask = Task.Run(() => searchViewModel.SearchAsync("A1"));
		searchTask.Wait();

		// Assert
		var sections = searchViewModel.SearchResults;
		AssertEquals(3, sections.Count);
		AssertEquals("Section 1 of: A1", sections[0].Name);
		AssertEquals("Section 2 of: fix name", sections[1].Name);
		AssertEquals("Result 1 of: A1", sections[0].Items[0].MultilingualText);
		AssertEquals("Result 2 of: A1", sections[0].Items[1].MultilingualText);
		AssertEquals("Result 3 of: A1", sections[1].Items[0].MultilingualText);
		AssertEquals("Result 4 of: A1", sections[1].Items[1].MultilingualText);

		// Act
		searchTask = Task.Run(() => searchViewModel.SearchAsync("A2"));
		searchTask.Wait();

		// Assert
		sections = searchViewModel.SearchResults;
		AssertEquals(3, sections.Count);
		AssertEquals("Section 1 of: A2", sections[0].Name);
		AssertEquals("Section 2 of: fix name", sections[1].Name);
		AssertEquals("Result 1 of: A2", sections[0].Items[0].MultilingualText);
		AssertEquals("Result 2 of: A2", sections[0].Items[1].MultilingualText);
		AssertEquals("Result 3 of: A2", sections[1].Items[0].MultilingualText);
		AssertEquals("Result 4 of: A2", sections[1].Items[1].MultilingualText);

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestAdditionalSpaceShouldNotTriggerAnotherSearch()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns(() => GetSearchResult(() => Task.Delay(50), ("fast", "Fast Result")));

		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask1 = Task.Run(() => searchViewModel.SearchAsync(" ")); //Triggers no search
		Thread.Sleep(5);
		var searchTask2 = Task.Run(() => searchViewModel.SearchAsync("Test")); // Triggers search
		Thread.Sleep(5);
		var searchTask3 = Task.Run(() => searchViewModel.SearchAsync("Test   ")); // Triggers no search

		Task.WaitAll(searchTask1, searchTask2, searchTask3);

		// Assert
		searchSourceMock.Verify(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()), Times.Once);
		AssertNotNull(searchViewModel.SearchResults);
		AssertEquals(1, searchViewModel.SearchResults.Count);
		AssertEquals(1, searchViewModel.SearchResults[0].Items.Count);

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestLastEmptyValueClearsSearchResults()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns(() => GetSearchResult(() => Task.Delay(1), ("fast", "Fast Result")));

		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask1 = Task.Run(() => searchViewModel.SearchAsync("ABC"));
		var searchTask2 = Task.Run(async () =>
		{
			await Task.Delay(200);
			await searchViewModel.SearchAsync("123");
		});
		Task.WaitAll(searchTask1, searchTask2);

		AssertEquals(1, searchViewModel.SearchResults.Count);
		AssertEquals(1, searchViewModel.SearchResults[0].Items.Count);
		AssertEquals("Fast Result", searchViewModel.SearchResults[0].Items[0].MultilingualText);

		var searchTask3 = Task.Run(() => searchViewModel.SearchAsync("")); // Clears results
		Task.WaitAll(searchTask3);

		// Assert
		AssertEquals(0, searchViewModel.SearchResults.Count);

		// Should called twiced
		searchSourceMock.Verify(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()), Times.Exactly(2));

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestSearchAsyncShouldMergeResultsFromMiltipleSource()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();

		// Following search sources will return sections with order of following
		// Order by ResultOrder, then by Name
		// X, E, A, B, 中文, 后面
		var searchSourceMockX = CreateMockWithSections("X", 1);
		var searchSourceMockS1 = CreateMockWithSections("中文", 100);
		var searchSourceMockA = CreateMockWithSections("A", 100);
		var searchSourceMockE = CreateMockWithSections("E", 2);
		var searchSourceMockB = CreateMockWithSections("B", 100);
		var searchSourceMockS2 = CreateMockWithSections("后面", 100);

		searchViewModel.AddSearchSource(searchSourceMockX.Object);
		searchViewModel.AddSearchSource(searchSourceMockS1.Object);
		searchViewModel.AddSearchSource(searchSourceMockA.Object);
		searchViewModel.AddSearchSource(searchSourceMockE.Object);
		searchViewModel.AddSearchSource(searchSourceMockB.Object);
		searchViewModel.AddSearchSource(searchSourceMockS2.Object);

		// Act
		var searchTask = Task.Run(() => searchViewModel.SearchAsync("does not matter"));
		try
		{
			searchTask.Wait();
		}
		catch (AggregateException ex)
		{
			throw ex.InnerException;
		}
		searchTask.Wait();

		// Assert
		Assert(!searchViewModel.IsBusy);
		AssertEquals(6, searchViewModel.SearchResults.Count);

		AssertEquals("Section X", searchViewModel.SearchResults[0].Name);
		AssertEquals("Section E", searchViewModel.SearchResults[1].Name);
		AssertEquals("Section A", searchViewModel.SearchResults[2].Name);
		AssertEquals("Section B", searchViewModel.SearchResults[3].Name);
		AssertEquals("Section 中文", searchViewModel.SearchResults[4].Name);
		AssertEquals("Section 后面", searchViewModel.SearchResults[5].Name);

		var sectionA = searchViewModel.SearchResults[2];
		var sectionB = searchViewModel.SearchResults[3];
		AssertNotNull(sectionA);
		AssertNotNull(sectionB);

		AssertEquals(2, sectionA.Items.Count);
		AssertEquals(2, sectionB.Items.Count);

		static Mock<ISearchSource> CreateMockWithSections(string key, int order)
		{
			var searchSourceMockA = CreateMock(key, order);
			_ = searchSourceMockA
				.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
				.Returns(() => GetSearchResult(() =>
					Task.Delay(1),
					($"Section {key}", $"Item {key} - 1"),
					($"Section {key}", $"Item {key} - 2")));
			return searchSourceMockA;
		}

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestSearchAsyncEmptyValue()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();

		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns(() => GetSearchResult(() => Task.Delay(1)));
		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask = Task.Run(() => searchViewModel.SearchAsync(string.Empty));
		Task.WaitAll(searchTask, Task.Delay(10));

		// Assert
		Assert(!searchViewModel.SearchResults.Any());
		Assert(!searchViewModel.IsBusy);

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestSearchShouldTrimWhiteSpaces()
	{
		PerformSearchAndAssert("test ", "test");
		PerformSearchAndAssert(" test ", "test");
		PerformSearchAndAssert("		test 123 ", "test 123");

		static void PerformSearchAndAssert(string valueToSearch, string expectedSearchedValue)
		{
			// Arrange
			var searchViewModel = new SearchViewModel();
			var searchSourceMock = CreateMock();

			string searchedValue = null;
			_ = searchSourceMock
				.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
				.Callback<SearchSource.SearchParameters>(p =>
				{
					searchedValue = p.SearchValue;
					AssertEquals(expectedSearchedValue, p.SearchValue);
				})
				.Returns(() => GetSearchResult(() => Task.Delay(1)));
			searchViewModel.AddSearchSource(searchSourceMock.Object);

			// Act
			var searchTask = Task.Run(() => searchViewModel.SearchAsync(valueToSearch));
			Task.WaitAll(searchTask, Task.Delay(10));

			// Assert
			AssertEquals(expectedSearchedValue, searchedValue);
			Assert(!searchViewModel.IsBusy);

			// Cleanup
			searchViewModel.WaitingForSearchToComplete();
		}
	}

	public void TestSearchAsyncErrorHandling()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
				.Returns(() =>
				{
					if (Math.Abs(2) == 2)
					{
						throw new Exception("Test Exception");
					}
					return Task.FromResult(
						new SearchSource.SearchResults(new List<SearchResultSection>()));
				});
		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask1 = Task.Run(() => searchViewModel.SearchAsync("test"));
		Task.WaitAll(searchTask1);

		// Assert
		Assert(searchViewModel.Errors.Any());
		Assert(!searchViewModel.IsBusy);

		Assert("Error should be reported", ExceptionReporterTestListener.Instance.Count > 0);
		ExceptionReporterTestListener.Instance.Clear();
		CargoWise.Common.ErrorReporter.Clear();

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestBusyStateIsWhenSearching()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns(() => GetSearchResult(() => Task.Delay(50)));
		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask = Task.Run(() => searchViewModel.SearchAsync("test"));

		// Assert
		Task.WaitAll(Task.Delay(10));
		Assert(searchViewModel.IsBusy);
		Task.WaitAll(searchTask);
		Assert(!searchViewModel.IsBusy);

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestBusyStateIsFalseWhenSearchValueIsCleared()
	{
		// Arrange
		var searchViewModel = new SearchViewModel();
		var searchSourceMock = CreateMock();
		_ = searchSourceMock
			.Setup(s => s.SearchAsync(It.IsAny<SearchSource.SearchParameters>()))
			.Returns<SearchSource.SearchParameters>(
				p => GetSearchResult(() => Task.Delay(500, p.CancellationToken)));
		searchViewModel.AddSearchSource(searchSourceMock.Object);

		// Act
		var searchTask1 = Task.Run(() => searchViewModel.SearchAsync("test"));
		var searchTask2 = Task.Run(async () =>
		{
			await Task.Delay(20); // Wait for searchTask1 to set the busy state
			await searchViewModel.SearchAsync(string.Empty);
		});

		Thread.Sleep(5); // Wait for searchTask1 to set the search value
		Assert(searchViewModel.IsBusy); // Search value is set, so IsBusy should be true

		// Assert
		Thread.Sleep(50); // Wait for searchTask2 to clear the search value
		Assert(!searchViewModel.IsBusy); // Search value is cleared, so IsBusy should be false
		Assert(searchTask1.IsCompleted); // searchTask2 should be completed, as it should be cancelled resetting the search value

		// Cleanup
		searchViewModel.WaitingForSearchToComplete();
	}

	public void TestAddSearchSourceThrowOnInvalidValue()
	{
#if !NETFRAMEWORK
		const string expectedMessage = "Value cannot be null. (Parameter 'newSource')";
#else
		const string expectedMessage = "Value cannot be null.\r\nParameter name: newSource";
#endif
		var searchViewModel = new SearchViewModel();
		var exceptionNull = AssertExceptionThrown<ArgumentNullException>(() => searchViewModel.AddSearchSource(null));
		AssertEquals(expectedMessage, exceptionNull.Message);

		var searchSourceMock = new Mock<ISearchSource>();
		var exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceMock.Object));
		AssertEquals("Search source must have at least one filter", exception.Message);

		_ = searchSourceMock
			.Setup(s => s.Filters)
			.Returns(new ObservableCollection<SearchFilter>() { new() { Name = "dummy", IsSelected = true } });
		exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceMock.Object));
		AssertEquals("Search source must have a unique id", exception.Message);

		_ = searchSourceMock.Setup(s => s.UniqueId).Returns(string.Empty);
		exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceMock.Object));
		AssertEquals("Search source must have a unique id", exception.Message);

		_ = searchSourceMock.Setup(s => s.UniqueId).Returns("x");
		AssertNoExceptionThrown(() => searchViewModel.AddSearchSource(searchSourceMock.Object));

		exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceMock.Object));
		AssertEquals("Search source with the same id already exists", exception.Message);

		var searchSourceInstance = new SearchSource
		{
			UniqueId = "IndexSearch",
			ResultOrder = 1,
			Filters = new ObservableCollection<SearchFilter> { new SearchFilter { Name = (NoResString)"test" } },
		};
		exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceInstance));
		AssertEquals("Search source must have a search function", exception.Message);

		searchSourceInstance = new SearchSource
		{
			UniqueId = "IndexSearch",
			ResultOrder = 1,
			Filters = new ObservableCollection<SearchFilter> { new SearchFilter { Name = (NoResString)"test" } },
			SearchAsync = (_) =>
			{
				return Task.FromResult(new SearchSource.SearchResults(new List<SearchResultSection>()));
			},
		};
		AssertNoExceptionThrown(() => searchViewModel.AddSearchSource(searchSourceInstance));

		exception = AssertExceptionThrown<ArgumentException>(() => searchViewModel.AddSearchSource(searchSourceInstance));
		AssertEquals("Search source with the same id already exists", exception.Message);
	}

	static Mock<ISearchSource> CreateMock(string filterName = "Filter1", int order = 0)
	{
		var searchSourceMock = new Mock<ISearchSource>();
		_ = searchSourceMock.Setup(s => s.UniqueId).Returns(Guid.NewGuid().ToString());
		_ = searchSourceMock.Setup(s => s.ResultOrder).Returns(order);
		_ = searchSourceMock
			.Setup(s => s.Filters)
			.Returns(new ObservableCollection<SearchFilter>()
			{
					new() { Name = filterName, IsSelected = true }
			});
		return searchSourceMock;
	}

	async static Task<SearchSource.SearchResults> GetSearchResult(Func<Task> taskToRun, params (string SectionName, string ItemName)[] sections)
	{
		await taskToRun();
		var sectionsList = sections
			.GroupBy(s => s.SectionName)
			.Select(s => new SearchResultSection(
				displayName: (NoResString)s.Key,
				name: s.Key,
				items: s.Select(i => new MenuItem((NoResString)i.ItemName)).Where(i => !i.MultilingualText.IsEmpty).ToList(),
				sectionType: SectionType.GlobalSearch));
		return new SearchSource.SearchResults(sectionsList);
	}

	protected override void SetUp()
	{
		ViewModelWithNotification.IsInUnitTesting = true;
		base.SetUp();
	}
}
