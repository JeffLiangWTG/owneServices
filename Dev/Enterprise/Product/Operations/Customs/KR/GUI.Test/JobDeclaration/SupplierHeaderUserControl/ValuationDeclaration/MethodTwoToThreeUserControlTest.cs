using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MethodTwoToThreeUserControlTest : TestCaseWithFactory
	{
		public void TestMethodTwoToThreeUserControl()
		{
			using (var control = new MethodTwoToThreeUserControl())
			{
				var replacementGroupBox = control.FindSingle<ZGroupBox>("ReplacementGroupBox");
				AssertEquals(true, replacementGroupBox.Visible);
				var replacementDynamicLayoutPanel = replacementGroupBox.FindSingle<DynamicLayoutPanel>("ReplacementDynamicLayoutPanel");
				AssertEquals(true, replacementDynamicLayoutPanel.FindSingle<ZCalcFindBox>("ReplacementAmountCalcFindBox").Visible);
				AssertEquals(true, replacementDynamicLayoutPanel.FindSingle<ZCalcEdit>("ReplacementExchangeRateCalcEdit").Visible);
				AssertEquals(true, replacementDynamicLayoutPanel.FindSingle<ZCalcEdit>("ReplacementAmountKRWCalcEdit").Visible);

				var additionalAdjustmentGroupBox = control.FindSingle<ZGroupBox>("AdditionalAdjustmentGroupBox");
				AssertEquals(true, additionalAdjustmentGroupBox.Visible);
				var additionalAdjustmentDynamicLayoutPanel = additionalAdjustmentGroupBox.FindSingle<DynamicLayoutPanel>("AdditionalAdjustmentDynamicLayoutPanel");
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("AdditionalAdjustmentQuantityDiscountCalcEdit").Visible);
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("AdditionalAdjustmentCommercialAmountCalcEdit").Visible);
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("AdditionalAdjustmentTransportationCostCalcEdit").Visible);
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("AdditionalAdjustmentShippingPortCostCalcEdit").Visible);
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("AdditionalAdjustmentInsuranceCalcEdit").Visible);
				AssertEquals(true, additionalAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("TotalAdditionalAdjustmentAmountCalcEdit").Visible);

				var deductionAdjustmentGroupBox = control.FindSingle<ZGroupBox>("DeductionAdjustmentGroupBox");
				AssertEquals(true, deductionAdjustmentGroupBox.Visible);
				var deductionAdjustmentDynamicLayoutPanel = deductionAdjustmentGroupBox.FindSingle<DynamicLayoutPanel>("DeductionAdjustmentDynamicLayoutPanel");
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("DeductionAdjustmentQuantityDiscountCalcEdit").Visible);
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("DeductionAdjustmentCommercialAmountCalcEdit").Visible);
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("DeductionAdjustmentTransportationCostCalcEdit").Visible);
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("DeductionAdjustmentShippingPortCostCalcEdit").Visible);
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("DeductionAdjustmentInsuranceCalcEdit").Visible);
				AssertEquals(true, deductionAdjustmentDynamicLayoutPanel.FindSingle<ZCalcEdit>("TotalDeductionAdjustmentAmountCalcEdit").Visible);
			}
		}
	}
}
