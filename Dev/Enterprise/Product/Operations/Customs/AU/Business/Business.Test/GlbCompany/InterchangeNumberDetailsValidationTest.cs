using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class InterchangeNumberDetailsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckNewInterchangeNumber()
		{
			const string tooSmallErrorMessage = "The new interchange number must be greater than the current interchange number by at least 100,000.";
			const string tooLargeErrorMessage = "The new interchange number is too large, cannot be 2,000,000 greater than the current interchange number.";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_CustomsRegistrationNo = "TestCustomsRegistrationNo";

			var interchangeNumberDetails = new InterchangeNumberDetails(company);
			interchangeNumberDetails.currentInterchangeNumber = 1;
			var validator = interchangeNumberDetails.Validation;

			interchangeNumberDetails.NewInterchangeNumber = 0;
			interchangeNumberDetails.Validation.ValidateNewInterchangeNumber();
			AssertHasError("NewInterchangeNumber is 0", interchangeNumberDetails.NewInterchangeNumberInfo, tooSmallErrorMessage);

			interchangeNumberDetails.NewInterchangeNumber = 100_001;
			AssertHasError("NewInterchangeNumber is 100,001", interchangeNumberDetails.NewInterchangeNumberInfo, tooSmallErrorMessage);

			interchangeNumberDetails.NewInterchangeNumber = 100_002;
			AssertNoErrors("NewInterchangeNumber is 100,002", interchangeNumberDetails.NewInterchangeNumberInfo);

			interchangeNumberDetails.NewInterchangeNumber = 2_000_001;
			AssertNoErrors("NewInterchangeNumber is 2,000,001", interchangeNumberDetails.NewInterchangeNumberInfo);

			interchangeNumberDetails.NewInterchangeNumber = 2_000_002;
			AssertHasError("NewInterchangeNumber is 2,000,002", interchangeNumberDetails.NewInterchangeNumberInfo, tooLargeErrorMessage);
		}
	}
}
