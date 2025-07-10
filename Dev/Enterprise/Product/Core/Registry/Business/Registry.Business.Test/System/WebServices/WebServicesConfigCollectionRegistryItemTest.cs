using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WebServicesConfigRegistryItem))]
	sealed class WebServicesConfigCollectionRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<WebServicesConfigCollection>
	{
		protected override StronglyTypedRegistryItem<WebServicesConfigCollection, WebServicesConfigCollection> GetNewRegistryItem()
		{
			return new WebServicesConfigRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.Company, RegistryOptions.Default, new WebServicesConfigCollection());
		}
	}
}
