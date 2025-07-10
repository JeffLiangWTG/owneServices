using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(InterfaceConnectorTemporarilyEnabledUntilControl))]
	sealed class InterfaceConnectorTemporarilyEnabledUntilControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InterfaceConnectorTemporarilyEnabledUntil();
		}
	}
}
