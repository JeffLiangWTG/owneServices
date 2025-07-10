using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ExcludedFullyDigitalizedElectronicInvoiceDataControl))]
	public class ExcludedFullyDigitalizedElectronicInvoiceDataControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ExcludedFullyDigitalizedElectronicInvoiceData();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ExcludedFullyDigitalizedElectronicInvoiceDataControl)control).ReadOnly;
		}
	}
}
