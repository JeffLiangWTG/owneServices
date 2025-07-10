using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(SageAccountCodeMappingRegistryBusinessObjectCollection))]
	public class SageAccountCodeMappingRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<SageAccountCodeMappingRegistryBusinessObjectCollection>
	{
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override SageAccountCodeMappingRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new SageAccountCodeMappingRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SageAccountCodeMappingRegistryBusinessObject();
		}
	}
}
