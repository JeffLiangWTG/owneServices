using System;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class SecurityCheckPointWithCustomVisibilityTest : TestCase
	{
		public void TestVisible()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);
			var visibleCheckPoint = new SecurityCheckPointWithCustomVisibility("visible", (NoResString)"visible", null, security, true);
			AssertEquals("Visible", true, visibleCheckPoint.Visible);

			var hiddenCheckPoint = new SecurityCheckPointWithCustomVisibility("hidden", (NoResString)"hidden", null, security, false);
			AssertEquals("Visible", false, hiddenCheckPoint.Visible);
		}
	}
}
