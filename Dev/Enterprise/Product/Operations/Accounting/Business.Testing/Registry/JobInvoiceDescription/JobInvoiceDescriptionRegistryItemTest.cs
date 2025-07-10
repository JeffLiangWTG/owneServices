using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoiceDescriptionRegistryItem))]
	class JobInvoiceDescriptionRegistryItemTest : StronglyTypedRegistryItemTestCase<JobInvoiceDescriptionCollection>
	{
		protected override StronglyTypedRegistryItem<JobInvoiceDescriptionCollection, JobInvoiceDescriptionCollection> GetNewRegistryItem()
		{
			return new JobInvoiceDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobInvoiceDescriptionCollection());
		}
	}
}
