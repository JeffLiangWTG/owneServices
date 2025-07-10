using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public partial class ZAccExchangeRate : ZExchangeRate
	{
		public ZAccExchangeRate(BusinessObject bizObj, ExchangeRateType exchangeRateType, ZPropertyInfo rateInfo, ZPropertyInfoString currencyInfo, ZPropertyInfo companyPKPropertyInfo)
			: base(bizObj, exchangeRateType, rateInfo, currencyInfo)
		{
			rateInfo.AdditionalValidation += new RunValidationInvoker(RateInfo_AdditionalValidation);
			IsCurrencyRequired = true;
			IsRateRequired = true;
			this.companyPKPropertyInfo = companyPKPropertyInfo;
		}

		public string ExpiryDateWarning
		{
			get { return f_ExpiryDateWarning ?? (f_ExpiryDateWarning = Res.GetString("8b4d7355-e690-4b34-b8c7-eee31f61b263", "This exchange rate has expired.")); }
		}
		string f_ExpiryDateWarning;

		public ZDecimal TodaysRate(ZString currency)
		{
			return GetTodaysRate(currency);
		}

		public BusinessObject BizO
		{
			get { return BizObj; }
		}

		public bool IsExchangeFallbackRequired
		{
			get
			{
				RefExchangeRate mostRecentRate = GetMostRecentExchangeRate(Currency);
				return mostRecentRate != null && mostRecentRate.RE_ExpiryDate < ZDateTime.Today &&
					AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			}
		}

		void RateInfo_AdditionalValidation()
		{
			if (IsNewRate && IsExchangeFallbackRequired)
			{
				RateInfo.AddWarning(ExpiryDateWarning);
			}
		}

		protected override ZDecimal GetTodaysRate(ZString currency)
		{
			ZDecimal result = 0M;
			if (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value)
			{
				RefExchangeRate exRate = GetMostRecentExchangeRate(currency);
				if (exRate != null)
				{
					result = exRate.RE_SellRate;
				}
			}
			else
			{
				result = base.GetTodaysRate(currency);
			}
			return result;
		}

		protected override int RateDecimalPlacesCore
		{
			get
			{
				return Company.ExchangeRateDecimalPlaces;
			}
		}

		protected GlbCompany Company
		{
			get
			{
				if (companyPKPropertyInfo != null && !companyPKPropertyInfo.IsNullable)
				{
					var companyPKInfoValue = (ZGuid)companyPKPropertyInfo.Value;
					if (company == null || company.PK != companyPKInfoValue)
					{
						company = BizObj.Factory.Load<GlbCompany>(companyPKInfoValue);
					}
				}
				else
				{
					company = GlbCompany.CurrentCompany;
				}
				return company;
			}
		}
		GlbCompany company;
		readonly ZPropertyInfo companyPKPropertyInfo;

		RefExchangeRate GetMostRecentExchangeRate(ZString currency)
		{
			RefExchangeRate result = null;
			if (BizObj != null)
			{
				result = BizObj.Factory.LoadTop1<RefExchangeRate>(GetExchangeRateFilter(currency, true));
			}
			return result;
		}

		ZQuery GetExchangeRateFilter(ZString currency, bool includeOrderBy)
		{
			ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, currency);
			filter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, Type == ExchangeRateType.Buy ? Constants.ExchangeRateTypes.Code.BuyRate : Constants.ExchangeRateTypes.Code.SellRate);
			filter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, ZDateTime.Today.AddDays(1));
			if (includeOrderBy)
			{
				filter.OrderBy = RefExchangeRateSchema.RE_StartDate.Name + " DESC";
			}
			return filter;
		}
	}
}
