using System;
using System.DirectoryServices;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public abstract class ADEntityTest : TestCaseWithFactoryAndMocks
	{
		protected abstract IADLinkedEntity CreateEnterpriseEntity(string identity, string domainName = "", ZGuid activeDirectoryObjectGuid = default(ZGuid));

		protected abstract IADEntity CreateADEntity(IADLinkedEntity enterpriseEntity);

		public abstract void TestSync_WithRaceCondition();

		public abstract void TestIsIdentityInConflict();

		public abstract void TestGetPropertiesRequiringMissingDirectoryEntry_ShouldThrowInformativeException();

		public void TestDomainCredentials_MultipleDomains()
		{
			var domainCredentials1 = ADTestHelper.CreateDomainCredentials("domain1", isDefaultDomain: false);
			var domainCredentials2 = ADTestHelper.CreateDomainCredentials("domain2", isDefaultDomain: true);
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials1, domainCredentials2));

			var adEntity1 = CreateADEntity(CreateEnterpriseEntity("sam", "domain1", ZGuid.Empty));
			var adEntity2 = CreateADEntity(CreateEnterpriseEntity("bob", "domain2", ZGuid.Empty));
			var adEntity3 = CreateADEntity(CreateEnterpriseEntity("jon", ""));
			var adEntity4 = CreateADEntity(CreateEnterpriseEntity("ham", "domainX"));

			AssertEquals("Entity1 should sync to domain1", domainCredentials1.DomainName, adEntity1.DomainCredentials.DomainName);
			AssertEquals("Entity2 should sync to domain2", domainCredentials2.DomainName, adEntity2.DomainCredentials.DomainName);
			AssertEquals("Entity3 should sync to default domain (domain2) as no domainName is set", domainCredentials2.DomainName, adEntity3.DomainCredentials.DomainName);
			AssertExceptionThrown<DirectoryServicesException>(
				"Entity4 should throw exception as DomainName is invalid",
				"The domain 'domainX' of 'ham' is not specified in the registry item: 'System -> Staff -> Active Directory -> Domain Credentials Collection'",
				() => { var a = adEntity4.DomainCredentials; }
			);
		}

		public void TestDomainCredentials_AutoSetDomain()
		{
			var domainCredentials1 = ADTestHelper.CreateDomainCredentials("domain1", isDefaultDomain: false);
			var domainCredentials2 = ADTestHelper.CreateDomainCredentials("domain2", isDefaultDomain: true);
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials1, domainCredentials2));

			var provider = new DirectorySearcherProviderWithMultiDomainsSupportForTest();
			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute<IDirectorySearcherProvider>(provider);

			var adEntity1 = CreateADEntity(CreateEnterpriseEntity("sam"));
			var adEntity2 = CreateADEntity(CreateEnterpriseEntity("bob"));
			var adEntity3 = CreateADEntity(CreateEnterpriseEntity("jon"));
			var adEntity4 = CreateADEntity(CreateEnterpriseEntity("ham", "domainX"));

			provider.DirectorySeacherDictionary["domain1"].Setup(s => s.FindUser(adEntity1.EnterpriseIdentity, domainCredentials1.UserOrganisationalUnit)).Returns(DummyDirectoryEntryWrapper.CreateUser("sam"));
			provider.DirectorySeacherDictionary["domain2"].Setup(s => s.FindUser(adEntity2.EnterpriseIdentity, domainCredentials2.UserOrganisationalUnit)).Returns(DummyDirectoryEntryWrapper.CreateUser("bob"));
			provider.DirectorySeacherDictionary["domain1"].Setup(s => s.FindGroup(adEntity1.EnterpriseIdentity, domainCredentials1.UserOrganisationalUnit)).Returns(DummyDirectoryEntryWrapper.CreateGroup("sam"));
			provider.DirectorySeacherDictionary["domain2"].Setup(s => s.FindGroup(adEntity2.EnterpriseIdentity, domainCredentials2.UserOrganisationalUnit)).Returns(DummyDirectoryEntryWrapper.CreateGroup("bob"));

			AssertEquals("Entity1 should sync to domain1", domainCredentials1.DomainName, adEntity1.DomainCredentials.DomainName);
			AssertEquals("Entity2 should sync to domain2", domainCredentials2.DomainName, adEntity2.DomainCredentials.DomainName);
			AssertEquals("Entity3 should sync to default domain (domain2) as no domainName is set", domainCredentials2.DomainName, adEntity3.DomainCredentials.DomainName);
			AssertExceptionThrown<DirectoryServicesException>(
				"Entity4 should throw exception as DomainName is invalid",
				"The domain 'domainX' of 'ham' is not specified in the registry item: 'System -> Staff -> Active Directory -> Domain Credentials Collection'",
				() => { var a = adEntity4.DomainCredentials; }
			);
		}

		public void TestDomainCredentials_AutoSetDomain_ShouldNotRequireWritePrivilege()
		{
			var domainCredentials1 = ADTestHelper.CreateDomainCredentials("domain1", isDefaultDomain: true);
			var domainCredentials2 = ADTestHelper.CreateDomainCredentials("domain2", isDefaultDomain: false);
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials1, domainCredentials2));

			var provider = new Mock<IDirectorySearcherProvider>();
			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(provider.Object);

			var adEntity = CreateADEntity(CreateEnterpriseEntity("bob"));

			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
			var domain1Searcher = new Mock<IDirectorySearcher>();
			var domain2Searcher = new Mock<IDirectorySearcher>();
			domain2Searcher.Setup(s => s.FindUser(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateUser("bob"));
			domain2Searcher.Setup(s => s.FindGroup(It.IsAny<string>(), It.IsAny<string>())).Returns(DummyDirectoryEntryWrapper.CreateGroup("bob"));

			provider.Setup(p => p.GetDirectorySearcher(It.IsAny<IDomainCredentials>(), It.IsAny<bool>())).Returns(
				(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege) =>
				{
					AssertEquals("Should not require domain write privilege", false, requireDomainWritePrivilege);

					if (domainCredentials.DomainName == domainCredentials2.DomainName)
					{
						return domain2Searcher.Object;
					}
					else
					{
						return domain1Searcher.Object;
					}
				});

			AssertEquals("Entity should sync to domain2", domainCredentials2.DomainName, adEntity.DomainCredentials.DomainName);
		}

		public void TestDomainCredentials_SingleDomain()
		{
			var domainCredentials = ADTestHelper.CreateDomainCredentials("domain1", isDefaultDomain: true);
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainCredentials));

			var adEntity1 = CreateADEntity(CreateEnterpriseEntity("sam", "domain1"));
			var adEntity2 = CreateADEntity(CreateEnterpriseEntity("bob", "domain2"));
			var adEntity3 = CreateADEntity(CreateEnterpriseEntity("jon", ""));

			AssertEquals("Entity1 with DomainName correctly set should sync to default domain", domainCredentials.DomainName, adEntity1.DomainCredentials.DomainName);
			AssertEquals("Entity2 with DomainName set incorrectly should sync to default domain", domainCredentials.DomainName, adEntity2.DomainCredentials.DomainName);
			AssertEquals("Entity3 with DomainName not set should sync to default domain", domainCredentials.DomainName, adEntity3.DomainCredentials.DomainName);
		}

		public void TestDomainCredentials_RegistryNotSet()
		{
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DomainCredentialsCollection());

			var adEntity1 = CreateADEntity(CreateEnterpriseEntity("sam", "domain1"));
			var adEntity2 = CreateADEntity(CreateEnterpriseEntity("bob", ""));

			AssertEquals("DomainCredentials should be null", null, adEntity1.DomainCredentials);
			AssertEquals("DomainCredentials should be null", null, adEntity2.DomainCredentials);
		}

		public void TestDisconnectFromAD()
		{
			var cw1Entity = CreateEnterpriseEntity("something", activeDirectoryObjectGuid: ZGuid.NewZGuid());
			var adEntity = CreateADEntity(cw1Entity);
			AssertEquals(true, cw1Entity.IsADLinked);
			AssertEquals(true, cw1Entity.IsActive);

			adEntity.DisconnectFromAD();

			AssertEquals(false, cw1Entity.IsADLinked);
			AssertEquals(false, cw1Entity.IsActive);
		}

		public void TestCommitChanges_NoSuchAttribute()
		{
			var expectedMessage = "One of the attributes in the Attribute Mapping Registry setting does not exist in the domain. Please review the mappings in the System -> Staff -> Active Directory -> Attribute Mapping setting in the Registry. If there are any custom attributes used in the mappings, make sure that the custom attributes are defined in all domains.";
			AssertCommitChangesWithAttributeError(unchecked((int)0x8007200A), expectedMessage); // ErrorCode_NO_SUCH_ATTRIBUTE
		}

		public void TestCommitChanges_NonWritableAttribute()
		{
			var expectedMessage = "One of the attributes in the Attribute Mapping Registry setting is not writable. Please review the mappings in the System -> Staff -> Active Directory -> Attribute Mapping setting in the Registry.";
			AssertCommitChangesWithAttributeError(unchecked((int)0x80072035), expectedMessage); // ErrorCode_UNWILLING_TO_PERFORM
		}

		void AssertCommitChangesWithAttributeError(int errorCode, string expectedMessage)
		{
			var innerExpMock = new Mock<DirectoryServicesCOMException>();
			innerExpMock.SetupGet(e => e.ErrorCode).Returns(errorCode); // ErrorCode_NO_SUCH_ATTRIBUTE
			var exception = new DirectoryServicesException("bla", innerExpMock.Object);

			var userEntry = new Mock<IUserDirectoryEntry>();
			userEntry.SetupGet(x => x.HasChanges).Returns(true);
			userEntry.SetupGet(x => x.CanUpdate).Returns(true);
			userEntry.Setup(x => x.CommitChanges()).Throws(exception);

			var groupEntry = new Mock<IGroupDirectoryEntry>();
			groupEntry.SetupGet(x => x.HasChanges).Returns(true);
			groupEntry.SetupGet(x => x.CanUpdate).Returns(true);
			groupEntry.Setup(x => x.CommitChanges()).Throws(exception);

			var cw1Entity = CreateEnterpriseEntity("dummy1", activeDirectoryObjectGuid: ZGuid.NewZGuid());
			var adEntity = CreateADEntity(cw1Entity);

			directorySearcherMock.Setup(s => s.FindUser("dummy1", "")).Returns(userEntry.Object);
			directorySearcherMock.Setup(s => s.FindGroup("dummy1", "")).Returns(groupEntry.Object);

			adEntity.GetDirectoryEntry(true); // need to do a GetDirectoryEntry(true) call first to avoid throwing an unexpected exception before CommitChanges()
			adEntity.CommitChanges();

			AssertContains("Error popup", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
