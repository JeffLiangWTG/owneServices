using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class GlowRegistryProviderTest : TestCase
	{
		public void TestInstance()
		{
			AssertNotNull(ObjectFactory.Get<ISystemDataRegistry>());
		}
	}
}
