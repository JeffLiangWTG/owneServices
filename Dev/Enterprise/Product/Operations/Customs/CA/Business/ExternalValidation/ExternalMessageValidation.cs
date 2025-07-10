using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ExternalMessageValidation : Customs.Business.ExternalMessageValidation
	{
		public ExternalMessageValidation(BusinessObject bizObj)
			: base(bizObj)
		{
			this.bizObj = bizObj;
		}
		readonly BusinessObject bizObj;

		protected override string GetAdviceHowToFixNoValidExchangeRates()
		{
			return AdviceHowToFixNoValidExchangeRates;
		}
		internal static string AdviceHowToFixNoValidExchangeRates
		{
			get { return "\r\n" + Res.GetString("e552c7bc-2ea7-4501-9358-133e46addcc3", "Please update the customs exchange rate."); }
		}

		public override void ValidateExchangeRateExist(RefCurrencyCurrencyConverter currencyConverter, ZPropertyInfo currInfo, int dayOffsetToFallbackBeforeWarningShown = 0)
		{
			base.ValidateExchangeRateExist(currencyConverter, currInfo, MaximumDaysToFallbackBeforeWarningShown);
		}

		public override string NoValidExRatesMessage(string currencyConverterDateForRate)
		{
			ZStringBuilder noValidExRatesMessage = new ZStringBuilder();
			noValidExRatesMessage.Append(Res.GetString("9f3ec1ef-1ca9-4d43-b613-585079e545aa", "There is no valid currency exchange rate for this currency on file for the past year."));
			noValidExRatesMessage.Append(GetAdviceHowToFixNoValidExchangeRates());
			return noValidExRatesMessage.ToString();
		}

		public override string ExchangeRateOutOfRangeWarningMessage(string currencyConverterDateForRate, string currencyCode, string foundRateDate)
		{
			ZStringBuilder message = new ZStringBuilder();
			message.Append(Res.GetString("21397fb3-aab4-4df6-8711-0b9faaad2020", "There is no exchange rate on file for {0}. The closest match of the exchange rate for {1} is the rate for the date {2}. This exchange rate will be used.", currencyConverterDateForRate, currencyCode, foundRateDate));
			message.Append(GetAdviceHowToFixNoValidExchangeRates());
			return message.ToString();
		}

		int MaximumDaysToFallbackBeforeWarningShown
		{
			get
			{
				int result = 0;
				var currencyConverterDataProvider = bizObj as ICurrencyConverterDataProvider;
				if (currencyConverterDataProvider != null)
				{
					var dateOfValudation = currencyConverterDataProvider.DateOfValuation;
					if (dateOfValudation.IsValid)
					{
						switch (dateOfValudation.DayOfWeek)
						{
							case DayOfWeek.Sunday:
								result = 1;
								break;
							case DayOfWeek.Monday:
								result = 2;
								break;
						}
					}
				}
				return result;
			}
		}
	}
}
