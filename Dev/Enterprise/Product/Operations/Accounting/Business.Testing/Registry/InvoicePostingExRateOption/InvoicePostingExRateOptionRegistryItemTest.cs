using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoicePostingExRateOptionRegistryItem))]
	class InvoicePostingExRateOptionRegistryItemTest : StronglyTypedRegistryItemTestCase<InvoicePostingExRateOptionCollection>
	{
		protected override StronglyTypedRegistryItem<InvoicePostingExRateOptionCollection, InvoicePostingExRateOptionCollection> GetNewRegistryItem()
		{
			return new InvoicePostingExRateOptionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		public void TestDefaultValues()
		{
			var registryItem = GetNewRegistryItem();
			AssertEquals(2, registryItem.Value.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "LOC", "FOR" },
				registryItem.Value.Cast<InvoicePostingExRateOption>().Select(x => x.InvoiceCurrencyType));
			Assert(registryItem.Value.Cast<InvoicePostingExRateOption>().All(x => x.ExRateOption == "DEF"));
			Assert(registryItem.Value.Cast<InvoicePostingExRateOption>().All(x => x.OffSet == 0));
		}
	}
}
