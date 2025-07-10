using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_ItemNumber()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_ItemNumber = -1;
		AssertHasNotifications("Item number cannot be negative", previousDocument.CSI_ItemNumberInfo);
		previousDocument.CSI_ItemNumber = -32453;
		AssertHasNotifications("Item number cannot be negative", previousDocument.CSI_ItemNumberInfo);
		previousDocument.CSI_ItemNumber = 10000;
		AssertHasNotifications("Item number cannot be more than 4 digits", previousDocument.CSI_ItemNumberInfo);
		previousDocument.CSI_ItemNumber = 9999;
		AssertNoNotifications("Item number should accept positive number less than 4 digits", previousDocument.CSI_ItemNumberInfo);
		previousDocument.CSI_ItemNumber = 0123;
		AssertNoNotifications("Item number should accept positive number less than 4 digits", previousDocument.CSI_ItemNumberInfo);
		previousDocument.CSI_ItemNumber = 0;
		AssertNoNotifications("Item number should accept 0", previousDocument.CSI_ItemNumberInfo);
	}

	public void TestCheckCSI_CustomsOffice()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_CustomsOffice = "WrongCode";
		AssertHasMessageErrors("No custom office will use this code", previousDocument.CSI_CustomsOfficeInfo);
	}

	public void TestCheckCSI_ReferenceNumber_Mandatory()
	{
		var doc = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			doc.CSI_Code = "ABC";
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining("CSI_ReferenceNumber is empty", doc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			doc.CSI_ReferenceNumber = "test";
			doc.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("CSI_ReferenceNumber is empty", doc.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestValidateCSI_ReferenceNumber_InvoiceLine_Mandatory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

		var previousDocument = invoiceLine.PreviousDocuments.AddNew();
		previousDocument.CSI_Code = "ABC";

		CombineAssertions(() =>
		{
			previousDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining("Empty", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_ReferenceNumber = "REF";
			AssertNoMessageErrorContaining("Entered", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_ReferenceNumber()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_ReferenceNumber = "#fagagatrta%";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
		previousDocument.CSI_ReferenceNumber = "xxxxx#fagagatrta%xxx11242324";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
		previousDocument.CSI_ReferenceNumber = "1xxxx#fagagatrta%xxx11242324";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
		previousDocument.CSI_ReferenceNumber = "fagagatrta";
		AssertNoNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
		previousDocument.CSI_ReferenceNumber = "fagagatrta1243";
		AssertNoNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
		previousDocument.CSI_ReferenceNumber = "fagaga  trta1243";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumberInfo);
	}

	public void TestCheckCSI_ReferenceNumber2()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		previousDocument.CSI_ReferenceNumber2 = "#eoiw5u985874%";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
		previousDocument.CSI_ReferenceNumber2 = "arewat#zr64ez%4624";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
		previousDocument.CSI_ReferenceNumber2 = "1xxxx#fagagatrta%xxx11242324";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
		previousDocument.CSI_ReferenceNumber2 = "atea36735";
		AssertNoNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
		previousDocument.CSI_ReferenceNumber2 = "14265687598060";
		AssertNoNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
		previousDocument.CSI_ReferenceNumber2 = "fagaga  trta1243";
		AssertHasNotifications("Reference should be alphanumeric", previousDocument.CSI_ReferenceNumber2Info);
	}

	public void TestValidateCSI_Quantity()
	{
		var previousDocument = Factory.New<PreviousDocument>();

		CombineAssertions(() =>
		{
			previousDocument.CSI_Quantity = new ZDecimal(1234567890);
			AssertNoErrors("Within range", previousDocument.CSI_QuantityInfo);

			previousDocument.CSI_Quantity = new ZDecimal(12345678901);
			AssertHasErrors("Outside range", previousDocument.CSI_QuantityInfo);
		});
	}

	public void TestValidateCSI_UnitOfQuantity()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Belgium, "Belgium");
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Unit of Quantity");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_UnitOfQuantity = "KGM";
			AssertNoMessageError("valid code", previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);
			previousDocument.CSI_UnitOfQuantity = "GRM";
			AssertHasMessageError("invalid code", previousDocument.CSI_UnitOfQuantityInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestValidateCSI_Quantity2()
	{
		var previousDocument = Factory.New<PreviousDocument>();

		CombineAssertions(() =>
		{
			previousDocument.CSI_Quantity2 = new ZDecimal(12345678);
			AssertNoErrors("Within range", previousDocument.CSI_Quantity2Info);

			previousDocument.CSI_Quantity2 = new ZDecimal(123456789);
			AssertHasErrors("Outside range", previousDocument.CSI_Quantity2Info);
		});
	}

	public void TestValidateCSI_UnitOfQuantity2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1A");

		Factory.Save();

		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_UnitOfQuantity2 = "1A";
			AssertNoMessageError("valid code", previousDocument.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);
			previousDocument.CSI_UnitOfQuantity2 = "1B";
			AssertHasMessageError("invalid code", previousDocument.CSI_UnitOfQuantity2Info, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCheckCSI_PackType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UnitedNationsRecommendations", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UNPKG");
		helper.Create_CusCodeList_RefData(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1A");

		Factory.Save();

		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_PackType = "1A";
			AssertNoMessageError("valid code", previousDocument.CSI_PackTypeInfo, ListValidation.InvalidCodeMessageError);
			previousDocument.CSI_PackType = "1B";
			AssertHasMessageError("invalid code", previousDocument.CSI_PackTypeInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestIsSubTypeMandatoryValue()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		var previousDocumentValidationForTest = new PreviousDocumentValidationForTest(previousDocument);

		AssertEquals("IsSubTypeMandatory should be false", expected: false, previousDocumentValidationForTest.IsSubTypeMandatoryFlag);
	}
}

public class PreviousDocumentValidationForTest : PreviousDocumentValidation
{
	public PreviousDocumentValidationForTest(PreviousDocument parent) : base(parent)
	{
	}

	public bool IsSubTypeMandatoryFlag => IsSubTypeMandatory;
}
