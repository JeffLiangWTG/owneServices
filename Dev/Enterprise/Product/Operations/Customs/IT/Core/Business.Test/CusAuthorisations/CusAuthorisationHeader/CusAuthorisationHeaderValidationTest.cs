using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorisationHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCPH_NumberFormat_Default()
	{
		CombineAssertions("By default an authorisation number is valid. Other validation rules are implemented in specific tests.", () =>
		{
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorisationHeader.CPH_Number = "";
			AssertNoMessageErrors(authorisationHeader.CPH_NumberInfo);

			authorisationHeader.CPH_Number = "123";
			AssertNoMessageErrors(authorisationHeader.CPH_NumberInfo);

			authorisationHeader.CPH_Number = "123456e";
			AssertNoMessageErrors(authorisationHeader.CPH_NumberInfo);
		});
	}

	public void TestCheckCPH_NumberFormat_ApprovedLocationForExport()
	{
		AssertAuthorisationNumberValidationMessage(CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport);
	}

	public void TestCheckCPH_NumberFormat_ApprovedLocationForImport()
	{
		AssertAuthorisationNumberValidationMessage(CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport);
	}

	public void TestCheckCPH_NumberFormat_DeclarationOfIntent()
	{
		var expectedMessage = ValidationCaptions.CusAuthorisations.Header.DeclarationOfIntentNotValid;
		const string validFirstBlock = "201231112233";
		const string invalidFirstBlock = "AABBCCDDEEFF";
		const string validSecondBlock = "12345123456";
		const string invalidSecondBlock = "AAAAABBBBBB";

		authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.DeclarationOfIntent;
		authorisationHeader.CPH_Number = "";
		AssertNoMessageErrorContaining("Empty authorisation number", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = validFirstBlock;
		AssertHasMessageErrorContaining("Invalid length", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = invalidFirstBlock + invalidSecondBlock;
		AssertHasMessageErrorContaining("Valid length, both blocks invalid", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = invalidFirstBlock + validSecondBlock;
		AssertHasMessageErrorContaining("Valid length, wrong first block", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = validFirstBlock + invalidSecondBlock;
		AssertHasMessageErrorContaining("Valid length, wrong second block", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = validFirstBlock + validSecondBlock;
		AssertNoMessageErrorContaining("Valid length, both blocks valid", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = "X";
		AssertNoMessageErrorContaining("Valid placeholder", authorisationHeader.CPH_NumberInfo, expectedMessage);

		authorisationHeader.CPH_Number = "Y";
		AssertHasMessageErrorContaining("Invalid placeholder", authorisationHeader.CPH_NumberInfo, expectedMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		authorisationHeader = Factory.New<CusAuthorisationHeader>();
	}
	CusAuthorisationHeader authorisationHeader;

	void AssertAuthorisationNumberValidationMessage(ZString authorisationType)
	{
		authorisationHeader.CPH_Type = authorisationType;

		CombineAssertions(authorisationType, () =>
		{
			authorisationHeader.CPH_Number = string.Empty;
			AssertNoMessageErrorContaining("Empty", authorisationHeader.CPH_NumberInfo, AuthorisationNumberInvalidFormatMessage);

			authorisationHeader.CPH_Number = "Q3496Q";
			AssertHasMessageErrorContaining("Must start with a digit", authorisationHeader.CPH_NumberInfo, AuthorisationNumberInvalidFormatMessage);

			authorisationHeader.CPH_Number = "13496q";
			AssertHasMessageErrorContaining("Must end with an uppercase letter", authorisationHeader.CPH_NumberInfo, AuthorisationNumberInvalidFormatMessage);

			authorisationHeader.CPH_Number = "13496Q";
			AssertNoMessageErrorContaining("Valid with less than 6 characters", authorisationHeader.CPH_NumberInfo, AuthorisationNumberInvalidFormatMessage);

			authorisationHeader.CPH_Number = "1349645Q";
			AssertNoMessageErrorContaining("Valid with more than 6 characters", authorisationHeader.CPH_NumberInfo, AuthorisationNumberInvalidFormatMessage);
		});
	}

	const string AuthorisationNumberInvalidFormatMessage = "The Authorization Number must start with one or more digits and end with one upper alphabetical character.";
}
