using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Provides functionality for searching within a network.
	/// </summary>
	public static class NetworkSearcher
	{
		/// <summary>
		/// Searches for a specific text within the network.
		/// </summary>
		/// <param name="searchTerm">The term to search for.</param>
		/// <param name="networkViewModel">The network view model to search within.</param>
		/// <returns>A list of network entities <see cref="INetworkEntity"/> that match the search term.</returns>
		public static List<INetworkEntity> TextSearch(Regex searchTerm, NetworkViewModel networkViewModel)
		{
			return NetworkTextSearcher.SearchTextProperties(searchTerm, networkViewModel);
		}
	}
}
