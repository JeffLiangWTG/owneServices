using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing.MessageSending.AidaXml.Shared;

sealed class AuthorizationWrapperBaseOnlyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When authorization is null",
				() => new AuthorizationWrapperForTest(authorization: null, x => ""));

			AssertExceptionThrown<ArgumentNullException>(
				"When authorizationTypeFunc is null",
				() => new AuthorizationWrapperForTest(Factory.New<EU.Business.CusAuthorizationUsage>(), authorizationTypeFunc: null));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			var authorizationOwner = Factory.New<OrgHeader>();
			authorizationOwner.CustomsCodes.AddNew("EOR", "385040449", "IT");

			var authorizationUsage = Factory.New<CusAuthorizationUsage>();
			var authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.IdentificationNumber), "", authorizationWrapper.IdentificationNumber);

			authorizationUsage.AGC_OH_Owner = authorizationOwner.PK;
			authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.IdentificationNumber), "IT385040449", authorizationWrapper.IdentificationNumber);
		});
	}

	public void TestReferenceNumber()
	{
		CombineAssertions(() =>
		{
			var authorizationUsage = Factory.New<CusAuthorizationUsage>();
			var authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.ReferenceNumber), "", authorizationWrapper.ReferenceNumber);

			authorizationUsage.AGC_Number = "1234";
			authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.ReferenceNumber), "1234", authorizationWrapper.ReferenceNumber);
		});
	}

	IAuthorization GetWrapper(EU.Business.CusAuthorizationUsage authorizationUsage) => new AuthorizationWrapperForTest(authorizationUsage, x => "");
}

sealed class AuthorizationWrapperForTest : AuthorizationWrapperBase
{
	public AuthorizationWrapperForTest(EU.Business.CusAuthorizationUsage authorization, Func<EU.Business.CusAuthorizationUsage, string> authorizationTypeFunc) : base(authorization, authorizationTypeFunc)
	{
	}
}
