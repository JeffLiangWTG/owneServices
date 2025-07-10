using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ClientOverrideControllerInfo_Test : TestCase
	{
		public void TestTypedConstructorWithoutCountry()
		{
			Type registeredType = typeof(ClientOverrideControllerInfo_Test);
			ClientOverrideControllerInfo info = new ClientOverrideControllerInfo(null, registeredType);
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
		}

		public void TestTypedConstructorWithCountry()
		{
			Type registeredType = typeof(ClientOverrideControllerInfo_Test);
			ClientOverrideControllerInfo info = new ClientOverrideControllerInfo(null, registeredType, "AU");
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
			AssertEquals("AU", info.CountryCode);
		}
	}
}
