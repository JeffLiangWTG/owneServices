using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestedType(typeof(GlowTempOrgRequiredFieldsControl))]
	class GlowTempOrgRequiredFieldsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GlowTempOrgRequiredFieldCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GlowTempOrgRequiredFieldsControl)control).GlowTempOrgRequiredFieldsGrid.ReadOnly;
		}
	}
}
