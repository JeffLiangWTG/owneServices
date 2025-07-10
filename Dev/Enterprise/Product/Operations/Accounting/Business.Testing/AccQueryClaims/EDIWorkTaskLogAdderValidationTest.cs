using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	internal class EDIWorkTaskLogAdderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateLogComment()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			AccQueryClaimLogAdder logAdder = new AccQueryClaimLogAdder(claim);
			logAdder.Validation.ValidateAll();
			AssertMandatoryValidationError(logAdder.LogCommentInfo, true);
			logAdder.LogComment = "meh";
			AssertMandatoryValidationError(logAdder.LogCommentInfo, false);
			logAdder = new AccQueryClaimLogAdder(claim);
			logAdder.Validation.ValidateAll();
			AssertMandatoryValidationError(logAdder.LogCommentInfo, true);
		}
	}
}