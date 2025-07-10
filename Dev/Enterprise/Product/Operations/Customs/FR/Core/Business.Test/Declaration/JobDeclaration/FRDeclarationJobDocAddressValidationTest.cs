using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.MessagesWrappers;
using Enterprise.Customs.FR.Business.Testing.ConfigurationProviders;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class FRDeclarationJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckJE_OH_SupplierMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var invoiceHeader = declaration.Invoices.AddNew();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");

			declaration.JE_OH_Supplier = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");

			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;

			var address1 = supplier.Addresses.AddNew();
			address1.OA_Address1 = "test";
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = false;

			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");

			declaration.SupplierDocumentaryAddress.E2_OA_Address = address1.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "Supplier is mandatory");
		}

		public void TestCheckOrganisationPK_CheckRuleNat_020()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			Factory.Save();

			var errorMessage = "[NAT_020] The selected organization does not have an EORI.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.DefermentPartyDocAddress.OrganisationPK = orgHeader.PK;
			declaration.DefermentPartyDocAddress.E2_AddressType = DocAddressTypes.Codes.DefermentParty;

			using (var context = new DeclarationValidationDeciderTestContext(declaration))
			{
				context.EnableRule(x => x.IsRuleNAT_020Active);
				declaration.DefermentPartyDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError(declaration.DefermentPartyDocAddress.OrganisationPKInfo, errorMessage);
				DeclarationValidationTestHelper.AssertEORI(declaration.DefermentPartyDocAddress, orgHeader);
			}
		}

		public void TestCheckJE_OH_ImporterMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			var invoiceHeader = declaration.Invoices.AddNew();

			var importer = Factory.NewWithValidTestData<OrgHeader>();

			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");

			declaration.JE_OH_Importer = importer.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");

			declaration.JE_OH_Importer = ZGuid.Empty;
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");

			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;

			var address1 = importer.Addresses.AddNew();
			address1.OA_Address1 = "test";

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = false;

			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.RunPreSaveValidation();
			AssertHasMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");

			declaration.ImporterDocumentaryAddress.E2_OA_Address = address1.PK;
			declaration.RunPreSaveValidation();
			AssertNoMessageErrorContaining(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Importer is mandatory");
		}

		public void TestCheckImporter()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "A", Core.Constants.CountryCodes.France);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;

			orgHeader.MainAddress.OA_Address1 = "3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_Address2 = "BURLEIGH HEADS, QLD";
			orgHeader.MainAddress.OA_AIREquipmentNeeded = "PSL";
			orgHeader.MainAddress.OA_City = "AUBNE";
			orgHeader.MainAddress.OA_Code = "PST: 3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_PostCode = "4220";
			orgHeader.MainAddress.OA_State = "QLD";

			var address1 = orgHeader.Addresses.AddNew();
			address1.AddAddressType(OrgAddressType.PickupAndDelivery);
			address1.OA_AccessPoint = "";
			address1.OA_Address1 = "3 RARNLY DRIVE";
			address1.OA_Address2 = "BURLEIGH";
			address1.OA_City = "HEADS";
			address1.OA_Code = "Pickup and Delivery Addre";
			address1.OA_PostCode = "";
			address1.OA_State = "QLD";

			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.ImporterIsRequired);

			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.ImporterIsRequired);

			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = address1.PK;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.ImporterIsRequired);
			AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");
			AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);

			address1.OA_PostCode = "A12345";
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");

			address1.OA_PostCode = "012345";
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");

			orgCusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
			AssertHasMessageError(declaration.ImporterDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.ImporterSrtNotConfigured);
		}

		public void TestCheckImporter_ImporterDocumentaryAddressIsOverrided()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "A", Core.Constants.CountryCodes.France);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;

			orgHeader.MainAddress.OA_Address1 = "3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_Address2 = "BURLEIGH HEADS, QLD";
			orgHeader.MainAddress.OA_AIREquipmentNeeded = "PSL";
			orgHeader.MainAddress.OA_City = "AUBNE";
			orgHeader.MainAddress.OA_Code = "PST: 3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_PostCode = "4220";
			orgHeader.MainAddress.OA_State = "QLD";
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("ImporterDocumentaryAddress is not overrided and the organisation has a post code => no error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");
			AssertNoMessageError("ImporterDocumentaryAddress is not overrided and the organisation has a TVA code => no error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);

			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			declaration.ImporterDocumentaryAddress.E2_Postcode = ZString.Empty;
			declaration.ImporterDocumentaryAddress.E2_GovRegNum = ZString.Empty;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("ImporterDocumentaryAddress is overrided and E2_Postcode is empty => error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");
			AssertHasMessageError("ImporterDocumentaryAddress is overrided and E2_GovRegNum is empty => error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);

			declaration.ImporterDocumentaryAddress.E2_Postcode = "24750";
			declaration.ImporterDocumentaryAddress.E2_GovRegNum = "1";
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("ImporterDocumentaryAddress is overrided and E2_Postcode is not empty => no error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, "Configure post code of importer main address.");
			AssertNoMessageError("ImporterDocumentaryAddress is overrided and E2_GovRegNum is not empty => no error.", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
		}

		public void TestCheckSupplier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "A", Core.Constants.CountryCodes.France);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._3Indirect;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierIsRequired);

			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierIsRequired);

			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierIsRequired);
			AssertHasMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
			AssertNoMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierSrtNotConfigured);

			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Denmark;
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("No SRT missing error as long as one SRT of EUN-countries is provided.", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierSrtNotConfigured);

			orgHeader.MainAddress.OA_PostCode = "012345";
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();

			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			orgCusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
			AssertHasMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, ErrorCollectorHelper.SupplierSrtNotConfigured);

			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Denmark;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("No TVA missing error as long as one VAT of EUN-countries is provided.", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
		}

		public void TestCheckSupplier_SupplierDocumentaryAddressIsOverrided()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var orgCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "A", Core.Constants.CountryCodes.France);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;

			orgHeader.MainAddress.OA_Address1 = "3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_Address2 = "BURLEIGH HEADS, QLD";
			orgHeader.MainAddress.OA_AIREquipmentNeeded = "PSL";
			orgHeader.MainAddress.OA_City = "AUBNE";
			orgHeader.MainAddress.OA_Code = "PST: 3 RARNLY DRIVE";
			orgHeader.MainAddress.OA_PostCode = "4220";
			orgHeader.MainAddress.OA_State = "QLD";
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;

			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("SupplierDocumentaryAddress is not overrided and the organisation has a TVA code => no error.", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_Postcode = ZString.Empty;
			declaration.SupplierDocumentaryAddress.E2_GovRegNum = ZString.Empty;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("SupplierDocumentaryAddress is overrided and E2_GovRegNum is empty => error.", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);

			declaration.SupplierDocumentaryAddress.E2_Postcode = "24750";
			declaration.SupplierDocumentaryAddress.E2_GovRegNum = "1";
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("SupplierDocumentaryAddress is overrided and E2_GovRegNum is not empty => no error.", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MessageBuilderHelper.MessageTvaCodeNotFound);
		}
	}
}
