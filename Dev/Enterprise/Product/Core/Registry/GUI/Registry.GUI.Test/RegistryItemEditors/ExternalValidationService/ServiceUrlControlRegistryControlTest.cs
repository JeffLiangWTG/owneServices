using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ServiceUrlControl))]
	sealed class ServiceUrlControlRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return null;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ServiceUrlControl)control).ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new ServiceUrlControl();
		}
	}
}
