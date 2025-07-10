using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultGatewayDepartmentsCollection))]
	public class JobInvoicingDefaultGatewayDepartmentsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobInvoicingDefaultGatewayDepartmentsCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobInvoicingDefaultGatewayDepartmentsCollection GetCollectionToTest()
		{
			return new JobInvoicingDefaultGatewayDepartmentsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var newBizO = new JobInvoicingDefaultGatewayDepartments { Direction = "ALL", TransportMode = "ALL", ConsolType = "ALL" };
			newBizO.Department = newBizO.Departments[0].PK;
			return newBizO;
		}
	}
}
