using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusAuthorisationRule))]
sealed class CusAuthorisationRuleTest : EnterpriseBusinessObjectTestCase
{
	public void TestGoodsLocationDescription()
	{
		var authorisationRule = (CusAuthorisationRule)GetNewBusinessObject();
		CombineAssertions(() =>
		{
			AssertEquals("GoodsLocationDescription has the default values initially", "T;B;;;NL", authorisationRule.GoodsLocationDescription);

			var goodsLocation = authorisationRule.GoodsLocation;
			goodsLocation.AdditionalIdentifier = "59";
			goodsLocation.Address.E2_Postcode = "1234 AB";
			AssertEquals("GoodsLocationDescription when there's a GoodsLocation","T;B;1234 AB;59;NL", authorisationRule.GoodsLocationDescription);
		});
	}

	public void TestGoodsLocation()
	{
		var authorisationRule = (CusAuthorisationRule)GetNewBusinessObject();
		var goodsLocation = authorisationRule.GoodsLocation;
		CombineAssertions(() =>
		{
			AssertEquals("CGL_ParentID", authorisationRule.PK, goodsLocation.CGL_ParentID);
			AssertEquals("CGL_ParentTableCode", "CPR", goodsLocation.CGL_ParentTableCode);
			AssertEquals("CGL_LocationUse", "CPR", goodsLocation.CGL_LocationUse);
			AssertSame("Cached", goodsLocation, authorisationRule.GoodsLocation);
			AssertEquals("IsRegisteredEditableChildObject", true, authorisationRule.IsRegisteredEditableChildObject(goodsLocation));
		});
	}

	public void TestICusGoodsLocationProvider_ProviderKey()
	{
		AssertEquals(ZString.Empty, (GetNewBusinessObject() as EU.Business.ICusGoodsLocationProvider).ProviderKey);
	}

	protected override BusinessObject GetNewBusinessObject() => SetupData(Factory).authorisationRule;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		(_, var rule) = SetupData(factory);
		rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
		rule.CPR_ValueFrom = "VALUE";
		return rule;
	}

	(CusAuthorisationHeader authorisationHeader, CusAuthorisationRule authorisationRule) SetupData(BusinessObjectFactory factory)
	{
		var authorisationHeader = factory.NewWithValidTestData<CusAuthorisationHeader>();
		var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		return (authorisationHeader, authorisationRule);
	}
}
