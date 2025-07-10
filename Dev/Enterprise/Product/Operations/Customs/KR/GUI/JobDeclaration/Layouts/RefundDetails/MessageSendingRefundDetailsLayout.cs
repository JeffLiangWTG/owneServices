using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MessageSendingRefundDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public MessageSendingRefundDetailsLayout()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var layout = new PanelLayout();
			var bag = RefundDetailsControlBag.InstanceForMessageSending;
			layout.RegisterControlBag(bag);

			var ruler1 = layout.CreateRuler(10);
			var ruler2 = layout.CreateRuler(190);
			var ruler3 = layout.CreateRuler(320);

			layout.Include(0, ruler2, bag.PaidLabel, ruler3, bag.RefundLabel);
			layout.Include(0, ruler1, bag.TotalTaxLabel, ruler2, bag.TotalPaidAmountCalcEdit, ruler3, bag.TotalRefundAmountCalcEdit);
			layout.Include(0, ruler1, bag.DutyAmountLabel, ruler2, bag.PaidDutyAmountCalcEdit, ruler3, bag.RefundDutyAmountCalcEdit);
			layout.Include(0, ruler1, bag.LiquorTaxLabel, ruler2, bag.PaidLiquorTaxAmountCalcEdit, ruler3, bag.RefundLiquorTaxAmountCalcEdit);
			layout.Include(0, ruler1, bag.SpecialConsumptionTaxLabel, ruler2, bag.PaidSpecialConsumptionTaxAmountCalcEdit, ruler3, bag.RefundSpecialConsumptionTaxAmountCalcEdit);
			layout.Include(0, ruler1, bag.TransportTaxLabel, ruler2, bag.PaidTransportTaxAmountCalcEdit, ruler3, bag.RefundTransportTaxAmountCalcEdit);
			layout.Include(0, ruler1, bag.EducationTaxLabel, ruler2, bag.PaidEducationTaxAmountCalcEdit, ruler3, bag.RefundEducationTaxAmountCalcEdit);
			layout.Include(0, ruler1, bag.AgricultureTaxLabel, ruler2, bag.PaidAgricultureTaxAmountCalcEdit, ruler3, bag.RefundAgricultureTaxAmountCalcEdit);
			layout.Include(0, ruler1, bag.VATLabel, ruler2, bag.PaidVATAmountCalcEdit, ruler3, bag.RefundVATAmountCalcEdit);
			layout.Include(0, ruler1, bag.TotalPenaltyLabel, ruler2, bag.PaidTotalPenaltyCalcEdit);
			layout.Include(0, ruler1, bag.LatePaymentPenaltyLabel, ruler2, bag.PaidLatePaymentPenaltyCalcEdit, ruler3, bag.TotalLateRefundAmountCalcEdit);

			layout.AddColumn();

			layout.Include(1, ruler2, bag.Refund2Label);
			layout.Include(1, ruler1, bag.PenaltyForLateDeclarationLabel, ruler2, bag.PenaltyForLateDeclarationCalcEdit);
			layout.Include(1, ruler1, bag.PenaltyForMissedDeclarationLabel, ruler2, bag.PenaltyForMissedDeclarationCalcEdit);
			layout.Include(1, ruler1, bag.PenaltyForLatePaymentLabel, ruler2, bag.PenaltyForLatePaymentCalcEdit);
			layout.Include(1, ruler1, bag.NonDutyTaxRevenueLabel, ruler2, bag.NonDutyTaxRevenueCalcEdit);

			return layout;
		}
	}
}
