using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(MiscOptionsControlBag))]
	sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(MiscOptionsControlBag.ReturnReasonDropEdit);
				yield return nameof(MiscOptionsControlBag.ReturnTypeDropEdit);
				yield return nameof(MiscOptionsControlBag.SouthNorthTradeDropEdit);
				yield return nameof(MiscOptionsControlBag.SouthNorthTradeAreaDropEdit);
				yield return nameof(MiscOptionsControlBag.BondedTransportationPeriodUserControl);
				yield return nameof(MiscOptionsControlBag.UCRTextBox);
				yield return nameof(MiscOptionsControlBag.LateDecPenaltyDateCodeDropEdit);
				yield return nameof(MiscOptionsControlBag.MissedDecPenaltyRateCalcEdit);
				yield return nameof(MiscOptionsControlBag.PercentageLabel);
				yield return nameof(MiscOptionsControlBag.TaxOfficeCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
	}
}
