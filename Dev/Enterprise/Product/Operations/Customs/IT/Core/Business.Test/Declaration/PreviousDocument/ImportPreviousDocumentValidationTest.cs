using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ImportPreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_SubType()
	{
		previousDocument.CSI_Procedure = "LC";
		previousDocument.CSI_SubType = ZString.Empty;
		AssertNoMessageErrors(previousDocument.CSI_SubTypeInfo);
	}

	public void TestCheckCSI_Status()
	{
		previousDocument.CSI_Procedure = ZString.Empty;
		previousDocument.CSI_Status = "AA";
		AssertNoMessageErrors(previousDocument.CSI_StatusInfo);
	}

	public void TestCheckCSI_Tariff()
	{
		previousDocument.CSI_Procedure = "2";
		previousDocument.Validation.ValidateCSI_Tariff();
		AssertNoWarnings(previousDocument.CSI_TariffInfo);
	}

	public void TestCheckCSI_Quantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var importTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Italy, Universal.Constants.TariffTypes.Import);
		Factory.Save();

		var importTariffWithUOM = helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "1111111111", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateTariffUOM(importTariffWithUOM, UnitOfMeasureTypes.AdditionalUOMType, "NAR");
		helper.CreateTariff(Core.Constants.CountryCodes.Italy, importTariffType.PK, "2222222222", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		previousDocument.CSI_Procedure = "2";
		previousDocument.FormattedTariff = "1111111111";
		previousDocument.CSI_Quantity2 = 0m;
		previousDocument.Validation.ValidateCSI_Quantity2();
		AssertNoMessageErrors(previousDocument.CSI_Quantity2Info);
	}

	public void TestCheckCSI_UnitOfQuantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		previousDocument.CSI_UnitOfQuantity2 = "XXX";
		AssertNoMessageErrors(previousDocument.CSI_UnitOfQuantity2Info);
	}

	public void TestCheckCSI_Quantity3()
	{
		previousDocument.CSI_Quantity3 = 123.45678;
		previousDocument.CSI_UnitOfQuantity3 = "G";

		previousDocument.Validation.ValidateCSI_Quantity3();

		AssertNoNotifications(previousDocument.CSI_Quantity3Info);
	}

	public void TestCheckCSI_UnitOfQuantity3()
	{
		previousDocument.CSI_UnitOfQuantity3 = "X";

		previousDocument.Validation.ValidateCSI_UnitOfQuantity3();

		AssertNoNotifications(previousDocument.CSI_UnitOfQuantity3Info);
	}

	public void TestCheckCSI_LineNo()
	{
		previousDocument.CSI_Procedure = "MRN";
		CombineAssertions("When CSI_Procedure = MRN", () =>
		{
			previousDocument.CSI_LineNo = 0;
			AssertHasMessageErrorContaining("When CSI_LineNo = 0", previousDocument.CSI_LineNoInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_LineNo = 1;
			AssertNoMessageErrors("When CSI_LineNo = 1,", previousDocument.CSI_LineNoInfo);
		});

		previousDocument.CSI_Procedure = "A3";
		CombineAssertions("When CSI_Procedure = A3", () =>
		{
			previousDocument.CSI_LineNo = 0;
			AssertHasMessageErrorContaining("When CSI_LineNo = 0, CSI_Procedure = A3", previousDocument.CSI_LineNoInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_LineNo = 1;
			AssertNoMessageErrors("When CSI_LineNo = 1, CSI_Procedure = A3", previousDocument.CSI_LineNoInfo);
		});
	}

	public void TestCheckCSI_LineNo_MR1()
	{
		previousDocument.CSI_Procedure = "MR1";

		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining(previousDocument.CSI_LineNoInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_LineNo = 1;
			AssertNoMessageErrors(previousDocument.CSI_LineNoInfo);
		});
	}

	public void TestCheckCSI_ReferenceNumberFormat()
	{
		const string expectedMessage = "The number exceeds the maximum length in the message";

		previousDocument.CSI_Procedure = "2";
		previousDocument.CSI_ReferenceNumber = "012345678901234567890123456789";
		AssertNoMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, expectedMessage);

		previousDocument.CSI_CustomsOffice = "IT137100";
		previousDocument.CSI_DateOfIssue = ZDateTime.Today;
		previousDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumberInfo, expectedMessage);
	}

	public void TestCheckCSI_ReferenceNumber2_A3()
	{
		const string maxLengthMessageError = "The MRN field must be entered with 18 characters.";

		previousDocument.CSI_Procedure = "A3";

		previousDocument.CSI_ReferenceNumber2 = ZString.Empty;
		AssertHasMessageErrorContaining(previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);

		previousDocument.CSI_ReferenceNumber2 = "123";
		AssertHasMessageError(previousDocument.CSI_ReferenceNumber2Info, maxLengthMessageError);

		previousDocument.CSI_ReferenceNumber2 = "25ITQX3300051541U2";
		AssertNoMessageErrors(previousDocument.CSI_ReferenceNumber2Info);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		previousDocument = declaration.PreviousDocuments.AddNew();
	}

	PreviousDocument previousDocument;
}
