using System.Linq;
using System.Reflection;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Provides configuration for searching within the network visualization.
	/// </summary>
	public static class NetworkSearchConfiguration
	{
		/// <summary>
		/// Gets the properties of the <see cref="NodeViewModel"/> type that are searchable,
		/// i.e., those marked with the <see cref="NetworkDiagramSearchableAttribute"/>.
		/// </summary>
		/// <returns>An array of <see cref="PropertyInfo"/> representing the searchable properties.</returns>
		public static PropertyInfo[] GetSearchableProperties()
		{
			return typeof(NodeViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.CustomAttributes.Any(y => y.AttributeType.Name == nameof(NetworkDiagramSearchableAttribute)))
				.ToArray();
		}
	}
}
