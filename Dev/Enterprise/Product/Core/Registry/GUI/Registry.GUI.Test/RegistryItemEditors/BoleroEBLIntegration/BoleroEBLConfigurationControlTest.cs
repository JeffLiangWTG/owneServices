using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing.BoleroEBLIntegration
{
	[TestedType(typeof(BoleroEBLConfigurationControl))]
	sealed class BoleroEBLConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BoleroEBLConfiguration();
		}
	}
}
