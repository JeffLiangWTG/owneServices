using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// This ViewModel performs text searches and highlights search results by selecting the entity, given a <see cref="NetworkViewModel"/>.
	/// </summary>
	public class SearchFinderViewModel
	{
		public SearchFinderViewModel(NetworkViewModel networkViewModel)
		{
			NetworkViewModel = networkViewModel;
		}

		public NetworkViewModel NetworkViewModel { get; }
		INetworkRefresher Refresher => NetworkViewModel.Network.Refresher;
		List<INetworkEntity> SearchResults;
		string lastRefreshToken;

		internal bool HasPerformedSearch => SearchResults != null && SearchResultsAreUpToDate;

		bool SearchResultsAreUpToDate => Refresher.RefreshToken == lastRefreshToken;

		/// <summary>
		/// It gets the search results from NetworkSeacher with nodes, that matches the search term.
		/// If search is successful, the first result is highlighted. Otherwise, an error message is displayed to the user.
		/// To iterate through search results use <see cref="ShowNextResult"/>.
		/// </summary>
		internal void PerformSearch(string searchTerm)
		{
			if (!string.IsNullOrEmpty(searchTerm))
			{
				SearchResults = NetworkSearcher.TextSearch(FilterSearchTerm(searchTerm), NetworkViewModel);
				lastRefreshToken = Refresher.RefreshToken;
				if (HasPerformedSearch)
				{
					ShowNextResult();
				}
				else
				{
					DisplaySearchError(Res.GetString("310784ae-6cba-4c61-bcb9-aa674559089b", "No results were found."));
				}
			}
			else
			{
				DisplaySearchError(Res.GetString("101906c6-4e7b-4534-a525-9e15858d2084", "Please enter a search term."));
			}
		}

		Regex FilterSearchTerm(string searchTerm)
		{
			searchTerm = Regex.Escape(searchTerm.Trim(' ')).Replace("\\*", ".*");
			return new Regex(string.Format(CultureInfo.InvariantCulture, ".*{0}.*", searchTerm), RegexOptions.IgnoreCase);
		}

		void DisplaySearchError(string message)
		{
			NetworkViewModel.UserInteractionImplementor.ShowMessage(message, Res.GetString("892ddc61-b752-43ad-a85a-c1d9466446d3", "No Search Results"));
		}

		/// <summary>
		/// Highlights the next entity in the search results. If the final result was highlighted by the last call to this method, the first result will be highlighted.
		/// </summary>
		internal void ShowNextResult()
		{
			if (HasPerformedSearch)
			{
				var selectedEntity = NetworkViewModel.FirstSelectedEntity;
				var selectedIndex = 0;

				if (selectedEntity != null && SearchResults.Contains(selectedEntity))
				{
					var index = SearchResults.IndexOf(selectedEntity) + 1;
					selectedIndex = index == SearchResults.Count ? 0 : index;
				}

				if (selectedIndex < SearchResults.Count)
				{
					var entity = SearchResults[selectedIndex];
					NetworkViewModel.SelectSingleEntity(entity, shouldFocusOnSelection: true);
				}
			}
		}

		internal void ResetSearchState()
		{
			SearchResults = null;
		}

		#region Test
#if DEBUG
		public List<INetworkEntity> GetSearchResults_ForTest() => SearchResults;

		public void PerformSearch_ForTest(string searchTerm)
		{
			PerformSearch(searchTerm);
		}

		public bool HasPerformedSearch_ForTest => HasPerformedSearch;
#endif
		#endregion
	}
}
