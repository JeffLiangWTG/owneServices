using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	class DeclarationJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckE2_GovRegNum_ClearanceLocalInvolvedParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			var involvedPartyAdd = declaration.ClearanceLocalInvolvedParty;
			var info = involvedPartyAdd.E2_GovRegNumInfo;

			involvedPartyAdd.E2_AddressOverride = true;
			involvedPartyAdd.E2_GovRegNum = "03142520000127";
			AssertNoMessageError(info, "The entered CNPJ is not valid.");

			involvedPartyAdd.E2_GovRegNum = "03142520000128";
			AssertHasMessageError(info, "The entered CNPJ is not valid.");

			involvedPartyAdd.E2_GovRegNum = "03142520000127XXXXX";
			AssertHasMessageError(info, "The entered CNPJ is not valid.");

			involvedPartyAdd.E2_AddressOverride = false;
			involvedPartyAdd.Validation.ValidateE2_GovRegNum();
			AssertNoMessageError(info, "The entered CNPJ is not valid.");
		}

		public void TestCheckOrganisationPK_ClearanceLocalInvolvedParty_GeoLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;

			var clearanceLocal = declaration.ClearanceLocalInvolvedParty;
			clearanceLocal.DocAddressType = DocAddressType.ClearanceLocalInvolvedParty;
			clearanceLocal.E2_AddressOverride = ZBool.True;
			clearanceLocal.E2_Address1 = "Address1";

			clearanceLocal.E2_Longitude = 1.2m;
			clearanceLocal.E2_Latitude = 2.2m;

			clearanceLocal.Validation.ValidateAll();
			AssertNoMessageError(clearanceLocal.E2_LongitudeInfo, "You have not entered a valid Longitude.");
			AssertNoMessageError(clearanceLocal.E2_LatitudeInfo, "You have not entered a valid Latitude.");

			clearanceLocal.E2_Latitude = 0m;
			clearanceLocal.E2_Longitude = 0m;

			clearanceLocal.Validation.ValidateAll();
			AssertHasMessageError(clearanceLocal.E2_LongitudeInfo, "You have not entered a valid Longitude.");
			AssertHasMessageError(clearanceLocal.E2_LatitudeInfo, "You have not entered a valid Latitude.");

			clearanceLocal.E2_AddressOverride = ZBool.False;

			var clearanceLocalAddress = Factory.New<OrgHeader>();
			clearanceLocalAddress.OH_Code = "TEST";
			clearanceLocalAddress.OH_FullName = "TEST COMPANY";
			clearanceLocalAddress.MainAddress.OA_Address1 = "TEST ADDRESS 1";

			clearanceLocalAddress.Addresses.AddNew();
			clearanceLocalAddress.Addresses[0].OA_Longitude = 2.1;
			clearanceLocalAddress.Addresses[0].OA_Latitude = 3.1;

			clearanceLocal.OrganisationPK = clearanceLocalAddress.PK;

			AssertNoMessageError(clearanceLocal.OrganisationPKInfo, "Clearance Local Address does not have a valid GPS> Longitude.");
			AssertNoMessageError(clearanceLocal.OrganisationPKInfo, "Clearance Local Address does not have a valid GPS> Latitude.");

			clearanceLocalAddress.Addresses[0].OA_Longitude = 0;
			clearanceLocalAddress.Addresses[0].OA_Latitude = 0;
			clearanceLocal.Validation.ValidateAll();

			AssertHasMessageError(clearanceLocal.OrganisationPKInfo, "Clearance Local Address does not have a valid GPS> Longitude.");
			AssertHasMessageError(clearanceLocal.OrganisationPKInfo, "Clearance Local Address does not have a valid GPS> Latitude.");
		}

		public void TestCheckOrganisationPK_ClearanceLocalInvolvedParty_MandatoryCheck()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.ClearanceOfficeIsCustomsEnclosure = true;

			var docAddress = declaration.ClearanceLocalInvolvedParty;
			AssertNoMessageErrorContaining("No Message Error when ClearanceOfficeIsCustomsEnclosure is true", declaration.ClearanceLocalInvolvedParty.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ClearanceOfficeIsCustomsEnclosure = false;
			AssertHasMessageErrorContaining(declaration.ClearanceLocalInvolvedParty.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			docAddress.E2_AddressOverride = true;
			AssertNoMessageErrorContaining("No Message Error when E2_AddressOverride is true", declaration.ClearanceLocalInvolvedParty.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			docAddress.E2_AddressOverride = false;
			docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertNoMessageErrorContaining(declaration.ClearanceLocalInvolvedParty.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

			docAddress.E2_AddressOverride = true;

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			docAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageErrorContaining("No Message Error when MessageType is Import", docAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
