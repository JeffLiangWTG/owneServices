using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoicingDefaultGatewayDepartmentsControl))]
	internal sealed class JobInvoicingDefaultGatewayDepartmentsControlTestCase : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			var collection = new JobInvoicingDefaultGatewayDepartmentsCollection();
			collection.AddNew();

			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobInvoicingDefaultGatewayDepartmentsControl)control).ReadOnly;
		}
	}
}
