using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(InvoicePostingExRateOptionControl))]
	class InvoicePostingExRateOptionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new InvoicePostingExRateOptionCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		 => ((InvoicePostingExRateOptionControl)control).InvoicePostingExRateOptionGrid.ReadOnly;
	}
}
