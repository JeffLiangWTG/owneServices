using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ChargeCodeInvoiceTaxMessageOverrideUserControl))]
	public class ChargeCodeInvoiceTaxMessageOverrideUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ChargeCodeInvoiceTaxMessageOverrideCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ChargeCodeInvoiceTaxMessageOverrideUserControl)control).ChargeCodeInvoiceTaxMessageOverrideGrid_ForTestOnly.ReadOnly;
		}
	}
}
