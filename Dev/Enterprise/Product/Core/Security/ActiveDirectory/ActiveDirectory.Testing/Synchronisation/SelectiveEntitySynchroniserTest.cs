using System;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.Synchronisation;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test.Synchronisation
{
	class SelectiveEntitySynchroniserTest : TestCaseWithFactoryAndMocks
	{
		public void TestSynchronise_ShouldOnlySyncSelectedEntities()
		{
			var syncedStaff1 = Factory.New<GlbStaff>();
			syncedStaff1.GS_LoginName = "frodo.baggins";
			var syncedStaff2 = Factory.New<GlbStaff>();
			syncedStaff2.GS_LoginName = "samwise.gamgee";

			var unsyncedStaff1 = Factory.New<GlbStaff>();
			unsyncedStaff1.GS_LoginName = "merriadoc.brandybuck";
			var unsyncedStaff2 = Factory.New<GlbStaff>();
			unsyncedStaff2.GS_LoginName = "peregrin.took";

			var syncedGroup1 = Factory.New<GlbGroup>();
			syncedGroup1.GG_Desc = "Bagginses";
			var syncedGroup2 = Factory.New<GlbGroup>();
			syncedGroup2.GG_Desc = "Gamgees";

			var unsyncedGroup1 = Factory.New<GlbGroup>();
			unsyncedGroup1.GG_Desc = "Brandybucks";
			var unsyncedGroup2 = Factory.New<GlbGroup>();
			unsyncedGroup2.GG_Desc = "Tooks";

			var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("frodo.baggins");
			var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("samwise.gamgee");
			var userEntry3 = DummyDirectoryEntryWrapper.CreateUser("merriadoc.brandybuck");
			var userEntry4 = DummyDirectoryEntryWrapper.CreateUser("peregrin.took");

			var groupEntry1 = DummyDirectoryEntryWrapper.CreateGroup("Bagginses");
			var groupEntry2 = DummyDirectoryEntryWrapper.CreateGroup("Gamgees");
			var groupEntry3 = DummyDirectoryEntryWrapper.CreateGroup("Brandybucks");
			var groupEntry4 = DummyDirectoryEntryWrapper.CreateGroup("Tooks");

			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;

			searcher.Setup(s => s.FindUser("frodo.baggins", TestConstants.ValidOU)).Returns(userEntry1);
			searcher.Setup(s => s.FindUser("samwise.gamgee", TestConstants.ValidOU)).Returns(userEntry2);
			searcher.Setup(s => s.FindUser("merriadoc.brandybuck", TestConstants.ValidOU)).Returns(userEntry3);
			searcher.Setup(s => s.FindUser("peregrin.took", TestConstants.ValidOU)).Returns(userEntry4);

			searcher.Setup(s => s.FindGroup("Bagginses", TestConstants.ValidOU)).Returns(groupEntry1);
			searcher.Setup(s => s.FindGroup("Gamgees", TestConstants.ValidOU)).Returns(groupEntry2);
			searcher.Setup(s => s.FindGroup("Brandybucks", TestConstants.ValidOU)).Returns(groupEntry3);
			searcher.Setup(s => s.FindGroup("Tooks", TestConstants.ValidOU)).Returns(groupEntry4);

			var syncer = new SelectiveEntitySynchroniser(Factory, new BusinessObject[] { syncedStaff1, syncedStaff2, syncedGroup1, syncedGroup2 });
			syncer.Synchronise();

			Assert(syncedStaff1.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedStaff2.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedGroup1.GG_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedGroup2.GG_ActiveDirectoryObjectGuid.IsValid);

			Assert(!unsyncedStaff1.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(!unsyncedStaff2.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(!unsyncedGroup1.GG_ActiveDirectoryObjectGuid.IsValid);
			Assert(!unsyncedGroup2.GG_ActiveDirectoryObjectGuid.IsValid);
		}

		public void TestSync_WhenNoEntitiesSelected_ShouldSyncAll()
		{
			var syncedStaff1 = Factory.New<GlbStaff>();
			syncedStaff1.GS_LoginName = "frodo.baggins";
			var syncedStaff2 = Factory.New<GlbStaff>();
			syncedStaff2.GS_LoginName = "samwise.gamgee";

			var syncedGroup1 = Factory.New<GlbGroup>();
			syncedGroup1.GG_Code = "G01";
			syncedGroup1.GG_Desc = "Bagginses";
			var syncedGroup2 = Factory.New<GlbGroup>();
			syncedGroup2.GG_Code = "G02";
			syncedGroup2.GG_Desc = "Gamgees";
			Factory.Save();

			var userEntry1 = DummyDirectoryEntryWrapper.CreateUser("frodo.baggins");
			var userEntry2 = DummyDirectoryEntryWrapper.CreateUser("samwise.gamgee");

			var groupEntry1 = DummyDirectoryEntryWrapper.CreateGroup("Bagginses");
			var groupEntry2 = DummyDirectoryEntryWrapper.CreateGroup("Gamgees");

			var searcher = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = searcher.Object;

			searcher.Setup(s => s.FindUser("frodo.baggins", TestConstants.ValidOU)).Returns(userEntry1);
			searcher.Setup(s => s.FindUser("samwise.gamgee", TestConstants.ValidOU)).Returns(userEntry2);

			searcher.Setup(s => s.FindGroup("Bagginses", TestConstants.ValidOU)).Returns(groupEntry1);
			searcher.Setup(s => s.FindGroup("Gamgees", TestConstants.ValidOU)).Returns(groupEntry2);

			var syncer = new SelectiveEntitySynchroniser(Factory, Enumerable.Empty<BusinessObject>());
			syncer.Synchronise();

			Assert(syncedStaff1.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedStaff2.GS_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedGroup1.GG_ActiveDirectoryObjectGuid.IsValid);
			Assert(syncedGroup2.GG_ActiveDirectoryObjectGuid.IsValid);
		}

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
			var syncer = new SelectiveEntitySynchroniser(factory1, new[] { staff });
			syncer.Synchronise();

			var expectedMessage = @"Cannot synchronize 'jon'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'jon' was created or activated recently please try again later.";
			AssertEquals("Expecting user error message when sync from Instance 1", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Staff should be linked", true, staff.IsADLinked);
			AssertEquals("Staff should be linked to the same AD object", staffFromFactory2.GS_ActiveDirectoryObjectGuid, staff.GS_ActiveDirectoryObjectGuid);

			UnitTestUserNotification.Instance.ClearMessages();
			syncer = new SelectiveEntitySynchroniser(factory1, new[] { group });
			syncer.Synchronise();
			expectedMessage = @"Cannot synchronize 'groupJ'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'groupJ' was created or activated recently please try again later.";
			AssertEquals("Expecting error message when sync from Instance 1", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Group should be linked", true, group.IsADLinked);
			AssertEquals("Group should be linked to the same AD object", groupFromFactory2.GG_ActiveDirectoryObjectGuid, group.GG_ActiveDirectoryObjectGuid);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
		}
	}
}
