using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundRequestControlBag))]
	sealed class RefundRequestControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(RefundRequestControlBag.RefundTypeDropEdit);
				yield return nameof(RefundRequestControlBag.RefundCauseDropEdit);
				yield return nameof(RefundRequestControlBag.RefundReasonDropEdit);
				yield return nameof(RefundRequestControlBag.RefundSentWith5FEDropEdit);
				yield return nameof(RefundRequestControlBag.TaxOfficeCodeFindBox);
				yield return nameof(RefundRequestControlBag.CustomsDisbursementBillTextBox);
				yield return nameof(RefundRequestControlBag.VersionNoCalcEdit);
				yield return nameof(RefundRequestControlBag.RefundAmountOfValueForVATCalcEdit);
				yield return nameof(RefundRequestControlBag.RefundAmountOfVATExemptionValueCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => RefundRequestControlBag.Instance;
	}
}
