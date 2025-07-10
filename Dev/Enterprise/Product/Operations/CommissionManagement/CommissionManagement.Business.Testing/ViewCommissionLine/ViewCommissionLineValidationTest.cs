using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class ViewCommissionLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckVCL_TransactionAmount()
		{
			var line = Factory.New<ViewCommissionLine>();
			line.VCL_TransactionAmount = 0;

			line.Validation.ValidateVCL_TransactionAmount();
			AssertNoErrors("Should allow 0s", line.VCL_TransactionAmountInfo);
		}

		public void TestCheckVCL_CommissionAmountsInLocalCurrency()
		{
			var line = Factory.New<ViewCommissionLine>();
			line.VCL_CommissionDate = new ZDate(2002, 2, 2);
			line.VCL_RX_NKCommissionCurrency = "AUD";
			line.VCL_RX_NKLocalCurrency = "GBP";

			var revenueAmountPropertyInfos = new[]
				{
					line.VCL_TotalCommissionableAmountInLocalCurrencyInfo,
					line.VCL_ShareCommissionAmountInLocalCurrencyInfo,
					line.VCL_EntityCommissionAmountInLocalCurrencyInfo,
				};

			line.Validation.ValidateAllLocalAmounts();
			foreach (var propertyInfo in revenueAmountPropertyInfos)
			{
				AssertHasWarning(propertyInfo, string.Format("No 'AUD' exchange rate valid on the {0}", new ZDate(2002, 2, 2).ToShortDateString()));
			}

			line.VCL_CommissionToLocalExchangeRate = 0.55m;
			line.Validation.ValidateAllLocalAmounts();
			foreach (var propertyInfo in revenueAmountPropertyInfos)
			{
				AssertNoWarnings(propertyInfo);
			}
		}

		public void TestValidateFullyPaymentOfInvoices()
		{
			var line = Factory.New<ViewCommissionLine>();
			string warningMsgWhenRegistrySetToYes = "AR invoice(s) have to be fully paid prior to payment process action";
			string warningMsgWhenRegistrySetToNo = "AR Invoice(s) are not fully paid";
			var dateInPast = new ZDateTime(2017, 07, 10);

			line.VCL_Ledger = "AR";
			line.VCL_TransactionType = "INV";
			line.VCL_TransactionFullyPaidDate = dateInPast;
			line.Validation.ValidateFullyPaymentOfARInvoices();
			NoFullAmountNotifications();

			line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			line.Validation.ValidateFullyPaymentOfARInvoices();
			HasOnlyExpectedNotification(warningMsgWhenRegistrySetToYes, warningMsgWhenRegistrySetToNo);

			line.VCL_Ledger = "AP";
			line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			line.Validation.ValidateFullyPaymentOfARInvoices();
			NoFullAmountNotifications();

			line.VCL_TransactionFullyPaidDate = dateInPast;
			line.Validation.ValidateFullyPaymentOfARInvoices();
			NoFullAmountNotifications();

			using (OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
				line.Validation.ValidateFullyPaymentOfARInvoices();
				NoFullAmountNotifications();

				line.VCL_TransactionFullyPaidDate = dateInPast;
				line.Validation.ValidateFullyPaymentOfARInvoices();
				NoFullAmountNotifications();

				line.VCL_Ledger = "AR";
				line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
				line.Validation.ValidateFullyPaymentOfARInvoices();
				HasOnlyExpectedNotification(warningMsgWhenRegistrySetToNo, warningMsgWhenRegistrySetToYes);

				line.VCL_TransactionFullyPaidDate = dateInPast;
				line.Validation.ValidateFullyPaymentOfARInvoices();
				NoFullAmountNotifications();
			}

			void NoFullAmountNotifications()
			{
				AssertNoWarning(line.HasFullyPaidInfo, warningMsgWhenRegistrySetToNo);
				AssertNoWarning(line.HasFullyPaidInfo, warningMsgWhenRegistrySetToYes);
			}

			void HasOnlyExpectedNotification(string messageExpected, string messageNotExpected)
			{
				AssertNoWarning(line.HasFullyPaidInfo, messageNotExpected);
				AssertHasWarning(line.HasFullyPaidInfo, messageExpected);
			}
		}

		public void TestStopPaymentForProcess()
		{
			var line = Factory.New<ViewCommissionLine>();

			line.VCL_Ledger = "AR";
			line.VCL_TransactionFullyPaidDate = ZDateTime.Today;
			line.VCL_TransactionType = "INV";
			line.Validation.ValidateFullyPaymentOfARInvoices();
			AssertEquals(false, line.Validation.ShouldStopPaymentForProcess);

			line.VCL_Ledger = "AP";
			line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			line.VCL_TransactionType = "INV";
			line.Validation.ValidateFullyPaymentOfARInvoices();
			AssertEquals(false, line.Validation.ShouldStopPaymentForProcess);

			line.VCL_Ledger = "AR";
			line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			line.VCL_TransactionType = "INV";
			line.Validation.ValidateFullyPaymentOfARInvoices();

			OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, line.Validation.ShouldStopPaymentForProcess);

			OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, line.Validation.ShouldStopPaymentForProcess);

			line.VCL_CommissionDate = new ZDate(2002, 2, 2);
			line.VCL_RX_NKCommissionCurrency = "AUD";
			var preferredAmountPropertyInfos = new[]
				{
					line.VCL_TotalCommissionableAmountInPreferredCurrencyInfo,
					line.VCL_ShareCommissionAmountInPreferredCurrencyInfo,
					line.VCL_EntityCommissionAmountInPreferredCurrencyInfo
				};
			line.VCL_RX_NKPreferredPaymentCurrency = "";
			line.Validation.ValidateAllPreferredAmounts();
			line.Validation.ValidateFullyPaymentOfARInvoices();
			AssertEquals(true, line.Validation.ShouldStopPaymentForProcess);

			line.VCL_RX_NKLocalCurrency = "GBP";
			line.VCL_RX_NKPreferredPaymentCurrency = "USD";
			line.Validation.ValidateAllPreferredAmounts();
			foreach (var propertyInfo in preferredAmountPropertyInfos)
			{
				AssertHasWarning(propertyInfo, string.Format("No 'AUD' exchange rate valid on the {0}", new ZDateTime(2002, 2, 2).ToShortDateString()));
			}
			AssertEquals(true, line.Validation.ShouldStopPaymentForProcess);

			line.VCL_Ledger = "AR";
			line.VCL_TransactionType = "INV";
			line.VCL_TransactionFullyPaidDate = ZDateTime.Empty;
			line.VCL_CommissionToLocalExchangeRate = 0.55m;
			line.VCL_LocalToPreferredExchangeRate = 1.5m;
			line.VCL_CommissionDate = ZDateTime.Today.Date;
			line.Validation.ValidateAllPreferredAmounts();
			line.Validation.ValidateFullyPaymentOfARInvoices();

			OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, line.Validation.ShouldStopPaymentForProcess);

			OrganisationsDataRegistry.Instance.DisallowCommissionPaymentIfARInvoiceNotFullyPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, line.Validation.ShouldStopPaymentForProcess);

			var testObjCreator = new TestObjectCreator(Factory);
			var chargeCode = testObjCreator.CreateChargeCode("DDD");
			chargeCode.AC_IsActive = false;
			line.VCL_AC = chargeCode.PK;
			line.Validation.ValidateVCL_AC();
			AssertEquals(false, line.Validation.ShouldStopPaymentForProcess);
		}

		public void TestCheckVCL_CommissionAmountsInPreferredCurrency()
		{
			var line = Factory.New<ViewCommissionLine>();
			line.VCL_CommissionDate = new ZDate(2002, 2, 2);
			line.VCL_RX_NKCommissionCurrency = "AUD";

			var preferredAmountPropertyInfos = new[]
				{
					line.VCL_TotalCommissionableAmountInPreferredCurrencyInfo,
					line.VCL_ShareCommissionAmountInPreferredCurrencyInfo,
					line.VCL_EntityCommissionAmountInPreferredCurrencyInfo,
				};

			line.VCL_RX_NKPreferredPaymentCurrency = "";
			line.Validation.ValidateAllPreferredAmounts();
			foreach (var propertyInfo in preferredAmountPropertyInfos)
			{
				AssertHasWarning(propertyInfo, "Preferred Payment Company has not been entered for this Entity");
			}

			line.VCL_RX_NKLocalCurrency = "GBP";
			line.VCL_RX_NKPreferredPaymentCurrency = "USD";
			line.Validation.ValidateAllPreferredAmounts();
			foreach (var propertyInfo in preferredAmountPropertyInfos)
			{
				AssertHasWarning(propertyInfo, string.Format("No 'AUD' exchange rate valid on the {0}", new ZDateTime(2002, 2, 2).ToShortDateString()));
			}

			line.VCL_CommissionToLocalExchangeRate = 0.55m;
			line.Validation.ValidateAllPreferredAmounts();
			foreach (var propertyInfo in preferredAmountPropertyInfos)
			{
				AssertHasWarning(propertyInfo, string.Format("No 'USD' exchange rate valid on the {0}", new ZDateTime(2002, 2, 2).ToShortDateString()));
			}

			line.VCL_LocalToPreferredExchangeRate = 1.5m;
			line.Validation.ValidateAllPreferredAmounts();
			foreach (var propertyInfo in preferredAmountPropertyInfos)
			{
				AssertNoWarnings(propertyInfo);
			}
		}
	}
}
