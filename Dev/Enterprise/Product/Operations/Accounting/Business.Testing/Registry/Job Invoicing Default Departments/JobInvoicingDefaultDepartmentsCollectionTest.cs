using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobInvoicingDefaultDepartmentsCollection))]
	public class JobInvoicingDefaultDepartmentsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobInvoicingDefaultDepartmentsCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobInvoicingDefaultDepartmentsCollection GetCollectionToTest()
		{
			return new JobInvoicingDefaultDepartmentsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobInvoicingDefaultDepartments newBizO = new JobInvoicingDefaultDepartments { ConsolType = "ALL" };
			newBizO.Department = newBizO.Departments[0].PK;

			return newBizO;
		}

		#endregion
	}
}
