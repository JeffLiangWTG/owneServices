using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolCostDefaultApportionmentMethodControl))]
	class ConsolCostDefaultApportionmentMethodControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ConsolCostDefaultApportionmentMethodConfiguration();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ConsolCostDefaultApportionmentMethodControl)control).ConsolCostDefaultApportionmentMethodGrid_ForTestOnly.ReadOnly;
		}
	}
}
