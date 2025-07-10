using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class RegistrationInfo_Test : TestCase
	{
		public void TestTypedConstructorWithoutCountry()
		{
			Type registeredType = typeof(RegistrationInfo_Test);
			RegistrationInfo info = new RegistrationInfo(null, registeredType);
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
		}

		public void TestTypedConstructorWithCountry()
		{
			Type registeredType = typeof(RegistrationInfo_Test);
			RegistrationInfo info = new RegistrationInfo(null, registeredType, "AU");
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
			AssertEquals("AU", info.CountryCode);
		}
	}
}
