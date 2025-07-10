using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_CodeMandatoryValidation()
		{
			additionalInfo.CSI_Description = "test description";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			additionalInfo.CSI_Code = "IMITY";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertHasMessageErrorContaining(additionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

			additionalInfo.CSI_Code = "IMITM";
			additionalInfo.Validation.ValidateCSI_Code();
			AssertNoMessageErrors(additionalInfo.CSI_CodeInfo);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();

			additionalInfo = packedItem.AdditionalInfos.AddNew();

			helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var uk = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", uk);

			var countryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService;
			var additionalInformationCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;

			helper.CreateNewOrGetExistingCusCodeType(additionalInformationCode, "AdditionalInformation");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", additionalInformationCode, countryCode);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", additionalInformationCode, countryCode);

			var addInfo = helper.CreateCusCodeList(countryCode, additionalInformationCode, "IMITM", "Import Item", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			addInfo.Attributes.AddNew("Direction", "IMPORT");
			addInfo.Attributes.AddNew("Level", "ITEM");

			Factory.Save();
		}

		AdditionalInfo additionalInfo;
		AsycudaPackedItem packedItem;
		UniversalReferenceTestDataHelper helper;
		#endregion
	}
}
