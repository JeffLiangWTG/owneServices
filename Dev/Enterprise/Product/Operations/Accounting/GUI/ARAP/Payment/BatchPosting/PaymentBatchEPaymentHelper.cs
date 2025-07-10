using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.GUI.ARAP
{
	internal static class PaymentBatchEPaymentHelper
	{
		internal static void CheckExRate(APPaymentBatchPoster batchPoster, Action onSuccess)
		{
			var quotes = new List<EPaymentQuote>();
			var paymentsWithRequestedQuotes = new List<PaymentApprovalBase>();
			var tempFactory = new BusinessObjectFactory();
			var batchPosterInTempFactory = tempFactory.Load<APPaymentBatchPoster>(batchPoster.PK);
			batchPosterInTempFactory.LoadPayments();

			var payments = batchPosterInTempFactory.PaymentApprovalCollectionWithoutCancelledOrPosted;
			foreach (PaymentApprovalBase payment in payments)
			{
				var (status, userMessage) = EPaymentDealCreator.ValidateExRate(payment);
				if (status != EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
				{
					switch (status)
					{
						case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
							Globals.Message.ShowError(userMessage);
							break;
						case EPaymentDealCreator.QuoteAcceptingStatus.UserNotAuthorized:
							PaymentApprovalEPaymentHelper.PromptUserToAuthorize(payment, userMessage);
							break;
						default:
							throw new DeveloperNotificationException(FormattableString.Invariant($"Unexpected QuoteAcceptingStatus value '{status}' found. Please implement processing logic for it here."));
					}
					return;
				}
				var (quote, errorMessage, quoteStatus) = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX);
				// if quote is in requested status, create a new quote and discard existing active quotes if new quote is created successfully.
				if (quoteStatus == EPaymentDealCreator.QuoteAcceptingStatus.QuoteRequested)
				{
					paymentsWithRequestedQuotes.Add(payment);
				}
				if (quoteStatus == EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
				{
					quotes.Add(quote);
					payment.DiscardActiveFXQuotes(EPaymentProviderCodes.Codes.OFX, false);
				}
			}

			if (!paymentsWithRequestedQuotes.IsNullOrEmpty())
			{
				var caption = Res.GetString("cbad8f93-cf12-43ec-8979-20ece0aee0f6", "Quotes already requested");
				var message = Res.GetString("b9efa53e-f10d-4c90-9289-358ad16ae322", "This batch contains E-Quotes in Requested status. Response may take up to several minutes to be received. If you generate new requests, then the previous requests will be discarded. Are you sure you want to proceed?");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dialogResult == DialogResult.Yes)
				{
					foreach (PaymentApprovalBase payment in paymentsWithRequestedQuotes)
					{
						var quote = payment.TryToCreateQuote(EPaymentProviderCodes.Codes.OFX, true).Quote;
						quotes.Add(quote);
						payment.DiscardActiveFXQuotes(EPaymentProviderCodes.Codes.OFX, false);
					}
				}
				else
				{
					return;
				}
			}
			else if (quotes.IsNullOrEmpty())
			{
				Globals.Message.ShowWarning(Res.GetString("456d2cc3-a85a-4f10-85af-f1a3080e380c", @"All of the payments in this batch already have active Quote Requests.
No new Quotes have been requested"));
				return;
			}
			tempFactory.Save();

			onSuccess();
			Globals.Message.ShowInformation(Res.GetString("db4e7336-5b3a-45b9-8152-85964286954e", "E-Quote requests generated to service provider {0}. Response may take from a few moments up to several minutes to be received.", EPaymentProviderCodes.Codes.OFX));
		}

		internal static void AcceptQuotes(APPaymentBatchPoster batchPoster, Action onSuccess)
		{
			if (batchPoster.HasChanges || batchPoster.PaymentApprovalCollection.Any(x => x.HasChanges))
			{
				Globals.Message.ShowWarning(Res.GetString("33d6c31b-258d-4dfa-8acd-77605e2e3043", "Please save the Payment Batch before Accepting Quotes."));
				return;
			}

			var tempFactory = new BusinessObjectFactory();
			var batchPosterInTempFactory = tempFactory.Load<APPaymentBatchPoster>(batchPoster.PK);
			batchPosterInTempFactory.LoadPayments();

			var payments = batchPosterInTempFactory.PaymentApprovalCollectionWithoutCancelledOrPosted;
			var query = new ZQuery(AccEPaymentQuoteSchema.QU_AV, payments.Select(x => x.PK));
			query.AddToFilter(AccEPaymentQuoteSchema.QU_Status, SQLComparisonOperator.NotEqual, QuoteStatusCodes.Discarded);
			var quotes = tempFactory.Load<EPaymentQuote>(query);

			if (!quotes.Any() || quotes.Length < payments.Count())
			{
				Globals.Message.ShowWarning(Res.GetString("617c0b94-a129-4591-a7e3-9212a9b9e5c6", @"There are payments on this batch that do not have an active quote.
Please click 'Check E-Pay rate' to create quotes for these payments."));
				return;
			}

			foreach (var quote in quotes)
			{
				var (isAccepted, _) = EPaymentQuoteAcceptor.AcceptEPaymentQuote(quote, payments.First(x => x.PK == quote.QU_AV), false);
				if (!isAccepted)
				{
					Globals.Message.ShowError(Res.GetString("8c255122-e527-4a50-a29b-e7c8d5f56c2c", "E-Quote can be accepted only when it is in Received status, has a Provider Reference and the associated payment is not yet posted"));
					return;
				}
			}

			tempFactory.Save();

			onSuccess();
		}

		internal static bool TryToCreateDeal(APPaymentBatchPoster batchPoster)
		{
			if (batchPoster.HasChanges || batchPoster.PaymentApprovalCollection.Any(x => x.HasChanges))
			{
				Globals.Message.ShowWarning(Res.GetString("1d8cdab5-015f-4475-ab18-3c9ca20ee89b", "Please save your payment batch before processing E-Payments."));
				return false;
			}

			ZDecimal fundingTotal = 0m;
			ZDecimal feeTotal = 0m;
			string provider = null;
			var tempFactory = new BusinessObjectFactory();
			var batchPosterInTempFactory = tempFactory.Load<APPaymentBatchPoster>(batchPoster.PK);
			batchPosterInTempFactory.LoadPayments();

			var payments = batchPosterInTempFactory.PaymentApprovalCollectionWithoutCancelledOrPosted;
			if (!payments.Any())
			{
				Globals.Message.ShowWarning(Res.GetString("3c55f6a6-aac8-4948-ad86-2c9274ae7c03", "All payments in this batch are either canceled or posted."));
			}
			else if (payments.FirstOrDefault(x => !x.ProcessEPaymentSecurityCheckPoint.IsAllowed) is PaymentApprovalBase paymentWithoutSecurityRight)
			{
				Globals.Message.ShowError(paymentWithoutSecurityRight.ProcessEPaymentSecurityCheckPoint.ErrorMessageForNotAllowed);
			}
			else
			{
				var currencySummaries = new Dictionary<ZString, (ZDecimal amount, int decimalPlaces)>();
				foreach (var paymentApproval in payments)
				{
					var (status, quote, validationMessage) = EPaymentDealCreator.FindAcceptedQuote(paymentApproval);
					bool isDealCreated = false;
					if (status != EPaymentDealCreator.QuoteAcceptingStatus.NoErrors)
					{
						switch (status)
						{
							case EPaymentDealCreator.QuoteAcceptingStatus.ValidationErrors:
								Globals.Message.ShowError(validationMessage);
								return false;
							case EPaymentDealCreator.QuoteAcceptingStatus.UserNotAuthorized:
								PaymentApprovalEPaymentHelper.PromptUserToAuthorize(paymentApproval, validationMessage);
								return false;
							case EPaymentDealCreator.QuoteAcceptingStatus.QuoteAlreadyAccepted:
								isDealCreated = EPaymentDealCreator.TryToCreateEPaymentDeal(paymentApproval, quote, false).isDealCreated;
								break;
						}
					}
					if (!isDealCreated)
					{
						Globals.Message.ShowError(Res.GetString("c17e4696-26ef-4dae-ba7d-a78a4e2c0247", "E-Payments can be processed only if an authorized OFX account is provided, Creditor Organizations are configured for E-Payment, payments are in Approved status and have corresponding E-Quotes in Accepted status."));
						return false;
					}

					provider = provider ?? quote.QU_ProviderCode;
					fundingTotal += quote.QU_FromAmount;
					feeTotal += quote.QU_FeeAmount;
					UpdateCurrencySummary(currencySummaries, quote);
				}

				var popupMessage = GetPopupMessageForFundingCurrency(batchPoster, provider, fundingTotal, feeTotal, ConvertCurrencySummaryToString(currencySummaries));
				var confirm = Globals.Message.Show(popupMessage, Res.GetString("138be18c-72fd-4488-8dde-b7d6f2d036ce", "Process E-Payments for Payment Batch"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (confirm == DialogResult.Yes)
				{
					tempFactory.Save();
					Globals.Message.Show(Res.GetString("349a5832-ed22-47c4-a9c2-10a6d729ba4b", "E-Payment Deal requests generated to service provider {0}. Response may take from a few moments up to several minutes to be received.", provider));
					return true;
				}
			}
			return false;
		}

		static string GetPopupMessageForFundingCurrency(APPaymentBatchPoster batchPoster, string provider, ZDecimal total, ZDecimal feeTotal, ZString currencySummary)
		{
			var currency = batchPoster.FundingBankAccountCurrency;
			var refCurrency = batchPoster.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			var decimalPlaces = refCurrency.Decimals;
			return GetPopupMessageForForeignCurrency(provider, total, feeTotal, currency, currencySummary, decimalPlaces);
		}

		static string GetPopupMessageForForeignCurrency(string provider, ZDecimal total, ZDecimal feeTotal, ZString currency, ZString currencySummary, int decimalPlaces)
		{
			return Res.GetString("BCE5BACD-5AD4-4508-AFBB-6E26D8AFF482", @"The FX transactions in this Payment Batch will be executed by a third party provider, {0}. CargoWise provides the messaging and information exchange only.

Review the details of the transactions to ensure they are correct.

Provider: {0}
{5}
Total in Funding Currency: {1} {4}
Total Fees in Funding Currency: {2} {4}
Total Cost in Funding Currency: {3} {4}

The Exchange Rate used for each transaction within this Batch Payment is as per the accepted quote.
Total Cost is the amount you will be required to pay to {0}. The OS Amount of each transaction is the amount your recipients will receive*.
*Occasionally third-party intermediary banks may deduct a fee from your transfer before paying your recipient. This fee may vary, and {0} receives no portion of it.

By clicking ""Yes"", the FX transactions in this Batch become legally binding if accepted by {0}. Would you like to continue?

Please check the status of each transaction after clicking ""Yes"".
If an {0} quote has expired, {0} cannot process the FX transaction and CargoWise will display the status as ""DEC - Provider Declined"".
If you have any issues or questions about the transactions, please contact {0} directly.",
				provider,
				total.Round(decimalPlaces),
				feeTotal.Round(decimalPlaces),
				new ZDecimal(total + feeTotal).Round(decimalPlaces),
				currency,
				currencySummary);
		}

		static void UpdateCurrencySummary(Dictionary<ZString, (ZDecimal amount, int decimalPlaces)> currencySummaries, EPaymentQuote quote)
		{
			if (currencySummaries.TryGetValue(quote.ToCurrency.Code, out var summary))
			{
				summary.amount += quote.QU_ToAmount;
				currencySummaries[quote.ToCurrency.Code] = summary;
			}
			else
			{
				currencySummaries[quote.ToCurrency.Code] = (quote.QU_ToAmount, quote.ToCurrency.Decimals);
			}
		}

		static ZString ConvertCurrencySummaryToString(Dictionary<ZString, (ZDecimal amount, int decimalPlaces)> currencySummaries)
		{
			var summary = new ZStringBuilder();
			foreach (var currency in currencySummaries.OrderBy(key => key.Key))
			{
				summary.Append(Res.GetString("84c8d1ae-f17c-42bd-88bd-27cd7eede84f", @"{0} Total: {1}",
	currency.Key,
	currency.Value.amount.Round(currency.Value.decimalPlaces)));
			}
			return summary.ToStringWithNewLineBetweenAppends();
		}
	}
}
