using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoiceMaximumNumberOfChargesRegistryItem))]
	public class JobInvoiceMaximumNumberOfChargesRegistryItemTest : IntRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<int, int> GetNewRegistryItem()
		{
			return new JobInvoiceMaximumNumberOfChargesRegistryItem("", null, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, 0, 1, 100);
		}
	}
}
