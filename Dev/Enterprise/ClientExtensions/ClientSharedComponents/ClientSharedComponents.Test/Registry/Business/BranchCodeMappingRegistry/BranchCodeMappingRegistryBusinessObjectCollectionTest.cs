using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(BranchCodeMappingRegistryBusinessObjectCollection))]
	public class BranchCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BranchCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BranchCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new BranchCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BranchCodeMappingRegistryBusinessObject();
		}
	}
}
