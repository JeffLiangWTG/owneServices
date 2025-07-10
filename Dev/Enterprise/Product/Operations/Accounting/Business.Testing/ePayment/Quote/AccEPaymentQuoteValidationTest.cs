using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using ProviderCodes = Enterprise.MasterFiles.Business.EPaymentProviderCodes.Codes;
using StatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.Testing
{
	sealed class AccEPaymentQuoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckQU_AV()
		{
			quote.QU_AV = ZGuid.Empty;
			AssertHasError(quote.QU_AVInfo, "Please enter a value.");
			quote.QU_AV = ZGuid.NewZGuid();
			AssertHasError(quote.QU_AVInfo, "Quote must be attached to a valid Payment Approval.");
			var approval = Factory.New<AccPaymentApproval>();
			quote.QU_AV = approval.PK;
			AssertNoErrors(quote.QU_AVInfo);
		}

		public void TestCheckQU_GC()
		{
			quote.QU_GC = ZGuid.Empty;
			AssertHasError(quote.QU_GCInfo, "Please enter a value.");
			quote.QU_GC = ZGuid.NewZGuid();
			AssertHasError(quote.QU_GCInfo, "Quote must specify a valid Company.");
			var approval = Factory.New<GlbCompany>();
			quote.QU_GC = approval.PK;
			AssertNoErrors(quote.QU_GCInfo);
		}

		public void TestCheckQU_InternalReference()
		{
			quote.QU_InternalReference = ZString.Empty;
			AssertHasError(quote.QU_InternalReferenceInfo, "Please enter a value.");
			quote.QU_InternalReference = "A hopefully valid reference.";
			AssertNoErrors(quote.QU_InternalReferenceInfo);
		}

		public void TestCheckQU_ProviderReference()
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				quote.QU_ProviderReference = ZString.Empty;
				AssertHasError(quote.QU_ProviderReferenceInfo, "A Response has been received from the provider but no Reference has been entered.");
				quote.QU_ProviderReference = "AAA";
				AssertNoErrors(quote.QU_ProviderReferenceInfo);
			});

			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				quote.QU_ProviderReference = ZString.Empty;
				AssertNoErrors(quote.QU_ProviderReferenceInfo);
				quote.QU_ProviderReference = "AAA";
				AssertHasError(quote.QU_ProviderReferenceInfo, "A reference cannot be entered before a response is received from the provider.");
			});
		}

		public void TestCheckQU_ProviderCode()
		{
			quote.QU_ProviderCode = ZString.Empty;
			AssertHasError(quote.QU_ProviderCodeInfo, "Please enter a value.");
			quote.QU_ProviderCode = "AAA";
			AssertHasError(quote.QU_ProviderCodeInfo, "Enter a valid selection.");
			quote.QU_ProviderCode = ProviderCodes.OFX;
			AssertNoErrors(quote.QU_ProviderCodeInfo);
		}

		public void TestCheckQU_Status()
		{
			quote.QU_Status = ZString.Empty;
			AssertHasError(quote.QU_StatusInfo, "Please enter a value.");
			quote.QU_Status = "AAA";
			AssertHasError(quote.QU_StatusInfo, "Enter a valid selection.");
			AssertWithStatusIndicatingValidResponseReceived(() => AssertNoErrors(quote.QU_StatusInfo));
			AssertWithStatusNotIndicatingValidResponseReceived(() => AssertNoErrors(quote.QU_StatusInfo));
		}

		public void TestCheckQU_ToAmount()
		{
			AssertAmountWhenValidResponseReceived(quote.QU_ToAmountInfo as ZPropertyInfo<ZDecimal>);
			AssertAmountWhenValidResponseNotReceived(quote.QU_ToAmountInfo as ZPropertyInfo<ZDecimal>, quote.QU_FromAmountInfo as ZPropertyInfo<ZDecimal>);
		}
		public void TestCheckQU_FromAmount()
		{
			AssertAmountWhenValidResponseReceived(quote.QU_FromAmountInfo as ZPropertyInfo<ZDecimal>);
			AssertAmountWhenValidResponseNotReceived(quote.QU_FromAmountInfo as ZPropertyInfo<ZDecimal>, quote.QU_ToAmountInfo as ZPropertyInfo<ZDecimal>);
		}
		void AssertAmountWhenValidResponseReceived(ZPropertyInfo<ZDecimal> amountInfo)
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				amountInfo.Value = -1;
				AssertHasError(amountInfo, "Quote amounts must be greater than 0 when recording the details of a Quote Response from a provider.");
				amountInfo.Value = 0;
				AssertHasError(amountInfo, "Quote amounts must be greater than 0 when recording the details of a Quote Response from a provider.");
				amountInfo.Value = 1;
				AssertNoErrors(amountInfo);
			});
		}

		void AssertAmountWhenValidResponseNotReceived(ZPropertyInfo<ZDecimal> amountInfo, ZPropertyInfo<ZDecimal> otherAmountInfo)
		{
			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				amountInfo.Value = -1;
				AssertHasError(amountInfo, "Quote amounts cannot be negative.");
				otherAmountInfo.Value = 0;
				AssertNoErrors(otherAmountInfo);
				amountInfo.Value = 0;
				AssertHasError(amountInfo, "Exactly one amount must be specified when requesting a Quote.");
				amountInfo.Value = 1;
				AssertNoErrors(otherAmountInfo);
				otherAmountInfo.Value = 1;
				AssertHasError(otherAmountInfo, "Exactly one amount must be specified when requesting a Quote.");
			});
		}

		public void TestCheckQU_RX_NKToCurrency()
		{
			quote.QU_RX_NKToCurrency = ZString.Empty;
			AssertHasError(quote.QU_RX_NKToCurrencyInfo, "Please enter a value.");
			quote.QU_RX_NKToCurrency = "AAA";
			AssertHasError(quote.QU_RX_NKToCurrencyInfo, "Enter a valid selection.");
			quote.QU_RX_NKToCurrency = CurrencyCodes.Australia;
			AssertNoErrors(quote.QU_RX_NKToCurrencyInfo);
		}

		public void TestCheckQU_RX_NKFromCurrency()
		{
			quote.QU_RX_NKFromCurrency = ZString.Empty;
			AssertHasError(quote.QU_RX_NKFromCurrencyInfo, "Please enter a value.");
			quote.QU_RX_NKFromCurrency = "AAA";
			AssertHasError(quote.QU_RX_NKFromCurrencyInfo, "Enter a valid selection.");
			quote.QU_RX_NKFromCurrency = CurrencyCodes.Australia;
			AssertNoErrors(quote.QU_RX_NKFromCurrencyInfo);
		}

		public void TestCheckQU_ExchangeRate()
		{
			AssertExchangeRate(quote.QU_ExchangeRateInfo as ZPropertyInfo<ZDecimal>, quote.QU_ExchangeRateInvertedInfo as ZPropertyInfo<ZDecimal>);
		}

		public void TestCheckQU_ExchangeRateInverted()
		{
			AssertExchangeRate(quote.QU_ExchangeRateInvertedInfo as ZPropertyInfo<ZDecimal>, quote.QU_ExchangeRateInfo as ZPropertyInfo<ZDecimal>);
		}

		void AssertExchangeRate(ZPropertyInfo<ZDecimal> exRateInfo, ZPropertyInfo<ZDecimal> otherExRateInfo)
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				exRateInfo.Value = -1;
				AssertHasError(exRateInfo, "Exchange rates cannot be 0 or negative.");
				exRateInfo.Value = 0;
				AssertHasError(exRateInfo, "Exchange rates cannot be 0 or negative.");
				exRateInfo.Value = 1;
				AssertNoErrors(exRateInfo);

				quote.QU_RX_NKFromCurrency = quote.QU_RX_NKToCurrency;
				exRateInfo.Value = 0.5;
				AssertHasError(exRateInfo, "If the currencies are equal, then exchange rate must be 1.");
				exRateInfo.Value = 1;
				AssertNoErrors(exRateInfo);

				quote.QU_RX_NKFromCurrency = "AUD";
				quote.QU_RX_NKToCurrency = "USD";
				otherExRateInfo.Value = 0.5;
				exRateInfo.Value = 0.5;
				AssertHasError(exRateInfo, "The Exchange Rate must be the reciprocal of the Inverted Exchange Rate.");

				otherExRateInfo.Value = 1;
				exRateInfo.Value = 0.5;
				AssertNoErrors(exRateInfo);

				otherExRateInfo.Value = 1;
				exRateInfo.Value = 1.5;
				AssertNoErrors(exRateInfo);

				otherExRateInfo.Value = 0.5;
				exRateInfo.Value = 1.5;
				AssertNoErrors(exRateInfo);

				otherExRateInfo.Value = 1.5;
				exRateInfo.Value = 1.5;
				AssertHasError(exRateInfo, "The Exchange Rate must be the reciprocal of the Inverted Exchange Rate.");
			});

			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				exRateInfo.Value = -1;
				AssertHasError(exRateInfo, "Exchange rates cannot be entered before a response is received from the provider.");
				exRateInfo.Value = 1;
				AssertHasError(exRateInfo, "Exchange rates cannot be entered before a response is received from the provider.");
				exRateInfo.Value = 0;
				AssertNoErrors(exRateInfo);
			});
		}

		public void TestCheckQU_FeeAmount()
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				quote.QU_FeeAmount = -1;
				AssertHasError(quote.QU_FeeAmountInfo, "Fee amount cannot be negative.");
				quote.QU_FeeAmount = 0;
				AssertNoErrors(quote.QU_FeeAmountInfo);
				quote.QU_FeeAmount = 1;
				AssertNoErrors(quote.QU_FeeAmountInfo);
			});

			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				quote.QU_FeeAmount = -1;
				AssertHasError(quote.QU_FeeAmountInfo, "Fee amount cannot be entered before a response is received from the provider.");
				quote.QU_FeeAmount = 1;
				AssertHasError(quote.QU_FeeAmountInfo, "Fee amount cannot be entered before a response is received from the provider.");
				quote.QU_FeeAmount = 0;
				AssertNoErrors(quote.QU_FeeAmountInfo);
			});
		}

		public void TestCheckQU_RX_NKFeeCurrency()
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				quote.QU_FeeAmount = 0;
				quote.QU_RX_NKFeeCurrency = ZString.Empty;
				AssertNoErrors(quote.QU_RX_NKFeeCurrencyInfo);
				quote.QU_RX_NKFeeCurrency = "AUD";
				AssertHasError(quote.QU_RX_NKFeeCurrencyInfo, "There should be no Fee Currency if there is no Fee.");

				quote.QU_FeeAmount = 5;
				quote.Validation.ValidateQU_RX_NKFeeCurrency();
				AssertNoErrors(quote.QU_RX_NKFeeCurrencyInfo);
				quote.QU_RX_NKFeeCurrency = ZString.Empty;
				AssertHasError(quote.QU_RX_NKFeeCurrencyInfo, "If a Fee is specified then a Fee Currency must be specified.");
			});

			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				quote.QU_RX_NKFeeCurrency = "AUD";
				AssertHasError(quote.QU_RX_NKFeeCurrencyInfo, "Fee currency cannot be entered before a response is received from the provider.");
				quote.QU_RX_NKFeeCurrency = ZString.Empty;
				AssertNoErrors(quote.QU_RX_NKFeeCurrencyInfo);
			});
		}

		public void TestCheckQU_LastResponseReceivedUtc()
		{
			AssertWithStatusIndicatingValidResponseReceived(() =>
			{
				quote.QU_LastResponseReceivedUtc = ZDateTime.Empty;
				AssertHasError(quote.QU_LastResponseReceivedUtcInfo, "Response time must be recorded if a response from the provider has been received.");
				quote.QU_SystemCreateTimeUtc = ZDateTime.Today;
				quote.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(-1);
				AssertHasError(quote.QU_LastResponseReceivedUtcInfo, "Response time cannot be earlier than the creation time.");

				quote.QU_LastResponseReceivedUtc = ZDateTime.Today;
				AssertNoErrors(quote.QU_LastResponseReceivedUtcInfo);
				quote.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(1);
				AssertNoErrors(quote.QU_LastResponseReceivedUtcInfo);
			});

			AssertWithStatusNotIndicatingValidResponseReceived(() =>
			{
				quote.QU_LastResponseReceivedUtc = ZDateTime.Today.AddDays(1);
				AssertHasError(quote.QU_LastResponseReceivedUtcInfo, "Response Time cannot be entered before a response is received from the provider.");
				quote.QU_LastResponseReceivedUtc = ZDateTime.Empty;
				AssertNoErrors(quote.QU_LastResponseReceivedUtcInfo);
			});
		}

		public void TestCheckQU_ErrorDescription()
		{
			foreach (var code in quote.Lookups.StatusCodeList.GetAllCodes())
			{
				quote.QU_Status = code;
				quote.QU_ErrorDescription = ZString.Empty;
				AssertNoErrors(quote.QU_ErrorDescriptionInfo);

				quote.QU_ErrorDescription = "Heyo an error happened";
				if (code == StatusCodes.Failed || code == StatusCodes.Error)
				{
					AssertNoErrors(quote.QU_ErrorDescriptionInfo);
				}
				else
				{
					AssertHasError(quote.QU_ErrorDescriptionInfo, "Error Description should only be recorded if the status is either RQF or ERR.");
				}
			}
		}

		void AssertWithStatusIndicatingValidResponseReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Received, StatusCodes.Accepted, StatusCodes.Discarded, StatusCodes.Expired }))
			{
				quote.QU_Status = code;
				quote.Validation.ValidateAll();
				testToRun();
			}
		}

		void AssertWithStatusNotIndicatingValidResponseReceived(Action testToRun)
		{
			foreach (var code in (new[] { StatusCodes.Queued, StatusCodes.Requested, StatusCodes.Failed, StatusCodes.Error }))
			{
				quote.QU_Status = code;
				quote.Validation.ValidateAll();
				testToRun();
			}
		}

		AccEPaymentQuote quote;

		protected override void SetUp()
		{
			base.SetUp();
			quote = Factory.New<AccEPaymentQuote>();
		}
	}
}
