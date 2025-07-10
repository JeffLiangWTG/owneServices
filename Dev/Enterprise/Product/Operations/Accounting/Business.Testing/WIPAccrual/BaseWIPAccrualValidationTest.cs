using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.WIPAccrual.Testing
{
	public class BaseWIPAccrualValidationTest : TransactionLineValidation_InnerTest
	{
		public void TestCriticalValidationNotificationsRemovedBeforeValidateAll()
		{
			WIP testWIP = Factory.NewWithValidTestData<WIP>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = testWIP.PK;
			testWIP.Validation.ValidateAll();
			AssertNoRowErrors(testWIP);
			string expectedError = "expectedError";
			testWIP.AddRowError(expectedError);
			AssertHasRowErrorContaining(testWIP, expectedError);
			testWIP.Validation.ValidateAll();
			AssertNoRowErrors(testWIP);
		}
	}
}
