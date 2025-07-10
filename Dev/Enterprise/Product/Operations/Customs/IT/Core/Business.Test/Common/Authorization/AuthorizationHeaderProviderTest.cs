using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AuthorizationHeaderProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new AuthorizationHeaderProvider(new Mock<IAuthorizationHeaderDataProvider>().Object, null));
		AssertExceptionThrown<ArgumentNullException>("When dataprovider is null", () => new AuthorizationHeaderProvider(null, Factory));
	}

	public void TestAuthorization()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertNull("Empty ZG_AuthorisationNumber", declaration.Authorization);

		declaration.ZG_AuthorisationNumber = "123456";
		AssertNull("Invalid ZG_AuthorisationNumber", declaration.Authorization);

		declaration.ZG_AuthorisationNumber = "1111111";
		AssertNull("Valid ZG_AuthorisationNumber, but other needed data not filled", declaration.Authorization);

		declaration.JE_MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		declaration.JE_OH_Supplier = organisation.PK;
		CombineAssertions("EXP/ALE Supplier", () =>
		{
			AssertNotNull(declaration.Authorization);
			AssertEquals("1111111", declaration.Authorization.CPH_Number);
		});
	}
}
