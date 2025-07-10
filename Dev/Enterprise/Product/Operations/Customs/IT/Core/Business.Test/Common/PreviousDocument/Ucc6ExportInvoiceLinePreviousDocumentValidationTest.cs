using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using MandatoryValidation = CargoWise.EntityFramework.MandatoryValidation;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class Ucc6ExportInvoiceLinePreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_ReferenceNumberFormat_WhenDocumentTypeIsNMRN()
	{
		var factory = Factory;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		_ = helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Italy, parent: eun);
		_ = helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		_ = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.Italy, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT135000", "VICENZA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.DeclarationOrNotificationMrn;

			var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;
			SetReferenceNumberAndValidate("5T- 137893-2020-279100");
			AssertHasMessageError("Contains space characters", targetPropertyInfo, "This type of document must contain an MRN or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). No spaces are allowed.");

			SetReferenceNumberAndValidate("5T137893");
			AssertHasMessageError("Length is not 18 and has no dashes", targetPropertyInfo, "This type of document must contain an MRN (18 characters) or a registration number as per the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office).");

			SetReferenceNumberAndValidate("5T-137893-2020");
			AssertHasMessageError("Has less than 3 dashes", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You didn't enter all the fields required.");

			SetReferenceNumberAndValidate("5T-137893-2020-279100-AAA");
			AssertHasMessageError("Has more than 3 dashes", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office). You entered more fields than required.");

			SetReferenceNumberAndValidate("-137893-2020-279100");
			AssertHasMessageError("Procedure is empty", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the first part must contain a Procedure.");

			SetReferenceNumberAndValidate("5T-C37893-2020-279100");
			AssertHasMessageError("Registration is not numeric", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration");

			SetReferenceNumberAndValidate("5T-123456789-2020-279100");
			AssertHasMessageError("Registration has more than 8 digits", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the second part must contain a number of registration between 1 and 8 digits.");

			SetReferenceNumberAndValidate("5T-12345678-ABCD-279100");
			AssertHasMessageError("Invalid string for Year", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.");

			SetReferenceNumberAndValidate($"5T-12345678-{ZDateTime.Now.AddYears(2).Year}-279100");
			AssertHasMessageError("Year is grater than now", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.");

			SetReferenceNumberAndValidate("5T-12345678-1899-279100");
			AssertHasMessageError("Year is less than 1900", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the third part must contain the year of issuing.");

			SetReferenceNumberAndValidate("5T-137893-2020-279100");
			AssertHasMessageError("Non-IT customs office", targetPropertyInfo, "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the fourth part must contain a valid Italian Customs Office");

			SetReferenceNumberAndValidate("5T-137893-2020-135000");
			AssertNoMessageErrors("A valid format", targetPropertyInfo);
		}
	}

	public void TestCSI_ReferenceNumber_VerifyDashCount_WhenDocumentTypeIsN337()
	{
		const string expectedMessageErrorForLessNoOfDashes = commonExpectedMessageErrorPart + ". You didn't enter all the fields required";
		const string expectedMessageErrorForMoreNoOfDashes = commonExpectedMessageErrorPart + ". You entered more fields than required";

		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

			CombineAssertions("When Dashes are less than required", () =>
			{
				SetReferenceNumberAndValidate("A2-122323-123221111");
				AssertHasMessageErrorContaining("When Number has 2 dashes", targetPropertyInfo, expectedMessageErrorForLessNoOfDashes);

				SetReferenceNumberAndValidate("A2122323111");
				AssertHasMessageErrorContaining("When Number has no dash", targetPropertyInfo, expectedMessageErrorForLessNoOfDashes);
			});

			CombineAssertions("When Dashes are more than required", () =>
			{
				SetReferenceNumberAndValidate("A2-122323-1232-2-1111");
				AssertHasMessageErrorContaining("When Number has 4 dashes", targetPropertyInfo, expectedMessageErrorForMoreNoOfDashes);

				SetReferenceNumberAndValidate("A2-122323--1232-21111");
				AssertHasMessageErrorContaining("When Number has 4 dashes (consecutive)", targetPropertyInfo, expectedMessageErrorForMoreNoOfDashes);

				SetReferenceNumberAndValidate("A-1-sw122-123222-11212--111");
				AssertHasMessageErrorContaining("When Number has 5 dashes", targetPropertyInfo, expectedMessageErrorForMoreNoOfDashes);
			});

			CombineAssertions("When Dashes are equal to 3", () =>
			{
				SetReferenceNumberAndValidate("5T-137893-2020-279100");
				AssertNoMessageErrorContaining(targetPropertyInfo, expectedMessageErrorForLessNoOfDashes);
				AssertNoMessageErrorContaining(targetPropertyInfo, expectedMessageErrorForMoreNoOfDashes);
			});
		}
	}

	public void TestCSI_ReferenceNumber_FirstPart_WhenDocumentTypeIsN337()
	{
		var allowedValuesInFirstPart = new[] { "2", "2S", "2T", "5", "5S", "5T", "7", "7S", "7T", "PF", "NN" };
		const string expectedMessageError = "Reference fields must start with a valid Procedure (accepted codes are 2, 2S, 2T, 5, 5S, 5T, 7, 7S, 7T, PF, NN)";

		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

			CombineAssertions("When First Part is not in the allowed list", () =>
			{
				SetReferenceNumberAndValidate("2A-1234567-2020-103293");
				AssertHasMessageErrorContaining("When First Part Code = 2A", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("555-1234567-2020-103293");
				AssertHasMessageErrorContaining("When First Part Code = 555", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("-1234567-2020-103293");
				AssertHasMessageErrorContaining("When First Part Code = EMPTY", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("6-1234567-2020-103293");
				AssertHasMessageErrorContaining("When First Part Code = 6", targetPropertyInfo, expectedMessageError);
			});

			CombineAssertions("When First Part is in allowed list", () =>
			{
				foreach (var code in allowedValuesInFirstPart)
				{
					var number = FormattableString.Invariant($"{code}-123433-2022-1232342");
					SetReferenceNumberAndValidate(number);
					AssertNoMessageErrorContaining($"When First Part Code={code}", targetPropertyInfo, expectedMessageError);
				}
			});
		}
	}

	public void TestCSI_ReferenceNumber_SecondPart_WhenDocumentTypeIsN337()
	{
		const string expectedMessageErrorForInvalidFormat = commonExpectedMessageErrorPart + ", the second part must contain a number of registration";
		const string expectedMessageErrorForLength = commonExpectedMessageErrorPart + ", the second part must contain a number of registration between 1 to 8 digits";
		const string expectedMessageErrorForSpaces = commonExpectedMessageErrorPart + ", spaces are not admitted in between";

		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

			CombineAssertions(() =>
			{
				SetReferenceNumberAndValidate("2-A123322-2020-2389444");
				AssertHasMessageErrorContaining("When Second Part contains alphabets", targetPropertyInfo, expectedMessageErrorForInvalidFormat);

				SetReferenceNumberAndValidate("2-1!23322-2020-2389444");
				AssertHasMessageErrorContaining("When Second Part contains special characters", targetPropertyInfo, expectedMessageErrorForInvalidFormat);

				SetReferenceNumberAndValidate("2-1 233 2-2020-2389444");
				AssertHasMessageErrorContaining("When Second Part contains spaces", targetPropertyInfo, expectedMessageErrorForSpaces);

				SetReferenceNumberAndValidate("2-123456789-2020-2389444");
				AssertHasMessageErrorContaining("When Second Part contains more than 8 digits", targetPropertyInfo, expectedMessageErrorForLength);

				SetReferenceNumberAndValidate("2-12345678-2020-2389444");
				AssertNoMessageErrorContaining("When Second Part contains exactly 8 digits", targetPropertyInfo, expectedMessageErrorForLength);

				SetReferenceNumberAndValidate("2--2020-2389444");
				AssertHasMessageErrorContaining("When Second Part is empty", targetPropertyInfo, expectedMessageErrorForLength);

				SetReferenceNumberAndValidate("2-1234567-2020-2389444");
				AssertNoMessageErrorContaining("When Second Part contains less than 8 digits", targetPropertyInfo, expectedMessageErrorForLength);
			});
		}
	}

	[TestDate(2024, 12, 31)]
	public void TestCSI_ReferenceNumber_ThirdPart_WhenDocumentTypeIsN337()
	{
		const string expectedMessageError = commonExpectedMessageErrorPart + ", the third part must contain the year of issuing";
		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

			CombineAssertions(() =>
			{
				SetReferenceNumberAndValidate("5T-137893-1-279100");
				AssertHasMessageErrorContaining("When Third Part is 1", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1A00-279100");
				AssertHasMessageErrorContaining("When Third Part contains an alphabet", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1899-279100");
				AssertHasMessageErrorContaining("When Third Part is a year before 1900", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate(FormattableString.Invariant($"5T-137893-2025-279100"));
				AssertHasMessageErrorContaining("When Third Part is a year is in the future", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate(FormattableString.Invariant($"5T-137893-2023-279100"));
				AssertNoMessageErrorContaining("When Third Part is a last year", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate(FormattableString.Invariant($"5T-137893-2024-279100"));
				AssertNoMessageErrorContaining("When Third Part is a current year", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1900-279100");
				AssertNoMessageErrorContaining("When Third Part is 1900", targetPropertyInfo, expectedMessageError);
			});
		}
	}

	public void TestCSI_ReferenceNumber_FourthPart_WhenDocumentTypeIsN337()
	{
		const string expectedMessageError = commonExpectedMessageErrorPart + ", the fourth part must contain a valid Italian Customs Office";

		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT12323", "CAMPOBASSO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT32342", "MILANOOFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "99887", "FRAOFFICE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "012322", "EUOffice", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeListTypes.TemporaryStorageDeclaration;

			CombineAssertions(() =>
			{
				SetReferenceNumberAndValidate("5T-137893-1991-");
				AssertHasMessageErrorContaining("When Fourth Part is empty", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1A00-ABCDE");
				AssertHasMessageErrorContaining("When Fourth Part is invalid customs code", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1899-012322");
				AssertHasMessageErrorContaining("When Fourth Part is EU Customs Code", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1899-99887");
				AssertHasMessageErrorContaining("When Fourth Part is DE Customs Code", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1900-12323");
				AssertNoMessageErrorContaining("When Fourth Part is Italian Customs Code", targetPropertyInfo, expectedMessageError);

				SetReferenceNumberAndValidate("5T-137893-1900-32342");
				AssertNoMessageErrorContaining("When Fourth Part is Italian Customs Code", targetPropertyInfo, expectedMessageError);
			});
		}
	}

	public void TestCSI_ReferenceNumber_MandatoryValidation_ForAnyDocumentType()
	{
		var targetPropertyInfo = previousDocument.CSI_ReferenceNumberInfo;

		using (TemporarilyClearAndSetUcc6Configuration(isActive: true))
		{
			previousDocument.CSI_Code = ZString.Empty;
			SetReferenceNumberAndValidate(ZString.Empty);
			AssertNoMessageErrorContaining("When Type and Reference Number are empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "765";
			SetReferenceNumberAndValidate(ZString.Empty);
			AssertHasMessageErrorContaining("When Type is set and reference number is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "N111";
			SetReferenceNumberAndValidate("1234");
			AssertNoMessageErrorContaining("When Type and Reference Number are not empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		previousDocument = invoiceLine.PreviousDocuments.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	PreviousDocument previousDocument;

	IDisposable TemporarilyClearAndSetUcc6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	void SetReferenceNumberAndValidate(ZString referenceNumber)
	{
		previousDocument.CSI_ReferenceNumber = referenceNumber;
		previousDocument.Validation.ValidateCSI_ReferenceNumber();
	}

	const string commonExpectedMessageErrorPart = "Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office without country)";
}
