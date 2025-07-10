using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class CurrencyExchangeService : IService
	{
		public CurrencyExchangeService(BusinessObjectFactory factory, ZDateTime dateForRate)
		{
			Factory = factory;
			this.dateForRate = dateForRate;
		}

		public static CurrencyExchangeService GetInstance(BusinessObjectFactory factory, ZDateTime dateForRate)
		{
			var result = factory.ServiceContainer.GetService<CurrencyExchangeService>();
			if (result == null)
			{
				result = new CurrencyExchangeService(factory, dateForRate);
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		/// <summary>
		/// Call this for each potential call to GetRate before any call to GetRate to minimize database round trips.
		/// GetRate will work without this, but may require additional trips.
		/// </summary>
		public void PrepareToGetRateFor(string sourceCurrency, string destinationCurrency, GlbCompany localCompany)
		{
			if (sourceCurrency != destinationCurrency)
			{
				PrepareOrGet(sourceCurrency, destinationCurrency, localCompany, false);
			}
		}

		/// <summary>
		/// Get the exchange rate as a multiplier rounded to 5 decimal places.
		/// Returns zero if rate not found.
		/// </summary>
		public decimal GetRate(string sourceCurrency, string destinationCurrency, GlbCompany localCompany)
		{
			return sourceCurrency != destinationCurrency
				? (decimal)PrepareOrGet(sourceCurrency, destinationCurrency, localCompany, true)
				: 1m;
		}

		public decimal GetAmount(string sourceCurrency, string destinationCurrency, GlbCompany localCompany, decimal sourceAmount)
		{
			var rate = GetRate(sourceCurrency, destinationCurrency, localCompany);
			var currency = GetCurrencyByCode(destinationCurrency);
			return Utilities.Round(rate * sourceAmount, currency.Decimals);
		}

		public RefCurrency GetCurrencyByCode(string code)
		{
			RefCurrency currency;
			if (!codeToCurrency.TryGetValue(code, out currency))
			{
				codeToCurrency.Add(code, null);
			}

			if (currency == null)
			{
				LoadCurrencies();
				codeToCurrency.TryGetValue(code, out currency);
			}

			return currency;
		}

		readonly ZDateTime dateForRate;
		readonly BusinessObjectFactory Factory;
		readonly Dictionary<GlbCompany, Dictionary<string, Dictionary<string, decimal?>>> companyToSourceToDestinationToRate = new Dictionary<GlbCompany, Dictionary<string, Dictionary<string, decimal?>>>();
		readonly Dictionary<string, RefCurrency> codeToCurrency = new Dictionary<string, RefCurrency>();
		readonly Dictionary<GlbCompany, CurrencyConverter> companyToConverter = new Dictionary<GlbCompany, CurrencyConverter>();

		decimal? PrepareOrGet(string sourceCurrency, string destinationCurrency, GlbCompany localCompany, bool isGet)
		{
			Dictionary<string, Dictionary<string, decimal?>> sourceToDestinationToRate;
			if (!companyToSourceToDestinationToRate.TryGetValue(localCompany, out sourceToDestinationToRate))
			{
				sourceToDestinationToRate = new Dictionary<string, Dictionary<string, decimal?>>();
				companyToSourceToDestinationToRate.Add(localCompany, sourceToDestinationToRate);
			}

			Dictionary<string, decimal?> destinationToRate;
			if (!sourceToDestinationToRate.TryGetValue(sourceCurrency, out destinationToRate))
			{
				destinationToRate = new Dictionary<string, decimal?>();
				sourceToDestinationToRate.Add(sourceCurrency, destinationToRate);
			}

			decimal? rate;
			if (!destinationToRate.TryGetValue(destinationCurrency, out rate))
			{
				rate = null;
				destinationToRate.Add(destinationCurrency, null);

				if (!codeToCurrency.ContainsKey(sourceCurrency))
				{
					codeToCurrency.Add(sourceCurrency, null);
				}

				if (!codeToCurrency.ContainsKey(destinationCurrency))
				{
					codeToCurrency.Add(destinationCurrency, null);
				}
			}

			if (isGet && !rate.HasValue)
			{
				Load();
				rate = destinationToRate[destinationCurrency];
			}

			return rate;
		}

		void LoadCurrencies()
		{
			var currenciesToLoad = codeToCurrency.Where(x => x.Value == null);
			if (currenciesToLoad.Any())
			{
				var currencies = Factory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currenciesToLoad.Select(x => x.Key)));
				foreach (var currency in currencies)
				{
					codeToCurrency[currency.RX_Code] = currency;
				}
			}
		}

		void Load()
		{
			LoadCurrencies();

			foreach (var companyMapPair in companyToSourceToDestinationToRate)
			{
				var sourceToDestinationToRate = companyMapPair.Value;

				if (sourceToDestinationToRate.Any(x => x.Value.Any(y => !y.Value.HasValue)))
				{
					CurrencyConverter currencyConverter;
					if (!companyToConverter.TryGetValue(companyMapPair.Key, out currencyConverter))
					{
						currencyConverter = CurrencyConverter.New(companyMapPair.Key, Factory, dateForRate, ExchangeRateType.Sell, 0);
						companyToConverter.Add(companyMapPair.Key, currencyConverter);
					}

					foreach (var sourceMapPair in sourceToDestinationToRate)
					{
						var rate1 = currencyConverter.GetExchangeRate(codeToCurrency[sourceMapPair.Key]);

						foreach (var destinationCurrency in sourceMapPair.Value.Where(x => !x.Value.HasValue).Select(x => x.Key).ToArray())
						{
							var rate2 = currencyConverter.GetExchangeRate(codeToCurrency[destinationCurrency]);
							decimal rate;
							if (rate1 != 0 && rate2 != 0)
							{
								rate = Utilities.Round(companyMapPair.Key.GC_IsReciprocal
									? rate1 / rate2
									: rate2 / rate1, 5);
							}
							else
							{
								rate = 0;
							}

							sourceMapPair.Value[destinationCurrency] = rate;
						}
					}
				}
			}
		}
	}
}

