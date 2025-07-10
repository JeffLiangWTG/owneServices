using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test.Synchronisation
{
	abstract class OneWaySyncTest : EntitySynchroniserTest_Common
	{
		protected override SyncDirection SyncDirection => SyncDirection.OneWay;

		#region Abstract tests
		public abstract void TestSync_ActiveUnLinkedStaff_WhenMatchFoundOutsideOU();

		#endregion

		#region SyncMode.ADIsMaster

		class ADIsMasterTest : OneWaySyncTest
		{
			protected override SyncMode SyncMode => SyncMode.ADIsMaster;

			protected override int TestSave_CommitChangesOnSyncedEntities_ExpectedCommit => 0;

			protected override void AssertOptOutSavingPersonalDataToAD(GlbStaff staff) => AssertOptOutSavingPersonalDataToAD_FromAD(staff);

			#region Active unlinked accounts

			public override void TestSync_ActiveUnLinkedStaff_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", staff.GS_LoginName);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
				AssertEquals("Lord Sauron", staff.GS_FullName);
				AssertEquals("sauron@mordor.com", staff.GS_EmailAddress);
				AssertEquals("0412345678", staff.GS_MobilePhone);
				AssertEquals("Lord", staff.GS_Title);
				AssertEquals("1 Barad Dur Way", staff.GS_UserAddress1);
				AssertEquals("Gorgoroth", staff.GS_City);
				AssertEquals("Mordor", staff.GS_State);
				AssertEquals("1111", staff.GS_Postcode);
				AssertEquals("0294811111", staff.GS_WorkPhone);
				AssertEquals("0294811110", staff.GS_FaxNum);
				AssertEquals("0294811111", staff.GS_HomePhone);
				AssertEquals("123456", staff.GS_Pager);
			}

			public override void TestSync_ActiveUnLinkedStaff_WhenMatchFoundOutsideOU()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", string.Empty)).Returns(directoryEntry);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals("Should be deactived", false, staff.GS_IsActive);
				AssertEquals("Should be unlinked", false, staff.IsADLinked);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			//Initial sync when AD is master - disable
			public override void TestSync_ActiveUnLinkedStaff_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns((IUserDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals("When AD is master, Staff should be deactivated when no match found", false, staff.GS_IsActive);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			//Initial sync when AD is master - disable
			public override void TestSync_ActiveUnLinkedGroup_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "Valar";
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var staff = group.Staff.AddNew();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, string.Empty)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(It.IsAny<string>(), TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals("When AD is master, Group should be deactivated when no match found", false, group.GG_IsActive);
				AssertEquals("Group member shouldn't change", 1, group.Staff.Count);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			public override void TestSync_ActiveUnLinkedGroup_ShouldActivateWhenMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "Valar";
				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			#endregion

			#region Active linked accounts

			//Linked staff should sync with latest, and AD is the latest
			[TestDate(2013, 10, 24, 9, 0, 0)]
			public override void TestSync_ActiveLinkedStaff_ShouldSync()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
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
				staff.GS_PasswordHash = ZBlob.FromAscii("booo");
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				//Set AD to be the latest so it sync from AD->CW1
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());

				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				Assert(staff.GS_IsActive);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();
				syncer.Save();

				var loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				Assert(loadedStaff.GS_IsActive);
				Assert(loadedStaff.GS_ActiveDirectoryObjectGuid.IsValid);

				AssertEquals("lord.sauron", loadedStaff.GS_LoginName);
				AssertEquals("Password Hash will be set to empty when sync with AD is master", ZBlob.Empty, loadedStaff.GS_PasswordHash);

				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();
				AssertEquals(17, histories.Length);

				AssertHistory(histories[0], "lord.sauron", "sauron", "lord.sauron", GlbStaffSchema.Constants.GS_LoginName);
				AssertHistory(histories[1], TestConstants.Domain, "", TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
				AssertHistory(histories[4], "Lord", "Title", "Lord", GlbStaffSchema.Constants.GS_Title);
				AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Lord Sauron", GlbStaffSchema.Constants.GS_FullName);
				AssertHistory(histories[6], "sauron@mordor.com", "Email", "sauron@mordor.com", GlbStaffSchema.Constants.GS_EmailAddress);
				AssertHistory(histories[7], "123", "Ext", "123", GlbStaffSchema.Constants.GS_WorkExtension);
				AssertHistory(histories[8], "0294811111", "Work phone", "0294811111", GlbStaffSchema.Constants.GS_WorkPhone);
				AssertHistory(histories[9], "0412345678", "Mobile phone", "0412345678", GlbStaffSchema.Constants.GS_MobilePhone);
				AssertHistory(histories[10], "0294811110", "Fax num", "0294811110", GlbStaffSchema.Constants.GS_FaxNum);
				AssertHistory(histories[11], "0294811111", "Home phone", "0294811111", GlbStaffSchema.Constants.GS_HomePhone);
				AssertHistory(histories[12], "123456", "Pager", "123456", GlbStaffSchema.Constants.GS_Pager);
				AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "1 Barad Dur Way", GlbStaffSchema.Constants.GS_UserAddress1);
				AssertHistory(histories[14], "Gorgoroth", "City", "Gorgoroth", GlbStaffSchema.Constants.GS_City);
				AssertHistory(histories[15], "Mordor", "State", "Mordor", GlbStaffSchema.Constants.GS_State);
				AssertHistory(histories[16], "1111", "Postcode", "1111", GlbStaffSchema.Constants.GS_Postcode);
			}

			[TestDate(2013, 10, 24, 9, 0, 0)]
			public override void TestSync_WithPreferredSyncMode()
			{
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay; //to prove it doesn't change this behaviour
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
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
				staff.StaffPlainTextPassword = "booo";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				//Set AD to be the latest
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());

				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				Assert(staff.GS_IsActive);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise(preferredSyncMode: SyncMode.EnterpriseIsMaster);
				syncer.Save();

				var loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				var expectedPassword = new TwoWayEncoder(loadedStaff.PK.ToGuid()).Encrypt("D4t P4s5wr0D");

				Assert(loadedStaff.GS_IsActive);
				Assert(loadedStaff.GS_ActiveDirectoryObjectGuid.IsValid);

				//AD is latest, but when SyncDirecton is OneWay, we ignore preferredSyncMode
				AssertEquals("lord.sauron", loadedStaff.GS_LoginName);

				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();
				AssertEquals(17, histories.Length);

				AssertHistory(histories[0], "lord.sauron", "sauron", "lord.sauron", GlbStaffSchema.Constants.GS_LoginName);
				AssertHistory(histories[1], TestConstants.Domain, "", TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
				AssertHistory(histories[4], "Lord", "Title", "Lord", GlbStaffSchema.Constants.GS_Title);
				AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Lord Sauron", GlbStaffSchema.Constants.GS_FullName);
				AssertHistory(histories[6], "sauron@mordor.com", "Email", "sauron@mordor.com", GlbStaffSchema.Constants.GS_EmailAddress);
				AssertHistory(histories[7], "123", "Ext", "123", GlbStaffSchema.Constants.GS_WorkExtension);
				AssertHistory(histories[8], "0294811111", "Work phone", "0294811111", GlbStaffSchema.Constants.GS_WorkPhone);
				AssertHistory(histories[9], "0412345678", "Mobile phone", "0412345678", GlbStaffSchema.Constants.GS_MobilePhone);
				AssertHistory(histories[10], "0294811110", "Fax num", "0294811110", GlbStaffSchema.Constants.GS_FaxNum);
				AssertHistory(histories[11], "0294811111", "Home phone", "0294811111", GlbStaffSchema.Constants.GS_HomePhone);
				AssertHistory(histories[12], "123456", "Pager", "123456", GlbStaffSchema.Constants.GS_Pager);
				AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "1 Barad Dur Way", GlbStaffSchema.Constants.GS_UserAddress1);
				AssertHistory(histories[14], "Gorgoroth", "City", "Gorgoroth", GlbStaffSchema.Constants.GS_City);
				AssertHistory(histories[15], "Mordor", "State", "Mordor", GlbStaffSchema.Constants.GS_State);
				AssertHistory(histories[16], "1111", "Postcode", "1111", GlbStaffSchema.Constants.GS_Postcode);
			}

			[TestDate(2013, 10, 24, 9, 0, 0)]
			public void TestSync_ADEnableDoesNotChangeEmploymentDateAndDepartureDate()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				//AD user is active
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.ToDateTime());

				//CW1 staff is inactive but linked
				var employmentDate = new ZDateTime(2010, 10, 11);
				var departureDate = new ZDateTime(2017, 1, 1);
				var staff = Factory.New<GlbStaff>();
				staff.GS_IsActive = false;
				staff.GS_EmploymentDate = employmentDate;
				staff.GS_DepartureDate = departureDate;
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				Assert(!staff.GS_IsActive);
				AssertEquals(employmentDate, staff.GS_EmploymentDate);
				AssertEquals(departureDate, staff.GS_DepartureDate);

				var adUser = new Mock<IADUser>();
				adUser.Setup(a => a.HasExistingDirectoryEntry()).Returns(true);
				ADEntityProviderSubstitution.ADUser = adUser.Object;

				//Set AD to be the latest so it sync from AD->CW1
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();
				syncer.Save();

				//after sync, CW1 staff should be activated, but employment date and departure date are not changed
				var loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				AssertEquals("lord.sauron", loadedStaff.GS_LoginName);
				Assert(loadedStaff.GS_IsActive);
				Assert(loadedStaff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(employmentDate, staff.GS_EmploymentDate);
				AssertEquals(departureDate, loadedStaff.GS_DepartureDate);
				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();
				AssertEquals(17, histories.Length);
				AssertHistory(histories[3], ZBool.True, ZBool.False, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
			}

			[TestDate(2013, 10, 24, 9, 0, 0)]
			public void TestSync_ADDisableDoesNotChangeEmploymentDateAndDepartureDate()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				//AD user is inactive
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", active: false);
				directoryEntry.SetLastModified(ZDateTime.UtcNow.ToDateTime());

				//CW1 staff is active and linked
				var employmentDate = new ZDateTime(2010, 10, 11);
				var staff = Factory.New<GlbStaff>();
				staff.GS_IsActive = true;
				staff.GS_EmploymentDate = employmentDate;
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				//Set AD to be the latest so it sync from AD->CW1
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				Assert(staff.GS_IsActive);
				AssertEquals(employmentDate, staff.GS_EmploymentDate);
				AssertEquals(default(ZDateTime), staff.GS_DepartureDate);

				//sync, this will be the first sync so it follow the sync mode: AD -> CW1
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();
				syncer.Save();

				//after sync, CW1 staff should be deactivated, but employment date and departure date are not changed
				var loadedStaff = new BusinessObjectFactory().Load<GlbStaff>(staff.PK);
				AssertEquals("lord.sauron", loadedStaff.GS_LoginName);
				Assert(!loadedStaff.GS_IsActive);
				Assert(loadedStaff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(employmentDate, staff.GS_EmploymentDate);
				AssertEquals(default(ZDateTime), loadedStaff.GS_DepartureDate);
				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();
				AssertEquals(17, histories.Length);
				AssertHistory(histories[3], ZBool.False, ZBool.True, ZBool.False, GlbStaffSchema.Constants.GS_IsActive);
			}

			// Linked group should sync with latest, AD is the latest
			public override void TestSync_ActiveLinkedGroup_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				Assert(group.GG_IsActive);

				group.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				//Set AD to be the latest so it sync from AD->CW1
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());

				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				//Using the opposite preferredSyncMode will be ignored.
				syncer.Synchronise(preferredSyncMode: SyncMode.EnterpriseIsMaster);

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			#endregion

			#region Group membership

			// Replication only happens at initial sync in either sync mode
			public override void TestSync_ActiveLinkedGroup_ShouldReplicateGroupMembership()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser($"thom.yorke@{TestConstants.Domain}");

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser($"robert.delnaja@{TestConstants.Domain}");

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				staff3.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser($"pj.harvey@{TestConstants.Domain}");

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == $"thom.yorke@{TestConstants.Domain}");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == $"pj.harvey@{TestConstants.Domain}");

				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty; // so it will do the initial sync and follow the sync mode, which will do replication
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;
				Factory.Save();

				directorySearcherMock.Setup(s => s.FindGroup("singers", TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindGroup(groupEntry.Guid, TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);

				syncer.Synchronise();

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "pj.harvey");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == $"thom.yorke@{TestConstants.Domain}");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == $"pj.harvey@{TestConstants.Domain}");
			}

			#endregion

			protected override void AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbStaff[] staff, DummyDirectoryEntryWrapper[] directoryEntry)
			{
				// Staff1: No sync - staff1 and directoryEntry1 were both modified before LastSyncUtc
				AssertEquals("staff1 should not be synced", "cw.staff1", staff[0].GS_LoginName);
				AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry[0].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff2: AD -> CW1 - directoryEntry2 was modified after LastSyncUtc and staff2 was modified before
				AssertEquals("staff2 should be synced", "ad.user2", staff[1].GS_LoginName);
				AssertEquals("directoryEntry2 should not be changed", "ad.user2", directoryEntry[1].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff3: AD -> CW - staff3 was modified after LastSyncUtc so it get picked up to sync, but this is ADIsMater+1Way, so CW1 will be overridden by AD
				AssertEquals("staff3 should not be changed", "ad.user3", staff[2].GS_LoginName);
				AssertEquals("directoryEntry3 should be synced", "ad.user3", directoryEntry[2].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff4: AD -> CW1 - when both modified after LastSyncUtc, use AD because ADIsMater+1Way
				AssertEquals("staff4 should not changed", "ad.user4", staff[3].GS_LoginName);
				AssertEquals("directoryEntry4 should be synced", "ad.user4", directoryEntry[3].GetValue(GlbStaffSchema.GS_LoginName));
			}

			protected override void AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbGroup[] group, DummyDirectoryEntryWrapper[] groupDirectoryEntry)
			{
				// Group1: No sync - group1 and groupDirectoryEntry1 were both modified before LastSyncUtc
				AssertEquals("group1 should not be synced", "cw.group1", group[0].GG_Desc);
				AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry[0].GetValue(GlbGroupSchema.GG_Desc));

				// Group2: AD -> CW1 - groupDirectoryEntry2 was modified after LastSyncUtc and group2 was modified before
				AssertEquals("group2 should be synced", "ad.group2", group[1].GG_Desc);
				AssertEquals("groupDirectoryEntry2 should not be changed", "ad.group2", groupDirectoryEntry[1].GetValue(GlbGroupSchema.GG_Desc));

				// Group3: AD -> CW - group3 was modified after LastSyncUtc so it get picked up to sync, but this is ADIsMater+1Way, so CW1 will be overridden by AD
				AssertEquals("group3 should not be changed", "ad.group3", group[2].GG_Desc);
				AssertEquals("groupDirectoryEntry3 should be synced", "ad.group3", groupDirectoryEntry[2].GetValue(GlbGroupSchema.GG_Desc));

				// Group4: AD -> CW1 - when both modified after LastSyncUtc, use AD because ADIsMater+1Way
				AssertEquals("group4 should not changed", "ad.group4", group[3].GG_Desc);
				AssertEquals("groupDirectoryEntry4 should be synced", "ad.group4", groupDirectoryEntry[3].GetValue(GlbGroupSchema.GG_Desc));
			}
		}

		#endregion

		#region SyncMode.EnterpriseIsMaster

		class EnterpriseIsMasterTest : OneWaySyncTest
		{
			protected override SyncMode SyncMode => SyncMode.EnterpriseIsMaster;

			protected override void AssertOptOutSavingPersonalDataToAD(GlbStaff staff) => AssertOptOutSavingPersonalDataToAD_FromCW(staff);

			#region Active unlinked accounts

			public override void TestSync_ActiveUnLinkedStaff_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_FullName = "Lord Sauron";
				staff.GS_EmailAddress = "sauron@mordor.com";
				staff.GS_MobilePhone = "0412345678";
				staff.GS_Title = "Lord";
				staff.GS_UserAddress1 = "1 Barad Dur Way";
				staff.GS_City = "Gorgoroth";
				staff.GS_State = "Mordor";
				staff.GS_Postcode = "1111";
				staff.GS_WorkPhone = "0294811111";
				staff.GS_FaxNum = "0294811110";
				staff.GS_HomePhone = "0294811111";
				staff.GS_Pager = "123456";

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				var directoryEntry = new DummyDirectoryEntryWrapper("lord.sauron", ZDateTime.UtcNow.ToDateTime(), "lord.sauron", "", "", "", 1, "", "", "", "", "", "", "", "", "");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddSeconds(-1).ToDateTime());

				Factory.Save();

				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				var adUser = new ADUser(staff);
				Assert(adUser.IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", adUser.LoginName);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
				AssertEquals("Lord Sauron", adUser.FullName);
				AssertEquals("sauron@mordor.com", adUser.EmailAddress);
				AssertEquals("0412345678", adUser.MobilePhone);
				AssertEquals("Lord", adUser.Title);
				AssertEquals("1 Barad Dur Way", adUser.StreetAddress);
				AssertEquals("Gorgoroth", adUser.City);
				AssertEquals("Mordor", adUser.State);
				AssertEquals("1111", adUser.Postcode);
				AssertEquals("0294811111", adUser.WorkPhone);
				AssertEquals("0294811110", adUser.FaxNum);
				AssertEquals("0294811111", adUser.HomePhone);
				AssertEquals("123456", adUser.Pager);
			}

			public override void TestSync_ActiveUnLinkedStaff_WhenMatchFoundOutsideOU()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", string.Empty)).Returns(directoryEntry);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals("Should be kept active", true, staff.GS_IsActive);
				AssertEquals("Should be unlinked", false, staff.IsADLinked);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			//Initial sync when CW1 is master - remain active and flag to create new
			public override void TestSync_ActiveUnLinkedStaff_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns((IUserDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals("When CW1 is master, Staff should remain active when no match found", true, staff.GS_IsActive);
				AssertEquals("LoginName unchange", "lord.sauron", staff.GS_LoginName);
				AssertEquals("Should flag to create new", ZGuid.Invalid, staff.GS_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			//Initial sync when CW1 is master - remain active and flag to create new
			public override void TestSync_ActiveUnLinkedGroup_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "Valar";
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var staff = group.Staff.AddNew();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, string.Empty)).Returns((IGroupDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals("When CW1 is master, Group should remain active when no match found", true, group.GG_IsActive);
				AssertEquals("Should flag to create new", ZGuid.Invalid, group.GG_ActiveDirectoryObjectGuid);
				AssertEquals("Group member shouldn't change", 1, group.Staff.Count);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			public override void TestSync_ActiveUnLinkedGroup_ShouldActivateWhenMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "Valar";
				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");

				directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);
				AssertEquals(group.GG_ActiveDirectoryObjectGuid, directoryEntry.Guid);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			#endregion

			#region Active linked accounts

			// Linked staff should sync with latest, CW1 is the latest
			public override void TestSync_ActiveLinkedStaff_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //ensure CW1 to be the latest
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				Assert(staff.GS_IsActive);

				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			public override void TestSync_WithPreferredSyncMode()
			{
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay; //to prove it doesn't change this behaviour
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //ensure CW1 to be the latest
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				Assert(staff.GS_IsActive);

				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise(preferredSyncMode: SyncMode.ADIsMaster);

				//CW1 is latest, but when SyncDirecton is OneWay, we ignore preferredSyncMode
				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
				AssertEquals("sauron", staff.GS_LoginName);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			// Linked group should sync with latest, CW1 is the latest
			public override void TestSync_ActiveLinkedGroup_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //ensure CW1 to be the latest
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				Assert(group.GG_IsActive);
				group.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;
				Factory.Save();

				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				//Using the opposite preferredSyncMode will be ignored.
				syncer.Synchronise(preferredSyncMode: SyncMode.ADIsMaster);

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(".-Valar-.", directoryEntry.GetValue(GlbGroupSchema.GG_Desc));
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			#endregion

			#region Group membership

			// Replication only happens on initial sync in either sync mode
			public override void TestSync_ActiveLinkedGroup_ShouldReplicateGroupMembership()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("robert.delnaja");

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				staff3.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("pj.harvey");

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");

				group.GG_ActiveDirectoryObjectGuid = ZGuid.Empty; // So it will do the initial sync and follow the sync mode
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;
				Factory.Save();

				directorySearcherMock.Setup(s => s.FindGroup("singers", TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);

				syncer.Synchronise();

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "robert.delnaja");
			}
			#endregion

			protected override void AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbStaff[] staff, DummyDirectoryEntryWrapper[] directoryEntry)
			{
				// Staff1: No sync - staff1 and directoryEntry1 were both modified before LastSyncUtc
				AssertEquals("staff1 should not be synced", "cw.staff1", staff[0].GS_LoginName);
				AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry[0].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff2: No sync - directoryEntry2 was modified after LastSyncUtc but it is EnterpriseIsMaster+1Way and staff2 was modified before so it wont be picked to sync
				AssertEquals("staff2 should be synced", "cw.staff2", staff[1].GS_LoginName);
				AssertEquals("directoryEntry2 should not be changed", "ad.user2", directoryEntry[1].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff3: CW1 -> AD - staff3 was modified after LastSyncUtc and directoryEntry3 was modified before
				AssertEquals("staff3 should not be changed", "cw.staff3", staff[2].GS_LoginName);
				AssertEquals("directoryEntry3 should be synced", "cw.staff3", directoryEntry[2].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
				AssertEquals("staff4 should not changed", "cw.staff4", staff[3].GS_LoginName);
				AssertEquals("directoryEntry4 should be synced", "cw.staff4", directoryEntry[3].GetValue(GlbStaffSchema.GS_LoginName));
			}

			protected override void AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbGroup[] group, DummyDirectoryEntryWrapper[] groupDirectoryEntry)
			{
				// Group1: No sync - group1 and groupDirectoryEntry1 were both modified before LastSyncUtc
				AssertEquals("group1 should not be synced", "cw.group1", group[0].GG_Desc);
				AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry[0].GetValue(GlbGroupSchema.GG_Desc));

				// Group2: No sync - groupDirectoryEntry2 was modified after LastSyncUtc but it is EnterpriseIsMaster+1Way and group2 was modified before so it wont be picked to sync
				AssertEquals("group2 should be synced", "cw.group2", group[1].GG_Desc);
				AssertEquals("groupDirectoryEntry2 should not be changed", "ad.group2", groupDirectoryEntry[1].GetValue(GlbGroupSchema.GG_Desc));

				// Group3: CW1 -> AD - group3 was modified after LastSyncUtc and groupDirectoryEntry3 was modified before
				AssertEquals("group3 should not be changed", "cw.group3", group[2].GG_Desc);
				AssertEquals("groupDirectoryEntry3 should be synced", "cw.group3", groupDirectoryEntry[2].GetValue(GlbGroupSchema.GG_Desc));

				// Group4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
				AssertEquals("group4 should not changed", "cw.group4", group[3].GG_Desc);
				AssertEquals("groupDirectoryEntry4 should be synced", "cw.group4", groupDirectoryEntry[3].GetValue(GlbGroupSchema.GG_Desc));
			}
		}

		#endregion

		#region UseLatestRecord

		abstract class UseLatestRecordTest : OneWaySyncTest
		{
			public abstract void TestSync_ActiveLinkedStaff_LastEditInEnterprise_ShouldSync();

			public abstract void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldSync();

			#region Active unlinked accounts

			public abstract void TestSync_ActiveUnLinkedStaff_LastModifiedInEnterprise_ShouldActivateAndSyncWhenMatchFound();

			public override void TestSync_ActiveUnLinkedGroup_ShouldActivateWhenMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "Valar";
				Factory.Save();

				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				directorySearcherMock.Setup(s => s.FindGroup("Valar", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			#endregion

			#region Active linked accounts

			public override void TestSync_WithPreferredSyncMode()
			{
				ActiveDirectoryRegistry.Instance.SyncDirectionGroup = SyncDirection.TwoWay; //to prove it doesn't change this behaviour
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
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
				Factory.Save();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise(preferredSyncMode: SyncMode.EnterpriseIsMaster);

				//AD is latest, but when SyncDirecton is OneWay, we ignore preferredSyncMode
				//which means we just use the syncMode
				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();
				AssertEquals(17, histories.Length);

				if (SyncMode == SyncMode.ADIsMaster)
				{
					AssertEquals("lord.sauron", staff.GS_LoginName);
					AssertHistory(histories[0], "lord.sauron", "sauron", "lord.sauron", GlbStaffSchema.Constants.GS_LoginName);
					AssertHistory(histories[1], TestConstants.Domain, "", TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
					AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
					AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
					AssertHistory(histories[4], "Lord", "Title", "Lord", GlbStaffSchema.Constants.GS_Title);
					AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Lord Sauron", GlbStaffSchema.Constants.GS_FullName);
					AssertHistory(histories[6], "sauron@mordor.com", "Email", "sauron@mordor.com", GlbStaffSchema.Constants.GS_EmailAddress);
					AssertHistory(histories[7], "123", "Ext", "123", GlbStaffSchema.Constants.GS_WorkExtension);
					AssertHistory(histories[8], "0294811111", "Work phone", "0294811111", GlbStaffSchema.Constants.GS_WorkPhone);
					AssertHistory(histories[9], "0412345678", "Mobile phone", "0412345678", GlbStaffSchema.Constants.GS_MobilePhone);
					AssertHistory(histories[10], "0294811110", "Fax num", "0294811110", GlbStaffSchema.Constants.GS_FaxNum);
					AssertHistory(histories[11], "0294811111", "Home phone", "0294811111", GlbStaffSchema.Constants.GS_HomePhone);
					AssertHistory(histories[12], "123456", "Pager", "123456", GlbStaffSchema.Constants.GS_Pager);
					AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "1 Barad Dur Way", GlbStaffSchema.Constants.GS_UserAddress1);
					AssertHistory(histories[14], "Gorgoroth", "City", "Gorgoroth", GlbStaffSchema.Constants.GS_City);
					AssertHistory(histories[15], "Mordor", "State", "Mordor", GlbStaffSchema.Constants.GS_State);
					AssertHistory(histories[16], "1111", "Postcode", "1111", GlbStaffSchema.Constants.GS_Postcode);
				}
				else
				{
					AssertEquals("sauron", staff.GS_LoginName);
					AssertHistory(histories[0], "lord.sauron", "sauron", "sauron", GlbStaffSchema.Constants.GS_LoginName);
					AssertHistory(histories[1], TestConstants.Domain, "", TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
					AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
					AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
					AssertHistory(histories[4], "Lord", "Title", "Title", GlbStaffSchema.Constants.GS_Title);
					AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Sauron the Great", GlbStaffSchema.Constants.GS_FullName);
					AssertHistory(histories[6], "sauron@mordor.com", "Email", "Email", GlbStaffSchema.Constants.GS_EmailAddress);
					AssertHistory(histories[7], "123", "Ext", "Ext", GlbStaffSchema.Constants.GS_WorkExtension);
					AssertHistory(histories[8], "0294811111", "Work phone", "Work phone", GlbStaffSchema.Constants.GS_WorkPhone);
					AssertHistory(histories[9], "0412345678", "Mobile phone", "Mobile phone", GlbStaffSchema.Constants.GS_MobilePhone);
					AssertHistory(histories[10], "0294811110", "Fax num", "Fax num", GlbStaffSchema.Constants.GS_FaxNum);
					AssertHistory(histories[11], "0294811111", "Home phone", "Home phone", GlbStaffSchema.Constants.GS_HomePhone);
					AssertHistory(histories[12], "123456", "Pager", "Pager", GlbStaffSchema.Constants.GS_Pager);
					AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "Address1", GlbStaffSchema.Constants.GS_UserAddress1);
					AssertHistory(histories[14], "Gorgoroth", "City", "City", GlbStaffSchema.Constants.GS_City);
					AssertHistory(histories[15], "Mordor", "State", "State", GlbStaffSchema.Constants.GS_State);
					AssertHistory(histories[16], "1111", "Postcode", "Postcode", GlbStaffSchema.Constants.GS_Postcode);
				}
			}

			#endregion

			#region Group membership
			public abstract void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldReplicateGroupMembershipButLeaveUntrackedUsersAlone();

			#endregion

			#region New Users/Groups

			public abstract void TestNewStaffInEnterprise();

			public void TestNewStaffInEnterprise_DoesNotCreateADUserIfInvalidOU()
			{
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(userOrganisationalUnit: TestConstants.InvalidOU, groupOrganisationalUnit: TestConstants.InvalidOU));

				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_LoginName = "AD.Test.User.1";
				staff1.GS_FullName = "AD Test User 1";
				staff1.GS_Code = "AT1";
				staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;
				staff1.GS_IsOperational = false;

				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_LoginName = "AD.Test.User.2";
				staff2.GS_FullName = "AD Test User 2";
				staff2.GS_Code = "AT2";
				staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				staff2.GS_IsOperational = false;

				Factory.Save();
				var searcher = new DirectorySearcherWrapper(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, TestConstants.Domain);
				var syncer = new EntitySynchroniser(Factory);
				DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher;

				AssertExceptionThrown<InvalidOUException>(() => syncer.Synchronise());
				syncer.Save();

				AssertEquals(ZGuid.Invalid, staff2.GS_ActiveDirectoryObjectGuid);
			}

			public abstract void TestNewGroupsInEnterprise();

			#endregion
		}

		class UseLatestRecordTest_ADIsMaster : UseLatestRecordTest
		{
			protected override SyncMode SyncMode => SyncMode.ADIsMaster;

			protected override int TestSave_CommitChangesOnSyncedEntities_ExpectedCommit => 0;

			protected override void AssertOptOutSavingPersonalDataToAD(GlbStaff staff) => AssertOptOutSavingPersonalDataToAD_FromAD(staff);

			public override void TestSync_ActiveLinkedStaff_ShouldSync()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
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
				Factory.Save();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", staff.GS_LoginName);

				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();

				AssertHistory(histories[0], "lord.sauron", "sauron", "lord.sauron", GlbStaffSchema.Constants.GS_LoginName);
				AssertHistory(histories[1], TestConstants.Domain, string.Empty, TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
				AssertHistory(histories[4], "Lord", "Title", "Lord", GlbStaffSchema.Constants.GS_Title);
				AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Lord Sauron", GlbStaffSchema.Constants.GS_FullName);
				AssertHistory(histories[6], "sauron@mordor.com", "Email", "sauron@mordor.com", GlbStaffSchema.Constants.GS_EmailAddress);
				AssertHistory(histories[7], "123", "Ext", "123", GlbStaffSchema.Constants.GS_WorkExtension);
				AssertHistory(histories[8], "0294811111", "Work phone", "0294811111", GlbStaffSchema.Constants.GS_WorkPhone);
				AssertHistory(histories[9], "0412345678", "Mobile phone", "0412345678", GlbStaffSchema.Constants.GS_MobilePhone);
				AssertHistory(histories[10], "0294811110", "Fax num", "0294811110", GlbStaffSchema.Constants.GS_FaxNum);
				AssertHistory(histories[11], "0294811111", "Home phone", "0294811111", GlbStaffSchema.Constants.GS_HomePhone);
				AssertHistory(histories[12], "123456", "Pager", "123456", GlbStaffSchema.Constants.GS_Pager);
				AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "1 Barad Dur Way", GlbStaffSchema.Constants.GS_UserAddress1);
				AssertHistory(histories[14], "Gorgoroth", "City", "Gorgoroth", GlbStaffSchema.Constants.GS_City);
				AssertHistory(histories[15], "Mordor", "State", "Mordor", GlbStaffSchema.Constants.GS_State);
				AssertHistory(histories[16], "1111", "Postcode", "1111", GlbStaffSchema.Constants.GS_Postcode);
			}

			public override void TestSync_ActiveLinkedStaff_LastEditInEnterprise_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //Should sync from AD to CW1 even if CW1 is latest
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", staff.GS_LoginName);
				AssertEquals("lord.sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
			}

			public override void TestSync_ActiveLinkedGroup_ShouldSync()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var valar = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				var manwë = DummyDirectoryEntryWrapper.CreateUser("Manwë");
				valar.AddMember(manwë);

				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				group.GG_ActiveDirectoryObjectGuid = valar.Guid;
				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				group.GG_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var staff = group.Staff.AddNew();
				staff.GS_LoginName = "Manwë";
				staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				directorySearcherMock.Setup(s => s.FindUser("Manwë", TestConstants.ValidOU)).Returns(manwë);
				directorySearcherMock.Setup(s => s.FindGroup(valar.Guid, TestConstants.ValidOU)).Returns(valar);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);

				AssertEquals(2, syncResults.Count);
				var histories = syncResults[1].ToArray();
				AssertEquals(5, histories.Length);
				AssertHistory(histories[0], "Valar", ".-Valar-.", "Valar", GlbGroupSchema.Constants.GG_Desc);
				AssertHistory(histories[1], TestConstants.Domain, TestConstants.Domain, TestConstants.Domain, GlbGroupSchema.Constants.GG_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbGroup.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbGroupSchema.Constants.GG_IsActive);
				AssertHistory(histories[4], "Manwë", "Manwë", "Manwë", null);
			}

			public override void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime()); //Should sync from AD to CW1 even if CW1 is latest
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				group.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("Valar", group.GG_Desc);
				AssertEquals("Valar", directoryEntry.GetValue(GlbGroupSchema.GG_Desc));
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			public override void TestSync_ActiveLinkedGroup_ShouldReplicateGroupMembership()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");
				group.GG_ActiveDirectoryObjectGuid = groupEntry.Guid;

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("robert.delnaja");
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("pj.harvey");
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;

				Factory.Save();

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");

				directorySearcherMock.Setup(s => s.FindGroup(groupEntry.Guid, TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);

				syncer.Synchronise();

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "pj.harvey");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");
			}

			public override void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldReplicateGroupMembershipButLeaveUntrackedUsersAlone()
			{
				var anotherDomainCredentials = ADTestHelper.CreateDomainCredentials("domain2", isDefaultDomain: false);
				var domainCredentialsCollection = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value;
				domainCredentialsCollection.Add(anotherDomainCredentials);
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domainCredentialsCollection);

				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				group.GG_DomainName = TestConstants.Domain;
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");
				groupEntry.SetLastModified(ZDateTime.UtcNow.ToDateTime().AddMinutes(-1));
				group.GG_ActiveDirectoryObjectGuid = groupEntry.Guid;

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				staff1.GS_DomainName = TestConstants.Domain;
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				staff2.GS_DomainName = TestConstants.Domain;
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("robert.delnaja");
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				staff3.GS_DomainName = TestConstants.Domain;
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("pj.harvey");
				userEntry3.SetLastModified(ZDateTime.UtcNow.ToDateTime());
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;

				var staff4 = group.Staff.AddNew();
				staff4.GS_LoginName = "tom.who";
				staff4.GS_DomainName = "domain2";
				var userEntry4 = DummyDirectoryEntryWrapper.CreateUser("tom.who");
				userEntry4.SetLastModified(ZDateTime.UtcNow.ToDateTime());
				staff4.GS_ActiveDirectoryObjectGuid = userEntry4.Guid;

				Factory.Save();

				var untrackedUser1 = DummyDirectoryEntryWrapper.CreateUser("johnny.greenwood");
				var untrackedUser2 = DummyDirectoryEntryWrapper.CreateUser("colin.greenwood");
				var untrackedUser3 = DummyDirectoryEntryWrapper.CreateUser("philip.selway");
				var untrackedUser4 = DummyDirectoryEntryWrapper.CreateUser("ed.obrien");

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);
				groupEntry.AddMember(untrackedUser1);
				groupEntry.AddMember(untrackedUser2);
				groupEntry.AddMember(untrackedUser3);
				groupEntry.AddMember(untrackedUser4);

				AssertEquals(3, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "tom.who");

				var members = groupEntry.GetMembers().ToArray();
				AssertEquals(6, members.Length);
				// tracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");
				// untracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "johnny.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "colin.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "philip.selway");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "ed.obrien");

				directorySearcherMock.Setup(s => s.FindGroup(groupEntry.Guid, TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);
				directorySearcherMock.Setup(s => s.FindUser(userEntry4.Guid, TestConstants.ValidOU)).Returns(userEntry4);

				syncer.Synchronise();

				AssertEquals(3, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "pj.harvey");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "tom.who");

				members = groupEntry.GetMembers().ToArray();
				AssertEquals(6, members.Length);
				// tracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");
				// untracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "johnny.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "colin.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "philip.selway");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "ed.obrien");
			}

			public override void TestSync_ActiveUnLinkedStaff_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Factory.Save();
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", staff.GS_LoginName);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
				AssertEquals("Lord Sauron", staff.GS_FullName);
				AssertEquals("sauron@mordor.com", staff.GS_EmailAddress);
				AssertEquals("0412345678", staff.GS_MobilePhone);
				AssertEquals("Lord", staff.GS_Title);
				AssertEquals("1 Barad Dur Way", staff.GS_UserAddress1);
				AssertEquals("Gorgoroth", staff.GS_City);
				AssertEquals("Mordor", staff.GS_State);
				AssertEquals("1111", staff.GS_Postcode);
				AssertEquals("0294811111", staff.GS_WorkPhone);
				AssertEquals("0294811110", staff.GS_FaxNum);
				AssertEquals("0294811111", staff.GS_HomePhone);
				AssertEquals("123456", staff.GS_Pager);
			}

			public override void TestSync_ActiveUnLinkedStaff_LastModifiedInEnterprise_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_Title = "The Lord of the Rings";
				Factory.Save();
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());

				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				var adUser = new ADUser(staff);

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", staff.GS_LoginName);
				AssertEquals("lord.sauron", adUser.LoginName);
				AssertEquals("Lord", adUser.Title);
				AssertEquals("Lord", staff.GS_Title);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			// Should disable staff
			public override void TestSync_ActiveUnLinkedStaff_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_FullName = "Load of the Fly";
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid; // so it triggers new AD user to be created

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var organisationalUnit = new Mock<IOrganisationalUnit>();

				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);

				syncer.Synchronise();

				AssertEquals("One-way sync with AD Master, Staff should be deactived when no match found", false, staff.GS_IsActive);
				AssertEquals(ZGuid.Empty, staff.GS_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			public override void TestSync_ActiveUnLinkedStaff_WhenMatchFoundOutsideOU()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", string.Empty)).Returns(directoryEntry);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals("Should be deactived", false, staff.GS_IsActive);
				AssertEquals("Should be unlinked", false, staff.IsADLinked);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			// Should disable group
			public override void TestSync_ActiveUnLinkedGroup_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Desc = "JohnLock";
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				var staff = group.Staff.AddNew();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var organisationalUnit = new Mock<IOrganisationalUnit>();
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup(group.GG_Desc);
				var userEntry = DummyDirectoryEntryWrapper.CreateGroup(staff.GS_LoginName);

				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, string.Empty)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);

				syncer.Synchronise();

				AssertEquals("Group should be disabled when no match found", false, group.GG_IsActive);
				AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
				AssertEquals(group.GG_Desc, groupEntry.GetValue(GlbGroupSchema.GG_Desc));
				AssertEquals("Group member shouldn't change", 1, group.Staff.Count);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			// Should disable staff
			public override void TestNewStaffInEnterprise()
			{
				var staffWithMatchingUser = Factory.NewWithValidTestData<GlbStaff>();
				staffWithMatchingUser.GS_LoginName = "Benedict.Cumberbatch";
				staffWithMatchingUser.GS_DomainName = TestConstants.Domain;
				staffWithMatchingUser.GS_FullName = "Benedict Cumberbatch";
				staffWithMatchingUser.GS_Code = "BEN";
				staffWithMatchingUser.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				staffWithMatchingUser.GS_IsOperational = false;

				var staffWithNoMatchingUser = Factory.NewWithValidTestData<GlbStaff>();
				staffWithNoMatchingUser.GS_LoginName = "Martin.Freeman";
				staffWithNoMatchingUser.GS_DomainName = "";
				staffWithNoMatchingUser.GS_FullName = "Martin Freeman";
				staffWithNoMatchingUser.GS_Code = "MFM";
				staffWithNoMatchingUser.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				staffWithNoMatchingUser.GS_IsOperational = false;

				Factory.Save();

				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("Benedict.Cumberbatch", fullName: "Benny");

				directorySearcherMock.Setup(s => s.FindUser("Benedict.Cumberbatch", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser("Martin.Freeman", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals(true, staffWithMatchingUser.GS_IsActive);
				AssertEquals(directoryEntry.Guid, staffWithMatchingUser.GS_ActiveDirectoryObjectGuid);
				AssertEquals("Benny", staffWithMatchingUser.GS_FullName);
				AssertEquals("Benny", directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName)]);
				AssertEquals(TestConstants.Domain, staffWithMatchingUser.GS_DomainName);

				AssertEquals(false, staffWithNoMatchingUser.GS_IsActive);
				AssertEquals(ZGuid.Empty, staffWithNoMatchingUser.GS_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, staffWithNoMatchingUser.GS_DomainName);

				AssertEquals(0, directoryEntry.CommitCount);

				syncer.Save();

				AssertEquals(0, directoryEntry.CommitCount);
			}

			// Should disable group
			public override void TestNewGroupsInEnterprise()
			{
				var group1 = Factory.NewWithValidTestData<GlbGroup>();
				group1.GG_Desc = "JohnLock";
				group1.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

				var group2 = Factory.NewWithValidTestData<GlbGroup>();
				group2.GG_Desc = "FreeBatch";
				group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				Factory.Save();

				var syncer = GetSyncer();

				directorySearcherMock.Setup(s => s.FindGroup("JohnLock", TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindGroup("FreeBatch", TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);

				syncer.Synchronise();

				AssertEquals(false, group1.GG_IsActive);
				AssertEquals(ZGuid.Empty, group1.GG_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, group1.GG_DomainName);

				AssertEquals(false, group2.GG_IsActive);
				AssertEquals(ZGuid.Empty, group2.GG_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, group2.GG_DomainName);
			}

			protected override void AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbStaff[] staff, DummyDirectoryEntryWrapper[] directoryEntry)
			{
				// Staff1: No sync - staff1 and directoryEntry1 were both modified before LastSyncUtc
				AssertEquals("staff1 should not be synced", "cw.staff1", staff[0].GS_LoginName);
				AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry[0].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff2: AD -> CW1 - directoryEntry2 was modified after LastSyncUtc and staff2 was modified before
				AssertEquals("staff2 should be synced", "ad.user2", staff[1].GS_LoginName);
				AssertEquals("directoryEntry2 should not be changed", "ad.user2", directoryEntry[1].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff3: AD -> CW - staff3 was modified after LastSyncUtc so it get picked up to sync, but this is ADIsMater+1Way, so CW1 will be overridden by AD
				AssertEquals("staff3 should not be changed", "ad.user3", staff[2].GS_LoginName);
				AssertEquals("directoryEntry3 should be synced", "ad.user3", directoryEntry[2].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff4: AD -> CW1 - when both modified after LastSyncUtc, use AD because ADIsMater+1Way
				AssertEquals("staff4 should not changed", "ad.user4", staff[3].GS_LoginName);
				AssertEquals("directoryEntry4 should be synced", "ad.user4", directoryEntry[3].GetValue(GlbStaffSchema.GS_LoginName));
			}

			protected override void AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbGroup[] group, DummyDirectoryEntryWrapper[] groupDirectoryEntry)
			{
				// Group1: No sync - group1 and groupDirectoryEntry1 were both modified before LastSyncUtc
				AssertEquals("group1 should not be synced", "cw.group1", group[0].GG_Desc);
				AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry[0].GetValue(GlbGroupSchema.GG_Desc));

				// Group2: AD -> CW1 - groupDirectoryEntry2 was modified after LastSyncUtc and group2 was modified before
				AssertEquals("group2 should be synced", "ad.group2", group[1].GG_Desc);
				AssertEquals("groupDirectoryEntry2 should not be changed", "ad.group2", groupDirectoryEntry[1].GetValue(GlbGroupSchema.GG_Desc));

				// Group3: AD -> CW - group3 was modified after LastSyncUtc so it get picked up to sync, but this is ADIsMater+1Way, so CW1 will be overridden by AD
				AssertEquals("group3 should not be changed", "ad.group3", group[2].GG_Desc);
				AssertEquals("groupDirectoryEntry3 should be synced", "ad.group3", groupDirectoryEntry[2].GetValue(GlbGroupSchema.GG_Desc));

				// Group4: AD -> CW1 - when both modified after LastSyncUtc, use AD because ADIsMater+1Way
				AssertEquals("group4 should not changed", "ad.group4", group[3].GG_Desc);
				AssertEquals("groupDirectoryEntry4 should be synced", "ad.group4", groupDirectoryEntry[3].GetValue(GlbGroupSchema.GG_Desc));
			}
		}

		class UseLatestRecordTest_EnterpriseIsMaster : UseLatestRecordTest
		{
			protected override SyncMode SyncMode => SyncMode.EnterpriseIsMaster;

			protected override void AssertOptOutSavingPersonalDataToAD(GlbStaff staff) => AssertOptOutSavingPersonalDataToAD_FromCW(staff);

			public override void TestSync_ActiveLinkedStaff_ShouldSync()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
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
				Factory.Save();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("sauron", staff.GS_LoginName);

				AssertEquals(1, syncResults.Count);
				var histories = syncResults[0].ToArray();

				AssertHistory(histories[0], "lord.sauron", "sauron", "sauron", GlbStaffSchema.Constants.GS_LoginName);
				AssertHistory(histories[1], TestConstants.Domain, string.Empty, TestConstants.Domain, GlbStaffSchema.Constants.GS_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbStaff.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbStaffSchema.Constants.GS_IsActive);
				AssertHistory(histories[4], "Lord", "Title", "Title", GlbStaffSchema.Constants.GS_Title);
				AssertHistory(histories[5], "Lord Sauron", "Sauron the Great", "Sauron the Great", GlbStaffSchema.Constants.GS_FullName);
				AssertHistory(histories[6], "sauron@mordor.com", "Email", "Email", GlbStaffSchema.Constants.GS_EmailAddress);
				AssertHistory(histories[7], "123", "Ext", "Ext", GlbStaffSchema.Constants.GS_WorkExtension);
				AssertHistory(histories[8], "0294811111", "Work phone", "Work phone", GlbStaffSchema.Constants.GS_WorkPhone);
				AssertHistory(histories[9], "0412345678", "Mobile phone", "Mobile phone", GlbStaffSchema.Constants.GS_MobilePhone);
				AssertHistory(histories[10], "0294811110", "Fax num", "Fax num", GlbStaffSchema.Constants.GS_FaxNum);
				AssertHistory(histories[11], "0294811111", "Home phone", "Home phone", GlbStaffSchema.Constants.GS_HomePhone);
				AssertHistory(histories[12], "123456", "Pager", "Pager", GlbStaffSchema.Constants.GS_Pager);
				AssertHistory(histories[13], "1 Barad Dur Way", "Address1", "Address1", GlbStaffSchema.Constants.GS_UserAddress1);
				AssertHistory(histories[14], "Gorgoroth", "City", "City", GlbStaffSchema.Constants.GS_City);
				AssertHistory(histories[15], "Mordor", "State", "State", GlbStaffSchema.Constants.GS_State);
				AssertHistory(histories[16], "1111", "Postcode", "Postcode", GlbStaffSchema.Constants.GS_Postcode);
			}

			public override void TestSync_ActiveLinkedStaff_LastEditInEnterprise_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime()); //Should sync from CW1 to AD even if AD is latest
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "sauron";
				staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("sauron", staff.GS_LoginName);
				AssertEquals("sauron", directoryEntry.GetValue(GlbStaffSchema.GS_LoginName));
			}

			public override void TestSync_ActiveLinkedGroup_ShouldSync()
			{
				var syncer = GetSyncer();
				var syncResults = new List<IEnumerable<ISyncEvent>>();
				syncer.EntitySynchronised += (s, e) => syncResults.Add(e.SyncEvents);

				var valar = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				var manwë = DummyDirectoryEntryWrapper.CreateUser("Manwë");
				valar.AddMember(manwë);

				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				group.GG_ActiveDirectoryObjectGuid = valar.Guid;
				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				group.GG_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var staff = group.Staff.AddNew();
				staff.GS_LoginName = "Manwë";
				staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				directorySearcherMock.Setup(s => s.FindUser("Manwë", TestConstants.ValidOU)).Returns(manwë);
				directorySearcherMock.Setup(s => s.FindGroup(valar.Guid, TestConstants.ValidOU)).Returns(valar);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(".-Valar-.", group.GG_Desc);
				AssertEquals(".-Valar-.", valar.GetValue(GlbGroupSchema.GG_Desc));

				AssertEquals(2, syncResults.Count);
				var histories = syncResults[1].ToArray();
				AssertEquals(5, histories.Length);
				AssertHistory(histories[0], "Valar", ".-Valar-.", ".-Valar-.", GlbGroupSchema.Constants.GG_Desc);
				AssertHistory(histories[1], TestConstants.Domain, TestConstants.Domain, TestConstants.Domain, GlbGroupSchema.Constants.GG_DomainName);
				AssertHistory(histories[2], ZBool.True, ZBool.True, ZBool.True, GlbGroup.Schema.IsADLinked);
				AssertHistory(histories[3], ZBool.True, ZBool.True, ZBool.True, GlbGroupSchema.Constants.GG_IsActive);
				AssertHistory(histories[4], "Manwë", "Manwë", "Manwë", null);
			}

			public override void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldSync()
			{
				var syncer = GetSyncer();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("Valar");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime()); //Should sync from CW1 to AD even if AD is latest
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = ".-Valar-.";
				group.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;

				Factory.Save();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);

				directorySearcherMock.Setup(s => s.FindGroup(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				Assert(group.GG_IsActive);
				Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals(".-Valar-.", group.GG_Desc);
				AssertEquals(".-Valar-.", directoryEntry.GetValue(GlbGroupSchema.GG_Desc));
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			public override void TestSync_ActiveLinkedGroup_ShouldReplicateGroupMembership()
			{
				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");
				group.GG_ActiveDirectoryObjectGuid = groupEntry.Guid;

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("robert.delnaja");
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("pj.harvey");
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;
				Factory.Save();

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");

				directorySearcherMock.Setup(s => s.FindGroup(groupEntry.Guid, TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);

				syncer.Synchronise();

				AssertEquals(2, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");

				AssertEquals(2, groupEntry.GetMembers().Count());
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(groupEntry.GetMembers(), g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "robert.delnaja");
			}

			public override void TestSync_ActiveLinkedGroup_LastEditInEnterprise_ShouldReplicateGroupMembershipButLeaveUntrackedUsersAlone()
			{
				var anotherDomainCredentials = ADTestHelper.CreateDomainCredentials("domain2", isDefaultDomain: false);
				var domainCredentialsCollection = ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value;
				domainCredentialsCollection.Add(anotherDomainCredentials);
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, domainCredentialsCollection);

				var syncer = GetSyncer();
				var group = Factory.New<GlbGroup>();
				group.GG_Desc = "singers";
				group.GG_DomainName = TestConstants.Domain;
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup("singers");
				groupEntry.SetLastModified(ZDateTime.UtcNow.ToDateTime().AddMinutes(-1));
				group.GG_ActiveDirectoryObjectGuid = groupEntry.Guid;

				var staff1 = group.Staff.AddNew();
				staff1.GS_LoginName = "thom.yorke";
				staff1.GS_DomainName = TestConstants.Domain;
				var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("thom.yorke");
				staff1.GS_ActiveDirectoryObjectGuid = userEntry1.Guid;

				var staff2 = group.Staff.AddNew();
				staff2.GS_LoginName = "robert.delnaja";
				staff2.GS_DomainName = TestConstants.Domain;
				var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("robert.delnaja");
				staff2.GS_ActiveDirectoryObjectGuid = userEntry2.Guid;

				var staff3 = Factory.New<GlbStaff>();
				staff3.GS_LoginName = "pj.harvey";
				staff3.GS_DomainName = TestConstants.Domain;
				var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("pj.harvey");
				userEntry3.SetLastModified(ZDateTime.UtcNow.ToDateTime());
				staff3.GS_ActiveDirectoryObjectGuid = userEntry3.Guid;

				var staff4 = group.Staff.AddNew();
				staff4.GS_LoginName = "tom.who";
				staff4.GS_DomainName = "domain2";
				var userEntry4 = DummyDirectoryEntryWrapper.CreateUser("tom.who");
				userEntry4.SetLastModified(ZDateTime.UtcNow.ToDateTime());
				staff4.GS_ActiveDirectoryObjectGuid = userEntry4.Guid;

				Factory.Save();

				var untrackedUser1 = DummyDirectoryEntryWrapper.CreateUser("johnny.greenwood");
				var untrackedUser2 = DummyDirectoryEntryWrapper.CreateUser("colin.greenwood");
				var untrackedUser3 = DummyDirectoryEntryWrapper.CreateUser("philip.selway");
				var untrackedUser4 = DummyDirectoryEntryWrapper.CreateUser("ed.obrien");

				groupEntry.AddMember(userEntry1);
				groupEntry.AddMember(userEntry3);
				groupEntry.AddMember(untrackedUser1);
				groupEntry.AddMember(untrackedUser2);
				groupEntry.AddMember(untrackedUser3);
				groupEntry.AddMember(untrackedUser4);

				AssertEquals(3, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "tom.who");

				var members = groupEntry.GetMembers().ToArray();
				AssertEquals(6, members.Length);
				// tracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "pj.harvey");
				// untracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "johnny.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "colin.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "philip.selway");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "ed.obrien");

				directorySearcherMock.Setup(s => s.FindGroup(groupEntry.Guid, TestConstants.ValidOU)).Returns(groupEntry);
				directorySearcherMock.Setup(s => s.FindUser(userEntry1.Guid, TestConstants.ValidOU)).Returns(userEntry1);
				directorySearcherMock.Setup(s => s.FindUser(userEntry2.Guid, TestConstants.ValidOU)).Returns(userEntry2);
				directorySearcherMock.Setup(s => s.FindUser(userEntry3.Guid, TestConstants.ValidOU)).Returns(userEntry3);
				directorySearcherMock.Setup(s => s.FindUser(userEntry4.Guid, TestConstants.ValidOU)).Returns(userEntry4);

				syncer.Synchronise();

				AssertEquals(3, group.Staff.Count);
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "thom.yorke");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "robert.delnaja");
				AssertCollectionContains(group.Staff.Cast<GlbStaff>(), s => s.GS_LoginName == "tom.who");

				members = groupEntry.GetMembers().ToArray();
				AssertEquals(6, members.Length);
				// tracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "thom.yorke");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "robert.delnaja");
				// untracked users
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "johnny.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "colin.greenwood");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "philip.selway");
				AssertCollectionContains(members, g => (string)g.GetValue(GlbStaffSchema.GS_LoginName) == "ed.obrien");
			}

			public override void TestSync_ActiveUnLinkedStaff_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_FullName = "Lord Sauron";
				staff.GS_EmailAddress = "sauron@mordor.com";
				staff.GS_MobilePhone = "0412345678";
				staff.GS_Title = "Lord";
				staff.GS_UserAddress1 = "1 Barad Dur Way";
				staff.GS_City = "Gorgoroth";
				staff.GS_State = "Mordor";
				staff.GS_Postcode = "1111";
				staff.GS_WorkPhone = "0294811111";
				staff.GS_FaxNum = "0294811110";
				staff.GS_HomePhone = "0294811111";
				staff.GS_Pager = "123456";

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = new DummyDirectoryEntryWrapper("lord.sauron", ZDateTime.UtcNow.ToDateTime(), "lord.sauron", "", "", "", 1, "", "", "", "", "", "", "", "", "");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(1).ToDateTime());

				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				var adUser = new ADUser(staff);
				Assert(adUser.IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", adUser.LoginName);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
				AssertEquals("Lord Sauron", adUser.FullName);
				AssertEquals("sauron@mordor.com", adUser.EmailAddress);
				AssertEquals("0412345678", adUser.MobilePhone);
				AssertEquals("Lord", adUser.Title);
				AssertEquals("1 Barad Dur Way", adUser.StreetAddress);
				AssertEquals("Gorgoroth", adUser.City);
				AssertEquals("Mordor", adUser.State);
				AssertEquals("1111", adUser.Postcode);
				AssertEquals("0294811111", adUser.WorkPhone);
				AssertEquals("0294811110", adUser.FaxNum);
				AssertEquals("0294811111", adUser.HomePhone);
				AssertEquals("123456", adUser.Pager);
			}

			public override void TestSync_ActiveUnLinkedStaff_LastModifiedInEnterprise_ShouldActivateAndSyncWhenMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_Title = "The Lord of the Rings";
				Factory.Save();
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());

				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns(directoryEntry);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

				syncer.Synchronise();

				var adUser = new ADUser(staff);

				Assert(staff.GS_IsActive);
				Assert(staff.GS_ActiveDirectoryObjectGuid.IsValid);
				AssertEquals("lord.sauron", adUser.LoginName);
				AssertEquals("The Lord of the Rings", adUser.Title);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			public override void TestSync_ActiveUnLinkedStaff_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				staff.GS_FullName = "Load of the Fly";
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid; // so it triggers new AD user to be created

				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var organisationalUnit = new Mock<IOrganisationalUnit>();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser(staff.GS_LoginName);
				directoryEntry.PasswordNotRequired = true;

				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, string.Empty)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);
				organisationalUnit.Setup(x => x.CreateNewChild(staff.GS_LoginName, staff.GS_LoginName, staff.GS_LoginName, DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals("Staff should remain active when no match found", true, staff.GS_IsActive);
				AssertEquals(directoryEntry.Guid, staff.GS_ActiveDirectoryObjectGuid);
				AssertEquals("Load of the Fly", directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName)]);
				AssertEquals(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials.DefaultPassword, directoryEntry.Password);
				AssertEquals(true, directoryEntry.PasswordMustChangeAtNextLogon);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
				AssertEquals(false, directoryEntry.PasswordNotRequired);
			}

			public override void TestSync_ActiveUnLinkedStaff_WhenMatchFoundOutsideOU()
			{
				var syncer = GetSyncer();
				var staff = Factory.New<GlbStaff>();
				staff.GS_LoginName = "lord.sauron";
				Assert(staff.GS_IsActive);
				Assert(!staff.GS_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("lord.sauron", string.Empty)).Returns(directoryEntry);

				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, string.Empty)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals("Should stay active", true, staff.GS_IsActive);
				AssertEquals("Should be unlinked", false, staff.IsADLinked);
				AssertEquals(TestConstants.Domain, staff.GS_DomainName);
			}

			public override void TestSync_ActiveUnLinkedGroup_WhenNoMatchFound()
			{
				var syncer = GetSyncer();
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.GG_Desc = "JohnLock";
				group.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				var staff = group.Staff.AddNew();
				staff.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;

				Assert(group.GG_IsActive);
				Assert(!group.GG_ActiveDirectoryObjectGuid.IsValid);

				Factory.Save();

				var organisationalUnit = new Mock<IOrganisationalUnit>();
				var groupEntry = DummyDirectoryEntryWrapper.CreateGroup(group.GG_Desc);
				var userEntry = DummyDirectoryEntryWrapper.CreateGroup(staff.GS_LoginName);

				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, string.Empty)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindGroup(group.GG_Desc, TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);
				organisationalUnit.Setup(x => x.CreateNewChild(group.GG_Desc, DirectoryObjectType.Group, directorySearcherMock.Object)).Returns(groupEntry);
				organisationalUnit.Setup(x => x.CreateNewChild(staff.GS_LoginName, staff.GS_LoginName, staff.GS_LoginName, DirectoryObjectType.User, directorySearcherMock.Object)).Returns(userEntry);

				syncer.Synchronise();

				AssertEquals("Staff should remain active when no match found", true, group.GG_IsActive);
				AssertEquals(groupEntry.Guid, group.GG_ActiveDirectoryObjectGuid);
				AssertEquals(group.GG_Desc, groupEntry.GetValue(GlbGroupSchema.GG_Desc));
				AssertEquals("Group member shouldn't change", 1, group.Staff.Count);
				AssertEquals(TestConstants.Domain, group.GG_DomainName);
			}

			// Should create AD User
			public override void TestNewStaffInEnterprise()
			{
				var staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_LoginName = "Benedict.Cumberbatch";
				staff1.GS_DomainName = TestConstants.Domain;
				staff1.GS_FullName = "Benedict Cumberbatch";
				staff1.GS_Code = "BEN";
				staff1.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				staff1.GS_IsOperational = false;

				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_LoginName = "Martin.Freeman";
				staff2.GS_DomainName = "";
				staff2.GS_FullName = "Martin Freeman";
				staff2.GS_Code = "MFM";
				staff2.GS_ActiveDirectoryObjectGuid = ZGuid.Invalid;
				staff2.GS_IsOperational = false;

				var staff3 = Factory.NewWithValidTestData<GlbStaff>();
				staff3.GS_LoginName = "Mark.Gatiss";
				staff3.GS_FullName = "Mark Gatiss";
				staff3.GS_Code = "MKG";
				staff3.GS_ActiveDirectoryObjectGuid = ZGuid.Empty;

				Factory.Save();

				var syncer = GetSyncer();
				var organisationalUnit = new Mock<IOrganisationalUnit>();
				var directoryEntry1 = DummyDirectoryEntryWrapper.CreateUser("Benedict.Cumberbatch");
				var directoryEntry2 = DummyDirectoryEntryWrapper.CreateUser("Martin.Freeman");

				directoryEntry1.HasChanges = true;
				directoryEntry2.HasChanges = true;

				directorySearcherMock.Setup(s => s.FindUser("Benedict.Cumberbatch", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("Martin.Freeman", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindUser("Mark.Gatiss", TestConstants.ValidOU)).Returns((IUserDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);

				organisationalUnit.Setup(ou => ou.CreateNewChild("Benedict.Cumberbatch", "Benedict.Cumberbatch", "Benedict.Cumberbatch", DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry1);
				organisationalUnit.Setup(ou => ou.CreateNewChild("Martin.Freeman", "Martin.Freeman", "Martin.Freeman", DirectoryObjectType.User, directorySearcherMock.Object)).Returns(directoryEntry2);

				syncer.Synchronise();

				AssertEquals(true, staff1.GS_IsActive);
				AssertEquals(directoryEntry1.Guid, staff1.GS_ActiveDirectoryObjectGuid);
				AssertEquals("Benedict Cumberbatch", directoryEntry1[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName)]);
				AssertEquals(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials.DefaultPassword, directoryEntry1.Password);
				AssertEquals(TestConstants.Domain, staff1.GS_DomainName);

				AssertEquals(true, staff2.GS_IsActive);
				AssertEquals(directoryEntry2.Guid, staff2.GS_ActiveDirectoryObjectGuid);
				AssertEquals("Martin Freeman", directoryEntry2[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(GlbStaffSchema.GS_FullName)]);
				AssertEquals(ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value.DefaultDomainCredentials.DefaultPassword, directoryEntry2.Password);
				AssertEquals(TestConstants.Domain, staff2.GS_DomainName);

				AssertEquals(true, staff3.GS_IsActive);
				AssertEquals(ZGuid.Invalid, staff3.GS_ActiveDirectoryObjectGuid);

				AssertEquals(2, directoryEntry1.CommitCount);
				AssertEquals(2, directoryEntry2.CommitCount);

				syncer.Save();

				AssertEquals(3, directoryEntry1.CommitCount);
				AssertEquals(3, directoryEntry2.CommitCount);
			}

			// Should create AD Group
			public override void TestNewGroupsInEnterprise()
			{
				var group1 = Factory.NewWithValidTestData<GlbGroup>();
				group1.GG_Desc = "JohnLock";
				group1.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;

				var group2 = Factory.NewWithValidTestData<GlbGroup>();
				group2.GG_Desc = "FreeBatch";
				group2.GG_ActiveDirectoryObjectGuid = ZGuid.Empty;
				Factory.Save();

				var syncer = GetSyncer();
				var organisationalUnit = new Mock<IOrganisationalUnit>();
				var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup("JohnLock");

				directorySearcherMock.Setup(s => s.FindGroup("JohnLock", TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindGroup("FreeBatch", TestConstants.ValidOU)).Returns((IGroupDirectoryEntry)null);
				directorySearcherMock.Setup(s => s.FindOrganisationalUnit(TestConstants.ValidOU)).Returns(organisationalUnit.Object);
				organisationalUnit.Setup(ou => ou.CreateNewChild("JohnLock", DirectoryObjectType.Group, directorySearcherMock.Object)).Returns(directoryEntry);

				syncer.Synchronise();

				AssertEquals(true, group1.GG_IsActive);
				AssertEquals(directoryEntry.Guid, group1.GG_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, group1.GG_DomainName);

				AssertEquals(true, group2.GG_IsActive);
				AssertEquals(ZGuid.Invalid, group2.GG_ActiveDirectoryObjectGuid);
				AssertEquals(TestConstants.Domain, group2.GG_DomainName);
			}

			protected override void AssertStaffOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbStaff[] staff, DummyDirectoryEntryWrapper[] directoryEntry)
			{
				// Staff1: No sync - staff1 and directoryEntry1 were both modified before LastSyncUtc
				AssertEquals("staff1 should not be synced", "cw.staff1", staff[0].GS_LoginName);
				AssertEquals("directoryEntry1 should not be synced", "ad.user1", directoryEntry[0].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff2: No sync - directoryEntry2 was modified after LastSyncUtc but it is EnterpriseIsMaster+1Way and staff2 was modified before so it wont be picked to sync
				AssertEquals("staff2 should be synced", "cw.staff2", staff[1].GS_LoginName);
				AssertEquals("directoryEntry2 should not be changed", "ad.user2", directoryEntry[1].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff3: CW1 -> AD - staff3 was modified after LastSyncUtc and directoryEntry3 was modified before
				AssertEquals("staff3 should not be changed", "cw.staff3", staff[2].GS_LoginName);
				AssertEquals("directoryEntry3 should be synced", "cw.staff3", directoryEntry[2].GetValue(GlbStaffSchema.GS_LoginName));

				// Staff4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
				AssertEquals("staff4 should not changed", "cw.staff4", staff[3].GS_LoginName);
				AssertEquals("directoryEntry4 should be synced", "cw.staff4", directoryEntry[3].GetValue(GlbStaffSchema.GS_LoginName));
			}

			protected override void AssertGroupOfTestOnlySyncNewChangesAfterLastSuccessfulSyncUTC(GlbGroup[] group, DummyDirectoryEntryWrapper[] groupDirectoryEntry)
			{
				// Group1: No sync - group1 and groupDirectoryEntry1 were both modified before LastSyncUtc
				AssertEquals("group1 should not be synced", "cw.group1", group[0].GG_Desc);
				AssertEquals("groupDirectoryEntry1 should not be synced", "ad.group1", groupDirectoryEntry[0].GetValue(GlbGroupSchema.GG_Desc));

				// Group2: No sync - groupDirectoryEntry2 was modified after LastSyncUtc but it is EnterpriseIsMaster+1Way and group2 was modified before so it wont be picked to sync
				AssertEquals("group2 should be synced", "cw.group2", group[1].GG_Desc);
				AssertEquals("groupDirectoryEntry2 should not be changed", "ad.group2", groupDirectoryEntry[1].GetValue(GlbGroupSchema.GG_Desc));

				// Group3: CW1 -> AD - group3 was modified after LastSyncUtc and groupDirectoryEntry3 was modified before
				AssertEquals("group3 should not be changed", "cw.group3", group[2].GG_Desc);
				AssertEquals("groupDirectoryEntry3 should be synced", "cw.group3", groupDirectoryEntry[2].GetValue(GlbGroupSchema.GG_Desc));

				// Group4: CW1 -> AD - when both modified after LastSyncUtc, use CW1
				AssertEquals("group4 should not changed", "cw.group4", group[3].GG_Desc);
				AssertEquals("groupDirectoryEntry4 should be synced", "cw.group4", groupDirectoryEntry[3].GetValue(GlbGroupSchema.GG_Desc));
			}
		}

		#endregion
	}
}
