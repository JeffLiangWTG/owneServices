using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.FR.Business.NCTS.Testing;

sealed class NctsPackageDeparturePhase5ValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleB1819Active() => AssertEquals(expected: false, validationDecider.IsRuleB1819Active);

	public void TestIsRuleB1919Active() => AssertEquals(expected: false, validationDecider.IsRuleB1919Active);

	public void TestIsRuleC0060_1Active() => AssertEquals(expected: false, validationDecider.IsRuleC0060_1Active);

	public void TestIsRuleC0060_3Active() => AssertEquals(expected: false, validationDecider.IsRuleC0060_3Active);

	public void TestIsRuleC0670Active() => AssertEquals(expected: true, validationDecider.IsRuleC0670Active);

	public void TestIsRuleE1111Active() => AssertEquals(expected: true, validationDecider.IsRuleE1111Active);

	public void TestIsRuleNR0003Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0003Active);

	public void TestIsRuleNR0027Active() => AssertEquals(expected: false, validationDecider.IsRuleNR0027Active);

	public void TestIsRuleR0219Active() => AssertEquals(expected: false, validationDecider.IsRuleR0219Active);

	public void TestIsRuleR0220Active() => AssertEquals(expected: false, validationDecider.IsRuleR0220Active);

	public void TestIsRuleR0364_1Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_1Active);

	public void TestIsRuleR0364_2Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_2Active);

	public void TestIsRuleR0364_3Active() => AssertEquals(expected: false, validationDecider.IsRuleR0364_3Active);

	public void TestIsRuleTR0066Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0066Active);

	public void TestIsRuleTR0083Active() => AssertEquals(expected: true, validationDecider.IsRuleTR0083Active);

	public void TestIsRuleC0060Active()
	{
		Assert("Prerequisite: GoodsItems' packages are not customs compliant.", !nctsPackage.ItemsPackagesAreCustomsCompliant);
		Assert("RuleC0060 should only apply when GoodsItems' packages are not customs compliant.", validationDecider.IsRuleC0060Active);

		nctsPackage.B5_UnitCount = 1;
		Assert("Prerequisite: GoodsItems' packages are customs compliant.", nctsPackage.ItemsPackagesAreCustomsCompliant);
		Assert("RuleC0060 should not apply when GoodsItems' packages are customs compliant.", !validationDecider.IsRuleC0060Active);
	}

	public void TestIsRuleC0060_2Active()
	{
		Assert("Prerequisite: GoodsItems' packages are not customs compliant.", !nctsPackage.ItemsPackagesAreCustomsCompliant);
		Assert("RuleC0060_2 should only apply when GoodsItems' packages are not customs compliant.", validationDecider.IsRuleC0060_2Active);

		nctsPackage.B5_UnitCount = 1;
		Assert("Prerequisite: GoodsItems' packages are customs compliant.", nctsPackage.ItemsPackagesAreCustomsCompliant);
		Assert("RuleC0060_2 should not apply when GoodsItems' packages are customs compliant.", !validationDecider.IsRuleC0060_2Active);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var nctsBill = nctsHeader.Bills.AddNew();
		var goodsItem = nctsBill.GoodsItems.AddNew();
		nctsPackage = goodsItem.Packages.AddNew();

		validationDecider = new NctsPackageDeparturePhase5ValidationDecider(nctsPackage);
	}

	NctsPackageDeparturePhase5ValidationDecider validationDecider;
	NctsPackage nctsPackage;
}
