using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADPasswordSettings))]
	class ADPasswordSettingsTest : NonPersistentBusinessObjectTestCase
	{
		#region MaximumPasswordAge

		public void TestMaximumPasswordAge_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.MaximumPasswordAge_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMaximumPasswordAge = false;
			AssertEquals(true, bizo.MaximumPasswordAge_ReadOnly);

			bizo.EnforceMaximumPasswordAge = true;
			AssertEquals(false, bizo.MaximumPasswordAge_ReadOnly);
		}

		public void TestEnforceMaximumPasswordAge_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.EnforceMaximumPasswordAge_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.EnforceMaximumPasswordAge_ReadOnly);
		}

		#endregion

		#region MinimumPasswordAge

		public void TestMinimumPasswordAge_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.MinimumPasswordAge_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordAge = false;
			AssertEquals(true, bizo.MinimumPasswordAge_ReadOnly);

			bizo.EnforceMinimumPasswordAge = true;
			AssertEquals(false, bizo.MinimumPasswordAge_ReadOnly);
		}

		public void TestEnforceMinimumPasswordAge_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.EnforceMinimumPasswordAge_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.EnforceMinimumPasswordAge_ReadOnly);
		}

		#endregion

		#region MinimumPasswordLength

		public void TestMinimumPasswordLength_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.MinimumPasswordLength_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforceMinimumPasswordLength = false;
			AssertEquals(true, bizo.MinimumPasswordLength_ReadOnly);

			bizo.EnforceMinimumPasswordLength = true;
			AssertEquals(false, bizo.MinimumPasswordLength_ReadOnly);
		}

		public void TestEnforceMinimumPasswordLength_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.EnforceMinimumPasswordLength_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.EnforceMinimumPasswordLength_ReadOnly);
		}

		#endregion

		#region PasswordHistoryLength

		public void TestPasswordHistoryLength_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.PasswordHistoryLength_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordHistoryLength = false;
			AssertEquals(true, bizo.PasswordHistoryLength_ReadOnly);

			bizo.EnforcePasswordHistoryLength = true;
			AssertEquals(false, bizo.PasswordHistoryLength_ReadOnly);
		}

		public void TestEnforcePasswordHistoryLength_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.EnforcePasswordHistoryLength_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.EnforcePasswordHistoryLength_ReadOnly);
		}

		#endregion

		#region PasswordComplexityEnabled

		public void TestPasswordComplexityEnabled_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.PasswordComplexityEnabled_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.PasswordComplexityEnabled_ReadOnly);
		}

		#endregion

		#region PasswordLockoutPolicy

		public void TestPasswordLockoutPolicy_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.LockoutThreshold_ReadOnly);
			AssertEquals(true, bizo.LockoutObservationWindow_ReadOnly);
			AssertEquals(true, bizo.LockoutDuration_ReadOnly);
			AssertEquals(true, bizo.RequireAdminUnlock_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			bizo.EnforcePasswordLockoutPolicy = false;
			AssertEquals(true, bizo.LockoutThreshold_ReadOnly);
			AssertEquals(true, bizo.LockoutObservationWindow_ReadOnly);
			AssertEquals(true, bizo.LockoutDuration_ReadOnly);
			AssertEquals(true, bizo.RequireAdminUnlock_ReadOnly);

			bizo.EnforcePasswordLockoutPolicy = true;
			AssertEquals(false, bizo.LockoutThreshold_ReadOnly);
			AssertEquals(false, bizo.LockoutObservationWindow_ReadOnly);
			AssertEquals(false, bizo.LockoutDuration_ReadOnly);
			AssertEquals(false, bizo.RequireAdminUnlock_ReadOnly);

			bizo.RequireAdminUnlock = true;
			AssertEquals(true, bizo.LockoutDuration_ReadOnly);
		}

		public void TestEnforcePasswordLockoutPolicy_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.EnforcePasswordLockoutPolicy_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(false, bizo.EnforcePasswordLockoutPolicy_ReadOnly);
		}

		#endregion

		#region ADPasswordSettingsObjectName

		public void TestADPasswordSettingsObjectName_ReadOnly()
		{
			bizo.OverrideDomainPasswordPolicy = false;
			AssertEquals(true, bizo.ADPasswordSettingsObjectName_ReadOnly);

			bizo.OverrideDomainPasswordPolicy = true;
			AssertEquals(true, bizo.ADPasswordSettingsObjectName_ReadOnly);
		}

		public void TestADPasswordSettingsObjectName()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.Setup(r => r.Key).Returns(keyMock.Object);

			using (ObjectFactory.Substitute<IProductRegistration>(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("SYD");
				keyMock.Setup(k => k.EnterpriseCode).Returns("DMY");
				keyMock.Setup(k => k.ServerCode).Returns("WS1");
				AssertEquals("Should be hosted", true, EnvProxy.IsHostedWithCargowise);
				AssertEquals("DMYWS1_PasswordPolicy", bizo.ADPasswordSettingsObjectName);
			}
		}

		#endregion

		#region Implementation

		ADPasswordSettings bizo;

		protected override void SetUp()
		{
			base.SetUp();
			bizo = new ADPasswordSettings();
		}

		#endregion
	}
}
