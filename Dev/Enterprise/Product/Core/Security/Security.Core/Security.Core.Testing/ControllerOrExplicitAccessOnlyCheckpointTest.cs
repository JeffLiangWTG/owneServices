using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.Security.Core.Testing
{
	/// <summary>
	/// Checkpoint that only grants implicit access to Controllers
	/// All other users, operational or non-operational, much be granted access EXPLICITLY
	/// </summary>
	class ControllerOrExplicitAccessOnlyCheckpointTest : TestCaseWithFactory
	{
		ControllerOrExplicitAccessOnlyCheckpoint checkpoint;
		Mock<IZSecurity> mockSecurity;

		ControllerOrExplicitAccessOnlyCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new ControllerOrExplicitAccessOnlyCheckpoint("Code", DummyModuleIDs.Dummy.Description, parent, MockSecurity.Object)); }
		}

		Mock<IZSecurity> MockSecurity
		{
			get
			{
				if (mockSecurity == null)
				{
					mockSecurity = new Mock<IZSecurity>();
					mockSecurity.Setup(m => m.AddCheckPoint(It.IsAny<CheckpointLookupKey>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				}

				return mockSecurity;
			}
		}

		SecurityCheckpoint parent => new SecurityCheckpoint("Parent", (NoResString)"Parent", null, MockSecurity.Object);

		public void TestDisplayTextAssigned()
		{
			AssertEquals(DummyModuleIDs.Dummy.Description, Checkpoint.DisplayText);
		}

		public void TestControllerAllowed()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestStaffWithGroupIsAllowed()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
				MockSecurity.Setup(m => m.IsGroupExplicitlyAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(true);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestStaffWithRightsIsAllowed()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Granted);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestNonOperationalUserWithoutRightsIsDenied()
		{
			var nonOperationalUser = Factory.NewWithValidTestData<GlbStaff>();
			nonOperationalUser.GS_IsController = false;
			nonOperationalUser.GS_IsOperational = false;
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(nonOperationalUser.GS_LoginName))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
				MockSecurity.Setup(m => m.IsGroupExplicitlyAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(false);
				AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestImplicitIsDenied()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
				MockSecurity.Setup(m => m.IsGroupExplicitlyAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(false);
				AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestVisibleForController()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				Assert("This checkpoint should be visible under the security tree for controllers", Checkpoint.Visible);
			}
		}

		public void TestHiddenIfNotController()
		{
			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				Assert("This checkpoint should be hidden under the security tree for non controllers", !Checkpoint.Visible);
			}
		}

		public void TestOnlyExpectedItemsAreMarkedControllerOrExplicitAccessOnlyCheckpoint()
		{
			SecurityCore security = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var expectedCheckPoints = new List<ControllerOrExplicitAccessOnlyCheckpoint>
			{
				security.SystemFeatureTest
			};

			var controllerOrExplicitAccessOnlyCheckpoints = security.AllLoadedCheckPoints.Where(c => c is ControllerOrExplicitAccessOnlyCheckpoint).ToList();

			AssertEquals("Only the listed item(s) should use ControllerOrExplicitAccessOnlyCheckpoint, consider carefully before adding to the list", expectedCheckPoints.Count, controllerOrExplicitAccessOnlyCheckpoints.Count);

			// Only particularly sensitive items should be marked as ControllerOrExplicitAccessOnlyCheckpoint
			AssertContainsExactElementsInAnyOrder(expectedCheckPoints, controllerOrExplicitAccessOnlyCheckpoints);
		}
	}
}
