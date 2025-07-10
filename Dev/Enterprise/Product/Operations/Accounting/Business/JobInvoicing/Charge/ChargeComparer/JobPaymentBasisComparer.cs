using System.Collections.Generic;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	abstract class JobPaymentBasisComparer : IEqualityComparer<JobPaymentBasis>
	{
		protected abstract JobPaymentBasisComparer PaymentBasisComparer { get; }
		public static JobPaymentBasisComparer SameChargeable => new SameChargeableComparer();
		public static JobPaymentBasisComparer SameRate => new SameRateComparer();

		protected abstract bool EqualsCore(JobPaymentBasis x, JobPaymentBasis y);
		bool IEqualityComparer<JobPaymentBasis>.Equals(JobPaymentBasis x, JobPaymentBasis y)
		{
			if (x == null)
			{
				return y == null;
			}

			var result = y != null
				&& x.PBS_AdapterID == y.PBS_AdapterID
				&& x.ChargeCode == y.ChargeCode
				&& PaymentBasisComparer.EqualsCore(x, y);

			return result;
		}

		protected abstract int GetHashCodeCore(JobPaymentBasis jobPaymentBasis);
		int IEqualityComparer<JobPaymentBasis>.GetHashCode(JobPaymentBasis jobPaymentBasis)
		{
			return jobPaymentBasis.PBS_AdapterID.GetHashCode()
				^ jobPaymentBasis.ChargeCode.GetHashCode()
				^ PaymentBasisComparer.GetHashCodeCore(jobPaymentBasis);
		}
	}

	sealed class SameChargeableComparer : JobPaymentBasisComparer
	{
		protected override bool EqualsCore(JobPaymentBasis x, JobPaymentBasis y)
		{
			return x.PBS_ChargeableAmount == y.PBS_ChargeableAmount
				&& x.PBS_ChargeableUnit == y.PBS_ChargeableUnit;
		}

		protected override int GetHashCodeCore(JobPaymentBasis jobPaymentBasis)
		{
			return jobPaymentBasis.PBS_ChargeableAmount.GetHashCode()
				^ jobPaymentBasis.PBS_ChargeableUnit.GetHashCode();
		}

		protected override JobPaymentBasisComparer PaymentBasisComparer => new SameChargeableComparer();
	}

	sealed class SameRateComparer : JobPaymentBasisComparer
	{
		protected override bool EqualsCore(JobPaymentBasis x, JobPaymentBasis y)
		{
			return x.PBS_FlatRate == y.PBS_FlatRate
				&& x.PBS_PerUnitRate == y.PBS_PerUnitRate
				&& x.PBS_RateUnit == y.PBS_RateUnit
				&& x.PBS_MinRate == y.PBS_MinRate
				&& x.PBS_MaxRate == y.PBS_MaxRate;
		}

		protected override int GetHashCodeCore(JobPaymentBasis jobPaymentBasis)
		{
			return jobPaymentBasis.RateValue.GetHashCode()
				^ jobPaymentBasis.RateUnit.GetHashCode()
				^ jobPaymentBasis.RateReference.GetHashCode();
		}

		protected override JobPaymentBasisComparer PaymentBasisComparer => new SameRateComparer();
	}
}
