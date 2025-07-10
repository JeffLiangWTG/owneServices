using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultGatewayDepartmentsRegistryItem))]
	class JobInvoicingDefaultGatewayDepartmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<JobInvoicingDefaultGatewayDepartmentsCollection>
	{
		protected override StronglyTypedRegistryItem<JobInvoicingDefaultGatewayDepartmentsCollection, JobInvoicingDefaultGatewayDepartmentsCollection> GetNewRegistryItem()
		{
			var collection = new JobInvoicingDefaultGatewayDepartmentsCollection();
			var entry = collection.AddNew();
			entry.Direction = "IMP";
			entry.TransportMode = "AIR";
			entry.ConsolType = "AGT";
			entry.Department = entry.Departments.Single(x => x.GE_Code == "GIA").PK;

			return new JobInvoicingDefaultGatewayDepartmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);
		}
	}
}
