using System;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class APPaymentApprovalWithAuthorisationValidationTest : PaymentApprovalValidationTest
	{
		public void TestCheckAV_AB_FundingBankAccount()
		{
			var paymentApproval = CreatePaymentApprovalForPaymentBatch();
			paymentApproval.RunPreSaveValidation();
			AssertNoErrors(paymentApproval.FundingBankAccountPKInfo);

			paymentApproval.FundingBankAccountPK = TestObjectCreator.USDBankAccount.PK;
			AssertNoErrors(paymentApproval.FundingBankAccountPKInfo);

			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.GBPBankAccount.PK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			paymentBatch.APB_BatchNumber = "0001";

			paymentApproval.AV_APB_PaymentBatch = paymentBatch.PK;
			paymentApproval.RunPreSaveValidation();
			var expectError = $@"This payment belongs to Payment Batch {paymentBatch.APB_BatchNumber}.
Funding Bank Account needs to be changed for the batch in the Manage > Payables > Payment Batches module.";
			AssertHasError(paymentApproval.FundingBankAccountPKInfo, expectError);

			paymentApproval.FundingBankAccountPK = TestObjectCreator.GBPBankAccount.PK;
			AssertNoErrors(paymentApproval.FundingBankAccountPKInfo);
		}

		protected override Type GetValidationBizoType() => typeof(APPaymentApprovalWithAuthorisation);

		protected override Type ExpectValidationType => typeof(APPaymentApprovalWithAuthorisationValidation);

		APPaymentApprovalWithAuthorisation CreatePaymentApprovalForPaymentBatch()
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			approval.AV_RX_NKPaymentCurrency = "CAD";
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			return approval;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		TestObjectCreator testObjectCreator;
	}
}
