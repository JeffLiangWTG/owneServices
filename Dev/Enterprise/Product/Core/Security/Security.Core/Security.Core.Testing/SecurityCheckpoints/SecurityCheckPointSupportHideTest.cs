using System;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class SecurityCheckPointSupportHideTest : TestCase
	{
		public void TestVisible()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);

			var visibleCheckPoint = new SecurityCheckPointWithConstraint("visible", (NoResString)"visible", null, security, true);
			AssertEquals(false, visibleCheckPoint.Visible);

			var hiddenCheckPoint = new SecurityCheckPointWithConstraint("hidden", (NoResString)"hidden", null, security, false);
			AssertEquals(true, hiddenCheckPoint.Visible);
		}

		public void TestIsAllowed()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);

			AssertAction(shouldHideAndNotAllowed: true, isSecurityAllowed: true, false, false);
			AssertAction(shouldHideAndNotAllowed: true, isSecurityAllowed: false, false, false);

			AssertAction(shouldHideAndNotAllowed: false, isSecurityAllowed: true, true, true);
			AssertAction(shouldHideAndNotAllowed: false, isSecurityAllowed: false, true, false);

			void AssertAction(bool shouldHideAndNotAllowed, bool isSecurityAllowed, bool expectedIsVisible, bool expectedIsAllowed)
			{
				var checkPoint = new SecurityCheckPointWithConstraint(Guid.NewGuid().ToString(), (NoResString)"CheckPoint", null, security, shouldHideAndNotAllowed);
				checkPoint.IsAllowed = isSecurityAllowed;
				AssertEquals(expectedIsVisible, checkPoint.Visible);
				AssertEquals(expectedIsAllowed, (checkPoint as ISupportAllowWithConstraint).IsAllowedWithConstraint);
			}
		}
	}
}
