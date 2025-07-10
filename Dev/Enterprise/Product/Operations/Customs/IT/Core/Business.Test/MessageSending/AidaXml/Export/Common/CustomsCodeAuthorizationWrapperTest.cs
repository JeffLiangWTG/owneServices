using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class CustomsCodeAuthorizationWrapperTest : TestCaseWithFactory
{
	public void TestAuthorizationType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingCusCodeType("AUTH", "Authorisation");
		helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
		helper.CreateCusMap("EUNAU", "DPO", "C506", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
		helper.CreateCusMap("EUNAU", "SAS", "C515", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
		Factory.Save();

		var authorizationUsage = Factory.New<CusAuthorizationUsage>();
		var authorizationWrapper = GetWrapper(authorizationUsage);
		AssertEquals(nameof(IAuthorization.AuthorizationType), "", authorizationWrapper.AuthorizationType);

		authorizationUsage.AGC_Code = "SAS";
		authorizationWrapper = GetWrapper(authorizationUsage);
		AssertEquals(nameof(IAuthorization.AuthorizationType), "C515", authorizationWrapper.AuthorizationType);

		authorizationUsage.AGC_Code = "DPO";
		authorizationWrapper = GetWrapper(authorizationUsage);
		AssertEquals(nameof(IAuthorization.AuthorizationType), "C506", authorizationWrapper.AuthorizationType);
	}

	IAuthorization GetWrapper(EU.Business.CusAuthorizationUsage authorizationUsage) => new CustomsCodeAuthorizationWrapper(authorizationUsage);
}
