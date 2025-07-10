using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ServiceLevelRegistryControl))]
	public class ServiceLevelRegistryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ServiceLevelRegistryBusinessObjectCollection(Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ServiceLevelRegistryControl)control).ServiceLevelGrid.ReadOnly;
		}
	}
}
