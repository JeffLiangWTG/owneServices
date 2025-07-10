using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceDetailGroupByControl))]
	sealed class InvoiceDetailGroupByControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new InvoiceDetailGroupBy();
		}

		#endregion
	}
}
