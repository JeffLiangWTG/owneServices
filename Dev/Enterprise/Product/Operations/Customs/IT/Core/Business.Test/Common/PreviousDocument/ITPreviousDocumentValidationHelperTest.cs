using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITPreviousDocumentValidationHelperTest : TestCaseWithFactory
{
	public void TestValidateReferenceNumberFormatForNmrn()
	{
		var factory = Factory;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE234100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT135000", "VICENZA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();

		var testCases = new List<(string message, string expected, string referenceNumber)>
			{
				("Contains space characters",
					"This type of document must contain an MRN or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). No spaces are allowed.",
					"5T- 137893-2020-279100"),
				("Length is not 18 and has no dashes",
					"This type of document must contain an MRN (18 characters) or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office).",
					"5T137893"),
				("Has less than 3 dashes",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You didn't enter all the fields required.",
					"5T-137893-2020"),
				("Has more than 3 dashes",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You entered more fields than required.",
					"5T-137893-2020-279100-AAA"),
				("Procedure is empty",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the first part must contain a Procedure.",
					"-137893-2020-279100"),
				("Registration is not numeric",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration",
					"5T-C37893-2020-279100"),
				("Registration has more than 8 digits",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration between 1 and 8 digits.",
					"5T-123456789-2020-279100"),
				("Invalid string for Year",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.",
					"5T-12345678-ABCD-279100"),
				("Year is greater than now",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.",
					$"5T-12345678-{ZDateTime.Now.AddYears(2).Year}-279100"),
				("Year is less than 1900",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.",
					"5T-12345678-1899-279100"),
				("Customs office is empty",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the fourth part must contain a valid Italian Customs Office",
					"5T-137893-2020-"),
				("Not IT customs office",
					"Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the fourth part must contain a valid Italian Customs Office",
					"5T-137893-2020-234100"),
				("A valid format - contains only number and letters",
					string.Empty,
					"25ITQYH01AA05868A1"),
				("A valid format",
					string.Empty,
					"5T-137893-2020-135000")
			};

		foreach (var (message, expected, referenceNumber) in testCases)
		{
			AssertEquals(message, expected, ITPreviousDocumentValidationHelper.ValidateReferenceNumberFormatForNmrn(factory, referenceNumber));
		}
	}
}
