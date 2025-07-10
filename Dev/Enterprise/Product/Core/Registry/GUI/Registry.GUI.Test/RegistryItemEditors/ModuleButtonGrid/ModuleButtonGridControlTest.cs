using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI.Testing
{
	abstract class ModuleButtonGridControlTest<T> : RegistryZUserControlTestCase
			where T : RegistryProxyBusinessObject, new()
	{
		#region Implementation

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ModuleButtonGridControl<T>)control).ProxyCollectionGrid.ButtonsReadOnlyExposedForTesting;
		}

		#endregion
	}
}
