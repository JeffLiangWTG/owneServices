using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AAAddressRequirementTest : TestCaseWithFactory
	{
		public void TestE2_GovRegNumTypeMandatoryExceptDirection_ReleaseOnDocument()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(colsDirection.AAIdInfo);

			colsDirection.QCD_Direction = "Release on Documents";
			colsDirection.AAId = ZString.Empty;
			ValidationTestHelper.AssertNoWarningIfNotEntered(colsDirection.AAIdInfo, "AAID is not required");
		}

		public void TestOtherFieldsAreOptional()
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			colsDirection.AAId = "AAAA";
			colsDirection.AAAddress.Validation.ValidateAll();
			AssertNoNotifications("There shouldn't be any notifications", colsDirection.AAAddress);
		}

		public void TestValidateE2_Address1() => CombineAssertions(() =>
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			colsDirection.AALocation = ZString.Replicate('a', 20);
			AssertNoMessageError(colsDirection.AALocationInfo, "The maximum length 20 has been exceeded.");

			colsDirection.AALocation = ZString.Replicate('a', 21);
			AssertHasMessageError(colsDirection.AALocationInfo, "The maximum length 20 has been exceeded.");
		});

		public void TestValidateE2_CompanyName() => CombineAssertions(() =>
		{
			var colsDirection = Factory.New<QuarantineColsDirection>();
			colsDirection.AAName = ZString.Replicate('a', 100);
			AssertNoMessageError(colsDirection.AANameInfo, "The maximum length 100 has been exceeded.");

			colsDirection.AAName = ZString.Replicate('a', 101);
			AssertHasMessageError(colsDirection.AANameInfo, "The maximum length 100 has been exceeded.");
		});
	}
}
