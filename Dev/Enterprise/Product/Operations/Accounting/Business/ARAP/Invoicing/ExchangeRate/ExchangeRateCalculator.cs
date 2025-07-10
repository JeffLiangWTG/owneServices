using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ExchangeRateCalculator
	{
		public static ZDecimal GetRate(ZString currencyNK, ExchangeRateType exchangeRateType, DateTime rateValidAtDate, Guid? localClientPK = null)
		{
			ZDecimal rate = 0m;
			if (AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value)
			{
				rate = Env.CurrentCompany.ExchangeRate.GetRateIncludingExpired(currencyNK, exchangeRateType, rateValidAtDate, localClientPK);
			}
			else
			{
				rate = Env.CurrentCompany.ExchangeRate.GetRate(currencyNK, exchangeRateType, rateValidAtDate, localClientPK);
			}

			return rate;
		}

		public static bool IsExRateOptionApplicable(ExchangeRateValidLedgerEnum ledger, bool isLocalInvoiceCurrency, ZGuid companyPK, params string[] expectedExchangeRateOptions)
		{
			return (!expectedExchangeRateOptions.Any() && GetExchangeRateOption(ledger, isLocalInvoiceCurrency, companyPK) != ExRateOption.Default.Code)
				|| expectedExchangeRateOptions.Any(x => GetExchangeRateOption(ledger, isLocalInvoiceCurrency, companyPK) == x);
		}

		public static ZDecimal GetOverrideExchangeRate(ZString currency, bool isLocalInvoiceCurrency, ZGuid companyPK, ExchangeRateType exchangeRateType, ExchangeRateValidLedgerEnum ledger, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
		{
			var rateValidAtDate = GetExchangeRateDate(ledger, isLocalInvoiceCurrency, companyPK, invoiceDate, postDate, taxDate);

			if (!rateValidAtDate.IsValid)
			{
				return 0m;
			}

			return GetRate(currency, exchangeRateType, rateValidAtDate.ToDateTime());
		}

		public static ZDateTime GetExchangeRateDate(InvoicingBase invoice)
			=> GetExchangeRateDate(ExchangeRateEnumsExtensions.GetLedgerFromCode(invoice.AH_Ledger), invoice.IsLocalCurrencyTransaction, invoice.AH_GC, invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);

		public static ZDateTime GetExchangeRateDate(ExchangeRateValidLedgerEnum ledger, bool isLocalInvoiceCurrency, ZGuid companyPK, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate, string exchangeRateOption = "")
		{
			ZDateTime rateValidAtDate;
			exchangeRateOption = exchangeRateOption.IsNullOrEmpty() ? GetExchangeRateOption(ledger, isLocalInvoiceCurrency, companyPK) : exchangeRateOption;

			if (exchangeRateOption == ExRateOption.Default.Code)
			{
				return ZDateTime.Now;
			}

			if (exchangeRateOption == ExRateOption.TodayExchangeRate.Code)
			{
				rateValidAtDate = ZDateTime.Now;
			}
			else if (exchangeRateOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code)
			{
				rateValidAtDate = invoiceDate;
			}
			else if (exchangeRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code)
			{
				rateValidAtDate = postDate;
			}
			else if (exchangeRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code)
			{
				rateValidAtDate = GetEarliestOfInvoiceOrTaxDate(invoiceDate, taxDate);
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(exchangeRateOption), "ARAPInvoicePostingExchangeRateOption");
			}

			var offset = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOptionOffSet(ledger, isLocalInvoiceCurrency, companyPK);
			return rateValidAtDate.IsValid && offset != 0 ? rateValidAtDate.AddDays(offset) : rateValidAtDate;
		}

		static ZDateTime GetEarliestOfInvoiceOrTaxDate(ZDateTime invoiceDate, ZDateTime taxDate)
			=> !taxDate.IsValid || invoiceDate < taxDate ? invoiceDate : taxDate;

		public static string CheckInvoiceExchangeRate(InvoicingBase invoice)
		{
			var errorMessage = ZString.Empty;

			var localCurrency = invoice.Company.GC_RX_NKLocalCurrency;
			if (!invoice.AH_RX_NKTransactionCurrency.Equals(localCurrency))
			{
				if (invoice.AH_ExchangeRate.IsEmpty)
				{
					errorMessage = GetZeroExchangeRateErrorMessage(invoice.AH_RX_NKTransactionCurrency, invoice.IsLocalCurrencyTransaction, invoice.AH_GC, invoice.GetExRateLedger(), invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					var linesWithEmptyExchangeRate = invoice.Lines.Cast<InvoicingLineBase>().Where(x => !x.AL_RX_NKTransactionCurrency.Equals(localCurrency) && x.AL_ExchangeRate.IsEmpty).ToArray();
					if (linesWithEmptyExchangeRate.Any())
					{
						var lineWithEmptyExchangeRate = linesWithEmptyExchangeRate.First();
						errorMessage = GetZeroExchangeRateErrorMessage(lineWithEmptyExchangeRate.AL_RX_NKTransactionCurrency, false, lineWithEmptyExchangeRate.AL_GC, invoice.GetExRateLedger(), invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);
					}
				}
			}
			else
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					var chargeQuery = new ZQuery(JobChargeSchema.JR_AL_ARLine, invoice.Lines.Select(x => x.PK));
					chargeQuery.AddToFilter(JobChargeSchema.JR_RX_NKSellCurrency, SQLComparisonOperator.NotEqual, localCurrency);
					chargeQuery.AddToFilter(JobChargeSchema.JR_OSSellExRate, 0M);
					chargeQuery.FetchOnlyFromLocalCache = true;
					var osCurrencyChargesWithZeroExchangeRate = invoice.Factory.Load<Charge>(chargeQuery).Select(x => x.JR_RX_NKSellCurrency).Distinct().ToArray();

					if (osCurrencyChargesWithZeroExchangeRate.Any())
					{
						errorMessage = GetZeroExchangeRateErrorMessage(osCurrencyChargesWithZeroExchangeRate.First(), false, invoice.AH_GC, ExchangeRateValidLedgerEnum.AR, invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(errorMessage))
			{
				return Res.GetString("484d1cc3-3479-4fe7-90be-bdc5b095bdd6", @"{0} {1} number {2}
{3}",
					invoice.AH_Ledger,
					invoice.AH_TransactionType == TransactionTypes.Invoice ? Res.GetString("9660438e-c1d4-4501-bdca-4d86873dc107", "Invoice") : Res.GetString("fc15acd5-d20f-4440-ae57-db0a9b56be83", "Credit Note"),
					invoice.AH_TransactionNum,
					errorMessage);
			}

			return string.Empty;
		}

		public static string CheckInvoiceExchangeRate(PeriodicInvoiceBase invoice)
		{
			var result = CheckExchangeRate(invoice.CurrencyNK, invoice.IsLocalCurrency, GlbCompany.CurrentCompany, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AR, invoice.InvoiceDate, invoice.PostDate, invoice.InvoiceDate); //No tax date available for periodic invoice
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public static void UpdateChargesAndLinesExchangeRateForBackDating(InvoicingBase invoice, BusinessObjectFactory transactionFactory)
		{
			var registryValue = GetExchangeRateOption(invoice);
			if (registryValue == ExRateOption.Default.Code)
			{
				return;
			}

			var localCurrency = invoice.Company.GC_RX_NKLocalCurrency;
			bool isARInvoice = invoice.AH_Ledger == LedgerTypes.AccountsReceivable;

			var lines = invoice.Lines.Cast<InvoicingLineBase>().ToDictionary(x => x.PK, x => x);

			Charge[] charges;
			if (isARInvoice)
			{
				var chargeQuery = new ZQuery(JobChargeSchema.JR_AL_ARLine, invoice.Lines.Select(x => x.PK));
				charges = transactionFactory.Load<Charge>(chargeQuery);
			}
			else
			{
				var chargeQuery = new ZQuery(JobChargeSchema.JR_AL_APLine, invoice.Lines.Select(x => x.PK));
				charges = transactionFactory.Load<Charge>(chargeQuery);
			}

			var chargesAndLines = charges.Select(x =>
			{
				if (isARInvoice)
				{
					return new { Charge = x, ChargeCurrency = x.BillInInvoiceCurrencyWithLocalSellCurrency ? x.JR_RX_NKSellInvoiceCurrency : x.JR_RX_NKSellCurrency, Line = lines[x.JR_AL_ARLine] };
				}
				else
				{
					return new { Charge = x, ChargeCurrency = x.JR_RX_NKCostCurrency, Line = lines[x.JR_AL_APLine] };
				}
			}
			).ToArray();

			//get charges and lines/consol costs to be unlinked
			var unlinkedChargesAndLines = chargesAndLines.Where(x => x.ChargeCurrency != localCurrency).ToArray();

			try //unlink
			{
				foreach (var charge in unlinkedChargesAndLines)
				{
					if (isARInvoice)
					{
						charge.Charge.ClearRevenueLinkOnlyTemporary();
						charge.Charge.SetContext(BusinessContext.PostingReceivableCharges);
					}
					else
					{
						charge.Charge.ClearCostLinkOnlyTemporary();
					}
				}

				//update rate if the charge in os currency.
				var chargesGroupedByJobAndChargeCurrency = unlinkedChargesAndLines.Where(x => x.Charge.InvoicingJob != null)
					.GroupBy(x => new
					{
						InvoicingJob = x.Charge.InvoicingJob,
						ChargeCurrency = x.ChargeCurrency,
						OrgPk = isARInvoice ? x.Charge.JR_OH_SellAccount : x.Charge.JR_OH_CostAccount,
					}, x => new
					{
						x.Charge,
						x.Line
					}).ToArray();

				foreach (var group in chargesGroupedByJobAndChargeCurrency)
				{
					var currencyCode = group.Key.ChargeCurrency;
					var useJobExRateFlag = group.FirstOrDefault()?.Line?.InvoiceBase?.UseJobExchangeRate ?? false;
					var ledger = isARInvoice ? ExchangeRateValidLedgerEnum.AR : ExchangeRateValidLedgerEnum.AP;
					var rateExpected = GetExpectedRate(group.Key.InvoicingJob, useJobExRateFlag, currencyCode, invoice.IsLocalCurrencyTransaction, ledger, invoice.Header, invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);
					group.Key.InvoicingJob.UpdateBaseExchangeRate(currencyCode, ledger, rateExpected, group.Select(x => x.Charge));

					if (!isARInvoice)
					{
						var consolCosts = group.Select(x => x.Charge).Where(x => x.JR_IsApportioned && x.ParentConsolCost != null).Select(x => x.ParentConsolCost).ToArray();
						UpdateJobConsolCostsExRates(consolCosts, rateExpected);
					}

					if (invoice.AH_RX_NKTransactionCurrency == currencyCode && invoice.AH_ExchangeRate != rateExpected)
					{
						invoice.AH_ExchangeRate = rateExpected;
					}
				}

				if (isARInvoice)
				{
					var chargesWithSellInvoiceCurrencyAndLines = chargesAndLines
						.Where(x => x.Charge.InvoicingJob != null && x.Charge.BillInInvoiceCurrency && x.Charge.JR_RX_NKSellInvoiceCurrency == x.Line.AL_RX_NKTransactionCurrency)
						.ToArray();

					foreach (var chargeAndLine in chargesWithSellInvoiceCurrencyAndLines)
					{
						var currencyCode = chargeAndLine.Line.AL_RX_NKTransactionCurrency;
						var useJobExRateFlag = chargeAndLine.Line?.InvoiceBase?.UseJobExchangeRate ?? false;
						var rateExpected = GetExpectedRate(chargeAndLine.Line.InvoicingJob, useJobExRateFlag, currencyCode, invoice.IsLocalCurrencyTransaction, invoice.GetExRateLedger(), invoice.Header, invoice.AH_InvoiceDate, invoice.AH_PostDate, invoice.InvoiceTaxDate);
						chargeAndLine.Line.AL_ExchangeRate = rateExpected;
						if (invoice.AH_RX_NKTransactionCurrency == currencyCode && invoice.AH_ExchangeRate != rateExpected)
						{
							invoice.AH_ExchangeRate = rateExpected;
						}
					}
				}
			}
			finally
			{
				if (unlinkedChargesAndLines.IsNullOrEmpty())
				{
					chargesAndLines.ForEach(x =>
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(transactionFactory).AddInfoWhenAllowed(x.Charge.PK, CriticalValidationInfoCollectorServiceKeyType.UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty, () =>
						{
							return System.FormattableString.Invariant($"LocalCurrency = {localCurrency}, Charge BillInInvoiceCurrencyWithLocalSellCurrency = {x.Charge.BillInInvoiceCurrencyWithLocalSellCurrency}, JR_RX_NKSellInvoiceCurrency = {x.Charge.JR_RX_NKSellInvoiceCurrency}, JR_RX_NKSellCurrency = {x.Charge.JR_RX_NKSellCurrency}, ChargeCompanyLocalCurrency = {x.Charge.Company?.GC_RX_NKLocalCurrency}, JR_InvoiceType = {x.Charge.JR_InvoiceType}");
						});
					});
				}

				foreach (var chargeAndLine in unlinkedChargesAndLines) //relink & set invoice lines' exchange rate, re-sync the lines' amount.
				{
					if (isARInvoice)
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(transactionFactory).AddInfoWhenAllowed(chargeAndLine.Charge.PK, CriticalValidationInfoCollectorServiceKeyType.SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating, () =>
						{
							if (chargeAndLine.Charge.JR_OSSellExRate != chargeAndLine.Line.AL_ExchangeRate)
							{
								var info = new ZStringBuilder();
								info.AppendLine(System.FormattableString.Invariant($"ChargeCurrency = {chargeAndLine.ChargeCurrency}, Line Currency = {chargeAndLine.Line.AL_RX_NKTransactionCurrency}, BillInInvoiceCurrencyWithLocalSellCurrency = {chargeAndLine.Charge.BillInInvoiceCurrencyWithLocalSellCurrency}, Charge Exchange Rate = {chargeAndLine.Charge.JR_OSSellExRate}, Line Exchange Rate = {chargeAndLine.Line.AL_ExchangeRate}"));
								return info.ToString();
							}

							return null;
						});

						new ChargePoster(transactionFactory).CopyExchangeRateAndAmount(chargeAndLine.Line, chargeAndLine.Charge, chargeAndLine.Line.AL_RX_NKTransactionCurrency,
							chargeAndLine.ChargeCurrency == chargeAndLine.Line.AL_RX_NKTransactionCurrency && !chargeAndLine.Charge.BillInInvoiceCurrencyWithLocalSellCurrency ?
							chargeAndLine.Charge.JR_OSSellExRate : chargeAndLine.Line.AL_ExchangeRate);
						chargeAndLine.Charge.JR_AL_ARLine = chargeAndLine.Line.PK;
						chargeAndLine.Charge.RemoveContext(BusinessContext.PostingReceivableCharges);
					}
					else
					{
						chargeAndLine.Line.CopyExchangeRateAndAmount(chargeAndLine.Charge);
						chargeAndLine.Charge.JR_AL_APLine = chargeAndLine.Line.PK;
					}
				}
			}
		}

		public static string CheckExchangeRate(Job job, ZString currency, bool isLocalInvoiceCurrency, ExchangeRateValidLedgerEnum ledger, ZGuid orgPK, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
		{
			var result = ZString.Empty;
			if (currency != job.Company.GC_RX_NKLocalCurrency && IsExRateOptionApplicable(ledger, isLocalInvoiceCurrency, job.JH_GC))
			{
				var organisation = job.Factory.Load<OrgHeader>(orgPK);
				if (GetExpectedRate(job, true, currency, isLocalInvoiceCurrency, ledger, organisation, invoiceDate, postDate, taxDate) == 0)
				{
					result = GetZeroExchangeRateErrorMessage(currency, isLocalInvoiceCurrency, job.JH_GC, ledger, invoiceDate, postDate, taxDate);
				}
			}
			return result;
		}

		static ZString GetZeroExchangeRateErrorMessage(ZString currency, bool isLocalInvoiceCurrency, ZGuid companyPK, ExchangeRateValidLedgerEnum ledger, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate, string exchangeRateOption = "")
		{
			exchangeRateOption = exchangeRateOption.IsNullOrEmpty() ? GetExchangeRateOption(ledger, isLocalInvoiceCurrency, companyPK) : exchangeRateOption;
			var exchangeRateDate = GetExchangeRateDate(ledger, isLocalInvoiceCurrency, companyPK, invoiceDate, postDate, taxDate, exchangeRateOption);
			var registryCaption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ledger);
			var exchangeRateOptionDescription = ExRateOption.CodeList.GetDescriptionFromCode(exchangeRateOption);

			return Res.GetString("3c07a4b1-a436-4166-b1ac-a2dfb7e9ee6d", @"The ""{0}"" has been set to ""{1}"".
But the {2} exchange rate is not set for the date {3}. Please check your data and try again.",
					registryCaption,
					exchangeRateOptionDescription,
					currency,
					exchangeRateDate.ToShortDateString());
		}

		public static string CheckExchangeRate(ZString currency, bool isLocalInvoiceCurrency, GlbCompany company, ExchangeRateType exchangeRateType, ExchangeRateValidLedgerEnum ledger, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
		{
			var result = "";

			if (currency != company.GC_RX_NKLocalCurrency && IsExRateOptionApplicable(ledger, isLocalInvoiceCurrency, company.PK)
				&& 0 == GetOverrideExchangeRate(currency, isLocalInvoiceCurrency, company.PK, exchangeRateType, ledger, invoiceDate, postDate, taxDate))
			{
				result = GetZeroExchangeRateErrorMessage(currency, isLocalInvoiceCurrency, company.PK, ledger, invoiceDate, postDate, taxDate);
			}
			return result;
		}

		public static readonly INotificationType MissingExchangeRate = new NotificationType((NoResString)"Missing Exchange Rate", 100, true, "MissingExchangeRate");

		public static readonly INotificationType OverridenExchangeRate = new NotificationType((NoResString)"Overridden Exchange Rate", 100, true, "OverridenExchangeRate");

		public static INotification CheckExchangeRate(ZString currency, bool isLocalInvoiceCurrency, GlbCompany company, ExchangeRateType exchangeRateType, ExchangeRateValidLedgerEnum ledger, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate, ZDecimal inputRate)
		{
			INotification result = null;

			if (currency != company.GC_RX_NKLocalCurrency && IsExRateOptionApplicable(ledger, isLocalInvoiceCurrency, company.PK))
			{
				var registryValue = GetExchangeRateOption(ledger, isLocalInvoiceCurrency, company.PK);

				var rateExpected = GetOverrideExchangeRate(currency, isLocalInvoiceCurrency, company.PK, exchangeRateType, ledger, invoiceDate, postDate, taxDate);
				if (rateExpected == 0)
				{
					var message = GetZeroExchangeRateErrorMessage(currency, isLocalInvoiceCurrency, company.PK, ledger, invoiceDate, postDate, taxDate, registryValue);
					result = new Notification(MissingExchangeRate, message);
				}
				else if (rateExpected != inputRate)
				{
					var registryCaption = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateRegistryItemCaption(ledger);
					var exchangeRateOption = ExRateOption.CodeList.GetDescriptionFromCode(registryValue);
					var rateDecimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
					var message = Res.GetString("3e224f05-f1de-4044-8873-b33c8d395855", @"The ""{0}"" has been set to ""{1}"".
With this option, the exchange rate should not be changed manually. System expected {2} rate but {3} was entered.", registryCaption, exchangeRateOption, rateExpected.ToString(rateDecimals), inputRate.ToString(rateDecimals));
					result = new Notification(OverridenExchangeRate, message);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public static void UpdateJobsExchangeRatesToTheLatestExRates(params InvoicingBase[] invoices)
		{
			if (!invoices.Any())
			{
				return;
			}
			else if (invoices.Length > 1 && invoices.GroupBy(x => x.GetExRateLedger()).Count() > 1)
			{
				throw new ArgumentException("All the invoices should have the same type of ledger");
			}

			var ledger = invoices.First().GetExRateLedger();

			foreach (var invoicesGroupedByInvoiceCurrencyType in invoices.GroupBy(x => new { x.IsLocalCurrencyTransaction }, x => x).ToArray())
			{
				var registryValue = GetExchangeRateOption(ledger, invoicesGroupedByInvoiceCurrencyType.Key.IsLocalCurrencyTransaction, invoicesGroupedByInvoiceCurrencyType.First().AH_GC);
				if (registryValue == ExRateOption.Default.Code)
				{
					continue;
				}
				var offset = AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOptionOffSet(ledger, invoicesGroupedByInvoiceCurrencyType.Key.IsLocalCurrencyTransaction, invoicesGroupedByInvoiceCurrencyType.First().AH_GC);

				var linesWithNonLocalCurrency = GetNonLocalCurrencyForLines(invoicesGroupedByInvoiceCurrencyType.SelectMany(x => x.Lines.Cast<InvoicingLineBase>()));

				var invoicesGroupedByJobOrgCurrency = linesWithNonLocalCurrency
								.GroupBy(x => new { x.Line.InvoicingJob, x.Line.InvoiceBase.AH_OH, x.Currency }, x => x.Line.InvoiceBase)
								.ToArray();

				var invoicesGroupedByJobCurrency = linesWithNonLocalCurrency
								.GroupBy(x => new { x.Line.InvoicingJob, x.Currency }, x => x.Line.InvoiceBase)
								.ToArray();

				if (invoicesGroupedByJobOrgCurrency.Any())
				{
					if (registryValue == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
						|| registryValue == ExRateOption.ExchangeRateBasedOnPostDate.Code
						|| registryValue == ExRateOption.EarliestOfInvoiceOrTaxDate.Code)
					{
						var jobPKs = invoicesGroupedByJobOrgCurrency.Select(x => x.Key.InvoicingJob.PK).Distinct().ToArray();
						var maxDatesFromDb = GetMaxDatesPerJobsFromAllPostedTransactions(ledger, jobPKs, registryValue, offset);

						foreach (var group in invoicesGroupedByJobCurrency)
						{
							UpdateJobExchangeRateForInvoicesGroupedByJobCurrency(group, group.Key.InvoicingJob, group.Key.Currency, maxDatesFromDb, ledger, registryValue, offset);
						}
					}
					else // TodayExchangeRate
					{
						foreach (var group in invoicesGroupedByJobOrgCurrency)
						{
							var useJobExRateFlag = group.FirstOrDefault().UseJobExchangeRate;
							var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(invoicesGroupedByInvoiceCurrencyType.First().Company, ledger, invoicesGroupedByInvoiceCurrencyType.Key.IsLocalCurrencyTransaction ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign);
							var expectedRate = GetExpectedRate(group.Key.InvoicingJob, useJobExRateFlag, group.Key.Currency, invoiceCurrencyType, ledger, group.Key.AH_OH, ZDateTime.Now.AddDays(offset));
							group.Key.InvoicingJob.UpdateBaseExchangeRate(group.Key.Currency, ledger, expectedRate, invoiceCurrencyType: invoiceCurrencyType);
						}
					}
				}
			}
		}

		static ZString GetLineOriginalCurrency(InvoicingLineBase line, string localCurrency)
		{
			var result = line.AL_RX_NKTransactionCurrency;
			var charge = line.RelatedJobCharge;
			if (charge != null)
			{
				if (line.AL_LineType == TransactionLineTypes.Revenue)
				{
					if (charge.BillInLocalCurrency || charge.BillInInvoiceCurrency)
					{
						result = charge.JR_RX_NKSellCurrency;
					}
				}
				else if (line.AL_RX_NKTransactionCurrency == localCurrency)
				{
					result = charge.JR_RX_NKCostCurrency;
				}
			}
			return result;
		}

		static void UpdateJobExchangeRateForInvoicesGroupedByJobCurrency(IEnumerable<InvoicingBase> invoices, Job invoicingJob, ZString currency, Dictionary<(ZGuid, ZString, ZString, ZGuid), ZDateTime> maxDatesFromDb, ExchangeRateValidLedgerEnum ledger, string exRateOptionRegistryValue, int offset)
		{
			var useJobExRate = invoices.First().UseJobExchangeRate;
			var isLocalCurrencyTransaction = invoices.First().IsLocalCurrencyTransaction;
			var invoicesGroupedByOrg = invoices.Where(x => !x.AH_OH.IsEmpty).GroupBy(x => new { x.AH_OH });
			foreach (var group in invoicesGroupedByOrg)
			{
				var maxDate = GetGroupMaxDate(group, exRateOptionRegistryValue, offset);
				UpdateJobExchangeRatesToTheLatestBuyRateFromInvoices(invoicingJob, group.Key.AH_OH, ledger, currency, isLocalCurrencyTransaction, maxDate, useJobExRate, maxDatesFromDb, false);
			}

			var groupMaxDate = GetGroupMaxDate(invoices, exRateOptionRegistryValue, offset);
			UpdateJobExchangeRatesToTheLatestBuyRateFromInvoices(invoicingJob, Guid.Empty, ledger, currency, isLocalCurrencyTransaction, groupMaxDate, useJobExRate, maxDatesFromDb, true);
			UpdateJobExchangeRatesToTheLatestBuyRateFromInvoices(invoicingJob, Guid.Empty, ExchangeRateValidLedgerEnum.None, currency, isLocalCurrencyTransaction, groupMaxDate, useJobExRate, maxDatesFromDb, false);
		}

		static ZDateTime GetGroupMaxDate(IEnumerable<InvoicingBase> invoices, string exRateOptionRegistryValue, int offset)
		{
			var date = exRateOptionRegistryValue == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
				? invoices.Max(x => x.AH_InvoiceDate)
				: exRateOptionRegistryValue == ExRateOption.ExchangeRateBasedOnPostDate.Code
					? invoices.Max(x => x.AH_PostDate)
					: exRateOptionRegistryValue == ExRateOption.EarliestOfInvoiceOrTaxDate.Code
						? invoices.Max(x => GetEarliestOfInvoiceOrTaxDate(x.AH_InvoiceDate, x.InvoiceTaxDate))
						: ZDateTime.Invalid;

			return date.IsValid && offset != 0 ? date.AddDays(offset) : date;
		}

		static void UpdateJobExchangeRatesToTheLatestBuyRateFromInvoices(Job invoicingJob, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger, ZString currency, bool isLocalInvoiceCurrency, ZDateTime maxDate, ZBool useJobExRate, Dictionary<(ZGuid, ZString, ZString, ZGuid), ZDateTime> maxDatesFromDb, bool ignoreNoLedgerExRates)
		{
			var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(invoicingJob.Company, ledger, isLocalInvoiceCurrency ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign);
			var exRateToSet = GetExpectedRate(invoicingJob, useJobExRate, currency, invoiceCurrencyType, ledger, orgPk, maxDate);

			ZDateTime maxDateFromDb;
			ZBool getDateSuccessful;
			if (ledger == ExchangeRateValidLedgerEnum.None)
			{
				var jobCurrencyKeyAR = (invoicingJob.PK, currency, TransactionLineTypes.Revenue, orgPk);
				var jobCurrencyKeyAP = (invoicingJob.PK, currency, TransactionLineTypes.Cost, orgPk);
				getDateSuccessful = maxDatesFromDb.TryGetValue(jobCurrencyKeyAR, out ZDateTime maxDateFromDbAR);
				getDateSuccessful |= maxDatesFromDb.TryGetValue(jobCurrencyKeyAP, out ZDateTime maxDateFromDbAP);
				if (maxDateFromDbAR.IsEmpty)
				{
					maxDateFromDb = maxDateFromDbAP;
				}
				else if (maxDateFromDbAP.IsEmpty)
				{
					maxDateFromDb = maxDateFromDbAR;
				}
				else
				{
					maxDateFromDb = maxDateFromDbAR > maxDateFromDbAP ? maxDateFromDbAR : maxDateFromDbAP;
				}
			}
			else
			{
				var lineType = ledger == ExchangeRateValidLedgerEnum.AR ? TransactionLineTypes.Revenue : TransactionLineTypes.Cost;
				var jobCurrencyKey = (invoicingJob.PK, currency, lineType, orgPk);
				getDateSuccessful = maxDatesFromDb.TryGetValue(jobCurrencyKey, out maxDateFromDb);
			}

			if (getDateSuccessful && maxDateFromDb.Date > maxDate.Date)
			{
				var currentRate = ((IExchangeRateProvider)invoicingJob).GetExchangeRate(currency, orgPk, ledger, invoiceCurrencyType: invoiceCurrencyType);
				if (currentRate != null)
				{
					exRateToSet = currentRate.Rate;
				}
			}

			invoicingJob.UpdateBaseExchangeRate(currency, ledger, exRateToSet, orgPk, ignoreNoLedgerExRates, invoiceCurrencyType);
		}

		static Dictionary<(ZGuid, ZString, ZString, ZGuid), ZDateTime> GetMaxDatesPerJobsFromAllPostedTransactions(ExchangeRateValidLedgerEnum ledger, ZGuid[] jobPKs, string exchangeRateOption, int offset)
		{
			if (!jobPKs.Any())
			{
				throw new ArgumentException("Should be at least one Job passed in 'jobs' array");
			}

			var lineQuery = new ZDBOnlyQuery(typeof(InvoicingLineBase));
			lineQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, jobPKs);
			lineQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, new[] { TransactionLineTypes.Cost, TransactionLineTypes.Revenue });

			var transactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable });
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });
			transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);

			var transactionSubQuery = new ZDBOnlySubQuery(typeof(Invoice), AccTransactionHeaderSchema.PK);
			transactionSubQuery.AddToFilter(transactionQuery);
			lineQuery.AddSubQuery(AccTransactionLinesSchema.AL_AH, transactionSubQuery, JoinCondition.And);

			var factory = new ReadOnlyBusinessObjectFactory();
			var lines = factory.Load<TransactionLine>(lineQuery).Cast<InvoicingLineBase>();
			var linesPKs = lines.Select(x => x.PK);
			factory.AddFetchHint(typeof(JobCharge), new ZQuery(JobChargeSchema.JR_AL_ARLine, linesPKs));
			factory.AddFetchHint(typeof(JobCharge), new ZQuery(JobChargeSchema.JR_AL_APLine, linesPKs));
			factory.AddFetchHint(typeof(AccTransactionHeader), transactionQuery);

			var linesWithNonLocalCurrency = GetNonLocalCurrencyForLines(lines);

			var dates = new Dictionary<(ZGuid JobPK, ZString Currency, ZString LineType, ZGuid OrgPK), ZDateTime>();
			foreach (var lineWithCurrency in linesWithNonLocalCurrency)
			{
				var orgKey = (JobPK: lineWithCurrency.Line.AL_JH, lineWithCurrency.Currency, LineType: lineWithCurrency.Line.AL_LineType, OrgPK: lineWithCurrency.Line.TransactionHeader.AH_OH);
				var noOrgKey = (JobPK: lineWithCurrency.Line.AL_JH, lineWithCurrency.Currency, LineType: lineWithCurrency.Line.AL_LineType, OrgPK: ZGuid.Empty);

				var date = exchangeRateOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
					? lineWithCurrency.Line.TransactionHeader.AH_InvoiceDate : exchangeRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code
					? lineWithCurrency.Line.TransactionHeader.AH_PostDate : exchangeRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code
					? GetEarliestOfInvoiceOrTaxDate(lineWithCurrency.Line.TransactionHeader.AH_InvoiceDate, lineWithCurrency.Line.AL_TaxDate)
				: ZDateTime.Invalid;

				date = date.IsValid && offset != 0 ? date.AddDays(offset) : date;

				foreach (var key in new[] { orgKey, noOrgKey })
				{
					if (!dates.ContainsKey(key))
					{
						dates.Add(key, date);
					}
					else if (dates[key] < date)
					{
						dates[key] = date;
					}
				}
			}

			return dates;
		}

		static (InvoicingLineBase Line, ZString Currency)[] GetNonLocalCurrencyForLines(IEnumerable<InvoicingLineBase> lines)
		{
			var localCurrency = lines.FirstOrDefault()?.Company.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var linesWithOriginalCurrency = lines.Where(y => y.InvoicingJob != null)
				.Select(z =>
				(
					Line: z,
					Currency: GetLineOriginalCurrency(z, localCurrency)
				));

			var linesWithSellInvoiceCurrency = lines.Where(y => y.AL_LineType == TransactionLineTypes.Revenue && y.InvoicingJob != null && y.RelatedJobCharge != null && y.RelatedJobCharge.BillInInvoiceCurrency)
				.Select(z =>
				(
					Line: z,
					Currency: z.RelatedJobCharge.JR_RX_NKSellInvoiceCurrency
				));

			var linesWithNonLocalCurrency = linesWithOriginalCurrency
				.Union(linesWithSellInvoiceCurrency)
				.Where(x => x.Currency != localCurrency)
				.ToArray();

			return linesWithNonLocalCurrency;
		}

		public static void UpdateChargesExchangeRates(string ledgerType, IEnumerable<Charge> charges, ZDateTime postingTime, bool updateJobRatesOnly = false)
		{
			foreach (var charge in charges)
			{
				if (ledgerType == LedgerTypes.AccountsReceivable
					&& charge.JR_AT_SellGSTRate.IsValid
					&& charge.JR_SellTaxDate.IsEmpty
					&& IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR),
					charge.JR_GC, ExRateOption.EarliestOfInvoiceOrTaxDate.Code))
				{
					var taxDate = AccountingUtils.GetChargeTaxDate(charge, charge.JR_Calc_ARInvoiceDate.Date, LedgerTypes.AccountsReceivable);
					charge.JR_SellTaxDate = taxDate;
				}
				else if (ledgerType == LedgerTypes.AccountsPayable
					&& charge.JR_AT_CostGSTRate.IsValid
					&& charge.JR_CostTaxDate.IsEmpty
					&& IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AP),
					charge.JR_GC, ExRateOption.EarliestOfInvoiceOrTaxDate.Code))
				{
					var taxDate = AccountingUtils.GetChargeTaxDate(charge, charge.JR_APInvoiceDate.Date, LedgerTypes.AccountsPayable);
					if (charge.ParentConsolCost == null)
					{
						charge.JR_CostTaxDate = taxDate;
					}
					else
					{
						charge.ParentConsolCost.E6_TaxDate = taxDate;
					}
				}
			}

			UpdateShipmentChargesExchangeRates(charges, ledgerType, postingTime, updateJobRatesOnly);
			UpdateSellInvoiceExchangeRates(charges, ledgerType, postingTime);

			if (ledgerType == LedgerTypes.AccountsPayable)
			{
				UpdateConsolChargesExchangeRates(charges, ledgerType, postingTime);
			}
		}

		static void UpdateShipmentChargesExchangeRates(IEnumerable<Charge> charges, string ledgerType, ZDateTime postingTime, bool updateJobRatesOnly)
		{
			if (string.IsNullOrEmpty(charges.FirstOrDefault()?.Company?.GC_RX_NKLocalCurrency))
			{
				return;
			}

			var localCurrency = charges.First().Company.GC_RX_NKLocalCurrency;
			var ledger = ExchangeRateEnumsExtensions.GetLedgerFromCode(ledgerType);

			var chargesGroupedByJobAndCurrencyGroups = charges.Where(x => ((ledgerType == LedgerTypes.AccountsReceivable) || (ledgerType == LedgerTypes.AccountsPayable && x.ParentConsolCost == null))
					&& x.InvoicingJob != null
					&& x.GetChargeCurrency(ledgerType) != localCurrency)
				.GroupBy(x => new {
					x.InvoicingJob,
					OrgPk = (ledgerType == LedgerTypes.AccountsReceivable ? x.JR_OH_SellAccount : x.JR_OH_CostAccount),
					Currency = x.GetChargeCurrency(ledgerType),
					IsLocalInvoiceCurrency = x.IsInLocalInvoiceCurrencyForPosting(ledger)
				}, x => x);

			foreach (var chargesGroupedByJobAndCurrencyGroup in chargesGroupedByJobAndCurrencyGroups)
			{
				var exRateOption = GetExchangeRateOption(ledger, chargesGroupedByJobAndCurrencyGroup.Key.IsLocalInvoiceCurrency, chargesGroupedByJobAndCurrencyGroup.Key.InvoicingJob.JH_GC);
				if (exRateOption == ExRateOption.Default.Code)
				{
					continue;
				}

				var earliestOfInvoiceOrTaxDateOfAllCharges = chargesGroupedByJobAndCurrencyGroup.Where(x => GetChargeDate(x, ledger, postingTime).IsValid).Min(y => GetChargeDate(y, ledger, postingTime));
				var chargesGroupedByDateGroups = chargesGroupedByJobAndCurrencyGroup.Where(x => GetChargeDate(x, ledger, postingTime).IsValid)
					.GroupBy(x => new
					{
						Date = exRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code ? earliestOfInvoiceOrTaxDateOfAllCharges : GetChargeDate(x, ledger, postingTime)
					}, x => x);

				var currencyCode = chargesGroupedByJobAndCurrencyGroup.Key.Currency;
				foreach (var chargesGroupedByDateGroup in chargesGroupedByDateGroups)
				{
					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(charges.First().Company, ledger, chargesGroupedByJobAndCurrencyGroup.Key.IsLocalInvoiceCurrency ? InvoiceCurrencyType.Local : InvoiceCurrencyType.Foreign);
					var expectedRate = GetExpectedRate(chargesGroupedByJobAndCurrencyGroup.Key.InvoicingJob, true, currencyCode, invoiceCurrencyType, ledger, chargesGroupedByJobAndCurrencyGroup.Key.OrgPk, chargesGroupedByDateGroup.Key.Date);
					if (updateJobRatesOnly)
					{
						chargesGroupedByJobAndCurrencyGroup.Key.InvoicingJob.UpdateBaseExchangeRate(currencyCode, ledger, expectedRate, invoiceCurrencyType: invoiceCurrencyType);
					}
					else
					{
						chargesGroupedByJobAndCurrencyGroup.Key.InvoicingJob.UpdateBaseExchangeRate(currencyCode, ledger, expectedRate, chargesGroupedByJobAndCurrencyGroup);
					}
				}
			}
		}

		static void UpdateConsolChargesExchangeRates(IEnumerable<Charge> charges, string ledgerType, ZDateTime postingTime)
		{
			if (string.IsNullOrEmpty(charges.FirstOrDefault()?.Company?.GC_RX_NKLocalCurrency))
			{
				return;
			}

			var localCurrency = charges.First().Company.GC_RX_NKLocalCurrency;
			var ledger = ExchangeRateEnumsExtensions.GetLedgerFromCode(ledgerType);

			var costsGroupedByConsolCreditorAndCurrencyGroups = charges.Where(x => x.ParentConsolCost != null
					&& x.ParentConsolCost.Consol != null
					&& x.ParentConsolCost.E6_RX_NKCurrency != localCurrency)
				.GroupBy(x => new {
					Consol = x.ParentConsolCost.Consol.PK,
					OrgPk = x.JR_OH_CostAccount,
					Currency = x.ParentConsolCost.E6_RX_NKCurrency
				}, x => x);

			foreach (var costsGroupedByConsolCreditorAndCurrencyGroup in costsGroupedByConsolCreditorAndCurrencyGroups)
			{
				var jobConsolCosts = costsGroupedByConsolCreditorAndCurrencyGroup.Select(x => x.ParentConsolCost);
				var exRateOption = GetExchangeRateOption(ledger, false, jobConsolCosts.First().E6_GC);
				if (exRateOption == ExRateOption.Default.Code)
				{
					continue;
				}

				var earliestOfInvoiceOrTaxDateOfAllCharges = costsGroupedByConsolCreditorAndCurrencyGroup.Where(x => GetChargeDate(x, ledger, postingTime).IsValid).Min(y => GetChargeDate(y, ledger, postingTime));

				var costsGroupedByDateGroups = costsGroupedByConsolCreditorAndCurrencyGroup.Where(x => GetChargeDate(x, ledger, postingTime).IsValid)
					.GroupBy(x => new
					{
						Date = exRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code ? earliestOfInvoiceOrTaxDateOfAllCharges : GetChargeDate(x, ledger, postingTime)
					}, x => x);

				foreach (var costsGroupedByDateGroup in costsGroupedByDateGroups)
				{
					var organisation = costsGroupedByConsolCreditorAndCurrencyGroup.First().Factory.Load<OrgHeader>(costsGroupedByConsolCreditorAndCurrencyGroup.Key.OrgPk);
					var invoiceCurrencyType = AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(jobConsolCosts.First().Company, ledger, InvoiceCurrencyType.Foreign);
					var rateType = AccExchangeRateConfigurationRateFinder.GetExchangeRateConfigurationRateType(jobConsolCosts.First().ExchangeRateConfigurationRateConsumer, organisation, ledger, costsGroupedByConsolCreditorAndCurrencyGroup.Key.Currency, invoiceCurrencyType) ?? ExchangeRateType.Buy;
					var expectedRate = GetRate(costsGroupedByConsolCreditorAndCurrencyGroup.Key.Currency, rateType, costsGroupedByDateGroup.Key.Date.ToDateTime());
					UpdateJobConsolCostsExRates(jobConsolCosts, expectedRate);
				}
			}
		}

		static void UpdateSellInvoiceExchangeRates(IEnumerable<Charge> charges, string ledgerType, ZDateTime postingTime)
		{
			var localCurrency = charges.FirstOrDefault()?.Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			var ledger = ExchangeRateEnumsExtensions.GetLedgerFromCode(ledgerType);

			var chargesWithForeignSellInvoiceCurrency = charges.Where(x => (ledgerType == LedgerTypes.AccountsReceivable)
				&& x.InvoicingJob != null
				&& !string.IsNullOrEmpty(x.JR_RX_NKSellInvoiceCurrency)
				&& x.JR_RX_NKSellInvoiceCurrency != localCurrency
				&& GetChargeDate(x, ledger, postingTime).IsValid)
				.GroupBy(x => new {
					InvoicingJob = x.InvoicingJob,
					Currency = x.JR_RX_NKSellInvoiceCurrency,
					Date = GetChargeDate(x, ledger, postingTime)
				}, x => x);

			foreach (var group in chargesWithForeignSellInvoiceCurrency)
			{
				if (GetExchangeRateOption(ledger, false, group.Key.InvoicingJob.JH_GC) != ExRateOption.Default.Code)
				{
					foreach (var charge in group)
					{
						var sellInvoiceExpectedRate = GetExpectedRate(group.Key.InvoicingJob, false, group.Key.Currency, InvoiceCurrencyType.Foreign, ledger, charge.JR_OH_SellAccount, group.Key.Date.ToDateTime());
						charge.OverrideOSSellInvoiceExRateForPosting(sellInvoiceExpectedRate);
					}
				}
			}
		}

		static ZDateTime GetChargeDate(Charge charge, ExchangeRateValidLedgerEnum ledger, ZDateTime postingTime)
			=> GetExchangeRateDate(ledger, charge.IsInLocalInvoiceCurrencyForPosting(ledger), charge.JR_GC, charge.GetInvoiceDate(ledger), postingTime, charge.GetTaxDate(ledger));

		static ZDateTime GetInvoiceDate(this Charge charge, ExchangeRateValidLedgerEnum ledger)
				=> ledger == ExchangeRateValidLedgerEnum.AP
					? charge.ParentConsolCost?.E6_InvoiceDate.Date ?? charge.JR_APInvoiceDate.Date
					: charge.JR_Calc_ARInvoiceDate.Date;

		static ZDate GetTaxDate(this Charge charge, ExchangeRateValidLedgerEnum ledger)
			=> ledger == ExchangeRateValidLedgerEnum.AP
				? (charge.ParentConsolCost != null ? charge.ParentConsolCost.E6_TaxDate : charge.JR_CostTaxDate)
				: charge.JR_SellTaxDate;

		#region UpdateChargesExchangeRateExtensionMethods

		static ZString GetChargeCurrency(this Charge charge, string ledgerType)
		{
			return ledgerType == LedgerTypes.AccountsPayable ? charge.JR_CostCurrency : charge.JR_SellCurrency;
		}

		public static bool IsInLocalInvoiceCurrencyForPosting(this BaseCharge charge, ExchangeRateValidLedgerEnum ledger)
		{
			var localCurrency = charge.Company?.GC_RX_NKLocalCurrency ?? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (ledger == ExchangeRateValidLedgerEnum.AP)
			{
				return charge.JR_RX_NKCostCurrency == localCurrency;
			}
			else
			{
				if (!charge.JR_RX_NKSellInvoiceCurrency.IsEmpty)
				{
					return charge.JR_RX_NKSellInvoiceCurrency == localCurrency;
				}

				var foreignInvoiceTypes = new[] { InvoiceTypesList.Codes.ForeignCurrencyInvoice, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
				InvoiceTypesList.Codes.FreightInvoice, InvoiceTypesList.Codes.FreightInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency, InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching,
				AgencyInvoiceTypesList.Codes.ForeignCollect, AgencyInvoiceTypesList.Codes.ForeignCollect_Batching,
				AgencyInvoiceTypesList.Codes.ForeignPrePaid, AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching
			};

				if (foreignInvoiceTypes.Any(x => x == charge.JR_InvoiceType))
				{
					return false;
				}

				if (charge.JR_InvoiceType == InvoiceTypesList.Codes.SelfBillingInvoice || charge.JR_InvoiceType == InvoiceTypesList.Codes.SelfBillingInvoice_Batching)
				{
					return charge.JR_RX_NKSellCurrency == localCurrency;
				}

				return true;
			}
		}

		#endregion

		static string GetExchangeRateOption(InvoicingBase invoice) => GetExchangeRateOption(invoice.GetExRateLedger(), invoice.IsLocalCurrencyTransaction, invoice.AH_GC);

		static string GetExchangeRateOption(ExchangeRateValidLedgerEnum ledger, bool isLocalInvoiceCurrency, ZGuid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ledger, isLocalInvoiceCurrency, companyPK);
		}

		static void UpdateJobConsolCostsExRates(IEnumerable<JobConsolCost> jobConsolCosts, ZDecimal newRate)
		{
			foreach (var consolCost in jobConsolCosts)
			{
				if (consolCost.E6_ExchangeRate != newRate)
				{
					using (new DisposableAction(() => consolCost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting), () => consolCost.RemoveContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting)))
					{
						consolCost.E6_ExchangeRate = newRate;
					}
				}
			}
		}

		/// <summary>
		/// Get expected exchange Rate based on ARAPInvoicePostingExchangeRateOption registry
		/// </summary>
		static ZDecimal GetExpectedRate(Job job, bool useJobExRateFlag, ZString currency, bool isLocalInvoiceCurrency, ExchangeRateValidLedgerEnum ledger, OrgHeader organisation, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
		{
			var refCurrency = RefCurrency.LoadFromCurrencyCode(job.Factory, currency);
			var rateExpected = ZDecimal.Zero;
			if (useJobExRateFlag || ledger == ExchangeRateValidLedgerEnum.AR)
			{
				rateExpected = AccExchangeRateConfigurationRateFinder.GetExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, refCurrency, isLocalInvoiceCurrency, job.JH_GC, organisation, ledger, invoiceDate, postDate, taxDate, true);
			}
			else
			{
				rateExpected = AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateBasedOnInvoicePostingOption(job.ExchangeRateConfigurationRateConsumer, refCurrency, isLocalInvoiceCurrency, job.JH_GC, ledger, invoiceDate, postDate, taxDate);
			}
			return rateExpected;
		}

		/// <summary>
		/// Get expected rate for a given date
		/// </summary>
		static ZDecimal GetExpectedRate(Job job, bool useJobExRateFlag, ZString currency, InvoiceCurrencyType invoiceCurrencyType, ExchangeRateValidLedgerEnum ledger, ZGuid orgPK, ZDateTime preferredDate)
		{
			var refCurrency = RefCurrency.LoadFromCurrencyCode(job.Factory, currency);
			var organisation = job.Factory.Load<OrgHeader>(orgPK);
			var rateExpected = ZDecimal.Zero;
			if (useJobExRateFlag || ledger == ExchangeRateValidLedgerEnum.AR)
			{
				rateExpected = AccExchangeRateConfigurationRateFinder.GetExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, refCurrency, organisation, ledger, preferredDate, invoiceCurrencyType, true);
			}
			else
			{
				rateExpected = AccExchangeRateConfigurationRateFinder.GetBuyExchangeRateForDate(job.ExchangeRateConfigurationRateConsumer, refCurrency, preferredDate);
			}
			return rateExpected;
		}
	}
}
