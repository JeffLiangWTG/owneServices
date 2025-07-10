using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.BoleroEBLIntegration
{
	[TestedType(typeof(BoleroEBLForOrganisationConfigurationControl))]
	sealed class BoleroEBLForOrganisationConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BoleroEBLForOrganisationConfiguration();
		}
	}
}
