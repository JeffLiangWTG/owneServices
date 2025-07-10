using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StatusSentimentRegistryItem))]
	sealed class StatusSentimentRegitryItemTest : StronglyTypedRegistryItemTestCase<StatusSentimentCollection>
	{
		protected override StronglyTypedRegistryItem<StatusSentimentCollection, StatusSentimentCollection> GetNewRegistryItem()
		{
			return new StatusSentimentRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
