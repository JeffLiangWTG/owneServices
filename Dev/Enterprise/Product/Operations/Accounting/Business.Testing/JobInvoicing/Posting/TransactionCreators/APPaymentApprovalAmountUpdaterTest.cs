using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class APPaymentApprovalAmountUpdaterTest : TestCaseWithFactory
	{
		public void TestUpdateAmountsOnAllPaymentsAndInvoiceLinks()
			=> AssertUpdateAmountsOnAllPaymentsAndInvoiceLinks(isEnableNewOSOutstandingAmountFeature: false);

		public void TestUpdateAmountsOnAllPaymentsAndInvoiceLinks_EnableNewOSOutstandingAmountFeature()
			=> AssertUpdateAmountsOnAllPaymentsAndInvoiceLinks(isEnableNewOSOutstandingAmountFeature: true);

		public void AssertUpdateAmountsOnAllPaymentsAndInvoiceLinks(bool isEnableNewOSOutstandingAmountFeature)
		{
			AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature);

			var transactions = new TransactionCreatorHashtable();

			var payment1 = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			transactions.AddAPPaymentApproval(payment1, "org1", "", "", "", "");
			var invoice11 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var approvalItem11 = invoice11.GetPaymentApprovalItem(payment1);
			var invoice12 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var approvalItem12 = invoice12.GetPaymentApprovalItem(payment1);

			var payment2 = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			transactions.AddAPPaymentApproval(payment2, "org2", "", "", "", "");
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var approvalItem2 = invoice2.GetPaymentApprovalItem(payment2);

			invoice11.AH_OSTotalAmount = 200;
			invoice11.AH_LocalExTaxAmount = 100;

			invoice12.AH_OSTotalAmount = 30;
			invoice12.AH_LocalExTaxAmount = 20;

			invoice2.AH_OSTotalAmount = 5;
			invoice2.AH_LocalExTaxAmount = 4;

			IAPPaymentApprovalAmountUpdater updater = new APPaymentApprovalAmountUpdater();
			updater.UpdateAmountsOnAllPaymentsAndInvoiceLinks(transactions);
			CombineAssertions(() =>
			{
				AssertEquals("payment1.AV_Amount", 230m, payment1.AV_Amount);
				AssertEquals("payment1.AV_Calc_LocalAmount", 120m, payment1.AV_Calc_LocalAmount);
				AssertEquals("approvalItem11.A2_PaymentThisRun", -100m, approvalItem11.A2_PaymentThisRun);
				AssertEquals("approvalItem11.A2_OSPaymentThisRun", isEnableNewOSOutstandingAmountFeature ? -200m : 0m, approvalItem11.A2_OSPaymentThisRun);
				AssertEquals("approvalItem12.A2_PaymentThisRun", -20m, approvalItem12.A2_PaymentThisRun);
				AssertEquals("approvalItem11.A2_OSPaymentThisRun", isEnableNewOSOutstandingAmountFeature ? -30m : 0m, approvalItem12.A2_OSPaymentThisRun);

				AssertEquals("payment2.AV_Amount", 5m, payment2.AV_Amount);
				AssertEquals("payment2.AV_Calc_LocalAmount", 4m, payment2.AV_Calc_LocalAmount);
				AssertEquals("approvalItem2.A2_PaymentThisRun", -4m, approvalItem2.A2_PaymentThisRun);
			});
		}

		public void TestUpdateAmountsOnAllPaymentsAndInvoiceLinks_ApprovesPayments()
		{
			AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreatePaymentAuthorisationSettingsExample());

			var transactions = new TransactionCreatorHashtable();

			var payment = Factory.New<APPaymentApprovalWithAuthorisation>();
			transactions.AddAPPaymentApproval(payment, "", "", "", "", "");
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice));
			var approvalItem = invoice.GetPaymentApprovalItem(payment);

			invoice.AH_OSTotalAmount = 200;
			invoice.AH_LocalExTaxAmount = 10000;

			Assert("Precondition: payment.IsAwaitingApproval", payment.IsAwaitingApproval);

			IAPPaymentApprovalAmountUpdater updater = new APPaymentApprovalAmountUpdater();
			updater.UpdateAmountsOnAllPaymentsAndInvoiceLinks(transactions);
			CombineAssertions(() =>
			{
				AssertEquals("PostCondition: payment.AV_Amount", 200m, payment.AV_Amount);
				AssertEquals("PostCondition: payment.AV_Calc_LocalAmount", 10000m, payment.AV_Calc_LocalAmount);
				AssertEquals("PostCondition: approvalItem.A2_PaymentThisRun", -10000m, approvalItem.A2_PaymentThisRun);
				Assert("payment.IsFullyApproved", payment.IsFullyApproved);
			});
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
