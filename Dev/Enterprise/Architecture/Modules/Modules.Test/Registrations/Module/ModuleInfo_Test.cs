using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ModuleInfo_Test : TestCase
	{
		public void TestTypedConstructorWithoutCountry()
		{
			Type registeredType = typeof(ModuleInfo_Test);
			RegistrationInfo info = new ModuleInfo(null, registeredType);
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
		}

		public void TestTypedConstructorWithCountry()
		{
			Type registeredType = typeof(ModuleInfo_Test);
			RegistrationInfo info = new ModuleInfo(null, registeredType, "AU");
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
			AssertEquals("AU", info.CountryCode);
		}
	}
}
