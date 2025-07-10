using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusCodeType(code: "ADDIN", desc: "ADDIN", dataGrouping: Core.Constants.CountryCodes.Israel);
			var code1 = helper.CreateCusCodeList(dataGroupingCode: Core.Constants.CountryCodes.Israel, codeType: "ADDIN", code: "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, "Level", "Bill");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining("when CSI_Code is empty", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInfo.CSI_Code = "2";
			AssertNoMessageErrorContaining("when CSI_Code is not empty", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("when CSI_Code is not in the list", additionalInfo.CSI_CodeInfo, "not in the list");

			additionalInfo.CSI_Code = "code1";
			AssertNoMessageErrorContaining("when CSI_Code is in the list", additionalInfo.CSI_CodeInfo, "not in the list");
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			var addInfo = Factory.New<AsycudaAdditionalInfo>();
			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.UNLOCO;
			addInfo.CSI_ReferenceNumber = "12345";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "BSASD";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ExporterTypeID;
			addInfo.CSI_ReferenceNumber = "4";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.NightStop;
			addInfo.CSI_ReferenceNumber = "2";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsCooling;
			addInfo.CSI_ReferenceNumber = "2";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.MultipleDeals;
			addInfo.CSI_ReferenceNumber = "2";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.CargoType;
			addInfo.CSI_ReferenceNumber = "6";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ActionCode;
			addInfo.CSI_ReferenceNumber = "6";
			AssertHasMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
			addInfo.CSI_ReferenceNumber = "1";
			AssertNoMessageErrorContaining(addInfo.CSI_ReferenceNumberInfo, "not in the list");
		}

		public void TestCheckCSI_Description()
		{
			var addInfo = Factory.New<AsycudaAdditionalInfo>();
			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery;
			addInfo.CSI_Description = "a";
			AssertHasMessageErrorContaining(addInfo.CSI_DescriptionInfo, "not in the list");
			addInfo.CSI_Description = "D";
			AssertNoMessageErrorContaining(addInfo.CSI_DescriptionInfo, "not in the list");
		}

		public void TestCSI_ReferenceNumberAndCSI_DescriptionCannotBeEmpty()
		{
			var expectedMessage = "Statement Code or Content must be provided.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			additionalInfo.Validation.ValidateCSI_Description();

			AssertNoMessageErrorContaining(additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
			AssertNoMessageErrorContaining(additionalInfo.CSI_DescriptionInfo, expectedMessage);

			additionalInfo.CSI_Code = "1";
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			additionalInfo.Validation.ValidateCSI_Description();

			AssertHasMessageErrorContaining(additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
			AssertHasMessageErrorContaining(additionalInfo.CSI_DescriptionInfo, expectedMessage);

			additionalInfo.CSI_ReferenceNumber = "12345";
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			additionalInfo.Validation.ValidateCSI_Description();

			AssertNoMessageErrorContaining(additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
			AssertNoMessageErrorContaining(additionalInfo.CSI_DescriptionInfo, expectedMessage);

			additionalInfo.CSI_ReferenceNumber = ZString.Empty;
			additionalInfo.CSI_Description = "2";
			additionalInfo.Validation.ValidateCSI_ReferenceNumber();
			additionalInfo.Validation.ValidateCSI_Description();

			AssertNoMessageErrorContaining(additionalInfo.CSI_ReferenceNumberInfo, expectedMessage);
			AssertNoMessageErrorContaining(additionalInfo.CSI_DescriptionInfo, expectedMessage);
		}
	}
}
