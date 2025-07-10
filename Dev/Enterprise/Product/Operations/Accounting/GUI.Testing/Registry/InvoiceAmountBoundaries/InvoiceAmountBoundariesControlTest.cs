using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing.Registry.InvoiceAmountBoundaries;

[TestedType(typeof(InvoiceAmountBoundariesControl))]
sealed class InvoiceAmountBoundariesControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity()
	{
		return new InvoiceAmountBoundaryCollection();
	}

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
	{
		var invoiceAmountBoundariesControl = (InvoiceAmountBoundariesControl)control;
		return invoiceAmountBoundariesControl.AmountsGrid.ReadOnly;
	}
}
