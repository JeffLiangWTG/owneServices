using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem))]
	public class ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItemTest : StronglyTypedRegistryItemTestCase<ExcludedFullyDigitalizedElectronicInvoiceData>
	{
		protected override StronglyTypedRegistryItem<ExcludedFullyDigitalizedElectronicInvoiceData, ExcludedFullyDigitalizedElectronicInvoiceData> GetNewRegistryItem()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem("", null, null, null, RegistryStorageFlags.Company, new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerAddress = true });
		}
	}
}
