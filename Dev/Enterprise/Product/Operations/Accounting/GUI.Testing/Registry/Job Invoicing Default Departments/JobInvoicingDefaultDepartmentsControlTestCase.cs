using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoicingDefaultDepartmentsControl))]
	internal sealed class JobInvoicingDefaultDepartmentsControlTestCase : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			collection.AddNew();

			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobInvoicingDefaultDepartmentsControl)control).ReadOnly;
		}
	}
}
