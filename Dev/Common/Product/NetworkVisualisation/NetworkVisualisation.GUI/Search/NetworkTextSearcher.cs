using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	static class NetworkTextSearcher
	{
		internal static List<INetworkEntity> SearchTextProperties(Regex searchTerm, NetworkViewModel networkViewModel)
		{
			var propertiesToSearchOn = NetworkSearchConfiguration.GetSearchableProperties();

			if (propertiesToSearchOn.Any())
			{
				var properties = CreateSearchableProperties(propertiesToSearchOn);
				return SearchNetwork(searchTerm, networkViewModel, properties);
			}
			return new List<INetworkEntity>();
		}

		static List<INetworkEntity> SearchNetwork(Regex searchTerm, NetworkViewModel networkViewModel, List<PropertyInfo> propertyInfo)
		{
			var foundShapes = new List<INetworkEntity>();
			foreach (var shape in networkViewModel.Network.Entities)
			{
				var node = new NodeViewModel(shape, networkViewModel);
				if (IsAFoundShape(searchTerm, propertyInfo, node))
				{
					foundShapes.Add(shape);
				}
			}
			SortShapes(ref foundShapes);
			return foundShapes.Count > 0 ? foundShapes : null;
		}

		static void SortShapes(ref List<INetworkEntity> shapes)
		{
			shapes.Sort(delegate(INetworkEntity x, INetworkEntity y)
			{
				if (x.X == y.X)
				{
					return x.Y.CompareTo(y.Y);
				}
				return x.X.CompareTo(y.X);
			});
		}

		static bool IsAFoundShape(Regex searchTerm, List<PropertyInfo> propertiesToSearchOn, NodeViewModel node)
		{
			foreach (var property in propertiesToSearchOn)
			{
				if (IsMatch(searchTerm, property.GetValue(node)))
				{
					return true;
				}
			}
			return false;
		}

		static bool IsMatch(Regex searchTerm, object propertyValue)
		{
			return propertyValue != null && searchTerm.IsMatch(propertyValue.ToString());
		}

		static List<PropertyInfo> CreateSearchableProperties(PropertyInfo[] propertiesToSearchOn)
		{
			var searchableProperties = new List<PropertyInfo>();
			foreach (var property in propertiesToSearchOn)
			{
				searchableProperties.Add(typeof(NodeViewModel).GetProperty(property.Name));
			}
			return searchableProperties;
		}
	}
}
