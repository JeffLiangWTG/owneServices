using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ClientSharedComponents.Registry.Testing
{
	[TestedType(typeof(TransportAndChargeCodeMappingRegistryBusinessObjectCollection))]
	public class TransportAndChargeCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TransportAndChargeCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override TransportAndChargeCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObject();
		}
	}
}
