using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.MessagingRules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	public class EuJobDeclarationTraderIdMessageErrorsTest : TestCaseWithFactory
	{
		public void TestPostCodeMandatoryForDifferentScenarios()
		{
			var dec = Factory.New<JobDeclaration>();
			var consignee = Factory.New<OrgHeader>();
			consignee.Addresses.AddNewMainAddress();
			consignee.MainAddress.OA_PostCode = "";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			dec.JE_MessageType = "IMP";
			dec.JE_OH_Importer = consignee.PK;
			dec.ImporterDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			var germany = consignee.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Enterprise.Core.Constants.CountryCodes.Germany);
			var blighty = consignee.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Enterprise.Core.Constants.CountryCodes.UnitedKingdom);

			AssertHasMessageError("Imports - need a postcode when no British turn is present. Error expected because postcode is empty", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);

			consignee.MainAddress.OA_PostCode = "LU6";
			TriggerValidation(dec, consignee);
			AssertNoMessageError("Imports - need a postcode when no British turn is present. Error NOT expected because postcode is NOT empty.", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);

			consignee.MainAddress.OA_PostCode = "";
			TriggerValidation(dec, consignee);
			AssertHasMessageError("Imports - need a postcode when no British turn is present. Error expected because postcode is empty", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);

			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000", germany); // a German TURN is irrelevant
			TriggerValidation(dec, consignee);
			AssertHasMessageError("Imports - need a postcode when no British turn is present - DE turns are irrelevant", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);

			consignee.CustomsCodes.RemoveAll();
			consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000", blighty); // a British TURN 
			TriggerValidation(dec, consignee);
			AssertNoMessageError("Imports - a British turn means no postcode is needed", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);

			dec.JE_MessageType = "EXP";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USATL";
			consignee.MainAddress.OA_PostCode = "";
			TriggerValidation(dec, consignee);
			AssertHasMessageError("Exports - postcode is always needed even when we have a turn", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);
			consignee.MainAddress.OA_PostCode = "30342";  // Atlanta is abroad
			TriggerValidation(dec, consignee);
			AssertNoMessageError("Exports - with postcode, should be no error", dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, AddressValidationHelper.ErrorMessageConsigneePostcodeMandatory);
		}

		static void TriggerValidation(JobDeclaration dec, OrgHeader consignee)
		{
			dec.JE_OH_Importer = ZGuid.Empty;
			dec.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			dec.JE_OH_Importer = consignee.PK;
			dec.ImporterDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		readonly string tidErrorMessage = EoriCodeValidationHelper.ErrorMessageNoEoriCode;

		public void TestImporterTidMessageErrorPresentOnImport()
		{
			var dec = Factory.New<JobDeclaration>();
			var organisation = Factory.New<OrgHeader>();
			organisation.Addresses.AddNewMainAddress();
			dec.JE_MessageType = "IMP";
			dec.ImporterDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertHasMessageError(dec.ImporterDocumentaryAddress.E2_OA_AddressInfo, tidErrorMessage);
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000");
			dec.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			dec.ImporterDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertNoMessageError(dec.JE_OH_ImporterInfo, tidErrorMessage);
		}

		public void TestSupplierTidMessageErrorPresentOnExport()
		{
			var dec = Factory.New<JobDeclaration>();
			var organisation = Factory.New<OrgHeader>();
			organisation.Addresses.AddNewMainAddress();
			dec.JE_MessageType = "EXP";
			dec.SupplierDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertHasMessageError(dec.SupplierDocumentaryAddress.E2_OA_AddressInfo, tidErrorMessage);
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000");
			dec.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			dec.SupplierDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertNoMessageError(dec.JE_OH_SupplierInfo, tidErrorMessage);
		}

		public void TestDeclarantTidMessageErrorPresentImport()
		{
			var dec = Factory.New<JobDeclaration>();
			var organisation = Factory.New<OrgHeader>();

			dec.JE_MessageType = "IMP";
			organisation.OH_Code = "CDE";

			dec.JE_OA_DeclarantAddress = organisation.MainAddress.PK;  //NB!

			Factory.Save(); // needed because during validation the call to {Declaration}.DeclarantTurnCode reads from the DB, not from memory.

			AssertHasMessageError(dec.JE_OA_DeclarantAddressInfo, tidErrorMessage);

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000");
			Factory.Save(); // needed because during validation the call to {Declaration}.DeclarantTurnCode reads from the DB, not from memory.
			dec.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(dec.JE_OA_DeclarantAddressInfo, tidErrorMessage);
		}

		public void TestDeclarantTidMessageErrorPresentExport()
		{
			var dec = Factory.New<JobDeclaration>();
			var organisation = Factory.New<OrgHeader>();

			dec.JE_MessageType = "EXP";
			organisation.OH_Code = "FGH";

			dec.JE_OA_DeclarantAddress = organisation.MainAddress.PK;  //NB!

			Factory.Save(); // needed because during validation the call to {Declaration}.DeclarantTurnCode reads from the DB, not from memory.

			AssertHasMessageError(dec.JE_OA_DeclarantAddressInfo, tidErrorMessage);

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000");
			Factory.Save(); // needed because during validation the call to {Declaration}.DeclarantTurnCode reads from the DB, not from memory.
			dec.Validation.ValidateJE_OA_DeclarantAddress();
			AssertNoMessageError(dec.JE_OA_DeclarantAddressInfo, tidErrorMessage);
		}

		public void TestSupervisingOfficeTidMessageErrorPresentExport() //SPOFF
		{
			var declaration = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "USATL";

			var cusOrg = org.CustomsCodes.AddNew();
			cusOrg.OK_CustomsRegNo = "12345678";
			cusOrg.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			declaration.SupervisingOfficeDocAddress.OrganisationPK = org.PK;
			declaration.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(false, declaration.SupervisingOfficeDocAddress.E2_OA_AddressInfo.HasMessageError(tidErrorMessage));

			cusOrg.OK_CustomsRegNo = "";

			declaration.SupervisingOfficeDocAddress.E2_OA_Address = ZGuid.Empty;
			declaration.SupervisingOfficeDocAddress.E2_OA_Address = org.MainAddress.PK;
			AssertEquals(false, declaration.SupervisingOfficeDocAddress.E2_OA_AddressInfo.HasMessageError(tidErrorMessage));
		}
	}
}
