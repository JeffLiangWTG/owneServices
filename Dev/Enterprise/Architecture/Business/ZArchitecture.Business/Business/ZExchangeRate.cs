using System;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class ZExchangeRate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZExchangeRate(BusinessObject bizObj, ExchangeRateType exchangeRateType, ZPropertyInfo rateInfo, ZPropertyInfoString currencyInfo)
			: base(bizObj.Factory)
		{
			currencyInfo.AdditionalValidation += CurrencyInfo_AdditionalValidation;
			rateInfo.AdditionalValidation += RateInfo_AdditionalValidation;

			this.BizObj = bizObj;
			this.BizObjInternals = bizObj;
			this.Type = exchangeRateType;

			this.rateInfo = GetWrappedZPropertyInfo(nameof(Rate), x => rateInfo); // May be an identifier or GUID.
			this.currencyInfo = GetWrappedZPropertyInfo(nameof(Currency), x => currencyInfo); // May be an identifier or GUID.
		}

		public bool IsCurrencyRequired
		{
			get { return currencyRequired; }
			set { currencyRequired = value; }
		}

		bool currencyRequired;

		public bool IsRateRequired
		{
			get { return rateRequired; }
			set { rateRequired = value; }
		}

		[BusinessObjectTestExclude]
		public Func<bool> ShouldSetExchangeRateOnCurrencySetting { get; set; }

		bool rateRequired;

		internal void RateInfo_AdditionalValidation()
		{
			if (!IsValidationSuspended)
			{
				if (IsNewRate && Rate == 0 && !IsInPreSaveValidationNew)
				{
					var typeDescription = ObjectFactory.Get<IRefExchangeRateTypes>().GetDescriptionFromRateType(Type);
					RateInfo.AddWarning(Res.GetString("ef430aec-c7c1-440c-ba86-fb1f5d664477", "Today's {0} Not Found.", typeDescription));
				}
				else if (Rate < 0 || (Rate == 0 && !Currency.IsEmpty && IsRateRequired))
				{
					var errorMessage = Currency.IsEmpty
						? Res.GetString("31e78a1f-8322-459c-bdb1-8e71d48c5719", "Rate must be greater than 0.")
						: Res.GetString("2dd8c4a9-946d-4dc8-ae2a-3da4fea2432b", "Exchange Rate for Currency {0} must be greater than 0.", Currency);

					RateInfo.AddError(errorMessage);
				}
			}
		}

		internal void CurrencyInfo_AdditionalValidation()
		{
			if (IsCurrencyRequired)
			{
				MandatoryValidation.CheckEntered(CurrencyInfo);
				ListValidation.ErrorIfInvalidCode(CurrencyInfo, this.CurrencyList);
			}
			else if (!Currency.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(CurrencyInfo, this.CurrencyList);
			}
		}

		public readonly ExchangeRateType Type;

		public void RefetchExchangeRate()
		{
			if (!Currency.IsEmpty)
			{
				IsNewRate = true;
				RateInfo.Value = GetTodaysRate(Currency);
			}
		}

		#region Currency

		public bool DontSetTodaysRateOnCurrencyChange { get; set; }

		[List("CurrencyList")]
		public ZString Currency
		{
			get { return (ZString)CurrencyInfo.InnerInfo.Value; }
			set
			{
				if (Currency != value)
				{
					IsNewRate = true;
					CurrencyInfo.InnerInfo.Value = value;

					if (CurrencyIsValid && (ShouldSetExchangeRateOnCurrencySetting == null || ShouldSetExchangeRateOnCurrencySetting()))
					{
						if (StaticCurrentFetcher.Instance.CurrentCompany.Currency.RX_Code == value)
						{
							Rate = 1;
							RateInfo.RefreshBinding();
						}
						else
						{
							IsSetFromCurrency = true;
							if (!DontSetTodaysRateOnCurrencyChange)
							{
								Rate = GetTodaysRate(value);
							}

							IsNewRate = true;
							RateInfo.RefreshBinding();
						}
					}

					DontSetTodaysRateOnCurrencyChange = false;

					CurrencyInfo.InnerInfo.RefreshBinding();
					CurrencyInfo.RefreshBinding();
				}
			}
		}

		public bool CurrencyIsValid
		{
			get
			{
				if (!Currency.IsEmpty)
				{
					IRefCurrency currency = (IRefCurrency)BizObj.Factory.LoadFromNaturalKey(ObjectFactory.GetType<IRefCurrency>(), RefCurrencySchema.RX_Code, Currency);
					return currency != null;
				}
				else
				{
					return false;
				}
			}
		}

		public ZWrappedPropertyInfo CurrencyInfo
		{
			get { return currencyInfo; }
		}

		public bool Currency_ReadOnly
		{
			get { return currencyManualReadOnly || CurrencyInfo.InnerInfo.ReadOnly; }
			set
			{
				currencyManualReadOnly = value;
				CurrencyInfo.InnerInfo.RefreshBinding();
				CurrencyInfo.RefreshBinding();
			}
		}

		protected bool currencyManualReadOnly;

		protected IBusinessObjectCollection fCurrencyList;

		public IBusinessObjectCollection CurrencyList
		{
			get
			{
				if (fCurrencyList == null)
				{
					fCurrencyList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCurrencyCollection>(), new object[] { Factory });
				}

				return fCurrencyList;
			}
		}

		protected void ValidateCurrency()
		{
			((IBusinessObjectInternals)CurrencyInfo.InnerInfo.BizObj).Validate(CurrencyInfo.InnerInfo.Name);
		}

		#endregion

		#region Rate

		[DecimalPlaces("RateDecimalPlaces")]
		public ZDecimal Rate
		{
			get { return (ZDecimal)RateInfo.InnerInfo.Value; }
			set
			{
				if (Rate != value)
				{
					IsNewRate = IsSetFromCurrency;
					RateInfo.InnerInfo.Value = value;
				}
				IsSetFromCurrency = false;
				RateInfo.InnerInfo.RefreshBinding();
				RateInfo.RefreshBinding();
			}
		}

		protected bool IsSetFromCurrency { get; set; }

		public ZWrappedPropertyInfo RateInfo
		{
			get { return rateInfo; }
		}

		public bool IsRateReadOnly
		{
			get
			{
				return rateManualReadOnly ||
					Currency.IsEmpty || !CurrencyIsValid ||
					StaticCurrentFetcher.Instance.CurrentCompany.Currency.RX_Code == Currency ||
					AdditionalRateReadOnlyCondition;
			}
			set { Rate_ReadOnly = value; }
		}

		protected bool Rate_ReadOnly
		{
			get { return IsRateReadOnly || RateInfo.InnerInfo.ReadOnly; }
			set
			{
				rateManualReadOnly = value;
				RateInfo.InnerInfo.RefreshBinding();
				RateInfo.RefreshBinding();
			}
		}

		bool rateManualReadOnly;

		public bool AdditionalRateReadOnlyCondition { get; set; }

		#region RateDecimalPlaces

		internal protected int RateDecimalPlaces
		{
			get
			{
				return RateDecimalPlacesCore;
			}
		}

		protected virtual int RateDecimalPlacesCore
		{
			get
			{
				return 6;
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected readonly BusinessObject BizObj;
		protected readonly IBusinessObjectInternals BizObjInternals;
		protected readonly ZWrappedPropertyInfo rateInfo;
		protected readonly ZWrappedPropertyInfo currencyInfo;

		protected bool IsNewRate;

		protected virtual ZDecimal GetTodaysRate(ZString currency)
		{
			if (!currency.IsEmpty)
			{
				return new ZDecimal(EnvProxy.Instance.CurrentCompany.ExchangeRate.TodaysRate(currency, Type));
			}
			return 0m;
		}

		bool IsInPreSaveValidationNew
		{
			get { return BizObjInternals.IsInPreSaveValidation; }
		}

		#endregion
	}
}
