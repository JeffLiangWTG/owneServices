using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class IsLoggedInTesterTest : TestCase
	{
		public IsLoggedInTesterTest() : base()
		{
		}

		public void TestIsLoggedIn()
		{
			Assert("System has not logged in before running unit tests", EnvProxy.Instance.CurrentUser.PK != Guid.Empty);
		}
	}
}
