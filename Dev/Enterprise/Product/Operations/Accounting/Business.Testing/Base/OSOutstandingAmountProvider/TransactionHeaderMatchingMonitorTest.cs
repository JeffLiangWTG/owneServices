using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public class TransactionHeaderMatchingMonitorTest : TestCaseWithFactory
	{
		public void TestFullyPay()
		{
			AssertFullyPay(false);
			AssertFullyPay(true);

			void AssertFullyPay(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				AssertEquals("Pre-condition", 0m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(0m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);
				AssertEquals(-1000m, header.AH_OutstandingAmount);
				AssertEquals(-1200m, header.AH_OSOutstandingAmountWithoutMultiplier);

				var dateTime = new ZDateTime(2022, 5, 20);
				header.MatchingMonitor.FullyPay(dateTime);

				AssertEquals(-1000m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(-1200m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);
				AssertEquals(0m, header.AH_OutstandingAmount);
				AssertEquals(0m, header.AH_OSOutstandingAmountWithoutMultiplier);
				AssertEquals(dateTime, header.AH_FullyPaidDate);
			}
		}

		public void TestFullyPay_EnableNewFeatureToOldTransactionIfUnPaid()
		{
			TransactionHeader headerUnpaid = null;
			TransactionHeader headerPartiallyPaid = null;
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				headerUnpaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerUnpaid.AH_OutstandingAmount = -1000m;
				headerPartiallyPaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000002", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerPartiallyPaid.AH_OutstandingAmount = -900m;
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("PreCondition headerUnpaid AH_IsOSOutstandingAmountApplicable", false, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerUnpaid AH_OSOutstandingAmount", 0m, headerUnpaid.AH_OSOutstandingAmount);
			AssertEquals("PreCondition headerUnpaid InvoiceUnpaid", true, headerUnpaid.InvoiceUnpaid);
			AssertEquals("PreCondition headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerPartiallyPaid AH_OSOutstandingAmount", 0m, headerPartiallyPaid.AH_OSOutstandingAmount);
			AssertEquals("PreCondition headerPartiallyPaid InvoiceUnpaid", false, headerPartiallyPaid.InvoiceUnpaid);

			headerUnpaid.MatchingMonitor.FullyPay(new ZDateTime(2022, 5, 20));
			AssertEquals("headerUnpaid AH_IsOSOutstandingAmountApplicable", true, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("headerUnpaid AH_OSOutstandingAmount", 0m, headerUnpaid.AH_OSOutstandingAmount);
			AssertEquals("headerUnpaid InvoiceUnpaid", false, headerUnpaid.InvoiceUnpaid);

			headerPartiallyPaid.MatchingMonitor.FullyPay(new ZDateTime(2022, 5, 20));
			AssertEquals("headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("headerPartiallyPaid AH_OSOutstandingAmount", 0m, headerPartiallyPaid.AH_OSOutstandingAmount);
			AssertEquals("headerPartiallyPaid InvoiceUnpaid", false, headerPartiallyPaid.InvoiceUnpaid);
		}

		public void TestGenerateMatchLinks()
		{
			AssertGenerateMatchLinks(false);
			AssertGenerateMatchLinks(true);

			void AssertGenerateMatchLinks(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				((IMatching)header).OSPartialPaymentAmount = -600m;
				header.MatchingMonitor.RefreshPaidAmounts();
				AssertEquals("Pre-condition", -500m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(-600m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);

				var matchLink = header.MatchingMonitor.GenerateMatchLinks();

				AssertEquals(-500m, matchLink.AP_Amount);
				AssertEquals(isRegistryEnabled ? -600m : 0m, matchLink.AP_OSAmount);
				AssertEquals(header.PK, matchLink.AP_AH);
			}
		}

		public void TestPartiallyPay()
		{
			AssertPartiallyPay(false);
			AssertPartiallyPay(true);

			void AssertPartiallyPay(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				((IMatching)header).OSPartialPaymentAmount = -600m;
				AssertEquals("Pre-condition", 0m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(0m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);
				AssertEquals(-1000m, header.AH_OutstandingAmount);
				AssertEquals(-1200m, header.AH_OSOutstandingAmountWithoutMultiplier);
				AssertEquals(true, header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);

				header.MatchingMonitor.PartiallyPay();

				AssertEquals(-500m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(-600m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);
				AssertEquals(-500m, header.AH_OutstandingAmount);
				AssertEquals(-600m, header.AH_OSOutstandingAmountWithoutMultiplier);
				AssertEquals(true, header.OSOutstandingAmountValueChangeMonitor.IsValueUpToDate);
			}
		}

		public void TestPartiallyPay_EnableNewFeatureToOldTransactionIfUnPaid()
		{
			TransactionHeader headerUnpaid = null;
			TransactionHeader headerPartiallyPaid = null;
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				headerUnpaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerUnpaid.AH_OutstandingAmount = -1000m;
				headerPartiallyPaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000002", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerPartiallyPaid.AH_OutstandingAmount = -900m;
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("PreCondition headerUnpaid AH_IsOSOutstandingAmountApplicable", false, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerUnpaid AH_OSOutstandingAmount", 0m, headerUnpaid.AH_OSOutstandingAmount);
			AssertEquals("PreCondition headerUnpaid InvoiceUnpaid", true, headerUnpaid.InvoiceUnpaid);
			AssertEquals("PreCondition headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerPartiallyPaid AH_OSOutstandingAmount", 0m, headerPartiallyPaid.AH_OSOutstandingAmount);
			AssertEquals("PreCondition headerPartiallyPaid InvoiceUnpaid", false, headerPartiallyPaid.InvoiceUnpaid);

			((IMatching)headerUnpaid).OSPartialPaymentAmount = -600m;
			headerUnpaid.MatchingMonitor.PartiallyPay();
			AssertEquals("headerUnpaid AH_IsOSOutstandingAmountApplicable", true, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("headerUnpaid headerUnpaid AH_OSOutstandingAmount", -400m, headerUnpaid.AH_OSOutstandingAmount);
			AssertEquals("headerUnpaid InvoiceUnpaid", false, headerUnpaid.InvoiceUnpaid);

			((IMatching)headerPartiallyPaid).OSPartialPaymentAmount = -600m;
			headerPartiallyPaid.MatchingMonitor.PartiallyPay();
			AssertEquals("headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("headerPartiallyPaid AH_OSOutstandingAmount", 0m, headerPartiallyPaid.AH_OSOutstandingAmount);
			AssertEquals("headerPartiallyPaid InvoiceUnpaid", false, headerPartiallyPaid.InvoiceUnpaid);
		}

		public void TestRefreshPaidAmounts()
		{
			AssertRefreshPaidAmounts(false);
			AssertRefreshPaidAmounts(true);

			void AssertRefreshPaidAmounts(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				AssertEquals("Pre-condition", 0m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(0m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);

				((IMatching)header).OSPartialPaymentAmount = 600m;
				header.MatchingMonitor.RefreshPaidAmounts();

				AssertEquals(500m, header.MatchingMonitor.CurrentPaidAmount_ForTestOnly);
				AssertEquals(600m, header.MatchingMonitor.CurrentOSPaidAmount_ForTestOnly);
			}
		}

		public void TestGeneratePaymentApprovalItems()
		{
			AssertGeneratePaymentApprovalItems(false);
			AssertGeneratePaymentApprovalItems(true);

			void AssertGeneratePaymentApprovalItems(bool isRegistryEnabled)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);

				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				((IMatching)header).OSPartialPaymentAmount = -600m;

				AssertEquals(0, ((IMatching)header).PaymentApprovalItems.Count);
				header.MatchingMonitor.GeneratePaymentApprovalItems(Factory.New<APPaymentApprovalWithAuthorisation>());

				AssertEquals(1, ((IMatching)header).PaymentApprovalItems.Count);
				var newApprovelItem = ((IMatching)header).PaymentApprovalItems[0];
				AssertEquals(isRegistryEnabled ? -600m : 0m, newApprovelItem.A2_OSPaymentThisRun);
				AssertEquals(-500m, newApprovelItem.A2_PaymentThisRun);
			}
		}

		public void TestGeneratePaymentApprovalItems_EnableNewFeatureToOldTransactionIfUnPaid()
		{
			TransactionHeader headerUnpaid = null;
			TransactionHeader headerPartiallyPaid = null;
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				headerUnpaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerUnpaid.AH_OutstandingAmount = -1000m;
				headerPartiallyPaid = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000002", TestObjectCreator.USD, 1m, 1000m, 0m, 1000m, 0m);
				headerPartiallyPaid.AH_OutstandingAmount = -900m;
			}

			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("PreCondition headerUnpaid AH_IsOSOutstandingAmountApplicable", false, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerUnpaid is not matched via payment approval.", 0, ((IMatching)headerUnpaid).PaymentApprovalItems.Count);
			AssertEquals("PreCondition headerUnpaid InvoiceUnpaid", true, headerUnpaid.InvoiceUnpaid);
			AssertEquals("PreCondition headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			AssertEquals("PreCondition headerPartiallyPaid is not matched via payment approval.", 0, ((IMatching)headerPartiallyPaid).PaymentApprovalItems.Count);
			AssertEquals("PreCondition headerPartiallyPaid InvoiceUnpaid", false, headerPartiallyPaid.InvoiceUnpaid);

			((IMatching)headerUnpaid).OSPartialPaymentAmount = -600m;
			headerUnpaid.MatchingMonitor.GeneratePaymentApprovalItems(Factory.New<APPaymentApprovalWithAuthorisation>());
			AssertEquals("headerUnpaid AH_IsOSOutstandingAmountApplicable", true, headerUnpaid.AH_IsOSOutstandingAmountApplicable);
			var newApprovelItemUnpaid = ((IMatching)headerUnpaid).PaymentApprovalItems[0];
			AssertEquals("headerUnpaid ApprovelItem A2_OSPaymentThisRun", -600m, newApprovelItemUnpaid.A2_OSPaymentThisRun);
			AssertEquals("headerUnpaid ApprovelItem A2_PaymentThisRun", -600m, newApprovelItemUnpaid.A2_PaymentThisRun);

			((IMatching)headerPartiallyPaid).OSPartialPaymentAmount = -600m;
			headerPartiallyPaid.MatchingMonitor.GeneratePaymentApprovalItems(Factory.New<APPaymentApprovalWithAuthorisation>());
			AssertEquals("headerPartiallyPaid AH_IsOSOutstandingAmountApplicable", false, headerPartiallyPaid.AH_IsOSOutstandingAmountApplicable);
			var newApprovelItemPartiallyPaid = ((IMatching)headerPartiallyPaid).PaymentApprovalItems[0];
			AssertEquals("headerPartiallyPaid ApprovelItem A2_OSPaymentThisRun", 0m, newApprovelItemPartiallyPaid.A2_OSPaymentThisRun);
			AssertEquals("headerPartiallyPaid ApprovelItem A2_PaymentThisRun", -600m, newApprovelItemPartiallyPaid.A2_PaymentThisRun);
		}

		#region Implementation

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
