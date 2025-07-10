using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NumberGroupSeparatorRegistryItem))]
	public class NumberGroupSeparatorRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new NumberGroupSeparatorRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
		}
	}
}
