using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SupplyChainSecurityOrganisationToUseControl))]
	sealed class SupplyChainSecurityOrganisationToUseControlTest : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SupplyChainSecurityOrganisationToUseCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SupplyChainSecurityOrganisationToUseControl)control).OrganisationsToUseGrid.ReadOnly;
		}
	}
}
