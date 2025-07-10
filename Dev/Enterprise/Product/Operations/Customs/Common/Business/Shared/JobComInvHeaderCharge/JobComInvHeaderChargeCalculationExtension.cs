using System.Collections.Generic;

namespace Enterprise.Customs.Common
{
	public static class BaseJobComInvHeaderChargeCalculationExtension
	{
		public static bool HasSameChargeWithDifferentAdjustedFlag(this JobComInvCharge givenCharge)
		{
			return GetSameChargeWithDifferentAdjustedFlag(givenCharge).Length > 0;
		}

		public static JobComInvCharge[] GetSameChargeWithDifferentAdjustedFlag(this JobComInvCharge givenCharge)
		{
			IChargeApportionee apportioneeParent = givenCharge.Parent as IChargeApportionee;

			List<JobComInvCharge> result = new List<JobComInvCharge>();
			//JobComInvoiceHeader & JobComInvoiceLine
			if (apportioneeParent != null)
			{
				foreach (JobComInvCharge charge in apportioneeParent.Charges)
				{
					if (IsAdjustingCharge(charge, givenCharge))
					{
						result.Add(charge);
					}
				}

				foreach (JobComInvCharge charge in apportioneeParent.ApportionedCharges)
				{
					if (IsAdjustingCharge(charge, givenCharge))
					{
						result.Add(charge);
					}
				}
			}

			return result.ToArray();
		}

		static bool IsAdjustingCharge(JobComInvCharge xCharge, JobComInvCharge yCharge)
		{
			return xCharge != yCharge &&
					xCharge.J7_ChargeType == yCharge.J7_ChargeType &&
					xCharge.J7_IsIncludedInITOT == yCharge.J7_IsIncludedInITOT &&
					xCharge.J7_IsNotIncludedInInvoice == yCharge.J7_IsNotIncludedInInvoice &&
					xCharge.J7_AdjustedCharge == !yCharge.J7_AdjustedCharge;
		}
	}
}
