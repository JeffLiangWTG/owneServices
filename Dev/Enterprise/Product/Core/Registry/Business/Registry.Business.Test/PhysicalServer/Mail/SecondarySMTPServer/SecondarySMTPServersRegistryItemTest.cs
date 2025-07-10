using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SecondarySMTPServersRegistryItem))]
	sealed class SecondarySMTPServersRegistryItemTest : StronglyTypedRegistryItemTestCase<SecondarySMTPServerCollection>
	{
		protected override StronglyTypedRegistryItem<SecondarySMTPServerCollection, SecondarySMTPServerCollection> GetNewRegistryItem() => new SecondarySMTPServersRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
	}
}
