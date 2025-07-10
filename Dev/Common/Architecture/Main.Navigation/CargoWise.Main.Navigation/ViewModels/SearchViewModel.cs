using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

#if WINZOR
using System.Collections.Immutable;
using System.Collections.Specialized;
#endif

namespace CargoWise.Main.Navigation.ViewModels;
#nullable disable

public enum SearchResultNavigateDirection
{
	Up = -1,
	Down = 1
}

public class SearchViewModel : ViewModelWithNotification
{
	const int MinimumSearchCharactorNumbers = 1;
	readonly static TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(200);
	readonly List<ISearchSource> _searchSources = new List<ISearchSource>();
	CancellationTokenSource _searchCancellationTokenSource;
	CancellationTokenSource _typingCancellationTokenSource;

	public SearchViewModel()
	{
#if DEBUG
		if (IsInDesignMode)
		{
			// Design time data to be developed when we are moving WPF components to it's own file
			// LoadDesignData();
			// return;
		}
#endif
		_searchResults = new ObservableCollection<SearchResultSection>();
#if WINZOR
		SearchResultsToRender = SearchResults.ToImmutableArray();
		SearchResults.CollectionChanged += SearchResults_CollectionChanged;
#endif
	}

#if WINZOR
	void SearchResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		SearchResultsToRender = SearchResults.ToImmutableArray();
	}
#endif

	void TriggerDebounceSearch()
	{
		_ = Task.Run(() => DebounceSearchAsync(_searchValue));
	}

	string _searchValue;
	public string SearchValue
	{
		get => _searchValue;
		set
		{
			if (_searchValue != value)
			{
				_searchValue = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SearchValue));
				OnPropertyChanged(nameof(ShouldHideShortcuts));
				TriggerDebounceSearch();
			}
		}
	}

	bool _isBusy;
	public event EventHandler LoadResultCompleteEvent;
	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (_isBusy != value)
			{
				_isBusy = value;
				BeginInvokeOnUIThread(
					() =>
					{
						OnPropertyChanged(nameof(IsBusy));
						if (!value)
						{
							OnLoadResultComplete(EventArgs.Empty);
						}
					});
			}
		}
	}

	public int CurrentSectionIndex { get; private set; } = -1; // Tracks the current section
	public int CurrentItemIndex { get; private set; } = -1;    // Tracks the current item within a section

	public void Navigate(SearchResultNavigateDirection direction)
	{
		var result = _searchResults;
		if (result == null || result.Count == 0)
		{
			return;
		}

		if (CurrentSectionIndex == -1)
		{
			CurrentSectionIndex = 0;
			CurrentItemIndex = 0;
		}
		else if (CurrentSectionIndex < result.Count)
		{
			var currentSection = result[CurrentSectionIndex];

			CurrentItemIndex += (int)direction;

			if (CurrentItemIndex < 0)
			{
				CurrentSectionIndex--;
				if (CurrentSectionIndex >= 0)
				{
					CurrentItemIndex = result[CurrentSectionIndex].Items.Count - 1;
				}
			}
			else if (CurrentItemIndex >= currentSection.Items.Count)
			{
				CurrentSectionIndex++;
				if (CurrentSectionIndex < result.Count)
				{
					CurrentItemIndex = 0;
				}
			}
		}

		if (CurrentSectionIndex < 0)
		{
			CurrentSectionIndex = result.Count - 1;
			CurrentItemIndex = result[CurrentSectionIndex].Items.Count - 1;
		}
		else if (CurrentSectionIndex >= result.Count)
		{
			CurrentSectionIndex = 0;
			CurrentItemIndex = 0;
		}

		foreach (var section in result)
		{
			foreach (var item in section.Items)
			{
				item.IsSelected = false;
			}
		}

		if (CurrentSectionIndex >= 0 && CurrentSectionIndex < result.Count)
		{
			var section = result[CurrentSectionIndex];
			if (CurrentItemIndex >= 0 && CurrentItemIndex < section.Items.Count)
			{
				section.Items[CurrentItemIndex].IsSelected = true;
			}
		}
	}

	public bool ShouldShowNoResultsMessage => !string.IsNullOrEmpty(SearchValue) && !SearchResults.Any();
	public bool ShouldHideShortcuts => !ShouldShowNoResultsMessage;

	readonly ObservableCollection<SearchResultSection> _searchResults;
	public ObservableCollection<SearchResultSection> SearchResults => _searchResults;
	public IEnumerable<SearchResultSection> SearchResultsToRender;
	public ObservableCollection<SearchFilter> Filters => new ObservableCollection<SearchFilter>(_searchSources.SelectMany(s => s.Filters));

	public ObservableCollection<MultilingualString> Errors { get; } = new ObservableCollection<MultilingualString>();
	public ObservableCollection<MultilingualString> Warnings { get; } = new ObservableCollection<MultilingualString>();

	async Task DebounceSearchAsync(string newSearchValue)
	{
#if DEBUG
		Interlocked.Increment(ref debounceCountForDebugging);
#endif

		_typingCancellationTokenSource?.Cancel();
		_typingCancellationTokenSource = new CancellationTokenSource();
		var typingToken = _typingCancellationTokenSource.Token;

		try
		{
			var delayTask = Task.Delay(DebounceDelay, typingToken);
#if DEBUG
			TaskRegistryForTest.RegisterTask(delayTask, (NoResString)"DebounceSearch => DelayWait");
#endif
			await delayTask;
			if (!typingToken.IsCancellationRequested)
			{
				//Start actual search in a new outside UI thread
				var searchTask = Task.Run(() => SearchAsync(newSearchValue));
#if DEBUG
				TaskRegistryForTest.RegisterTask(searchTask, (NoResString)"DebounceSearch => SearchAsync");
#endif
				//await searchTask; //This is not needed, because we don't want to wait for the search to complete
			}
		}
		catch (TaskCanceledException)
		{
			// Ignore cancellation
		}
		finally
		{
#if DEBUG
			Interlocked.Decrement(ref debounceCountForDebugging);
#endif
		}
	}

	string lastSearchValue;
	public async Task SearchAsync(string newSearchValue)
	{
		if (string.IsNullOrWhiteSpace(newSearchValue) || newSearchValue.Length < MinimumSearchCharactorNumbers)
		{
			//Search value is empty, no need to search
			ResetSearch();
			_searchCancellationTokenSource?.Cancel();
			IsBusy = false;
			return;
		}

		if (newSearchValue.Length != newSearchValue.Trim().Length
			&& lastSearchValue != null
			&& newSearchValue.Trim() == lastSearchValue)
		{
			//Extra space is detected, and the search value is the same as the last search value
			//Note, we don't reset search results here, because we are still searching the same value
			return;
		}

		// Getting ready for new search
		newSearchValue = newSearchValue?.Trim();
		lastSearchValue = newSearchValue;

		ResetSearch();
		_searchCancellationTokenSource?.Cancel();
		_searchCancellationTokenSource = new CancellationTokenSource();
#if DEBUG
		Interlocked.Increment(ref searchCountForDebugging);
#endif
		var token = _searchCancellationTokenSource.Token;
		IsBusy = true;
		try
		{
			var results = new List<SearchResultSection>();
			var useFilters = _searchSources.Any(o => o.Filters.Any(f => f.IsSelected));
			var searchTasks = _searchSources.ToArray().Select(async source =>
			{
				var selectedFilters = useFilters ? source.Filters.Where(f => f.IsSelected).ToList() : source.Filters.ToList();

				if (selectedFilters.Count == 0)
				{
					return;
				}

				var parameters = new SearchSource.SearchParameters(newSearchValue, selectedFilters, token);
				var resultOrder = source.ResultOrder;

				SearchSource.SearchResults sourceResults;
				try
				{
					sourceResults = await source.SearchAsync(parameters);
				}
				catch (OperationCanceledException)
				{
					return; //Search cancelled, not continue to update UI
				}
				catch (Exception ex)
				{
					var errorMsg = ResString.GetMultilingualString("7af7babf-1be3-46ce-9b07-5a92a74c3708", "An error occurred during search.");
					sourceResults = SearchSource.SearchResults.OnError(errorMsg);

					ErrorReporter.ReportOnce(errorMsg, ex); // Log detailed error information should happened here
				}

				if (parameters.CancellationToken.IsCancellationRequested)
				{
					return; //Search cancelled, not continue to update UI
				}
				BeginInvokeOnUIThread(() => UpdateResult(sourceResults, resultOrder));
			});
			await Task.WhenAll(searchTasks);
		}
		catch (OperationCanceledException)
		{
			// Handle cancellation
		}
		finally
		{
			IsBusy = false;
#if DEBUG
			Interlocked.Decrement(ref searchCountForDebugging);
#endif
		}
	}

	readonly object lockObject_UpdateResult = new object();
	void UpdateResult(SearchSource.SearchResults sourceResults, int resultOrder)
	{
		if (_searchResults == null)
		{
			return; //Should not happen
		}

		if (sourceResults.Error != null)
		{
			Errors.Add(sourceResults.Error);
		}

		if (sourceResults.Warning != null)
		{
			Warnings.Add(sourceResults.Warning);
		}

		lock (lockObject_UpdateResult) // Lock to prevent multiple threads updating the same collection
		{
			UpdateResultImpl(sourceResults, resultOrder);
		}

		void UpdateResultImpl(SearchSource.SearchResults sourceResults, int resultOrder)
		{
			foreach (var newSection in sourceResults.Sections)
			{
				newSection.ResultOrder = resultOrder; //Set the order of the section

				var existingSection = _searchResults
					.FirstOrDefault(s => s.ResultSectionKey == newSection.ResultSectionKey);
				if (existingSection == null)
				{
					// Insert the new section in the correct order, based on the ResultOrder, then order by ResultSectionKey
					var startIndex = _searchResults
						.TakeWhile(s => s.ResultOrder < newSection.ResultOrder)
						.Count();

					// Find the correct index to insert the new section, based on Header (MultilingualText)
					var insertIndex = startIndex;
					for (var i = startIndex; i < _searchResults.Count; i++)
					{
						var item = _searchResults[i];
						if (item.ResultOrder != newSection.ResultOrder) //Different result order, no need to compare
						{
							break;
						}

						if (newSection.DisplayName.CompareTo(item.DisplayName) <= 0)
						{
							break;
						}
						insertIndex = i + 1;
					}

					_searchResults.Insert(insertIndex, newSection);
				}
				else
				{
					// TODO: update results as per search action based on each search, so the results can be merged instead of replacing
					existingSection.Items.Clear();
					foreach (var newItem in newSection.Items)
					{
						existingSection.Items.Add(newItem);
					}
				}
			}

			// Remove empty sections
			var emptySections = _searchResults
				.Where(s => s.Items.Count == 0)
				.ToList();
			foreach (var emptySection in emptySections)
			{
				_searchResults.Remove(emptySection);
			}

			OnPropertyChanged(nameof(ShouldShowNoResultsMessage));
			OnPropertyChanged(nameof(ShouldHideShortcuts));
			OnPropertyChanged(nameof(SearchResults));
		}
	}

	void ResetSearch()
	{
		BeginInvokeOnUIThread(() => ResetSearchImp());

		void ResetSearchImp()
		{
			_searchResults.Clear();
			Errors.Clear();
			Warnings.Clear();
		}
	}

#if DEBUG
	/// <summary>
	/// For test purpose only
	/// </summary>
	public void ResetSearchForUnitTesting()
	{
		_typingCancellationTokenSource?.Cancel();
		_searchCancellationTokenSource?.Cancel();

		ResetSearch();
		_searchSources.Clear();
	}

	int searchCountForDebugging;
	int debounceCountForDebugging;
	public void WaitingForSearchToComplete()
	{
		var thread = new Thread(() =>
		{
			Thread.Sleep(100);
			while (debounceCountForDebugging > 0 || searchCountForDebugging > 0)
			{
				Thread.Sleep(100);
			}
			return;
		});

		thread.Start();
		thread.Join();
	}
#endif

	public void AddSearchSource(ISearchSource newSource)
	{
		Argument.NotNull(newSource, nameof(newSource));

		if (newSource.Filters == null || newSource.Filters.Count == 0)
		{
			throw new ArgumentException("Search source must have at least one filter");
		}

		if (string.IsNullOrWhiteSpace(newSource.UniqueId))
		{
			throw new ArgumentException("Search source must have a unique id");
		}

		if (_searchSources.Any(s => s.UniqueId == newSource.UniqueId))
		{
			throw new ArgumentException("Search source with the same id already exists");
		}

		if (newSource is SearchSource instance && instance.SearchAsync == null)
		{
			throw new ArgumentException("Search source must have a search function");
		}

		foreach (var filter in newSource.Filters)
		{
			filter.PropertyChanged += (_, _) => TriggerDebounceSearch();
		}

		_searchSources.Add(newSource);
		OnPropertyChanged(nameof(Filters));
	}

	protected virtual void OnLoadResultComplete(EventArgs e)
	{
		LoadResultCompleteEvent?.Invoke(this, e);
	}
}

public interface ISearchSource
{
	public string UniqueId { get; init; }
	/// <summary>
	/// The order of the search source in the search results
	/// </summary>
	public int ResultOrder { get; init; }
	public ObservableCollection<SearchFilter> Filters { get; init; }
	public Task<SearchSource.SearchResults> SearchAsync(SearchSource.SearchParameters parameters);
}

public class SearchSource : ISearchSource
{
	public string UniqueId { get; init; }
	public int ResultOrder { get; init; }
	public ObservableCollection<SearchFilter> Filters { get; init; }
	public Func<SearchParameters, Task<SearchResults>> SearchAsync { internal get; init; }

	Task<SearchResults> ISearchSource.SearchAsync(SearchParameters parameters) => SearchAsync(parameters);

	public record SearchParameters(string SearchValue, List<SearchFilter> SelectedFilters, CancellationToken CancellationToken);
	public record SearchResults(IEnumerable<SearchResultSection> Sections, MultilingualString Error = null, MultilingualString Warning = null)
	{
		internal static SearchResults OnError(MultilingualString errorMsg)
		{
			return new SearchResults(Enumerable.Empty<SearchResultSection>(), Error: errorMsg);
		}
	}
}

public class SearchFilter : ViewModelWithNotification
{
	public string Name { get; init; }

	public SectionType[] SectionTypes { get; init; }

	bool _isSelected;
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				OnPropertyChanged();
			}
		}
	}
}
