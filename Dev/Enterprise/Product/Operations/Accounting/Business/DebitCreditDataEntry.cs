using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public enum DebitCredit
	{
		DR,
		CR
	}

	/// <summary>
	/// The mapping of an unsigned amount and a debit/credit indicator to a signed amount.
	/// </summary>
	public class DebitCreditDataEntry
	{
		public static readonly ZString CR = nameof(DebitCredit.CR);
		public static readonly ZString DR = nameof(DebitCredit.DR);

		public DebitCreditDataEntry(Func<ZDecimal> getAmount, Action<ZDecimal> setAmount, bool isDebitForPositiveAmount = true)
		{
			getUnderlyingAmount = getAmount;
			setUnderlyingAmount = setAmount;
			DebitCreditSign_cached = DR;
			IsDebitForPositiveAmount = isDebitForPositiveAmount;
		}

		public void OnLoaded()
		{
			SetDebitCreditWithoutRecalculation(GetDebitCreditByUnderlyingAmountSign());
		}

		public ZDecimal UnsignedAmount
		{
			get { return Math.Abs(UnderlyingAmount); }
			set { SetUnderlyingAmount(value); }
		}

		public void SetDebitCreditWithoutRecalculation(ZString debitCredit)
		{
			DebitCreditSign_cached = debitCredit;
		}

		public ZString DebitCreditSign
		{
			get
			{
				ZString result = DebitCreditSign_cached;

				if (List.ContainsCode(result) && UnderlyingAmount != 0)
				{
					result = GetDebitCreditByUnderlyingAmountSign();
				}

				return result;
			}
			set
			{
				DebitCreditSign_cached = value;
				if (List.ContainsCode(value))
				{
					SetUnderlyingAmount(UnderlyingAmount);
				}
			}
		}

		public CodeDescriptionPairList List
		{
			get { return List_cached ?? (List_cached = new CodeDescriptionPairList(OLookUpEditType.DebitCredit)); }
		}

		#region Implementation

		readonly Func<ZDecimal> getUnderlyingAmount;
		readonly Action<ZDecimal> setUnderlyingAmount;
		readonly bool IsDebitForPositiveAmount;
		CodeDescriptionPairList List_cached;
		ZString DebitCreditSign_cached;

		void SetUnderlyingAmount(ZDecimal value)
		{
			value = Math.Abs(value);
			if (!UnderlyingAmount.IsEmpty || !value.IsEmpty)
			{
				UnderlyingAmount = value.IsEmpty ? ZDecimal.Zero : (ZDecimal)(value * GetUnderlyingAmountSignByDebitCredit());
			}
		}

		ZDecimal UnderlyingAmount
		{
			get { return getUnderlyingAmount(); }
			set { setUnderlyingAmount(value); }
		}

		int GetUnderlyingAmountSignByDebitCredit()
		{
			int sign;
			if (List.ContainsCode(DebitCreditSign_cached))
			{
				sign = DebitCreditSign_cached == CR ^ IsDebitForPositiveAmount ? 1 : -1;
			}
			else
			{
				sign = UnderlyingAmount >= 0 ? 1 : -1;
			}

			return sign;
		}

		ZString GetDebitCreditByUnderlyingAmountSign()
		{
			return UnderlyingAmount < 0 ^ IsDebitForPositiveAmount ? DR : CR;
		}

		#endregion
	}
}