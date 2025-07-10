using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class PaymentApprovalItemOSOutstandingAmountProviderTest : TestCaseWithFactory
	{
		public void TestIsPaymentApprovalValidForMatchCalculation()
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			var accPaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>();
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV123", TestObjectCreator.USD, 1.6m, 1000m, 0m, 625m, 0m, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK);
			approval.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval.AV_Status = PaymentApprovalStatus.FullyApproved;

			var item = Factory.New<PaymentApprovalItem>();
			item.A2_AV = approval.PK;
			item.A2_AH = apInvoice.PK;

			AssertEquals("Should be false given BIZO is not in database.", false, PaymentApprovalItemOSAmountProvider.IsPaymentApprovalValidForMatch(item));

			Factory.Save();

			foreach (var status in typeof(PaymentApprovalStatus).GetConstantValues())
			{
				approval.AV_Status = status;
				if (status != PaymentApprovalStatus.Posted && status != PaymentApprovalStatus.Rejected && status != PaymentApprovalStatus.Cancelled)
				{
					AssertEquals($"Should be true when status is not Cancelled, Rejected nor Posted, AV_Status={status}.", true, PaymentApprovalItemOSAmountProvider.IsPaymentApprovalValidForMatch(item));
				}
				else
				{
					AssertEquals($"Should be false when status is Cancelled, Rejected or Posted, AV_Status={status}.", false, PaymentApprovalItemOSAmountProvider.IsPaymentApprovalValidForMatch(item));
				}
			}
		}

		public void TestSetOutstandingAmounts()
		{
			AssertSetPaymentAmounts(false, false, 100m, 0m);
			AssertSetPaymentAmounts(false, true, 100m, 0m);
			AssertSetPaymentAmounts(true, false, 100m, 0m);
			AssertSetPaymentAmounts(true, true, 100m, 200m);

			void AssertSetPaymentAmounts(bool isApplicable, bool isRegistryEnabled, ZDecimal expectedLocalAmount, ZDecimal expectedOSAmount)
			{
				AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled);
				var header = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "000001", TestObjectCreator.USD, 1.2m, 1200m, 0m, 1000m, 0m);
				header.AH_IsOSOutstandingAmountApplicable = isApplicable;
				header.AH_OutstandingAmount = 500m;
				header.AH_OSOutstandingAmount = 1000m;
				header.OSOutstandingAmountValueChangeMonitor.Reset();

				var payment = Factory.New<APPaymentApprovalWithoutAuthorisation>();
				var approvalItem = header.GetPaymentApprovalItem(payment);

				PaymentApprovalItemOSAmountProvider.SetPaymentAmounts(approvalItem, 100m, 200m);

				AssertEquals(expectedLocalAmount, approvalItem.A2_PaymentThisRun);
				AssertEquals(expectedOSAmount, approvalItem.A2_OSPaymentThisRun);
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
