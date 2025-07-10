using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class APAccQueryClaimValidation : AccQueryClaimBaseValidation
	{
		public APAccQueryClaimValidation(APAccQueryClaim parent)
			: base(parent)
		{
		}

		protected new APAccQueryClaim Parent
		{
			get { return base.Parent as APAccQueryClaim; }
		}

		protected override void CheckAY_AH()
		{
			base.CheckAY_AH();
			if (!Parent.AY_AHInfo.HasErrors())
			{
				if (Parent.RelatedUnapprovedCreditNote != null)
				{
					Parent.AY_AHInfo.AddWarning(Res.GetString("93568d90-1635-403f-8f27-980065f7ba55", "Invoice Number cannot be changed after claim charges are allocated."));
				}
			}
		}

		protected override void CheckAY_OH_Debtor()
		{
			base.CheckAY_OH_Debtor();
			if (!Parent.AY_OH_DebtorInfo.HasErrors())
			{
				if (Parent.RelatedUnapprovedCreditNote != null)
				{
					Parent.AY_OH_DebtorInfo.AddWarning(Res.GetString("76e5bf65-e834-4f57-9e74-fe956afda9ff", "Creditor cannot be changed after claim charges are allocated."));
				}
			}
		}

		protected override void CheckAY_QueryClaimAmount()
		{
			base.CheckAY_QueryClaimAmount();
			APAccQueryClaim apClaim = Parent;
			if (apClaim != null && !apClaim.IsCreatedFromCASS && apClaim.TransactionHeader != null)
			{
				APInvoice invoice = Parent.Factory.Load<APInvoice>(apClaim.TransactionHeader.PK);
				if (invoice != null && apClaim.AY_QueryClaimAmount > invoice.AH_OSTotalAmount)
				{
					apClaim.AY_QueryClaimAmountInfo.AddError(Res.GetString("14e9a3f0-664c-4e0d-8704-56077f20fda8", "Amount Claimed cannot exceed the original invoice’s amount."));
				}

				if (apClaim.RelatedUnapprovedCreditNote != null && apClaim.AY_QueryClaimAmount != apClaim.RelatedUnapprovedCreditNote.AH_OSTotalAmount)
				{
					apClaim.AY_QueryClaimAmountInfo.AddError(Res.GetString("a4b3896a-316c-4194-bc22-47ebe59a976a", @"The Amount Claimed does not match the Unapproved AP Credit Note Totals attached to this claim.
Please amend the Amount Claimed or cancel and re-allocate Claim Charges."));
				}
			}
		}

		protected override void CheckAY_HoldOption()
		{
			base.CheckAY_HoldOption();

			if (Parent.AY_HoldOption != AccountingMasterFilesConstants.CreditorGroupConstants.DefaultHoldOption && !Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed)
			{
				// Later there will be more IF here for new hold option types.
				if (!IsHoldOptionAllowed(HoldOptionType.Codes.ALM))
				{
					Parent.AY_HoldOptionInfo.AddError(ResString.GetMultilingualString("0E9A48A1-7228-4fb7-9BEC-5E911D02C9D1", "Unable to change Invoice Hold Option. Please review the Allowed Invoice Hold Options for the Creditor Group. Otherwise, ask your system administrator to adjust your security right: Manage > Payables > Claims and Queries > Modify Invoice Hold Option."));
				}
			}
		}

		#region Implementation

		bool IsHoldOptionAllowed(ZString holdOption)
		{
			switch (holdOption)
			{
				case HoldOptionType.Codes.ALM:
					return Parent?.Debtor?.CompanyData?.APCreditorGroup?.OG_IsAllowALM ?? false;
				default:
					return true;
			}
		}

		#endregion
	}
}

