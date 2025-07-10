using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(Extensions))]
sealed class ExtensionsTest : TestCaseWithFactory
{
	public void TestGetAEONumber()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN = org.Addresses.AddNew();
		OrgCusCode cusCode = org.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "1234");
		AssertEquals("1234", org.GetAEONumber());
	}

	public void TestGetCustomsRegNo()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN = org.Addresses.AddNew();
		OrgCusCode cusCode = org.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "1234");
		cusCode.OK_OA_PremisesAddress = addressIN.PK;

		var customsRegNo1 = org.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, Core.Constants.CountryCodes.India);
		AssertEquals("1234", customsRegNo1);

		var customsRegNo2 = addressIN.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, Core.Constants.CountryCodes.India);
		AssertEquals("1234", customsRegNo2);
	}
}
