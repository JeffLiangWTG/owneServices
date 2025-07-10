using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeCurrencyRegistryItem))]
	public class ElectronicProcessingChargeCurrencyRegistryItemTest : StronglyTypedRegistryItemTestCase<ElectronicProcessingChargeCurrencyCollection>
	{
		protected override StronglyTypedRegistryItem<ElectronicProcessingChargeCurrencyCollection, ElectronicProcessingChargeCurrencyCollection> GetNewRegistryItem()
		{
			return new ElectronicProcessingChargeCurrencyRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new ElectronicProcessingChargeCurrencyCollection());
		}
	}
}
