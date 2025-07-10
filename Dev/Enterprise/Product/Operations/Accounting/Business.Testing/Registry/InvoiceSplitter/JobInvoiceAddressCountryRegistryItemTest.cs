using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoiceAddressCountryRegistryItem))]
	public class JobInvoiceAddressCountryRegistryItemTest : CodePairRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new JobInvoiceAddressCountryRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
