using System;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	sealed class LocalAdminCheckpointTest : TestCase
	{
		LocalAdminCheckpoint checkpoint;
		LocalAdminCheckpoint groupOwnerCheckpoint;
		Mock<IZSecurity> mockSecurity;

		LocalAdminCheckpoint Checkpoint
		{
			get { return checkpoint ?? (checkpoint = new LocalAdminCheckpoint("Code", MockSecurity.Object, Guid.NewGuid())); }
		}

		LocalAdminCheckpoint GroupOwnerCheckpoint
		{
			get { return groupOwnerCheckpoint ?? (groupOwnerCheckpoint = new LocalAdminCheckpoint("StaffGroupOwnerPlaceholder", MockSecurity.Object, Guid.NewGuid())); }
		}

		Mock<IZSecurity> MockSecurity
		{
			get
			{
				if (mockSecurity == null)
				{
					mockSecurity = new Mock<IZSecurity>();
					mockSecurity.Setup(m => m.CachingEnabled).Returns(false);
					mockSecurity.Setup(m => m.AddCheckPoint(It.IsAny<CheckpointLookupKey>(), It.IsAny<ISecurityCheckpoint>())).Returns(true);
				}
				return mockSecurity;
			}
		}

		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "Local Administrator", Checkpoint.HumanReadableName);
			AssertEquals("HumanReadableName", "Group Owner", GroupOwnerCheckpoint.HumanReadableName);
		}

		public void TestIsAllowed()
		{
			MockSecurity.Setup(m => m.UserPK).Returns(EnvProxy.Instance.CurrentUser.PK);

			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Granted);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
			}

			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(false))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Denied);
				AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();

				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Granted);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();

				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
				MockSecurity.Setup(m => m.IsGroupExplicitlyAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(true);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();

				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Implicit);
				MockSecurity.Setup(m => m.IsGroupExplicitlyAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(false);
				AssertEquals("IsAllowed", false, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();

				MockSecurity.Reset();
				MockSecurity.Setup(m => m.UserPK).Returns(Guid.NewGuid());
			}

			using (EnvProxy.Instance.CurrentUser.SetIsControllerOverrideForTesting(true))
			{
				MockSecurity.Setup(m => m.IsStaffAllowed(It.IsAny<ISecurityCheckpoint>())).Returns(SecurityState.Granted);
				AssertEquals("IsAllowed", true, Checkpoint.IsAllowed);
				MockSecurity.VerifyAll();
			}
		}

		public void TestVisible()
		{
			AssertEquals("Visible", false, Checkpoint.Visible);
		}
	}
}
