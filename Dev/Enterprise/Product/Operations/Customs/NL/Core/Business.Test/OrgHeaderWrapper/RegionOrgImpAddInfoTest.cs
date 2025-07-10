using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(RegionOrgImpAddInfo))]
public class RegionOrgImpAddInfoTest : EU.Business.Testing.EUOrgImpAddInfoAbstractTest
{
	public void TestMethodOfPaymentDefaultValue()
	{
		var org = Factory.New<OrgHeader>();
		org.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		var orgImpAddInfo = RegionOrgImpAddInfo.Get(org, Core.Constants.CountryCodes.Netherlands);
		AssertContains(PaymentPartyList.Codes.Representative, orgImpAddInfo.ZO_OtherDeferType);
	}

	protected override BusinessObject GetNewBusinessObject() => new RegionOrgImpAddInfo(Factory);
}
