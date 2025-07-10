using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ConditionCalcDataForGuidedDecisionMakingBasicTest : TestCaseWithFactory
	{
		public void TestUnitOfMeasureValueList()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.CustomsFirstQuantity = 11m;
			guidedDecisionMakingBasic.CustomsSecondQuantity = 22m;
			guidedDecisionMakingBasic.CustomsSecondUnitQty = "LTR";
			guidedDecisionMakingBasic.CustomsThirdQuantity = 33m;
			guidedDecisionMakingBasic.CustomsThirdUnitQty = "HLT";
			var calcData = new ConditionCalcDataForGuidedDecisionMakingBasic(guidedDecisionMakingBasic);

			CombineAssertions(() =>
			{
				AssertEquals("CustomsValue", 0m, calcData.CustomsValue);
				AssertEquals("ValueForDuty", 0m, calcData.ValueForDuty);
				AssertEquals("UnitOfMeasureValueList: [KGM]", 11m, calcData.UnitOfMeasureValueList.GetValueSafe("KGM"));
				AssertEquals("UnitOfMeasureValueList: [LTR]", 22m, calcData.UnitOfMeasureValueList.GetValueSafe("LTR"));
				AssertEquals("UnitOfMeasureValueList: [HLT]", 33m, calcData.UnitOfMeasureValueList.GetValueSafe("HLT"));
				AssertEquals("CountrySpecificValueList", null, calcData.CountrySpecificValueList);
				AssertEquals("AdditionalInformationList", null, calcData.AdditionalInformationList);
				AssertNull("MeursingExpressionList", calcData.MeursingExpressionList);
			});
		}
	}
}
