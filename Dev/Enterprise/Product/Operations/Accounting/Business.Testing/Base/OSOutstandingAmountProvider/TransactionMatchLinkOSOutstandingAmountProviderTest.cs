using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class TransactionMatchLinkOSOutstandingAmountProviderTest : TestCaseWithFactory
	{
		public void TestGetMatchLinkOSOutstandingAmount()
		{
			Func<TransactionMatchLink, ZDecimal> func = (x) => TransactionMatchLinkOSAmountProvider.GetMatchLinkOSAmount(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertMatchLinkOSAmount(false, 600m, func);
			AssertMatchLinkOSAmount(true, 600m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertMatchLinkOSAmount(false, 600m, func);
			AssertMatchLinkOSAmount(true, 200m, func);
		}

		public void TestCalculateMatchLinkOSOutstandingAmount()
		{
			Func<TransactionMatchLink, ZDecimal> func = (x) => TransactionMatchLinkOSAmountProvider.CalculateMatchLinkOSAmount_ForTestOnly(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertMatchLinkOSAmount(false, 600m, func);
			AssertMatchLinkOSAmount(true, 600m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertMatchLinkOSAmount(false, 600m, func);
			AssertMatchLinkOSAmount(false, 600m, func);
		}

		public void TestFillReversingMatchLinkAmounts()
		{
			AssertForceEvaluation(false, false, 100m, 0m);
			AssertForceEvaluation(false, true, 100m, 0m);
			AssertForceEvaluation(true, false, 100m, 0m);
			AssertForceEvaluation(true, true, 100m, 200m);

			void AssertForceEvaluation(bool isRegistryEnabled, bool isTransactionHeaderApplicable, ZDecimal expectedLocalAmount, ZDecimal expectedOSAmount)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				header.AH_IsOSOutstandingAmountApplicable = isTransactionHeaderApplicable;

				var originalLink = TestObjectCreator.CreateMatchLink(header, 100m);
				originalLink.AP_OSAmount = 200M;

				header.GenerateMatchLinks();
				TransactionMatchLinkOSAmountProvider.FillReversingMatchLinkAmounts(header, originalLink);
				var reversingLink = ((IMatching)header).CurrentMatchGroup[0];

				AssertEquals(expectedLocalAmount, reversingLink.AP_Amount);
				AssertEquals(expectedOSAmount, reversingLink.AP_OSAmount);
			}
		}

		public void TestIsFeatureEnabled()
		{
			AssertFeatureEnabled(false, false, false);
			AssertFeatureEnabled(false, true, false);
			AssertFeatureEnabled(true, false, false);
			AssertFeatureEnabled(true, true, true);

			void AssertFeatureEnabled(bool isApplicable, bool isRegistryEnabled, bool expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				header.AH_IsOSOutstandingAmountApplicable = isApplicable;
				header.AH_OSOutstandingAmount = 1200m;

				var matchLink = ((IMatching)header).CurrentMatchGroup.AddNew();
				matchLink.AP_AH = header.PK;
				matchLink.AP_Amount = 500M;
				matchLink.AP_OSAmount = 600M;

				AssertEquals(expectedValue, TransactionMatchLinkOSAmountProvider.IsFeatureEnabled(matchLink));
			}
		}

		public void TestGetMatchAmountForPaymentDataAdapter()
		{
			const decimal paymentMatchAmountInPaymentCurrency = 601m;
			const decimal matchedOSAmount = 601m;

			AssertGetMatchAmountForPaymentDataAdapter(false, false, paymentMatchAmountInPaymentCurrency);
			AssertGetMatchAmountForPaymentDataAdapter(false, true, paymentMatchAmountInPaymentCurrency);
			AssertGetMatchAmountForPaymentDataAdapter(true, false, paymentMatchAmountInPaymentCurrency);
			AssertGetMatchAmountForPaymentDataAdapter(true, true, matchedOSAmount);

			void AssertGetMatchAmountForPaymentDataAdapter(bool isApplicable, bool isRegistryEnabled, decimal expectedValue)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				header.AH_IsOSOutstandingAmountApplicable = isApplicable;
				header.AH_OSOutstandingAmount = 1200m;

				const decimal matchLocalAmount = 500m;
				var matchLink = ((IMatching)header).CurrentMatchGroup.AddNew();
				matchLink.AP_AH = header.PK;
				matchLink.AP_Amount = matchLocalAmount;
				matchLink.AP_OSAmount = matchedOSAmount;

				AssertEquals(expectedValue, TransactionMatchLinkOSAmountProvider.GetOSPaidAmountForPaymentDataAdapter(matchLink, matchLocalAmount, paymentMatchAmountInPaymentCurrency));
			}
		}

		#region Implementation

		void AssertMatchLinkOSAmount(bool isApplicable, ZDecimal expectedOSAmount, Func<TransactionMatchLink, ZDecimal> methodToTest)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_OSOutstandingAmount = 1200m;

			var matchLink = ((IMatching)header).CurrentMatchGroup.AddNew();
			matchLink.AP_AH = header.PK;
			matchLink.AP_Amount = 500M;
			matchLink.AP_OSAmount = 200M;

			AssertEquals(expectedOSAmount, methodToTest(matchLink));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
