using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class DeclarationJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckImporterDocumentaryAddress_Export_VAT()
		{
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrors(declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
		}

		public void TestCheckImporterDocumentaryAddress_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors("No Importer Documentary Address", declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			AssertNoMessageErrors("Importer Documentary Address", declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
		}

		public void TestCheckImporterDocumentaryAddressMandatory_Import()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("No entry instructions", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				var entry = declaration.CustomsEntryInstructions.AddNew();
				entry.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Entry Instruction not EZA", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("Entry Instruction EZA", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
				AssertNoMessageErrorContaining("Has Importer Documentary Address", declaration.ImporterDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckImporterDocumentaryAddressMandatory_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrors("No Importer Documentary Address", declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
		}

		public void TestCheckSupplierDocumentaryAddressMandatory_Import()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("No entry instructions", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				var entry = declaration.CustomsEntryInstructions.AddNew();
				entry.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Entry Instruction not EZA", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("Entry Instruction EZA", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
				AssertNoMessageErrorContaining("Has Supplier Documentary Address", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
				declaration.SupplierDocumentaryAddress.OrganisationPK = Guid.Empty;
				declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("No Supplier Documentary Address", declaration.SupplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckSupplierDocumentaryAddressMandatory_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			CombineAssertions(() =>
			{
				AssertNoMessageErrors("No Supplier Documentary Address", declaration.SupplierDocumentaryAddress.OrganisationPKInfo);

				ValidationTestHelper.AssertWarningIfNotEntered(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "You have not entered a [2] Supplier.", "Warning if empty supplier on export");
			});
		}

		public void TestCheckE2_OA_Address()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Org";
			var orgAddress = orgHeader.Addresses.AddNew();
			declaration.SupplierPickupAddress.E2_AddressOverride = false;

			var info = declaration.SupplierPickupAddress.E2_OA_AddressInfo;
			declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();

			var entry1 = declaration.CustomsEntryInstructions.AddNew();
			entry1.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			entry1.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._00;
			entry1.GoodsLocation.LoadingPlace = "1";

			var im35Chars = "12345678901234567890123456789012345";
			var im36Chars = "123456789012345678901234567890123456";
			orgAddress.OA_Address1 = im35Chars;
			orgAddress.OA_Address2 = im35Chars;
			orgAddress.OA_PostCode = "12345";

			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();

			AssertNoMessageError(info, "Address 1 should have a maximum length of 35 characters.");
			AssertNoMessageError(info, "Address 2 should have a maximum length of 35 characters.");
			AssertNoMessageError(info, "Postcode should be exactly 5 characters.");

			orgAddress.OA_Address1 = im36Chars;
			orgAddress.OA_Address2 = im36Chars;
			orgAddress.OA_PostCode = "123456";
			declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();

			AssertHasMessageError(info, "Address 1 should have a maximum length of 35 characters.");
			AssertHasMessageError(info, "Address 2 should have a maximum length of 35 characters.");
			AssertHasMessageError(info, "Postcode should be exactly 5 characters.");
		}

		public void TestCheckE2_PostCode()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supAdd = declaration.SupplierPickupAddress;
			var info = supAdd.E2_PostcodeInfo;

			supAdd.E2_AddressOverride = true;
			supAdd.E2_Postcode = "1234";
			AssertHasMessageError(info, "Postcode should be exactly 5 characters.");

			supAdd.E2_Postcode = "12345";
			AssertNoMessageError(info, "Postcode should be exactly 5 characters.");

			supAdd.E2_Postcode = "123456";
			AssertHasMessageError(info, "Postcode should be exactly 5 characters.");

			supAdd.E2_AddressOverride = false;
			supAdd.Validation.ValidateE2_Postcode();
			AssertNoMessageError(info, "Postcode should be exactly 5 characters.");

			supAdd.E2_AddressOverride = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			supAdd.Validation.ValidateE2_Postcode();
			AssertNoMessageError(info, "Postcode should be exactly 5 characters.");
		}

		public void TestCheckE2_Address1()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supAdd = declaration.SupplierPickupAddress;
			var info = supAdd.E2_Address1Info;

			supAdd.E2_AddressOverride = true;
			supAdd.E2_Address1 = "Address thats exactly 34 char long";
			AssertNoMessageError(info, "Address 1 should have a maximum length of 35 characters.");

			supAdd.E2_Address1 = "Address thats exactly 35 chars long";
			AssertNoMessageError(info, "Address 1 should have a maximum length of 35 characters.");

			supAdd.E2_Address1 = "Address thats exactly 36 chars long!";
			AssertHasMessageError(info, "Address 1 should have a maximum length of 35 characters.");

			supAdd.E2_AddressOverride = false;
			supAdd.Validation.ValidateE2_Address1();
			AssertNoMessageError(info, "Address 1 should have a maximum length of 35 characters.");

			supAdd.E2_AddressOverride = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			supAdd.Validation.ValidateE2_Address1();
			AssertNoMessageError(info, "Address 1 should have a maximum length of 35 characters.");
		}

		public void TestCheckE2_Address2()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var supAdd = declaration.SupplierPickupAddress;
			var info = supAdd.E2_Address2Info;

			supAdd.E2_AddressOverride = true;
			supAdd.E2_Address2 = "Address thats exactly 34 char long";
			AssertNoMessageError(info, "Address 2 should have a maximum length of 35 characters.");

			supAdd.E2_Address2 = "Address thats exactly 35 chars long";
			AssertNoMessageError(info, "Address 2 should have a maximum length of 35 characters.");

			supAdd.E2_Address2 = "Address thats exactly 36 chars long!";
			AssertHasMessageError(info, "Address 2 should have a maximum length of 35 characters.");

			supAdd.E2_AddressOverride = false;
			supAdd.Validation.ValidateE2_Address2();
			AssertNoMessageError(info, "Address 2 should have a maximum length of 35 characters.");

			supAdd.E2_AddressOverride = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			supAdd.Validation.ValidateE2_Address2();
			AssertNoMessageError(info, "Address 2 should have a maximum length of 35 characters.");
		}

		public void TestCheckOrganisationPK_ImporterMustBeResidentInDE_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var info = declaration.ImporterDocumentaryAddress.OrganisationPKInfo;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "0000F49";

			CombineAssertions(() =>
			{
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("No Importer, Concession is 'F49'", info, EuCodeF49MessageError);

				var mainAddress = orgHeader.MainAddress;
				mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Importer is not German Resident, Concession is 'F49'", info, EuCodeF49MessageError);

				mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Importer is German Resident, Concession is 'F49'", info, EuCodeF49MessageError);
			});
		}

		public void TestCheckOrganisationPK_ImporterMustBeResidentInDE_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var info = declaration.ImporterDocumentaryAddress.OrganisationPKInfo;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "0000F49";
			var mainAddress = orgHeader.MainAddress;
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = mainAddress.PK;

			CombineAssertions(() =>
			{
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("EXPORT - Importer is not German Resident, Concession is 'F49'", info, EuCodeF49MessageError);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("IMPORT - Importer is not German Resident, Concession is 'F49'", info, EuCodeF49MessageError);
			});
		}

		public void TestCheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber_EoriBranch_Export()
		{
			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var carrierAddress = PrepareCarrierEUBorder(JobMessageTypeList.Codes.Export);
			var carrier = carrierAddress.Header;
			var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No EORINumber; no EORIBranch", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Greece);
				var eori2 = carrier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Greece);
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Has multiple EORINumbers; no EORIBranch", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				eori2.Delete();
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Has EORINumber; no EORIBranch", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				var eoriBranch = carrier.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0000", Core.Constants.CountryCodes.Germany);
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Has EORINumber; has EORIBranch but wrong PremissesAddress", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				eoriBranch.OK_OA_PremisesAddress = carrierAddress.PK;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Has EORINumber; has EORIBranch with correct PremissesAddress", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				declaration.CarrierEUBorderDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("CarrierEUBorderDocAddress empty, shouldn't throw exception", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);
			});
		}

		public void TestCheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber_EoriBranch_Import()
		{
			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var carrierAddress = PrepareCarrierEUBorder(JobMessageTypeList.Codes.Import);
			var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				carrierAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Greece);
				declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
				AssertNoMessageError("No OrgCusCodes, Import", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("No OrgCusCodes, Export", propertyInfo, CarrierEUBorderEORIIdentificationMandatoryMessageError);
			});
		}

		public void TestCheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber_EoriAndTcu_Export()
		{
			Enterprise.Customs.DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var carrierAddress = PrepareCarrierEUBorder(JobMessageTypeList.Codes.Export);
			var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("No EORI, no TCU", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);

				var eoriNumber = carrierAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Greece);
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Has EORI; no TCU", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);

				eoriNumber.Delete();
				carrierAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Greece);
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No EORI; Has TCU", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);

				carrierAddress.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Greece);
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("No EORI; Has multiple TCUs", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);
			});
		}

		public void TestCheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber_Import()
		{
			var carrierAddress = PrepareCarrierEUBorder(JobMessageTypeList.Codes.Import);
			var propertyInfo = declaration.CarrierEUBorderDocAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				declaration.CarrierEUBorderDocAddress.E2_OA_Address = carrierAddress.PK;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No EORI, no TCU: Import", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.CarrierEUBorderDocAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("No EORI, no TCU: Export", propertyInfo, CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError);
			});
		}

		public void TestCheckImporterDocumentaryAddress_StockMovement()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			declaration.ImporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoNotifications(declaration.ImporterDocumentaryAddress.OrganisationPKInfo);
		}

		public void TestCheckSupplierDocumentaryAddress_StockMovement()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoNotifications(declaration.SupplierDocumentaryAddress.OrganisationPKInfo);
		}

		public void TestSupplierPickupAddress_Mandatory()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("No entry instructions", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				var entry = declaration.CustomsEntryInstructions.AddNew();
				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("CGL_Qualifier isn't in 'U, W, Z'", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("CGL_Qualifier is 'U'", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("CGL_Qualifier is 'W'", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("CGL_Qualifier is 'Z'", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				var entry2 = declaration.CustomsEntryInstructions.AddNew();
				entry2.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageErrorContaining("Multiple entry instructions", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.SupplierPickupAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageErrorContaining("Import", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.OrganisationPK = orgHeader.PK;
				AssertNoMessageErrorContaining("Has Pickup Address", declaration.SupplierPickupAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckE2_OA_Address_SupplierPickupAddress_GeoLocation()
		{
			const string message = "Organization of Pickup Address has no GPS Coordinates.";
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Longitude = ZDecimal.Zero;
			orgAddress.OA_Latitude = ZDecimal.Zero;
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
				AssertNoMessageError("No entry instructions", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				var entry = declaration.CustomsEntryInstructions.AddNew();
				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("CGL_Qualifier isn't 'W'", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertHasMessageError("CGL_Qualifier is 'W'", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				var entry2 = declaration.CustomsEntryInstructions.AddNew();
				entry2.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertHasMessageError("Multiple entry instructions", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("Import", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.E2_OA_Address = ZGuid.Empty;
				AssertNoMessageError("E2_OA_Address is empty", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				declaration.SupplierPickupAddress.E2_AddressOverride = ZBool.True;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("E2_AddressOverride is true", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				declaration.SupplierPickupAddress.E2_AddressOverride = ZBool.False;
				declaration.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
				orgAddress.OA_Longitude = ZDecimal.Zero;
				orgAddress.OA_Latitude = 53.55109;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("OA_Latitude isn't empty", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);

				orgAddress.OA_Longitude = 9.99368;
				orgAddress.OA_Latitude = ZDecimal.Zero;
				declaration.SupplierPickupAddress.Validation.ValidateE2_OA_Address();
				AssertNoMessageError("OA_Longitude isn't empty", declaration.SupplierPickupAddress.E2_OA_AddressInfo, message);
			});
		}

		public void TestCheckE2_AddressOverride()
		{
			const string message = "You cannot use Address Override if Qualifier of Identification contains 'U' or 'W'.";
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.E2_AddressOverride = ZBool.True;
				AssertNoMessageError("No entry instructions", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				var entry = declaration.CustomsEntryInstructions.AddNew();
				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				declaration.SupplierPickupAddress.Validation.ValidateE2_AddressOverride();
				AssertNoMessageError("CGL_Qualifier isn't 'U' or 'W'", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				declaration.SupplierPickupAddress.Validation.ValidateE2_AddressOverride();
				AssertHasMessageError("CGL_Qualifier is 'U'", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				entry.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.GnssCoordinates;
				declaration.SupplierPickupAddress.Validation.ValidateE2_AddressOverride();
				AssertHasMessageError("CGL_Qualifier is 'W'", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				var entry2 = declaration.CustomsEntryInstructions.AddNew();
				entry2.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
				declaration.SupplierPickupAddress.Validation.ValidateE2_AddressOverride();
				AssertHasMessageError("Multiple entry instructions", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.SupplierPickupAddress.Validation.ValidateE2_AddressOverride();
				AssertNoMessageError("Import", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				declaration.SupplierPickupAddress.E2_AddressOverride = ZBool.False;
				AssertNoMessageError("E2_AddressOverride is false", declaration.SupplierPickupAddress.E2_AddressOverrideInfo, message);
			});
		}

		public void TestOrganizationContractualPartnerAndExporter_MustNotBeEqual_SameOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertOrganizationContractualPartnerAndExporterMustNotBeEqual(org, org);
		}

		public void TestOrganizationContractualPartnerAndExporter_MustNotBeEqual_SameEori()
		{
			var (org1, org2) = GetOrganisationsWithSameEori();
			AssertOrganizationContractualPartnerAndExporterMustNotBeEqual(org1, org2);
		}

		public void TestOrganizationContractualPartnerAndExporter_MustNotBeEqual_SameTcu()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "1234567", Core.Constants.CountryCodes.UnitedStates);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "1234567", Core.Constants.CountryCodes.UnitedStates);

			AssertOrganizationContractualPartnerAndExporterMustNotBeEqual(org1, org2);
		}

		public void TestOrganizationContractualPartnerAndExporter_MustNotBeEqual_DifferentEoriSameTcu()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321");
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "1234567", Core.Constants.CountryCodes.UnitedStates);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "5555555555");
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "1234567", Core.Constants.CountryCodes.UnitedStates);

			AssertOrganizationContractualPartnerAndExporterMustNotBeEqual(org1, org2);
		}

		public void Test_OrgainzationExporterAndDeclarant_MustNotBeEqual_SameOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertOrganizationExporterAndDeclarantMustNotBeEqual(org, org);
		}

		public void Test_OrgainzationExporterAndDeclarant_MustNotBeEqual_SameEori()
		{
			var (org1, org2) = GetOrganisationsWithSameEori();
			AssertOrganizationExporterAndDeclarantMustNotBeEqual(org1, org2);
		}

		public void Test_OrgainzationExporterAndSubcontractor_MustNotBeEqual_SameOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertOrganizationExporterAndSubcontractorMustNotBeEqualWhenConstellationIsSet(org, org);
		}

		public void Test_OrgainzationExporterAndSubcontractor_MustNotBeEqual_SameEori()
		{
			var (org1, org2) = GetOrganisationsWithSameEori();
			AssertOrganizationExporterAndSubcontractorMustNotBeEqualWhenConstellationIsSet(org1, org2);
		}

		void AssertOrganizationContractualPartnerAndExporterMustNotBeEqual(OrgHeader contractualPartner, OrgHeader exporter)
		{
			const string errorMsg = "Contractual Partner and Exporter must not be equal.";

			declaration.DocAddresses.AddNew(exporter.MainAddress, DocAddressType.Exporter);
			var docAddressToRemove = declaration.DocAddresses.AddNew(contractualPartner.MainAddress, DocAddressType.ContractualPartner);

			declaration.ContractualPartnerDocAddress.Validation.ValidateOrganisationPK();

			CombineAssertions(() =>
			{
				AssertHasMessageError("Org with Same PK/EORI/TCU set", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, errorMsg);
				docAddressToRemove.Delete();

				var newOrg = GetNewOrgWithCodesSet(true);
				declaration.DocAddresses.AddNew(newOrg.MainAddress, DocAddressType.ContractualPartner);

				declaration.ContractualPartnerDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Different organization PK / EORI / TCU", declaration.ContractualPartnerDocAddress.OrganisationPKInfo, errorMsg);
			});
		}

		void AssertOrganizationExporterAndDeclarantMustNotBeEqual(OrgHeader exporter, OrgHeader declarant)
		{
			var errorMsg = "Exporter and [14] Declarant must not be equal.";

			declaration.DocAddresses.AddNew(exporter.MainAddress, DocAddressType.Exporter);

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();

			CombineAssertions(() =>
			{
				AssertHasMessageError("Orgs with Same PK/EORI set", declaration.ExporterDocAddress.OrganisationPKInfo, errorMsg);

				var newOrg = GetNewOrgWithCodesSet();
				declaration.JE_OA_DeclarantAddress = newOrg.MainAddress.PK;

				declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();

				AssertNoMessageError("Different organization PK / EORI ", declaration.ExporterDocAddress.OrganisationPKInfo, errorMsg);
			});
		}

		void AssertOrganizationExporterAndSubcontractorMustNotBeEqualWhenConstellationIsSet(OrgHeader exporter, OrgHeader subContractor)
		{
			var errorMsg = "Exporter and [2] Subcontractor must not be equal.";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0111;

			Assert(entryInstruction.Constellation2ndDigitIs1And4thDigitIs1());

			declaration.ExporterDocAddress.E2_OA_Address = exporter.MainAddress.PK;
			declaration.JE_OA_SellerAddress = subContractor.MainAddress.PK;

			declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();

			CombineAssertions(() =>
			{
				AssertHasMessageError("Constellation set, Orgs with Same PK/EORI set", declaration.ExporterDocAddress.OrganisationPKInfo, errorMsg);

				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.ZG_PartyConstellation = PartyConstellationCodeList.Codes._1110;

				declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();

				AssertNoMessageError("Constellation not set correctly", declaration.ExporterDocAddress.OrganisationPKInfo, errorMsg);

				var newOrg = GetNewOrgWithCodesSet();
				declaration.JE_OA_SellerAddress = newOrg.MainAddress.PK;
				entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0111;

				declaration.ExporterDocAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Different organization PK / EORI ", declaration.ExporterDocAddress.OrganisationPKInfo, errorMsg);
			});
		}

		OrgHeader GetNewOrgWithCodesSet(bool setTcu = false)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999999999999");
			if (setTcu)
			{
				org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "222222222222", Core.Constants.CountryCodes.Japan);
			}

			return org;
		}

		(OrgHeader, OrgHeader) GetOrganisationsWithSameEori()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789");

			return (org1, org2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		}
		JobDeclaration declaration;
		OrgHeader orgHeader;

		OrgAddress PrepareCarrierEUBorder(string messageType)
		{
			declaration.JE_MessageType = messageType;

			var carrier = Factory.New<OrgHeader>();
			var carrierAddress = carrier.MainAddress;
			carrierAddress.Address1 = "Address1";
			carrierAddress.City = "City";
			carrierAddress.Postcode = "2730018";
			carrierAddress.OA_RN_NKCountryCode = "DE";
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = carrierAddress.PK;
			return carrierAddress;
		}

		const string EuCodeF49MessageError = "EU-Code 'F49' detected - [8] Importer must have an Organization Address in DE – Germany.";

		const string CarrierEUBorderEORINumberOrTCUNumberMandatoryMessageError = "Carrier EU Border is missing EORI or TCUI number.";

		const string CarrierEUBorderEORIIdentificationMandatoryMessageError = "Carrier EU Border is missing EORI Branch.";
	}
}
