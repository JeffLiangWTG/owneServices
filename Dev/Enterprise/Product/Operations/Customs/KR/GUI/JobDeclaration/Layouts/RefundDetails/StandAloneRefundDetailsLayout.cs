using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class StandAloneRefundDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public StandAloneRefundDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var bag = RefundDetailsControlBag.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(0);
			var ruler2 = layout.CreateRuler(180);
			var ruler3 = layout.CreateRuler(10);
			var ruler4 = layout.CreateRuler(130);

			layout.Include(0, ruler1, bag.EmptyLabel);
			layout.Include(0, ruler1, bag.TotalTaxLabel);
			layout.Include(0, ruler1, bag.DutyAmountLabel);
			layout.Include(0, ruler1, bag.LiquorTaxLabel);
			layout.Include(0, ruler1, bag.SpecialConsumptionTaxLabel);
			layout.Include(0, ruler1, bag.TransportTaxLabel);
			layout.Include(0, ruler1, bag.EducationTaxLabel);
			layout.Include(0, ruler1, bag.AgricultureTaxLabel);
			layout.Include(0, ruler1, bag.VATLabel);
			layout.Include(0, ruler1, bag.TotalPenaltyLabel);
			layout.Include(0, ruler1, bag.LatePaymentPenaltyLabel);

			layout.AddColumn();
			layout.Include(1, ruler1, bag.PaidLabel);
			layout.Include(1, ruler1, bag.TotalPaidAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidDutyAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidLiquorTaxAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidSpecialConsumptionTaxAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidTransportTaxAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidEducationTaxAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidAgricultureTaxAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidVATAmountCalcEdit);
			layout.Include(1, ruler1, bag.PaidTotalPenaltyCalcEdit);
			layout.Include(1, ruler1, bag.PaidLatePaymentPenaltyCalcEdit);

			layout.AddColumn();
			layout.Include(2, ruler3, bag.RefundLabel);
			layout.Include(2, ruler3, bag.TotalRefundAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundDutyAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundLiquorTaxAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundSpecialConsumptionTaxAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundTransportTaxAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundEducationTaxAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundAgricultureTaxAmountCalcEdit);
			layout.Include(2, ruler3, bag.RefundVATAmountCalcEdit);
			layout.Include(2, ruler3, bag.Empty2Label);
			layout.Include(2, ruler3, bag.TotalLateRefundAmountCalcEdit);
			
			layout.AddColumn();
			layout.Include(3, ruler3, bag.PenaltyLabel);
			layout.Include(3, ruler3, bag.Empty3Label);
			layout.Include(3, ruler3, bag.PenaltyToRefundDutyAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundLiquorTaxAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundTransportTaxAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundEducationTaxAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundAgricultureTaxAmountCalcEdit);
			layout.Include(3, ruler3, bag.PenaltyToRefundVATAmountCalcEdit);
			layout.Include(3, ruler3, bag.TotalPenaltyToRefundCalcEdit);

			layout.AddColumn();
			layout.Include(4, ruler2, bag.Refund2Label);
			layout.Include(4, ruler1, bag.ValueForVATLabel, ruler2, bag.ValueForVATCalcEdit);
			layout.Include(4, ruler1, bag.VATExemptionLabel, ruler2, bag.VATExemptionCalcEdit);
			layout.Include(4, ruler1, bag.PenaltyForLateDeclarationLabel, ruler2, bag.PenaltyForLateDeclarationCalcEdit);
			layout.Include(4, ruler1, bag.PenaltyForMissedDeclarationLabel, ruler2, bag.PenaltyForMissedDeclarationCalcEdit);
			layout.Include(4, ruler1, bag.PenaltyForLatePaymentLabel, ruler2, bag.PenaltyForLatePaymentCalcEdit);
			layout.Include(4, ruler1, bag.NonDutyTaxRevenueLabel, ruler2, bag.NonDutyTaxRevenueCalcEdit);
			return layout;
		}
	}
}
