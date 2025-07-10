using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(RefundDetailsControlBag))]
	sealed class RefundDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(RefundDetailsControlBag.PaidLabel);
				yield return nameof(RefundDetailsControlBag.RefundLabel);
				yield return nameof(RefundDetailsControlBag.PenaltyLabel);
				yield return nameof(RefundDetailsControlBag.TotalTaxLabel);
				yield return nameof(RefundDetailsControlBag.TotalPaidAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.TotalRefundAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.DutyAmountLabel);
				yield return nameof(RefundDetailsControlBag.PaidDutyAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundDutyAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundDutyAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.LiquorTaxLabel);
				yield return nameof(RefundDetailsControlBag.PaidLiquorTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundLiquorTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundLiquorTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.SpecialConsumptionTaxLabel);
				yield return nameof(RefundDetailsControlBag.PaidSpecialConsumptionTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundSpecialConsumptionTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.TransportTaxLabel);
				yield return nameof(RefundDetailsControlBag.PaidTransportTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundTransportTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundTransportTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.EducationTaxLabel);
				yield return nameof(RefundDetailsControlBag.PaidEducationTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundEducationTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundEducationTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.AgricultureTaxLabel);
				yield return nameof(RefundDetailsControlBag.PaidAgricultureTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundAgricultureTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundAgricultureTaxAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.VATLabel);
				yield return nameof(RefundDetailsControlBag.PaidVATAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.RefundVATAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyToRefundVATAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.TotalPenaltyLabel);
				yield return nameof(RefundDetailsControlBag.PaidTotalPenaltyCalcEdit);
				yield return nameof(RefundDetailsControlBag.TotalPenaltyToRefundCalcEdit);
				yield return nameof(RefundDetailsControlBag.LatePaymentPenaltyLabel);
				yield return nameof(RefundDetailsControlBag.PaidLatePaymentPenaltyCalcEdit);
				yield return nameof(RefundDetailsControlBag.TotalLateRefundAmountCalcEdit);
				yield return nameof(RefundDetailsControlBag.Refund2Label);
				yield return nameof(RefundDetailsControlBag.ValueForVATLabel);
				yield return nameof(RefundDetailsControlBag.ValueForVATCalcEdit);
				yield return nameof(RefundDetailsControlBag.VATExemptionLabel);
				yield return nameof(RefundDetailsControlBag.VATExemptionCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyForLateDeclarationLabel);
				yield return nameof(RefundDetailsControlBag.PenaltyForLateDeclarationCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationLabel);
				yield return nameof(RefundDetailsControlBag.PenaltyForMissedDeclarationCalcEdit);
				yield return nameof(RefundDetailsControlBag.PenaltyForLatePaymentLabel);
				yield return nameof(RefundDetailsControlBag.PenaltyForLatePaymentCalcEdit);
				yield return nameof(RefundDetailsControlBag.NonDutyTaxRevenueLabel);
				yield return nameof(RefundDetailsControlBag.NonDutyTaxRevenueCalcEdit);
				yield return nameof(RefundDetailsControlBag.EmptyLabel);
				yield return nameof(RefundDetailsControlBag.Empty2Label);
				yield return nameof(RefundDetailsControlBag.Empty3Label);
			}
		}

		protected override ControlBag GetControlBagForTesting() => RefundDetailsControlBag.Instance;
	}
}
