using System;
using System.Collections.Generic;
using System.DirectoryServices;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Integration;
using Moq;

namespace Enterprise.Security.ActiveDirectory.Test
{
	public class DirectorySearcherProviderForTest : IDirectorySearcherProvider
	{
		public IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege)
		{
			if (DoNotMock)
			{
				DirectorySearcherFactory.DirectorySearcherOverride_ForTest = null;
				return DirectorySearcherFactory.GetDirectorySearcher(domainCredentials, requireDomainWritePrivilege);
			}
			else
			{
				return DirectorySearcherMock.Object;
			}
		}

		public bool DoNotMock;

		public Mock<IDirectorySearcher> DirectorySearcherMock
		{
			get
			{
				if (directorySearcherMock == null)
				{
					directorySearcherMock = new Mock<IDirectorySearcher>();
					directorySearcherMock.Setup(s => s.FindOrganisationalUnit(It.IsAny<string>())).Returns(OrganisationalUnitMock);
					directorySearcherMock.Setup(s => s.FindUsersChangedAfter(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<SearchScope>()))
						.Returns(UserDirectorySearchResultsMock);
					directorySearcherMock.Setup(s => s.FindGroupsChangedAfter(It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<SearchScope>()))
						.Returns(GroupDirectorySearchResultsMock);
				}
				return directorySearcherMock;
			}
			set
			{
				directorySearcherMock = value;
				DoNotMock = false;
			}
		}
		Mock<IDirectorySearcher> directorySearcherMock;

		public IDirectorySearchResults<IUserDirectorySearchResult> UserDirectorySearchResultsMock
		{
			get
			{
				if (userDirectorySearchResultsMock == null)
				{
					var mock = new Mock<IDirectorySearchResults<IUserDirectorySearchResult>>();
					mock.Setup(a => a.GetEnumerator()).Returns(userDirectorySearcherResultList.GetEnumerator());

					userDirectorySearchResultsMock = mock.Object;
				}
				return userDirectorySearchResultsMock;
			}
			set
			{
				userDirectorySearchResultsMock = value;
				DoNotMock = false;
			}
		}
		IDirectorySearchResults<IUserDirectorySearchResult> userDirectorySearchResultsMock;

		public void AddToUserDirectorySearcherResult(params IUserDirectorySearchResult[] results)
		{
			results.ForEach(r => userDirectorySearcherResultList.Add(r));
			Mock.Get(UserDirectorySearchResultsMock).Setup(a => a.GetEnumerator()).Returns(userDirectorySearcherResultList.GetEnumerator());
		}
		readonly List<IUserDirectorySearchResult> userDirectorySearcherResultList = new List<IUserDirectorySearchResult>();

		public IDirectorySearchResults<IGroupDirectorySearchResult> GroupDirectorySearchResultsMock
		{
			get
			{
				if (groupDirectorySearchResultsMock == null)
				{
					var mock = new Mock<IDirectorySearchResults<IGroupDirectorySearchResult>>();
					mock.Setup(g => g.GetEnumerator()).Returns(groupDirectorySearcherResultList.GetEnumerator());

					groupDirectorySearchResultsMock = mock.Object;
				}
				return groupDirectorySearchResultsMock;
			}
			set
			{
				groupDirectorySearchResultsMock = value;
				DoNotMock = false;
			}
		}
		IDirectorySearchResults<IGroupDirectorySearchResult> groupDirectorySearchResultsMock;

		public void AddToGroupDirectorySearcherResult(params IGroupDirectorySearchResult[] results)
		{
			results.ForEach(r => groupDirectorySearcherResultList.Add(r));
			Mock.Get(GroupDirectorySearchResultsMock).Setup(g => g.GetEnumerator()).Returns(groupDirectorySearcherResultList.GetEnumerator());
		}
		readonly List<IGroupDirectorySearchResult> groupDirectorySearcherResultList = new List<IGroupDirectorySearchResult>();

		public IOrganisationalUnit OrganisationalUnitMock
		{
			get
			{
				if (organisationalUnitMock == null)
				{
					var mock = new Mock<IOrganisationalUnit>();
					mock.Setup(o => o.CreateNewChild(It.IsAny<string>(), It.IsAny<DirectoryObjectType>(), It.IsAny<IDirectorySearcher>()))
						.Returns((string name, DirectoryObjectType entityType, IDirectorySearcher directorySearcher) =>
						{
							switch (entityType)
							{
								case DirectoryObjectType.Group:
									return DummyDirectoryEntryWrapper.CreateGroup(name);

								case DirectoryObjectType.OrganisationalUnit:
									return DummyDirectoryEntryWrapper.CreateOU(name);

								default:
									return DummyDirectoryEntryWrapper.CreateUser(name);
							}
						});

					organisationalUnitMock = mock.Object;
				}
				return organisationalUnitMock;
			}
			set
			{
				organisationalUnitMock = value;
				DoNotMock = false;
			}
		}
		IOrganisationalUnit organisationalUnitMock;
	}

	public class DirectorySearcherProviderWithMultiDomainsSupportForTest : IDirectorySearcherProvider
	{
		public DirectorySearcherProviderWithMultiDomainsSupportForTest()
		{
			DirectorySeacherDictionary = new Dictionary<string, Mock<IDirectorySearcher>>();
			foreach (DomainCredentials dc in ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.Value)
			{
				DirectorySeacherDictionary.Add(dc.DomainName, new Mock<IDirectorySearcher>());
			}
		}

		public IDirectorySearcher GetDirectorySearcher(IDomainCredentials domainCredentials, bool requireDomainWritePrivilege)
		{
			return DirectorySeacherDictionary[domainCredentials.DomainName]?.Object;
		}

		public Dictionary<string, Mock<IDirectorySearcher>> DirectorySeacherDictionary { get; set; }
	}
}
