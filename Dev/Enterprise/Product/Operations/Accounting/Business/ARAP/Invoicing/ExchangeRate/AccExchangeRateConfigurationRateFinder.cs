using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.AccExchangeRateConfigurationViewLookups;

#if DEBUG
using Enterprise.MasterFiles.Business.Testing;
#endif

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class AccExchangeRateConfigurationRateFinder
	{
		public static bool GetExchangeRateConfigurationPromptAtCompanyLevel(IAccExchangeRateConfigurationRateConsumer rateConsumer) => GetCompanyLevelExchangeRateConfiguration(rateConsumer, string.Empty)?.Prompt ?? false;

		internal static ExchangeRateType? GetExchangeRateConfigurationRateType(IAccExchangeRateConfigurationRateConsumer rateConsumer, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, string currencyCode = CurrencyTypeCodes.ALL, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
			=> GetExchangeRateConfiguration(rateConsumer, organisation, ledgerType, invoiceCurrencyType, currencyCode)?.ExchangeRateType;

		#region Preference

		public static bool IsPreferenceArrivalDateAtCompanyLevel(IAccExchangeRateConfigurationRateConsumer rateConsumer)
		{
			var preference = GetCompanyLevelPreference(rateConsumer);
			return preference == Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate || preference == Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate;
		}

		public static bool IsPreferenceDepartureDateAtCompanyLevel(IAccExchangeRateConfigurationRateConsumer rateConsumer)
		{
			var preference = GetCompanyLevelPreference(rateConsumer);
			return preference == Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate || preference == Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate;
		}

		public static bool IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(IAccExchangeRateConfigurationRateConsumer jobRateConsumerForJobType, IAccExchangeRateConfigurationRateConsumer consolCostRateConsumerForDirectionAndTransportMode, ZString currencyCode)
		{
			var preference = GetCompanyLevelPreferenceForJobWithConsolDirectionAndConsolTransportMode(jobRateConsumerForJobType, consolCostRateConsumerForDirectionAndTransportMode, currencyCode);
			return preference == Core.Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate;
		}

		#endregion

		#region Exchange Rate For Date

		public static ZDecimal GetExchangeRate(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable, bool useLocaClient = true)
		{
			var exchangeRate = ZDecimal.Zero;

			if (currency != null)
			{
				var conf = GetExchangeRateConfiguration(rateConsumer, organisation, ledgerType, invoiceCurrencyType, currency.RX_Code);

				if (conf != null)
				{
					var localClientPK = (useLocaClient && !rateConsumer.LocalClientPK.IsEmpty && rateConsumer.LocalClientPK.IsValid) ? rateConsumer.LocalClientPK.ToGuid() : (Guid?)null;
					exchangeRate = GetExchangeRateByRateType(rateConsumer, currency, conf, localClientPK);
				}

				if (exchangeRate.IsEmpty && invoiceCurrencyType != InvoiceCurrencyType.NotApplicable)
				{
					exchangeRate = GetExchangeRate(rateConsumer, currency, organisation, ledgerType, InvoiceCurrencyType.NotApplicable);
				}

#if DEBUG
				if (exchangeRate == 0 && Globals.IsTest && !DisableZeroExchangeRateOverridingAttribute.IsActive)
				{
					exchangeRate = 1m;
				}
#endif
			}
			return Utilities.Round(exchangeRate, GetExchangeRateDecimals(rateConsumer));
		}

		internal static ZDecimal GetTodaysExchangeRate(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable)
		{
			return GetExchangeRateForDate(rateConsumer, currency, organisation, ledgerType, ZDateTime.Now, invoiceCurrencyType);
		}

		internal static ZDecimal GetExchangeRateForDate(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, ZDateTime preferredDate, InvoiceCurrencyType invoiceCurrencyType = InvoiceCurrencyType.NotApplicable, bool useBuyRateAsFallback = false)
		{
			var exchangeRate = ZDecimal.Zero;
			if (currency != null)
			{
				var config = GetExchangeRateConfiguration(rateConsumer, organisation, ledgerType, invoiceCurrencyType, currency.RX_Code);
				if (config != null)
				{
					exchangeRate = GetRate(currency.RX_Code, config.ExchangeRateType, preferredDate, ZInt.Zero, null);

					if (exchangeRate.IsEmpty && invoiceCurrencyType != InvoiceCurrencyType.NotApplicable)
					{
						exchangeRate = GetExchangeRateForDate(rateConsumer, currency, organisation, ledgerType, preferredDate, InvoiceCurrencyType.NotApplicable, useBuyRateAsFallback);
					}
				}
				else if (useBuyRateAsFallback)
				{
					return GetBuyExchangeRateForDate(rateConsumer, currency, preferredDate);
				}
			}
			return Utilities.Round(exchangeRate, GetExchangeRateDecimals(rateConsumer));
		}

		internal static ZDecimal GetBuyExchangeRateForDate(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, ZDateTime preferredDate)
		{
			var exchangeRate = ZDecimal.Zero;
			if (currency != null)
			{
				exchangeRate = GetRate(currency.RX_Code, ExchangeRateType.Buy, preferredDate, ZInt.Zero, null);
			}
			return Utilities.Round(exchangeRate, GetExchangeRateDecimals(rateConsumer));
		}

		#endregion

		public static ZDate GetPreferredExchangeRateDate(IAccExchangeRateConfigurationRateConsumer rateConsumer, OrgHeader org, ZString currencyCode, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType)
		{
			var exRateConfig = GetExchangeRateConfiguration(rateConsumer, org, ledgerType, invoiceCurrencyType, currencyCode);

			return exRateConfig != null
				? ObjectFactory.Get<IAccExchangeRateConfigurationsHelper>().GetExchangeRateDate(rateConsumer, exRateConfig.Preference).Date
				: ZDate.Invalid;
		}

		#region Exchange Rate Based On Invoice Posting Option

		internal static ZDecimal GetExchangeRateBasedOnInvoicePostingOption(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, bool isLocalCurrency, ZGuid companyPK, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate, bool useBuyRateAsFallback = false)
		{
			var exchangeRate = ZDecimal.Zero;
			if (currency != null)
			{
				var invoiceCurrencyType = GetInvoiceCurrencyType(rateConsumer.Company, ledgerType, isLocalCurrency ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign);
				var config = GetExchangeRateConfiguration(rateConsumer, organisation, ledgerType, invoiceCurrencyType, currency.RX_Code);
				if (config != null)
				{
					exchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(currency.RX_Code, isLocalCurrency, companyPK, config.ExchangeRateType, ledgerType, invoiceDate, postDate, taxDate);
				}
				else if (useBuyRateAsFallback)
				{
					return GetBuyExchangeRateBasedOnInvoicePostingOption(rateConsumer, currency, isLocalCurrency, companyPK, ledgerType, invoiceDate, postDate, taxDate);
				}
			}
			return Utilities.Round(exchangeRate, GetExchangeRateDecimals(rateConsumer));
		}

		internal static ZDecimal GetBuyExchangeRateBasedOnInvoicePostingOption(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, bool isLocalCurrency, ZGuid companyPK, ExchangeRateValidLedgerEnum ledgerType, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
		{
			var exchangeRate = ZDecimal.Zero;
			if (currency != null)
			{
				exchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(currency.RX_Code, isLocalCurrency, companyPK, ExchangeRateType.Buy, ledgerType, invoiceDate, postDate, taxDate);
			}
			return Utilities.Round(exchangeRate, GetExchangeRateDecimals(rateConsumer));
		}

		internal static bool IsExchangeRateTypeNotEqualToSpecified(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, ref ZString rateType)
		{
			var result = false;
			if (currency != null)
			{
				var conf = GetCompanyLevelExchangeRateConfiguration(rateConsumer, currency.RX_Code);
				if (conf != null)
				{
					var rateDate = ObjectFactory.Get<IAccExchangeRateConfigurationsHelper>().GetExchangeRateDate(rateConsumer, conf.Preference);
					var localClientPK = (!rateConsumer.LocalClientPK.IsEmpty && rateConsumer.LocalClientPK.IsValid) ? rateConsumer.LocalClientPK.ToGuid() : (Guid?)null;
					var includeExpired = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
					result = rateDate.IsValid && Env.CurrentCompany.ExchangeRate.IsExchangeRateTypeNotEqualToSpecified(currency.RX_Code, conf.ExchangeRateTypeAsZString, rateDate.ToDateTime().AddDays(conf.OffSet), includeExpired, localClientPK);
					rateType = conf.ExchangeRateTypeAsZString;
				}
			}
			return result;
		}

		#endregion

		#region Helpers

		public static AccExchangeRateConfigurationWrapper GetExchangeRateConfiguration(this IAccExchangeRateConfigurationRateConsumer rateConsumer, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType, ZString currencyCode)
		{
			if (rateConsumer != null)
			{
				var rateConfigurationCollection = GetAccExchangeRateConfigurationCollection(rateConsumer.Company, organisation, ledgerType);
				return rateConfigurationCollection?.GetRecord(ledgerType.ToCode(), rateConsumer.JobType, rateConsumer.Direction, rateConsumer.TransportMode, invoiceCurrencyType.ToCode(), currencyCode, rateConsumer);
			}
			return null;
		}

		public static InvoiceCurrencyType GetInvoiceCurrencyType(GlbCompany company, ExchangeRateValidLedgerEnum ledgerType, string currencyCode)
		{
			var invoiceCurrencyType = currencyCode == (company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign;
			return GetInvoiceCurrencyType(company, ledgerType, invoiceCurrencyType);
		}

		public static InvoiceCurrencyType GetInvoiceCurrencyType(GlbCompany company, ExchangeRateValidLedgerEnum ledgerType, BaseCharge charge)
		{
			if ((ledgerType == ExchangeRateValidLedgerEnum.AP && charge.JR_OH_CostAccount.IsEmpty || charge.JR_RX_NKCostCurrency.IsEmpty)
				|| ledgerType == ExchangeRateValidLedgerEnum.AR && charge.JR_OH_SellAccount.IsEmpty ||  charge.JR_RX_NKSellCurrency.IsEmpty || charge.JR_InvoiceType.IsEmpty)
			{
				return InvoiceCurrencyType.NotApplicable;
			}

			var invoiceCurrencyType = ExchangeRateCalculator.IsInLocalInvoiceCurrencyForPosting(charge, ledgerType) ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign;
			return GetInvoiceCurrencyType(company, ledgerType, invoiceCurrencyType);
		}

		public static InvoiceCurrencyType GetInvoiceCurrencyType(GlbCompany company, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType)
		{
			var companyPK = company?.PK ?? GlbCompany.CurrentCompany.PK;
			return (ledgerType == ExchangeRateValidLedgerEnum.AR && AccountingMasterFilesRegistry.Instance.EnableInvoiceCurrencyType.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
				? invoiceCurrencyType
				: InvoiceCurrencyType.NotApplicable;
		}

		static AccExchangeRateConfigurationWrapper GetExchangeRateConfigurationForJobWithConsolDirectionAndConsolTransportMode(IAccExchangeRateConfigurationRateConsumer jobRateConsumerForJobType,
			IAccExchangeRateConfigurationRateConsumer consolCostRateConsumerForDirectionAndTransportMode, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType, InvoiceCurrencyType invoiceCurrencyType, ZString currencyCode)
		{
			if (jobRateConsumerForJobType != null && consolCostRateConsumerForDirectionAndTransportMode != null)
			{
				var rateConfigurationCollection = GetAccExchangeRateConfigurationCollection(jobRateConsumerForJobType.Company, organisation, ledgerType);
				return rateConfigurationCollection?.GetRecord(ledgerType.ToCode(), jobRateConsumerForJobType.JobType, consolCostRateConsumerForDirectionAndTransportMode.Direction, consolCostRateConsumerForDirectionAndTransportMode.TransportMode, invoiceCurrencyType.ToCode(), currencyCode, consolCostRateConsumerForDirectionAndTransportMode);
			}
			return null;
		}

		static ZString GetCompanyLevelPreference(IAccExchangeRateConfigurationRateConsumer rateConsumer) => GetCompanyLevelExchangeRateConfiguration(rateConsumer, string.Empty)?.Preference ?? ZString.Empty;

		static ZString GetCompanyLevelPreferenceForJobWithConsolDirectionAndConsolTransportMode(IAccExchangeRateConfigurationRateConsumer jobRateConsumerForJobType, IAccExchangeRateConfigurationRateConsumer consolCostRateConsumerForDirectionAndTransportMode, ZString currencyCode)
			=> GetCompanyLevelExchangeRateConfigurationForJobWithConsolDirectionAndConsolTransportMode(jobRateConsumerForJobType, consolCostRateConsumerForDirectionAndTransportMode, currencyCode)?.Preference ?? ZString.Empty;

		static AccExchangeRateConfigurationWrapper GetCompanyLevelExchangeRateConfiguration(IAccExchangeRateConfigurationRateConsumer rateConsumer, ZString currencyCode) => GetExchangeRateConfiguration(rateConsumer, null, ExchangeRateValidLedgerEnum.None, InvoiceCurrencyType.NotApplicable, currencyCode);

		static AccExchangeRateConfigurationWrapper GetCompanyLevelExchangeRateConfigurationForJobWithConsolDirectionAndConsolTransportMode(IAccExchangeRateConfigurationRateConsumer jobRateConsumerForJobType, IAccExchangeRateConfigurationRateConsumer consolCostRateConsumerForDirectionAndTransportMode, ZString currencyCode)
			=> GetExchangeRateConfigurationForJobWithConsolDirectionAndConsolTransportMode(jobRateConsumerForJobType, consolCostRateConsumerForDirectionAndTransportMode, null, ExchangeRateValidLedgerEnum.None, InvoiceCurrencyType.NotApplicable, currencyCode);

		static AccExchangeRateConfigurationCollection GetAccExchangeRateConfigurationCollection(GlbCompany company, OrgHeader organisation, ExchangeRateValidLedgerEnum ledgerType)
		{
			AccExchangeRateConfigurationCollection rateConfigurationCollection;
			if (organisation == null || ledgerType == ExchangeRateValidLedgerEnum.None)
			{
				rateConfigurationCollection = company?.AccExchangeRateConfigurations;
			}
			else
			{
				rateConfigurationCollection = ledgerType == ExchangeRateValidLedgerEnum.AR ? organisation.CompanyData.AccARExchangeRateConfigurations : organisation.CompanyData.AccAPExchangeRateConfigurations;
			}
			return rateConfigurationCollection;
		}

		static ZDecimal GetExchangeRateByRateType(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, AccExchangeRateConfigurationWrapper configuration, Guid? localClientPK)
		{
			var exchangeRate = ZDecimal.Zero;
			if (configuration.Preference == Core.Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate)
			{
				exchangeRate = rateConsumer.GetExchangeRateForConsolExchangeRatePreference(currency);
			}
			if (exchangeRate.IsEmpty)
			{
				exchangeRate = GetExchangeRateByRateTypeCore(rateConsumer, currency, configuration, localClientPK);
			}

			return exchangeRate;
		}

		static ZDecimal GetExchangeRateByRateTypeCore(IAccExchangeRateConfigurationRateConsumer rateConsumer, RefCurrency currency, AccExchangeRateConfigurationWrapper configuration, Guid? localClientPK)
		{
			return GetRate(currency.RX_Code, configuration.ExchangeRateType, ObjectFactory.Get<IAccExchangeRateConfigurationsHelper>().GetExchangeRateDate(rateConsumer, configuration.Preference), configuration.OffSet, localClientPK);
		}

		static ZDecimal GetRate(ZString currencyCode, ExchangeRateType rateType, ZDateTime rateDate, ZInt offset, Guid? localClientPK)
		{
			return rateDate.IsValid ? ExchangeRateCalculator.GetRate(currencyCode, rateType, rateDate.ToDateTime().AddDays(offset), localClientPK) : 0;
		}

		static ZInt GetExchangeRateDecimals(IAccExchangeRateConfigurationRateConsumer rateConsumer) => rateConsumer.Company?.ExchangeRateDecimalPlaces ?? GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

	}
}
