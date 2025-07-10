using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business
{
	public class ChargePaymentBasisComparer : IEqualityComparer<BaseCharge>
	{
		public ChargePaymentBasisComparer(CostSell costSell, ChargeComparison minimumComparisionLevel = ChargeComparison.Same)
		{
			this.costSell = costSell;
			this.minimumComparisionLevel = minimumComparisionLevel;
		}

		readonly CostSell costSell;
		readonly ChargeComparison minimumComparisionLevel;

		bool IEqualityComparer<BaseCharge>.Equals(BaseCharge x, BaseCharge y)
		{
			if (x == null)
			{
				return y == null;
			}

			return y != null
				&& (costSell == CostSell.Cost
					? x.CostPaymentBases.Compare(y.CostPaymentBases) >= minimumComparisionLevel
					: x.SellPaymentBases.Compare(y.SellPaymentBases) >= minimumComparisionLevel);
		}

		int IEqualityComparer<BaseCharge>.GetHashCode(BaseCharge jobPaymentBasis) { return 0; }
	}
}
