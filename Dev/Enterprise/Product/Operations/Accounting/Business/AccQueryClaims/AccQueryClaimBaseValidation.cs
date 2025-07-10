using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class AccQueryClaimBaseValidation : AccQueryClaimValidation
	{
		public AccQueryClaimBaseValidation(AccQueryClaimBase parent)
			: base(parent)
		{
		}

		protected new AccQueryClaimBase Parent
		{
			get { return base.Parent as AccQueryClaimBase; }
		}

		protected override void CheckAY_OH_DebtorIsNotEmpty()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OH_DebtorIsNotEmpty();
			}
		}

		protected override void CheckAY_OH_DebtorIsValidZGuid()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OH_DebtorIsValidZGuid();
			}
		}

		protected override void CheckAY_AHIsValidZGuid()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_AHIsValidZGuid();
			}
		}

		protected override void CheckAY_OC()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OC();
			}
		}

		protected override void CheckAY_OCIsNotEmpty()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OCIsNotEmpty();
			}
		}

		protected override void CheckAY_OCIsValidZGuid()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OCIsValidZGuid();
			}
		}

		protected override void CheckAY_QueryClaimAmountIsValidMoney()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimAmountIsValidMoney();
			}
		}

		protected override void CheckAY_ShortDescriptionOfClaim()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_ShortDescriptionOfClaim();
			}
		}

		protected override void CheckAY_QueryClaimType()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimType();
			}
		}

		protected override void CheckAY_QueryClaimTypeIsWesternEuropean()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimTypeIsWesternEuropean();
			}
		}

		protected override void CheckAY_QueryClaimStatus()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimStatus();
				if (Parent.AY_QueryClaimStatus != (ZString)Parent.AY_QueryClaimStatusInfo.OriginalValue)
				{
					if (Parent.IsClosedByOriginalValue)
					{
						Parent.AY_QueryClaimStatusInfo.AddError(Res.GetString("28a24ba1-2dd5-4777-8ef5-73a170d57ffd", "Claim is closed and cannot be reopened."));
					}
				}
				if (Parent.IsClosed && Parent.RelatedUnapprovedCreditNote != null)
				{
					Parent.AY_QueryClaimStatusInfo.AddError(Res.GetString("81de8628-806f-40c2-a463-2bf97e7048d2", "Claim can't be closed because it has claim charges."));
				}
			}
		}

		protected override void CheckAY_QueryClaimStatusIsWesternEuropean()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimStatusIsWesternEuropean();
			}
		}

		protected override void CheckAY_QueryClaimNextFollowUp()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimNextFollowUp();
			}
		}

		protected override void CheckAY_QueryClaimNextFollowUpIsValidZDateTime()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimNextFollowUpIsValidZDateTime();
			}
		}

		protected override void CheckAY_QueryClaimNextFollowUpIsValidZDateTimeRange()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimNextFollowUpIsValidZDateTimeRange();
			}
		}

		protected override void CheckAY_AH()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_AH();
			}
		}

		protected override void CheckAY_OH_Debtor()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_OH_Debtor();
			}
		}

		protected override void CheckAY_QueryClaimAmount()
		{
			if (!Parent.IsIntercompanyClaim)
			{
				base.CheckAY_QueryClaimAmount();
			}
		}
	}
}

