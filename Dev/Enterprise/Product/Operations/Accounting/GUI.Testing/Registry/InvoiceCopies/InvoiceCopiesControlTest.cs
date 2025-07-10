using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoiceCopiesControl))]
	class InvoiceCopiesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InvoiceCopyCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((InvoiceCopiesControl)control).InvoiceCopiesGrid_ForTestOnly.ReadOnly;
		}
	}
}
