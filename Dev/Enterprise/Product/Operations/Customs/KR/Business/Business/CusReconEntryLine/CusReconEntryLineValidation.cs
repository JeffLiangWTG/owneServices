using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLineValidation : Customs.Business.CusReconEntryLineValidation
	{
		public CusReconEntryLineValidation(CusReconEntryLine parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDutyToRefund();
			ValidateDutyPenaltyToRefund();
			ValidateSCTToRefund();
			ValidateSCTPenaltyToRefund();
			ValidateTRTToRefund();
			ValidateTRTPenaltyToRefund();
			ValidateLQTToRefund();
			ValidateLQTPenaltyToRefund();
			ValidateEDTToRefund();
			ValidateEDTPenaltyToRefund();
			ValidateAGTToRefund();
			ValidateAGTPenaltyToRefund();
			ValidateVATToRefund();
			ValidateVATPenaltyToRefund();
			ValidatePenaltyLateDecToRefund();
			ValidatePenaltyMissedDecToRefund();
			ValidatePenaltyLatePaymentToRefund();
		}

		public void ValidateDutyToRefund()
		{
			ValidateCalculatedProperty(Parent.DutyToRefundInfo);
		}
		protected void CheckDutyToRefund()
		{
			CheckNegativeAmount(Parent.DutyToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.DutyToRefundInfo, Parent.PaidDutyAmount);
		}

		public void ValidateDutyPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.DutyPenaltyToRefundInfo);
		}
		protected void CheckDutyPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.DutyPenaltyToRefundInfo);
		}

		public void ValidateLQTToRefund()
		{
			ValidateCalculatedProperty(Parent.LQTToRefundInfo);
		}

		protected void CheckLQTToRefund()
		{
			CheckNegativeAmount(Parent.LQTToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.LQTToRefundInfo, Parent.PaidLiquorTaxAmount);
		}
		public void ValidateLQTPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.LQTPenaltyToRefundInfo);
		}

		protected void CheckLQTPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.LQTPenaltyToRefundInfo);
		}

		public void ValidateSCTToRefund()
		{
			ValidateCalculatedProperty(Parent.SCTToRefundInfo);
		}

		protected void CheckSCTToRefund()
		{
			CheckNegativeAmount(Parent.SCTToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.SCTToRefundInfo, Parent.PaidSpecialConsumptionTaxAmount);
		}

		public void ValidateSCTPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.SCTPenaltyToRefundInfo);
		}

		protected void CheckSCTPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.SCTPenaltyToRefundInfo);
		}

		public void ValidateTRTToRefund()
		{
			ValidateCalculatedProperty(Parent.TRTToRefundInfo);
		}

		protected void CheckTRTToRefund()
		{
			CheckNegativeAmount(Parent.TRTToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.TRTToRefundInfo, Parent.PaidTransportTaxAmount);
		}

		public void ValidateTRTPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.TRTPenaltyToRefundInfo);
		}

		protected void CheckTRTPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.TRTPenaltyToRefundInfo);
		}

		public void ValidateEDTToRefund()
		{
			ValidateCalculatedProperty(Parent.EDTToRefundInfo);
		}

		protected void CheckEDTToRefund()
		{
			CheckNegativeAmount(Parent.EDTToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.EDTToRefundInfo, Parent.PaidEducationTaxAmount);
		}
		public void ValidateEDTPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.EDTPenaltyToRefundInfo);
		}

		protected void CheckEDTPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.EDTPenaltyToRefundInfo);
		}
		public void ValidateAGTToRefund()
		{
			ValidateCalculatedProperty(Parent.AGTToRefundInfo);
		}

		protected void CheckAGTToRefund()
		{
			CheckNegativeAmount(Parent.AGTToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.AGTToRefundInfo, Parent.PaidAgricultureTaxAmount);
		}
		public void ValidateAGTPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.AGTPenaltyToRefundInfo);
		}

		protected void CheckAGTPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.AGTPenaltyToRefundInfo);
		}
		public void ValidateVATToRefund()
		{
			ValidateCalculatedProperty(Parent.VATToRefundInfo);
		}

		protected void CheckVATToRefund()
		{
			CheckNegativeAmount(Parent.VATToRefundInfo);
			CheckPaidAndRefundAmounts(Parent.VATToRefundInfo, Parent.PaidVATAmount);
		}
		public void ValidateVATPenaltyToRefund()
		{
			ValidateCalculatedProperty(Parent.VATPenaltyToRefundInfo);
		}

		protected void CheckVATPenaltyToRefund()
		{
			CheckNegativeAmount(Parent.VATPenaltyToRefundInfo);
		}

		public void ValidatePenaltyLateDecToRefund()
		{
			ValidateCalculatedProperty(Parent.PenaltyLateDecToRefundInfo);
		}

		protected void CheckPenaltyLateDecToRefund()
		{
			CheckNegativeAmount(Parent.PenaltyLateDecToRefundInfo);
			CheckPaidTotalPenalty(Parent.PenaltyLateDecToRefundInfo);
		}
		public void ValidatePenaltyMissedDecToRefund()
		{
			ValidateCalculatedProperty(Parent.PenaltyMissedDecToRefundInfo);
		}

		protected void CheckPenaltyMissedDecToRefund()
		{
			CheckNegativeAmount(Parent.PenaltyMissedDecToRefundInfo);
			CheckPaidTotalPenalty(Parent.PenaltyMissedDecToRefundInfo);
		}
		public void ValidatePenaltyLatePaymentToRefund()
		{
			ValidateCalculatedProperty(Parent.PenaltyLatePaymentToRefundInfo);
		}

		protected void CheckPenaltyLatePaymentToRefund()
		{
			CheckNegativeAmount(Parent.PenaltyLatePaymentToRefundInfo);
			CheckPaidTotalPenalty(Parent.PenaltyLatePaymentToRefundInfo);
		}

		public void ValidateNonDutyTaxRevenueToRefund()
		{
			ValidateCalculatedProperty(Parent.NonDutyTaxRevenueToRefundInfo);
		}

		protected void CheckNonDutyTaxRevenueToRefund()
		{
			CheckNegativeAmount(Parent.NonDutyTaxRevenueToRefundInfo);
			CheckPaidTotalPenalty(Parent.NonDutyTaxRevenueToRefundInfo);
		}

		void CheckNegativeAmount(ZPropertyInfo amountInfo)
		{
			if ((ZDecimal)amountInfo.Value < 0)
			{
				amountInfo.AddMessageError(NegativeAmountMessageError);
			}
		}

		void CheckPaidAndRefundAmounts(ZPropertyInfo refundAmountInfo, ZDecimal paidAmount)
		{
			if ((ZDecimal)refundAmountInfo.Value > paidAmount)
			{
				refundAmountInfo.AddMessageError(LessOrEqualToPaidAmountMessageError);
			}
		}

		void CheckPaidTotalPenalty(ZPropertyInfo refundAmountInfo)
		{
			if (Parent.TotalLateRefundAmount > Parent.PaidTotalPenalty)
			{
				refundAmountInfo.AddMessageError(PaidTotalPenaltyErrorMessage);
			}
		}

		public new CusReconEntryLine Parent => (CusReconEntryLine)base.Parent;

		string LessOrEqualToPaidAmountMessageError => Res.GetString("35CA1D6A-E2E3-4012-8724-8127196F39F5", "The refund amount must be less than or equal to the paid amount.");
		string PaidTotalPenaltyErrorMessage => Res.GetString("75DF3BE7-3AEC-469F-8C05-5E158C1EBA62", "The refund total amount of missed, late declaration and late payment penalties has exceeded the total penalty amount paid. Please check.");
		string NegativeAmountMessageError => Res.GetString("AF98B973-052F-4DC5-8447-1F49100D41D1", "Please enter an amount greater than or equal to 0.");
	}
}
