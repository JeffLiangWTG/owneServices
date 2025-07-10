using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPreviousDocumentPhase5RuleNR0047ValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new NctsPreviousDocumentPhase5RuleNR0047Validation(null));
	}

	public void TestCheckCSI_ReferenceNumber_NoSpacesAllowed()
	{
		const string errorMessageContainsSpaces = "[NR0047] This type of document must contain an MRN or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). No spaces are allowed.";
		const string referenceNumberContainsSpaces = "5T- 137893-2020-279100";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
			AssertHasMessageError("Type is NMRN, Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, errorMessageContainsSpaces);

			previousDocument.CSI_ReferenceNumber = "5T-137893-2020-279100";
			AssertNoMessageError("Type is NMRN,Not Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, errorMessageContainsSpaces);

			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageError("Type is NMRN, Is Empty", previousDocument.CSI_ReferenceNumberInfo, errorMessageContainsSpaces);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = referenceNumberContainsSpaces;
			AssertNoMessageError("Type is not NMRN, Contains Spaces", previousDocument.CSI_ReferenceNumberInfo, errorMessageContainsSpaces);
		});
	}

	public void TestCheckCSI_ReferenceNumber_Not18AndNoDashes()
	{
		const string errorMessageLengthNot18AndNoDashes = "[NR0047] This type of document must contain an MRN (18 characters) or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office).";
		const string referenceNumberLengthNot18AndNoDashes = "5T137893202027910";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = referenceNumberLengthNot18AndNoDashes;
			AssertHasMessageError("Type is NMRN, Length Not 18 And No Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLengthNot18AndNoDashes);

			previousDocument.CSI_ReferenceNumber = "123456789012345678";
			AssertNoMessageError("Type is NMRN, Length is 18 And No Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLengthNot18AndNoDashes);

			previousDocument.CSI_ReferenceNumber = "1234567890123456-";
			AssertNoMessageError("Type is NMRN, Length Not 18 And have Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLengthNot18AndNoDashes);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = referenceNumberLengthNot18AndNoDashes;
			AssertNoMessageError("Type is not NMRN, Length Not 18 And No Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLengthNot18AndNoDashes);
		});
	}

	public void TestCheckCSI_ReferenceNumber_AlphaNumericAndNoDashes()
	{
		const string errorMessage = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You didn't enter all the fields required.";
		const string referenceNumberLength18AndNoDashes = "5T137893202027910A";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = referenceNumberLength18AndNoDashes;
			AssertNoMessageError("Type is NMRN, Length is 18 And No Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessage);

			previousDocument.CSI_ReferenceNumber = "5T137893202027910@";
			AssertHasMessageError("Type is NMRN, Length is 18 But contains special character", previousDocument.CSI_ReferenceNumberInfo, errorMessage);
		});
	}

	public void TestCheckCSI_ReferenceNumber_LessThanThreeDashes()
	{
		const string errorMessageLessThanThreeDashes = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You didn't enter all the fields required.";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = "5T-137893-2020-279100AAA";
			AssertNoMessageError("Type is NMRN, Have Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLessThanThreeDashes);

			previousDocument.CSI_ReferenceNumber = "5T-137893-2020279100";
			AssertHasMessageError("Type is NMRN, Less Than Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLessThanThreeDashes);

			previousDocument.CSI_ReferenceNumber = "5T-13789322020279100";
			AssertHasMessageError("Type is NMRN, Less Than Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLessThanThreeDashes);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "5T-13789322020279100";
			AssertNoMessageError("Type is not NMRN, Less Than Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageLessThanThreeDashes);
		});
	}

	public void TestCheckCSI_ReferenceNumber_MoreThanThreeDashes()
	{
		const string errorMessageMoreThanThreeDashes = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You entered more fields than required.";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = "5T-137893-2020-279100-AAA";
			AssertHasMessageError("Type is NMRN, More Than Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageMoreThanThreeDashes);

			previousDocument.CSI_ReferenceNumber = "5T-137893-2020-279100AAA";
			AssertNoMessageError("Type is NMRN, have Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageMoreThanThreeDashes);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "5T-137893-2020-279100-AAA";
			AssertNoMessageError("Type is not NMRN, More Than Three Dashes", previousDocument.CSI_ReferenceNumberInfo, errorMessageMoreThanThreeDashes);
		});
	}

	public void TestCheckCSI_ReferenceNumber_ProcedureNotEmpty()
	{
		const string errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the first part must contain a Procedure.";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = "-12C137893-2020-279100";
			AssertHasMessageError("Type is NMRN, Procedure is Empty", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);

			previousDocument.CSI_ReferenceNumber = "5-123137893-2020-279100";
			AssertNoMessageError("Type is NMRN, Procedure is not Empty", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "-12C137893-2020-279100";
			AssertNoMessageError("Type is not NMRN, Procedure is Empty", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);
		});
	}

	public void TestCheckCSI_ReferenceNumber_RegistrationNonNumeric()
	{
		const string errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = "5T-12C137893-2020-279100";
			AssertHasMessageError("Type is NMRN, Registration have to be number", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);

			previousDocument.CSI_ReferenceNumber = "5T-123137893-2020-279100";
			AssertNoMessageError("Type is NMRN, Registration is number", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "5T-12C137893-2020-279100";
			AssertNoMessageError("Type is not NMRN, Registration have to be number", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationNonNumericBetweenFirstAndSecondDashes);
		});
	}

	public void TestCheckCSI_ReferenceNumber_RegistrationWithMoreThan8Digit()
	{
		const string errorMessageRegistrationShouldNotBeMoreThan8Digit = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration between 1 and 8 digits.";
		const string referenceNumberRegistrationWithMoreThan8Digit = "5T-123456789-2020-279100";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = referenceNumberRegistrationWithMoreThan8Digit;
			AssertHasMessageError("Type is NMRN, Registration length is More Than 8 Digit", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationShouldNotBeMoreThan8Digit);

			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-279100";
			AssertNoMessageError("Type is NMRN, Registration length is less than 8 Digit", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationShouldNotBeMoreThan8Digit);

			previousDocument.CSI_ReferenceNumber = "5T--2020-279100";
			AssertNoMessageError("Type is NMRN, Registration length is 0", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationShouldNotBeMoreThan8Digit);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = referenceNumberRegistrationWithMoreThan8Digit;
			AssertNoMessageError("Type is not NMRN, Registration length is More Than 8 Digit", previousDocument.CSI_ReferenceNumberInfo, errorMessageRegistrationShouldNotBeMoreThan8Digit);
		});
	}

	public void TestCheckCSI_ReferenceNumber_YearValidation()
	{
		const string errorMessageYearShouldBe1900TillNow = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.";

		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = $"5T-12345678-{ZDateTime.Now.AddYears(1).Year + 1}-279100";
			AssertHasMessageError("Type is NMRN, Year is grater than now", previousDocument.CSI_ReferenceNumberInfo, errorMessageYearShouldBe1900TillNow);

			previousDocument.CSI_ReferenceNumber = $"5T-12345678-1899-279100";
			AssertHasMessageError("Type is NMRN, Year is older than 1900", previousDocument.CSI_ReferenceNumberInfo, errorMessageYearShouldBe1900TillNow);

			previousDocument.CSI_ReferenceNumber = $"5T-12345678-2020-279100";
			AssertNoMessageError("Type is NMRN, Year valid", previousDocument.CSI_ReferenceNumberInfo, errorMessageYearShouldBe1900TillNow);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = $"5T-12345678-1899-279100";
			AssertNoMessageError("Type is not NMRN, Year Should Be 1900 Till Now", previousDocument.CSI_ReferenceNumberInfo, errorMessageYearShouldBe1900TillNow);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var bill = header.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		previousDocument = goodsItem.PreviousDocuments.AddNew();
	}

	NctsPreviousDocument previousDocument;
}
