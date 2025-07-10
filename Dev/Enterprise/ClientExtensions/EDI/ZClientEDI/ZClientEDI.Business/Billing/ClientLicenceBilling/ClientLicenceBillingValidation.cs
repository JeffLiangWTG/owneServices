using System;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingValidation : AutoClientLicenceBillingValidation
	{
		public ClientLicenceBillingValidation(AutoClientLicenceBilling parent)
			: base(parent)
		{
		}

		public new ClientLicenceBilling Parent
		{
			get { return (ClientLicenceBilling)base.Parent; }
		}

		protected override void CheckL4_ProcessingFee()
		{
			base.CheckL4_ProcessingFee();
			if (!Parent.L4_ProcessingFeeInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.L4_ProcessingFeeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.L4_ProcessingFeeInfo);
		}

		protected override void CheckL4_ProcessingFeePercent()
		{
			base.CheckL4_ProcessingFeePercent();

			if (Math.Abs(Parent.L4_ProcessingFeePercent) > 100.0m)
			{
				Parent.L4_ProcessingFeePercentInfo.AddError("Discounts and Fees cannot be bigger than 100.00%");
			}

			if (Parent.L4_ProcessingFeePercent < 0.0m)
			{
				Parent.L4_ProcessingFeePercentInfo.AddError("Only positive values allowed");
			}
		}

		protected void CheckPartnerEmail()
		{
			if (Parent.L4_IsPartner)
			{
				MandatoryValidation.CheckEntered(Parent.PartnerEmailInfo);
			}
		}

		public void ValidatePartnerEmail()
		{
			ValidateCalculatedProperty(Parent.PartnerEmailInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidatePartnerEmail();
			}
		}

		protected override void CheckL4_PredeterminedPrepaidBalance()
		{
			base.CheckL4_PredeterminedPrepaidBalance();
			MandatoryValidation.CheckNotNegative(Parent.L4_PredeterminedPrepaidBalanceInfo);
		}

		protected override void CheckL4_RX_NKPredeterminedPrepaidBalanceCurrency()
		{
			base.CheckL4_RX_NKPredeterminedPrepaidBalanceCurrency();

			if (Parent.L4_PredeterminedPrepaidBalance != 0m)
			{
				MandatoryValidation.CheckEntered(Parent.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);
			}
			else
			{
				if (!Parent.L4_RX_NKPredeterminedPrepaidBalanceCurrency.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);
				}
			}
		}

		protected override void CheckL4_FuturePredeterminedPrepaidBalance()
		{
			base.CheckL4_FuturePredeterminedPrepaidBalance();
			MandatoryValidation.CheckNotNegative(Parent.L4_FuturePredeterminedPrepaidBalanceInfo);
		}

		protected override void CheckL4_RX_NKFuturePredeterminedPrepaidBalanceCurrency()
		{
			base.CheckL4_RX_NKFuturePredeterminedPrepaidBalanceCurrency();

			if (Parent.L4_FuturePredeterminedPrepaidBalance != 0m)
			{
				MandatoryValidation.CheckEntered(Parent.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);
			}
			else
			{
				if (!Parent.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);
				}
			}
		}
	}
}

