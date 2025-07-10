using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class UpdateExchangeRatesHelper : IUpdateExchangeRates
	{
		#region UpdateExchangeRates

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not necessary to translate")]
		public void UpdateExchangeRates(BusinessObject target, Converter<BusinessObject, IExchangeRateSource> sourceProvider, object actionLog)
		{
			var log = actionLog as IOperationalActionSectionLog;
			if (target == null || sourceProvider == null || log == null)
			{
				return;
			}

			IJobInvoicingPlugIn parent = (IJobInvoicingPlugIn)target;
			Job job = new Job.Loader(parent).Load(true, false);
			IExchangeRateSource rateSource;

			if (job == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					(NoResString)"{0} does not have a job",
					ActionMethodsHelper.TargetReference(target));
			}
			else if (job.ExchangeRates.Count == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					(NoResString)"{0} does not have any exchange rates to update",
					ActionMethodsHelper.TargetReference(target));
			}
			else if ((rateSource = sourceProvider(target)) == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					"{0} could not be updated because it does not have an appropriate exchange rates source",
					ActionMethodsHelper.TargetReference(target));
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
					"{0} exchange rates were updated from {1}:",
					ActionMethodsHelper.TargetReference(target),
					ActionMethodsHelper.ExRateProviderReference(rateSource));

				ZBool isLoaded = false;
				foreach (ExchangeRate rate in job.ExchangeRates.ToArray())
				{
					RefCurrency currency = rate.RateCurrency;
					if (currency == null)
					{
						continue;
					}

					var newRate = rateSource.GetExchangeRate(currency.RX_Code, rate.JF_OH_Org, rate.OrgType.ToLedger());

					if (newRate == null)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
							"  \u2022 Exchange rate for '{0}' does not exist in {1}",
							currency.RX_Code,
							ActionMethodsHelper.ExRateProviderReference(rateSource));
					}
					else if (rate.JF_BaseRate != newRate.Value)
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							"  \u2022 Updated exchange rate for '{0}' from {1:0.000} to {2:0.000}",
							currency.RX_Code, rate.JF_BaseRate, newRate);

						if (!isLoaded)
						{
							_ = job.Charges;
							isLoaded = true;
						}
						job.InitializeParentFromGenericJobWithSettingDefaults();
						rate.JF_BaseRate = newRate.Value;
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational,
							"  \u2022 Exchange rate for '{0}' remains unchanged at {1:0.000}",
							currency.RX_Code, rate.JF_BaseRate);
					}
				}
			}
		}

		#endregion
	}
}
