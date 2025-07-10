using System;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ADGroupTest : ADEntityTest
	{
		public void TestConstructorWithNullGroup_ShouldThrow()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ADGroup(null));
			AssertNoExceptionThrown(() => new ADGroup(Factory.New<GlbGroup>()));
		}

		public override void TestGetPropertiesRequiringMissingDirectoryEntry_ShouldThrowInformativeException()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Valar";
			var adGroup = new ADGroup(group);

			directorySearcherMock.Setup(s => s.FindGroup("Valar", string.Empty)).Returns((IGroupDirectoryEntry)null);

			AssertExceptionThrown<DirectoryServicesException>(() => { var v = adGroup.GroupName; });
		}

		public override void TestIsIdentityInConflict()
		{
			var directoryEntry1 = DummyDirectoryEntryWrapper.CreateGroup("Valar");
			var directoryEntry2 = DummyDirectoryEntryWrapper.CreateGroup("Wizards");

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "Valar";
			group.GG_ActiveDirectoryObjectGuid = directoryEntry1.Guid;

			directorySearcherMock.Setup(s => s.FindGroup(directoryEntry1.Guid, string.Empty)).Returns(directoryEntry1);
			directorySearcherMock.Setup(s => s.FindGroup("Wizards", string.Empty)).Returns(directoryEntry2);

			var adGroup = new ADGroup(group);
			Assert(!adGroup.IsIdentityInConflict());
			group.GG_Desc = "Wizards";
			Assert(adGroup.IsIdentityInConflict());
		}

		public void TestGroupCanBeSyncAgainAfterDeactivated()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ttg";
			group.GG_Desc = "test group";

			group.DisconnectFromAD();
			AssertEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
			Assert(!group.IsADLinked);
			Assert(!group.GG_IsActive);

			group.GG_IsActive = true;

			var adGroup = new ADGroup(group);
			ADEntityProviderSubstitution.ADGroup = adGroup;
			var dirEntry = DummyDirectoryEntryWrapper.CreateGroup(group.GG_Desc);
			var orgUnit = new Mock<IOrganisationalUnit>();

			directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(orgUnit.Object);
			orgUnit.Setup(x => x.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), directorySearcherMock.Object)).Returns(dirEntry);

			group.SynchroniseWithAD();

			Assert("IsADLinked", group.IsADLinked);
			Assert("GG_IsActive", group.GG_IsActive);
			AssertNotEquals(ZGuid.Empty, group.GG_ActiveDirectoryObjectGuid);
			AssertNotEquals(ZGuid.Invalid, group.GG_ActiveDirectoryObjectGuid);
			Assert(group.GG_ActiveDirectoryObjectGuid.IsValid);
		}

		protected override IADLinkedEntity CreateEnterpriseEntity(string identity, string domainName, ZGuid activeDirectoryObjectGuid)
		{
			var group = Helper.CreateGroup(identity);
			group.GG_DomainName = domainName;
			group.GG_ActiveDirectoryObjectGuid = activeDirectoryObjectGuid;
			return group;
		}

		protected override IADEntity CreateADEntity(IADLinkedEntity enterpriseEntity)
		{
			return new ADGroup((GlbGroup)enterpriseEntity);
		}

		public override void TestSync_WithRaceCondition()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());

			// Instance 1 creates the group but not sync to AD
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var group = factory1.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "groupJ";
			group.GG_ActiveDirectoryObjectGuid = ZGuid.Invalid;
			factory1.Save();

			// Instance 2 sync and link it to a new AD user on different AD site not yet accessible by instance 1
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var groupFromFactory2 = factory2.Load<GlbGroup>(group.PK);
			groupFromFactory2.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			factory2.Save();

			// When sync from Instance 1, it should reload and relised it has been linked, and report error as it is not yet accessible.
			AssertEquals("Staff should not linked yet in the factory1 before sync", false, group.IsADLinked);
			var adGroup = new ADGroup(group);
			ADEntityProviderSubstitution.ADGroup = adGroup;
			group.SynchroniseWithAD();

			var expectedMessage = @"Cannot synchronize 'groupJ'. The linked Active Directory object is not accessible at this time. It is either pending creation, deleted or is located outside the Organizational Unit root/Accounts/ADUnitTesting of domain sand.wtg.zone set in the registry item: System -> Staff -> Active Directory -> Domain Credentials Collection.
If 'groupJ' was created or activated recently please try again later.";
			AssertEquals("Expecting error message when sync from Instance 1", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Staff should be linked", true, group.IsADLinked);
			AssertEquals("Group should be linked to the same AD object", groupFromFactory2.GG_ActiveDirectoryObjectGuid, group.GG_ActiveDirectoryObjectGuid);
		}
	}

	[TestedType(typeof(ADGroup))]
	class ADGroup_NonPersistentBizoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "nerd";
			return new ADGroup(group);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			directorySearcherMock = new Mock<IDirectorySearcher>();
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = directorySearcherMock.Object;
			directorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateGroup(""));
		}
		Mock<IDirectorySearcher> directorySearcherMock;
	}
}
