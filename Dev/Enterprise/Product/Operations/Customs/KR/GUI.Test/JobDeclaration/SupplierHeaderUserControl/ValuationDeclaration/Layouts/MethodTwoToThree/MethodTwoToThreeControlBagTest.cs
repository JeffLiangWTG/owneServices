using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodTwoToThreeControlBag))]
	sealed class MethodTwoToThreeControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MethodTwoToThreeControlBag.ReplacementAmountCalcFindBox);
				yield return nameof(MethodTwoToThreeControlBag.ReplacementExchangeRateCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.ReplacementAmountKRWCalcEdit);

				yield return nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentQuantityDiscountCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentCommercialAmountCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentTransportationCostCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentShippingPortCostCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.AdditionalAdjustmentInsuranceCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.TotalAdditionalAdjustmentAmountCalcEdit);

				yield return nameof(MethodTwoToThreeControlBag.DeductionAdjustmentQuantityDiscountCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.DeductionAdjustmentCommercialAmountCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.DeductionAdjustmentTransportationCostCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.DeductionAdjustmentShippingPortCostCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.DeductionAdjustmentInsuranceCalcEdit);
				yield return nameof(MethodTwoToThreeControlBag.TotalDeductionAdjustmentAmountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MethodTwoToThreeControlBag.InstanceForDeclaration;
	}
}
