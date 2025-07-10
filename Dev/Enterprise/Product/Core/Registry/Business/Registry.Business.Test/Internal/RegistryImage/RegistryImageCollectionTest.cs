using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryImageCollection))]
	sealed class RegistryImageCollectionTest : RegistryImageCollectionTest<RegistryImageCollection>
	{
		protected override RegistryImageCollection GetCollectionToTest()
		{
			return new RegistryImageCollection();
		}
	}
}
