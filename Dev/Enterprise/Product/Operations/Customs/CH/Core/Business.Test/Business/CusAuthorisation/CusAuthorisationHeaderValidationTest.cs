using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusAuthorisationHeaderValidation))]
class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPH_Number_TypeALP()
	{
		const string messageError = "ALP code should begin with ‘ZO’, followed by 10 numeric digits.";

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining(CusAuthorisationHeader.CPH_NumberInfo, messageError);

			CusAuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
			CusAuthorisationHeader.Validation.ValidateCPH_Number();
			AssertHasMessageErrorContaining(CusAuthorisationHeader.CPH_NumberInfo, messageError);

			CusAuthorisationHeader.CPH_Number = "ZO";
			AssertHasMessageErrorContaining("Starts with ZO", CusAuthorisationHeader.CPH_NumberInfo, messageError);

			CusAuthorisationHeader.CPH_Number = "ZOabcdefghij";
			AssertHasMessageErrorContaining("ZO followed by 10 chars", CusAuthorisationHeader.CPH_NumberInfo, messageError);

			CusAuthorisationHeader.CPH_Number = "ZO12345";
			AssertHasMessageErrorContaining("ZO followed by 5 numeric digits", CusAuthorisationHeader.CPH_NumberInfo, messageError);

			CusAuthorisationHeader.CPH_Number = "ZO1234567890";
			AssertNoMessageErrorContaining("Valid Authorization Number", CusAuthorisationHeader.CPH_NumberInfo, messageError);
		});
	}

	public void TestCheckCPH_PermitDescription()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(CusAuthorisationHeader.CPH_PermitDescriptionInfo);
	}

	CusAuthorisationHeader CusAuthorisationHeader => cusAuthorisationHeader ?? (cusAuthorisationHeader = Factory.New<CusAuthorisationHeader>());
	CusAuthorisationHeader cusAuthorisationHeader;
}
