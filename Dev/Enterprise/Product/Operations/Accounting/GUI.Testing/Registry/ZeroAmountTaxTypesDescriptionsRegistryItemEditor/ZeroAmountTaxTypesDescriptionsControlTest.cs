using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ZeroAmountTaxTypesDescriptionsControl))]
	public class ZeroAmountTaxTypesDescriptionsControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ZeroAmountTaxTypesDescriptionsCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ZeroAmountTaxTypesDescriptionsControl)control).zeroAmountTaxTypesDescriptionsGrid.ReadOnly;
		}
	}
}
