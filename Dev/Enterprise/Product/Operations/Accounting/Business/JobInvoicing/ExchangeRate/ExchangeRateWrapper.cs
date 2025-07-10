using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ExchangeRateWrapper : IExchangeRateJobBilling
	{
		public ExchangeRateWrapper(ExchangeRate exchangeRate, OrgHeader org)
		{
			if (exchangeRate == null)
			{
				throw new ArgumentNullException(nameof(exchangeRate));
			}
			if (exchangeRate.JF_OH_Org.IsValid && org != null && org.PK != exchangeRate.JF_OH_Org)
			{
				throw new ArgumentException("Org parameter must match the Exchange Rate Organisation on non-generic Exchange Rate.", nameof(org));
			}
			this.exchangeRate = exchangeRate;
			this.org = org;
		}

		readonly ExchangeRate exchangeRate;
		readonly OrgHeader org;
		event EventHandler changed;
		bool hasSubscribed;

		public ZDecimal Rate => exchangeRate.JF_BaseRate;

		public ZDecimal SellRate
		{
			get
			{
				if (!exchangeRate.IsGenericRate)
				{
					return exchangeRate.JF_SellRate;
				}
				return ExchangeRateHelper.GetBaseRateAdjustedByCFX(CurrencyCode, Rate, CFXPercent, exchangeRate.Job?.Company);
			}
		}

		public ZDecimal CFXPercent
		{
			get
			{
				if (!exchangeRate.IsGenericRate)
				{
					return exchangeRate.JF_CFXPercent;
				}

				if (RefCurrency.IsExcludedCFXCalculation(CurrencyCode))
				{
					return 0;
				}

				GetCFXPair(out var cfxPercent, out _);

				return cfxPercent;
			}
		}

		public ZDecimal CFXMinimum
		{
			get
			{
				if (!exchangeRate.IsGenericRate)
				{
					return exchangeRate.JF_CFXMinimum;
				}

				if (RefCurrency.IsExcludedCFXCalculation(CurrencyCode))
				{
					return 0;
				}

				GetCFXPair(out _, out var cfxMin);

				return cfxMin;
			}
		}

		void GetCFXPair(out ZDecimal cfxPercent, out ZDecimal cfxMin)
		{
			if (exchangeRate.ParentJob == null)
			{
				cfxPercent = cfxMin = 0;
				return;
			}

			var ledgerType = OrgType.ToLedger();
			var invoiceCurrencyType = ExchangeRateEnumsExtensions.GetInvoiceCurrencyTypeFromCode(exchangeRate.EffectiveInvoiceCurrencyType);
			var preferredDate = AccExchangeRateConfigurationRateFinder.GetPreferredExchangeRateDate(
				exchangeRate.ParentJob.ExchangeRateConfigurationRateConsumer, org, CurrencyCode, ledgerType, invoiceCurrencyType);
			exchangeRate.ParentJob.GetCFXPairFromOrganization(org, CurrencyCode, preferredDate, out cfxPercent, out cfxMin);
		}

		public ZGuid OrgPk => org?.PK ?? exchangeRate.JF_OH_Org; //they either equal or the last one is empty

		public ExchangeRateOrgTypeEnum OrgType => exchangeRate.OrgType;

		public ZString CurrencyCode => exchangeRate.JF_RX_NKRateCurrency;

		public event EventHandler Changed
		{
			add
			{
				changed += value;
				if (!hasSubscribed)
				{
					exchangeRate.Changed += OnExchangeRateChanged;
					hasSubscribed = true;
				}
			}
			remove
			{
				changed -= value;
				if (changed == null && hasSubscribed)
				{
					exchangeRate.Changed -= OnExchangeRateChanged;
					hasSubscribed = false;
				}
			}
		}

		public bool IsUserDefinedOrTransformed => exchangeRate.JF_IsTransformed;

		public void SetBaseRate(decimal value)
		{
			exchangeRate.JF_BaseRate = value;
		}

		public void RefreshCFXMinimum()
		{
			exchangeRate.RefreshCFXMinimum();
		}

		public void EnsureWillNotBeAutoDeleted()
		{
			exchangeRate.JF_IsTransformed = true;
		}

		public ZGuid ExchangeRatePk => exchangeRate.PK;

		void OnExchangeRateChanged(object sender, EventArgs e)
		{
			changed?.Invoke(sender, e);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			var exRateWrapper = (ExchangeRateWrapper)obj;

			return exRateWrapper.exchangeRate == exchangeRate && exRateWrapper.OrgPk == OrgPk;
		}

		public override int GetHashCode()
		{
			return exchangeRate.GetHashCode() ^ OrgPk.GetHashCode();
		}

		public bool IsDeleted => exchangeRate.IsDeleted;

#if DEBUG
		public void SetBuyRate_ForTestOnly(decimal rate)
		{
			exchangeRate.JF_BaseRate = rate;
		}
#endif
	}
}
