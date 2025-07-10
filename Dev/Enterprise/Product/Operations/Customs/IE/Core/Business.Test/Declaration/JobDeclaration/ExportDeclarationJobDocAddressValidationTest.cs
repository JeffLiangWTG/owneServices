using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportDeclarationJobDocAddressValidationTest : DeclarationJobDocAddressValidationAbstractTest<ExportDeclarationJobDocAddressValidation>
	{
		public void TestCheckOrganisationPK_Supplier_Mandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			var message = "Supplier/Exporter is required for Export Declarations.";
			AssertHasMessageError("Export, SupplierDocumentaryAddress mandatory.", supplierDocumentaryAddress.OrganisationPKInfo, message);
			supplierDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			AssertNoMessageError("EXP, SupplierDocumentaryAddress mandatory(validation passes).", supplierDocumentaryAddress.OrganisationPKInfo, message);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			supplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError("IMP, SupplierDocumentaryAddress NOT mandatory.", supplierDocumentaryAddress.OrganisationPKInfo, message);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("EXS, SupplierDocumentaryAddress NOT mandatory.", supplierDocumentaryAddress.OrganisationPKInfo, message);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("REX, SupplierDocumentaryAddress NOT mandatory.", supplierDocumentaryAddress.OrganisationPKInfo, message);
		}

		public void TestCheckOrganisationPK_NoAmend()
		{
			var originalSupplier = Factory.NewWithValidTestData<OrgHeader>();
			originalSupplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "IEFREORI");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_OA_Address = originalSupplier.MainAddress.PK;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();

			var anotherSupplier = Factory.NewWithValidTestData<OrgHeader>();
			anotherSupplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001");
			Factory.Save();

			declaration = NewFactory().Load<JobDeclaration>(declaration.PK);
			docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_OA_Address = anotherSupplier.MainAddress.PK;
			docAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("Amend Check", docAddress.OrganisationPKInfo, CommonResStrings.ShouldNotAmendThisValue);

			docAddress.E2_OA_Address = originalSupplier.MainAddress.PK;
			docAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("Amend Check(passes)", docAddress.OrganisationPKInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckOrganisationHasEORIValue()
		{
			const string messageError = "Supplier/Exporter's EORI is missing";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var supplier = Factory.New<OrgHeader>();
			var docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_OA_Address = supplier.MainAddress.PK;
			docAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("EORI for exporter/supplier", docAddress.OrganisationPKInfo, messageError);
			supplier.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001");
			docAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("EORI for exporter/supplier", docAddress.OrganisationPKInfo, messageError);
		}
	}
}
