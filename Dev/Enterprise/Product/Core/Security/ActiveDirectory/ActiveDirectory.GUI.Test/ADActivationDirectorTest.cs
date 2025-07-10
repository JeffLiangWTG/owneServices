using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	class ADActivationDirectorTest : TestCaseWithFactoryAndMocks
	{
		[ExpectNoExceptions]
		public void TestEnableActivation_Proceed_ShouldCallInner()
		{
			var activator = new Mock<IADIntegrationActivator>();
			var userNotification = new Mock<IUserNotification>();
			var director = new ADActivationDirector
			{
				UserNotification = userNotification.Object,
				Activator = activator.Object
			};
			var activationReportBuilder = new Mock<IActivationReportBuilder>();
			ObjectFactory.Substitute(activationReportBuilder.Object);
			var excelFile = new Mock<IExcelInterface>();
			var syncHistories = CreateSyncHistoriesForTest();

			activator.Setup(a => a.EnableIntegration(EntitiesToSync.UsersAndGroups)).Returns(syncHistories);
			activationReportBuilder.Setup(r => r.BuildReport(syncHistories)).Returns(excelFile.Object);
			userNotification.Setup(n => n.ShowConfirmation(@"Please thoroughly review the changes about to be made to your system before proceeding.", "Confirm Changes", "Continue", ZMessageBoxIcon.Question)).Returns(ZDialogResult.OK);
			userNotification.Setup(n => n.ShowConfirmation(ADActivationDirector.EnableWarning, "Enable Integration", "Enable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.OK);

			director.EnableIntegration(EntitiesToSync.UsersAndGroups);

			activator.Verify(a => a.EnableIntegration(EntitiesToSync.UsersAndGroups));
			activationReportBuilder.Verify(r => r.BuildReport(syncHistories));
			excelFile.Verify(e => e.Dispose());
			excelFile.Verify(e => e.PreviewInXl());
			userNotification.VerifyAll();
		}

		public void TestEnableActivation_Cancel_ShouldThrow()
		{
			var director = new ADActivationDirector();
			var activator = new Mock<IADIntegrationActivator>();
			var userNotification = new Mock<IUserNotification>();
			director.Activator = activator.Object;
			director.UserNotification = userNotification.Object;

			userNotification.Setup(n => n.ShowConfirmation(ADActivationDirector.EnableWarning, "Enable Integration", "Enable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.Cancel);
			AssertExceptionThrown(typeof(DirectoryServicesException), "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));

			userNotification.VerifyAll();
			activator.Verify(s => s.EnableIntegration(EntitiesToSync.UsersAndGroups), Times.Never);
		}

		public void TestEnableActivation_InvalidConfig_ShouldThrow()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(userOrganisationalUnit: TestConstants.InvalidOU, groupOrganisationalUnit: TestConstants.InvalidOU));

			var activator = new Mock<IADIntegrationActivator>();
			var userNotification = new Mock<IUserNotification>();
			var director = new ADActivationDirector
			{
				Activator = activator.Object,
				UserNotification = userNotification.Object
			};

			DirectorySearcherProviderSubstitution.DoNotMock = true;

			AssertExceptionThrown(typeof(DirectoryServicesException), @"Integration cannot be enabled.
Please disable integration, correct the issues below and save your changes before retrying to enable the integration:
- The Users' Organizational Unit for domain sand.wtg.zone in the registry setting 'System -> Staff -> Active Directory -> Domain Credentials Collection' is pointing to an invalid Organizational Unit in Active Directory. New Staff records will not be synchronized with Active Directory. Please correct this registry setting.
- The Groups' Organizational Unit for domain sand.wtg.zone in the registry setting 'System -> Staff -> Active Directory -> Domain Credentials Collection' is pointing to an invalid Organizational Unit in Active Directory. New Group records will not be synchronized with Active Directory. Please correct this registry setting.", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));

			userNotification.Verify(u => u.ShowConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZMessageBoxIcon>()), Times.Never);
			activator.Verify(a => a.EnableIntegration(It.IsAny<EntitiesToSync>()), Times.Never);
		}

		public void TestEnableActivation_ADIsMaster_AffectingLastNonOperationalOrController_ShouldAbort()
		{
			var item = new ADConfigRegistryItem("TestADConfigItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue);
			var dataType = (ADConfigRegistryDataType)item.DataType;
			var directorySearcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_Title = "Title";
			staff.GS_IsController = false;
			staff.GS_IsSystemAccount = false;
			staff.GS_IsActive = true;

			Factory.Save();

			var directoryEntryInactive = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny", active: false);

			directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntryInactive);
			AssertExceptionThrown("Should cancel validation if no user left to be Active AND Controller or Non-Operational", typeof(RegistryValidationException), "Enabling integration canceled", () => dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.ADIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty));

			var directoryEntryActive = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny", active: true);
			staff.GS_IsController = true;
			Factory.Save();

			directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntryActive);
			AssertNoExceptionThrown("Validation should go through as user 'lord.sauron' is Active and Controller", () => dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.ADIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestEnableActivation_EDIIsMaster_AffectingLastNonOperationalOrController_ShouldAbort()
		{
			var item = new ADConfigRegistryItem("TestADConfigItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue);
			var dataType = (ADConfigRegistryDataType)item.DataType;
			var directorySearcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_Title = "Title";
			staff.GS_IsController = false;
			staff.GS_IsActive = true;
			staff.GS_IsSystemAccount = false;

			Factory.Save();

			directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			AssertExceptionThrown("Should cancel validation if no user left to be Active AND Controller or Non-Operational", typeof(RegistryValidationException), "Enabling integration canceled", () => dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.EnterpriseIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty));

			staff.GS_IsController = true;
			Factory.Save();

			AssertNoExceptionThrown("Validation should go through as user 'lord.sauron' is Active and Controller", () => dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.EnterpriseIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty));
		}

		List<EntitySynchronisedEventArgs> CreateSyncHistoriesForTest()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_Title = "Title";
			staff.GS_IsActive = true;
			staff.GS_IsSystemAccount = false;
			staff.GS_IsController = true;

			var history = new EntitySynchronisedEventArgs();
			var adEntity = new ADUser(staff);
			history.Entity = adEntity;

			var syncHistories = new List<EntitySynchronisedEventArgs>
			{
				history
			};

			return syncHistories;
		}

		[ExpectNoExceptions]
		public void TestDisableActivation_Proceed_ShouldCallInner()
		{
			var activator = new Mock<IADIntegrationActivator>();
			var userNotification = new Mock<IUserNotification>();
			var director = new ADActivationDirector
			{
				Activator = activator.Object,
				UserNotification = userNotification.Object
			};

			userNotification.Setup(n => n.ShowConfirmation(ADActivationDirector.GetDisableWarning(false), "Disable Integration", "Disable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.OK);
			activator.Setup(a => a.DisableIntegration(false)).Returns(true);
			director.DisableIntegration();

			Mock.VerifyAll(userNotification, activator);
		}

		public void TestDisableActivation_Cancel_ShouldThrow()
		{
			var activator = new Mock<IADIntegrationActivator>();
			var userNotification = new Mock<IUserNotification>();
			var director = new ADActivationDirector
			{
				Activator = activator.Object,
				UserNotification = userNotification.Object
			};

			userNotification.Setup(n => n.ShowConfirmation(ADActivationDirector.GetDisableWarning(false), "Disable Integration", "Disable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.Cancel);

			AssertExceptionThrown<DirectoryServicesException>(() => director.DisableIntegration());
			activator.Verify(a => a.DisableIntegration(It.IsAny<bool>()), Times.Never);
			userNotification.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestValidateTwice_WhenHasChanges_ShouldShowSameProposedChanges()
		{
			var syncDirectorProviderForTest = new SynchronisationDirectorProviderForTest();
			ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProviderForTest);

			var item = new ADConfigRegistryItem("TestADConfigItem", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, RegistryOptions.Default, ADConfig.DefaultValue);
			var dataType = (ADConfigRegistryDataType)item.DataType;
			var directorySearcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_Title = "Title";
			staff.GS_IsController = true;
			staff.GS_IsSystemAccount = false;
			staff.GS_IsActive = true;

			Factory.Save();

			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny");
			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "Ronny");

			var activationDirector = (ADActivationDirector)dataType.ActivationDirector;

			var syncHistories1 = new List<IEnumerable<ISyncEvent>>();
			var syncHistories2 = new List<IEnumerable<ISyncEvent>>();

			directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry1);
			syncDirectorProviderForTest.SetExtraEntitySynchronisedCallback((s, e) => syncHistories1.Add(e.SyncEvents));
			dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.EnterpriseIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty);

			directorySearcher.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry2);
			syncDirectorProviderForTest.SetExtraEntitySynchronisedCallback((s, e) => syncHistories2.Add(e.SyncEvents));
			dataType.ValidateBeforeRegistryFormSave(item, new ADConfig { IsADIntegrationEnabled = true, SyncMode = SyncMode.EnterpriseIsMaster }, Guid.Empty, Guid.Empty, Guid.Empty);

			AssertContainsExactElementsInAnyOrder("Should calculate the same SyncEntities before 2 successive validations", new ListISyncEventComparer(), syncHistories1, syncHistories2);
		}

		public void TestEnableIntegrationAfterCancelation_Staff()
		{
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_Title = "Title";
			staff.GS_FullName = "Sauron the Great";
			staff.GS_EmailAddress = "Email";
			staff.GS_WorkExtension = "Ext";
			staff.GS_WorkPhone = "Work phone";
			staff.GS_MobilePhone = "Mobile phone";
			staff.GS_FaxNum = "Fax num";
			staff.GS_HomePhone = "Home phone";
			staff.GS_Pager = "Pager";
			staff.GS_UserAddress1 = "Address1";
			staff.GS_City = "City";
			staff.GS_State = "State";
			staff.GS_Postcode = "Postcode";
			staff.StaffPlainTextPassword = "booo";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;// to trigger initial sync
			Factory.Save();

			var syncDirectorProviderForTest = new SynchronisationDirectorProviderForTest();
			ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProviderForTest);

			var director = new ADActivationDirector();

			var directorySearcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;
			directorySearcher.Setup(x => x.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);

			var userNotification = new Mock<IUserNotification>();
			director.UserNotification = userNotification.Object;
			userNotification.Setup(x => x.ShowConfirmation(ADActivationDirector.EnableWarning, "Enable Integration", "Enable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.OK);
			//So it cancel after report
			userNotification.Setup(x => x.ShowConfirmation(@"Please thoroughly review the changes about to be made to your system before proceeding.
Please ensure there is at least one active Controller and one active Operational user before proceeding.", "Confirm Changes", "Continue", ZMessageBoxIcon.Question)).Returns(ZDialogResult.Cancel);

			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertStaffSyncResults(syncDirectorProviderForTest.SyncHistories, 1);

			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertStaffSyncResults(syncDirectorProviderForTest.SyncHistories, 2);

			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertStaffSyncResults(syncDirectorProviderForTest.SyncHistories, 3);
		}

		void AssertStaffSyncResults(List<EntitySynchronisedEventArgs> syncResults, int syncAttempt)
		{
			AssertEquals(1, syncResults.Count);

			var histories = syncResults[0].SyncEvents.ToArray();
			var extraMsg = string.Format("Sync attemp {0}", syncAttempt);
			AssertEquals(extraMsg, 17, histories.Length);

			AssertHistory(histories[0], "lord.sauron", "lord.sauron", "lord.sauron", GlbStaffSchema.Constants.GS_LoginName, extraMsg);
			AssertHistory(histories[1], TestConstants.Domain, string.Empty, TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName, extraMsg);
			AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked, extraMsg);
			AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive, extraMsg);
			AssertHistory(histories[4], "Lord", "Title", "Lord", GlbStaffSchema.Constants.GS_Title, extraMsg);
			AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Lord Sauron", GlbStaffSchema.Constants.GS_FullName, extraMsg);
			AssertHistory(histories[6], "sauron@mordor.com", "Email", "sauron@mordor.com", GlbStaffSchema.Constants.GS_EmailAddress, extraMsg);
			AssertHistory(histories[7], "123", "Ext", "123", GlbStaffSchema.Constants.GS_WorkExtension, extraMsg);
			AssertHistory(histories[8], "0294811111", "Work phone", "0294811111", GlbStaffSchema.Constants.GS_WorkPhone, extraMsg);
			AssertHistory(histories[9], "0412345678", "Mobile phone", "0412345678", GlbStaffSchema.Constants.GS_MobilePhone, extraMsg);
			AssertHistory(histories[10], "0294811110", "Fax num", "0294811110", GlbStaffSchema.Constants.GS_FaxNum, extraMsg);
			AssertHistory(histories[11], "0294811111", "Home phone", "0294811111", GlbStaffSchema.Constants.GS_HomePhone, extraMsg);
			AssertHistory(histories[12], "123456", "Pager", "123456", GlbStaffSchema.Constants.GS_Pager, extraMsg);
			AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "1 Barad Dur Way", GlbStaffSchema.Constants.GS_UserAddress1, extraMsg);
			AssertHistory(histories[14], "Gorgoroth", "City", "Gorgoroth", GlbStaffSchema.Constants.GS_City, extraMsg);
			AssertHistory(histories[15], "Mordor", "State", "Mordor", GlbStaffSchema.Constants.GS_State, extraMsg);
			AssertHistory(histories[16], "1111", "Postcode", "1111", GlbStaffSchema.Constants.GS_Postcode, extraMsg);
		}

		public void TestEnableIntegrationAfterCancelation_Group()
		{
			//Group Memberships setup
			//Before Sync
			//CW1: user1
			//AD : user2

			//After Sync
			//CW1: user1, user2
			//AD : user1, user2

			var syncDirectorProviderForTest = new SynchronisationDirectorProviderForTest();
			ObjectFactory.Substitute<ISynchronisationDirectorProvider>(syncDirectorProviderForTest);

			var cwGroup = Factory.New<GlbGroup>();
			cwGroup.GG_Desc = "MyGroup";
			cwGroup.GG_ActiveDirectoryObjectGuid = ZGuid.Empty; // so it will do the initial sync and follow the sync mode, which will do replication
			var adGroup = DummyDirectoryEntryWrapper.CreateGroup("MyGroup");

			var cwUser1 = cwGroup.Staff.AddNew();
			cwUser1.GS_LoginName = "user1";
			cwUser1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			var adUser1 = DummyDirectoryEntryWrapper.CreateUser($"user1@{TestConstants.Domain}");

			var cwUser2 = Factory.New<GlbStaff>();
			cwUser2.GS_LoginName = "user2";
			cwUser2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			var adUser2 = DummyDirectoryEntryWrapper.CreateUser($"user2@{TestConstants.Domain}");

			adGroup.AddMember(adUser2);

			AssertEquals(1, cwGroup.Staff.Count);
			AssertCollectionContains(cwGroup.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "user1");

			AssertEquals(1, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == $"user2@{TestConstants.Domain}");

			cwUser1.GS_ActiveDirectoryObjectGuid = adUser1.Guid;
			cwUser2.GS_ActiveDirectoryObjectGuid = adUser2.Guid;
			Factory.Save();

			var director = new ADActivationDirector();

			var directorySearcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcher.Object;
			directorySearcher.Setup(x => x.FindGroup("MyGroup", TestConstants.ValidOU)).Returns(adGroup);
			directorySearcher.Setup(x => x.FindGroup(adGroup.Guid, TestConstants.ValidOU)).Returns(adGroup);
			directorySearcher.Setup(x => x.FindUser(adUser1.Guid, TestConstants.ValidOU)).Returns(adUser1);
			directorySearcher.Setup(x => x.FindUser(adUser2.Guid, TestConstants.ValidOU)).Returns(adUser2);

			var userNotification = new Mock<IUserNotification>();
			director.UserNotification = userNotification.Object;
			userNotification.Setup(x => x.ShowConfirmation(ADActivationDirector.EnableWarning, "Enable Integration", "Enable", ZMessageBoxIcon.Exclamation)).Returns(ZDialogResult.OK);
			//So it cancel after report
			userNotification.Setup(x => x.ShowConfirmation(@"Please thoroughly review the changes about to be made to your system before proceeding.
Please ensure there is at least one active Controller and one active Operational user before proceeding.", "Confirm Changes", "Continue", ZMessageBoxIcon.Question)).Returns(ZDialogResult.Cancel);

			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertGroupSyncResults(syncDirectorProviderForTest.SyncHistories, 1);

			adGroup.GroupMembership.Clear();
			adGroup.AddMember(adUser2);
			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertGroupSyncResults(syncDirectorProviderForTest.SyncHistories, 2);

			adGroup.GroupMembership.Clear();
			adGroup.AddMember(adUser2);
			AssertExceptionThrown<DirectoryServicesException>("Unexpected error message", "Enabling integration canceled", () => director.EnableIntegration(EntitiesToSync.UsersAndGroups));
			AssertGroupSyncResults(syncDirectorProviderForTest.SyncHistories, 3);
		}

		void AssertGroupSyncResults(List<EntitySynchronisedEventArgs> syncResults, int syncAttempt)
		{
			var extraMsg = string.Format("Sync attemp {0}", syncAttempt);
			AssertEquals(extraMsg, 3, syncResults.Count);

			foreach (var syncResult in syncResults)
			{
				var histories = syncResult.SyncEvents;

				//Only check for group member sync
				var groupDescription = histories.FirstOrDefault(h => h.PropertyName == "GG_Desc");
				if (groupDescription != null)
				{
					AssertHistory(histories[4], "user2", "user1", "user1, user2", null, extraMsg);
					return;
				}
			}
			Fail("A group sync result should have been found.");
		}

		#region Implementation

		protected static void AssertHistory(ISyncEvent history, object adStartingValue, object enterpriseStartingValue, object synchronisedValue, string propertyInfoName, string extraMessage = null)
		{
			extraMessage = (extraMessage != null ? ", " + extraMessage : null);
			AssertEquals("AD starting value" + extraMessage, adStartingValue, history.ADStartingValue);
			AssertEquals("Enterprise starting value" + extraMessage, enterpriseStartingValue, history.EnterpriseStartingValue);
			AssertEquals("Synchronised value" + extraMessage, synchronisedValue, history.SynchronisedValue);
			AssertEquals("Property name" + extraMessage, propertyInfoName, history.PropertyName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
		}

		#endregion
	}

	class ListISyncEventComparer : IEqualityComparer<IEnumerable<ISyncEvent>>
	{
		public bool Equals(IEnumerable<ISyncEvent> syncEvents1, IEnumerable<ISyncEvent> syncEvents2)
		{
			var firstNotSecond = syncEvents1.Except(syncEvents2).ToList();
			var secondNotFirst = syncEvents2.Except(syncEvents1).ToList();

			return !firstNotSecond.Any() && !secondNotFirst.Any();
		}
		public int GetHashCode(IEnumerable<ISyncEvent> codeh)
		{
			return 0;
		}
	}
}
