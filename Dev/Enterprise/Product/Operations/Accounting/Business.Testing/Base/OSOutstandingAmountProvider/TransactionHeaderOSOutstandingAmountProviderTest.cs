using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class TransactionHeaderOSOutstandingAmountProviderTest : TestCaseWithFactory
	{
		public void TestGetTransactionHeaderOSOutstandingAmount()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.GetAndRefreshOSOutstandingAmount(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -600m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -800m, func);
		}

		public void TestGetTransactionHeaderOSOutstandingAmount_FullyPaid()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.GetAndRefreshOSOutstandingAmount(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(false, 0m, func);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(true, 0m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(false, 0m, func);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(true, -500m, func);
		}

		public void TestGetTransactionHeaderOSOutstandingAmount_Unpaid()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.GetAndRefreshOSOutstandingAmount(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(false, -1200m, func);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(true, -1200m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(false, -1200m, func);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(true, -500m, func);
		}

		public void TestCalculateTransactionHeaderOSOutstandingAmount()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.CalculateOSOutstandingAmount_ForTestOnly(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -600m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -600m, func);
		}

		public void TestCalculateTransactionHeaderOSOutstandingAmount_FullyPaid()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.CalculateOSOutstandingAmount_ForTestOnly(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(false, 0m, func);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(true, 0m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(false, 0m, func);
			AssertTransactionHeaderOSOutstandingAmount_FullyPaid(true, 0m, func);
		}

		public void TestCalculateTransactionHeaderOSOutstandingAmount_Unpaid()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.CalculateOSOutstandingAmount_ForTestOnly(x);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(false, -1200m, func);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(true, -1200m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(false, -1200m, func);
			AssertTransactionHeaderOSOutstandingAmount_Unpaid(true, -1200m, func);
		}

		public void TestUpdateOSOutstandingAmountSafe()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertUpdateOSOutstandingAmountSafe(false, false, 0m, true);
			AssertUpdateOSOutstandingAmountSafe(false, true, 0m, true);
			AssertUpdateOSOutstandingAmountSafe(true, false, 0m, true);
			AssertUpdateOSOutstandingAmountSafe(true, true, 0m, true);

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertUpdateOSOutstandingAmountSafe(false, false, 0m, true);
			AssertUpdateOSOutstandingAmountSafe(false, true, 0m, false);
			AssertUpdateOSOutstandingAmountSafe(true, false, 0m, true);
			AssertUpdateOSOutstandingAmountSafe(true, true, -1200m, true);
		}

		public void TestGetOSMatchedAmount()
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertGetOSMatchedAmount(false, -600m);
			AssertGetOSMatchedAmount(true, -600m);

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertGetOSMatchedAmount(false, -600m);
			AssertGetOSMatchedAmount(true, -700m);
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

				AssertEquals(expectedValue, TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(header));
			}
		}

		public void TestForceToSetOutstandingAmounts()
		{
			AssertForceEvaluation(false, false, false, 100m, 1000m, true);
			AssertForceEvaluation(false, false, true, 600m, 1000m, true);

			AssertForceEvaluation(false, true, false, 100m, 1000m, false);
			AssertForceEvaluation(false, true, true, 600m, 1000m, false);

			AssertForceEvaluation(true, false, false, 100m, 1000m, true);
			AssertForceEvaluation(true, false, true, 600m, 1000m, true);

			AssertForceEvaluation(true, true, false, 100m, 200m, true);
			AssertForceEvaluation(true, true, true, 600m, 1200m, true);

			void AssertForceEvaluation(bool isApplicable, bool isRegistryEnabled, bool isIncremental, ZDecimal expectedLocalAmount, ZDecimal expectedOSAmount, bool expectedIsUpToDate)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				header.AH_IsOSOutstandingAmountApplicable = isApplicable;
				header.AH_OutstandingAmount = 500m;
				header.AH_OSOutstandingAmount = 1000m;
				header.OSOutstandingAmountValueChangeMonitor.Reset();

				TransactionHeaderOSOutstandingAmountProvider.ForceToSetOutstandingAmounts(header, 100m, 200m, isIncremental);

				AssertEquals(expectedLocalAmount, header.AH_OutstandingAmount);
				AssertEquals(expectedOSAmount, header.AH_OSOutstandingAmount);
				AssertEquals(expectedIsUpToDate, header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);
			}
		}

		public void TestGetHighPrecisionOSOutstandingAmount()
		{
			AssertOSAmount(true, true);
			AssertOSAmount(true, false);
			AssertOSAmount(false, true);
			AssertOSAmount(false, false);

			void AssertOSAmount(bool registry, bool isReciprocal)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
				AssertEquals(5.45m, TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(11m, 12m, 5m, TestObjectCreator.AUD.Code));
				AssertEquals(5m, TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(11m, 12m, 5m, TestObjectCreator.KRW.Code));
			}
		}

		public void TestGetHighPrecisionOSAmount()
		{
			AssertOSAmount(true, true);
			AssertOSAmount(true, false);
			AssertOSAmount(false, true);
			AssertOSAmount(false, false);

			void AssertOSAmount(bool registry, bool isReciprocal)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
				GlbCompany.CurrentCompany.GC_IsReciprocal = isReciprocal;
				AssertEquals(5.45m, TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSAmount(11m, 12m, 5m, TestObjectCreator.AUD.Code));
				AssertEquals(5m, TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSAmount(11m, 12m, 5m, TestObjectCreator.KRW.Code));
			}
		}

		public void TestGetLowPrecisionOSOutstandingAmount()
		{
			Func<TransactionHeader, ZDecimal> func = (x) => TransactionHeaderOSOutstandingAmountProvider.GetLowPrecisionOSOutstandingAmount(x.AH_OutstandingAmount, x.AH_ExchangeRate, x.AH_RX_NKTransactionCurrency);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -600m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount(false, -600m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -600m, func);

			func = (x) => TransactionHeaderOSOutstandingAmountProvider.GetLowPrecisionOSOutstandingAmount(x.AH_OutstandingAmount, 1.1m, x.AH_RX_NKTransactionCurrency);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertTransactionHeaderOSOutstandingAmount(false, -550m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -550m, func);
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertTransactionHeaderOSOutstandingAmount(false, -550m, func);
			AssertTransactionHeaderOSOutstandingAmount(true, -550m, func);
		}

		#region Implementation

		void AssertTransactionHeaderOSOutstandingAmount(bool isApplicable, ZDecimal expectedOSOutstandingAmount, Func<TransactionHeader, ZDecimal> methodToTest)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_OutstandingAmount = -500m;
			header.AH_OSOutstandingAmount = -800m;
			header.OSOutstandingAmountValueChangeMonitor.Reset();

			AssertEquals(expectedOSOutstandingAmount, methodToTest(header));
		}

		void AssertTransactionHeaderOSOutstandingAmount_FullyPaid(bool isApplicable, ZDecimal expectedOSOutstandingAmount, Func<TransactionHeader, ZDecimal> methodToTest)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_OutstandingAmount = 0m;
			header.AH_OSOutstandingAmount = -500m;
			header.OSOutstandingAmountValueChangeMonitor.Reset();

			AssertEquals(expectedOSOutstandingAmount, methodToTest(header));
		}

		void AssertTransactionHeaderOSOutstandingAmount_Unpaid(bool isApplicable, ZDecimal expectedOSOutstandingAmount, Func<TransactionHeader, ZDecimal> methodToTest)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_LocalOutstandingAmount = header.AH_LocalTotalAmount;
			header.AH_OSOutstandingAmount = -500m;
			header.OSOutstandingAmountValueChangeMonitor.Reset();

			AssertEquals(expectedOSOutstandingAmount, methodToTest(header));
		}

		void AssertUpdateOSOutstandingAmountSafe(bool isApplicable, bool isValueChanged, ZDecimal expectedOSOutstandingAmount, bool expectedValueUpToDate)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_LocalOutstandingAmount = header.AH_LocalTotalAmount;
			if (!isValueChanged)
			{
				header.OSOutstandingAmountValueChangeMonitor.Reset();
			}

			TransactionHeaderOSOutstandingAmountProvider.UpdateOSOutstandingAmount(header);
			AssertEquals(expectedOSOutstandingAmount, header.AH_OSOutstandingAmount);
			AssertEquals(expectedValueUpToDate, header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);
		}

		void AssertGetOSMatchedAmount(bool isApplicable, ZDecimal expectedMatchedTotal)
		{
			var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
			header.AH_IsOSOutstandingAmountApplicable = isApplicable;
			header.AH_OutstandingAmount = -500m;
			header.AH_OSOutstandingAmount = -500m;
			header.OSOutstandingAmountValueChangeMonitor.Reset();

			AssertEquals(expectedMatchedTotal, TransactionHeaderOSOutstandingAmountProvider.GetOSMatchedAmount(header));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
