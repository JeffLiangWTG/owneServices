using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientOverrideModuleInfo_Test : TestCase
	{
		public void TestTypedConstructorWithoutCountry()
		{
			Type registeredType = typeof(ClientOverrideModuleInfo_Test);
			RegistrationInfo info = new ClientOverrideModuleInfo(null, registeredType);
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
		}

		public void TestTypedConstructorWithCountry()
		{
			Type registeredType = typeof(ClientOverrideModuleInfo_Test);
			RegistrationInfo info = new ClientOverrideModuleInfo(null, registeredType, "AU");
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
			AssertEquals("AU", info.CountryCode);
		}
	}
}
