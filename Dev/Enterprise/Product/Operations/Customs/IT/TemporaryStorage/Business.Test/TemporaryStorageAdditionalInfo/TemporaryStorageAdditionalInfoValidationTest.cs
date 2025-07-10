using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageAdditionalInfoValidation))]
sealed class TemporaryStorageAdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestCSI_SubType_HasValidCode()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		ValidationTestHelper.AssertInvalidCodeMessageError(addInfo.CSI_SubTypeInfo, "XXXX", AdditionalInfoSubTypeList.Codes.AdditionalInformation);
	}

	public void TestCSI_Code_HasValidCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeType("ADDIN", "ADDIN", Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "ADDIN", "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		ValidationTestHelper.AssertInvalidCodeMessageError(addInfo.CSI_CodeInfo, "XXXX", "code1");
	}

	public void TestValidateTypeAndDescription()
	{
		const string errorMessage = "You have not entered a Type or a Description";
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		addInfo.Validation.ValidateCSI_Code();
		AssertHasMessageError("When Kind = INF and Description and Type is Empty", addInfo.CSI_CodeInfo, errorMessage);
		addInfo.Validation.ValidateCSI_Description();
		AssertHasMessageError("When Kind = INF and Description and Type is Empty", addInfo.CSI_DescriptionInfo, errorMessage);

		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		addInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError("When Kind != INF and Description and Type is Empty", addInfo.CSI_CodeInfo, errorMessage);
		addInfo.Validation.ValidateCSI_Description();
		AssertNoMessageError("When Kind != INF and Description and Type is Empty", addInfo.CSI_DescriptionInfo, errorMessage);

		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		addInfo.CSI_Description = "Test-Description";
		addInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError("When Kind = INF and Description is not Empty and Type is Empty", addInfo.CSI_CodeInfo, errorMessage);
		addInfo.Validation.ValidateCSI_Description();
		AssertNoMessageError("When Kind = INF and Description is not Empty and Type is Empty", addInfo.CSI_DescriptionInfo, errorMessage);

		addInfo.CSI_Code = "Test-Code";
		addInfo.CSI_Description = ZString.Empty;
		addInfo.Validation.ValidateCSI_Code();
		AssertNoMessageError("When Kind = INF and Description is Empty and Type is not Empty", addInfo.CSI_CodeInfo, errorMessage);
		addInfo.Validation.ValidateCSI_Description();
		AssertNoMessageError("When Kind = INF and Description is Empty and Type is not Empty", addInfo.CSI_DescriptionInfo, errorMessage);
	}
}

