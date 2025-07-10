using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(DepartmentCodeMappingRegistryBusinessObjectCollection))]
	public class DepartmentCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DepartmentCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DepartmentCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new DepartmentCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DepartmentCodeMappingRegistryBusinessObject();
		}
	}
}
