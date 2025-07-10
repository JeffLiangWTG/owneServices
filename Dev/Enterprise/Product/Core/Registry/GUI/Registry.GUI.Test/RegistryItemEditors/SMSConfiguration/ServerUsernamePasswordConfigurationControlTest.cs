using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ServerUsernamePasswordConfigurationControl))]
	sealed class ServerUsernamePasswordConfigurationControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ServerUsernamePasswordConfiguration();
		}

		#endregion
	}
}
