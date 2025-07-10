using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchDepartmentCodeMappingRegistryBusinessObjectCollection))]
	public class BranchDepartmentCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BranchDepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BranchDepartmentCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BranchDepartmentCodeMappingRegistryBusinessObject();
		}
	}
}
