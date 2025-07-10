using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ControllerInfo_Test : TestCase
	{
		public void TestTypedConstructorWithoutCountry()
		{
			Type registeredType = typeof(ControllerInfo_Test);
			ControllerInfo info = new ControllerInfo(null, registeredType);
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
		}

		public void TestTypedConstructorWithCountry()
		{
			Type registeredType = typeof(ControllerInfo_Test);
			ControllerInfo info = new ControllerInfo(null, registeredType, "AU");
			AssertEquals(registeredType.FullName + "," + registeredType.Assembly, info.TypePath);
			AssertEquals("AU", info.CountryCode);
		}
	}
}
