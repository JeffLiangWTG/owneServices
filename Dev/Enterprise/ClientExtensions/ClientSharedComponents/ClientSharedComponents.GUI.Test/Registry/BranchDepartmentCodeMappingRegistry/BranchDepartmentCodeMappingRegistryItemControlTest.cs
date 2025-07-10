using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryItemControl))]
	public class BranchDepartmentCodeMappingRegistryItemControlTest : RegistryZUserControlTestCase
	{
		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control1, IBusiness businessEntity)
		{
			BranchDepartmentCodeMappingRegistryItemControl control = (BranchDepartmentCodeMappingRegistryItemControl)control1;
			return control.ReadOnly;
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObjectCollection();
		}
	}
}
