using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryItemControl))]
	public class DepartmentCodeMappingRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			DepartmentCodeMappingRegistryItemControl control = (DepartmentCodeMappingRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DepartmentCodeMappingRegistryBusinessObjectCollection();
		}
	}
}
