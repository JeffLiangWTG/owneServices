using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL4_ProcessingFee()
		{
			var newRegistry = new CodeDescriptionBoolCollection();
			newRegistry.AddRange(EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.Value);
			newRegistry.Add("AAA", (NoResString)"DESC", false);
			EDIDataRegistry.Instance.InvoicingProcessingFeeLookup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistry);

			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			billing.L4_ProcessingFee = "";
			AssertHasErrors(billing.L4_ProcessingFeeInfo);

			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_ProcessingFeeInfo);

			billing.L4_ProcessingFee = "XXX";
			AssertHasErrors(billing.L4_ProcessingFeeInfo);

			billing.L4_ProcessingFee = "DDE";
			AssertNoErrors(billing.L4_ProcessingFeeInfo);

			billing.L4_ProcessingFee = "MPF";
			AssertNoErrors(billing.L4_ProcessingFeeInfo);

			billing.L4_ProcessingFee = "AAA";
			AssertNoErrors(billing.L4_ProcessingFeeInfo);

			billing.L4_IsPartner = true;
			billing.L4_ProcessingFee = "";
			AssertNoErrors(billing.L4_ProcessingFeeInfo);
		}

		public void TestCheckL4_ProcessingFeePercent()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			billing.L4_ProcessingFeePercent = 0.0m;
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_ProcessingFeePercentInfo);

			billing.L4_ProcessingFeePercent = -100.0m;
			AssertHasErrors(billing.L4_ProcessingFeePercentInfo);
			billing.L4_ProcessingFeePercent = 100.0m;
			AssertNoErrors(billing.L4_ProcessingFeePercentInfo);
			billing.L4_ProcessingFeePercent = -101.0m;
			AssertHasErrors(billing.L4_ProcessingFeePercentInfo);
			billing.L4_ProcessingFeePercent = 101.0m;
			AssertHasErrors(billing.L4_ProcessingFeePercentInfo);
			billing.L4_ProcessingFeePercent = 1.0m;
			AssertNoErrors(billing.L4_ProcessingFeePercentInfo);
		}

		public void TestCheckPartnerEmail()
		{
			ClientLicenceBilling billing = Factory.New<ClientLicenceBilling>();
			billing.L4_IsPartner = true;
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.PartnerEmailInfo);

			billing = Factory.New<ClientLicenceBilling>();
			billing.L4_IsPartner = true;
			billing.PartnerEmail = "al@cargowise.com";
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.PartnerEmailInfo);
		}

		public void TestL4_PredeterminedPrepaidBalance()
		{
			var billing = Factory.New<ClientLicenceBilling>();
			billing.L4_PredeterminedPrepaidBalance = -1;
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_PredeterminedPrepaidBalanceInfo);

			billing.L4_PredeterminedPrepaidBalance = 0;
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_PredeterminedPrepaidBalanceInfo);

			billing.L4_PredeterminedPrepaidBalance = 1;
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_PredeterminedPrepaidBalanceInfo);
		}

		public void TestL4_RX_NKPredeterminedPrepaidBalanceCurrency()
		{
			var billing = Factory.New<ClientLicenceBilling>();
			billing.L4_PredeterminedPrepaidBalance = 10;
			billing.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "";
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "A11";
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_PredeterminedPrepaidBalance = 0;
			billing.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "";
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_RX_NKPredeterminedPrepaidBalanceCurrencyInfo);
		}

		public void TestL4_FuturePredeterminedPrepaidBalance()
		{
			var billing = Factory.New<ClientLicenceBilling>();
			billing.L4_FuturePredeterminedPrepaidBalance = -1;
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_FuturePredeterminedPrepaidBalanceInfo);

			billing.L4_FuturePredeterminedPrepaidBalance = 0;
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_FuturePredeterminedPrepaidBalanceInfo);

			billing.L4_FuturePredeterminedPrepaidBalance = 1;
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_FuturePredeterminedPrepaidBalanceInfo);
		}

		public void TestL4_RX_NKFuturePredeterminedPrepaidBalanceCurrency()
		{
			var billing = Factory.New<ClientLicenceBilling>();
			billing.L4_FuturePredeterminedPrepaidBalance = 10;
			billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "";
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "A11";
			billing.Validation.ValidateAll();
			AssertHasErrors(billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "AUD";
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);

			billing.L4_FuturePredeterminedPrepaidBalance = 0;
			billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "";
			billing.Validation.ValidateAll();
			AssertNoErrors(billing.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrencyInfo);
		}
	}
}