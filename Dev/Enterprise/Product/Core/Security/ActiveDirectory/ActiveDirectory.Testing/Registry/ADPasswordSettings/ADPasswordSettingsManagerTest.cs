using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	[TestedType(typeof(ADPasswordSettingsManager))]
	class ADPasswordSettingsManagerTest : NonPersistentBusinessObjectTestCase
	{
		#region Load and read

		public void TestLoadFromAD()
		{
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(new Mock<IPasswordSettings>().Object);
			manager.LoadFromAD();
			AssertEquals("OverrideDomainPasswordPolicy ", true, manager.BizO.OverrideDomainPasswordPolicy);

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns((IPasswordSettings)null);
			manager.LoadFromAD();
			AssertEquals("OverrideDomainPasswordPolicy ", false, manager.BizO.OverrideDomainPasswordPolicy);
		}

		public void TestLoadFromAD_HandleException()
		{
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new DirectoryServicesException("DirectoryServicesException bla..."));
			AssertEquals(false, manager.LoadFromAD());
			AssertEquals("Should handle DirectoryServicesException", "DirectoryServicesException bla...", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new COMException("COMException bla..."));
			AssertEquals(false, manager.LoadFromAD());
			AssertEquals("Should handle COMException", "COMException bla...", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLoadFromAD_HandleOldPolicyName()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Code = "CCC";
			var newCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			newCompanyBranch.GB_GC = newCompany.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, newCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var keyMock = new Mock<IProductRegistrationKey>();
				var productRegistrationMock = new Mock<IProductRegistration>();
				productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
				productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);
				using (ObjectFactory.Substitute(productRegistrationMock.Object))
				{
					keyMock.SetupGet(k => k.EnterpriseCode).Returns("EEE");
					keyMock.SetupGet(k => k.HostedLocation).Returns("SYD");
					keyMock.SetupGet(k => k.ServerCode).Returns("SSS");
					AssertEquals("Should be hosted", true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("EEESSS_PasswordPolicy", manager.BizO.ADPasswordSettingsObjectName);
					AssertEquals("EEECCCSSS_PasswordPolicy", manager.GetOldADPasswordSettingsObjectName());

					// Policy with old name exists but new name not - should call rename
					var psoMock = new Mock<IPasswordSettings>();
					directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings("EEECCCSSS_PasswordPolicy")).Returns(psoMock.Object);
					directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings("EEESSS_PasswordPolicy")).Returns((IPasswordSettings)null);
					manager.LoadFromAD();
					psoMock.Verify(p => p.Rename("EEESSS_PasswordPolicy"), Times.Once);
					AssertEquals("OverrideDomainPasswordPolicy ", true, manager.BizO.OverrideDomainPasswordPolicy);

					// Policy with new name exists - should not call rename
					psoMock = new Mock<IPasswordSettings>();
					directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings("EEESSS_PasswordPolicy")).Returns(psoMock.Object);
					manager.LoadFromAD();
					psoMock.Verify(p => p.Rename("EEESSS_PasswordPolicy"), Times.Never);
					AssertEquals("OverrideDomainPasswordPolicy ", true, manager.BizO.OverrideDomainPasswordPolicy);

					// Policy with old or new name doesn't exist, should not override
					psoMock = new Mock<IPasswordSettings>();
					directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings("EEECCCSSS_PasswordPolicy")).Returns((IPasswordSettings)null);
					directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings("EEESSS_PasswordPolicy")).Returns((IPasswordSettings)null);
					manager.LoadFromAD();
					psoMock.Verify(p => p.Rename("EEESSS_PasswordPolicy"), Times.Never);
					AssertEquals("OverrideDomainPasswordPolicy ", false, manager.BizO.OverrideDomainPasswordPolicy);
				}
			}
		}

		public void TestReadMaximumPasswordAge()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.MaximumPasswordAge).Returns(TimeSpan.FromDays(99));
			manager.LoadFromAD();

			AssertEquals("EnforceMaximumPasswordAge", true, manager.BizO.EnforceMaximumPasswordAge);
			AssertEquals("MaximumPasswordAge_ReadOnly", false, manager.BizO.MaximumPasswordAge_ReadOnly);
			AssertEquals("MaximumPasswordAge", 99, manager.BizO.MaximumPasswordAge);

			psoMock.SetupGet(p => p.MaximumPasswordAge).Returns(TimeSpan.FromTicks(long.MinValue));
			manager.LoadFromAD();

			AssertEquals("EnforceMaximumPasswordAge", false, manager.BizO.EnforceMaximumPasswordAge);
			AssertEquals("MaximumPasswordAge_ReadOnly", true, manager.BizO.MaximumPasswordAge_ReadOnly);
		}

		public void TestReadMinimumPasswordAge()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.MinimumPasswordAge).Returns(TimeSpan.FromDays(99));
			manager.LoadFromAD();

			AssertEquals("EnforceMinimumPasswordAge", true, manager.BizO.EnforceMinimumPasswordAge);
			AssertEquals("MinimumPasswordAge_ReadOnly", false, manager.BizO.MinimumPasswordAge_ReadOnly);
			AssertEquals("MinimumPasswordAge", 99, manager.BizO.MinimumPasswordAge);

			psoMock.SetupGet(p => p.MinimumPasswordAge).Returns(TimeSpan.FromTicks(long.MinValue));
			manager.LoadFromAD();

			AssertEquals("EnforceMinimumPasswordAge", false, manager.BizO.EnforceMinimumPasswordAge);
			AssertEquals("MinimumPasswordAge_ReadOnly", true, manager.BizO.MinimumPasswordAge_ReadOnly);
		}

		public void TestReadMinimumPasswordLength()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.MinimumPasswordLength).Returns(12);
			manager.LoadFromAD();

			AssertEquals("EnforceMinimumPasswordLength", true, manager.BizO.EnforceMinimumPasswordLength);
			AssertEquals("MinimumPasswordLength_ReadOnly", false, manager.BizO.MinimumPasswordLength_ReadOnly);
			AssertEquals("MinimumPasswordLength", 12, manager.BizO.MinimumPasswordLength);

			psoMock.SetupGet(p => p.MinimumPasswordLength).Returns(0);
			manager.LoadFromAD();

			AssertEquals("EnforceMinimumPasswordLength", false, manager.BizO.EnforceMinimumPasswordLength);
			AssertEquals("MinimumPasswordLength_ReadOnly", true, manager.BizO.MinimumPasswordLength_ReadOnly);
		}

		public void TestReadPasswordHistoryLength()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.PasswordHistoryLength).Returns(99);
			manager.LoadFromAD();

			AssertEquals("EnforcePasswordHistoryLength", true, manager.BizO.EnforcePasswordHistoryLength);
			AssertEquals("PasswordHistoryLength_ReadOnly", false, manager.BizO.PasswordHistoryLength_ReadOnly);
			AssertEquals("PasswordHistoryLength", 99, manager.BizO.PasswordHistoryLength);

			psoMock.SetupGet(p => p.PasswordHistoryLength).Returns(0);
			manager.LoadFromAD();

			AssertEquals("EnforcePasswordHistoryLength", false, manager.BizO.EnforcePasswordHistoryLength);
			AssertEquals("PasswordHistoryLength_ReadOnly", true, manager.BizO.PasswordHistoryLength_ReadOnly);
		}

		public void TestReadPasswordComplexityEnabled()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.PasswordComplexityEnabled).Returns(true);
			manager.LoadFromAD();

			AssertEquals("PasswordComplexityEnabled", true, manager.BizO.PasswordComplexityEnabled);

			psoMock.SetupGet(p => p.PasswordComplexityEnabled).Returns(false);
			manager.LoadFromAD();

			AssertEquals("PasswordComplexityEnabled", false, manager.BizO.PasswordComplexityEnabled);
		}

		public void TestReadPasswordLockoutPolicy()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			psoMock.SetupGet(p => p.LockoutThreshold).Returns(10);
			psoMock.SetupGet(p => p.LockoutObservationWindow).Returns(TimeSpan.FromMinutes(33));
			psoMock.SetupGet(p => p.LockoutDuration).Returns(TimeSpan.FromMinutes(44));

			manager.LoadFromAD();

			AssertEquals("EnforcePasswordLockoutPolicy", true, manager.BizO.EnforcePasswordLockoutPolicy);
			AssertEquals("LockoutThreshold_ReadOnly", false, manager.BizO.LockoutThreshold_ReadOnly);
			AssertEquals("LockoutObservationWindow_ReadOnly", false, manager.BizO.LockoutObservationWindow_ReadOnly);
			AssertEquals("LockoutDuration_ReadOnly", false, manager.BizO.LockoutDuration_ReadOnly);

			AssertEquals("LockoutThreshold", 10, manager.BizO.LockoutThreshold);
			AssertEquals("LockoutObservationWindow", 33, manager.BizO.LockoutObservationWindow);
			AssertEquals("LockoutDuration", 44, manager.BizO.LockoutDuration);
			AssertEquals("RequireAdminUnlock", false, manager.BizO.RequireAdminUnlock);

			//Require admin unlock
			psoMock.SetupGet(p => p.LockoutThreshold).Returns(10);
			psoMock.SetupGet(p => p.LockoutObservationWindow).Returns(TimeSpan.FromMinutes(33));
			psoMock.SetupGet(p => p.LockoutDuration).Returns(TimeSpan.FromTicks(long.MinValue));

			manager.LoadFromAD();

			AssertEquals("EnforcePasswordLockoutPolicy", true, manager.BizO.EnforcePasswordLockoutPolicy);
			AssertEquals("LockoutThreshold_ReadOnly", false, manager.BizO.LockoutThreshold_ReadOnly);
			AssertEquals("LockoutObservationWindow_ReadOnly", false, manager.BizO.LockoutObservationWindow_ReadOnly);
			AssertEquals("LockoutDuration_ReadOnly", true, manager.BizO.LockoutDuration_ReadOnly);

			AssertEquals("LockoutThreshold", 10, manager.BizO.LockoutThreshold);
			AssertEquals("LockoutObservationWindow", 33, manager.BizO.LockoutObservationWindow);
			AssertEquals("RequireAdminUnlock", true, manager.BizO.RequireAdminUnlock);

			//Not enforced
			psoMock.SetupGet(p => p.LockoutThreshold).Returns(0);

			manager.LoadFromAD();

			AssertEquals("EnforcePasswordLockoutPolicy", false, manager.BizO.EnforcePasswordLockoutPolicy);
			AssertEquals("LockoutThreshold_ReadOnly", true, manager.BizO.LockoutThreshold_ReadOnly);
			AssertEquals("LockoutObservationWindow_ReadOnly", true, manager.BizO.LockoutObservationWindow_ReadOnly);
			AssertEquals("LockoutDuration_ReadOnly", true, manager.BizO.LockoutDuration_ReadOnly);
		}

		#endregion

		#region Save and write

		public void TestSaveToAD_NotHosted()
		{
			AssertEquals("Should not be hosted", false, EnvProxy.IsHostedWithCargowise);
			manager.BizO.OverrideDomainPasswordPolicy = true;
			manager.SaveToAD();
			AssertEquals("Error for not hosted", "Only WiseCloud hosted customers can access this registry setting", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSaveToAD_NoPrivilegeToCreatePSO()
		{
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.GetPasswordSettingsContainer()).Returns((IPasswordSettingsContainer)null);
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns((IPasswordSettings)null);
			directorySearcherProviderSubstitution.DirectorySearcherMock.SetupGet(s => s.UserName).Returns("user1");
			directorySearcherProviderSubstitution.DirectorySearcherMock.SetupGet(s => s.DomainName).Returns("domainA.local");

			manager.BizO.OverrideDomainPasswordPolicy = true;
			ExecuteSaveToADAsHosted();
			AssertEquals("Hosted, no privilege", "Domain user user1 does not have write privileges to the Password Settings Container for domain domainA.local set in the registry 'Password Control -> AD Password Settings'. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSaveToAD_WCAGroupNotSet()
		{
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.GetPasswordSettingsContainer()).Returns(new Mock<IPasswordSettingsContainer>().Object);
			manager.BizO.OverrideDomainPasswordPolicy = true;
			ExecuteSaveToADAsHosted();
			AssertEquals("Error when WCA Group not set", "WiseCloud Access Security Group is not set, please contact customer support.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSaveToAD_CreateNewADPasswordSettingsObject()
		{
			ActiveDirectoryRegistry.Instance.WiseCloudAccessSecurityGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new string[] { "WCAGroup" });

			var psoMock = new Mock<IPasswordSettings>();

			var psContainerMock = new Mock<IPasswordSettingsContainer>();
			var groupEntryMock = new Mock<IGroupDirectoryEntry>();

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.GetPasswordSettingsContainer()).Returns(psContainerMock.Object);
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup("WCAGroup", string.Empty)).Returns(groupEntryMock.Object);

			psContainerMock.Setup(c => c.CreateNewChild("DMYWS1_PasswordPolicy", directorySearcherProviderSubstitution.DirectorySearcherMock.Object)).Returns(psoMock.Object);
			psoMock.SetupGet(p => p.Count).Returns(1); // pretend group is added successfully

			manager.BizO.OverrideDomainPasswordPolicy = true;
			ExecuteSaveToADAsHosted();

			psContainerMock.Verify(c => c.CreateNewChild("DMYWS1_PasswordPolicy", directorySearcherProviderSubstitution.DirectorySearcherMock.Object));
			psoMock.Verify(p => p.Add(groupEntryMock.Object));
			psoMock.Verify(p => p.CommitChanges());
		}

		public void TestSaveToAD_ShouldDeleteWhenNotOverridden()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			manager.BizO.OverrideDomainPasswordPolicy = true;
			ExecuteSaveToADAsHosted();
			psoMock.Verify(p => p.Delete(), Times.Never);

			manager.BizO.OverrideDomainPasswordPolicy = false;
			ExecuteSaveToADAsHosted();
			psoMock.Verify(p => p.Delete(), Times.Once);
		}

		public void TestWriteMaximumPasswordAge()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.EnforceMaximumPasswordAge = true;
			manager.BizO.MaximumPasswordAge = 99;
			ExecuteSaveToADAsHosted();
			AssertEquals(99d, pso.MaximumPasswordAge.TotalDays);

			manager.BizO.EnforceMaximumPasswordAge = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(long.MinValue, pso.MaximumPasswordAge.Ticks);
		}

		public void TestWriteMinimumPasswordAge()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.EnforceMinimumPasswordAge = true;
			manager.BizO.MinimumPasswordAge = 99;
			ExecuteSaveToADAsHosted();
			AssertEquals(99d, pso.MinimumPasswordAge.TotalDays);

			manager.BizO.EnforceMinimumPasswordAge = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(0d, pso.MinimumPasswordAge.TotalDays);
		}

		public void TestWriteMinimumPasswordLength()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.EnforceMinimumPasswordLength = true;
			manager.BizO.MinimumPasswordLength = 11;
			ExecuteSaveToADAsHosted();
			AssertEquals(11, pso.MinimumPasswordLength);

			manager.BizO.EnforceMinimumPasswordLength = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(0, pso.MinimumPasswordLength);
		}

		public void TestWritePasswordHistoryLength()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.EnforcePasswordHistoryLength = true;
			manager.BizO.PasswordHistoryLength = 99;
			ExecuteSaveToADAsHosted();
			AssertEquals(99, pso.PasswordHistoryLength);

			manager.BizO.EnforcePasswordHistoryLength = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(0, pso.PasswordHistoryLength);
		}

		public void TestWritePasswordComplexityEnabled()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.PasswordComplexityEnabled = true;
			ExecuteSaveToADAsHosted();
			AssertEquals(true, pso.PasswordComplexityEnabled);

			manager.BizO.PasswordComplexityEnabled = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(false, pso.PasswordComplexityEnabled);
		}

		public void TestWritePasswordLockoutPolicy()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;
			var pso = new DummyPasswordSettingsWrapper();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(pso);

			manager.BizO.EnforcePasswordLockoutPolicy = true;
			manager.BizO.LockoutThreshold = 11;
			manager.BizO.LockoutObservationWindow = 33;
			manager.BizO.RequireAdminUnlock = false;
			manager.BizO.LockoutDuration = 44;

			ExecuteSaveToADAsHosted();
			AssertEquals(11, pso.LockoutThreshold);
			AssertEquals(33d, pso.LockoutObservationWindow.TotalMinutes);
			AssertEquals(44d, pso.LockoutDuration.TotalMinutes);

			//Require Admin to unlock
			manager.BizO.RequireAdminUnlock = true;
			ExecuteSaveToADAsHosted();
			AssertEquals(11, pso.LockoutThreshold);
			AssertEquals(33d, pso.LockoutObservationWindow.TotalMinutes);
			AssertEquals("LockoutDuration should be long.MinValue when RequireAdminUnlock", long.MinValue, pso.LockoutDuration.Ticks);

			// Not enforced
			manager.BizO.EnforcePasswordLockoutPolicy = false;
			ExecuteSaveToADAsHosted();
			AssertEquals(0, pso.LockoutThreshold);
			AssertEquals("LockoutObservationWindow should be 30 mins when not EnforcePasswordLockoutPolicy", 30d, pso.LockoutObservationWindow.TotalMinutes);
			AssertEquals("LockoutDuration should be 30 mins when not EnforcePasswordLockoutPolicy", 30d, pso.LockoutDuration.TotalMinutes);
		}

		public void TestSaveToAD_HandleException()
		{
			manager.BizO.OverrideDomainPasswordPolicy = true;

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new DirectoryServicesException("DirectoryServicesException bla..."));
			AssertEquals(false, ExecuteSaveToADAsHosted());
			AssertEquals("Should handle DirectoryServicesException", "DirectoryServicesException bla...", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new COMException("COMException bla..."));
			AssertEquals(false, ExecuteSaveToADAsHosted());
			AssertEquals("Should handle COMException", "COMException bla...", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		bool ExecuteSaveToADAsHosted()
		{
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.SetupGet(k => k.HostedLocation).Returns("SYD");
				keyMock.SetupGet(k => k.EnterpriseCode).Returns("DMY");
				keyMock.SetupGet(k => k.ServerCode).Returns("WS1");

				AssertEquals("Should be hosted", true, EnvProxy.IsHostedWithCargowise);
				AssertEquals("DMYWS1_PasswordPolicy", manager.BizO.ADPasswordSettingsObjectName);

				return manager.SaveToAD();
			}
		}

		#endregion

		#region Delete

		public void TestDeleteADPasswordSettingsObject()
		{
			var psoMock = new Mock<IPasswordSettings>();
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Returns(psoMock.Object);

			Assert(manager.DeleteADPasswordSettingsObject());
			psoMock.Verify(p => p.Delete(), Times.Once);
		}

		public void TestDeleteADPasswordSettingsObject_HandleException()
		{
			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new DirectoryServicesException("DirectoryServicesException bla..."));
			AssertEquals(false, manager.DeleteADPasswordSettingsObject());
			AssertEquals("Should handle DirectoryServicesException", "DirectoryServicesException bla...", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();

			directorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindPasswordSettings(It.IsAny<string>())).Throws(new COMException("COMException bla..."));
			AssertEquals(false, manager.DeleteADPasswordSettingsObject());
			AssertEquals("Should handle COMException", "COMException bla...", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		DirectorySearcherProviderForTest directorySearcherProviderSubstitution;
		ADPasswordSettingsManager manager;

		protected override void SetUp()
		{
			base.SetUp();

			directorySearcherProviderSubstitution = new DirectorySearcherProviderForTest();
			ObjectFactory.Substitute<IDirectorySearcherProvider>(directorySearcherProviderSubstitution);

			manager = new ADPasswordSettingsManager();
		}
	}

	class DummyPasswordSettingsWrapper : DummyDirectoryEntryWrapper, IPasswordSettings
	{
		public DummyPasswordSettingsWrapper() : base("")
		{
		}

		public int PasswordSettingsPrecedence { get; set; }
		public bool PasswordReversibleEncryptionEnabled { get; set; }
		public int PasswordHistoryLength { get; set; }
		public bool PasswordComplexityEnabled { get; set; }
		public int MinimumPasswordLength { get; set; }
		public TimeSpan MinimumPasswordAge { get; set; }
		public TimeSpan MaximumPasswordAge { get; set; }
		public int LockoutThreshold { get; set; }
		public TimeSpan LockoutObservationWindow { get; set; }
		public TimeSpan LockoutDuration { get; set; }
		public string Description { get; set; }

		public PropertyValueCollection AppliesTo => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		public void Add(IDirectoryEntry entry)
		{
			throw new NotImplementedException();
		}

		public void Remove(IDirectoryEntry entry)
		{
			throw new NotImplementedException();
		}
	}
}
