using CargoWise.Definitions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using GlowGlowRegistry = CargoWise.Definitions.GlowRegistry;

namespace Enterprise.ZArchitecture.GlowCompatibility.Test
{
	class SharedRegistryKeysTests : TestCase
	{
		public void TestEncryptedRegistrationKey()
		{
			AssertEquals(RawDataRegistry.Instance.EncryptedRegistrationKey.Name, EnterpriseRegistry.EncryptedRegistrationKey);
		}

		public void TestEnterpriseServicesRootUriKey()
		{
			AssertEquals(WebDataRegistry.Instance.RootServicesUri.Name, GlowGlowRegistry.EnterpriseServicesRootUriKey);
		}
	}
}
