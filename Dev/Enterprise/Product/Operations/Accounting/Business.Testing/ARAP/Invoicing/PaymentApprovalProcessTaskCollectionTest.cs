using System;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	abstract class PaymentApprovalProcessTaskCollectionTest : ProcessTaskCollectionTest<PaymentApprovalProcessTaskCollection>
	{
		protected override PaymentApprovalProcessTaskCollection GetCollectionToTestCore()
		{
			var paymentApproval = Factory.NewWithValidTestData(GetExpectedBusinessObjectTypeForPaymentApproval()) as PaymentApprovalBase;
			return new PaymentApprovalProcessTaskCollection(paymentApproval);
		}

		protected abstract Type GetExpectedBusinessObjectTypeForPaymentApproval();
	}

	[TestedType(typeof(PaymentApprovalProcessTaskCollection))]
	class PaymentApprovalProcessTaskCollectionTest_ForAPPaymentApprovalWithAuthorisation : PaymentApprovalProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(APPaymentApprovalWithAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTaskCollection))]
	class PaymentApprovalProcessTaskCollectionTest_ForARPaymentApprovalWithAuthorisation : PaymentApprovalProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(ARPaymentApprovalWithAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTaskCollection))]
	class PaymentApprovalProcessTaskCollectionTest_ForARPaymentApprovalWithoutAuthorisation : PaymentApprovalProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(ARPaymentApprovalWithoutAuthorisation);
	}

	[TestedType(typeof(PaymentApprovalProcessTaskCollection))]
	class PaymentApprovalProcessTaskCollectionTest_ForAPPaymentApprovalWithoutAuthorisation : PaymentApprovalProcessTaskCollectionTest
	{
		protected override Type GetExpectedBusinessObjectTypeForPaymentApproval() => typeof(APPaymentApprovalWithoutAuthorisation);
	}
}

