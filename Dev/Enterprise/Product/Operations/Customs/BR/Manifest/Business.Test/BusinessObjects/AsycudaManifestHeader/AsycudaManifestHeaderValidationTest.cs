using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMasterUCR()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;
			var targetInfo = header.CustomsOwnNumberInfo;
			header.CustomsOwnNumber = "11111111111255555555555555555554444";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, "The entered Master UCR does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");

			header.CustomsOwnNumber = "1BB11111111255555555555555555554444";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, "The entered Master UCR does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");

			header.CustomsOwnNumber = "1BR11111111255555555555555555554444";
			header.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, "The entered Master UCR does not match either the CNPJ format: <year, 1>BR<CNPJ, 8><decade, 1><reference, 23> or the CPF format: <year, 1>BR<CPF, 11><decade, 1><reference, 20>.");
		}

		public void TestCheckCEMercante()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "MER";
			var targetInfo = header.CustomsOwnNumberInfo;
			header.CustomsOwnNumber = "";
			header.RunPreSaveValidation();
			AssertHasMessageErrorContaining(targetInfo, "You have not entered a CE Merchant.");

			header.CustomsOwnNumber = "1BR11111111255555555555555555554444";
			header.RunPreSaveValidation();
			AssertNoMessageError(targetInfo, "You have not entered a CE Merchant.");
		}

		public void TestCheckAMA_Nature()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ZString.Empty;
			header.AMA_ManifestType = "MER";

			header.Validation.ValidateAMA_Nature();

			AssertHasMessageErrorContaining(header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_Nature = "XXX";
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_Nature = "IMP";
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ZString.Empty;
			header.AMA_ManifestType = "MUCR";
			AssertNoNotifications(header.AMA_NatureInfo);
		}

		public void TestCheckAMA_OA_DeconsolidateAddress()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "Z1";
			var orgAddress1 = testOrg1.Addresses.AddNew();
			orgAddress1.OA_Address1 = "line 1";
			orgAddress1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "96944490", Core.Constants.CountryCodes.Brazil);

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = "Z2";
			var orgAddress2 = testOrg2.Addresses.AddNew();
			orgAddress2.OA_Address1 = "line 2";

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_Code = "Z3";
			var orgAddress3 = testOrg3.Addresses.AddNew();
			orgAddress3.OA_Address1 = "line 3";
			testOrg3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "85291444", Core.Constants.CountryCodes.Brazil);

			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.OH_Code = "Z4";
			var orgAddress4 = testOrg4.Addresses.AddNew();
			orgAddress4.OA_Address1 = "line 4";
			testOrg4.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "85291444", Core.Constants.CountryCodes.Brazil);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;

			header.AMA_OA_DeconsolidateAddress = orgAddress1.PK;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "The selected Deconsolidator does not have a CJN or CPF number");

			header.AMA_OA_DeconsolidateAddress = orgAddress2.PK;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertHasMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "The selected Deconsolidator does not have a CJN or CPF number");

			header.AMA_OA_DeconsolidateAddress = orgAddress3.PK;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "The selected Deconsolidator does not have a CJN or CPF number");

			header.AMA_OA_DeconsolidateAddress = orgAddress4.PK;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertHasMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "The selected Deconsolidator does not have a CJN or CPF number");

			header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertHasMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			header.AMA_OA_DeconsolidateAddress = orgAddress2.PK;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "The selected Deconsolidator does not have a CJN or CPF number");

			header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_DeconsolidateAddress();
			AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
