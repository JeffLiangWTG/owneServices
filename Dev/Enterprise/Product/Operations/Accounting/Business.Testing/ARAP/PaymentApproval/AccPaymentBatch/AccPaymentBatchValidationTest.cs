using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class AccPaymentBatchValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestAPB_PaymentType()
		{
			var accPaymentBatch = Factory.New<AccPaymentBatch>();
			accPaymentBatch.APB_PaymentType = "";
			accPaymentBatch.Validation.ValidateAPB_PaymentType();
			AssertHasError("payment type should not be empty",accPaymentBatch.APB_PaymentTypeInfo, "Please enter a Payment Type.");

			foreach (var paymentType in accPaymentBatch.Lookups.PaymentTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				accPaymentBatch.APB_PaymentType = paymentType;
				accPaymentBatch.Validation.ValidateAPB_PaymentType();
				AssertNoError(accPaymentBatch.APB_PaymentTypeInfo, "Enter a valid selection.");
			}

			accPaymentBatch.APB_PaymentType = "ZZZ";
			accPaymentBatch.Validation.ValidateAPB_PaymentType();
			AssertHasError(accPaymentBatch.APB_PaymentTypeInfo, "Enter a valid selection.");
		}

		public void TestAPB_AB_FundingBankAccount()
		{
			var accPaymentBatch = Factory.New<AccPaymentBatch>();

			accPaymentBatch.APB_AB_FundingBankAccount = ZGuid.Empty;
			AssertNoErrors(accPaymentBatch.APB_AB_FundingBankAccountInfo);

			accPaymentBatch.APB_AB_FundingBankAccount = ZGuid.Invalid;
			AssertHasError(accPaymentBatch.APB_AB_FundingBankAccountInfo, "Enter a valid selection.");

			accPaymentBatch.APB_AB_FundingBankAccount = TestObjectCreator.AUDBankAccount.PK;
			AssertNoErrors(accPaymentBatch.APB_AB_FundingBankAccountInfo);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
