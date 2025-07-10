using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public class LocalForeignDataEntry
	{
		public LocalForeignDataEntry(ZPropertyInfo currencyFieldInfo, ZPropertyInfo exchangeRateFieldInfo, ZPropertyInfo localAmountFieldInfo, ZPropertyInfo foreignAmountFieldInfo, ZAccExchangeRate exchangeRate,
			bool doNotUpdateFromLocal = false, bool doNotSetRateOnCurrencyChange = false)
		{
			this.DoNotSetRateOnCurrencyChange = doNotSetRateOnCurrencyChange;
			this.CurrencyFieldInfo = currencyFieldInfo;
			this.CurrencyFieldInfo.ValueChanged += new EventHandler(CurrencyFieldInfo_ValueChanged);

			this.ExchangeRateFieldInfo = exchangeRateFieldInfo;
			this.ExchangeRateFieldInfo.ValueChanged += new EventHandler(ExchangeRateFieldInfo_ValueChanged);

			this.DoNotUpdateFromLocal = doNotUpdateFromLocal;
			this.LocalAmountFieldInfo = localAmountFieldInfo;
			if (!doNotUpdateFromLocal)
			{
				this.LocalAmountFieldInfo.ValueChanged += new EventHandler(LocalAmountFieldInfo_ValueChanged);
			}

			this.ForeignAmountFieldInfo = foreignAmountFieldInfo;
			this.ForeignAmountFieldInfo.ValueChanged += new EventHandler(ForeignAmountFieldInfo_ValueChanged);

			this.ExchangeRate = exchangeRate;
		}

		public void UnhookEvents()
		{
			this.CurrencyFieldInfo.ValueChanged -= new EventHandler(CurrencyFieldInfo_ValueChanged);
			this.ExchangeRateFieldInfo.ValueChanged -= new EventHandler(ExchangeRateFieldInfo_ValueChanged);
			if (!DoNotUpdateFromLocal)
			{
				this.LocalAmountFieldInfo.ValueChanged -= new EventHandler(LocalAmountFieldInfo_ValueChanged);
			}

			this.ForeignAmountFieldInfo.ValueChanged -= new EventHandler(ForeignAmountFieldInfo_ValueChanged);
		}

		#region Implementation

		protected bool IsUpdating;

		protected ZPropertyInfo CurrencyFieldInfo;
		protected ZPropertyInfo ExchangeRateFieldInfo;
		protected ZPropertyInfo LocalAmountFieldInfo;
		protected ZPropertyInfo ForeignAmountFieldInfo;
		readonly bool DoNotUpdateFromLocal;
		readonly bool DoNotSetRateOnCurrencyChange;

		readonly ZAccExchangeRate ExchangeRate;

		void CurrencyFieldInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsUpdating)
			{
				try
				{
					IsUpdating = true;
					ZString currencyCode = GetCurrencyCode();

					if (ExchangeRate.CurrencyIsValid)
					{
						if (!DoNotSetRateOnCurrencyChange)
						{
							if (currencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
							{
								ExchangeRateFieldInfo.Value = new ZDecimal(1);
							}
							else
							{
								ExchangeRateFieldInfo.Value = ExchangeRate.TodaysRate(currencyCode);
							}
						}

						SetLocalAmount();
					}
				}
				finally
				{
					IsUpdating = false;
				}
			}
		}

		ZString GetCurrencyCode()
		{
			if (CurrencyFieldInfo is ZPropertyInfoGuid)
			{
				if (CurrencyFieldInfo.Value.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					RefCurrency currency = (new BusinessObjectFactory()).Load<RefCurrency>((ZGuid)CurrencyFieldInfo.Value);
					return currency != null ? currency.RX_Code : ZString.Empty;
				}
			}
			else
			{
				return (ZString)CurrencyFieldInfo.Value;
			}
		}

		void ExchangeRateFieldInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsUpdating)
			{
				try
				{
					IsUpdating = true;
					SetLocalAmount();
				}
				finally
				{
					IsUpdating = false;
				}
			}
		}

		void LocalAmountFieldInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsUpdating)
			{
				try
				{
					IsUpdating = true;
					SetExchangeAmount();
				}
				finally
				{
					IsUpdating = false;
				}
			}
		}

		void ForeignAmountFieldInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsUpdating)
			{
				try
				{
					IsUpdating = true;
					SetLocalAmount();
				}
				finally
				{
					IsUpdating = false;
				}
			}
		}

		protected void SetLocalAmount()
		{
			LocalAmountFieldInfo.Value = (ZDecimal)CurrentExchangeRate.ForeignToLocal((ZDecimal)ForeignAmountFieldInfo.Value, (ZDecimal)ExchangeRateFieldInfo.Value);
		}

		protected void SetForeignAmount()
		{
			if (CurrencyFieldInfo.Value.IsValid)
			{
				ForeignAmountFieldInfo.Value = (ZDecimal)CurrentExchangeRate.LocalToForeign((ZDecimal)LocalAmountFieldInfo.Value, (ZDecimal)ExchangeRateFieldInfo.Value, ((ZString)CurrencyFieldInfo.Value));
			}
		}
		protected void SetExchangeAmount()
		{
			if (!ForeignAmountFieldInfo.Value.IsEmpty && !LocalAmountFieldInfo.Value.IsEmpty)
			{
				ExchangeRateFieldInfo.Value = (ZDecimal)Env.CurrentCompany.ExchangeRate.GetRate((ZDecimal)LocalAmountFieldInfo.Value, (ZDecimal)ForeignAmountFieldInfo.Value);
			}
		}

		protected ExchangeRate CurrentExchangeRate
		{
			get { return Env.CurrentCompany.ExchangeRate; }
		}

		#endregion
	}
}
