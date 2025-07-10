using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public partial class CashbookExchangeDiffCalculation
	{
		public CashbookExchangeDiffCalculation(AccBankAccount bank)
		{
			BankAccount = bank;

			amountValuesCaches = new Dictionary<string, AmountValues>();
		}

		#region BankCurrencyBalance

		public ZDecimal BankCurrencyBalance
		{
			get
			{
				ZDecimal value = 0m;
				if (fBank != null)
				{
					value = fOSAmount + fOSBankBalance;
				}
				return value;
			}
		}

		#endregion

		#region LocalAmountBeforeAdjustment

		public ZDecimal LocalAmountBeforeAdjustment
		{
			get
			{
				ZDecimal value = 0m;
				if (fBank != null)
				{
					value = fLocalAmount + fLocalBankBalance;
				}
				return value;
			}
		}

		#endregion

		#region CurrentExchangeRate

		public ZDecimal CurrentExchangeRate
		{
			get
			{
				ZDecimal value = 0m;
				if (fBank != null)
				{
					value = Env.CurrentCompany.ExchangeRate.GetRate(LocalAmountBeforeAdjustment, BankCurrencyBalance);
				}
				return value;
			}
		}

		#endregion

		public AccBankAccount BankAccount
		{
			set
			{
				var hasChanges = fBank != value;
				fBank = value;
				if (hasChanges && fBank != null)
				{
					Factory = fBank.Factory;
					Load();
				}
			}
			get { return fBank; }
		}

		public ZDateTime PostDate
		{
			get
			{
				return fPostDate;
			}
			set
			{
				var hasChanges = fPostDate != value;
				fPostDate = value;
				if (hasChanges && fBank != null)
				{
					Factory = fBank.Factory;
					Load();
				}
			}
		}

		#region GetLocalAmountAfterAdjustment

		public ZDecimal GetLocalAmountAfterAdjustment(ZDecimal newExchangeRate)
		{
			ZDecimal value = 0m;
			if (fBank != null)
			{
				value = Env.CurrentCompany.ExchangeRate.ForeignToLocal(BankCurrencyBalance, newExchangeRate);
			}
			return value;
		}

		#endregion

		#region Foreign Currency Gain/Loss

		public ZDecimal GetForeignCurrencyGainLoss(ZDecimal newExchangeRate)
		{
			ZDecimal value = 0m;
			if (fBank != null)
			{
				value = GetLocalAmountAfterAdjustment(newExchangeRate) - LocalAmountBeforeAdjustment;
			}
			return value;
		}

		#endregion

		#region Implementation

		#region Load

		void Load()
		{
			if (IsSmallDateTimeValid(PostDate) && BankAccount != null)
			{
				var key = BankAccount.PK + PostDate.ToString();

				AmountValues amountValues;
				if (!amountValuesCaches.TryGetValue(key, out amountValues))
				{
					string sQLText = @"SELECT SUM(LocalAmount) AS LocalAmount, SUM(OSAmount) AS OSAmount
FROM dbo.BankAmounts
WHERE BankPK = @BankPK
AND PostDate < @PostDate";

					DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
					ZSqlParameterCollection parameters = new ZSqlParameterCollection();
					parameters.Add("@BankPK", fBank.PK, AccTransactionHeaderSchema.AH_AB);
					parameters.Add("@PostDate", PostDate.Date.AddDays(1).ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);

					dynBizOs.Load(sQLText, parameters);

					fBank.Reload();

					SetAmountValues((ZDecimal)dynBizOs[0]["LocalAmount"], (ZDecimal)dynBizOs[0]["OSAmount"], fBank.AB_OpenOSBalance, fBank.AB_OpenBalance);

					amountValuesCaches.Add(key,
						new AmountValues()
						{
							LocalAmount = fLocalAmount,
							OSAmount = fOSAmount,
							OSBankBalance = fOSBankBalance,
							LocalBankBalance = fLocalBankBalance
						});
				}
				else
				{
					SetAmountValues(amountValues.LocalAmount, amountValues.OSAmount, amountValues.OSBankBalance, amountValues.LocalBankBalance);
				}
			}
			else
			{
				SetAmountValues(0m, 0m, 0m, 0m);
			}
		}

		struct AmountValues
		{
			public ZDecimal LocalAmount;
			public ZDecimal OSAmount;
			public ZDecimal OSBankBalance;
			public ZDecimal LocalBankBalance;
		}

		readonly Dictionary<string, AmountValues> amountValuesCaches;

		public void ClearAmountValuesCaches()
		{
			amountValuesCaches.Clear();
		}

		bool IsSmallDateTimeValid(ZDateTime time)
		{
			if (!time.IsValid || time.IsEmpty)
			{
				return false;
			}

			if (time < ZDateTime.MinSmallDateTimeValue || time > ZDateTime.MaxSmallDateTimeValue)
			{
				return false;
			}

			return true;
		}

		void SetAmountValues(ZDecimal osAmount, ZDecimal localAmount, decimal osBankBalance, decimal localBankBalance)
		{
			fLocalAmount = osAmount;
			fOSAmount = localAmount;
			fOSBankBalance = osBankBalance;
			fLocalBankBalance = localBankBalance;
		}

		#endregion

		AccBankAccount fBank;
		ZDateTime fPostDate;
		BusinessObjectFactory Factory;
		ZDecimal fOSAmount;
		ZDecimal fLocalAmount;
		decimal fOSBankBalance;
		decimal fLocalBankBalance;

		#endregion
	}
}
