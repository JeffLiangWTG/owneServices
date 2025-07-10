using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InternetAddressListRegistryItem))]
	sealed class InternetAddressListRegistryItemTest : StronglyTypedRegistryItemTestCase<InternetAddressRuleset>
	{
		protected override StronglyTypedRegistryItem<InternetAddressRuleset, InternetAddressRuleset> GetNewRegistryItem()
		{
			var defaultCollection = new InternetAddressRuleset();
			return new InternetAddressListRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, defaultCollection);
		}
	}
}
