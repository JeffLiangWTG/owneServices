using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodFourControlBag))]
	sealed class MethodFourControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MethodFourControlBag.SalesOfHighestQuantityAmountCalcFindBox);
				yield return nameof(MethodFourControlBag.SalesOfHighestQuantityExchangeRateCalcEdit);
				yield return nameof(MethodFourControlBag.SalesOfHighestQuantityAmountKRWCalcEdit);

				yield return nameof(MethodFourControlBag.DeductionCostCustomsReferenceNumberTextBox);
				yield return nameof(MethodFourControlBag.DeductionCostConsignmentSalesFeeCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostGeneralCostCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostCostRateCodeDropEdit);
				yield return nameof(MethodFourControlBag.DeductionCostCostRateCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostTransportationCostCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCosInsuranceCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostUnloadCostCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostOtherTransportationCostsCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostAdditionalCostCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCosTaxCalcEdit);
				yield return nameof(MethodFourControlBag.DeductionCostTotalDeductionAmountCalcEdit);

				yield return nameof(MethodFourControlBag.PercentageLabel);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MethodFourControlBag.InstanceForDeclaration;
	}
}
