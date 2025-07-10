using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public class DutyResult : IComparable
	{
		public Money Amount
		{
			get
			{
				if (fAmount == null)
				{
					fAmount = new Money(0, GlbCompany.CurrentCompany.Country.LocalCurrency);
				}
				return fAmount;
			}
			set
			{
				fAmount = value;
			}
		}
		Money fAmount;

		public bool HasDuty
		{
			get
			{
				return Amount.Amount > 0 || Percent > 0;
			}
		}

		public bool IsValid
		{
			get { return Amount.IsValid; }
		}

		public ZDecimal Percent
		{
			get;
			set;
		}

		public ZDecimal FlatRateAmount
		{
			get;
			set;
		}

		public ZString FlatRateUQ
		{
			get;
			set;
		}

		public DutyResult RoundDown(int decimalPlaces)
		{
			DutyResult result = new DutyResult();
			result.Amount = Amount.FuzzyRoundDown(decimalPlaces);
			result.Percent = Percent;
			return result;
		}

		public int CompareTo(object obj)
		{
			if (!(obj is DutyResult))
			{
				throw new ArgumentException("obj is not the same type as this instance.");
			}

			DutyResult valueToCompare = (DutyResult)obj;

			if (Amount.Currency.Code != valueToCompare.Amount.Currency.Code)
			{
				throw new ApplicationException("Unable to compare two DutyResult objects - They have different currencies");
			}
			return Amount.Amount.CompareTo(valueToCompare.Amount.Amount);
		}

		public ZString DutyRateDescription
		{
			get
			{
				ZString percentageBasedDutyRate = Percent.IsEmpty ? "" : Percent.ToString(2) + "%";
				ZString flatRateBaseDutyRate = FlatRateAmount.IsEmpty ? "" : FlatRateAmount.ToString(2) + (FlatRateUQ.IsEmpty ? "" : "/" + FlatRateUQ);
				return percentageBasedDutyRate
					+ (percentageBasedDutyRate.IsEmpty || flatRateBaseDutyRate.IsEmpty ? "" : "+")
					+ flatRateBaseDutyRate;
			}
		}
	}

	public interface ILineDutyData
	{
		void SetDutyResult(DutyResult dutyResult);
		void SetFeeResult(ZString feeType, ZDecimal feeAmount);
	}

	public interface IHeaderFeeData
	{
		void SetFeeResult(ZString feeType, ZDecimal feeAmount);

		IEnumerable<ILineDutyData> Lines { get; }
	}

	public interface IDutyCalculationManager
	{
		void Calculate(IEnumerable<IHeaderFeeData> entries);
	}
}
