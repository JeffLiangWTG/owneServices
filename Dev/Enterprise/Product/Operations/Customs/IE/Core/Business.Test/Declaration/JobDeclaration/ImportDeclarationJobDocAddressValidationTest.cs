using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportDeclarationJobDocAddressValidationTest : DeclarationJobDocAddressValidationAbstractTest<ImportDeclarationJobDocAddressValidation>
	{
		public void TestCheckOrganisationPK_Supplier_BR3001()
		{
			var message = "[BR3001] An Additional Information (Full Type: '00400') must be entered under Entry Instruction > Additional Documents when Exporter is the same as Declarant.";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var declarantDocumentaryAddress = declaration.Declarant;
			var supplierAddressInfo = supplierDocumentaryAddress.OrganisationPKInfo;
			supplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			CombineAssertions(() =>
			{
				supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Exporter different from Declarant", supplierAddressInfo, message);
				declarantDocumentaryAddress.OA_OH = orgHeader.PK;
				supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Exporter same as Declarant and no Additional Doc", supplierAddressInfo, message);
				var additionalDoc = declaration.CustomsEntryInstructions.AddNew().AdditionalInfos.AddNew();
				additionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDoc.CSI_Code = "00700";
				supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Exporter same as Declarant and Additional Doc not equals 00400", supplierAddressInfo, message);
				additionalDoc.CSI_Code = Constants.AdditionalInformationCodes._00400;
				supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Exporter same as Declarant and Additional Doc equals 00400", supplierAddressInfo, message);
			});
		}

		public void TestCheckOrganisationPK_Importer_BR3162()
		{
			const string errorMessage = "[BR3162] Please enter an Additional Document where Kind is 'INF' and Full Type is '00500' under the Entry Instructions / Invoice Header > Additional Documents tab.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();

			var declarant = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionAdditionalDoc = instruction.AdditionalInfos.AddNew();
			instructionAdditionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			instructionAdditionalDoc.CSI_Code = "00700";
			instructionAdditionalDoc.CSI_Description = "00700 desc";

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var invoiceAdditionalDoc = invoiceHeader.AdditionalInfos.AddNew();
			invoiceAdditionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			invoiceAdditionalDoc.CSI_Code = "00600";
			invoiceAdditionalDoc.CSI_Description = "00600 desc";

			var importerInfo = importerDocumentaryAddress.OrganisationPKInfo;

			CombineAssertions(() =>
			{
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No error: importerDocumentaryAddress.OrganisationPK != Declarant.Header.PK, No INF - 00500 docs", importerInfo, errorMessage);

				importerDocumentaryAddress.OrganisationPK = declaration.Declarant.Header.PK;
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Has error: importerDocumentaryAddress.OrganisationPK == Declarant.Header.PK, No INF - 00500 docs", importerInfo, errorMessage);

				instructionAdditionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				instructionAdditionalDoc.CSI_Code = Constants.AdditionalInformationCodes._00500;
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No error: importerDocumentaryAddress.OrganisationPK == Declarant.Header.PK, INF - 00500 doc under instruction", importerInfo, errorMessage);

				instructionAdditionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				invoiceAdditionalDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				invoiceAdditionalDoc.CSI_Code = Constants.AdditionalInformationCodes._00500;
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No error: importerDocumentaryAddress.OrganisationPK == Declarant.Header.PK, INF - 00500 doc under invoice header", importerInfo, errorMessage);
			});
		}

		public void TestCheckOrganisationPK_Importer_IsNotEmpty()
		{
			const string errorMessage_noImporter = "You have not entered an Importer.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var orgGUID = ZGuid.NewZGuid();

			var declarant = Factory.New<OrgHeader>();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var instruction = declaration.CustomsEntryInstructions.FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			var instructionAdditionalDoc = instruction.AdditionalInfos.AddNew();

			var importerInfo = importerDocumentaryAddress.OrganisationPKInfo;
			CombineAssertions(() =>
			{
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H1);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H2);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H3);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H4);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H5);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.H6);
				AssertImporterIsRequired(ImportDeclarationTypeList.Codes.I1);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H7;
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				AssertNoMessageError("No error: H7", importerInfo, errorMessage_noImporter);
			});

			void AssertImporterIsRequired(ZString cei_Style)
			{
				instruction.CEI_Style = cei_Style;
				importerDocumentaryAddress.Validation.ValidateOrganisationPK();
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(importerInfo, errorMessage_noImporter);
			}
		}

		public void TestCheckOrganisationHasEORIValue()
		{
			const string errorMessage = "Importer's EORI is missing";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importerDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			var targetInfo = importerDocumentaryAddress.OrganisationPKInfo;

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("JE_ApplicationCode == V1 and Importer's EORI is missing", targetInfo, errorMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("JE_ApplicationCode == ITF and Importer's EORI is missing", targetInfo, errorMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("JE_ApplicationCode == V2 and Importer's EORI is missing", targetInfo, errorMessage);

			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001");
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("JE_ApplicationCode == V2 and Importer's EORI is added", targetInfo, errorMessage);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("JE_ApplicationCode == V1 and Importer's EORI is added", targetInfo, errorMessage);
		}
	}
}
