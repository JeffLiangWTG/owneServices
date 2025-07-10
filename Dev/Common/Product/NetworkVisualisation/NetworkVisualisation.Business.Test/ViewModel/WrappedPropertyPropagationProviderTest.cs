using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class WrappedPropertyPropagationProviderTest : TestCase
	{
		public void TestAllOfTheWrappedPropertiesOnNodeViewModelExist()
		{
			var entityTypes = new[] { typeof(IScheduledNetworkEntity), typeof(IDiagramEntity) };
			var inheritedEntityTypes = entityTypes.SelectMany(t => t.GetInterfaces());
			var entityProperties = entityTypes.Concat(inheritedEntityTypes).SelectMany(i => i.GetProperties());
			var viewModelProperties = typeof(NodeViewModel).GetProperties();

			var dictionaryOfExistingProperties = new HashSet<string>(entityProperties.Concat(viewModelProperties).Select(s => s.Name));

			var map = WrappedPropertyPropagationProvider.GetNodeViewModelPropertyMap();
			var referencedProperties = map.SelectMany(pair => pair.Value)
				.Concat(map.Keys)
				.Distinct()
				.OrderBy(t => t)
				.ToArray();

			CombineAssertions(() =>
			{
				foreach (var property in referencedProperties)
				{
					Assert("Property exists: " + property, dictionaryOfExistingProperties.Contains(property));
				}
			});
		}
	}
}
