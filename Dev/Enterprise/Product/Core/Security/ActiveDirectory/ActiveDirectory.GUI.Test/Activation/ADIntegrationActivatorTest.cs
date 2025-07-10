using System;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class ADIntegrationActivatorTest : TestCaseWithFactoryAndMocks
	{
		[ExpectNoExceptions]
		public void TestEnableIntegration_ShouldActiveServiceTask()
		{
			var activator = new ADIntegrationActivator(Factory);
			var governorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(governorMock.Object))
			{
				activator.EnableIntegration(EntitiesToSync.UsersOnly);
				activator.SaveChanges();
				governorMock.Verify(g => g.SetServiceTaskIsActive(Constants.ActiveDirectorySynchronisationTask.Code, true), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestDisableIntegration_ShouldDeactivateServiceTask()
		{
			var activator = new ADIntegrationActivator(Factory);
			var governorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(governorMock.Object))
			{
				activator.DisableIntegration();
				activator.SaveChanges();
				governorMock.Verify(g => g.SetServiceTaskIsActive(Constants.ActiveDirectorySynchronisationTask.Code, false), Times.Once);

				governorMock.Reset();
				activator.DisableIntegration(true);
				activator.SaveChanges();
				governorMock.Verify(g => g.SetServiceTaskIsActive(Constants.ActiveDirectorySynchronisationTask.Code, true), Times.Once);
			}
		}

		public void TestDisableIntegration_ShouldClearOneOffSyncMode()
		{
			var activator = new ADIntegrationActivator(Factory);

			//Disable AD Integration
			ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AD");
			activator.DisableIntegration();
			activator.SaveChanges();
			AssertEquals("OneOffSyncMode should be cleared when disable AD Integration", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);

			//Disable Group sync only
			ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "AD");
			activator.DisableIntegration(true);
			activator.SaveChanges();
			AssertEquals("OneOffSyncMode should not be cleared when not disable AD Integration", "AD", ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
		}

		[GuiTest]
		public void TestEnableIntegration_EnterpriseMaster_ShouldUpdateAD()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var syncFactory = new BusinessObjectFactory();
			var activator = new ADIntegrationActivator(syncFactory);
			var guid = Guid.NewGuid();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Smaug", guid: guid);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Smaug";
			staff.GS_FullName = "Smaug the Great";
			Assert(staff.GS_IsActive);
			Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser("Smaug", string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser("sysadmin", string.Empty)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser(guid, string.Empty)).Returns(directoryEntry);

			activator.EnableIntegration(EntitiesToSync.UsersOnly);
			activator.SaveChanges();

			AssertEquals("Smaug the Great", directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName)]);
			Assert("Should have saved changes", staff.GS_ActiveDirectoryObjectGuid.IsValid);
		}

		[GuiTest]
		public void TestEnableIntegration_ADMaster_ShoulddNotUpdateAD()
		{
			var syncFactory = new BusinessObjectFactory();
			var activator = new ADIntegrationActivator(syncFactory);
			var directoryEntry = new Mock<IUserDirectoryEntry>();
			var guid = Guid.NewGuid();

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "Smaug";
			staff.GS_FullName = "Smaug the Great";
			Assert(staff.GS_IsActive);
			Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

			Factory.Save();

			directoryEntry.SetupGet(x => x.Guid).Returns(guid);
			directoryEntry.As<IDirectoryEntry>().SetupGet(x => x.Guid).Returns(guid);
			directoryEntry.SetupGet(x => x.UserPrincipalName).Returns("Smaug@wtg");
			directoryEntry.SetupGet(x => x[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName), 0]).Returns("Lord Smaug");

			directorySearcherMock.Setup(s => s.FindUser("Smaug", string.Empty)).Returns(directoryEntry.Object);
			directorySearcherMock.Setup(s => s.FindUser("sysadmin", string.Empty)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser(guid, string.Empty)).Returns(directoryEntry.Object);

			activator.EnableIntegration(EntitiesToSync.UsersOnly);
			Assert("Should not have saved changes", !staff.GS_ActiveDirectoryObjectGuid.IsValid);
			activator.SaveChanges();
			Assert("Should have saved changes", staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("CW1 staff should be synced with AD", "Lord Smaug", staff.GS_FullName);

			directoryEntry.Verify(x => x.CommitChanges(), Times.Never);
		}

		[GuiTest]
		public void TestEnableIntegration_WithADLinkConflict()
		{
			//Not to sync LoginName to simplify the test
			var map = new AttributeMap();
			map.MapItems.AddRange(
				new[]
				{
					new AttributeMapItem(GlbStaffSchema.GS_LoginName, ADAttributes.UserPrincipalName, false),
				});
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			var syncFactory = new BusinessObjectFactory();
			var activator = new ADIntegrationActivator(syncFactory);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "user1";
			staff1.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "user2";
			staff2.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "user3";
			staff3.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_LoginName = "user4";
			staff4.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("userX", path: "CN=userX");
			//staff1, 2 and 4 match to the same AD user, but staff3 match to a unique one
			directorySearcherMock.Setup(s => s.FindUser(staff1.GS_LoginName, string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(staff2.GS_LoginName, string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(staff3.GS_LoginName, string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateUser("user3"));
			directorySearcherMock.Setup(s => s.FindUser(staff4.GS_LoginName, string.Empty)).Returns(directoryEntry);

			var syncHistories = activator.EnableIntegration(EntitiesToSync.UsersOnly);
			AssertEquals("SyncHistories should only have one item", 1, syncHistories.Count());
			AssertNotNull("SyncHistories should have user3 only", syncHistories.FirstOrDefault(a => a.Entity.EnterpriseIdentity == "user3"));
			AssertEquals(string.Format(@"Multiple staff/group match against the same Active Directory record and will not be processed:
user1, user2, user4 <--> Active Directory record with Guid {0}", directoryEntry.Guid), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[GuiTest]
		public void TestEnableIntegration_CanLogin()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var syncFactory = new BusinessObjectFactory();
			var activator = new ADIntegrationActivator(syncFactory);
			var guid = Guid.NewGuid();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("can", guid: guid);

			var staffCanLogin = Factory.New<GlbStaff>();
			staffCanLogin.GS_IsActive = true;
			staffCanLogin.GS_LoginName = "can";
			staffCanLogin.GS_FullName = "Can Can";
			staffCanLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staffCanLogin.GS_CanLogin = true;

			var staffCannotLogin = Factory.New<GlbStaff>();
			staffCannotLogin.GS_IsActive = true;
			staffCannotLogin.GS_LoginName = "cant";
			staffCannotLogin.GS_FullName = "Can Not";
			staffCannotLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			staffCannotLogin.GS_CanLogin = false;

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser("can", string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser("sysadmin", string.Empty)).Returns((IUserDirectoryEntry)null);

			activator.EnableIntegration(EntitiesToSync.UsersOnly);
			activator.SaveChanges();

			AssertEquals("Can Login should be linked", guid, staffCanLogin.GS_ActiveDirectoryObjectGuid);
			AssertEquals("Cannot Login should not be linked", ZGuid.Empty, staffCannotLogin.GS_ActiveDirectoryObjectGuid);
		}

		public void TestDisableIntegration_All_ShouldUnLinkStaffAndGroups()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_ChangePasswordAtNextLogin = false;
			staff.StaffPlainTextPassword = "booo";
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var activator = new ADIntegrationActivator(new BusinessObjectFactory());
			activator.DisableIntegration();

			AssertNotEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			Assert(!staff.GS_ChangePasswordAtNextLogin);
			AssertNotEquals(ZBlob.Empty, staff.GS_PasswordHash);
			AssertEquals(false, staff.LocalPasswordMustBeReset);
			AssertNotEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);

			activator.SaveChanges();

			AssertEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			AssertEquals(ZBlob.Empty, staff.GS_PasswordHash);
			AssertEquals(true, staff.LocalPasswordMustBeReset);
			Assert(staff.GS_ChangePasswordAtNextLogin);
			AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
		}

		public void TestDisableIntegration_Group_ShouldUnLinkGroups()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_ChangePasswordAtNextLogin = false;
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var activator = new ADIntegrationActivator(new BusinessObjectFactory());
			activator.DisableIntegration(true);

			AssertNotEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			Assert(!staff.GS_ChangePasswordAtNextLogin);
			AssertNotEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);

			activator.SaveChanges();

			AssertNotEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
			Assert(!staff.GS_ChangePasswordAtNextLogin);
			AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
		}

		[GuiTest]
		public void TestEnableIntegration_EnterpriseIsMaster_ShouldRecordSyncHistoryCorrectly()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Lord.Sauron";
			staff.GS_FullName = "sauron";
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny");

			var syncDirectorProviderForTest = new SynchronisationDirectorProviderForTest();
			ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProviderForTest);

			var activator = new ADIntegrationActivator(Factory);

			directorySearcherMock.Setup(s => s.FindUser("Lord.Sauron", string.Empty)).Returns(directoryEntry);

			activator.EnableIntegration(EntitiesToSync.UsersAndGroups);

			var expectedSyncEvent_FullName = new SyncEvent
			{
				ADStartingValue = "Ronny",
				EnterpriseStartingValue = "sauron",
				PropertyName = GlbStaffSchema.GS_FullName.Name,
				SynchronisedValue = "sauron"
			};

			var expectedSyncEvent_LoginName = new SyncEvent
			{
				ADStartingValue = "lord.sauron",
				EnterpriseStartingValue = "lord.sauron",
				PropertyName = GlbStaffSchema.GS_LoginName.Name,
				SynchronisedValue = "lord.sauron"
			};

			AssertGreaterThanOrEqualTo(syncDirectorProviderForTest.SyncHistories.Count, 1);
			Assert(syncDirectorProviderForTest.SyncHistories.Any(syncHistory => syncHistory.SyncEvents.Contains(expectedSyncEvent_FullName)));
			Assert(syncDirectorProviderForTest.SyncHistories.Any(syncHistory => syncHistory.SyncEvents.Contains(expectedSyncEvent_LoginName)));
		}

		[GuiTest]
		public void TestEnableIntegration_ADIsMaster_ShouldRecordSyncHistoryCorrectly()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "Lord.Sauron";
			staff.GS_FullName = "sauron";
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny");

			var syncDirectorProviderForTest = new SynchronisationDirectorProviderForTest();
			ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProviderForTest);

			var activator = new ADIntegrationActivator(Factory);

			directorySearcherMock.Setup(s => s.FindUser("Lord.Sauron", string.Empty)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser("sysadmin", string.Empty)).Returns((IUserDirectoryEntry)null);

			activator.EnableIntegration(EntitiesToSync.UsersAndGroups);

			var expectedSyncEvent_FullName = new SyncEvent
			{
				ADStartingValue = "Ronny",
				EnterpriseStartingValue = "sauron",
				PropertyName = GlbStaffSchema.GS_FullName.Name,
				SynchronisedValue = "Ronny"
			};

			var expectedSyncEvent_LoginName = new SyncEvent
			{
				ADStartingValue = "lord.sauron",
				EnterpriseStartingValue = "lord.sauron",
				PropertyName = GlbStaffSchema.GS_LoginName.Name,
				SynchronisedValue = "lord.sauron"
			};

			AssertGreaterThanOrEqualTo(syncDirectorProviderForTest.SyncHistories.Count, 1);
			Assert(syncDirectorProviderForTest.SyncHistories.Any(syncHistory => syncHistory.SyncEvents.Contains(expectedSyncEvent_FullName)));
			Assert(syncDirectorProviderForTest.SyncHistories.Any(syncHistory => syncHistory.SyncEvents.Contains(expectedSyncEvent_LoginName)));
		}

		public void TestEnableIntegration_FailWhenNoStaffSecurityRight()
		{
			var staffWithRight = Helper.CreateStaffWithSecurityRights(true, "StaffViewHomeAddressDetails", "GroupsModify");
			var staffWithNoRight = Helper.CreateStaffWithSecurityRights(false, "StaffViewHomeAddressDetails", "GroupsModify");
			Factory.Save();

			var activator = new ADIntegrationActivator(Factory);

			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateUser("Dummy"));
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), string.Empty)).Returns(DummyDirectoryEntryWrapper.CreateGroup("Gummy"));

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithNoRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertExceptionThrown<SecurityAccessDeniedException>(() => activator.EnableIntegration(EntitiesToSync.UsersAndGroups));
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(staffWithRight.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				AssertNoExceptionThrown(() => activator.EnableIntegration(EntitiesToSync.UsersAndGroups));
			}
		}

		class CommonTestEntities
		{
			public readonly GlbStaff StaffOutsideOU;
			public readonly GlbStaff StaffInsideOU;
			public readonly GlbStaff StaffNoMatchInAD;

			public readonly GlbGroup GroupOutsideOU;
			public readonly GlbGroup GroupInsideOU;
			public readonly GlbGroup GroupNoMatchInAD;

			readonly BusinessObjectFactory Factory;
			readonly IDirectorySearcher Searcher = new Mock<IDirectorySearcher>().Object;
			readonly ADIntegrationActivator Activator;

			public CommonTestEntities(BusinessObjectFactory factory)
			{
				DirectorySearcherFactory.DirectorySearcherOverride_ForTest = Searcher;
				Factory = factory;
				Activator = new ADIntegrationActivator(factory);

				//Staff with matched AD user outside the OU
				StaffOutsideOU = factory.NewWithValidTestData<GlbStaff>();
				StaffOutsideOU.GS_LoginName = "He.Out";
				StaffOutsideOU.GS_FullName = "He Out";
				var userEntryOut = DummyDirectoryEntryWrapper.CreateUser(StaffOutsideOU.GS_LoginName);
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffOutsideOU.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffOutsideOU.GS_LoginName, string.Empty)).Returns(userEntryOut);

				//Staff with matched AD user inside the OU
				StaffInsideOU = factory.NewWithValidTestData<GlbStaff>();
				StaffInsideOU.GS_LoginName = "She.In";
				StaffInsideOU.GS_FullName = "She In";
				var userEntryIn = DummyDirectoryEntryWrapper.CreateUser(StaffInsideOU.GS_LoginName);
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffInsideOU.GS_LoginName, TestConstants.ValidOU)).Returns(userEntryIn);
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffInsideOU.GS_LoginName, string.Empty)).Returns(userEntryIn);

				//Staff with no matched AD user in the Domain
				StaffNoMatchInAD = factory.NewWithValidTestData<GlbStaff>();
				StaffNoMatchInAD.GS_LoginName = "NotMatched";
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffNoMatchInAD.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				Mock.Get(Searcher).Setup(x => x.FindUser(StaffNoMatchInAD.GS_LoginName, string.Empty)).Returns((IUserDirectoryEntry)null);

				//Group with matched AD group outside the OU
				GroupOutsideOU = factory.NewWithValidTestData<GlbGroup>();
				GroupOutsideOU.GG_Code = "TOG";
				GroupOutsideOU.GG_Desc = "The Outer Group";
				var groupEntryOut = DummyDirectoryEntryWrapper.CreateGroup(GroupOutsideOU.GG_Desc);
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupOutsideOU.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupOutsideOU.GG_Desc, string.Empty)).Returns(groupEntryOut);

				//Group with matched AD group inside the OU
				GroupInsideOU = factory.NewWithValidTestData<GlbGroup>();
				GroupInsideOU.GG_Code = "TIG";
				GroupInsideOU.GG_Desc = "The Inner Group";
				var groupEntryIn = DummyDirectoryEntryWrapper.CreateGroup(GroupInsideOU.GG_Desc);
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupInsideOU.GG_Desc, TestConstants.ValidOU)).Returns(groupEntryIn);
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupInsideOU.GG_Desc, string.Empty)).Returns(groupEntryIn);

				//Group with no matched AD Group in the Domain
				GroupNoMatchInAD = factory.NewWithValidTestData<GlbGroup>();
				GroupNoMatchInAD.GG_Code = "TNG";
				GroupNoMatchInAD.GG_Desc = "The No Match Group";
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupNoMatchInAD.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				Mock.Get(Searcher).Setup(x => x.FindGroup(GroupNoMatchInAD.GG_Desc, string.Empty)).Returns((IGroupDirectoryEntry)null);

				Factory.Save();

				Assert(StaffOutsideOU.GS_IsActive);
				Assert(!StaffOutsideOU.IsADLinked);

				Assert(StaffInsideOU.GS_IsActive);
				Assert(!StaffInsideOU.IsADLinked);

				Assert(StaffNoMatchInAD.GS_IsActive);
				Assert(!StaffNoMatchInAD.IsADLinked);

				Assert(GroupOutsideOU.GG_IsActive);
				Assert(!GroupOutsideOU.IsADLinked);

				Assert(GroupInsideOU.GG_IsActive);
				Assert(!GroupInsideOU.IsADLinked);

				Assert(GroupNoMatchInAD.GG_IsActive);
				Assert(!GroupNoMatchInAD.IsADLinked);
			}

			public void EnableIntegrationAndSave()
			{
				Activator.EnableIntegration(EntitiesToSync.UsersAndGroups);
				Activator.SaveChanges();
			}
		}

		public void TestEnableIntegration_ADIsMaster_ShouldActivateTheCorrectEntities()
		{
			//Arrange
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			CommonTestEntities testEntities = new CommonTestEntities(Factory);

			//Action
			testEntities.EnableIntegrationAndSave();

			//Assert
			AssertEquals("StaffOutsideOU should be deactivated", false, testEntities.StaffOutsideOU.GS_IsActive);
			AssertEquals("StaffOutsideOU should not be linked", false, testEntities.StaffOutsideOU.IsADLinked);

			AssertEquals("StaffInsideOU should be active", true, testEntities.StaffInsideOU.GS_IsActive);
			AssertEquals("StaffInsideOU should be linked", true, testEntities.StaffInsideOU.IsADLinked);

			AssertEquals("StaffNoMatchInAD should be deactivated", false, testEntities.StaffNoMatchInAD.GS_IsActive);
			AssertEquals("StaffNoMatchInAD should not be linked", false, testEntities.StaffNoMatchInAD.IsADLinked);

			AssertEquals("GroupOutsideOU should be deactivated", false, testEntities.GroupOutsideOU.GG_IsActive);
			AssertEquals("GroupOutsideOU should not be linked", false, testEntities.GroupOutsideOU.IsADLinked);

			AssertEquals("GroupInsideOU should be active", true, testEntities.GroupInsideOU.GG_IsActive);
			AssertEquals("GroupInsideOU should be linked", true, testEntities.GroupInsideOU.IsADLinked);

			AssertEquals("GroupNoMatchInAD should be deactivated", false, testEntities.GroupNoMatchInAD.GG_IsActive);
			AssertEquals("GroupNoMatchInAD should not be linked", false, testEntities.GroupNoMatchInAD.IsADLinked);

			Assert("Should not get any error during enable integration", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestEnableIntegration_EnterpriseIsMaster_ShouldActivateTheCorrectEntities()
		{
			//Arrange
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			CommonTestEntities testEntities = new CommonTestEntities(Factory);

			//Action
			testEntities.EnableIntegrationAndSave();

			//Assert
			AssertEquals("StaffOutsideOU should be active", true, testEntities.StaffOutsideOU.GS_IsActive);
			AssertEquals("StaffOutsideOU should not be linked", false, testEntities.StaffOutsideOU.IsADLinked);
			AssertEquals("StaffOutsideOU linked guid should be Invalid", ZGuid.Invalid, testEntities.StaffOutsideOU.GS_ActiveDirectoryObjectGuid);

			AssertEquals("StaffInsideOU should be active", true, testEntities.StaffInsideOU.GS_IsActive);
			AssertEquals("StaffInsideOU should be linked", true, testEntities.StaffInsideOU.IsADLinked);

			AssertEquals("StaffNoMatchInAD should be active", true, testEntities.StaffNoMatchInAD.GS_IsActive);
			AssertEquals("StaffOutsideOU should not be linked", false, testEntities.StaffOutsideOU.IsADLinked);
			AssertEquals("StaffNoMatchInAD linked guid should be Invalid", ZGuid.Invalid, testEntities.StaffNoMatchInAD.GS_ActiveDirectoryObjectGuid);

			AssertEquals("GroupOut should be active", true, testEntities.GroupOutsideOU.GG_IsActive);
			AssertEquals("GroupOut should not be linked", false, testEntities.GroupOutsideOU.IsADLinked);
			AssertEquals("GroupOutsideOU linked guid should be Invalid", ZGuid.Invalid, testEntities.GroupOutsideOU.GG_ActiveDirectoryObjectGuid);

			AssertEquals("GroupIn should be active", true, testEntities.GroupInsideOU.GG_IsActive);
			AssertEquals("GroupIn should be linked", true, testEntities.GroupInsideOU.IsADLinked);

			AssertEquals("GroupNoMatchInAD should be active", true, testEntities.GroupNoMatchInAD.GG_IsActive);
			AssertEquals("GroupNoMatchInAD should not be linked", false, testEntities.GroupNoMatchInAD.IsADLinked);
			AssertEquals("GroupNoMatchInAD linked guid should be Invalid", ZGuid.Invalid, testEntities.GroupNoMatchInAD.GG_ActiveDirectoryObjectGuid);

			Assert("Should not get any error during enable integration", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}
	}
}
