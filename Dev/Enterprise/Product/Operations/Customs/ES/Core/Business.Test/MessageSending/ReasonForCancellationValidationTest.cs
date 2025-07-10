using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ReasonForCancellationValidationTest : TestCaseWithFactory
	{
		public void TestValidateCode()
		{
			CombineAssertions(() =>
			{
				var cancellation = new ReasonForCancellation(Factory);
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cancellation.CodeInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(cancellation.CodeInfo, "XXX", ReasonForCancellationList.Codes.DuplicatedSad);
			});
		}

		public void TestValidateReason()
		{
			var cancellation = new ReasonForCancellation(Factory);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cancellation.ReasonInfo);
		}
	}
}
