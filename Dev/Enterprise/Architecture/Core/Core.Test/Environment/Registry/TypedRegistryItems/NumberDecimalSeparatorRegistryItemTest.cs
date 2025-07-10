using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(NumberDecimalSeparatorRegistryItem))]
	public class NumberDecimalSeparatorRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new NumberDecimalSeparatorRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsValueMandatory);
		}
	}
}
