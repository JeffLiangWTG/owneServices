using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(ServiceTypeRegistryControl))]
	class ServiceTypeRegistryControlTest : RegistryZUserControlTestCase
	{
		#region Implementation
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SystemProductCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			var control = (ServiceTypeRegistryControl)control1;
			return control.ModuleGrid.ReadOnly && control.SourceModuleGrid.ReadOnly && control.ProductsGrid.ReadOnly;
		}
		#endregion
	}
}
