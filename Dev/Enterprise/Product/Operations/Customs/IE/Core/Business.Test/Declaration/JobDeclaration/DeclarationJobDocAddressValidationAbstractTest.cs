using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class DeclarationJobDocAddressValidationAbstractTest<TValidation> : BusinessObjectValidationTestCase
		where TValidation : DeclarationJobDocAddressValidation
	{
		public void TestCheckOrganisationPK_Importer()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var targetInfo = importerDocumentaryAddress.OrganisationPKInfo;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("EXP, ImporterDocumentaryAddress mandatory.", targetInfo, MandatoryValidation.YouHaveNotEntered);
			importerDocumentaryAddress.OrganisationPK = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining("EXP Instruction, ImporterDocumentaryAddress mandatory(validation passes).", targetInfo, MandatoryValidation.YouHaveNotEntered);
			importerDocumentaryAddress.OrganisationPK = ZGuid.Empty;

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("EXS, ImporterDocumentaryAddress mandatory.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("EXS, ImporterDocumentaryAddress NOT mandatory.", targetInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			importerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("Export, SupplierDocumentaryAddress mandatory.", supplierDocumentaryAddress.OrganisationPKInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
