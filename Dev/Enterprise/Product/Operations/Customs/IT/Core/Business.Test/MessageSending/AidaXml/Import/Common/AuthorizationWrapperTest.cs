using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class AuthorizationWrapperTest : TestCaseWithFactory
{
	public void TestAuthorizationType()
	{
		CombineAssertions(() =>
		{
			var authorizationUsage = Factory.New<CusAuthorizationUsage>();
			var authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.AuthorizationType), "", authorizationWrapper.AuthorizationType);

			authorizationUsage.AGC_Code = "DPO";
			authorizationWrapper = GetWrapper(authorizationUsage);
			AssertEquals(nameof(IAuthorization.AuthorizationType), "DPO", authorizationWrapper.AuthorizationType);
		});
	}

	IAuthorization GetWrapper(EU.Business.CusAuthorizationUsage authorizationUsage) => new AuthorizationWrapper(authorizationUsage);
}
