using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPreviousDocumentPhase5ValidationTest : TestCaseWithFactory
{
	public void TestCheckCSI_SubType()
	{
		previousDocument.Validation.ValidateCSI_SubType();
		AssertNoMessageErrors(previousDocument.CSI_SubTypeInfo);
	}

	public void TestCheckCSI_ItemNumberWithRefCusCodeAttribute()
	{
		var refCusCodes = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
			"IT",
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
			new ZString[] { "ABCD", "C651", "C658" },
			("Level", "Item"), ("ItemNumber", "Y")).ToList();
		Factory.Save();

		CombineAssertions(() =>
		{
			previousDocument.CSI_Code = "C651";
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertNoMessageErrorContaining("CusCode C651 and ItemNumber is empty", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "C658";
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertNoMessageErrorContaining("CusCode C658 and ItemNumber is empty", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = "ABCD";
			previousDocument.Validation.ValidateCSI_ItemNumber();
			AssertHasMessageErrorContaining("CusCode has ItemNumber attribute and ItemNumber is empty", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_ItemNumber = 3;
			AssertNoMessageErrorContaining("CusCode has ItemNumber attribute and ItemNumber is filled", previousDocument.CSI_ItemNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_Quantity_AddMessageError_WhenTotalNumberOfDigitsIsBetween17And19()
	{
		const string expectedMessageError = "The maximum allowed number of digits is 16.";
		var quantityInfo = previousDocument.CSI_QuantityInfo;

		CombineAssertions("Length is 16 Digits", () =>
		{
			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456.000000");
			AssertNoMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("1,234,5678,901.234560");
			AssertNoMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("123,4567,890.123456");
			AssertNoMessageError(quantityInfo, expectedMessageError);
		});

		CombineAssertions("Length is 17 Digits", () =>
		{
			previousDocument.CSI_Quantity = new ZDecimal("12,345,678,901,234,567.000000");
			AssertHasMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456.7000");
			AssertHasMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("123,456,789,012,34567");
			AssertHasMessageError(quantityInfo, expectedMessageError);
		});

		CombineAssertions("Length is 18 Digits", () =>
		{
			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456.7800");
			AssertHasMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("12,345,678,901,234,567.800");
			AssertHasMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("123,456,789,012,345678");
			AssertHasMessageError(quantityInfo, expectedMessageError);
		});

		CombineAssertions("Length is 19 Digits", () =>
		{
			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456,789");
			AssertHasMessageError(quantityInfo, expectedMessageError);

			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456.789");
			AssertHasMessageError(quantityInfo, expectedMessageError);
		});

		CombineAssertions("Length is 20 Digits", () =>
		{
			previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890,123,456.7891");
			AssertNoMessageError(quantityInfo, expectedMessageError);
		});
	}

	public void TestCheckCSI_QuantityIsValidZDecimal()
	{
		const string expectedErrorWhenIsTooLarge = "the maximum value allowed for Quantity is";

		previousDocument.CSI_Quantity = new ZDecimal("1,234,567,890.12345");
		AssertNoErrorContaining(previousDocument.CSI_QuantityInfo, expectedErrorWhenIsTooLarge);

		previousDocument.CSI_Quantity = new ZDecimal("9,999,999,999,999.999999");
		AssertNoErrorContaining("the maximum value allowed ", previousDocument.CSI_QuantityInfo, expectedErrorWhenIsTooLarge);

		previousDocument.CSI_Quantity = new ZDecimal("12,345,678,901,234.123456");
		AssertHasErrorContaining("When 20 digits", previousDocument.CSI_QuantityInfo, expectedErrorWhenIsTooLarge);
	}

	public void TestCheckCSI_ReferenceNumber_CustomsOfficeValidation()
	{
		const string errorMessageInvalidCustomsOffice = "[NR0047] Reference fields must be as for the following example: 5T-137893-2020-279100 (Procedure, Number without CIN, Year, Customs Office), the fourth part must contain a valid Italian Customs Office";
		var factory = Factory;
		var helper = new UniversalReferenceTestDataHelper(factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IE234100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT135000", "VICENZA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
		CombineAssertions("NR0047 is Active", () =>
		{
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.NMRN;
			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-234100";
			AssertHasMessageError("Type is NMRN, not IT", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);

			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-135000";
			AssertNoMessageError("Type is NMRN, valid IT Customs Office 135000", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);

			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020";
			AssertNoMessageErrorContaining("Type is NMRN, in Reference fields - Custom Office is not filled.", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);

			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-234100-2151";
			AssertNoMessageErrorContaining("Type is NMRN, in Reference fields - extra input", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);

			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-123456";
			AssertHasMessageError("Type is NMRN, not in list", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);

			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
			previousDocument.CSI_ReferenceNumber = "5T-12345678-2020-123456";
			AssertNoMessageError("Type is not NMRN, Invalid IT Customs Office 234100", previousDocument.CSI_ReferenceNumberInfo, errorMessageInvalidCustomsOffice);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		previousDocument = nctsHeader
			.Bills.AddNew()
			.GoodsItems.AddNew()
			.PreviousDocuments.AddNew();
	}

	NctsPreviousDocument previousDocument;
}
