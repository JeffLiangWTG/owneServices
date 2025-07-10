using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AutomaticSignatureExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDelegateList()
	{
		SetUpRefSysConfigs(Factory);

		const string expectedElementsAsString =
			"ITARDSDL01 - brunoluigi.soldati\r\n" +
			"ITARDSDL02 - angela.stecca";

		var password = Factory.New<AutomaticSignatureExternalPassword>();
		var delegateList = password.Lookups.DelegateList;
		AssertEquals("DelegateList", expectedElementsAsString, delegateList.ElementsAsString);
		AssertSame("Cached", delegateList, password.Lookups.DelegateList);
	}

	internal static void SetUpRefSysConfigs(BusinessObjectFactory factory)
	{
		const string italyRemoteDigitalSignatureDelegate1 = "Italy Automatic Remote Digital Signature Delegate 01";
		const string italyRemoteDigitalSignatureDelegate2 = "Italy Automatic Remote Digital Signature Delegate 02";

		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateRefSysConfigType("ITARDSDL01", italyRemoteDigitalSignatureDelegate1, italyRemoteDigitalSignatureDelegate1);
		helper.CreateRefSysConfigType("ITARDSDL02", italyRemoteDigitalSignatureDelegate2, italyRemoteDigitalSignatureDelegate2);
		helper.CreateRefSysConfig("ITARDSDL01", "brunoluigi.soldati", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateRefSysConfig("ITARDSDL02", "angela.stecca", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}
}
