using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExitSummaryDeclarationJobDocAddressValidationTest : DeclarationJobDocAddressValidationAbstractTest<ExitSummaryDeclarationJobDocAddressValidation>
	{
		public void TestCheckOrganisationPK_Supplier_EORI()
		{
			var orgHeaderWithEori = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR001");
			var orgHeaderWithoutEori = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var message = "An EORI number is required for Exit Summary Declarations.";

			supplierDocumentaryAddress.OrganisationPK = orgHeaderWithoutEori.PK;
			AssertNoMessageError("EXP, no EORI", supplierDocumentaryAddress.OrganisationPKInfo, message);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			supplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError("EXS, no EORI", supplierDocumentaryAddress.OrganisationPKInfo, message);

			supplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageError("EXS, empty OrganisationPK", supplierDocumentaryAddress.OrganisationPKInfo, message);

			supplierDocumentaryAddress.OrganisationPK = orgHeaderWithEori.PK;
			AssertNoMessageError("EXS, has EORI", supplierDocumentaryAddress.OrganisationPKInfo, message);
		}
	}
}
