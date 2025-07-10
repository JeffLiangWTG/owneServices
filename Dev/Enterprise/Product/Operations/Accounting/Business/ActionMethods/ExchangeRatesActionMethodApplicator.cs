using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business
{
	public class ExchangeRatesActionMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not necessary to translate")]
		public ExchangeRatesActionMethodApplicator(ExchangeRatesSettings settings)
			: base("Updating Exchange Rates")
		{
			settingsRatesSource = settings.ExchangeRatesSource;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			switch (settingsRatesSource)
			{
				case ExchangeRatesSourceList.Codes.SailingSchedule:
					ApplyExchangeRates(log, GetSourceProvider(ExRateSourceType.Voyage), targets);
					break;

				case ExchangeRatesSourceList.Codes.BillingJobExRateRegistry:
					ApplyExchangeRates(log, GetSourceProvider(ExRateSourceType.BillingJob), targets);
					break;

				case ExchangeRatesSourceList.Codes.CurrencyFile:
					ApplyExchangeRates(log, (t) => new CurrencyFileSource(t.Factory), targets);
					break;

				default:
					throw new InvalidOperationException(string.Format("unrecognised source type '{0}'", settingsRatesSource));
			}
		}

		#region Implementation

		Converter<BusinessObject, IExchangeRateSource> GetSourceProvider(ExRateSourceType sourceType)
		{
			return delegate(BusinessObject target)
			{
				IJobInvoicingExRateSourceProvider provider = target as IJobInvoicingExRateSourceProvider;
				return provider == null ? null : provider.GetExRateSource(sourceType);
			};
		}

		void ApplyExchangeRates(IOperationalActionSectionLog log, Converter<BusinessObject, IExchangeRateSource> sourceProvider, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			var helper = ObjectFactory.Get<IUpdateExchangeRates>();
			foreach (BusinessObject target in targets)
			{
				helper.UpdateExchangeRates(target, sourceProvider, log);

				log.BumpSectionProgress();
			}
		}

		class CurrencyFileSource : IExchangeRateSource
		{
			public CurrencyFileSource(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public ZString Description
			{
				get { return (NoResString)"the current buy rates"; }
			}

			public ZDecimal? GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);

				decimal rate;

				if (currency != null && (rate = currency.CurrentBuyRate) != 0)
				{
					return rate;
				}
				else
				{
					return null;
				}
			}

			public ControllerID SourceController
			{
				get { return null; }
			}

			public ZGuid SourcePK
			{
				get { return ZGuid.Empty; }
			}

			readonly BusinessObjectFactory factory;

			#region GetEnumerator

			IEnumerator<IExchangeRate> IEnumerable<IExchangeRate>.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			#endregion
		}

		readonly ZString settingsRatesSource;

		#endregion
	}
}

