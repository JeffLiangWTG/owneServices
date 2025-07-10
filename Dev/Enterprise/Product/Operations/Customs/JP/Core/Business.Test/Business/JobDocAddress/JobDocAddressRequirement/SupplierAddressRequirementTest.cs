using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(SupplierAddressRequirement))]
	sealed class SupplierAddressRequirementTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var targetInfo = supplierDocumentaryAddress.OrganisationPKInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			entryInstruction.ExportControlNumber = "123";
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "AIR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_MessageType = "IMP";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			var expectedErrorMessage = "The selected Exporter must have either an LPC Legal Person Code, a CIE Importer/Exporter Code, or a JAS JASTPRO Code.";
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_Code = "OrgTest1";
			declaration.JE_MessageType = "EXP";

			supplierDocumentaryAddress.OrganisationPK = header.PK;
			AssertNoMessageError(targetInfo, expectedErrorMessage);
			var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customsEntryInstruction.CEI_Style = "M";

			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			var customsCode = header.CustomsCodes.AddNew("LPC", "2HDN8", Core.Constants.CountryCodes.Japan);
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			customsCode.OK_CodeType = "CIE";
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			customsCode.OK_CodeType = "JAS";
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			customsCode.OK_CodeType = "CCC";
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckE2_CompanyName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;

			var targetInfo = supplierDocumentaryAddress.E2_CompanyNameInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			entryInstruction.ExportControlNumber = "123";
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_TransportMode = "AIR";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_MessageType = "IMP";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			declaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckE2_GovRegNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			var expectedErrorMessage = "The selected Exporter must have either an LPC Legal Person Code, a CIE Importer/Exporter Code, or a JAS JASTPRO Code.";
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var targetInfo = supplierDocumentaryAddress.E2_GovRegNumInfo;

			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.E2_GovRegNumType = ZString.Empty;
			supplierDocumentaryAddress.E2_GovRegNum = ZString.Empty;
			AssertNoMessageError(targetInfo, expectedErrorMessage);
			var customsEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			customsEntryInstruction.CEI_Style = "M";

			supplierDocumentaryAddress.Validation.ValidateE2_GovRegNum();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			supplierDocumentaryAddress.E2_GovRegNumType = "LPC";
			supplierDocumentaryAddress.E2_GovRegNum = "2HDN8";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			supplierDocumentaryAddress.E2_GovRegNumType = "CIE";
			supplierDocumentaryAddress.Validation.ValidateE2_GovRegNum();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			supplierDocumentaryAddress.E2_GovRegNumType = "JAS";
			supplierDocumentaryAddress.Validation.ValidateE2_GovRegNum();
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			supplierDocumentaryAddress.E2_GovRegNumType = "CCC";
			supplierDocumentaryAddress.Validation.ValidateE2_GovRegNum();
			AssertHasMessageError(targetInfo, expectedErrorMessage);
		}
	}
}
