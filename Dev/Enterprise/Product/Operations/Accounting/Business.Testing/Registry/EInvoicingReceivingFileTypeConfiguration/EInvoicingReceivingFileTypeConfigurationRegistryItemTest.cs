using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingReceivingFileTypeConfigurationRegistryItem))]
	public class EInvoicingReceivingFileTypeConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<EInvoicingReceivingFileTypeConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<EInvoicingReceivingFileTypeConfigurationCollection, EInvoicingReceivingFileTypeConfigurationCollection> GetNewRegistryItem()
		{
			return new EInvoicingReceivingFileTypeConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
