using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTotalRoundingRegistryItem))]
	public class InvoiceTotalRoundingRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoiceTotalRoundingCollection>
	{
		protected override StronglyTypedRegistryItem<InvoiceTotalRoundingCollection, InvoiceTotalRoundingCollection> GetNewRegistryItem()
		{
			return new InvoiceTotalRoundingRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers);
		}
	}
}
