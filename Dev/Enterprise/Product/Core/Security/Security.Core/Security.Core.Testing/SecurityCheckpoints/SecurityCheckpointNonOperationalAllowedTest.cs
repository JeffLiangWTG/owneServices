using System;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class SecurityCheckpointNonOperationalAllowedTest : TestCase
	{
		public void TestNonOperationalUserProhibitions()
		{
			using (CurrentUserChanger.SwitchToNewUserTemporarily("sysadmin"))
			{
				SecurityCore security = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
				foreach (SecurityCheckpoint checkpoint in security.AllLoadedCheckPoints)
				{
					if ((checkpoint.Parent != null) && (checkpoint.Parent.Parent != null))
					{
						if ((security.System.IsAncestorOf(checkpoint) || security.UserAdmin.IsAncestorOf(checkpoint) ||
							// these 3 used to be under System and are allowed for non-operational
							security.PrintingSection.IsAncestorOf(checkpoint) || security.ReportSection.IsAncestorOf(checkpoint) || security.EmailSection.IsAncestorOf(checkpoint))
							&& !(checkpoint is ControllerOrExplicitAccessOnlyCheckpoint) // Non operational users should NOT have implicit access to items marked ControllerOrExplicitAccessOnlyCheckpoint
						)
						{
							AssertEquals(checkpoint.ToString() + " should be allowed.", true, checkpoint.IsAllowed);
						}
						else
						{
							AssertEquals(checkpoint.ToString() + " should not be allowed.", false, checkpoint.IsAllowed);
						}
					}
				}
			}
		}

		public void TestVisible()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);

			var visibleCheckPoint = new SecurityCheckpointNonOperationalAllowed("visible", (NoResString)"visible", null, security, true);
			AssertEquals(false, visibleCheckPoint.Visible);

			var hiddenCheckPoint = new SecurityCheckpointNonOperationalAllowed("hidden", (NoResString)"hidden", null, security, false);
			AssertEquals(true, hiddenCheckPoint.Visible);
		}

		public void TestIsAllowedWithConstraint()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, Guid.Empty, Guid.Empty, Guid.Empty);

			AssertAction(shouldHideAndNotAllowed: true, isSecurityAllowed: true, false, false);
			AssertAction(shouldHideAndNotAllowed: true, isSecurityAllowed: false, false, false);

			AssertAction(shouldHideAndNotAllowed: false, isSecurityAllowed: true, true, true);
			AssertAction(shouldHideAndNotAllowed: false, isSecurityAllowed: false, true, false);

			void AssertAction(bool shouldHideAndNotAllowed, bool isSecurityAllowed, bool expectedIsVisible, bool expectedIsAllowed)
			{
				var checkPoint = new SecurityCheckpointNonOperationalAllowed(Guid.NewGuid().ToString(), (NoResString)"CheckPoint", null, security, shouldHideAndNotAllowed);
				checkPoint.IsAllowed = isSecurityAllowed;
				AssertEquals(expectedIsVisible, checkPoint.Visible);
				AssertEquals(expectedIsAllowed, (checkPoint as ISupportAllowWithConstraint).IsAllowedWithConstraint);
			}
		}
	}
}
