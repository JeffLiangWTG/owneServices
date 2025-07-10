using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MethodFiveToSixControlBag))]
	sealed class MethodFiveToSixControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MethodFiveToSixControlBag.AmountAgreedUponWithCustomsKRWCalcEdit);
				yield return nameof(MethodFiveToSixControlBag.AdditionalCostFreightToArrivalPortCalcEdit);
				yield return nameof(MethodFiveToSixControlBag.AdditionalCostFreightToDeparturePortCalcEdit);
				yield return nameof(MethodFiveToSixControlBag.AdditionalCostInsuranceCalcEdit);
				yield return nameof(MethodFiveToSixControlBag.AdditionalCostTotalAdditionalAmountCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MethodFiveToSixControlBag.InstanceForDeclaration;
	}
}
