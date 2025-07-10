using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class B2AdjustmentsOperationalActionMethodApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSubmissionDate()
		{
			var applicator = new B2AdjustmentsOperationalActionMethodApplicator(Factory);

			applicator.SubmissionDate = ZDateTime.Today.AddDays(1);
			AssertHasWarning(applicator.SubmissionDateInfo, "Submitted Date should not be in the future.");

			applicator.SubmissionDate = ZDateTime.Today.AddDays(-1);
			AssertNoWarning(applicator.SubmissionDateInfo, "Submitted Date should not be in the future.");
		}
	}
}
