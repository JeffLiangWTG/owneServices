using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Synchronisation
{
	abstract class EntitySynchroniserTest : TestCaseWithFactoryAndMocks
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			Helper.PurgeAllGroups();
			Helper.PurgeAllStaff();
		}

		protected EntitySynchroniser GetSyncer() => new EntitySynchroniser(Factory);

		#endregion
	}

	abstract class EntitySynchroniserTest_Common : EntitySynchroniserTest
	{
		#region Abstract tests

		public abstract void TestSync_ActiveUnLinkedStaff_ShouldActivateAndSyncWhenMatchFound();
		public abstract void TestSync_ActiveUnLinkedStaff_WhenNoMatchFound();
		public abstract void TestSync_ActiveUnLinkedGroup_WhenNoMatchFound();
		public abstract void TestSync_ActiveUnLinkedGroup_ShouldActivateWhenMatchFound();
		public abstract void TestSync_ActiveLinkedStaff_ShouldSync();
		public abstract void TestSync_ActiveLinkedGroup_ShouldSync();
		public abstract void TestSync_ActiveLinkedGroup_ShouldReplicateGroupMembership();
		public abstract void TestSync_WithPreferredSyncMode();

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection;
		}

		protected abstract SyncMode SyncMode { get; }

		protected abstract SyncDirection SyncDirection { get; }

		protected internal static void AssertHistory(ISyncEvent history, object adStartingValue, object enterpriseStartingValue, object synchronisedValue, string propertyInfoName)
		{
			AssertEquals("AD starting value", adStartingValue, history.ADStartingValue);
			AssertEquals("Enterprise starting value", enterpriseStartingValue, history.EnterpriseStartingValue);
			AssertEquals("Synchronised value", synchronisedValue, history.SynchronisedValue);
			AssertEquals("Property name", propertyInfoName, history.PropertyName);
		}

		#endregion

		#region Shared tests

		#region EntitiesToSync

		public void TestSynchronise_UsersAndGroups()
		{
			bool syncedStaff = false, syncedGroups = false;

			var syncer = GetSyncer();
			syncer.ProgressUpdated += (s, e) =>
			{
				if ((e.TaskName ?? "").Contains("user"))
				{
					syncedStaff = true;
				}
				if ((e.TaskName ?? "").Contains("group"))
				{
					syncedGroups = true;
				}
			};

			syncer.Synchronise(EntitiesToSync.UsersAndGroups);

			Assert(syncedStaff);
			Assert(syncedGroups);
		}

		public void TestSynchronise_NoEntitiesToSync_ShouldUseRegistry_UsersAndGroups()
		{
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;

			bool syncedStaff = false, syncedGroups = false;

			var syncer = GetSyncer();
			syncer.ProgressUpdated += (s, e) =>
			{
				if ((e.TaskName ?? "").Contains("user"))
				{
					syncedStaff = true;
				}
				if ((e.TaskName ?? "").Contains("group"))
				{
					syncedGroups = true;
				}
			};

			syncer.Synchronise();

			Assert(syncedStaff);
			Assert(syncedGroups);
		}

		public void TestSynchronise_NoEntitiesToSync_ShouldUseRegistry_UsersOnly()
		{
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;

			bool syncedStaff = false, syncedGroups = false;

			var syncer = GetSyncer();
			syncer.ProgressUpdated += (s, e) =>
			{
				if ((e.TaskName ?? "").Contains("user"))
				{
					syncedStaff = true;
				}
				if ((e.TaskName ?? "").Contains("group"))
				{
					syncedGroups = true;
				}
			};

			syncer.Synchronise();

			Assert(syncedStaff);
			Assert(!syncedGroups);
		}

		#endregion

		#region Sync system accounts

		public void TestSync_ShouldNotSyncSystemStaff()
		{
			var syncer = GetSyncer();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.AddToFilter(GlbStaffSchema.GS_IsSystemAccount, true);
			var systemStaffArray = Factory.Load<GlbStaff>(query);
			AssertEquals("There should be SystemAccount GlbStaff records", true, systemStaffArray.Length > 0);

			var systemStaff = systemStaffArray[0];

			AssertNotNull(systemStaff);
			Assert(systemStaff.GS_IsActive);
			Assert(!systemStaff.GS_ActiveDirectoryObjectGuid.IsValid);

			syncer.Synchronise();

			Assert(systemStaff.GS_IsActive);
			Assert(!systemStaff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals(string.Empty, systemStaff.GS_DomainName);
		}

		public void TestSync_ShouldNotSyncSystemGroup()
		{
			var syncer = GetSyncer();
			var allUsers = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, "ALL");
			AssertNotNull(allUsers);
			Assert(allUsers.GG_IsActive);
			Assert(!allUsers.GG_ActiveDirectoryObjectGuid.IsValid);

			syncer.Synchronise();

			Assert(allUsers.GG_IsActive);
			Assert(!allUsers.GG_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals(string.Empty, allUsers.GG_DomainName);
		}

		public void TestSync_ShouldOnlySyncStaffGroup()
		{
			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "StaffGroup";
			group1.GG_Desc = "StaffGroup";
			group1.GG_IsActive = true;
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "OrgGroup";
			group2.GG_Desc = "OrgGroup";
			group2.GG_IsActive = true;
			group2.GG_Type = "ORG";
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
			Factory.Save();

			var syncer = GetSyncer();
			directorySearcherMock.Setup(s => s.FindGroup("StaffGroup", TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateGroup("StaffGroup"));
			directorySearcherMock.Setup(s => s.FindGroup("OrgGroup", TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateGroup("OrgGroup"));
			syncer.Synchronise();

			Assert(group1.GG_ActiveDirectoryObjectGuid.IsValid);
			AssertNotEquals(string.Empty, group1.GG_DomainName);

			Assert(!group2.GG_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals(string.Empty, group2.GG_DomainName);
		}

		#endregion

		#region Inactive unlinked accounts

		public void TestSync_InactiveUnLinkedStaff_ShouldNotSync()
		{
			var syncer = GetSyncer();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_IsActive = false;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

			directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("lord.sauron"));

			syncer.Synchronise();

			Assert(!staff.GS_IsActive);
			Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);
		}

		public void TestSync_InactiveUnLinkedGroup_ShouldNotSync()
		{
			var syncer = GetSyncer();
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Valar";
			group.GG_IsActive = false;
			group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
			Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

			directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateGroup("Valar"));

			syncer.Synchronise();

			Assert(!group.GG_IsActive);
			Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);
		}

		#endregion

		#region Constructor

		public void TestConstructor_NullArgs_ShouldThrow()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new EntitySynchroniser(null));
			AssertNoExceptionThrown(() => new EntitySynchroniser(Factory));
		}

		#endregion

		#region Should not activate when not syncing entity type

		public void TestSync_ActiveUnLinkedGroup_WhenNotSyncingGroups_ShouldNotActivateWhenMatchFound()
		{
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersOnly;

			var syncer = GetSyncer();
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Valar";
			Assert(group.GG_IsActive);
			Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
			directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();

			Assert(group.GG_IsActive);
			Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("Valar", group.GG_Desc);
		}

		#endregion

		#region SyncPercent

		public void TestSyncMultipleTimes_ShouldNotGoPast100Percent()
		{
			var syncer = GetSyncer();
			syncer.ProgressUpdated += (s, e) => Assert((e.OverallPercentComplete ?? 0) <= 100);
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Valar";

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
			directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();
			syncer.Synchronise();
		}

		public void TestSyncZeroEntities_ShouldUpdateWith100Percent()
		{
			var syner = GetSyncer();
			int? syncPercent = 0;

			syner.ProgressUpdated += (s, e) => syncPercent = e.OverallPercentComplete;

			syner.Synchronise();
			AssertEquals(100, syncPercent.Value);
		}

		#endregion

		#region Non-synced properties

		public void TestSync_ShouldNotSyncPropertiesMarkedAsNonSynced()
		{
			var syncer = GetSyncer();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "thom.yorke";
			staff.GS_EmailAddress = "thom@radiohead.com";
			Factory.Save();
			Assert(staff.GS_IsActive);
			Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");
			directoryEntry.SetValue(GlbStaffSchema.GS_EmailAddress, "thom@atomsforpeace.com");

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var emailMapItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_EmailAddress);
			emailMapItem.IsSynced = false;
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("thom.yorke", TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();

			Assert(staff.GS_IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("thom@radiohead.com", staff.GS_EmailAddress);
			AssertEquals("thom@atomsforpeace.com", directoryEntry.GetValue(GlbStaffSchema.GS_EmailAddress));
		}

		#endregion

		#region Commit changes

		public void TestSave_CommitChangesOnSyncedEntities()
		{
			var syncer = GetSyncer();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(DateTime.UtcNow.AddDays(-1)); // to ensure it is older than CW1

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();
			syncer.Save();

			AssertEquals(TestSave_CommitChangesOnSyncedEntities_ExpectedCommit, directoryEntry.CommitCount);
		}

		protected virtual int TestSave_CommitChangesOnSyncedEntities_ExpectedCommit => 1;

		#endregion

		#region Different GUID

		public void TestSync_WhenGUIDChanges_ShouldUpdateGUID()
		{
			var oldGUID = Guid.NewGuid();
			var newGUID = Guid.NewGuid();

			var syncer = GetSyncer();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "benedict.cumberbatch";
			staff.GS_ActiveDirectoryObjectGuid = oldGUID;
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("benedict.cumberbatch", fullName: "Benedict Cumberbatch", guid: newGUID);

			directorySearcherMock.Setup(s => s.FindUser(oldGUID, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser("benedict.cumberbatch", TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();

			AssertEquals(newGUID, staff.GS_ActiveDirectoryObjectGuid);
			AssertEquals(true, staff.GS_IsActive);

			directorySearcherMock.Verify(s => s.FindUser(oldGUID, TestConstants.ValidOU), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser("benedict.cumberbatch", TestConstants.ValidOU), Times.Once);
		}

		#endregion

		#region AD Link conflict

		public void TestSync_ADLinkConflict()
		{
			DontSyncAnything();
			var syncer = GetSyncer();
			var directoryEntry_X = DummyDirectoryEntryWrapper.CreateUser("userX", path: "CN=userX");

			var directoryEntry_Y = DummyDirectoryEntryWrapper.CreateUser("userY", path: "CN=userY");

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

			var staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_LoginName = "user5";
			staff5.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff6 = Factory.NewWithValidTestData<GlbStaff>();
			staff6.GS_LoginName = "user6";
			staff6.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff7 = Factory.NewWithValidTestData<GlbStaff>();
			staff7.GS_LoginName = "user7";
			staff7.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff8 = Factory.NewWithValidTestData<GlbStaff>();
			staff8.GS_LoginName = "user8";
			staff8.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			Factory.Save();

			//staff 1, 2, 4 match to the same AD user X, but staff 3 match to a unique one
			directorySearcherMock.Setup(s => s.FindUser(staff1.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry_X);
			directorySearcherMock.Setup(s => s.FindUser(staff2.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry_X);
			directorySearcherMock.Setup(s => s.FindUser(staff4.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry_X);
			directorySearcherMock.Setup(s => s.FindUser(staff3.GS_LoginName, TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("user3"));

			//staff 7, 8 match to another AD user Y
			directorySearcherMock.Setup(s => s.FindUser(staff7.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry_Y);
			directorySearcherMock.Setup(s => s.FindUser(staff8.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry_Y);

			syncer.Synchronise();

			AssertContains(string.Format(@"user1, user2, user4 <--> Active Directory record with Guid {0}", directoryEntry_X.Guid), UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains(string.Format(@"user7, user8 <--> Active Directory record with Guid {0}", directoryEntry_Y.Guid), UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("user1 should not be saved", false, staff1.HasChanges);
			AssertEquals("user2 should not be saved", false, staff2.HasChanges);
			AssertEquals("user3 should be saved", true, staff3.HasChanges);
			AssertEquals("user4 should not be saved", false, staff4.HasChanges);
			AssertEquals("user5 should be saved", true, staff5.HasChanges);
			AssertEquals("user6 should be saved", true, staff6.HasChanges);
			AssertEquals("user7 should not be saved", false, staff7.HasChanges);
			AssertEquals("user8 should not be saved", false, staff8.HasChanges);

			AssertNoExceptionThrown(() => Factory.Save());

			var newFactory = new BusinessObjectFactory();
			var staff1Reloaded = newFactory.Load<GlbStaff>(staff1.PK);
			var staff2Reloaded = newFactory.Load<GlbStaff>(staff2.PK);
			var staff3Reloaded = newFactory.Load<GlbStaff>(staff3.PK);
			var staff4Reloaded = newFactory.Load<GlbStaff>(staff4.PK);
			var staff5Reloaded = newFactory.Load<GlbStaff>(staff5.PK);
			var staff6Reloaded = newFactory.Load<GlbStaff>(staff6.PK);
			var staff7Reloaded = newFactory.Load<GlbStaff>(staff7.PK);
			var staff8Reloaded = newFactory.Load<GlbStaff>(staff8.PK);

			AssertEquals("user1 should not be linked if error in linking", false, staff1Reloaded.IsADLinked);
			AssertEquals("user2 should not be linked if error in linking", false, staff2Reloaded.IsADLinked);
			AssertEquals("user3 should be linked as no error in linking", true, staff3Reloaded.IsADLinked);
			AssertEquals("user4 should not be linked if error in linking", false, staff4Reloaded.IsADLinked);
			AssertEquals("user5 should not be linked", false, staff5Reloaded.IsADLinked);
			AssertEquals("user6 should not be linked", false, staff6Reloaded.IsADLinked);
			AssertEquals("user7 should not be linked if error in linking", false, staff7Reloaded.IsADLinked);
			AssertEquals("user8 should not be linked if error in linking", false, staff8Reloaded.IsADLinked);
		}

		public void TestSync_ADLinkConflict_WithExistingLink()
		{
			DontSyncAnything();
			var syncer = GetSyncer();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("userX", path: "CN=userX");

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "user1";
			staff1.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "user2";
			staff2.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "user3";
			staff3.GS_ActiveDirectoryObjectGuid = Guid.Empty;

			Factory.Save();

			// staff2 match to the same AD user that staff1 currently linked to
			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(staff2.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry);
			// staff3 match to a different AD user
			directorySearcherMock.Setup(s => s.FindUser(staff3.GS_LoginName, TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("user3"));

			syncer.Synchronise();

			AssertEquals(string.Format(@"Multiple staff/group match against the same Active Directory record and will not be processed:
user1, user2 <--> Active Directory record with Guid {0}", directoryEntry.Guid), UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Staff1 should not be saved", false, staff1.HasChanges);
			AssertEquals("Staff2 should not be saved", false, staff2.HasChanges);
			AssertEquals("Staff3 should be saved", true, staff3.HasChanges);

			AssertNoExceptionThrown(() => Factory.Save());

			var newFactory = new BusinessObjectFactory();
			var staff1Reloaded = newFactory.Load<GlbStaff>(staff1.PK);
			var staff2Reloaded = newFactory.Load<GlbStaff>(staff2.PK);
			var staff3Reloaded = newFactory.Load<GlbStaff>(staff3.PK);

			AssertEquals("Staff1 should still be linked if error in linking", true, staff1Reloaded.IsADLinked);
			AssertEquals("Staff2 should not be linked if error in linking", false, staff2Reloaded.IsADLinked);
			AssertEquals("staff3 should be linked as no error in linking", true, staff3Reloaded.IsADLinked);
		}

		void DontSyncAnything()
		{
			var map = AttributeMap.DefaultMap.Clone();
			foreach (var item in map.MapItems.Cast<AttributeMapItem>())
			{
				item.IsSynced = false;
			}

			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);
		}

		#endregion

		#region Previously linked staff when AD account no longer exist - either deleted or is pointing to a new domain

		public void TestSync_ActiveLinkedStaffAndGroupWithNoADMatch()
		{
			var syncer = GetSyncer();
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Assert(staff.GS_IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Test Group";
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Assert(group.GG_IsActive);
			Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

			directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindGroup(group.GG_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);

			AssertNoExceptionThrown(() => syncer.Synchronise());

			var expectedMessageForStaff = @"Cannot synchronize 'lord.sauron'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'lord.sauron' was created or activated recently please try again later.";
			var expectedMessageForGroup = @"Cannot synchronize 'Test Group'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'Test Group' was created or activated recently please try again later.";
			var actualMessages = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text);
			AssertCollectionContains(expectedMessageForStaff, actualMessages);
			AssertCollectionContains(expectedMessageForGroup, actualMessages);
			AssertEquals("Staff should remain active", true, staff.GS_IsActive);
			AssertEquals("Group should remain active", true, group.GG_IsActive);
		}

		#endregion

		#region Sync only new changes from the Last Successful Sync

		[TestDate]
		public void TestOnlySyncNewChangesAfterLastSuccessfulSyncUTC()
		{
			var referenceDate = new DateTime(2020, 02, 29, 13, 42, 56);

			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceDate);
			var syncer = GetSyncer();

			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("ad.user1");
			directoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("ad.user2");
			directoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1));

			var directoryEntry3 = DummyDirectoryEntryWrapper.CreateUser("ad.user3");
			directoryEntry3.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry4 = DummyDirectoryEntryWrapper.CreateUser("ad.user4");
			directoryEntry4.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1));

			var groupDirectoryEntry1 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group1");
			groupDirectoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var groupDirectoryEntry2 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group2");
			groupDirectoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1));

			var groupDirectoryEntry3 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group3");
			groupDirectoryEntry3.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var groupDirectoryEntry4 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group4");
			groupDirectoryEntry4.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1));

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "cw.staff1";
			staff1.GS_ActiveDirectoryObjectGuid = directoryEntry1.Guid;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "cw.staff2";
			staff2.GS_ActiveDirectoryObjectGuid = directoryEntry2.Guid;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "cw.group1";
			group1.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry1.Guid;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "cw.group2";
			group2.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry2.Guid;

			//staff1,2 and group1,2 were changed yesterday
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1);
			Factory.Save();

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "cw.staff3";
			staff3.GS_ActiveDirectoryObjectGuid = directoryEntry3.Guid;

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_LoginName = "cw.staff4";
			staff4.GS_ActiveDirectoryObjectGuid = directoryEntry4.Guid;

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Desc = "cw.group3";
			group3.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry3.Guid;

			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Desc = "cw.group4";
			group4.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry4.Guid;

			//staff3,4 and group 3,4 were changed tomorrow
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1);
			Factory.Save();

			//We test it today
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value;

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry1.Guid, TestConstants.ValidOU)).Returns(directoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry2.Guid, TestConstants.ValidOU)).Returns(directoryEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry3.Guid, TestConstants.ValidOU)).Returns(directoryEntry3);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry4.Guid, TestConstants.ValidOU)).Returns(directoryEntry4);

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry1.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry2.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry3.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry3);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry4.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry4);

			//Find only return directoryEntry2, directoryEntry4, groupDirectoryEntry2, groupDirectoryEntry4
			DirectorySearcherProviderSubstitution.AddToUserDirectorySearcherResult(directoryEntry2, directoryEntry4);
			DirectorySearcherProviderSubstitution.AddToGroupDirectorySearcherResult(groupDirectoryEntry2, groupDirectoryEntry4);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			syncer.Synchronise();

			AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(new[] { staff1, staff2, staff3, staff4 }, new[] { directoryEntry1, directoryEntry2, directoryEntry3, directoryEntry4 });
			AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(new[] { group1, group2, group3, group4 }, new[] { groupDirectoryEntry1, groupDirectoryEntry2, groupDirectoryEntry3, groupDirectoryEntry4 });
		}

		protected virtual void AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbStaff[] staff, DummyDirectoryEntryWrapper[] directoryEntry)
		{
			// Staff1: No sync - staff1 and directoryEntry1 were both modified before LastSyncUtc
			AssertEquals("staff1 should not be synced", "cw.staff1", staff[0].GS_LoginName);
			AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry[0].GetValue(GlbStaffSchema.GS_LoginName));

			// Staff2: AD -> CW1 - directoryEntry2 was modified after LastSyncUtc and staff2 was modified before
			AssertEquals("staff2 should be synced", "ad.user2", staff[1].GS_LoginName);
			AssertEquals("directoryEntry2 should not be changed", "ad.user2", directoryEntry[1].GetValue(GlbStaffSchema.GS_LoginName));

			// Staff3: CW1 -> AD - staff3 was modified after LastSyncUtc and directoryEntry3 was modified before
			AssertEquals("staff3 should not be changed", "cw.staff3", staff[2].GS_LoginName);
			AssertEquals("directoryEntry3 should be synced", "cw.staff3", directoryEntry[2].GetValue(GlbStaffSchema.GS_LoginName));

			// Staff4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
			AssertEquals("staff4 should not changed", "cw.staff4", staff[3].GS_LoginName);
			AssertEquals("directoryEntry4 should be synced", "cw.staff4", directoryEntry[3].GetValue(GlbStaffSchema.GS_LoginName));
		}

		protected virtual void AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbGroup[] group, DummyDirectoryEntryWrapper[] groupDirectoryEntry)
		{
			// Group1: No sync - group1 and directoryEntry1 were both modified before LastSyncUtc
			AssertEquals("group1 should not be synced", "cw.group1", group[0].GG_Desc);
			AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry[0].GetValue(GlbGroupSchema.GG_Desc));

			// Group2: AD -> CW1 - directoryEntry2 was modified after LastSyncUtc and group2 was modified before
			AssertEquals("group2 should be synced", "ad.group2", group[1].GG_Desc);
			AssertEquals("groupDirectoryEntry2 should not be changed", "ad.group2", groupDirectoryEntry[1].GetValue(GlbGroupSchema.GG_Desc));

			// Group3: CW1 -> AD - group3 was modified after LastSyncUtc and directoryEntry3 was modified before
			AssertEquals("group3 should not be changed", "cw.group3", group[2].GG_Desc);
			AssertEquals("groupDirectoryEntry3 should be synced", "cw.group3", groupDirectoryEntry[2].GetValue(GlbGroupSchema.GG_Desc));

			// Group4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
			AssertEquals("group4 should not changed", "cw.group4", group[3].GG_Desc);
			AssertEquals("groupDirectoryEntry4 should be synced", "cw.group4", groupDirectoryEntry[3].GetValue(GlbGroupSchema.GG_Desc));
		}

		#endregion

		#region GDPR

		public virtual void TestSync_OptOutSavingPersonalDataToAD()
		{
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddDays(-1).ToDateTime());

			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
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
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			staff.GS_SavePersonalDataToActiveDirectory = false;
			Factory.Save();

			GetSyncer().Synchronise();
			AssertOptOutSavingPersonalDataToAD(staff);
		}

		protected abstract void AssertOptOutSavingPersonalDataToAD(GlbStaff staff);

		protected void AssertOptOutSavingPersonalDataToAD_FromAD(GlbStaff staff)
		{
			// Staff details overriden by directoryEntry (AD)
			Assert(staff.GS_IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("lord.sauron", staff.GS_LoginName);
			AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			AssertEquals("Lord Sauron", staff.GS_FullName);
			AssertEquals("sauron@mordor.com", staff.GS_EmailAddress);
			AssertEquals("Lord", staff.GS_Title);
			AssertEquals("0294811111", staff.GS_WorkPhone);
			AssertEquals("0294811110", staff.GS_FaxNum);
			AssertEquals("123456", staff.GS_Pager);

			// Personal data should not sync
			AssertEquals("Mobile phone", staff.GS_MobilePhone);
			AssertEquals("Home phone", staff.GS_HomePhone);
			AssertEquals("Address1", staff.GS_UserAddress1);
			AssertEquals("City", staff.GS_City);
			AssertEquals("State", staff.GS_State);
			AssertEquals("Postcode", staff.GS_Postcode);
		}

		protected void AssertOptOutSavingPersonalDataToAD_FromCW(GlbStaff staff)
		{
			// AD details overriden by staff details
			var adUser = new ADUser(staff);
			Assert(adUser.IsActive);
			Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("sauron", adUser.LoginName);
			AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			AssertEquals("Sauron the Great", adUser.FullName);
			AssertEquals("Email", adUser.EmailAddress);
			AssertEquals("Title", adUser.Title);
			AssertEquals("Work phone", adUser.WorkPhone);
			AssertEquals("Fax num", adUser.FaxNum);
			AssertEquals("Pager", adUser.Pager);

			// Personal data should be cleared in AD
			AssertEquals("Mobile should be cleared", ZString.Empty, adUser.MobilePhone);
			AssertEquals("Address should be cleared", ZString.Empty, adUser.StreetAddress);
			AssertEquals("City should be cleared", ZString.Empty, adUser.City);
			AssertEquals("State should be cleared", ZString.Empty, adUser.State);
			AssertEquals("Postcode should be cleared", ZString.Empty, adUser.Postcode);
			AssertEquals("HomePhone should be cleared", ZString.Empty, adUser.HomePhone);
		}
		#endregion

		#region Sync Race Condition

		public void TestSync_WithRaceCondition()
		{
			// Instance 1 creates the staff and group but not sync to AD
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "jon";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			var group = factory1.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "groupJ";
			group.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			factory1.Save();

			// Instance 2 sync and link them to new AD objets on different AD site not yet accessible by instance 1
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staffFromFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffFromFactory2.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			var groupFromFactory2 = factory2.Load<GlbGroup>(group.PK);
			groupFromFactory2.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			factory2.Save();

			// When sync from Instance 1, it should reload and relised it has been linked, and report error as it is not yet accessible.
			AssertEquals("Staff should not linked yet in the factory1 before sync", false, staff.IsADLinked);
			AssertEquals("Group should not linked yet in the factory1 before sync", false, group.IsADLinked);
			var syncer = new EntitySynchroniser(factory1);
			syncer.Synchronise();

			var staffError = @"Cannot synchronize 'jon'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'jon' was created or activated recently please try again later.";
			AssertEquals("Expecting user error message when sync from Instance 1", staffError, UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Staff should be linked", true, staff.IsADLinked);
			AssertEquals("Staff should be linked to the same AD object", staffFromFactory2.GS_ActiveDirectoryObjectGuid, staff.GS_ActiveDirectoryObjectGuid);

			var groupError = @"Cannot synchronize 'groupJ'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'groupJ' was created or activated recently please try again later.";
			AssertEquals("Expecting group error message when sync from Instance 1", groupError, UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			AssertEquals("Group should be linked", true, group.IsADLinked);
			AssertEquals("Group should be linked to the same AD object", groupFromFactory2.GG_ActiveDirectoryObjectGuid, group.GG_ActiveDirectoryObjectGuid);
		}

		#endregion

		#endregion
	}

	sealed class EntitySynchroniserTest_StandAlone : EntitySynchroniserTest
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
		}

		#endregion

		#region MaxLength

		public void TestSync_ShouldRespectFieldMaxLengths()
		{
			var syncer = GetSyncer();

			var fullName = new string('w', GlbStaffSchema.GS_FullName.MaxLength + 1);
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: fullName);
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();

			AssertEquals(fullName.Substring(0, fullName.Length - 1), staff.GS_FullName);
		}

		#endregion

		#region Resources

		public void TestSync_ShouldIgnoreResources()
		{
			var syncer = GetSyncer();
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "sauron");
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var resource = Factory.New<GlbStaff>();
			resource.GS_FullName = "Resource";
			resource.GS_Code = "$01";
			resource.GS_IsResource = true;
			resource.GS_IsActive = true;

			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

			syncer.Synchronise();

			AssertEquals("Should perform sync of staff", "sauron", staff.GS_FullName);
			AssertEquals("Should not deactivate resource", true, resource.GS_IsActive);
		}

		#endregion

		#region Handle ZSaveConcurrencyException

		public void TestSaveCanIgnoreConcurrency()
		{
			var factory1 = new BusinessObjectFactory() { NameForDebugging = "jj" };
			factory1.RefreshEnabled = false;
			var syncer = new EntitySynchroniser(factory1);
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var staffFromFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffFromFactory2.Address1 = "1 Dodgy st";
			//field that is not synced
			staffFromFactory2.GS_EmergencyContactName = "triple-zeros";

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("jon");
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(directoryEntry);

			syncer.Synchronise();
			factory2.Save();

			AssertNoExceptionThrown(syncer.Save);

			//AD should win - on synced fields only, fields that is not synced should not affected
			factory1.ReloadAllSafe<GlbStaff>();
			AssertEquals("staff.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staff.GS_LoginName);
			AssertEquals("staff.Address1", directoryEntry[ADAttributes.Address], staff.Address1);
			AssertEquals("staff.GS_EmergencyContactName should not changed", "triple-zeros", staff.GS_EmergencyContactName);

			factory2.ReloadAllSafe<GlbStaff>();
			AssertEquals("staffFromFactory2.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staffFromFactory2.GS_LoginName);
			AssertEquals("staffFromFactory2.Address1", directoryEntry[ADAttributes.Address], staffFromFactory2.Address1);
			AssertEquals("staffFromFactory2.GS_EmergencyContactName should not changed", "triple-zeros", staffFromFactory2.GS_EmergencyContactName);

			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			AssertEquals("staffReloaded.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staffReloaded.GS_LoginName);
			AssertEquals("staffReloaded.Address1", directoryEntry[ADAttributes.Address], staffReloaded.Address1);
			AssertEquals("staffReloaded.GS_EmergencyContactName should not changed", "triple-zeros", staffReloaded.GS_EmergencyContactName);
		}

		public void TestSaveCanIgnoreConcurrencyWithGlbPerson()
		{
			var factory1 = new BusinessObjectFactory() { NameForDebugging = "factory1" };
			factory1.RefreshEnabled = false;
			var syncer = new EntitySynchroniser(factory1);
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { NameForDebugging = "factory2" };
			var staffFromFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffFromFactory2.Address1 = "1 sunshines coast";
			factory2.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("jon");
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(directoryEntry);
			directoryEntry.SetLastModified(DateTime.Now.AddDays(1)); // to force AD -> CW1 sync

			syncer.Synchronise();

			// So it won't do person.Reload() in GlbStaff.OnSaving()
			// To mimic the racing condition where Person has changed after the person.Reload()
			staff.Person.HasChanges = true;
			AssertNoExceptionThrown(syncer.Save);

			//AD should win - on synced fields only, fields that is not synced should not affected
			factory1.ReloadAllSafe<GlbStaff>();
			AssertEquals("staff.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staff.GS_LoginName);
			AssertEquals("staff.Address1", directoryEntry[ADAttributes.Address], staff.Address1);

			factory2.ReloadAllSafe<GlbStaff>();
			AssertEquals("staffFromFactory2.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staffFromFactory2.GS_LoginName);
			AssertEquals("staffFromFactory2.Address1", directoryEntry[ADAttributes.Address], staffFromFactory2.Address1);

			var staffReloaded = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
			AssertEquals("staffReloaded.GS_LoginName", directoryEntry[ADAttributes.UserPrincipalName], staffReloaded.GS_LoginName);
			AssertEquals("staffReloaded.Address1", directoryEntry[ADAttributes.Address], staffReloaded.Address1);
		}

		public void TestNoConcurrencyExceptionFromDeletedPasswordHistory()
		{
			DataRegistry.Instance.PasswordHistoryCount = 2;

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var staff = factory1.NewWithValidTestData<GlbStaff>();
			staff.StaffPlainTextPassword = "Changeme1234";
			factory1.Save();

			staff.ChangeLocalPassword("Changeme1234", "Changeme1234A");
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var staffFromFactory2 = factory2.Load<GlbStaff>(staff.PK);
			staffFromFactory2.ChangeLocalPassword("Changeme1234A", "Changeme1234X");

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("jon");
			directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(directoryEntry);

			var syncer = new EntitySynchroniser(factory1);
			syncer.Synchronise();

			factory2.Save();
			AssertNoExceptionThrown(syncer.Save);
		}

		public void TestSaveDoesNotReportZCannotSaveException()
		{
			var syncer = new EntitySynchroniser(Factory);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = true;
			staff.GS_IsActive = true;
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			staff.GS_IsActive = false;

			CombineAssertions(() =>
			{
				AssertExceptionThrown("Should have thrown exception", typeof(ZCannotSaveException), "You are attempting to affect the last active non operational or controller staff member.", syncer.Save);
				Assert("Should not have reported message", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			});
		}

		#endregion

		#region LoginName conflict

		public void TestCanHandleLoginNameConflictWhenSyncADAsMaster()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "lord.sauron";
			staff1.GS_IsActive = false;
			staff1.GS_CanLogin = false;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "harry.pothead";
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron@domain");
			directoryEntry.SetLastModified(DateTime.UtcNow.AddDays(1));
			directorySearcherMock.Setup(s => s.FindUser(staff2.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

			var syncer = GetSyncer();
			syncer.Synchronise();
			AssertNoExceptionThrown("Should not crash", syncer.Save);
			AssertContains("Should report LoginName conflict", @"Cannot synchronize Login Name 'harry.pothead' with Active Directory User's Logon Name 'lord.sauron'", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Staff2 LoginName should not be changed", "harry.pothead", staff2.GS_LoginName);
			var staff2Reloaded = Factory.Load<GlbStaff>(staff2.PK);
			AssertEquals("Staff2 LoginName in DB should not be changed", "harry.pothead", staff2Reloaded.GS_LoginName);
		}

		public void TestSync_NotAllowLinkingIfLoginNamesHaveAccentDifferent()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "Meissner";
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Meißner");
			directoryEntry.SetLastModified(DateTime.UtcNow.AddDays(1));
			directorySearcherMock.Setup(s => s.FindUser(staff1.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.UserPrincipalName, TestConstants.ValidOU)).Returns(directoryEntry);

			var syncer = GetSyncer();
			AssertNoExceptionThrown("Should not crash", () => syncer.Synchronise());
			AssertContains("Cannot synchronize 'Meissner' with the matching Active Directory User 'Meißner' because the Login Name has different diacritic marks or ligature (e.g. à and a).\r\nPlease change Login Name 'Meissner' to match with AD user's logon name 'Meißner' and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should not be linked", ZGuid.Empty, staff1.GS_ActiveDirectoryObjectGuid);

			UnitTestUserNotification.Instance.ClearMessages();
			staff1.GS_LoginName = "Meißner";
			Factory.Save();

			syncer = GetSyncer();
			syncer.Synchronise();
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should be linked", directoryEntry.Guid, staff1.GS_ActiveDirectoryObjectGuid);
		}

		#endregion

		#region Profile Photo sync

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestProfilePhotoNotSyncWhenLargerThan100KB()
		{
			var largeImagePath = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Operations", "MasterFiles", "Business", "MasterFiles.Business", "Testing", "160kb.bmp");
			var smallImagePath = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Operations", "MasterFiles", "Business", "MasterFiles.Business", "Testing", "100kb.bmp");

			var largeImageSize = new FileInfo(largeImagePath).Length;
			AssertGreaterThan(largeImageSize, GlbStaffValidationReal.MaxADProfilePhotoBytes);

			var smallImageSize = new FileInfo(smallImagePath).Length;
			AssertLessThan(smallImageSize, GlbStaffValidationReal.MaxADProfilePhotoBytes);

			var syncer = GetSyncer();

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "lord.sauron";
			staff1.GS_FullName = "Lord Sauron";
			staff1.ProfileImage = new Bitmap(largeImagePath);

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "harry.potter";
			staff2.GS_FullName = "Harry Potter";
			staff2.ProfileImage = new Bitmap(smallImagePath);

			Factory.Save();

			var directoryEntry1 = new DummyDirectoryEntryWrapper("lord.sauron", ZDateTime.UtcNow.ToDateTime(), "no name", "", "", "", 1, "", "", "", "", "", "", "", "", "");
			directoryEntry1.SetLastModified(ZDateTime.UtcNow.AddHours(-1).ToDateTime());

			var directoryEntry2 = new DummyDirectoryEntryWrapper("harry.potter", ZDateTime.UtcNow.ToDateTime(), "no name", "", "", "", 1, "", "", "", "", "", "", "", "", "");
			directoryEntry2.SetLastModified(ZDateTime.UtcNow.AddHours(-1).ToDateTime());

			directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry1);
			directorySearcherMock.Setup(s => s.FindUser("harry.potter", TestConstants.ValidOU)).Returns(directoryEntry2);

			syncer.Synchronise();

			var adUser1 = new ADUser(staff1);
			Assert(adUser1.IsActive);
			Assert(staff1.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("lord.sauron", adUser1.LoginName);
			AssertEquals("Lord Sauron", adUser1.FullName);
			AssertEquals("Profile image should not be synced when it is larger than 100kb", 0, adUser1.ThumbnailImage.Length);

			var adUser2 = new ADUser(staff2);
			Assert(adUser2.IsActive);
			Assert(staff2.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("harry.potter", adUser2.LoginName);
			AssertEquals("Harry Potter", adUser2.FullName);
			AssertEquals("Profile image should be synced when it is smaller than 100kb", smallImageSize, adUser2.ThumbnailImage.Length);
		}

		#endregion

		#region Should Not Sync Inactive New Staff

		public void TestSync_ShouldIgnoreInactiveNewStaff()
		{
			var syncer = GetSyncer();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "lord.sauron";
			staff1.GS_IsActive = true;
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "harry.potter";
			staff2.GS_IsActive = false;
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser("lord.sauron", fullName: "sauron"));

			syncer.Synchronise();

			AssertEquals("Should sync active new staff", true, staff1.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("Should not sync inactive new staff", false, staff2.GS_ActiveDirectoryObjectGuid.IsValid);
			AssertEquals("Should not sync inactive new staff", ZGuid.Invalid, staff2.GS_ActiveDirectoryObjectGuid);

			directorySearcherMock.Verify(s => s.FindUser("lord.sauron", TestConstants.ValidOU), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser("harry.potter", TestConstants.ValidOU), Times.Never);
		}

		#endregion

		#region Sync staff who Cannot Login

		public void TestSync_CannotLogin_Initial()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var syncer = GetSyncer();
			var staffCanLogin = Factory.New<GlbStaff>();
			staffCanLogin.GS_LoginName = "can";
			staffCanLogin.GS_IsActive = true;
			staffCanLogin.GS_CanLogin = true;
			staffCanLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var staffCannotLogin = Factory.New<GlbStaff>();
			staffCannotLogin.GS_LoginName = "cannot";
			staffCannotLogin.GS_IsActive = true;
			staffCannotLogin.GS_CanLogin = false;
			staffCannotLogin.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			Factory.Save();

			directorySearcherMock.Setup(s => s.FindUser("can", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

			syncer.Synchronise();

			AssertEquals("Should sync staff who can login", ZGuid.Invalid, staffCanLogin.GS_ActiveDirectoryObjectGuid);
			AssertEquals("Should not sync staff who cant login", ZGuid.Empty, staffCannotLogin.GS_ActiveDirectoryObjectGuid);

			directorySearcherMock.Verify(s => s.FindUser("can", TestConstants.ValidOU), Times.AtLeastOnce);
			directorySearcherMock.Verify(s => s.FindUser("cannot", TestConstants.ValidOU), Times.Never);
		}

		public void TestSync_CannotLogin_Ongoing()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;

			var syncer = GetSyncer();
			var staffCanLogin = Factory.New<GlbStaff>();
			staffCanLogin.GS_LoginName = "can";
			staffCanLogin.GS_IsActive = true;
			staffCanLogin.GS_CanLogin = true;
			staffCanLogin.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var staffCannotLoginNotLinked = Factory.New<GlbStaff>();
			staffCannotLoginNotLinked.GS_LoginName = "cannot";
			staffCannotLoginNotLinked.GS_IsActive = true;
			staffCannotLoginNotLinked.GS_CanLogin = false;
			staffCannotLoginNotLinked.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

			var staffCannotLoginLinked = Factory.New<GlbStaff>();
			staffCannotLoginLinked.GS_LoginName = "linked_cannot";
			staffCannotLoginLinked.GS_IsActive = true;
			staffCannotLoginLinked.GS_CanLogin = false;
			staffCannotLoginLinked.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			var dirEntry2 = DummyDirectoryEntryWrapper.CreateUser("dummy2");

			var staffCanLoginGuid = staffCanLogin.GS_ActiveDirectoryObjectGuid.ToGuid();
			var staffCannotLoginLinkedGuid = staffCannotLoginLinked.GS_ActiveDirectoryObjectGuid.ToGuid();
			directorySearcherMock.Setup(s => s.FindUser(staffCanLoginGuid, TestConstants.ValidOU)).Returns(dirEntry1);
			directorySearcherMock.Setup(s => s.FindUser(staffCannotLoginLinkedGuid, TestConstants.ValidOU)).Returns(dirEntry2);

			syncer.Synchronise();

			AssertNotEquals("Should sync staff who can login", ZGuid.Empty, staffCanLogin.GS_ActiveDirectoryObjectGuid);
			AssertEquals("Should sync staff who can login", "dummy1", staffCanLogin.GS_LoginName);
			AssertNotEquals("Should sync linked staff who cannot login", ZGuid.Empty, staffCannotLoginLinked.GS_ActiveDirectoryObjectGuid);
			AssertEquals("Should sync linked staff who cannot login", "dummy2", staffCannotLoginLinked.GS_LoginName);

			AssertEquals("Should not sync staff who cant login", ZGuid.Empty, staffCannotLoginNotLinked.GS_ActiveDirectoryObjectGuid);
			AssertEquals("Should not sync staff who cant login", "cannot", staffCannotLoginNotLinked.GS_LoginName);

			directorySearcherMock.Verify(s => s.FindUser(staffCanLoginGuid, TestConstants.ValidOU), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser(staffCannotLoginLinkedGuid, TestConstants.ValidOU), Times.Once);
			directorySearcherMock.Verify(s => s.FindUser("cannot", TestConstants.ValidOU), Times.Never);
		}

		#endregion

		[TestDate]
		public void TestSync_ShouldResyncLastFailedSyncEntities()
		{
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow);
			var syncer = GetSyncer();

			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("ad.user1");
			directoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("ad.user2");
			directoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry3 = DummyDirectoryEntryWrapper.CreateUser("ad.user3");
			directoryEntry3.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry4 = DummyDirectoryEntryWrapper.CreateUser("ad.user4");
			directoryEntry4.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-2));

			var directoryEntry5 = DummyDirectoryEntryWrapper.CreateUser("ad.user5");
			directoryEntry5.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-2));

			var groupDirectoryEntry1 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group1");
			groupDirectoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var groupDirectoryEntry2 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group2");
			groupDirectoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var groupDirectoryEntry3 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group3");
			groupDirectoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "cw.staff1";
			staff1.GS_ActiveDirectoryObjectGuid = directoryEntry1.Guid;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "cw.staff2";
			staff2.GS_ActiveDirectoryObjectGuid = directoryEntry2.Guid;

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_LoginName = "cw.staff4";
			staff4.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff5 = Factory.NewWithValidTestData<GlbStaff>();
			staff5.GS_LoginName = "cw.staff5";
			staff5.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "cw.group1";
			group1.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry1.Guid;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "cw.group2";
			group2.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry2.Guid;

			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1);
			Factory.Save();

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "cw.staff3";
			staff3.GS_ActiveDirectoryObjectGuid = directoryEntry3.Guid;

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Desc = "cw.group3";
			group3.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry3.Guid;

			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1);
			Factory.Save();

			//We test it today
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value;

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry1.Guid, TestConstants.ValidOU)).Returns(directoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry2.Guid, TestConstants.ValidOU)).Returns(directoryEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry3.Guid, TestConstants.ValidOU)).Returns(directoryEntry3);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser("cw.staff4", TestConstants.ValidOU)).Returns(directoryEntry4);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser("cw.staff5", TestConstants.ValidOU)).Returns(directoryEntry5);

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry1.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry2.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry3.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry3);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			// staff 2 & 4 failed to sync previously and registered in LastFailedSyncStaffPKs, we dont register staff5
			ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { staff2.PK.ToGuid(), staff4.PK.ToGuid() });
			// group 2 was a failed sync
			ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid[] { group2.PK.ToGuid() });

			syncer.Synchronise();

			//should not sync staff1
			AssertEquals("staff1 should not be synced", "cw.staff1", staff1.GS_LoginName);
			AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry1.GetValue(GlbStaffSchema.GS_LoginName));

			//should sync staff2 as it was a failed sync
			AssertEquals("staff2 should not be changed", "cw.staff2", staff2.GS_LoginName);
			AssertEquals("directoryEntry2 should be synced", "cw.staff2", directoryEntry2.GetValue(GlbStaffSchema.GS_LoginName));

			//should sync staff3 as it is newer
			AssertEquals("staff3 should not be changed", "cw.staff3", staff3.GS_LoginName);
			AssertEquals("directoryEntry3 should be synced", "cw.staff3", directoryEntry3.GetValue(GlbStaffSchema.GS_LoginName));

			//should sync staff4 as it is an unlinked staff, regardless of GS_SystemLastEditTimeUtc
			AssertEquals("staff4 should be linked", directoryEntry4.Guid, staff4.GS_ActiveDirectoryObjectGuid);
			AssertEquals("staff4 should not be changed", "cw.staff4", staff4.GS_LoginName);
			AssertEquals("directoryEntry4 should be synced", "cw.staff4", directoryEntry4.GetValue(GlbStaffSchema.GS_LoginName));

			//should sync staff5 as it is an unlinked staff, regardless of GS_SystemLastEditTimeUtc
			AssertEquals("staff5 should be linked", directoryEntry5.Guid, staff5.GS_ActiveDirectoryObjectGuid);
			AssertEquals("staff5 should not be changed", "cw.staff5", staff5.GS_LoginName);
			AssertEquals("directoryEntry5 should be synced", "cw.staff5", directoryEntry5.GetValue(GlbStaffSchema.GS_LoginName));

			//should not sync group1
			AssertEquals("group1 should not be synced", "cw.group1", group1.GG_Desc);
			AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry1.GetValue(GlbGroupSchema.GG_Desc));

			//should sync group2 as it was a failed sync
			AssertEquals("group2 should not be changed", "cw.group2", group2.GG_Desc);
			AssertEquals("groupDirectoryEntry2 should be synced", "cw.group2", groupDirectoryEntry2.GetValue(GlbGroupSchema.GG_Desc));

			//should sync group3 as it is newer
			AssertEquals("group3 should not be changed", "cw.group3", group3.GG_Desc);
			AssertEquals("groupDirectoryEntry3 should be synced", "cw.group3", groupDirectoryEntry3.GetValue(GlbGroupSchema.GG_Desc));
		}

		public void TestSync_PreferredLanguage_ADIsMaster()
		{
			var syncer = GetSyncer();
			var syncResults = new List<IEnumerable<ISyncEvent>>();
			syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

			var staffNames = new string[] { "A", "B", "C", "D", "E", "F" };
			var staffLanguages = new string[] { "en-AU", "en-GB", "en-US", "zh-CN", "asdf", string.Empty };
			var expectedLanguages = new string[] { "EN", "EN-GB", "EN-US", "ZH-CN", "ASDF", "EN" };
			for (var i = 0; i < staffNames.Length; ++i)
			{
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staffNames[i]);
				directoryEntry.SetValue(GlbStaffSchema.GS_WorkingLanguage, staffLanguages[i]);
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = staffNames[i];
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				staff.GS_WorkingLanguage = "SQ-AL";
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime()); //ensure AD to be the latest
			}

			Factory.Save();

			var map = AttributeMap.DefaultMap;
			map.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_WorkingLanguage).IsSynced = true;
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			syncer.Synchronise();

			AssertEquals(staffNames.Length, syncResults.Count);
			for (var i = 0; i < staffNames.Length; ++i)
			{
				var histories = syncResults[i].ToArray();
				AssertEquals(18, histories.Length);

				//histories appear in an arbitrary order, so loop staffNames to find which index this is
				var j = 0;
				var staffName = histories[0].SynchronisedValue.ToString();
				for (; j < staffNames.Length; ++j)
				{
					if (staffName.Equals(staffNames[j], StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
				}
				if (j == staffNames.Length)
				{
					Fail("Unexpected name: " + staffName);
				}

				EntitySynchroniserTest_Common.AssertHistory(histories[17], staffLanguages[j], "SQ-AL", expectedLanguages[j], GlbStaffSchema.Constants.GS_WorkingLanguage);
			}
		}

		public void TestSync_PreferredLanguage_EnterpriseIsMaster()
		{
			var syncer = GetSyncer();
			var syncResults = new List<IEnumerable<ISyncEvent>>();
			syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

			var staffNames = new string[] { "A", "B", "C", "D", "E", "F" };
			var staffLanguages = new string[] { ZString.Empty, "ASDF", "EN", "EN-US", "EN-GB", "ZH-CN" };
			var expectedLanguages = new string[] { "en-AU", "ASDF", "en-AU", "en-US", "en-GB", "zh-CN" };
			for (var i = 0; i < staffNames.Length; ++i)
			{
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staffNames[i]);
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = staffNames[i];
				staff.GS_WorkingLanguage = staffLanguages[i];
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				//Set AD to be older
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);
			}
			Factory.Save();

			var map = AttributeMap.DefaultMap;
			map.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_WorkingLanguage).IsSynced = true;
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			syncer.Synchronise();

			AssertEquals(staffNames.Length, syncResults.Count);
			for (var i = 0; i < staffNames.Length; ++i)
			{
				var histories = syncResults[i].ToArray();
				AssertEquals(18, histories.Length);

				//histories appear in an arbitrary order, so loop staffNames to find which index this is
				var j = 0;
				var staffName = histories[0].SynchronisedValue.ToString();
				for (; j < staffNames.Length; ++j)
				{
					if (staffName.Equals(staffNames[j], StringComparison.OrdinalIgnoreCase))
					{
						break;
					}
				}
				if (j == staffNames.Length)
				{
					Fail("Unexpected name: " + staffName);
				}

				EntitySynchroniserTest_Common.AssertHistory(histories[17], null, staffLanguages[j], expectedLanguages[j], GlbStaffSchema.Constants.GS_WorkingLanguage);
			}
		}

		public void TestSync_IsRobot()
		{
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(true);
			ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "rpaTestGroup");
			var syncer = GetSyncer();

			var rpaGroupName = ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.Value;

			var groupEntry = DummyDirectoryEntryWrapper.CreateGroup(rpaGroupName);

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "user1";
			var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("user1");
			staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;
			staff1.GS_IsRobot = false;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_LoginName = "user2";
			var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("user2");
			staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;
			staff2.GS_IsRobot = false;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "user3";
			var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("user3");
			staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;
			staff3.GS_IsRobot = true;

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_LoginName = "user4";
			staff4.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			staff4.GS_IsRobot = true;

			Factory.Save();

			groupEntry.AddMember(userEntry1);
			groupEntry.AddMember(userEntry2);

			AssertEquals("Pre-req: 2 member in the robotic group before sync.", 2, groupEntry.GetMembers().Count());

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(rpaGroupName, It.IsAny<string>())).Returns(groupEntry);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);
			//staff4 will fail to sync
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff4.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff4.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			syncer.Synchronise();
			syncer.Save();

			AssertEquals("Only one member in the group after sync (failed sync user won't be added)", 1, groupEntry.GetMembers().Count());
			AssertEquals("Only one staff with IsRobot in the group", staff3.GS_ActiveDirectoryObjectGuid, groupEntry.GetMembers().First().Guid);
			AssertEquals("One staff failed to be synced", 1, syncer.EntitiesWithErrors.Count());
		}

		public void TestSync_IsRobot_RPAGroupNotFound()
		{
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(true);
			ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "rpaTestGroup");
			var syncer = GetSyncer();

			var rpaGroupName = ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.Value;

			var adUser = new Mock<IADUser>();
			ADEntityProviderSubstitution.ADUser = adUser.Object;

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(rpaGroupName, It.IsAny<string>())).Returns((IGroupDirectoryEntry)null);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			AssertExceptionThrown(typeof(NoDomainPrivilegeException), "Robotic group 'rpaTestGroup' could not be found.", () => syncer.SyncUsersToRoboticGroupIfRequired(new[] { adUser.Object }));
		}

		public void TestSync_IsRobot_RPAGroupNotWritable()
		{
			DataRegistry.Instance.SetEnableRPAOnWiseCloudForTest(true);
			ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "rpaTestGroup");
			var syncer = GetSyncer();

			var rpaGroupName = ActiveDirectoryRegistry.Instance.RoboticProcessAutomationGroup.Value;
			var rpaGroup = new Mock<IGroupDirectoryEntry>();

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "user1";
			var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("user1");
			staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;
			staff1.GS_IsRobot = true;

			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(rpaGroupName, It.IsAny<string>())).Returns(rpaGroup.Object);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
			rpaGroup.Setup(g => g.CommitChanges()).Throws(new DirectoryServicesException("Cannot write to group"));

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			syncer.Synchronise();
			AssertExceptionThrown(typeof(NoDomainPrivilegeException), "Domain user does not have write privileges to the Robotic Group 'rpaTestGroup'.", () => syncer.Save());
		}

		[TestDate]
		public void TestSync_ShouldSyncAllEntitiesWhenPreferredSyncModeIsSupplied()
		{
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow);
			var syncer = GetSyncer();

			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("ad.user1");
			directoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("ad.user2");
			directoryEntry2.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var groupDirectoryEntry1 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group1");
			groupDirectoryEntry1.AddMember(directoryEntry2);
			groupDirectoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "cw.staff1";
			staff1.GS_ActiveDirectoryObjectGuid = directoryEntry1.Guid;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "cw.staff2";
			staff2.GS_ActiveDirectoryObjectGuid = directoryEntry2.Guid;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "cw.group1";
			group1.GG_ActiveDirectoryObjectGuid = groupDirectoryEntry1.Guid;
			group1.Staff.Add(staff1);

			//staff and group were changed yesterday
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1);
			Factory.Save();

			//We test it today
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value;

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry1.Guid, TestConstants.ValidOU)).Returns(directoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry2.Guid, TestConstants.ValidOU)).Returns(directoryEntry2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(groupDirectoryEntry1.Guid, TestConstants.ValidOU)).Returns(groupDirectoryEntry1);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			//Sync without preferred sync mode - should not sync anyone as LastSyncUTC is newer
			syncer.Synchronise();
			AssertEquals("staff1 should not be synced", "cw.staff1", staff1.GS_LoginName);
			AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry1.GetValue(GlbStaffSchema.GS_LoginName));

			AssertEquals("group1 should not be synced", 1, group1.Staff.Count);
			AssertEquals("groupDirectoryEntry1 should not be synced", 1, groupDirectoryEntry1.GroupMembership.Count);

			// --Now test with preferred sync mode--

			// Sync with preferred sync mode EnterpriseIsMaster: CW1 -> AD
			syncer.Synchronise(null, SyncMode.EnterpriseIsMaster);
			AssertEquals("staff1 should not be synced", "cw.staff1", staff1.GS_LoginName);
			AssertEquals("directoryEntry1 should be synced", "cw.staff1", directoryEntry1.GetValue(GlbStaffSchema.GS_LoginName));
			//group membership should be unioned for any preferred sync mode
			AssertEquals("group1 should be synced (with Union)", 2, group1.Staff.Count);
			AssertCollectionContains("group1 should have staff1", staff1, group1.Staff);
			AssertCollectionContains("group1 should have staff2", staff2, group1.Staff);
			AssertEquals("groupDirectoryEntry1 should be synced (with Union)", 2, groupDirectoryEntry1.GroupMembership.Count);
			AssertCollectionContains("groupDirectoryEntry1 should have directoryEntry1", directoryEntry1, groupDirectoryEntry1.GroupMembership);
			AssertCollectionContains("groupDirectoryEntry1 should have directoryEntry2", directoryEntry2, groupDirectoryEntry1.GroupMembership);

			// Sync with preferred sync mode ADIsMaster: AD -> CW1
			// reset for ADIsMaster
			directoryEntry1.UserPrincipalName = "ad.user1";
			groupDirectoryEntry1.GroupMembership.Clear();
			groupDirectoryEntry1.AddMember(directoryEntry2);
			groupDirectoryEntry1.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1));
			group1.Staff.RemoveAll();
			group1.Staff.Add(staff1);
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1);
			Factory.Save();
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value;

			AssertEquals(1, group1.Staff.Count);
			AssertEquals(1, groupDirectoryEntry1.GroupMembership.Count);
			syncer.Synchronise(null, SyncMode.ADIsMaster);
			AssertEquals("staff1 should be synced", "ad.user1", staff1.GS_LoginName);
			AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry1.GetValue(GlbStaffSchema.GS_LoginName));
			//group membership should be unioned for any preferred sync mode
			AssertEquals("group1 should be synced (with Union)", 2, group1.Staff.Count);
			AssertCollectionContains("group1 should have staff1", staff1, group1.Staff);
			AssertCollectionContains("group1 should have staff2", staff2, group1.Staff);
			AssertEquals("groupDirectoryEntry1 should be synced (with Union)", 2, groupDirectoryEntry1.GroupMembership.Count);
			AssertCollectionContains("groupDirectoryEntry1 should have directoryEntry1", directoryEntry1, groupDirectoryEntry1.GroupMembership);
			AssertCollectionContains("groupDirectoryEntry1 should have directoryEntry2", directoryEntry2, groupDirectoryEntry1.GroupMembership);
		}

		[TestDate]
		public void TestSync_ShouldRecordFailedSyncAndConflictEntities()
		{
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow);
			var syncer = GetSyncer();

			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("ad.user1");

			var directoryEntryX = DummyDirectoryEntryWrapper.CreateUser("ad.userX");
			directoryEntryX.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(2)); // make it newer so it wont need write access to sync

			var groupDirectoryEntry1 = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.group1");

			var groupDirectoryEntryX = DummyGroupDirectoryEntryWrapper.CreateGroup("ad.groupX");
			groupDirectoryEntryX.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(2)); // make it newer so it wont need write access to sync

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "cw.staff1";
			staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_LoginName = "cw.staff2";
			staff2.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_LoginName = "cw.staff3";
			staff3.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_LoginName = "cw.staff4";
			staff4.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Desc = "cw.group1";
			group1.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Desc = "cw.group2";
			group2.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Desc = "cw.group3";
			group3.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Desc = "cw.group4";
			group4.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1);
			Factory.Save();

			//We test it today
			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value;

			//staff1 and group1 should return a match entry
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff1.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntry1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(group1.GG_Desc, TestConstants.ValidOU)).Returns(groupDirectoryEntry1);

			//staff2's and group2's linked entry is missing so they will fail with exception
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff2.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff2.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(group2.GG_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(group2.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);

			// staff3, staff4 and group3, group4 link to the same AD entry so they will be in conflict when link
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff3.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntryX);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff4.GS_LoginName, TestConstants.ValidOU)).Returns(directoryEntryX);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(group3.GG_Desc, TestConstants.ValidOU)).Returns(groupDirectoryEntryX);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(group4.GG_Desc, TestConstants.ValidOU)).Returns(groupDirectoryEntryX);

			directorySearcherMock = DirectorySearcherProviderSubstitution.DirectorySearcherMock;
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			syncer.Synchronise();

			//staff2, 3, 4, group2, 3, 4 should sync with errors and should be recorded
			AssertEquals("Should have 6 sync errors", 6, syncer.EntitiesWithErrors.Count());
			var failedSyncPKs = syncer.EntitiesWithErrors.Select(s => ((BusinessObject)s.EnterpriseEntity).PK);
			AssertCollectionContains("staff2 should be in EntitiesWithErrors", staff2.PK, failedSyncPKs);
			AssertCollectionContains("staff3 should be in EntitiesWithErrors", staff3.PK, failedSyncPKs);
			AssertCollectionContains("staff4 should be in EntitiesWithErrors", staff4.PK, failedSyncPKs);

			AssertCollectionContains("group2 should be in EntitiesWithErrors", group2.PK, failedSyncPKs);
			AssertCollectionContains("group3 should be in EntitiesWithErrors", group3.PK, failedSyncPKs);
			AssertCollectionContains("group4 should be in EntitiesWithErrors", group4.PK, failedSyncPKs);

			//staff1 and group1 should be synced as they have no error
			AssertEquals("staff1 should be linked", directoryEntry1.Guid, staff1.GS_ActiveDirectoryObjectGuid);
			AssertEquals("directoryEntry1 should be synced", "cw.staff1", directoryEntry1.GetValue(GlbStaffSchema.GS_LoginName));

			AssertEquals("group1 should be linked", groupDirectoryEntry1.Guid, group1.GG_ActiveDirectoryObjectGuid);
			AssertEquals("groupDirectoryEntry1 should be synced", "cw.group1", groupDirectoryEntry1.GetValue(GlbGroupSchema.GG_Desc));
		}

		public void TestSync_ShouldIgnoreCachedTable()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("dummy1");

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "test1";
			staff.GS_UserAddress1 = "hobart";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (RowFactory.SetCachedTables(GlbStaff.Schema.TableName))
			{
				// Load staff to uber factory/cache
				AssertEquals("hobart", new BusinessObjectFactory().Load<GlbStaff>(staff.PK).GS_UserAddress1);

				// Update staff directly into DB without using factory
				var updateSql = string.Format(@"UPDATE {0} SET {1} = @address, GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E' where {2} = @pk",
					GlbStaff.Schema.TableName,
					GlbStaff.Schema.GS_UserAddress1,
					GlbStaff.Schema.PK);
				using (var command = TestConnection.Command(updateSql))
				{
					command.AddParameter("@address", SqlDbType.VarChar, "sydney");
					command.AddParameter("@pk", SqlDbType.UniqueIdentifier, staff.PK.ToGuid());
					command.ExecuteNonQuery();
				}

				// Cached data is used even with a new factory
				AssertEquals("Cached data should be used even with new factory", "hobart", new BusinessObjectFactory().Load<GlbStaff>(staff.PK).GS_UserAddress1);

				// But during sync, it should ignore cached data
				directorySearcherMock.Setup(x => x.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

				var syncer = new EntitySynchroniser(new BusinessObjectFactory());
				syncer.Synchronise();
				AssertEquals("Should sync from CW1", "test1", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
				AssertEquals("Should sync with new data reloaded from db", "sydney", directoryEntry.GetValue(GlbStaffSchema.GS_UserAddress1));
			}
		}

		public void TestSync_ReloadDataFromDB()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("dummy1");

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "test1";
			staff.GS_IsActive = true;
			staff.GS_UserAddress1 = "hobart";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			directorySearcherMock.Setup(x => x.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(directoryEntry);

			// factory2 preloaded with data and does not auto-refresh
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staffReloaded = factory2.Load<GlbStaff>(staff.PK);

			// staff is changed from another factory
			staff.GS_UserAddress1 = "sydney";
			Factory.Save();

			AssertEquals("Staff from factory2 should not be auto-refreshed", "hobart", staffReloaded.GS_UserAddress1);
			AssertEquals("factory2 should not be auto-refreshed", "hobart", factory2.Load<GlbStaff>(staff.PK).GS_UserAddress1);

			// Sync with factory2 and should reload data
			var syncer = new EntitySynchroniser(factory2);
			syncer.Synchronise();
			AssertEquals("Should sync with latest changes from CW1", "sydney", directoryEntry.GetValue(GlbStaffSchema.GS_UserAddress1));
		}

		public void TestSync_OnlyCallRefreshedCacheWhenSyncFromAD()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");

			var staff = Factory.New<GlbStaff>();
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			directorySearcherMock.Setup(x => x.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(dirEntry1);

			//AD -> CW
			AssertEquals(DateTime.MinValue, dirEntry1.CacheLastRefreshTime);
			syncer.Synchronise();
			AssertDateTimeWithinOneSecond("Sync with AD Master should refresh cache", DateTime.Now, dirEntry1.CacheLastRefreshTime);

			// CW -> AD
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			dirEntry1.CacheLastRefreshTime = DateTime.MinValue;

			AssertEquals(DateTime.MinValue, dirEntry1.CacheLastRefreshTime);
			syncer.Synchronise();
			AssertEquals("Sync with CW Master should not refresh cache", DateTime.MinValue, dirEntry1.CacheLastRefreshTime);
		}

		public void TestSync_AttributeMapping_CWMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1["otherMobile"] = "04000000";

			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "0412345678";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mobilePhoneMapItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_MobilePhone);
			mobilePhoneMapItem.ActiveDirectoryAttributeName = "otherMobile";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);

			syncer.Synchronise();

			AssertEquals("Unexpected directory otherMobile", "0412345678", dirEntry1["otherMobile"]);
		}

		public void TestSync_AttributeMapping_ADMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1["otherMobile"] = "04000000";

			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "0412345678";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mobilePhoneMapItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_MobilePhone);
			mobilePhoneMapItem.ActiveDirectoryAttributeName = "otherMobile";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);

			syncer.Synchronise();

			AssertEquals("Unexpected enterprise mobile phone", "04000000", staff.GS_MobilePhone);
		}

		public void TestSync_AttributeMapping_InvalidType_CWMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1["accountExpires"] = 123456789;

			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "0412345678";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mobilePhoneMapItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_MobilePhone);
			mobilePhoneMapItem.ActiveDirectoryAttributeName = "accountExpires";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);
			syncer.Synchronise();
			AssertContains(string.Format("Cannot synchronize {0} with AD attribute {1}. Please review the mappings in the {2} setting in the Registry.", "GS_MobilePhone", "accountExpires", ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).LocationMultilingual), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSync_AttributeMapping_InvalidType_ADMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1["accountExpires"] = 123456789;

			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "0412345678";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mobilePhoneMapItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_MobilePhone);
			mobilePhoneMapItem.ActiveDirectoryAttributeName = "accountExpires";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);
			syncer.Synchronise();
			AssertContains(string.Format("Cannot synchronize {0} with AD attribute {1}. Please review the mappings in the {2} setting in the Registry.", "GS_MobilePhone", "accountExpires", ((IMultilingualRegistryItem)ActiveDirectoryRegistry.Instance.AttributeMapping).LocationMultilingual), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSync_AttributeMapping_Name_CWMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1.SetLastModified(DateTime.Now.AddDays(-1));
			dirEntry1["name"] = "user";

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mappedItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_FullName);
			mappedItem.ActiveDirectoryAttributeName = "name";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);

			syncer.Synchronise();
			syncer.Save();
			var newFactory = new BusinessObjectFactory();
			var staffReloaded = newFactory.Load<GlbStaff>(staff.PK);

			AssertEquals("Unexpected directory attribute name", "John Smith", dirEntry1["name"]);
		}

		public void TestSync_AttributeMapping_Name_ADMaster()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.OneWay;

			var syncer = GetSyncer();

			var dirEntry1 = DummyDirectoryEntryWrapper.CreateUser("dummy1");
			dirEntry1["name"] = "user";

			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "John Smith";
			staff.GS_LoginName = "dummy1";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			var attributeMap = ActiveDirectoryRegistry.Instance.AttributeMapping.Value;
			var mappedItem = attributeMap.MapItems.Cast<AttributeMapItem>().First(i => i.EnterpriseColumnName == GlbStaffSchema.Constants.GS_FullName);
			mappedItem.ActiveDirectoryAttributeName = "name";
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attributeMap);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", TestConstants.ValidOU)).Returns(dirEntry1);

			syncer.Synchronise();

			AssertEquals("Unexpected enterprise full name", "user", staff.GS_FullName);
		}

		#region GDPR

		public void TestSync_OptOutThenOutInSavingPersonalDataToADAfterADChanged()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddDays(-1).ToDateTime());

			directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
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
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			staff.GS_SavePersonalDataToActiveDirectory = false;
			Factory.Save();

			GetSyncer().Synchronise();

			// AD details overriden by staff details
			AssertEquals("sauron", directoryEntry[ADAttributes.SAMAccountName]);
			AssertEquals("Sauron the Great", directoryEntry[ADAttributes.DisplayName]);

			// Personal data should be cleared
			AssertEquals("Mobile should be cleared", ZString.Empty, directoryEntry[ADAttributes.PhoneMobilePrimary]);
			AssertEquals("Address should be cleared", ZString.Empty, directoryEntry[ADAttributes.Address]);
			AssertEquals("City should be cleared", ZString.Empty, directoryEntry[ADAttributes.City]);
			AssertEquals("State should be cleared", ZString.Empty, directoryEntry[ADAttributes.State]);
			AssertEquals("Postcode should be cleared", ZString.Empty, directoryEntry[ADAttributes.PostalCode]);
			AssertEquals("HomePhone should be cleared", ZString.Empty, directoryEntry[ADAttributes.PhoneHomePrimary]);

			// staff now opt-in
			staff.GS_SavePersonalDataToActiveDirectory = true;
			Factory.Save();

			// AD change before sync
			directoryEntry[ADAttributes.DisplayName] = "Simpson the fat";
			directoryEntry[ADAttributes.TelephoneNumber] = "8888888";
			directoryEntry[ADAttributes.PhoneFaxOther] = "";
			directoryEntry[ADAttributes.PhoneHomePrimary] = "0202002";
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddSeconds(1).ToDateTime());
			directoryEntry.HasChanges = false;

			GetSyncer().Synchronise();

			// AD details overrides CW1 except personal data
			AssertEquals("Non-personal data should sync", "Simpson the fat", staff.GS_FullName);
			AssertEquals("Non-personal data should sync", "8888888", staff.GS_WorkPhone);
			AssertEquals("Empty non-personal data should sync", "", staff.GS_FaxNum);

			AssertEquals("Empty personal data should not sync", "Mobile phone", staff.GS_MobilePhone);
			AssertEquals("Non-empty personal data should sync", "0202002", staff.GS_HomePhone);
			AssertEquals("Address1", staff.GS_UserAddress1);
			AssertEquals("City", staff.GS_City);
			AssertEquals("State", staff.GS_State);
			AssertEquals("Postcode", staff.GS_Postcode);
		}

		#endregion

		#region EDI Group sync regex

		public void TestSync_GroupSyncRegex_DescOnly()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "^core|release group$");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = CreateTestGroup("G01", "Core Dev", "CA1");
			var group2 = CreateTestGroup("G02", "Accounting release group 1", "CA2");
			var group3 = CreateTestGroup("G03", "Accounting Release group", "CA1");
			var group4 = CreateTestGroup("G04", "DB Release group", "SE1");
			var group5 = CreateTestGroup("G05", "Accounting PAVE group", "SE2");
			var group6 = CreateTestGroup("G06", "Core group2", "CA1");
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var syncer = GetSyncer();
				syncer.Synchronise();

				AssertEquals($"group {group1.GG_Desc}", expected: true, group1.IsADLinked);
				AssertEquals($"group {group2.GG_Desc}", expected: false, group2.IsADLinked);
				AssertEquals($"group {group3.GG_Desc}", expected: true, group3.IsADLinked);
				AssertEquals($"group {group4.GG_Desc}", expected: true, group4.IsADLinked);
				AssertEquals($"group {group5.GG_Desc}", expected: false, group5.IsADLinked);
				AssertEquals($"group {group6.GG_Desc}", expected: true, group6.IsADLinked);
			}
		}

		public void TestSync_GroupSyncRegex_CategoryOnly()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Category=^CA1$|^S");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = CreateTestGroup("G01", "Core Dev", "CA1");
			var group2 = CreateTestGroup("G02", "Accounting release group 1", "CA2");
			var group3 = CreateTestGroup("G03", "Accounting Release group", "CA1");
			var group4 = CreateTestGroup("G04", "DB Release group", "SE1");
			var group5 = CreateTestGroup("G05", "Accounting PAVE group", "SE2");
			var group6 = CreateTestGroup("G06", "Core group2", "CA1");
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var syncer = GetSyncer();
				syncer.Synchronise();

				AssertEquals($"group {group1.GG_Desc} category {group1.GG_Category}", expected: true, group1.IsADLinked);
				AssertEquals($"group {group2.GG_Desc} category {group2.GG_Category}", expected: false, group2.IsADLinked);
				AssertEquals($"group {group3.GG_Desc} category {group3.GG_Category}", expected: true, group3.IsADLinked);
				AssertEquals($"group {group4.GG_Desc} category {group4.GG_Category}", expected: true, group4.IsADLinked);
				AssertEquals($"group {group5.GG_Desc} category {group5.GG_Category}", expected: true, group5.IsADLinked);
				AssertEquals($"group {group6.GG_Desc} category {group6.GG_Category}", expected: true, group6.IsADLinked);
			}
		}

		public void TestSync_GroupSyncRegex_DescAndCategory()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "^Accounting&&Category=^CA1$");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = CreateTestGroup("G01", "Core Dev", "CA1");
			var group2 = CreateTestGroup("G02", "Accounting release group 1", "CA2");
			var group3 = CreateTestGroup("G03", "Accounting Release group", "CA1");
			var group4 = CreateTestGroup("G04", "DB Release group", "SE1");
			var group5 = CreateTestGroup("G05", "Accounting PAVE group", "SE2");
			var group6 = CreateTestGroup("G06", "Core group2", "CA1");
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var syncer = GetSyncer();
				syncer.Synchronise();

				AssertEquals($"group {group1.GG_Desc} category {group1.GG_Category}", expected: false, group1.IsADLinked);
				AssertEquals($"group {group2.GG_Desc} category {group2.GG_Category}", expected: false, group2.IsADLinked);
				AssertEquals($"group {group3.GG_Desc} category {group3.GG_Category}", expected: true, group3.IsADLinked);
				AssertEquals($"group {group4.GG_Desc} category {group4.GG_Category}", expected: false, group4.IsADLinked);
				AssertEquals($"group {group5.GG_Desc} category {group5.GG_Category}", expected: false, group5.IsADLinked);
				AssertEquals($"group {group6.GG_Desc} category {group6.GG_Category}", expected: false, group6.IsADLinked);
			}
		}

		public void TestSync_GroupSyncRegex_DescOrCategory()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "^Accounting||Category=^CA1$");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = CreateTestGroup("G01", "Core Dev", "CA1");
			var group2 = CreateTestGroup("G02", "Accounting release group 1", "CA2");
			var group3 = CreateTestGroup("G03", "Accounting Release group", "CA1");
			var group4 = CreateTestGroup("G04", "DB Release group", "SE1");
			var group5 = CreateTestGroup("G05", "Accounting PAVE group", "SE2");
			var group6 = CreateTestGroup("G06", "Core group2", "CA1");
			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var syncer = GetSyncer();
				syncer.Synchronise();

				AssertEquals($"group {group1.GG_Desc} category {group1.GG_Category}", expected: true, group1.IsADLinked);
				AssertEquals($"group {group2.GG_Desc} category {group2.GG_Category}", expected: true, group2.IsADLinked);
				AssertEquals($"group {group3.GG_Desc} category {group3.GG_Category}", expected: true, group3.IsADLinked);
				AssertEquals($"group {group4.GG_Desc} category {group4.GG_Category}", expected: false, group4.IsADLinked);
				AssertEquals($"group {group5.GG_Desc} category {group5.GG_Category}", expected: true, group5.IsADLinked);
				AssertEquals($"group {group6.GG_Desc} category {group6.GG_Category}", expected: true, group6.IsADLinked);
			}
		}

		public void TestSync_GroupSyncRegex_CanHandleException()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			using (ActiveDirectoryRegistry.Instance.GroupSyncRegex.DataType.SuspendValidation())
			using (ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "^Core|*^core"))
			{
				var group1 = Factory.New<GlbGroup>();
				group1.GG_Code = "gg1";
				group1.GG_Desc = "Core Dev";

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
				directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
				{
					var syncer = GetSyncer();
					syncer.Synchronise();

					AssertEquals($"Group {group1.GG_Desc} that matches should not be sync as there is an error in the regex", expected: false, group1.IsADLinked);
				}
			}
		}

		public void TestSync_GroupSyncRegex_VerifySyntax()
		{
			List<GlbGroup> groups;
			groups =
			[
				CreateTestGroup("G01", "Dev", "SE1"),
				CreateTestGroup("G02", "Developer", "SE2"),
				CreateTestGroup("G03", "Diver", "CA1"),
				CreateTestGroup("G04", "Driver", "CA2"),
				CreateTestGroup("G05", "Prod", "SE1"),
				CreateTestGroup("G06", "Producer", "SE2"),
				CreateTestGroup("G07", "Product", "CA1"),
				CreateTestGroup("G08", "Production", "CA2"),
				CreateTestGroup("G09", "Director", "SE2"),
				CreateTestGroup("G10", "Secretary", "CA3"),
				CreateTestGroup("G11", "President", "SE3"),
				CreateTestGroup("G12", "Deputy President", "SS2"),
				CreateTestGroup("G13", "SE2", "SE1"),
				CreateTestGroup("G14", "Category=SE2", "CA2"),
			];

			// Desc
			AssertRegexMatches(groups, "^Dev", 2);
			AssertRegexMatches(groups, "^Category=SE2", 1);

			// Category
			AssertRegexMatches(groups, "Category=SE1", 3);

			// Desc OR Category
			AssertRegexMatches(groups, "^Dev|tion$||Category=CA2|SE2", 7);
			AssertRegexMatches(groups, "Dev.*   ||   Category = SE1  ", 4);

			// Desc AND Category
			AssertRegexMatches(groups, "^Dev|tion$&&Category=CA2|SE2", 2);
			AssertRegexMatches(groups, "Dev.*   &&   Category = SE1  ", 1);

			// Invalid multiple ANDs/ORs
			AssertRegexMatches(groups, "Dev.*||Category=SE1||Category=CA2", 0);
			AssertRegexMatches(groups, "Dev.*&&Category=SE1&&Category=CA2", 0);
			AssertRegexMatches(groups, "Dev.*&&Category=SE1||Category=CA2", 0);
			AssertRegexMatches(groups, "&&Category=SE1", 0);
			AssertRegexMatches(groups, "||Category=SE1", 0);
		}

		void AssertRegexMatches(List<GlbGroup> groups, string pattern, int expectedCount)
		{
			var result = EntitySynchroniser.GetGroupsMatchWithRegex(groups, pattern);
			AssertEquals($"Regex : {pattern}", expectedCount, result.Count());
		}

		GlbGroup CreateTestGroup(string code, string desc, string category)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = code;
			group.GG_Desc = desc;
			group.GG_Category = category;
			return group;
		}

		public void TestSync_GroupSyncRegex_NonEDI()
		{
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.EntitiesToSync = EntitiesToSync.UsersAndGroups;
			ActiveDirectoryRegistry.Instance.GroupSyncRegex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "^syd|ane$");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group1 = Factory.New<GlbGroup>();
			group1.GG_Code = "gg1";
			group1.GG_Desc = "Sydney";

			var group2 = Factory.New<GlbGroup>();
			group2.GG_Code = "gg2";
			group2.GG_Desc = "Melbourne";

			var group3 = Factory.New<GlbGroup>();
			group3.GG_Code = "gg3";
			group3.GG_Desc = "brisbane";

			Factory.Save();

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("synced");
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			var syncer = GetSyncer();
			syncer.Synchronise();

			AssertEquals($"group {group1.GG_Desc}", expected: true, group1.IsADLinked);
			AssertEquals($"group {group2.GG_Desc}", expected: true, group2.IsADLinked);
			AssertEquals($"group {group3.GG_Desc}", expected: true, group3.IsADLinked);
		}

		#endregion

		#region PasswordDoesntMatchPolicyException

		public void TestSync_PasswordDoesntMatchPolicyException()
		{
			var syncer = GetSyncer();

			var linkedStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			linkedStaff1.GS_LoginName = "linked1";
			linkedStaff1.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			var unlinkedStaff = Factory.NewWithValidTestData<GlbStaff>();
			unlinkedStaff.GS_LoginName = "unlinked";
			unlinkedStaff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid; // so it triggers new AD user to be created

			var linkedStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			linkedStaff2.GS_LoginName = "linked2";
			linkedStaff2.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();

			Factory.Save();

			var directoryEntry = new Mock<IUserDirectoryEntry>();
			directoryEntry.Setup(x => x.SetPassword(It.IsAny<string>(), It.IsAny<bool>())).Throws(new PasswordDoesNotMatchPolicyException("Password does not match policy."));

			var organisationalUnit = new Mock<IOrganisationalUnit>();
			organisationalUnit.Setup(x => x.CreateNewChild(unlinkedStaff.GS_LoginName, unlinkedStaff.GS_LoginName, unlinkedStaff.GS_LoginName, DirectoryObjectType.User, directorySearcherMock.Object)).
				Returns(directoryEntry.Object);

			directorySearcherMock.Setup(s => s.FindUser(linkedStaff1.GS_LoginName, TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser(linkedStaff1.GS_LoginName, "AD Linked 1"));
			directorySearcherMock.Setup(s => s.FindUser(linkedStaff2.GS_LoginName, TestConstants.ValidOU)).Returns(DummyDirectoryEntryWrapper.CreateUser(linkedStaff2.GS_LoginName, "AD Linked 2"));
			directorySearcherMock.Setup(s => s.FindUser(unlinkedStaff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);

			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value[0].DefaultPasswordFailsToMeetDomainPolicy = false;
			AssertExceptionThrown<PasswordDoesNotMatchPolicyException>(() => syncer.Synchronise());

			AssertEquals("LinkedStaff1 should be sync", "AD Linked 1", linkedStaff1.GS_FullName);
			AssertEquals("LinkedStaff2 should be sync", "AD Linked 2", linkedStaff2.GS_FullName);
			AssertEquals("UnlinkedStaff is still active", true, unlinkedStaff.GS_IsActive);
			AssertEquals("UnlinkedStaff's ADObjectGuid is still Invalid", ZGuid.Invalid, unlinkedStaff.GS_ActiveDirectoryObjectGuid);
			AssertEquals("DefaultPasswordFailsToMeetDomainPolicy should be set", true, ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value[0].DefaultPasswordFailsToMeetDomainPolicy);
		}

		#endregion
	}

	[TestDate]
	class EntitySynchroniserGroupMembershipsSyncTest : TestCaseWithFactoryAndMocks
	{
		protected override void SetUp()
		{
			base.SetUp();

			Helper.PurgeAllGroups();
			Helper.PurgeAllStaff();

			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.ADIsMaster;
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.UtcNow);

			group = Factory.New<GlbGroup>();
			group.GG_Desc = "BigGroup";
			group.GG_DomainName = TestConstants.Domain;
			adGroup = DummyDirectoryEntryWrapper.CreateGroup(group.GG_Desc);
			group.GG_ActiveDirectoryObjectGuid = adGroup.Guid;

			staff1 = group.Staff.AddNew();
			staff1.GS_LoginName = "Ann";
			staff1.GS_DomainName = TestConstants.Domain;
			adUser1 = DummyDirectoryEntryWrapper.CreateUser(staff1.GS_LoginName);
			staff1.GS_ActiveDirectoryObjectGuid = adUser1.Guid;

			staff2 = group.Staff.AddNew();
			staff2.GS_LoginName = "Bob";
			staff2.GS_DomainName = TestConstants.Domain;
			adUser2 = DummyDirectoryEntryWrapper.CreateUser(staff2.GS_LoginName);
			staff2.GS_ActiveDirectoryObjectGuid = adUser2.Guid;

			// staff not in group
			staff3 = Factory.New<GlbStaff>();
			staff3.GS_LoginName = "Cal";
			staff3.GS_DomainName = TestConstants.Domain;
			adUser3 = DummyDirectoryEntryWrapper.CreateUser(staff3.GS_LoginName);
			staff3.GS_ActiveDirectoryObjectGuid = adUser3.Guid;

			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1); // To ensure they won't be picked up for sync
			Factory.Save();

			adGroup.AddMember(adUser1);
			adGroup.AddMember(adUser2);
			adGroup.SetLastModified(ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(-1)); // To ensure they won't be picked up for sync

			// Pre-conditions checks - group members are current in-sync
			AssertEquals(2, group.Staff.Count);
			AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == staff1.GS_LoginName);
			AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == staff2.GS_LoginName);

			AssertEquals(2, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff1.GS_LoginName);
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff2.GS_LoginName);

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(adUser1.Guid, TestConstants.ValidOU)).Returns(adUser1);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(adUser2.Guid, TestConstants.ValidOU)).Returns(adUser2);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(adUser3.Guid, TestConstants.ValidOU)).Returns(adUser3);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(adGroup.Guid, TestConstants.ValidOU)).Returns(adGroup);

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;

			TestDateAttribute.Date = ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value.AddDays(1); // To ensure new changes get synced
		}

		GlbGroup group;
		GlbStaff staff1;
		GlbStaff staff2;
		GlbStaff staff3;

		DummyGroupDirectoryEntryWrapper adGroup;
		DummyDirectoryEntryWrapper adUser1;
		DummyDirectoryEntryWrapper adUser2;
		DummyDirectoryEntryWrapper adUser3;

		public void TestGroupAddedFromStaff()
		{
			staff3.Groups.Add(group);
			Factory.Save();

			var syncer = new EntitySynchroniser(new BusinessObjectFactory());
			syncer.Synchronise();

			AssertEquals(3, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff1.GS_LoginName);
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff2.GS_LoginName);
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff3.GS_LoginName);
		}

		public void TestGroupRemovedFromStaff()
		{
			staff2.Groups.Remove(group);
			Factory.Save();

			var syncer = new EntitySynchroniser(new BusinessObjectFactory());
			syncer.Synchronise();

			AssertEquals(1, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff1.GS_LoginName);
		}

		public void TestStaffAddedFromGroup()
		{
			group.Staff.Add(staff3);
			Factory.Save();

			var syncer = new EntitySynchroniser(new BusinessObjectFactory());
			syncer.Synchronise();

			AssertEquals(3, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff1.GS_LoginName);
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff2.GS_LoginName);
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff3.GS_LoginName);
		}

		public void TestStaffRemovedFromGroup()
		{
			group.Staff.Remove(staff1);
			Factory.Save();

			var syncer = new EntitySynchroniser(new BusinessObjectFactory());
			syncer.Synchronise();

			AssertEquals(1, adGroup.GetMembers().Count());
			AssertCollectionContains(adGroup.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == staff2.GS_LoginName);
		}
	}
}
