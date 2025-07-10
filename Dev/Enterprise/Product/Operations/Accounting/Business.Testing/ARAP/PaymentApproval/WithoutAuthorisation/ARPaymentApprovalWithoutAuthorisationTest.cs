using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(ARPaymentApprovalWithoutAuthorisation))]
	public class ARPaymentApprovalWithoutAuthorisationTest : PaymentApprovalWithoutAuthorisationTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var eDocsParsingSupport = TestPaymentApproval as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestProcessEPaymentSecurity()
		{
			Env.Security.ARPaymentProcessingProcessEPayment.IsAllowed = false;
			Assert(!Env.Security.ARPaymentProcessingProcessEPayment.IsAllowed);
			Env.Security.ARPaymentProcessingProcessEPayment.IsAllowed = true;
			Assert(Env.Security.ARPaymentProcessingProcessEPayment.IsAllowed);
		}

		public void TestPaymentApprovalSecurity()
		{
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCheque, false, ReceiptTypes.Cheque, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCash, false, ReceiptTypes.Cash, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCreditCard, false, ReceiptTypes.CreditCard, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentDirectDebit, false, ReceiptTypes.DirectDebit, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentEFT, false, ReceiptTypes.EFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentSFT, false, ReceiptTypes.ScheduledEFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCRQ, false, ReceiptTypes.CollectionRequest, false);

			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCheque, true, ReceiptTypes.Cheque, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCash, true, ReceiptTypes.Cash, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCreditCard, true, ReceiptTypes.CreditCard, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentDirectDebit, true, ReceiptTypes.DirectDebit, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentEFT, true, ReceiptTypes.EFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentSFT, true, ReceiptTypes.ScheduledEFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentCRQ, true, ReceiptTypes.CollectionRequest, true);

			AssertPaymentApprovalSecurityCase(Env.Security.NewReceivablesPaymentDirectDebit, false, ReceiptTypes.DirectCredit, true);
		}

		protected override ZString ExpectedDefaultLedger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestPaymentApproval = (ARPaymentApprovalWithoutAuthorisation)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		ARPaymentApprovalWithoutAuthorisation TestPaymentApproval;
	}
}
