using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(ChargeCodeMappingRegistryBusinessObjectCollection))]
	public class ChargeCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ChargeCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new ChargeCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeCodeMappingRegistryBusinessObject();
		}
	}
}
