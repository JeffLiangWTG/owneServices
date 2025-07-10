using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeInvoiceTaxMessageOverrideRegistryItem))]
	class ChargeCodeInvoiceTaxMessageOverrideRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeCodeInvoiceTaxMessageOverrideCollection>
	{
		protected override StronglyTypedRegistryItem<ChargeCodeInvoiceTaxMessageOverrideCollection, ChargeCodeInvoiceTaxMessageOverrideCollection> GetNewRegistryItem()
		{
			return new ChargeCodeInvoiceTaxMessageOverrideRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
