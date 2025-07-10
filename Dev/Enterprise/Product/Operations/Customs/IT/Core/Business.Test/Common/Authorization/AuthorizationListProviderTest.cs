using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AuthorizationListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new AuthorizationListProvider(null));
	}

	public void TestGetAuthorisationsThrowsExceptionWhenProviderIsNull()
	{
		AssertExceptionThrown<ArgumentNullException>("When authorizationListDataProvider is null", () => authorizationListProvider.GetAuthorizations(null));
	}

	public void TestGetAuthorisationsThrowsException()
	{
		authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns((IEnumerable<ZGuid>)null);
		authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns((IEnumerable<ZString>)null);
		AssertExceptionThrown<ArgumentNullException>($"When {nameof(IAuthorizationListDataProvider.GetEligibleHolders)} is null", () => authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object));
	}

	public void TestGetAuthorisations()
	{
		var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

		var startDate = ZDate.Today.AddDays(-10);
		var endDate = ZDate.Today.AddDays(10);
		var authorisationHeader1 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: AuthorizationTypeForTest, permitHolder: orgHeader1.PK, "1111111", startDate: startDate, endDate: endDate);
		var authorisationHeader2 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: AuthorizationTypeForTest, permitHolder: ZGuid.Empty, "2222222", startDate: startDate, endDate: endDate);
		var authorisationHeader3 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: "AAA", permitHolder: orgHeader1.PK, "3333333", startDate: startDate, endDate: endDate);
		var authorisationHeader4 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: AuthorizationTypeForTest, permitHolder: orgHeader2.PK, "4444444", startDate: startDate, endDate: endDate);

		CombineAssertions($"Listing {AuthorizationTypeForTest} - {nameof(orgHeader1)}", () =>
		{
			authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(AuthorizationTypes);
			authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { orgHeader1.PK });
			var authorizationList = authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object);
			AssertEquals(nameof(authorizationList.ElementsAsString), $"1111111 - {orgHeader1.OH_Code}", authorizationList.ElementsAsString);

			authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(AuthorizationTypes);
			authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { orgHeader1.PK });
			AssertSame($"{nameof(authorizationList)} and {nameof(authorizationListProvider.GetAuthorizations)}", authorizationList, authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object));
		});

		CombineAssertions($"Listing {AuthorizationTypeForTest} - {nameof(orgHeader1)} and {nameof(orgHeader2)}", () =>
		{
			authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(AuthorizationTypes);
			authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { orgHeader1.PK, orgHeader2.PK });
			var authorizationList = authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object);
			AssertEquals(nameof(authorizationList.ElementsAsString), $"1111111 - {orgHeader1.OH_Code}\r\n4444444 - {orgHeader2.OH_Code}", authorizationList.ElementsAsString);

			authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(AuthorizationTypes);
			authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { orgHeader1.PK, orgHeader2.PK });
			AssertSame($"{nameof(authorizationList)} and {nameof(authorizationListProvider.GetAuthorizations)}", authorizationList, authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object));
		});

		CombineAssertions($"Listing 'XYZ' - {nameof(orgHeader1)} ", () =>
		{
			authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(new ZString[] { "XYZ" });
			authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { orgHeader1.PK });
			var authorizationList = authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object);
			AssertEquals(nameof(authorizationList.CodesAsString), ZString.Empty, authorizationList.CodesAsString);
		});
	}

	public void TestGetAuthorisationsWhenEmptyAuthorizationTypeProvided()
	{
		authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(new ZString[] { ZString.Empty });
		authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(new ZGuid[] { ZGuid.NewZGuid() });
		var authorizationList = authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object);
		AssertEquals(nameof(authorizationList.Count), 0, authorizationList.Count);
	}

	public void TestGetAuthorisationsWhenEmptyEligibleHoldersProvided()
	{
		authorizationListDataProviderMock.Setup(m => m.AuthorizationTypes).Returns(new ZString[] { "AAA" });
		authorizationListDataProviderMock.Setup(m => m.GetEligibleHolders()).Returns(Array.Empty<ZGuid>());
		var authorizationList = authorizationListProvider.GetAuthorizations(authorizationListDataProviderMock.Object);
		AssertEquals(nameof(authorizationList.Count), 0, authorizationList.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorizationListProvider = new AuthorizationListProvider(Factory);
		authorizationListDataProviderMock = new Mock<IAuthorizationListDataProvider>() { CallBase = true };
	}
	AuthorizationListProvider authorizationListProvider;
	Mock<IAuthorizationListDataProvider> authorizationListDataProviderMock;

	ZString[] AuthorizationTypes => new ZString[] { AuthorizationTypeForTest };
	const string AuthorizationTypeForTest = "XXX";
}
