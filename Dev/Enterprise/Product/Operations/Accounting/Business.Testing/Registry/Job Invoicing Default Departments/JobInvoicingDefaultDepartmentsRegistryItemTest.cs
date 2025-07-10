using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultDepartmentsRegistryItem))]
	class JobInvoicingDefaultDepartmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<JobInvoicingDefaultDepartmentsCollection>
	{
		protected override StronglyTypedRegistryItem<JobInvoicingDefaultDepartmentsCollection, JobInvoicingDefaultDepartmentsCollection> GetNewRegistryItem()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments entry = collection.AddNew();
			entry.ConsolType = "ALL";
			entry.Department = entry.Departments[0].PK;

			return new JobInvoicingDefaultDepartmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);
		}
	}
}
