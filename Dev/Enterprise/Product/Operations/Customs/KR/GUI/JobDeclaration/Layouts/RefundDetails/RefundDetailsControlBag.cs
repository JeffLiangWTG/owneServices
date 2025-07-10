using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundDetailsControlBag : ControlBag
	{
		public static RefundDetailsControlBag Instance => instance ?? (instance = new RefundDetailsControlBag(BindingContext.Declaration5UL));
		public static RefundDetailsControlBag InstanceForMessageSending => instanceForMessageSending ?? (instanceForMessageSending = new RefundDetailsControlBag(BindingContext.MessageSending));

		protected override Control CreateTemplate()
		{
			var result = new RefundDetailsUserControl();
			if (bindingContext == BindingContext.MessageSending)
			{
				result.BindToMessageSending();
			}
			return result;
		}

		RefundDetailsControlBag(BindingContext bindingContext)
		{
			this.bindingContext = bindingContext;

			PaidLabel = RegisterControl(nameof(RefundDetailsUserControl.PaidLabel));
			RefundLabel = RegisterControl(nameof(RefundDetailsUserControl.RefundLabel));
			PenaltyLabel = RegisterControl(nameof(RefundDetailsUserControl.PenaltyLabel));

			TotalTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.TotalTaxLabel));
			TotalPaidAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.TotalPaidAmountCalcEdit));
			TotalRefundAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.TotalRefundAmountCalcEdit));

			DutyAmountLabel = RegisterControl(nameof(RefundDetailsUserControl.DutyAmountLabel));
			PaidDutyAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidDutyAmountCalcEdit));
			RefundDutyAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundDutyAmountCalcEdit));
			PenaltyToRefundDutyAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundDutyAmountCalcEdit));

			LiquorTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.LiquorTaxLabel));
			PaidLiquorTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidLiquorTaxAmountCalcEdit));
			RefundLiquorTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundLiquorTaxAmountCalcEdit));
			PenaltyToRefundLiquorTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundLiquorTaxAmountCalcEdit));

			SpecialConsumptionTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.SpecialConsumptionTaxLabel));
			PaidSpecialConsumptionTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidSpecialConsumptionTaxAmountCalcEdit));
			RefundSpecialConsumptionTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundSpecialConsumptionTaxAmountCalcEdit));
			PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit));

			TransportTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.TransportTaxLabel));
			PaidTransportTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidTransportTaxAmountCalcEdit));
			RefundTransportTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundTransportTaxAmountCalcEdit));
			PenaltyToRefundTransportTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundTransportTaxAmountCalcEdit));

			EducationTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.EducationTaxLabel));
			PaidEducationTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidEducationTaxAmountCalcEdit));
			RefundEducationTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundEducationTaxAmountCalcEdit));
			PenaltyToRefundEducationTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundEducationTaxAmountCalcEdit));

			AgricultureTaxLabel = RegisterControl(nameof(RefundDetailsUserControl.AgricultureTaxLabel));
			PaidAgricultureTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidAgricultureTaxAmountCalcEdit));
			RefundAgricultureTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundAgricultureTaxAmountCalcEdit));
			PenaltyToRefundAgricultureTaxAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundAgricultureTaxAmountCalcEdit));

			VATLabel = RegisterControl(nameof(RefundDetailsUserControl.VATLabel));
			PaidVATAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidVATAmountCalcEdit));
			RefundVATAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.RefundVATAmountCalcEdit));
			PenaltyToRefundVATAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyToRefundVATAmountCalcEdit));

			TotalPenaltyLabel = RegisterControl(nameof(RefundDetailsUserControl.TotalPenaltyLabel));
			PaidTotalPenaltyCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidTotalPenaltyCalcEdit));
			TotalPenaltyToRefundCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.TotalPenaltyToRefundCalcEdit));

			LatePaymentPenaltyLabel = RegisterControl(nameof(RefundDetailsUserControl.LatePaymentPenaltyLabel));
			PaidLatePaymentPenaltyCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PaidLatePaymentPenaltyCalcEdit));
			TotalLateRefundAmountCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.TotalLateRefundAmountCalcEdit));

			Refund2Label = RegisterControl(nameof(RefundDetailsUserControl.Refund2Label));

			ValueForVATLabel = RegisterControl(nameof(RefundDetailsUserControl.ValueForVATLabel));
			ValueForVATCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.ValueForVATCalcEdit));

			VATExemptionLabel = RegisterControl(nameof(RefundDetailsUserControl.VATExemptionLabel));
			VATExemptionCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.VATExemptionCalcEdit));

			PenaltyForLateDeclarationLabel = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForLateDeclarationLabel));
			PenaltyForLateDeclarationCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForLateDeclarationCalcEdit));

			PenaltyForMissedDeclarationLabel = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForMissedDeclarationLabel));
			PenaltyForMissedDeclarationCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForMissedDeclarationCalcEdit));

			PenaltyForLatePaymentLabel = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForLatePaymentLabel));
			PenaltyForLatePaymentCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.PenaltyForLatePaymentCalcEdit));

			NonDutyTaxRevenueLabel = RegisterControl(nameof(RefundDetailsUserControl.NonDutyTaxRevenueLabel));
			NonDutyTaxRevenueCalcEdit = RegisterControl(nameof(RefundDetailsUserControl.NonDutyTaxRevenueCalcEdit));

			EmptyLabel = RegisterControl(nameof(RefundDetailsUserControl.EmptyLabel));
			Empty2Label = RegisterControl(nameof(RefundDetailsUserControl.Empty2Label));
			Empty3Label = RegisterControl(nameof(RefundDetailsUserControl.Empty3Label));
		}

		#region ControlReference
		readonly BindingContext bindingContext;
		[ThreadStatic]
		static RefundDetailsControlBag instance;
		[ThreadStatic]
		static RefundDetailsControlBag instanceForMessageSending;

		public ControlReference PaidLabel { get; }
		public ControlReference RefundLabel { get; }
		public ControlReference PenaltyLabel { get; }
		public ControlReference TotalTaxLabel { get; }
		public ControlReference TotalPaidAmountCalcEdit { get; }
		public ControlReference TotalRefundAmountCalcEdit { get; }
		public ControlReference DutyAmountLabel { get; }
		public ControlReference PaidDutyAmountCalcEdit { get; }
		public ControlReference RefundDutyAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundDutyAmountCalcEdit { get; }
		public ControlReference LiquorTaxLabel { get; }
		public ControlReference PaidLiquorTaxAmountCalcEdit { get; }
		public ControlReference RefundLiquorTaxAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundLiquorTaxAmountCalcEdit { get; }
		public ControlReference SpecialConsumptionTaxLabel { get; }
		public ControlReference PaidSpecialConsumptionTaxAmountCalcEdit { get; }
		public ControlReference RefundSpecialConsumptionTaxAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit { get; }
		public ControlReference TransportTaxLabel { get; }
		public ControlReference PaidTransportTaxAmountCalcEdit { get; }
		public ControlReference RefundTransportTaxAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundTransportTaxAmountCalcEdit { get; }
		public ControlReference EducationTaxLabel { get; }
		public ControlReference PaidEducationTaxAmountCalcEdit { get; }
		public ControlReference RefundEducationTaxAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundEducationTaxAmountCalcEdit { get; }
		public ControlReference AgricultureTaxLabel { get; }
		public ControlReference PaidAgricultureTaxAmountCalcEdit { get; }
		public ControlReference RefundAgricultureTaxAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundAgricultureTaxAmountCalcEdit { get; }
		public ControlReference VATLabel { get; }
		public ControlReference PaidVATAmountCalcEdit { get; }
		public ControlReference RefundVATAmountCalcEdit { get; }
		public ControlReference PenaltyToRefundVATAmountCalcEdit { get; }
		public ControlReference TotalPenaltyLabel { get; }
		public ControlReference PaidTotalPenaltyCalcEdit { get; }
		public ControlReference TotalPenaltyToRefundCalcEdit { get; }
		public ControlReference LatePaymentPenaltyLabel { get; }
		public ControlReference PaidLatePaymentPenaltyCalcEdit { get; }
		public ControlReference TotalLateRefundAmountCalcEdit { get; }
		public ControlReference Refund2Label { get; }
		public ControlReference ValueForVATLabel { get; }
		public ControlReference ValueForVATCalcEdit { get; }
		public ControlReference VATExemptionLabel { get; }
		public ControlReference VATExemptionCalcEdit { get; }
		public ControlReference PenaltyForLateDeclarationLabel { get; }
		public ControlReference PenaltyForLateDeclarationCalcEdit { get; }
		public ControlReference PenaltyForMissedDeclarationLabel { get; }
		public ControlReference PenaltyForMissedDeclarationCalcEdit { get; }
		public ControlReference PenaltyForLatePaymentLabel { get; }
		public ControlReference PenaltyForLatePaymentCalcEdit { get; }
		public ControlReference NonDutyTaxRevenueLabel { get; }
		public ControlReference NonDutyTaxRevenueCalcEdit  { get; }
		public ControlReference EmptyLabel  { get; }
		public ControlReference Empty2Label { get; }
		public ControlReference Empty3Label { get; }
		#endregion
	}
}
